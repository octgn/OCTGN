import { Player } from '../types/game';

interface PlayerListProps {
  players: Player[];
  currentPlayerId: string | null;
  activePlayerId: string | null;
  onPlayerClick?: (player: Player) => void;
  className?: string;
}

export default function PlayerList({
  players,
  currentPlayerId,
  activePlayerId,
  onPlayerClick,
  className = '',
}: PlayerListProps) {
  return (
    <div className={`player-list ${className}`}>
      <h3 className="text-sm font-bold text-white mb-2 px-2">Players</h3>
      <div className="space-y-1">
        {players.map((player) => {
          const isCurrentPlayer = player.id === currentPlayerId;
          const isActivePlayer = player.id === activePlayerId;

          return (
            <div
              key={player.id}
              onClick={() => onPlayerClick?.(player)}
              className={`
                flex items-center justify-between px-3 py-2 rounded-lg cursor-pointer
                transition-colors
                ${isCurrentPlayer ? 'bg-octgn-highlight/30' : 'hover:bg-octgn-accent/50'}
                ${isActivePlayer ? 'ring-2 ring-octgn-highlight' : ''}
              `}
            >
              <div className="flex items-center space-x-2">
                {/* Player color indicator */}
                <div
                  className="w-3 h-3 rounded-full"
                  style={{ backgroundColor: player.color || '#6b7280' }}
                />

                {/* Player name */}
                <span className={`text-sm ${isCurrentPlayer ? 'text-white font-medium' : 'text-gray-300'}`}>
                  {player.name}
                </span>

                {/* Spectator badge */}
                {player.spectator && (
                  <span className="text-xs text-gray-500">👁️</span>
                )}

                {/* Ready indicator */}
                {player.ready && (
                  <span className="text-xs text-green-500">✓</span>
                )}

                {/* Active turn indicator */}
                {isActivePlayer && (
                  <span className="text-xs text-octgn-highlight animate-pulse">
                    ⏳
                  </span>
                )}
              </div>

              {/* Table side indicator */}
              {!player.spectator && (
                <span className="text-xs text-gray-500">
                  {player.tableSide ? '⬇️' : '⬆️'}
                </span>
              )}
            </div>
          );
        })}

        {players.length === 0 && (
          <div className="text-gray-400 text-sm text-center py-4">
            No players connected
          </div>
        )}
      </div>
    </div>
  );
}
