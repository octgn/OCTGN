interface TurnIndicatorProps {
  turnNumber: number;
  activePlayerName: string | null;
  phaseName?: string;
  onNextTurn?: () => void;
  onSetPhase?: (phase: number) => void;
  phases?: { name: string; icon?: string }[];
  currentPhase?: number;
  isMyTurn: boolean;
}

export default function TurnIndicator({
  turnNumber,
  activePlayerName,
  phaseName,
  onNextTurn,
  onSetPhase,
  phases = [],
  currentPhase,
  isMyTurn,
}: TurnIndicatorProps) {
  return (
    <div className="turn-indicator bg-octgn-primary rounded-lg p-4 shadow-lg">
      {/* Turn number */}
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center space-x-2">
          <span className="text-gray-400 text-sm">Turn</span>
          <span className="text-2xl font-bold text-white">{turnNumber}</span>
        </div>

        {onNextTurn && isMyTurn && (
          <button
            onClick={onNextTurn}
            className="btn btn-primary text-sm px-3 py-1"
          >
            End Turn
          </button>
        )}
      </div>

      {/* Active player */}
      <div className="mb-3">
        {activePlayerName ? (
          <div className="flex items-center space-x-2">
            <div className={`w-2 h-2 rounded-full ${isMyTurn ? 'bg-green-500 animate-pulse' : 'bg-yellow-500'}`} />
            <span className={`text-sm ${isMyTurn ? 'text-octgn-highlight font-bold' : 'text-gray-300'}`}>
              {isMyTurn ? 'Your Turn' : `${activePlayerName}'s Turn`}
            </span>
          </div>
        ) : (
          <span className="text-sm text-gray-500">No active player</span>
        )}
      </div>

      {/* Phases */}
      {phases.length > 0 && (
        <div className="border-t border-octgn-accent pt-3">
          <div className="flex flex-wrap gap-1">
            {phases.map((phase, index) => {
              const isActive = index === currentPhase;
              const isPast = currentPhase !== undefined && index < currentPhase;

              return (
                <button
                  key={index}
                  onClick={() => onSetPhase?.(index)}
                  disabled={!isMyTurn || !onSetPhase}
                  className={`
                    px-2 py-1 rounded text-xs font-medium transition-colors
                    ${isActive
                      ? 'bg-octgn-highlight text-white'
                      : isPast
                      ? 'bg-octgn-accent text-gray-300'
                      : 'bg-octgn-accent/30 text-gray-400 hover:bg-octgn-accent/50'
                    }
                    ${!isMyTurn ? 'cursor-not-allowed opacity-50' : 'cursor-pointer'}
                  `}
                >
                  {phase.icon && <span className="mr-1">{phase.icon}</span>}
                  {phase.name}
                </button>
              );
            })}
          </div>

          {/* Current phase name */}
          {phaseName && (
            <div className="mt-2 text-center text-sm text-gray-300">
              Current: <span className="font-bold text-white">{phaseName}</span>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
