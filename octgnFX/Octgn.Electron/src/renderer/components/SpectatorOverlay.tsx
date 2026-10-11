import React from 'react';
import { clsx } from 'clsx';

interface Player {
  id: string;
  username: string;
}

interface SpectatorOverlayProps {
  /** Whether the overlay is visible */
  visible: boolean;
  /** List of players in the game */
  players: Player[];
  /** ID of the player currently being viewed */
  activePlayerId: string;
  /** Called when the spectator switches to a different player's view */
  onSwitchPlayer: (playerId: string) => void;
  /** Optional extra className */
  className?: string;
}

const SpectatorOverlay: React.FC<SpectatorOverlayProps> = ({
  visible,
  players,
  activePlayerId,
  onSwitchPlayer,
  className,
}) => {
  if (!visible) return null;

  const activePlayer = players.find((p) => p.id === activePlayerId);

  return (
    <div
      className={clsx(
        'absolute top-0 left-0 right-0 z-50',
        'bg-octgn-surface/80 backdrop-blur-md border-b border-octgn-border/40',
        'px-5 py-2.5',
        'animate-fade-in',
        className
      )}
    >
      <div className="flex items-center gap-4">
        {/* Spectating badge */}
        <div className="flex items-center gap-2 shrink-0">
          <EyeIcon />
          <span className="text-[10px] font-bold uppercase tracking-[0.15em] text-octgn-warning">
            Spectating
          </span>
        </div>

        {/* Divider */}
        <div className="w-px h-5 bg-octgn-border/40" />

        {/* Currently watching */}
        <div className="flex items-center gap-2 min-w-0">
          <span className="text-xs text-octgn-text-dim whitespace-nowrap">Watching:</span>
          <span className="text-sm font-semibold text-octgn-text truncate">
            {activePlayer?.username || 'Unknown'}
          </span>
        </div>

        {/* Spacer */}
        <div className="flex-1" />

        {/* Player switch buttons */}
        <div className="flex items-center gap-1.5">
          <span className="text-[10px] text-octgn-text-dim uppercase tracking-wide mr-1">
            Switch to:
          </span>
          {players.map((player) => (
            <button
              key={player.id}
              onClick={() => onSwitchPlayer(player.id)}
              className={clsx(
                'px-2.5 py-1 rounded-md text-xs font-medium transition-all duration-150',
                player.id === activePlayerId
                  ? 'bg-octgn-primary/20 text-octgn-primary border border-octgn-primary/40'
                  : 'bg-white/5 text-octgn-text-muted border border-octgn-border/30 hover:bg-white/10 hover:text-octgn-text'
              )}
            >
              {player.username}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
};

/* -- Inline SVG icon -------------------------------------------------------- */

function EyeIcon() {
  return (
    <svg
      className="w-4 h-4 text-octgn-warning"
      viewBox="0 0 16 16"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.3"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M1 8s2.5-5 7-5 7 5 7 5-2.5 5-7 5-7-5-7-5z" />
      <circle cx="8" cy="8" r="2.5" />
    </svg>
  );
}

export default SpectatorOverlay;
