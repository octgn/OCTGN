import React, { useEffect, useRef, useState, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { clsx } from 'clsx';

// ─── Types ──────────────────────────────────────────────────────────

export type ContextMenuItemDef =
  | { type: 'action'; label: string; shortcut?: string; bold?: boolean; onClick: () => void; disabled?: boolean }
  | { type: 'header'; label: string; color?: string }
  | { type: 'submenu'; label: string; children: ContextMenuItemDef[] }
  | { type: 'separator' };

interface ContextMenuProps {
  x: number;
  y: number;
  items: ContextMenuItemDef[];
  onClose: () => void;
}

// ─── Submenu Component ──────────────────────────────────────────────

const SubMenu: React.FC<{
  item: Extract<ContextMenuItemDef, { type: 'submenu' }>;
  onClose: () => void;
}> = ({ item, onClose }) => {
  const [open, setOpen] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout>>(undefined);
  const ref = useRef<HTMLDivElement>(null);

  const handleEnter = useCallback(() => {
    clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => setOpen(true), 150);
  }, []);

  const handleLeave = useCallback(() => {
    clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => setOpen(false), 200);
  }, []);

  useEffect(() => () => clearTimeout(timerRef.current), []);

  return (
    <div
      ref={ref}
      className="relative"
      onMouseEnter={handleEnter}
      onMouseLeave={handleLeave}
    >
      <div className="flex items-center justify-between px-3 py-1.5 text-xs text-octgn-text hover:bg-octgn-primary/20 cursor-default transition-colors">
        <span>{item.label}</span>
        <svg className="w-3 h-3 ml-4 opacity-50" viewBox="0 0 16 16" fill="currentColor">
          <path d="M6 3l5 5-5 5V3z" />
        </svg>
      </div>
      {open && (
        <div className="absolute left-full top-0 -mt-1 ml-0.5 z-[60]">
          <MenuPanel items={item.children} onClose={onClose} />
        </div>
      )}
    </div>
  );
};

// ─── Menu Panel (renders a list of items) ───────────────────────────

const MenuPanel: React.FC<{
  items: ContextMenuItemDef[];
  onClose: () => void;
}> = ({ items, onClose }) => (
  <div className="py-1 min-w-[160px] max-w-[280px] rounded-lg border border-octgn-border/50 bg-octgn-surface/95 backdrop-blur-md shadow-xl shadow-black/40">
    {items.map((item, i) => {
      if (item.type === 'separator') {
        return <div key={i} className="my-1 border-t border-octgn-border/30" />;
      }
      if (item.type === 'header') {
        return (
          <div
            key={i}
            className="px-3 py-1 text-[10px] font-bold uppercase tracking-wider"
            style={{ color: item.color || 'var(--color-octgn-text-muted)' }}
          >
            {item.label}
          </div>
        );
      }
      if (item.type === 'submenu') {
        return <SubMenu key={i} item={item} onClose={onClose} />;
      }
      // action
      return (
        <button
          key={i}
          type="button"
          onClick={() => {
            if (!item.disabled) {
              item.onClick();
              onClose();
            }
          }}
          disabled={item.disabled}
          className={clsx(
            'w-full text-left px-3 py-1.5 text-xs transition-colors flex items-center justify-between',
            item.disabled
              ? 'text-octgn-text-dim/50 cursor-default'
              : 'text-octgn-text hover:bg-octgn-primary/20 cursor-default',
            item.bold && 'font-semibold',
          )}
        >
          <span>{item.label}</span>
          {item.shortcut && (
            <span className="ml-4 text-[10px] text-octgn-text-dim">{item.shortcut}</span>
          )}
        </button>
      );
    })}
  </div>
);

// ─── ContextMenu (positioned at x,y, closes on outside click/Escape) ─

const ContextMenu: React.FC<ContextMenuProps> = ({ x, y, items, onClose }) => {
  const menuRef = useRef<HTMLDivElement>(null);

  // Close on Escape key
  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKey);
    return () => document.removeEventListener('keydown', handleKey);
  }, [onClose]);

  // Adjust position to stay within viewport
  const [pos, setPos] = useState({ left: x, top: y });
  const measuredRef = useCallback(
    (node: HTMLDivElement | null) => {
      if (!node) return;
      (menuRef as React.MutableRefObject<HTMLDivElement | null>).current = node;
      const rect = node.getBoundingClientRect();
      let left = x;
      let top = y;
      if (left + rect.width > window.innerWidth) left = window.innerWidth - rect.width - 4;
      if (top + rect.height > window.innerHeight) top = window.innerHeight - rect.height - 4;
      if (left < 0) left = 4;
      if (top < 0) top = 4;
      if (left !== pos.left || top !== pos.top) setPos({ left, top });
    },
    [x, y, pos.left, pos.top],
  );

  if (items.length === 0) return null;

  // Use a portal to render at document body level, avoiding any stacking context
  // or event interference from parent elements. A transparent backdrop catches
  // outside clicks reliably without document-level listeners.
  return createPortal(
    <>
      {/* Invisible backdrop to catch outside clicks */}
      <div
        className="fixed inset-0 z-[9998]"
        onMouseDown={onClose}
        onContextMenu={(e) => {
          e.preventDefault();
          onClose();
        }}
      />
      {/* Menu panel */}
      <div
        ref={measuredRef}
        className="fixed z-[9999]"
        style={{ left: pos.left, top: pos.top }}
        onContextMenu={(e) => e.preventDefault()}
      >
        <MenuPanel items={items} onClose={onClose} />
      </div>
    </>,
    document.body,
  );
};

export default ContextMenu;
