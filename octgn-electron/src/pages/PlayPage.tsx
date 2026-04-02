import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { useHostedGames, HostedGame } from '../services/OctgnApiService';
import { Button, Modal, Badge, EmptyState, HostGameModal, JoinGameModal } from '../components';

type Tab = 'browse' | 'host' | 'history';

export default function PlayPage() {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuthStore();
  const { games, loading, error, refresh } = useHostedGames();
  
  const [activeTab, setActiveTab] = useState<Tab>('browse');
  const [showHostModal, setShowHostModal] = useState(false);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const [selectedGame, setSelectedGame] = useState<HostedGame | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filter, setFilter] = useState<'all' | 'staging' | 'playing' | 'no-password'>('all');

  // Redirect if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
    }
  }, [isAuthenticated, navigate]);

  // Filter games
  const filteredGames = games.filter((game) => {
    // Search filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      const matches = 
        game.name.toLowerCase().includes(term) ||
        game.gameName.toLowerCase().includes(term) ||
        game.hostUser.username.toLowerCase().includes(term);
      if (!matches) return false;
    }

    // Status filter
    switch (filter) {
      case 'staging':
        return game.status === 'Staging';
      case 'playing':
        return game.status === 'Playing';
      case 'no-password':
        return !game.hasPassword;
      default:
        return true;
    }
  });

  // Handle joining a game
  const handleJoinGame = useCallback((game: HostedGame) => {
    setSelectedGame(game);
    setShowJoinModal(true);
  }, []);

  // Handle hosting a game
  const handleHostGame = useCallback(() => {
    setShowHostModal(true);
  }, []);

  // Handle connecting to a game
  const handleConnect = useCallback((game: HostedGame, password?: string) => {
    // Navigate to game table with connection info
    const params = new URLSearchParams({
      host: game.hostAddress,
      port: String(game.port || 8888),
      game: game.gameId,
      gameName: game.gameName,
      name: game.name,
    });
    
    if (password) {
      params.set('password', password);
    }
    
    navigate(`/play/${game.id}?${params.toString()}`);
  }, [navigate]);

  // Start local game
  const handleLocalGame = useCallback(() => {
    navigate('/play/local');
  }, [navigate]);

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="bg-octgn-primary/50 border-b border-octgn-accent/30 p-6">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-3xl font-bold text-white">Play Games</h1>
            <p className="text-gray-400 mt-1">
              {games.length} games available • {loading ? 'Refreshing...' : 'Live'}
            </p>
          </div>
          
          <div className="flex items-center space-x-3">
            <Button variant="secondary" onClick={refresh} loading={loading}>
              🔄 Refresh
            </Button>
            <Button variant="secondary" onClick={handleLocalGame}>
              🎮 Local Game
            </Button>
            <Button variant="primary" onClick={handleHostGame}>
              ➕ Host Game
            </Button>
          </div>
        </div>

        {/* Search and Filters */}
        <div className="flex items-center space-x-4">
          <div className="flex-1 relative">
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search games..."
              className="input w-full pl-10"
            />
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500">🔍</span>
          </div>

          <div className="flex rounded-lg overflow-hidden border border-octgn-accent">
            {[
              { id: 'all', label: 'All' },
              { id: 'staging', label: 'Staging' },
              { id: 'playing', label: 'In Progress' },
              { id: 'no-password', label: 'No Password' },
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

      {/* Game List */}
      <div className="flex-1 overflow-y-auto p-6">
        {error && (
          <div className="glass rounded-xl p-4 mb-4 text-red-400">
            <p className="font-medium">Failed to load games</p>
            <p className="text-sm text-gray-400">{error}</p>
            <Button variant="secondary" size="sm" className="mt-2" onClick={refresh}>
              Retry
            </Button>
          </div>
        )}

        {filteredGames.length === 0 ? (
          <EmptyState
            icon="🎮"
            title={searchTerm ? 'No games match your search' : 'No games available'}
            description={
              searchTerm
                ? 'Try a different search term'
                : 'Be the first to host a game!'
            }
            action={
              !searchTerm && (
                <Button variant="primary" onClick={handleHostGame}>
                  Host a Game
                </Button>
              )
            }
          />
        ) : (
          <div className="grid grid-cols-2 lg:grid-cols-3 gap-4">
            {filteredGames.map((game) => (
              <div
                key={game.id}
                className="card-glow cursor-pointer"
                onClick={() => handleJoinGame(game)}
              >
                {/* Game Header */}
                <div className="h-24 bg-gradient-to-br from-octgn-highlight/20 to-octgn-blue/20 flex items-center justify-center relative">
                  {game.gameIconUrl ? (
                    <img
                      src={game.gameIconUrl}
                      alt={game.gameName}
                      className="h-16 w-16 object-contain"
                    />
                  ) : (
                    <span className="text-4xl">🃏</span>
                  )}
                  
                  <div className="absolute top-2 right-2 flex items-center space-x-2">
                    {game.hasPassword && (
                      <span className="text-sm" title="Password required">🔒</span>
                    )}
                    <Badge variant={game.status === 'Staging' ? 'success' : 'warning'}>
                      {game.status}
                    </Badge>
                  </div>
                </div>

                {/* Game Info */}
                <div className="p-4">
                  <h3 className="text-lg font-bold text-white mb-1 truncate">
                    {game.name}
                  </h3>
                  <p className="text-sm text-octgn-highlight mb-3">{game.gameName}</p>
                  
                  <div className="flex items-center justify-between text-xs text-gray-500">
                    <div className="flex items-center space-x-2">
                      <div className="w-6 h-6 rounded-full bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center text-white text-xs font-bold">
                        {game.hostUser.username[0].toUpperCase()}
                      </div>
                      <span>{game.hostUser.username}</span>
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      {game.spectators && (
                        <span title="Spectators allowed">👁️</span>
                      )}
                      <span>v{game.gameVersion}</span>
                    </div>
                  </div>
                </div>

                {/* Action */}
                <div className="p-4 pt-0">
                  <Button
                    variant={game.status === 'Staging' ? 'primary' : 'secondary'}
                    className="w-full"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleJoinGame(game);
                    }}
                  >
                    {game.status === 'Staging' ? 'Join Game' : 'Spectate'}
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Host Game Modal */}
      <HostGameModal
        isOpen={showHostModal}
        onClose={() => setShowHostModal(false)}
        onHost={(options) => {
          // Host the game and navigate
          const gameId = crypto.randomUUID();
          navigate(`/play/${gameId}?host=true&${new URLSearchParams(options as any).toString()}`);
        }}
      />

      {/* Join Game Modal */}
      <JoinGameModal
        isOpen={showJoinModal}
        game={selectedGame}
        onClose={() => {
          setShowJoinModal(false);
          setSelectedGame(null);
        }}
        onJoin={(game, password) => {
          handleConnect(game, password);
        }}
      />
    </div>
  );
}
