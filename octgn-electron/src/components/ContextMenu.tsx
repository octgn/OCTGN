import { useRef, useCallback, useMemo } from 'react';

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
  data?: unknown;
}

export function useContextMenu() {
  const [state, setState] = useState<ContextMenuState>({
    isOpen: false,
    x: 0,
    y: 0,
    items: [],
  });

  const open = useCallback(
    (x: number, y: number, items: ContextMenuItem[], data?: unknown) => {
      setState({ isOpen: true, x, y, items, data });
    },
    []
  );

  const close = useCallback(() => {
    setState((s) => ({ ...s, isOpen: false }));
  }, []);

  return { ...state, open, close, menuRef: useRef<HTMLDivElement>(null) };
}

// Add useState import
import { useState } from 'react';

export function getCardContextMenuItems(
  cards: Array<{ id: number; faceUp: boolean; rotation: number }>,
  handlers: {
    onFlipFaceUp: () => void;
    onFlipFaceDown: () => void;
    onRotate: (deg: number) => void;
    onDelete: () => void;
    onTarget?: () => void;
    onHighlight?: (color: string) => void;
  }
): ContextMenuItem[] {
  const hasCards = cards.length > 0;
  const singleCard = cards.length === 1 ? cards[0] : null;

  return [
    {
      label: 'Flip Face Up',
      icon: '⬆️',
      onClick: handlers.onFlipFaceUp,
      disabled: !hasCards,
    },
    {
      label: 'Flip Face Down',
      icon: '⬇️',
      onClick: handlers.onFlipFaceDown,
      disabled: !hasCards,
    },
    { divider: true, label: '', disabled: true },
    {
      label: 'Rotate',
      icon: '🔄',
      submenu: [
        { label: 'Rotate 90° CW', icon: '↻', onClick: () => handlers.onRotate(90) },
        { label: 'Rotate 90° CCW', icon: '↺', onClick: () => handlers.onRotate(-90) },
        { label: 'Rotate 180°', icon: '🔃', onClick: () => handlers.onRotate(180) },
        { divider: true, label: '', disabled: true },
        { label: 'Reset Rotation', icon: '⬆️', onClick: () => handlers.onRotate(0) },
      ],
      disabled: !hasCards,
    },
    { divider: true, label: '', disabled: true },
    {
      label: 'Target',
      icon: '🎯',
      onClick: handlers.onTarget,
      disabled: !hasCards || !handlers.onTarget,
    },
    {
      label: 'Highlight',
      icon: '✨',
      submenu: [
        { label: 'Red', onClick: () => handlers.onHighlight?.('#EF4444') },
        { label: 'Green', onClick: () => handlers.onHighlight?.('#22C55E') },
        { label: 'Blue', onClick: () => handlers.onHighlight?.('#3B82F6') },
        { label: 'Yellow', onClick: () => handlers.onHighlight?.('#EAB308') },
        { label: 'Purple', onClick: () => handlers.onHighlight?.('#A855F7') },
        { divider: true, label: '', disabled: true },
        { label: 'Clear', onClick: () => handlers.onHighlight?.('') },
      ],
      disabled: !hasCards || !handlers.onHighlight,
    },
    { divider: true, label: '', disabled: true },
    {
      label: 'Add Marker',
      icon: '🏷️',
      disabled: !hasCards,
      submenu: [
        { label: '+1/+1 Counter', onClick: () => {} },
        { label: '-1/-1 Counter', onClick: () => {} },
        { label: 'Charge Counter', onClick: () => {} },
        { label: 'Custom...', onClick: () => {} },
      ],
    },
    { divider: true, label: '', disabled: true },
    {
      label: 'Delete',
      icon: '🗑️',
      onClick: handlers.onDelete,
      danger: true,
      disabled: !hasCards,
      shortcut: 'Del',
    },
  ];
}

export function ContextMenu({
  state,
  menuRef,
  onClose,
}: {
  state: ContextMenuState;
  menuRef: React.RefObject<HTMLDivElement>;
  onClose: () => void;
}) {
  const { isOpen, x, y, items } = state;

  // Position calculation to keep menu in viewport
  const position = useMemo(() => {
    if (!isOpen) return { x, y };

    const menuWidth = 200;
    const menuHeight = items.length * 36;
    const padding = 10;

    let posX = x;
    let posY = y;

    // Adjust horizontal position
    if (x + menuWidth + padding > window.innerWidth) {
      posX = window.innerWidth - menuWidth - padding;
    }

    // Adjust vertical position
    if (y + menuHeight + padding > window.innerHeight) {
      posY = window.innerHeight - menuHeight - padding;
    }

    return { x: Math.max(padding, posX), y: Math.max(padding, posY) };
  }, [isOpen, x, y, items]);

  // Close on click outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen, onClose, menuRef]);

  if (!isOpen) return null;

  return (
    <div
      ref={menuRef}
      className="fixed glass rounded-xl py-2 min-w-[200px] z-50 animate-fade-in shadow-2xl"
      style={{ left: position.x, top: position.y }}
    >
      {items.map((item, index) => {
        if (item.divider) {
          return <div key={index} className="h-px bg-octgn-accent/50 my-2 mx-3" />;
        }

        return (
          <div key={index} className="relative group">
            <button
              onClick={() => {
                if (!item.disabled && item.onClick) {
                  item.onClick();
                  onClose();
                }
              }}
              disabled={item.disabled}
              className={`
                w-full px-4 py-2 flex items-center justify-between
                text-sm transition-colors
                ${item.disabled
                  ? 'text-gray-600 cursor-not-allowed'
                  : item.danger
                    ? 'text-red-400 hover:bg-red-500/20'
                    : 'text-gray-300 hover:bg-octgn-accent/50 hover:text-white'
                }
              `}
            >
              <div className="flex items-center space-x-3">
                {item.icon && <span className="w-5 text-center">{item.icon}</span>}
                <span>{item.label}</span>
              </div>
              
              <div className="flex items-center space-x-2 text-gray-500">
                {item.shortcut && (
                  <span className="text-xs font-mono">{item.shortcut}</span>
                )}
                {item.submenu && (
                  <span>▸</span>
                )}
              </div>
            </button>

            {/* Submenu */}
            {item.submenu && !item.disabled && (
              <div className="absolute left-full top-0 hidden group-hover:block glass rounded-xl py-2 min-w-[180px] ml-1 shadow-2xl">
                {item.submenu.map((subItem, subIndex) => {
                  if (subItem.divider) {
                    return <div key={subIndex} className="h-px bg-octgn-accent/50 my-2 mx-3" />;
                  }

                  return (
                    <button
                      key={subIndex}
                      onClick={() => {
                        if (!subItem.disabled && subItem.onClick) {
                          subItem.onClick();
                          onClose();
                        }
                      }}
                      disabled={subItem.disabled}
                      className={`
                        w-full px-4 py-2 flex items-center space-x-3
                        text-sm transition-colors
                        ${subItem.disabled
                          ? 'text-gray-600 cursor-not-allowed'
                          : 'text-gray-300 hover:bg-octgn-accent/50 hover:text-white'
                        }
                      `}
                    >
                      {subItem.icon && <span className="w-5 text-center">{subItem.icon}</span>}
                      <span>{subItem.label}</span>
                    </button>
                  );
                })}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}

// Add useEffect import
import { useEffect } from 'react';
