import { useGameStore } from '../stores/gameStore';
import { useNavigate } from 'react-router-dom';

export default function HomePage() {
  const navigate = useNavigate();
  const { connected, playerName, hostGame, connect, isHost } = useGameStore();

  const handleHostGame = async () => {
    try {
      await hostGame(8888);
      navigate('/play');
    } catch (err) {
      console.error('Failed to host game:', err);
    }
  };

  const handleJoinGame = async () => {
    const address = prompt('Enter server address:', 'localhost:8888');
    if (address) {
      const [host, portStr] = address.split(':');
      const port = parseInt(portStr, 10) || 8888;
      await connect(host, port);
      navigate('/play');
    }
  };

  return (
    <div className="p-8">
      <div className="max-w-4xl mx-auto">
        {/* Welcome Header */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-white mb-4">
            Welcome to OCTGN
          </h1>
          <p className="text-gray-400 text-lg">
            Online Card and Tabletop Gaming Network
          </p>
          {playerName && (
            <p className="text-octgn-highlight mt-2">Hello, {playerName}!</p>
          )}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-12">
          <button
            onClick={handleHostGame}
            className="panel hover:bg-octgn-accent/50 transition-colors text-left"
          >
            <div className="flex items-center space-x-4">
              <span className="text-4xl">🎮</span>
              <div>
                <h3 className="text-xl font-bold text-white">Host a Game</h3>
                <p className="text-gray-400">
                  Start a new game session for others to join
                </p>
              </div>
            </div>
          </button>

          <button
            onClick={handleJoinGame}
            className="panel hover:bg-octgn-accent/50 transition-colors text-left"
          >
            <div className="flex items-center space-x-4">
              <span className="text-4xl">🔗</span>
              <div>
                <h3 className="text-xl font-bold text-white">Join a Game</h3>
                <p className="text-gray-400">
                  Connect to an existing game session
                </p>
              </div>
            </div>
          </button>
        </div>

        {/* Recent Games */}
        <div className="panel mb-8">
          <h2 className="text-xl font-bold text-white mb-4">Recent Games</h2>
          <div className="text-gray-400 text-center py-8">
            No recent games. Host or join a game to get started!
          </div>
        </div>

        {/* Features */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="panel">
            <span className="text-3xl mb-2 block">🃏</span>
            <h3 className="font-bold text-white mb-2">Deck Editor</h3>
            <p className="text-gray-400 text-sm">
              Build and manage your card decks with our powerful editor
            </p>
            <button
              onClick={() => navigate('/deckeditor')}
              className="btn btn-secondary mt-4 text-sm"
            >
              Open Editor
            </button>
          </div>

          <div className="panel">
            <span className="text-3xl mb-2 block">📦</span>
            <h3 className="font-bold text-white mb-2">Game Library</h3>
            <p className="text-gray-400 text-sm">
              Browse and install card games from the community
            </p>
            <button
              onClick={() => navigate('/games')}
              className="btn btn-secondary mt-4 text-sm"
            >
              Browse Games
            </button>
          </div>

          <div className="panel">
            <span className="text-3xl mb-2 block">🌍</span>
            <h3 className="font-bold text-white mb-2">Cross-Platform</h3>
            <p className="text-gray-400 text-sm">
              Works on Windows, macOS, and Linux
            </p>
            <p className="text-octgn-highlight mt-4 text-sm">
              {window.electronAPI?.platform || 'Web'} Edition
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
