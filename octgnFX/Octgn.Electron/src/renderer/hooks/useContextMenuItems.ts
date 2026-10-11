import { useCallback } from 'react';
import type { ContextMenuItemDef } from '../components/ContextMenu';
import type { Card, GameState, ActionMenuItem, CardAction, GroupAction, Player, Group } from '../../shared/types';
import { readablePlayerColor } from '../utils/player-colors';

type ActionDefs = Record<string, { cardActions: ActionMenuItem[]; groupActions: ActionMenuItem[] }>;

function executeAction(request: Parameters<typeof window.octgn.executeAction>[0], actionName: string) {
  const result = window.octgn.executeAction(request);
  if (result && typeof result.then === 'function') {
    result.then((r) => {
      if (r && !r.success) {
        console.warn(`[ContextMenu] Action "${actionName}" failed: ${r.error ?? 'unknown error'}`);
      }
    });
  }
}

function convertActionMenuItems(
  items: ActionMenuItem[],
  onExecute: (action: CardAction | GroupAction, actionType: 'card' | 'group') => void,
): ContextMenuItemDef[] {
  const result: ContextMenuItemDef[] = [];
  for (const item of items) {
    if (item.type === 'separator') {
      result.push({ type: 'separator' });
    } else if (item.type === 'submenu') {
      result.push({
        type: 'submenu',
        label: item.name,
        children: convertActionMenuItems(item.children, onExecute),
      });
    } else if (item.type === 'action') {
      const action = item.action;
      result.push({
        type: 'action',
        label: action.name,
        shortcut: action.shortcut,
        bold: 'isDefault' in action && action.isDefault === true,
        onClick: () => onExecute(action, item.actionType),
      });
    }
  }
  return result;
}

function findCardOwner(card: Card, players: Player[]): Player | undefined {
  return players.find((p) => String(p.id) === card.ownerId);
}

function findCardGroup(card: Card, gameState: GameState): { group: Group; groupName: string } | null {
  // Check table
  if (gameState.table.cards.some((c) => c.id === card.id)) {
    return { group: { id: '0', name: 'Table', cards: [], visibility: 0, controller: 0 }, groupName: '__table__' };
  }
  // Check player groups
  for (const player of gameState.players) {
    for (const group of player.groups) {
      if (group.cards.some((c) => c.id === card.id)) {
        return { group, groupName: group.name };
      }
    }
  }
  return null;
}

export function useContextMenuItems() {
  const buildCardMenuItems = useCallback((
    card: Card,
    gameState: GameState,
    callbacks: {
      flipCard: (cardId: number, faceUp: boolean) => void;
      rotateCard: (cardId: number, rotation: number) => void;
      peekCard: (cardId: number) => void;
      moveCards: (cardIds: number[], groupId: number, indices: number[], faceUp: boolean[]) => void;
    },
  ): ContextMenuItemDef[] => {
    const items: ContextMenuItemDef[] = [];
    const actionDefs = gameState.actionDefs as ActionDefs | undefined;
    const cardInfo = findCardGroup(card, gameState);
    const groupName = cardInfo?.groupName ?? '__table__';
    const owner = findCardOwner(card, gameState.players);

    // Card name header
    items.push({
      type: 'header',
      label: card.name || 'Unknown Card',
      color: owner ? readablePlayerColor(owner.color) : undefined,
    });

    // Game-defined card actions
    const defs = actionDefs?.[groupName];
    if (defs?.cardActions?.length) {
      const onExecute = (action: CardAction | GroupAction, actionType: 'card' | 'group') => {
        executeAction({
          type: actionType,
          action,
          cardId: Number(card.id),
          groupId: Number(cardInfo?.group.id ?? 0),
        }, action.name);
      };
      items.push(...convertActionMenuItems(defs.cardActions, onExecute));
    }

    items.push({ type: 'separator' });

    // Built-in card actions
    items.push({
      type: 'action',
      label: card.faceUp ? 'Flip Face Down' : 'Flip Face Up',
      onClick: () => callbacks.flipCard(Number(card.id), !card.faceUp),
    });

    items.push({
      type: 'submenu',
      label: 'Rotate',
      children: [
        { type: 'action', label: '90°', onClick: () => callbacks.rotateCard(Number(card.id), 1) },
        { type: 'action', label: '180°', onClick: () => callbacks.rotateCard(Number(card.id), 2) },
        { type: 'action', label: '270°', onClick: () => callbacks.rotateCard(Number(card.id), 3) },
        { type: 'separator' },
        { type: 'action', label: 'Reset', onClick: () => callbacks.rotateCard(Number(card.id), 0) },
      ],
    });

    if (!card.faceUp) {
      items.push({
        type: 'action',
        label: 'Peek',
        onClick: () => callbacks.peekCard(Number(card.id)),
      });
    }

    // Move to group submenu
    const localPlayer = gameState.players.find((p) => p.id === gameState.localPlayerId);
    if (localPlayer && localPlayer.groups.length > 0) {
      const moveChildren: ContextMenuItemDef[] = localPlayer.groups
        .filter((g) => g.id !== cardInfo?.group.id) // exclude current group
        .map((g) => ({
          type: 'action' as const,
          label: g.name,
          onClick: () => callbacks.moveCards([Number(card.id)], Number(g.id), [0], [true]),
        }));
      if (moveChildren.length > 0) {
        items.push({
          type: 'submenu',
          label: 'Move To',
          children: moveChildren,
        });
      }
    }

    // Game-defined group actions (shown below card actions when right-clicking a card)
    if (defs?.groupActions?.length) {
      items.push({ type: 'separator' });
      const onGroupExecute = (action: CardAction | GroupAction, actionType: 'card' | 'group') => {
        executeAction({
          type: actionType,
          action,
          groupId: Number(cardInfo?.group.id ?? 0),
        }, action.name);
      };
      items.push(...convertActionMenuItems(defs.groupActions, onGroupExecute));
    }

    return items;
  }, []);

  const buildGroupMenuItems = useCallback((
    group: Group,
    gameState: GameState,
    callbacks: {
      shuffleGroup: (groupId: number) => void;
    },
  ): ContextMenuItemDef[] => {
    const items: ContextMenuItemDef[] = [];
    const actionDefs = gameState.actionDefs as ActionDefs | undefined;

    // Group name header
    items.push({ type: 'header', label: group.name });

    // Game-defined group actions
    const defs = actionDefs?.[group.name];
    if (defs?.groupActions?.length) {
      const onExecute = (action: CardAction | GroupAction, actionType: 'card' | 'group') => {
        executeAction({
          type: actionType,
          action,
          groupId: Number(group.id),
        }, action.name);
      };
      items.push(...convertActionMenuItems(defs.groupActions, onExecute));
    }

    items.push({ type: 'separator' });

    // Built-in: Shuffle (for piles, not hand)
    if (group.name.toLowerCase() !== 'hand') {
      items.push({
        type: 'action',
        label: 'Shuffle',
        onClick: () => callbacks.shuffleGroup(Number(group.id)),
      });
    }

    return items;
  }, []);

  const buildTableMenuItems = useCallback((
    gameState: GameState,
  ): ContextMenuItemDef[] => {
    const items: ContextMenuItemDef[] = [];
    const actionDefs = gameState.actionDefs as ActionDefs | undefined;

    const defs = actionDefs?.['__table__'];
    if (defs?.groupActions?.length) {
      const tableGroupId = 0x01000000; // table group ID
      const onExecute = (action: CardAction | GroupAction, actionType: 'card' | 'group') => {
        executeAction({
          type: actionType,
          action,
          groupId: tableGroupId,
        }, action.name);
      };
      items.push(...convertActionMenuItems(defs.groupActions, onExecute));
    }

    return items;
  }, []);

  return { buildCardMenuItems, buildGroupMenuItems, buildTableMenuItems };
}
