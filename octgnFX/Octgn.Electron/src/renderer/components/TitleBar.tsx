import React, { useCallback } from 'react';
import { clsx } from 'clsx';
import { useAppStore } from '../stores/app-store';
import { useGameStore } from '../stores/game-store';

interface TitleBarProps {
  isGameWindow?: boolean;
}

type GameWindowStatus = 'connecting' | 'lobby' | 'spectating' | 'playing';

function useGameWindowInfo(): { gameName: string; status: GameWindowStatus } {
  const gameState = useGameStore((s) => s.gameState);
  if (!gameState) return { gameName: '', status: 'connecting' };
  return {
    gameName: gameState.gameName || 'Game',
    status: gameState.isSpectator
      ? 'spectating'
      : gameState.isStarted
        ? 'playing'
        : 'lobby',
  };
}

const statusConfig: Record<GameWindowStatus, { label: string; color: string; dotColor: string; borderColor: string; animate?: boolean }> = {
  connecting: {
    label: 'Connecting',
    color: 'text-octgn-warning',
    dotColor: 'bg-octgn-warning',
    borderColor: 'border-octgn-warning/30',
    animate: true,
  },
  lobby: {
    label: 'In Lobby',
    color: 'text-octgn-primary',
    dotColor: 'bg-octgn-primary',
    borderColor: 'border-octgn-primary/30',
  },
  spectating: {
    label: 'Spectating',
    color: 'text-octgn-accent',
    dotColor: 'bg-octgn-accent',
    borderColor: 'border-octgn-accent/30',
    animate: true,
  },
  playing: {
    label: 'In Game',
    color: 'text-octgn-success',
    dotColor: 'bg-octgn-success',
    borderColor: 'border-octgn-success/30',
  },
};

const TitleBar: React.FC<TitleBarProps> = ({ isGameWindow }) => {
  const minimize = useAppStore((s) => s.minimize);
  const maximize = useAppStore((s) => s.maximize);
  const quit = useAppStore((s) => s.quit);
  const { gameName, status } = useGameWindowInfo();

  const handleMinimize = useCallback(() => {
    minimize();
  }, [minimize]);

  const handleMaximize = useCallback(() => {
    maximize();
  }, [maximize]);

  const handleClose = useCallback(() => {
    if (isGameWindow) {
      if (window.confirm('Leave the game?')) {
        window.octgn.closeGameWindow();
      }
    } else {
      quit();
    }
  }, [quit, isGameWindow]);

  const cfg = statusConfig[status];

  return (
    <header className="drag-region flex items-center justify-between h-9 px-3 bg-octgn-surface/80 backdrop-blur-md border-b border-octgn-border/50 shrink-0 z-50">
      {/* Logo + game info */}
      <div className="flex items-center gap-2 min-w-0">
        <span className="font-display text-sm font-bold tracking-widest text-octgn-primary glow-text-blue shrink-0">
          OCTGN
        </span>

        {isGameWindow ? (
          <>
            <span className="text-octgn-border text-[10px] shrink-0">/</span>
            <span className="text-[11px] text-octgn-text-muted font-medium truncate max-w-[200px]">
              {gameName || 'Game'}
            </span>
            <span className={clsx(
              'inline-flex items-center gap-1 px-1.5 py-px rounded-sm border text-[9px] font-bold tracking-wider uppercase shrink-0',
              cfg.color,
              cfg.borderColor,
              'bg-white/[0.03]',
            )}>
              <span className={clsx(
                'w-1.5 h-1.5 rounded-full shrink-0',
                cfg.dotColor,
                cfg.animate && 'animate-pulse',
              )} />
              {cfg.label}
            </span>
          </>
        ) : (
          <span className="text-[10px] text-octgn-text-dim font-medium tracking-wide uppercase">
            Electron
          </span>
        )}
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
