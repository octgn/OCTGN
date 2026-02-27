import { useState } from 'react';
import { Button, EmptyState, Badge } from '../components';

// Demo games data
const GAMES = [
  {
    id: 'magic',
    name: 'Magic: The Gathering',
    icon: '🧙',
    description: 'The original trading card game',
    version: '3.5.0',
    installed: true,
    cardCount: 25000,
    tags: ['TCG', 'Fantasy', 'Popular'],
  },
  {
    id: 'netrunner',
    name: 'Netrunner',
    icon: '🤖',
    description: 'Cyberpunk card game of hackers and corps',
    version: '2.1.0',
    installed: false,
    cardCount: 1500,
    tags: ['LCG', 'Cyberpunk', 'Strategy'],
  },
  {
    id: 'pokemon',
    name: 'Pokémon TCG',
    icon: '⚡',
    description: 'Collect and battle with Pokémon',
    version: '4.0.0',
    installed: true,
    cardCount: 12000,
    tags: ['TCG', 'Anime', 'Family'],
  },
  {
    id: 'yugioh',
    name: 'Yu-Gi-Oh!',
    icon: '🌟',
    description: 'Duel with monsters, spells, and traps',
    version: '5.2.0',
    installed: false,
    cardCount: 10000,
    tags: ['TCG', 'Anime', 'Fast-paced'],
  },
];

export default function GamesPage() {
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<'all' | 'installed'>('all');
  const [selectedGame, setSelectedGame] = useState<string | null>(null);

  const filteredGames = GAMES.filter((game) => {
    const matchesSearch = game.name.toLowerCase().includes(search.toLowerCase());
    const matchesFilter = filter === 'all' || game.installed;
    return matchesSearch && matchesFilter;
  });

  return (
    <div className="h-full flex">
      {/* Main Content */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-octgn-accent/30">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-3xl font-bold text-white">Game Library</h1>
              <p className="text-gray-400 mt-1">Manage your installed games</p>
            </div>
            
            <Button variant="primary" icon="➕">
              Install Game
            </Button>
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
              <button
                onClick={() => setFilter('all')}
                className={`px-4 py-2 text-sm font-medium transition-colors ${
                  filter === 'all'
                    ? 'bg-octgn-highlight text-white'
                    : 'bg-octgn-primary text-gray-300 hover:bg-octgn-accent'
                }`}
              >
                All ({GAMES.length})
              </button>
              <button
                onClick={() => setFilter('installed')}
                className={`px-4 py-2 text-sm font-medium transition-colors ${
                  filter === 'installed'
                    ? 'bg-octgn-highlight text-white'
                    : 'bg-octgn-primary text-gray-300 hover:bg-octgn-accent'
                }`}
              >
                Installed ({GAMES.filter((g) => g.installed).length})
              </button>
            </div>
          </div>
        </div>

        {/* Games Grid */}
        <div className="flex-1 overflow-y-auto p-6">
          {filteredGames.length === 0 ? (
            <EmptyState
              icon="📦"
              title="No games found"
              description={search ? 'Try a different search term' : 'Install some games to get started'}
            />
          ) : (
            <div className="grid grid-cols-3 gap-6">
              {filteredGames.map((game) => (
                <div
                  key={game.id}
                  onClick={() => setSelectedGame(game.id)}
                  className={`
                    card-glow cursor-pointer overflow-hidden
                    ${selectedGame === game.id ? 'ring-2 ring-octgn-highlight' : ''}
                  `}
                >
                  {/* Game Header */}
                  <div className="relative h-32 bg-gradient-to-br from-octgn-highlight/20 to-octgn-blue/20 flex items-center justify-center">
                    <span className="text-6xl">{game.icon}</span>
                    
                    {game.installed && (
                      <div className="absolute top-3 right-3">
                        <Badge variant="success">Installed</Badge>
                      </div>
                    )}
                  </div>

                  {/* Game Info */}
                  <div className="p-4">
                    <h3 className="text-lg font-bold text-white mb-1">{game.name}</h3>
                    <p className="text-sm text-gray-400 mb-3 line-clamp-2">{game.description}</p>
                    
                    {/* Tags */}
                    <div className="flex flex-wrap gap-1 mb-3">
                      {game.tags.map((tag) => (
                        <span
                          key={tag}
                          className="text-xs px-2 py-0.5 rounded-full bg-octgn-accent/30 text-gray-300"
                        >
                          {tag}
                        </span>
                      ))}
                    </div>

                    {/* Stats */}
                    <div className="flex items-center justify-between text-xs text-gray-500">
                      <span>v{game.version}</span>
                      <span>{game.cardCount.toLocaleString()} cards</span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="p-4 pt-0">
                    {game.installed ? (
                      <div className="flex space-x-2">
                        <Button variant="primary" size="sm" className="flex-1">
                          Play
                        </Button>
                        <Button variant="secondary" size="sm" className="flex-1">
                          Decks
                        </Button>
                      </div>
                    ) : (
                      <Button variant="secondary" size="sm" className="w-full">
                        Install
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
        <div className="w-80 border-l border-octgn-accent/30 bg-octgn-primary/50 p-6">
          {(() => {
            const game = GAMES.find((g) => g.id === selectedGame);
            if (!game) return null;

            return (
              <div className="space-y-6">
                <div className="text-center">
                  <div className="w-24 h-24 rounded-2xl bg-gradient-to-br from-octgn-highlight/30 to-octgn-blue/30 flex items-center justify-center mx-auto mb-4">
                    <span className="text-5xl">{game.icon}</span>
                  </div>
                  <h2 className="text-2xl font-bold text-white">{game.name}</h2>
                  <p className="text-gray-400 text-sm">v{game.version}</p>
                </div>

                <p className="text-gray-300 text-sm">{game.description}</p>

                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">Cards</span>
                    <span className="text-white font-mono">{game.cardCount.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">Status</span>
                    <span className={game.installed ? 'text-octgn-success' : 'text-gray-400'}>
                      {game.installed ? 'Installed' : 'Not Installed'}
                    </span>
                  </div>
                </div>

                <div className="space-y-2">
                  {game.installed ? (
                    <>
                      <Button variant="primary" className="w-full">
                        🎮 Play Game
                      </Button>
                      <Button variant="secondary" className="w-full">
                        🃏 Deck Editor
                      </Button>
                      <Button variant="ghost" className="w-full text-red-400">
                        🗑️ Uninstall
                      </Button>
                    </>
                  ) : (
                    <Button variant="primary" className="w-full">
                      ⬇️ Install
                    </Button>
                  )}
                </div>
              </div>
            );
          })()}
        </div>
      )}
    </div>
  );
}
