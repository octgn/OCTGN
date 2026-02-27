import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

interface GameInfo {
  id: string;
  name: string;
  description: string;
  version: string;
  author: string;
  iconUrl?: string;
  installed: boolean;
}

// Demo games for now
const DEMO_GAMES: GameInfo[] = [
  {
    id: 'magic',
    name: 'Magic: The Gathering',
    description: 'The original trading card game',
    version: '1.0.0',
    author: 'Wizards of the Coast',
    installed: true,
  },
  {
    id: 'pokemon',
    name: 'Pokemon TCG',
    description: 'Gotta catch em all!',
    version: '1.0.0',
    author: 'The Pokemon Company',
    installed: false,
  },
  {
    id: 'yugioh',
    name: 'Yu-Gi-Oh!',
    description: 'Duel Monsters card game',
    version: '1.0.0',
    author: 'Konami',
    installed: false,
  },
];

export default function GamesPage() {
  const navigate = useNavigate();
  const [games, setGames] = useState<GameInfo[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filter, setFilter] = useState<'all' | 'installed'>('all');

  useEffect(() => {
    // Load games (would come from file system in real app)
    setGames(DEMO_GAMES);
  }, []);

  const filteredGames = games.filter((game) => {
    const matchesSearch =
      game.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      game.description.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesFilter = filter === 'all' || game.installed;
    return matchesSearch && matchesFilter;
  });

  const handleInstall = (gameId: string) => {
    // TODO: Implement game installation from feed
    console.log('Installing game:', gameId);
  };

  const handlePlay = (gameId: string) => {
    navigate(`/play/${gameId}`);
  };

  return (
    <div className="p-8">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold text-white">Game Library</h1>

          <div className="flex items-center space-x-4">
            {/* Search */}
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search games..."
              className="input"
            />

            {/* Filter */}
            <select
              value={filter}
              onChange={(e) => setFilter(e.target.value as 'all' | 'installed')}
              className="input"
            >
              <option value="all">All Games</option>
              <option value="installed">Installed</option>
            </select>

            {/* Add Feed */}
            <button className="btn btn-secondary">+ Add Feed</button>
          </div>
        </div>

        {/* Games Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredGames.map((game) => (
            <div key={game.id} className="panel hover:border-octgn-highlight border-2 border-transparent transition-colors">
              {/* Game Icon */}
              <div className="flex items-start space-x-4 mb-4">
                <div className="w-16 h-16 bg-octgn-accent rounded-lg flex items-center justify-center text-2xl">
                  {game.iconUrl ? (
                    <img src={game.iconUrl} alt={game.name} className="w-full h-full rounded-lg" />
                  ) : (
                    '🃏'
                  )}
                </div>
                <div className="flex-1">
                  <h3 className="font-bold text-white">{game.name}</h3>
                  <p className="text-sm text-gray-400">{game.author}</p>
                  <p className="text-xs text-gray-500">v{game.version}</p>
                </div>
                {game.installed && (
                  <span className="text-green-500 text-sm">✓ Installed</span>
                )}
              </div>

              {/* Description */}
              <p className="text-gray-400 text-sm mb-4">{game.description}</p>

              {/* Actions */}
              <div className="flex space-x-2">
                {game.installed ? (
                  <>
                    <button
                      onClick={() => handlePlay(game.id)}
                      className="btn btn-primary flex-1"
                    >
                      Play
                    </button>
                    <button className="btn btn-secondary">Uninstall</button>
                  </>
                ) : (
                  <button
                    onClick={() => handleInstall(game.id)}
                    className="btn btn-primary flex-1"
                  >
                    Install
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>

        {/* Empty State */}
        {filteredGames.length === 0 && (
          <div className="text-center py-16">
            <p className="text-gray-400 text-lg mb-4">No games found</p>
            <p className="text-gray-500 text-sm">
              Try adjusting your search or add a game feed
            </p>
          </div>
        )}

        {/* Info Box */}
        <div className="panel mt-8 bg-octgn-accent/30">
          <h3 className="font-bold text-white mb-2">Adding Games</h3>
          <p className="text-gray-300 text-sm">
            OCTGN uses game definition packages (.o8g files) to support various card games.
            Add game feeds to discover and install new games, or manually import game packages.
          </p>
        </div>
      </div>
    </div>
  );
}
