import React from 'react';
import { clsx } from 'clsx';
import { readablePlayerColor } from '../utils/player-colors';
import type { Counter, Group } from '../../shared/types';

export interface PlayerRowHeaderProps {
  playerId: number | 'global';
  playerName: string;
  playerColor: string;
  isLocal: boolean;
  isSpectator: boolean;
  isExpanded: boolean;
  counters: Counter[];
  visibleGroups: Group[];
  onToggle: () => void;
}

const PlayerRowHeader: React.FC<PlayerRowHeaderProps> = ({
  playerId,
  playerName,
  playerColor,
  isLocal,
  isSpectator,
  isExpanded,
  counters,
  visibleGroups,
  onToggle,
}) => {
  const readableColor = readablePlayerColor(playerColor);
  const isGlobal = playerId === 'global';
  const testId = isGlobal ? 'tab-global' : `tab-player-${playerId}`;
  const totalCards = visibleGroups.reduce((sum, g) => sum + g.cards.length, 0);

  return (
    <button
      data-testid={testId}
      onClick={onToggle}
      className={clsx(
        'group w-full flex items-center gap-2.5 px-3 sm:px-4 py-2 text-left',
        'transition-all duration-200 ease-out',
        'hover:bg-white/[0.03]',
        isExpanded
          ? 'bg-white/[0.02]'
          : 'active:bg-white/[0.04]',
      )}
      style={{
        borderLeftWidth: 3,
        borderLeftStyle: 'solid',
        borderLeftColor: isExpanded ? playerColor : 'transparent',
        transition: 'border-color 0.2s ease, background-color 0.2s ease',
      }}
    >
      {/* Chevron — rotates on expand */}
      <svg
        className={clsx(
          'w-2.5 h-2.5 shrink-0 transition-transform duration-200 ease-out',
          isExpanded
            ? 'rotate-90 text-octgn-text-muted'
            : 'text-octgn-text-dim/40 group-hover:text-octgn-text-dim/70',
        )}
        viewBox="0 0 6 10"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <path d="M1 1l4 4-4 4" />
      </svg>

      {/* Player identity — color dot or globe icon */}
      {isGlobal ? (
        <svg
          className={clsx(
            'w-3.5 h-3.5 shrink-0 transition-opacity duration-200',
            isExpanded ? 'opacity-70 text-octgn-text-dim' : 'opacity-30 group-hover:opacity-50 text-octgn-text-dim',
          )}
          viewBox="0 0 16 16"
        >
          <circle cx="8" cy="8" r="5.5" stroke="currentColor" strokeWidth="1.2" fill="none" />
          <path d="M2.5 8h11M8 2.5c-1.8 1.8-1.8 3.5-1.8 5.5s0 3.7 1.8 5.5M8 2.5c1.8 1.8 1.8 3.5 1.8 5.5s0 3.7-1.8 5.5" stroke="currentColor" strokeWidth="0.8" fill="none" />
        </svg>
      ) : (
        <div className="relative shrink-0">
          <div
            className={clsx(
              'w-2.5 h-2.5 rounded-full transition-all duration-200',
              isExpanded && 'scale-110',
            )}
            style={{
              backgroundColor: playerColor,
              boxShadow: isExpanded ? `0 0 8px ${playerColor}50, 0 0 2px ${playerColor}80` : undefined,
            }}
          />
        </div>
      )}

      {/* Player name */}
      <span
        className={clsx(
          'text-xs font-semibold truncate transition-colors duration-200',
          isExpanded ? '' : 'text-octgn-text-dim group-hover:text-octgn-text-muted',
        )}
        style={isExpanded ? { color: readableColor } : undefined}
      >
        {playerName}
        {isLocal && !isSpectator && (
          <span className="ml-1 text-[9px] opacity-40 font-normal">(you)</span>
        )}
      </span>

      {/* Counter pills — compact, visible on desktop */}
      {!isGlobal && counters.length > 0 && (
        <div className="hidden sm:flex items-center gap-2 ml-0.5">
          {counters.slice(0, 3).map((c) => (
            <span
              key={c.id}
              className="flex items-center gap-1 text-[9px] text-octgn-text-dim whitespace-nowrap"
            >
              <span className="uppercase tracking-wider opacity-60">{c.name}</span>
              <span className="font-mono font-bold text-octgn-text-muted tabular-nums">{c.value}</span>
            </span>
          ))}
        </div>
      )}

      <div className="flex-1 min-w-0" />

      {/* Group summary pills — at-a-glance card counts per group */}
      <div className="flex items-center gap-1 overflow-hidden shrink-0">
        {visibleGroups.map((g) => (
          <span
            key={g.id}
            className={clsx(
              'hidden sm:inline-flex items-center gap-0.5 text-[9px] whitespace-nowrap',
              'px-1.5 py-0.5 rounded',
              'bg-white/[0.025] border border-white/[0.04]',
              'text-octgn-text-dim/70',
              'transition-colors duration-200 group-hover:text-octgn-text-dim',
            )}
          >
            <span className="truncate max-w-[48px]">{g.name}</span>
            <span className="font-mono font-bold text-octgn-text-muted/70 tabular-nums">{g.cards.length}</span>
          </span>
        ))}

        {/* Total card count — always visible including mobile */}
        <span
          className={clsx(
            'text-[10px] font-mono font-bold tabular-nums ml-1 px-1.5 py-0.5 rounded',
            totalCards > 0
              ? 'text-octgn-text-dim/60 bg-white/[0.02]'
              : 'text-octgn-text-dim/30',
          )}
        >
          {totalCards}
        </span>
      </div>
    </button>
  );
};

export default PlayerRowHeader;
