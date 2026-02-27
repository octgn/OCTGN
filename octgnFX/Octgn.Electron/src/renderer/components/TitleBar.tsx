import React, { useCallback } from 'react';
import { useAppStore } from '../stores/app-store';

const TitleBar: React.FC = () => {
  const minimize = useAppStore((s) => s.minimize);
  const maximize = useAppStore((s) => s.maximize);
  const quit = useAppStore((s) => s.quit);

  const handleMinimize = useCallback(() => {
    minimize();
  }, [minimize]);

  const handleMaximize = useCallback(() => {
    maximize();
  }, [maximize]);

  const handleClose = useCallback(() => {
    quit();
  }, [quit]);

  return (
    <header className="drag-region flex items-center justify-between h-9 px-3 bg-octgn-surface/80 backdrop-blur-md border-b border-octgn-border/50 shrink-0 z-50">
      {/* Logo */}
      <div className="flex items-center gap-2">
        <span className="font-display text-sm font-bold tracking-widest text-octgn-primary glow-text-blue">
          OCTGN
        </span>
        <span className="text-[10px] text-octgn-text-dim font-medium tracking-wide uppercase">
          Electron
        </span>
      </div>

      {/* Window controls */}
      <div className="no-drag flex items-center gap-0.5">
        <button
          onClick={handleMinimize}
          className="group flex items-center justify-center w-10 h-9 transition-colors hover:bg-white/5"
          aria-label="Minimize"
        >
          <svg className="w-3 h-3 text-octgn-text-muted group-hover:text-octgn-text" viewBox="0 0 12 12" fill="none">
            <rect x="1" y="5.5" width="10" height="1" rx="0.5" fill="currentColor" />
          </svg>
        </button>
        <button
          onClick={handleMaximize}
          className="group flex items-center justify-center w-10 h-9 transition-colors hover:bg-white/5"
          aria-label="Maximize"
        >
          <svg className="w-3 h-3 text-octgn-text-muted group-hover:text-octgn-text" viewBox="0 0 12 12" fill="none">
            <rect x="1.5" y="1.5" width="9" height="9" rx="1" stroke="currentColor" strokeWidth="1.2" fill="none" />
          </svg>
        </button>
        <button
          onClick={handleClose}
          className="group flex items-center justify-center w-10 h-9 transition-colors hover:bg-red-500/80"
          aria-label="Close"
        >
          <svg className="w-3 h-3 text-octgn-text-muted group-hover:text-white" viewBox="0 0 12 12" fill="none">
            <path d="M2 2l8 8M10 2l-8 8" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
          </svg>
        </button>
      </div>
    </header>
  );
};

export default TitleBar;
