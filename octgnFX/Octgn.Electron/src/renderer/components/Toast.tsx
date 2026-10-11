import React, { useEffect, useState } from 'react';
import { clsx } from 'clsx';
import type { Toast as ToastData, ToastType } from '../stores/toast-store';
import { useToastStore } from '../stores/toast-store';

interface ToastProps {
  toast: ToastData;
}

const typeStyles: Record<ToastType, { border: string; icon: string; bg: string }> = {
  info: {
    border: 'border-octgn-primary/40',
    icon: 'text-octgn-primary',
    bg: 'bg-octgn-primary/10',
  },
  success: {
    border: 'border-emerald-500/40',
    icon: 'text-emerald-400',
    bg: 'bg-emerald-500/10',
  },
  warning: {
    border: 'border-amber-500/40',
    icon: 'text-amber-400',
    bg: 'bg-amber-500/10',
  },
  error: {
    border: 'border-octgn-danger/40',
    icon: 'text-octgn-danger',
    bg: 'bg-octgn-danger/10',
  },
};

const typeIcons: Record<ToastType, React.ReactNode> = {
  info: (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="8" cy="8" r="6" />
      <path d="M8 7v4M8 5.5v0" />
    </svg>
  ),
  success: (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="8" cy="8" r="6" />
      <path d="M5.5 8l2 2 3-4" />
    </svg>
  ),
  warning: (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <path d="M8 2L1.5 13h13L8 2z" />
      <path d="M8 6v3M8 11v0" />
    </svg>
  ),
  error: (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="8" cy="8" r="6" />
      <path d="M6 6l4 4M10 6l-4 4" />
    </svg>
  ),
};

const Toast: React.FC<ToastProps> = ({ toast }) => {
  const removeToast = useToastStore((s) => s.removeToast);
  const [isVisible, setIsVisible] = useState(false);
  const [isExiting, setIsExiting] = useState(false);

  useEffect(() => {
    const enterTimer = requestAnimationFrame(() => {
      setIsVisible(true);
    });
    return () => cancelAnimationFrame(enterTimer);
  }, []);

  useEffect(() => {
    if (toast.duration > 0) {
      const exitDelay = toast.duration - 300;
      const exitTimer = setTimeout(() => {
        setIsExiting(true);
      }, exitDelay > 0 ? exitDelay : 0);
      return () => clearTimeout(exitTimer);
    }
  }, [toast.duration]);

  const handleDismiss = () => {
    setIsExiting(true);
    setTimeout(() => {
      removeToast(toast.id);
    }, 300);
  };

  const styles = typeStyles[toast.type];

  return (
    <div
      className={clsx(
        'flex items-start gap-2.5 px-3.5 py-2.5 rounded-lg border',
        'backdrop-blur-xl bg-octgn-surface/80 shadow-lg shadow-black/30',
        'transition-all duration-300 ease-out',
        'max-w-sm w-full',
        styles.border,
        isVisible && !isExiting
          ? 'translate-x-0 opacity-100'
          : 'translate-x-8 opacity-0',
      )}
    >
      <div className={clsx('shrink-0 mt-0.5', styles.icon)}>
        {typeIcons[toast.type]}
      </div>

      <p className="flex-1 text-xs text-octgn-text leading-relaxed">
        {toast.message}
      </p>

      <button
        onClick={handleDismiss}
        className="shrink-0 w-5 h-5 flex items-center justify-center rounded text-octgn-text-dim hover:text-octgn-text hover:bg-white/10 transition-colors"
      >
        <svg className="w-3 h-3" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
          <path d="M2 2l8 8M10 2l-8 8" />
        </svg>
      </button>
    </div>
  );
};

export default Toast;
