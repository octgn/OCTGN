import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { useToastStore } from '@renderer/stores/toast-store';
import type { ToastType } from '@renderer/stores/toast-store';

describe('useToastStore', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    useToastStore.setState({ toasts: [] });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('addToast', () => {
    it('should create a toast with correct properties', () => {
      useToastStore.getState().addToast('Hello world', 'success', 5000);

      const toasts = useToastStore.getState().toasts;
      expect(toasts).toHaveLength(1);
      expect(toasts[0].message).toBe('Hello world');
      expect(toasts[0].type).toBe('success');
      expect(toasts[0].duration).toBe(5000);
      expect(toasts[0].id).toMatch(/^toast-/);
      expect(toasts[0].createdAt).toBeTypeOf('number');
    });

    it('should default to info type', () => {
      useToastStore.getState().addToast('Info toast');

      const toasts = useToastStore.getState().toasts;
      expect(toasts[0].type).toBe('info');
    });

    it('should default to 4000ms duration', () => {
      useToastStore.getState().addToast('Default duration');

      const toasts = useToastStore.getState().toasts;
      expect(toasts[0].duration).toBe(4000);
    });

    it('should support all toast types', () => {
      const types: ToastType[] = ['info', 'success', 'warning', 'error'];

      for (const type of types) {
        useToastStore.getState().addToast(`${type} toast`, type);
      }

      const toasts = useToastStore.getState().toasts;
      expect(toasts).toHaveLength(4);
      expect(toasts.map((t) => t.type)).toEqual(types);
    });

    it('should add multiple toasts', () => {
      useToastStore.getState().addToast('First');
      useToastStore.getState().addToast('Second');
      useToastStore.getState().addToast('Third');

      expect(useToastStore.getState().toasts).toHaveLength(3);
    });

    it('should assign unique ids to each toast', () => {
      useToastStore.getState().addToast('A');
      useToastStore.getState().addToast('B');

      const toasts = useToastStore.getState().toasts;
      expect(toasts[0].id).not.toBe(toasts[1].id);
    });
  });

  describe('removeToast', () => {
    it('should remove a toast by id', () => {
      useToastStore.getState().addToast('To remove', 'info', 0);
      const id = useToastStore.getState().toasts[0].id;

      useToastStore.getState().removeToast(id);

      expect(useToastStore.getState().toasts).toHaveLength(0);
    });

    it('should only remove the specified toast', () => {
      useToastStore.getState().addToast('Keep', 'info', 0);
      useToastStore.getState().addToast('Remove', 'info', 0);
      const removeId = useToastStore.getState().toasts[1].id;

      useToastStore.getState().removeToast(removeId);

      const toasts = useToastStore.getState().toasts;
      expect(toasts).toHaveLength(1);
      expect(toasts[0].message).toBe('Keep');
    });

    it('should be a no-op for a non-existent id', () => {
      useToastStore.getState().addToast('Keep', 'info', 0);

      useToastStore.getState().removeToast('does-not-exist');

      expect(useToastStore.getState().toasts).toHaveLength(1);
    });
  });

  describe('auto-dismiss', () => {
    it('should auto-remove a toast after its duration', () => {
      useToastStore.getState().addToast('Auto dismiss', 'info', 3000);

      expect(useToastStore.getState().toasts).toHaveLength(1);

      vi.advanceTimersByTime(3000);

      expect(useToastStore.getState().toasts).toHaveLength(0);
    });

    it('should not auto-remove before the duration elapses', () => {
      useToastStore.getState().addToast('Still here', 'info', 5000);

      vi.advanceTimersByTime(4999);
      expect(useToastStore.getState().toasts).toHaveLength(1);

      vi.advanceTimersByTime(1);
      expect(useToastStore.getState().toasts).toHaveLength(0);
    });

    it('should not auto-dismiss when duration is 0', () => {
      useToastStore.getState().addToast('Permanent', 'info', 0);

      vi.advanceTimersByTime(60_000);

      expect(useToastStore.getState().toasts).toHaveLength(1);
    });

    it('should dismiss each toast independently after its own duration', () => {
      useToastStore.getState().addToast('Short', 'info', 1000);
      useToastStore.getState().addToast('Long', 'info', 5000);

      vi.advanceTimersByTime(1000);
      expect(useToastStore.getState().toasts).toHaveLength(1);
      expect(useToastStore.getState().toasts[0].message).toBe('Long');

      vi.advanceTimersByTime(4000);
      expect(useToastStore.getState().toasts).toHaveLength(0);
    });
  });
});
