import { create } from 'zustand';

export type ToastType = 'info' | 'success' | 'warning' | 'error';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration: number;
  createdAt: number;
}

interface ToastStoreState {
  toasts: Toast[];
}

interface ToastStoreActions {
  addToast: (message: string, type?: ToastType, duration?: number) => void;
  removeToast: (id: string) => void;
}

export type ToastStore = ToastStoreState & ToastStoreActions;

let toastIdCounter = 0;

export const useToastStore = create<ToastStore>()((set) => ({
  toasts: [],

  addToast: (message: string, type: ToastType = 'info', duration: number = 4000) => {
    const id = `toast-${Date.now()}-${toastIdCounter++}`;
    const toast: Toast = {
      id,
      message,
      type,
      duration,
      createdAt: Date.now(),
    };

    set((state) => ({
      toasts: [...state.toasts, toast],
    }));

    if (duration > 0) {
      setTimeout(() => {
        set((state) => ({
          toasts: state.toasts.filter((t) => t.id !== id),
        }));
      }, duration);
    }
  },

  removeToast: (id: string) => {
    set((state) => ({
      toasts: state.toasts.filter((t) => t.id !== id),
    }));
  },
}));
