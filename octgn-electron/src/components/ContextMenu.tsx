import { useState, useCallback, useEffect, useRef } from 'react';
import { Card as CardType, Group } from '../types/game';

export interface ContextMenuItem {
  label: string;
  icon?: string;
  shortcut?: string;
  onClick?: () => void;
  disabled?: boolean;
  danger?: boolean;
  submenu?: ContextMenuItem[];
  divider?: boolean;
}

export interface ContextMenuState {
  isOpen: boolean;
  x: number;
  y: number;
  items: ContextMenuItem[];
  data: {
    cards?: CardType[];
    group?: Group;
    player?: { id: string; name: string };
    tablePosition?: { x: number; y: number };
  };
}

export function useContextMenu() {
  const [state, setState] = useState<ContextMenuState>({
    isOpen: false,
    x: 0,
    y: 0,
    items: [],
    data: {},
  });

  const menuRef = useRef<HTMLDivElement>(null);

  const open = useCallback(
    (
      x: number,
      y: number,
      items: ContextMenuItem[],
      data: ContextMenuState['data'] = {}
    ) => {
      setState({ isOpen: true, x, y, items, data });
    },
    []
  );

  const close = useCallback(() => {
    setState((prev) => ({ ...prev, isOpen: false }));
  }, []);

  const toggle = useCallback(
    (
      x: number,
      y: number,
      items: ContextMenuItem[],
      data: ContextMenuState['data'] = {}
    ) => {
      if (state.isOpen) {
        close();
      } else {
        open(x, y, items, data);
      }
    },
    [state.isOpen, open, close]
  );

  // Close on click outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        close();
      }
    };

    if (state.isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      document.addEventListener('contextmenu', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('contextmenu', handleClickOutside);
    };
  }, [state.isOpen, close]);

  // Close on escape
  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        close();
      }
    };

    if (state.isOpen) {
      document.addEventListener('keydown', handleEscape);
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
    };
  }, [state.isOpen, close]);

  return {
    ...state,
    menuRef,
    open,
    close,
    toggle,
  };
}

interface ContextMenuProps {
  state: ContextMenuState;
  menuRef: React.RefObject<HTMLDivElement>;
  onClose: () => void;
}

export function ContextMenu({ state, menuRef, onClose }: ContextMenuProps) {
  if (!state.isOpen) return null;

  // Calculate position to keep menu on screen
  const calculatePosition = () => {
    let x = state.x;
    let y = state.y;
    const menuWidth = 200;
    const menuHeight = 300; // Estimate

    if (x + menuWidth > window.innerWidth) {
      x = window.innerWidth - menuWidth - 10;
    }
    if (y + menuHeight > window.innerHeight) {
      y = window.innerHeight - menuHeight - 10;
    }

    return { x: Math.max(10, x), y: Math.max(10, y) };
  };

  const position = calculatePosition();

  const renderMenuItem = (item: ContextMenuItem, depth = 0): React.ReactNode => {
    if (item.divider) {
      return <hr key={Math.random()} className="border-octgn-accent my-1" />;
    }

    const hasSubmenu = item.submenu && item.submenu.length > 0;

    return (
      <div key={item.label} className="relative group">
        <button
          onClick={() => {
            if (!item.disabled && item.onClick) {
              item.onClick();
              onClose();
            }
          }}
          disabled={item.disabled}
          className={`
            w-full px-4 py-2 text-left flex items-center justify-between
            transition-colors
            ${item.disabled
              ? 'text-gray-500 cursor-not-allowed'
              : item.danger
              ? 'text-red-400 hover:bg-red-600/30'
              : 'text-gray-200 hover:bg-octgn-accent/50'
            }
          `}
        >
          <span className="flex items-center space-x-2">
            {item.icon && <span>{item.icon}</span>}
            <span>{item.label}</span>
          </span>
          <span className="flex items-center space-x-2 text-xs">
            {item.shortcut && <span className="text-gray-500">{item.shortcut}</span>}
            {hasSubmenu && <span>▶</span>}
          </span>
        </button>

        {/* Submenu */}
        {hasSubmenu && (
          <div className="absolute left-full top-0 hidden group-hover:block bg-octgn-primary border border-octgn-accent rounded-lg shadow-lg min-w-[180px]">
            {item.submenu!.map((subItem) => renderMenuItem(subItem, depth + 1))}
          </div>
        )}
      </div>
    );
  };

  return (
    <div
      ref={menuRef}
      className="fixed z-50 bg-octgn-primary border border-octgn-accent rounded-lg shadow-xl min-w-[180px] py-1"
      style={{ left: position.x, top: position.y }}
    >
      {state.items.map((item) => renderMenuItem(item))}
    </div>
  );
}

/**
 * Default card context menu items
 */
export function getCardContextMenuItems(
  cards: CardType[],
  actions: {
    onFlipFaceUp?: () => void;
    onFlipFaceDown?: () => void;
    onRotate?: (degrees: number) => void;
    onMoveToGroup?: (groupId: number) => void;
    onAddMarker?: () => void;
    onRemoveMarker?: () => void;
    onTarget?: () => void;
    onHighlight?: (color: string) => void;
    onDelete?: () => void;
  }
): ContextMenuItem[] {
  const singleCard = cards.length === 1;
  const card = cards[0];

  return [
    {
      label: 'Flip Face Up',
      icon: '👁️',
      shortcut: 'F',
      onClick: actions.onFlipFaceUp,
      disabled: card?.faceUp,
    },
    {
      label: 'Flip Face Down',
      icon: '🔽',
      shortcut: 'Shift+F',
      onClick: actions.onFlipFaceDown,
      disabled: !card?.faceUp,
    },
    { divider: true, label: '' },
    {
      label: 'Rotate',
      icon: '🔄',
      submenu: [
        { label: '90° CW', onClick: () => actions.onRotate?.(90) },
        { label: '180°', onClick: () => actions.onRotate?.(180) },
        { label: '90° CCW', onClick: () => actions.onRotate?.(270) },
        { label: 'Reset (0°)', onClick: () => actions.onRotate?.(0) },
      ],
    },
    {
      label: 'Move To',
      icon: '📦',
      submenu: [
        { label: 'Hand', onClick: () => actions.onMoveToGroup?.(-1) },
        { label: 'Table', onClick: () => actions.onMoveToGroup?.(0) },
        { label: 'Deck', onClick: () => actions.onMoveToGroup?.(-2) },
        { label: 'Discard', onClick: () => actions.onMoveToGroup?.(-3) },
      ],
    },
    { divider: true, label: '' },
    {
      label: 'Add Marker',
      icon: '➕',
      onClick: actions.onAddMarker,
      disabled: !singleCard,
    },
    {
      label: 'Remove Marker',
      icon: '➖',
      onClick: actions.onRemoveMarker,
      disabled: !singleCard || !card?.markers?.length,
    },
    { divider: true, label: '' },
    {
      label: 'Highlight',
      icon: '🎨',
      submenu: [
        { label: '🔴 Red', onClick: () => actions.onHighlight?.('#ef4444') },
        { label: '🟢 Green', onClick: () => actions.onHighlight?.('#10b981') },
        { label: '🔵 Blue', onClick: () => actions.onHighlight?.('#3b82f6') },
        { label: '🟡 Yellow', onClick: () => actions.onHighlight?.('#f59e0b') },
        { label: 'None', onClick: () => actions.onHighlight?.('') },
      ],
    },
    {
      label: card?.targeted ? 'Untarget' : 'Target',
      icon: '🎯',
      onClick: actions.onTarget,
    },
    { divider: true, label: '' },
    {
      label: `Delete (${cards.length})`,
      icon: '🗑️',
      danger: true,
      onClick: actions.onDelete,
    },
  ];
}
