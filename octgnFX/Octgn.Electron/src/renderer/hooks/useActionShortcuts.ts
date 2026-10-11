import { useEffect, useMemo } from 'react';
import type { GameState, ActionMenuItem, CardAction, GroupAction } from '../../shared/types';

interface ShortcutEntry {
  action: CardAction | GroupAction;
  actionType: 'card' | 'group';
  groupName: string;
}

/** Flatten all action menu items to extract shortcut mappings */
function collectShortcuts(items: ActionMenuItem[], groupName: string): ShortcutEntry[] {
  const entries: ShortcutEntry[] = [];
  for (const item of items) {
    if (item.type === 'action') {
      if (item.action.shortcut) {
        entries.push({ action: item.action, actionType: item.actionType, groupName });
      }
    } else if (item.type === 'submenu') {
      entries.push(...collectShortcuts(item.children, groupName));
    }
  }
  return entries;
}

/** Normalize a shortcut string (e.g. "Ctrl+F") to a comparable key */
function normalizeShortcut(shortcut: string): string {
  return shortcut.toLowerCase().replace(/\s+/g, '');
}

/** Build a key string from a keyboard event */
function eventToKey(e: KeyboardEvent): string {
  const parts: string[] = [];
  if (e.ctrlKey) parts.push('ctrl');
  if (e.altKey) parts.push('alt');
  if (e.shiftKey) parts.push('shift');
  parts.push(e.key.toLowerCase());
  return parts.join('+');
}

export function useActionShortcuts(
  gameState: GameState | null,
  selectedCardId: string | null,
  isSpectator: boolean,
) {
  const shortcutMap = useMemo(() => {
    if (!gameState?.actionDefs) return new Map<string, ShortcutEntry[]>();
    const map = new Map<string, ShortcutEntry[]>();
    for (const [groupName, defs] of Object.entries(gameState.actionDefs)) {
      const entries = [
        ...collectShortcuts(defs.cardActions, groupName),
        ...collectShortcuts(defs.groupActions, groupName),
      ];
      for (const entry of entries) {
        const key = normalizeShortcut(entry.action.shortcut!);
        const existing = map.get(key) ?? [];
        existing.push(entry);
        map.set(key, existing);
      }
    }
    return map;
  }, [gameState?.actionDefs]);

  useEffect(() => {
    if (isSpectator || shortcutMap.size === 0) return;

    const handler = (e: KeyboardEvent) => {
      // Don't trigger shortcuts when typing in inputs
      const target = e.target as HTMLElement;
      if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable) return;

      const key = eventToKey(e);
      const entries = shortcutMap.get(key);
      if (!entries || entries.length === 0) return;

      // Find the best matching entry based on context
      for (const entry of entries) {
        if (entry.actionType === 'card' && selectedCardId) {
          e.preventDefault();
          // Find which group the selected card is in
          let groupId = 0;
          if (gameState) {
            // Check table
            const onTable = gameState.table.cards.find((c) => c.id === selectedCardId);
            if (onTable) {
              groupId = 0x01000000; // table
            } else {
              // Check player groups
              for (const player of gameState.players) {
                for (const group of player.groups) {
                  if (group.cards.some((c) => c.id === selectedCardId)) {
                    groupId = Number(group.id);
                    break;
                  }
                }
                if (groupId) break;
              }
            }
          }
          window.octgn.executeAction({
            type: 'card',
            action: entry.action,
            cardId: Number(selectedCardId),
            groupId,
          });
          return;
        }
        if (entry.actionType === 'group') {
          e.preventDefault();
          // Execute group action on the local player's group matching this group name
          if (gameState) {
            const localPlayer = gameState.players.find((p) => p.id === gameState.localPlayerId);
            const group = localPlayer?.groups.find((g) => g.name === entry.groupName);
            if (group) {
              window.octgn.executeAction({
                type: 'group',
                action: entry.action,
                groupId: Number(group.id),
              });
              return;
            }
          }
        }
      }
    };

    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [shortcutMap, selectedCardId, isSpectator, gameState]);
}
