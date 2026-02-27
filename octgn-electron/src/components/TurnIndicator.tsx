import { useEffect, useState } from 'react';

export default function TurnIndicator({
  turnNumber,
  activePlayerName,
  currentPhase,
  isMyTurn,
  phases,
  onNextTurn,
  onSetPhase,
}: {
  turnNumber: number;
  activePlayerName: string | null;
  currentPhase: string | null;
  isMyTurn: boolean;
  phases: string[];
  onNextTurn: () => void;
  onSetPhase: (phase: string) => void;
}) {
  const [pulse, setPulse] = useState(false);

  useEffect(() => {
    if (isMyTurn) {
      setPulse(true);
      const timer = setTimeout(() => setPulse(false), 1000);
      return () => clearTimeout(timer);
    }
  }, [isMyTurn, turnNumber]);

  return (
    <div className="glass rounded-xl p-4">
      {/* Turn Header */}
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center space-x-2">
          <span className="text-2xl">🎯</span>
          <div>
            <p className="text-xs text-gray-500 uppercase tracking-wide">Turn</p>
            <p className="text-2xl font-bold text-gradient">{turnNumber}</p>
          </div>
        </div>
        
        {isMyTurn && (
          <div className={`px-3 py-1 rounded-full bg-octgn-highlight/20 ${pulse ? 'animate-pulse' : ''}`}>
            <span className="text-octgn-highlight text-sm font-semibold">Your Turn</span>
          </div>
        )}
      </div>

      {/* Active Player */}
      {activePlayerName && (
        <div className="mb-3 p-3 rounded-lg bg-octgn-dark/50">
          <p className="text-xs text-gray-500 mb-1">Active Player</p>
          <p className={`font-semibold ${isMyTurn ? 'text-octgn-highlight' : 'text-white'}`}>
            {activePlayerName}
          </p>
        </div>
      )}

      {/* Phase Selector */}
      {phases.length > 0 && (
        <div className="mb-3">
          <p className="text-xs text-gray-500 mb-2">Phase</p>
          <div className="flex flex-wrap gap-1">
            {phases.map((phase) => (
              <button
                key={phase}
                onClick={() => onSetPhase(phase)}
                className={`
                  px-2 py-1 rounded text-xs font-medium transition-all
                  ${currentPhase === phase
                    ? 'bg-octgn-highlight text-white shadow-glow-sm'
                    : 'bg-octgn-accent/30 text-gray-400 hover:bg-octgn-accent/50 hover:text-white'
                  }
                `}
              >
                {phase}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Next Turn Button */}
      <button
        onClick={onNextTurn}
        disabled={!isMyTurn}
        className={`
          w-full py-2 rounded-lg font-semibold transition-all
          ${isMyTurn
            ? 'btn-primary hover:shadow-glow'
            : 'bg-octgn-accent/30 text-gray-500 cursor-not-allowed'
          }
        `}
      >
        {isMyTurn ? 'End Turn →' : 'Waiting...'}
      </button>
    </div>
  );
}

// Compact turn indicator for toolbar
export function TurnBadge({
  turnNumber,
  isMyTurn,
}: {
  turnNumber: number;
  isMyTurn: boolean;
}) {
  return (
    <div className={`
      flex items-center space-x-2 px-3 py-1.5 rounded-lg
      ${isMyTurn ? 'bg-octgn-highlight/20 shadow-glow-sm' : 'bg-octgn-accent/30'}
    `}>
      <span className="text-sm">🎯</span>
      <span className={`font-bold ${isMyTurn ? 'text-octgn-highlight' : 'text-gray-400'}`}>
        Turn {turnNumber}
      </span>
    </div>
  );
}
