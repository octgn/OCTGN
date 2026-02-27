import React, { useState, useCallback, useEffect } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import AppSidebar from '../components/AppSidebar';
import CreateGameDialog from '../components/CreateGameDialog';
import { useLobbyStore } from '../stores/lobby-store';
import { useDefinitionsStore } from '../stores/definitions-store';
import type { HostedGame } from '../../shared/types';

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
    async (game: HostedGame) => {
      await joinGame(game.id);
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
              className="group hover:bg-octgn-surface-light/50 transition-colors duration-150 cursor-pointer"
            >
              <div className="flex items-center gap-4 px-4 py-3">
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
                  <p className="text-xs text-octgn-text-dim mt-0.5">
                    {game.gameName}
                    <span className="mx-1.5 text-octgn-border">|</span>
                    Hosted by {game.hostUser.username}
                  </p>
                </div>

                {/* Players */}
                <div className="text-xs text-octgn-text-muted whitespace-nowrap">
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

                {/* Join */}
                <Button
                  variant="primary"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleJoin(game);
                  }}
                  className="opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  Join
                </Button>
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

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="6.5" cy="6.5" r="4.5" />
      <path d="M10 10l4 4" />
    </svg>
  );
}

export default LobbyPage;
