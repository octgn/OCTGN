import React, { useState, useCallback, useEffect } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import AppSidebar from '../components/AppSidebar';
import CreateGameDialog from '../components/CreateGameDialog';
import { useLobbyStore } from '../stores/lobby-store';
import { useDefinitionsStore } from '../stores/definitions-store';
import type { HostedGame, User } from '../../shared/types';
import { GameStatus } from '../../shared/types';

const statusLabel: Record<number, string> = {
  0: 'Unknown',
  1: 'Ready',
  2: 'In Progress',
  3: 'Finished',
};

const statusColor: Record<number, string> = {
  0: 'bg-octgn-text-dim/30 text-octgn-text-dim',
  1: 'bg-octgn-success/15 text-octgn-success border border-octgn-success/30',
  2: 'bg-octgn-warning/15 text-octgn-warning border border-octgn-warning/30',
  3: 'bg-octgn-text-dim/15 text-octgn-text-dim border border-octgn-text-dim/30',
};

const LobbyPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateGame, setShowCreateGame] = useState(false);
  const { games, isLoading, joinGame, startAutoRefresh } = useLobbyStore();
  const { installedGames, loadInstalled } = useDefinitionsStore();

  useEffect(() => {
    const cleanup = startAutoRefresh();
    loadInstalled();
    return cleanup;
  }, [startAutoRefresh, loadInstalled]);

  const filteredGames = games.filter(
    (g: HostedGame) =>
      g.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      g.gameName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      g.hostUser.username.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleJoin = useCallback(
    async (game: HostedGame, spectator: boolean = false) => {
      await joinGame(game.id, undefined, spectator);
    },
    [joinGame]
  );

  return (
    <div className="flex h-full">
      <AppSidebar activePage="lobby" />

      {/* Main */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Toolbar */}
        <div className="flex items-center gap-3 px-5 py-3 border-b border-octgn-border/30">
          <h2 className="font-display text-lg font-semibold tracking-wide text-octgn-text">
            Game Lobby
          </h2>
          <div className="flex-1" />

          {/* Search */}
          <div className="relative">
            <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-octgn-text-dim" />
            <input
              type="text"
              placeholder="Search games..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="h-8 pl-9 pr-3 w-56 rounded-lg bg-octgn-surface border border-octgn-border/50 text-xs text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 focus:shadow-[0_0_10px_rgba(59,130,246,0.15)] transition-all"
            />
          </div>

          {/* Refresh indicator */}
          <div className="flex items-center gap-1.5 text-[10px] text-octgn-text-dim">
            <span
              className={clsx(
                'w-1.5 h-1.5 rounded-full',
                isLoading ? 'bg-octgn-warning animate-pulse' : 'bg-octgn-success'
              )}
            />
            {isLoading ? 'Refreshing' : 'Live'}
          </div>

          <Button variant="primary" size="sm" onClick={() => setShowCreateGame(true)}>
            Host Game
          </Button>
        </div>

        {/* Games list */}
        <div className="flex-1 overflow-y-auto p-4 space-y-2">
          {filteredGames.length === 0 && !isLoading && (
            <div className="flex flex-col items-center justify-center h-full text-octgn-text-dim">
              <p className="text-sm">No games found</p>
              <p className="text-xs mt-1">Host a game or wait for others to appear</p>
            </div>
          )}

          {filteredGames.map((game: HostedGame) => (
            <GlassPanel
              key={game.id}
              variant="light"
              padding="none"
              glow="none"
              className="group hover:bg-octgn-surface-light/50 transition-all duration-200 cursor-pointer hover:shadow-[0_0_24px_rgba(59,130,246,0.06)]"
            >
              <div className="flex items-center gap-3 px-4 py-3">
                {/* Host avatar */}
                <HostAvatar user={game.hostUser} />

                {/* Game info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-semibold text-octgn-text truncate">{game.name}</p>
                    {game.hasPassword && (
                      <svg className="w-3 h-3 text-octgn-text-dim shrink-0" viewBox="0 0 16 16" fill="currentColor">
                        <path d="M11.5 6V4.5a3.5 3.5 0 10-7 0V6H3v7.5A1.5 1.5 0 004.5 15h7a1.5 1.5 0 001.5-1.5V6h-1.5zm-5.5 0V4.5a2 2 0 114 0V6H6z" />
                      </svg>
                    )}
                  </div>
                  <div className="flex items-center gap-1.5 mt-0.5">
                    <span className="text-xs text-octgn-text-dim truncate">{game.gameName}</span>
                    <span className="text-octgn-border text-[10px]">/</span>
                    <span className="text-xs text-octgn-primary/80 font-medium truncate">{game.hostUser.username}</span>
                  </div>
                </div>

                {/* Players */}
                <div className="flex items-center gap-1 text-xs text-octgn-text-muted whitespace-nowrap">
                  <svg className="w-3 h-3 opacity-50" viewBox="0 0 16 16" fill="currentColor">
                    <path d="M8 8a3 3 0 100-6 3 3 0 000 6zm5 6H3s-1 0-1-1 1-4 6-4 6 3 6 4-1 1-1 1z" />
                  </svg>
                  {game.playerCount}/{game.maxPlayers}
                </div>

                {/* Status badge */}
                <span
                  className={clsx(
                    'px-2 py-0.5 rounded-full text-[10px] font-medium whitespace-nowrap',
                    statusColor[game.status as number]
                  )}
                >
                  {statusLabel[game.status as number]}
                </span>

                {/* Spectator indicator */}
                {game.spectators && (
                  <span className="text-[10px] text-octgn-text-dim" title="Spectators allowed">
                    <svg className="w-3 h-3 inline" viewBox="0 0 16 16" fill="currentColor">
                      <path d="M8 3C4.5 3 1.5 5.5 0 8c1.5 2.5 4.5 5 8 5s6.5-2.5 8-5c-1.5-2.5-4.5-5-8-5zm0 8a3 3 0 110-6 3 3 0 010 6zm0-5a2 2 0 100 4 2 2 0 000-4z" />
                    </svg>
                  </span>
                )}

                {/* Join / Watch */}
                {game.status === GameStatus.InProgress && game.spectators ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleJoin(game, true);
                    }}
                    className="opacity-0 group-hover:opacity-100 transition-opacity"
                  >
                    Watch
                  </Button>
                ) : game.status === GameStatus.GameReady ||
                  game.status === GameStatus.Unknown ? (
                  <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={(e) => {
                        e.stopPropagation();
                        handleJoin(game);
                      }}
                    >
                      Join
                    </Button>
                    {game.spectators && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleJoin(game, true);
                        }}
                      >
                        Watch
                      </Button>
                    )}
                  </div>
                ) : null}
              </div>
            </GlassPanel>
          ))}
        </div>
      </div>

      {/* Create Game Dialog */}
      {showCreateGame && (
        <CreateGameDialog
          onClose={() => setShowCreateGame(false)}
          gameDefinitions={installedGames.map((g) => ({
            id: g.id,
            name: g.name,
            version: g.version,
            maxPlayers: g.players.length > 0 ? g.players.length * 4 : 8,
          }))}
        />
      )}
    </div>
  );
};

const avatarColors = [
  'from-blue-500/80 to-blue-700/80',
  'from-violet-500/80 to-violet-700/80',
  'from-emerald-500/80 to-emerald-700/80',
  'from-amber-500/80 to-amber-700/80',
  'from-rose-500/80 to-rose-700/80',
  'from-cyan-500/80 to-cyan-700/80',
  'from-fuchsia-500/80 to-fuchsia-700/80',
  'from-teal-500/80 to-teal-700/80',
];

function hashToIndex(str: string): number {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = ((hash << 5) - hash + str.charCodeAt(i)) | 0;
  }
  return Math.abs(hash) % avatarColors.length;
}

function HostAvatar({ user }: { user: User }) {
  const initial = (user.username || '?')[0].toUpperCase();
  const colorIdx = hashToIndex(user.id || user.username || '');

  if (user.iconUrl) {
    return (
      <div className="relative shrink-0">
        <img
          src={user.iconUrl}
          alt={user.username}
          className="w-8 h-8 rounded-full object-cover ring-1 ring-white/10"
        />
      </div>
    );
  }

  return (
    <div
      className={clsx(
        'w-8 h-8 rounded-full shrink-0 flex items-center justify-center',
        'bg-gradient-to-br text-white/90 text-xs font-bold tracking-wide',
        'ring-1 ring-white/10',
        avatarColors[colorIdx],
      )}
    >
      {initial}
    </div>
  );
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="6.5" cy="6.5" r="4.5" />
      <path d="M10 10l4 4" />
    </svg>
  );
}

export default LobbyPage;
