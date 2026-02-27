import { useEffect, useCallback } from 'react';

export interface KeyboardShortcut {
  key: string;
  ctrl?: boolean;
  shift?: boolean;
  alt?: boolean;
  meta?: boolean;
  action: () => void;
  description?: string;
}

/**
 * Hook for managing keyboard shortcuts
 */
export function useKeyboardShortcuts(shortcuts: KeyboardShortcut[]): void {
  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      // Don't trigger shortcuts when typing in inputs
      if (
        event.target instanceof HTMLInputElement ||
        event.target instanceof HTMLTextAreaElement
      ) {
        return;
      }

      for (const shortcut of shortcuts) {
        const keyMatch = event.key.toLowerCase() === shortcut.key.toLowerCase();
        const ctrlMatch = !shortcut.ctrl || event.ctrlKey || event.metaKey;
        const shiftMatch = !shortcut.shift || event.shiftKey;
        const altMatch = !shortcut.alt || event.altKey;

        if (keyMatch && ctrlMatch && shiftMatch && altMatch) {
          event.preventDefault();
          shortcut.action();
          return;
        }
      }
    },
    [shortcuts]
  );

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);
}

/**
 * Default game table shortcuts
 */
export const GAME_SHORTCUTS: KeyboardShortcut[] = [
  { key: 'Delete', action: () => {}, description: 'Delete selected cards' },
  { key: 'Escape', action: () => {}, description: 'Clear selection' },
  { key: 'a', ctrl: true, action: () => {}, description: 'Select all cards' },
  { key: 'd', ctrl: true, action: () => {}, description: 'Deselect all' },
  { key: 'z', ctrl: true, action: () => {}, description: 'Undo' },
  { key: 'y', ctrl: true, action: () => {}, description: 'Redo' },
  { key: 's', ctrl: true, action: () => {}, description: 'Save game state' },
  { key: 'f', action: () => {}, description: 'Flip selected cards' },
  { key: 'r', action: () => {}, description: 'Rotate selected cards 90°' },
  { key: 'R', shift: true, action: () => {}, description: 'Rotate selected cards -90°' },
  { key: ' ', action: () => {}, description: 'End turn' },
  { key: 'h', action: () => {}, description: 'Toggle hand visibility' },
  { key: 'c', action: () => {}, description: 'Toggle chat visibility' },
  { key: '=', ctrl: true, action: () => {}, description: 'Zoom in' },
  { key: '-', ctrl: true, action: () => {}, description: 'Zoom out' },
  { key: '0', ctrl: true, action: () => {}, description: 'Reset zoom' },
];
