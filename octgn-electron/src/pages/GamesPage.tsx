import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, Badge, EmptyState } from '../components';
import { 
  useGameFeeds, 
  useInstalledGames, 
  useGameSearch,
  GamePackage 
} from '../services/GameFeedService';

export default function GamesPage() {
  const navigate = useNavigate();
  const { feeds, allGames, loading, refreshAll } = useGameFeeds();
  const { games: installedGames, install, uninstall } = useInstalledGames();
  
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<'all' | 'installed' | 'available'>('all');
  const [selectedGame, setSelectedGame] = useState<GamePackage | null>(null);
  const [installing, setInstalling] = useState<string | null>(null);
  const [uninstalling, setUninstalling] = useState<string | null>(null);

  // Initial load
  useEffect(() => {
    if (allGames.length === 0 && !loading) {
      refreshAll();
    }
  }, [allGames.length, loading, refreshAll]);

  // Filter games
  const filteredGames = allGames.filter((game) => {
    // Search filter
    if (search) {
      const term = search.toLowerCase();
      const matches =
        game.name.toLowerCase().includes(term) ||
        game.description?.toLowerCase().includes(term) ||
        game.tags?.some((tag) => tag.toLowerCase().includes(term));
      if (!matches) return false;
    }

    // Status filter
    switch (filter) {
      case 'installed':
        return game.installed;
      case 'available':
        return !game.installed;
      default:
        return true;
    }
  });

  // Handle install
  const handleInstall = async (gameId: string) => {
    setInstalling(gameId);
    try {
      await install(gameId);
    } catch (error: any) {
      alert(`Failed to install: ${error.message}`);
    } finally {
      setInstalling(null);
    }
  };

  // Handle uninstall
  const handleUninstall = async (gameId: string) => {
    if (!confirm('Are you sure you want to uninstall this game?')) return;
    
    setUninstalling(gameId);
    try {
      await uninstall(gameId);
    } catch (error: any) {
      alert(`Failed to uninstall: ${error.message}`);
    } finally {
      setUninstalling(null);
    }
  };

  const installedCount = installedGames.length;

  return (
    <div className="h-full flex">
      {/* Main Content */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-octgn-accent/30">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-3xl font-bold text-white">Game Library</h1>
              <p className="text-gray-400 mt-1">
                {allGames.length} games available • {installedCount} installed
              </p>
            </div>
            
            <div className="flex items-center space-x-3">
              <Button variant="secondary" onClick={refreshAll} loading={loading}>
                🔄 Refresh
              </Button>
              <Button
                variant="secondary"
                onClick={() => navigate('/deckeditor')}
              >
                🃏 Deck Editor
              </Button>
            </div>
          </div>

          {/* Search and Filter */}
          <div className="flex space-x-4">
            <div className="flex-1 relative">
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search games..."
                className="input w-full pl-10"
              />
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500">
                🔍
              </span>
            </div>

            <div className="flex rounded-lg overflow-hidden border border-octgn-accent">
              {[
                { id: 'all', label: `All (${allGames.length})` },
                { id: 'installed', label: `Installed (${installedCount})` },
                { id: 'available', label: 'Available' },
              ].map((f) => (
                <button
                  key={f.id}
                  onClick={() => setFilter(f.id as any)}
                  className={`px-4 py-2 text-sm font-medium transition-colors ${
                    filter === f.id
                      ? 'bg-octgn-highlight text-white'
                      : 'bg-octgn-primary text-gray-300 hover:bg-octgn-accent'
                  }`}
                >
                  {f.label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Games Grid */}
        <div className="flex-1 overflow-y-auto p-6">
          {loading && allGames.length === 0 ? (
            <div className="text-center py-16">
              <div className="w-12 h-12 border-4 border-octgn-highlight border-t-transparent rounded-full animate-spin mx-auto mb-4" />
              <p className="text-gray-400">Loading games from feeds...</p>
            </div>
          ) : filteredGames.length === 0 ? (
            <EmptyState
              icon="📦"
              title="No games found"
              description={
                search
                  ? 'Try a different search term'
                  : filter === 'installed'
                    ? 'No games installed yet'
                    : 'Refresh to load games from feeds'
              }
              action={
                !search && (
                  <Button variant="primary" onClick={refreshAll}>
                    Refresh Feeds
                  </Button>
                )
              }
            />
          ) : (
            <div className="grid grid-cols-3 gap-6">
              {filteredGames.map((game) => (
                <div
                  key={`${game.id}-${game.version}`}
                  className={`card cursor-pointer overflow-hidden transition-all ${
                    selectedGame?.id === game.id ? 'ring-2 ring-octgn-highlight' : ''
                  }`}
                  onClick={() => setSelectedGame(game)}
                >
                  {/* Game Header */}
                  <div className="relative h-32 bg-gradient-to-br from-octgn-highlight/20 to-octgn-blue/20 flex items-center justify-center">
                    {game.iconUrl ? (
                      <img
                        src={game.iconUrl}
                        alt={game.name}
                        className="h-20 w-20 object-contain"
                      />
                    ) : (
                      <span className="text-6xl">🃏</span>
                    )}
                    
                    {game.installed && (
                      <div className="absolute top-3 right-3">
                        <Badge variant="success">Installed</Badge>
                      </div>
                    )}
                  </div>

                  {/* Game Info */}
                  <div className="p-4">
                    <h3 className="text-lg font-bold text-white mb-1 truncate">
                      {game.name}
                    </h3>
                    <p className="text-sm text-gray-400 mb-3 line-clamp-2">
                      {game.summary || game.description || 'No description available'}
                    </p>
                    
                    {/* Tags */}
                    {game.tags && game.tags.length > 0 && (
                      <div className="flex flex-wrap gap-1 mb-3">
                        {game.tags.slice(0, 3).map((tag) => (
                          <span
                            key={tag}
                            className="text-xs px-2 py-0.5 rounded-full bg-octgn-accent/30 text-gray-300"
                          >
                            {tag}
                          </span>
                        ))}
                        {game.tags.length > 3 && (
                          <span className="text-xs text-gray-500">
                            +{game.tags.length - 3}
                          </span>
                        )}
                      </div>
                    )}

                    {/* Stats */}
                    <div className="flex items-center justify-between text-xs text-gray-500">
                      <span>v{game.version}</span>
                      {game.authors && (
                        <span className="truncate ml-2">
                          by {game.authors[0]}
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="p-4 pt-0">
                    {game.installed ? (
                      <div className="flex space-x-2">
                        <Button
                          variant="primary"
                          size="sm"
                          className="flex-1"
                          onClick={(e) => {
                            e.stopPropagation();
                            navigate('/play');
                          }}
                        >
                          Play
                        </Button>
                        <Button
                          variant="danger"
                          size="sm"
                          loading={uninstalling === game.id}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleUninstall(game.id);
                          }}
                        >
                          🗑️
                        </Button>
                      </div>
                    ) : (
                      <Button
                        variant="secondary"
                        size="sm"
                        className="w-full"
                        loading={installing === game.id}
                        onClick={(e) => {
                          e.stopPropagation();
                          handleInstall(game.id);
                        }}
                      >
                        ⬇️ Install
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Sidebar - Selected Game Details */}
      {selectedGame && (
        <div className="w-80 border-l border-octgn-accent/30 bg-octgn-primary/50 p-6 overflow-y-auto">
          <button
            onClick={() => setSelectedGame(null)}
            className="absolute top-4 right-4 text-gray-400 hover:text-white"
          >
            ✕
          </button>

          <div className="text-center mb-6">
            <div className="w-24 h-24 rounded-2xl bg-gradient-to-br from-octgn-highlight/30 to-octgn-blue/30 flex items-center justify-center mx-auto mb-4">
              {selectedGame.iconUrl ? (
                <img
                  src={selectedGame.iconUrl}
                  alt={selectedGame.name}
                  className="w-16 h-16 object-contain"
                />
              ) : (
                <span className="text-5xl">🃏</span>
              )}
            </div>
            <h2 className="text-2xl font-bold text-white">{selectedGame.name}</h2>
            <p className="text-gray-400 text-sm">v{selectedGame.version}</p>
          </div>

          {selectedGame.description && (
            <p className="text-gray-300 text-sm mb-6">{selectedGame.description}</p>
          )}

          <div className="space-y-3 mb-6">
            {selectedGame.authors && (
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Authors</span>
                <span className="text-white">{selectedGame.authors.join(', ')}</span>
              </div>
            )}
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Status</span>
              <span className={selectedGame.installed ? 'text-octgn-success' : 'text-gray-400'}>
                {selectedGame.installed ? 'Installed' : 'Not Installed'}
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Feed</span>
              <span className="text-white text-xs truncate ml-2">
                {selectedGame.feedUrl.split('/').filter(Boolean).pop()}
              </span>
            </div>
          </div>

          <div className="space-y-2">
            {selectedGame.installed ? (
              <>
                <Button
                  variant="primary"
                  className="w-full"
                  onClick={() => navigate('/play')}
                >
                  🎮 Play Game
                </Button>
                <Button
                  variant="secondary"
                  className="w-full"
                  onClick={() => navigate('/deckeditor')}
                >
                  🃏 Deck Editor
                </Button>
                <Button
                  variant="ghost"
                  className="w-full text-red-400"
                  loading={uninstalling === selectedGame.id}
                  onClick={() => handleUninstall(selectedGame.id)}
                >
                  🗑️ Uninstall
                </Button>
              </>
            ) : (
              <Button
                variant="primary"
                className="w-full"
                loading={installing === selectedGame.id}
                onClick={() => handleInstall(selectedGame.id)}
              >
                ⬇️ Install
              </Button>
            )}
          </div>

          {selectedGame.projectUrl && (
            <a
              href={selectedGame.projectUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="block text-center text-octgn-highlight text-sm mt-4 hover:underline"
            >
              View Project Page →
            </a>
          )}
        </div>
      )}
    </div>
  );
}
