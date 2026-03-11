export default function PlayerList({
  players,
  currentPlayerId,
  activePlayerId,
}: {
  players: Array<{
    id: string;
    name: string;
    ready?: boolean;
    spectator?: boolean;
    disconnected?: boolean;
  }>;
  currentPlayerId?: string;
  activePlayerId?: string | null;
}) {
  return (
    <div className="space-y-2">
      <h3 className="text-sm font-semibold text-gray-400 uppercase tracking-wide mb-3">
        Players ({players.length})
      </h3>
      
      {players.length === 0 ? (
        <p className="text-gray-500 text-sm text-center py-4">No players</p>
      ) : (
        players.map((player) => {
          const isMe = player.id === currentPlayerId;
          const isActive = player.id === activePlayerId;
          
          return (
            <div
              key={player.id}
              className={`
                flex items-center space-x-3 p-3 rounded-lg transition-all
                ${isMe 
                  ? 'bg-octgn-highlight/10 border border-octgn-highlight/30' 
                  : 'bg-octgn-dark/50 hover:bg-octgn-accent/20'
                }
                ${isActive ? 'ring-2 ring-octgn-highlight/50' : ''}
              `}
            >
              {/* Avatar */}
              <div className="relative">
                <div
                  className="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold"
                  style={{
                    background: `linear-gradient(135deg, hsl(${player.name.charCodeAt(0) * 137}, 50%, 40%), hsl(${player.name.charCodeAt(0) * 137 + 40}, 50%, 35%))`,
                  }}
                >
                  {player.name.slice(0, 2).toUpperCase()}
                </div>
                
                {/* Status indicator */}
                <div className={`
                  absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 rounded-full border-2 border-octgn-dark
                  ${player.disconnected 
                    ? 'bg-gray-500' 
                    : player.ready 
                      ? 'bg-octgn-success' 
                      : 'bg-octgn-warning'
                  }
                `} />
              </div>
              
              {/* Info */}
              <div className="flex-1 min-w-0">
                <div className="flex items-center space-x-2">
                  <span className={`font-medium truncate ${isMe ? 'text-octgn-highlight' : 'text-white'}`}>
                    {player.name}
                  </span>
                  {isMe && (
                    <span className="text-xs px-1.5 py-0.5 rounded bg-octgn-highlight/30 text-octgn-highlight">
                      You
                    </span>
                  )}
                  {player.spectator && (
                    <span className="text-xs">👁️</span>
                  )}
                </div>
                
                <div className="flex items-center space-x-2 mt-0.5">
                  {isActive && (
                    <span className="text-xs text-octgn-highlight">● Active</span>
                  )}
                  {player.disconnected ? (
                    <span className="text-xs text-gray-500">Disconnected</span>
                  ) : player.ready ? (
                    <span className="text-xs text-octgn-success">Ready</span>
                  ) : (
                    <span className="text-xs text-octgn-warning">Not Ready</span>
                  )}
                </div>
              </div>
            </div>
          );
        })
      )}
    </div>
  );
}

// Compact player list for sidebar
export function PlayerListCompact({
  players,
  currentPlayerId,
}: {
  players: Array<{ id: string; name: string }>;
  currentPlayerId?: string;
}) {
  return (
    <div className="flex -space-x-2">
      {players.slice(0, 5).map((player) => (
        <div
          key={player.id}
          className="w-8 h-8 rounded-full border-2 border-octgn-dark flex items-center justify-center text-white text-xs font-bold"
          style={{
            background: `linear-gradient(135deg, hsl(${player.name.charCodeAt(0) * 137}, 50%, 40%), hsl(${player.name.charCodeAt(0) * 137 + 40}, 50%, 35%))`,
          }}
          title={player.name}
        >
          {player.name[0].toUpperCase()}
        </div>
      ))}
      {players.length > 5 && (
        <div className="w-8 h-8 rounded-full border-2 border-octgn-dark bg-octgn-accent flex items-center justify-center text-white text-xs">
          +{players.length - 5}
        </div>
      )}
    </div>
  );
}
