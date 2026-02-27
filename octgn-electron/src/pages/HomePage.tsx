import { useNavigate } from 'react-router-dom';
import { Button, Badge } from '../components';
import { useAuthStore } from '../stores/authStore';
import { useHostedGames, useOctgnStats } from '../services/OctgnApiService';

export default function HomePage() {
  const navigate = useNavigate();
  const { user, logout, isAuthenticated } = useAuthStore();
  const { games, loading: gamesLoading } = useHostedGames();
  const stats = useOctgnStats();

  const quickActions = [
    {
      icon: '🎮',
      title: 'Host Game',
      description: 'Start a new game and invite players',
      path: '/play',
      color: 'from-octgn-highlight to-octgn-blue',
      auth: true,
    },
    {
      icon: '🔍',
      title: 'Join Game',
      description: `${games.length} games available`,
      path: '/play',
      color: 'from-octgn-blue to-blue-600',
      auth: true,
      badge: games.length > 0 ? `${games.length}` : undefined,
    },
    {
      icon: '🃏',
      title: 'Deck Editor',
      description: 'Build and manage your decks',
      path: '/deckeditor',
      color: 'from-green-600 to-emerald-600',
      auth: false,
    },
    {
      icon: '📦',
      title: 'Games Library',
      description: 'Browse and install card games',
      path: '/games',
      color: 'from-orange-600 to-amber-600',
      auth: true,
    },
  ];

  const recentActivity = [
    { type: 'stats', message: `${stats.usersOnline} players online`, time: 'Now' },
  ];

  return (
    <div className="h-full overflow-y-auto p-8">
      <div className="max-w-5xl mx-auto space-y-8">
        {/* Hero Section */}
        <div className="text-center py-12">
          <div className="inline-flex items-center justify-center w-24 h-24 rounded-2xl bg-gradient-to-br from-octgn-highlight to-octgn-blue shadow-glow mb-6">
            <span className="text-6xl">🃏</span>
          </div>
          <h1 className="text-5xl font-bold text-gradient mb-4">
            {isAuthenticated && user ? `Welcome back, ${user.username}` : 'Welcome to OCTGN'}
          </h1>
          <p className="text-xl text-gray-400 max-w-2xl mx-auto">
            The ultimate cross-platform card and tabletop gaming experience
          </p>
          
          {/* Stats bar */}
          <div className="flex items-center justify-center space-x-6 mt-6">
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 rounded-full bg-octgn-success animate-pulse" />
              <span className="text-sm text-gray-400">
                {stats.usersOnline} Players Online
              </span>
            </div>
            <div className="text-sm text-gray-500">
              Subscribers: {stats.subPercent}%
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <section>
          <h2 className="text-xl font-bold text-white mb-4 flex items-center">
            <span className="w-1 h-6 bg-octgn-highlight rounded-full mr-3" />
            Quick Actions
          </h2>
          <div className="grid grid-cols-2 gap-4">
            {quickActions.map((action) => {
              const requiresAuth = action.auth && !isAuthenticated;
              
              return (
                <button
                  key={action.path}
                  onClick={() => !requiresAuth && navigate(action.path)}
                  disabled={requiresAuth}
                  className={`group relative overflow-hidden rounded-2xl p-6 text-left transition-all duration-300 hover:-translate-y-1 ${
                    requiresAuth ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
                  }`}
                >
                  {/* Gradient background */}
                  <div className={`absolute inset-0 bg-gradient-to-br ${action.color} opacity-80 group-hover:opacity-100 transition-opacity`} />
                  
                  {/* Shine effect */}
                  <div className="absolute inset-0 shine" />
                  
                  {/* Content */}
                  <div className="relative z-10">
                    <div className="flex items-start justify-between">
                      <span className="text-5xl mb-4 drop-shadow-lg">{action.icon}</span>
                      <div className="flex items-center space-x-2">
                        {action.badge && (
                          <Badge variant="success">{action.badge}</Badge>
                        )}
                        {requiresAuth && (
                          <Badge variant="warning">Login Required</Badge>
                        )}
                      </div>
                    </div>
                    <h3 className="text-2xl font-bold text-white mb-2 drop-shadow-md">
                      {action.title}
                    </h3>
                    <p className="text-white/80 text-sm">{action.description}</p>
                  </div>
                </button>
              );
            })}
          </div>
        </section>

        {/* Games Browser */}
        <section>
          <h2 className="text-xl font-bold text-white mb-4 flex items-center">
            <span className="w-1 h-6 bg-octgn-blue rounded-full mr-3" />
            Live Games
          </h2>
          
          {gamesLoading ? (
            <div className="glass rounded-xl p-6 text-center">
              <p className="text-gray-400">Loading games...</p>
            </div>
          ) : games.length === 0 ? (
            <div className="glass rounded-xl p-6 text-center">
              <p className="text-gray-400 mb-4">No games are currently being hosted</p>
              {isAuthenticated && (
                <Button onClick={() => navigate('/play')}>
                  Host a Game
                </Button>
              )}
            </div>
          ) : (
            <div className="grid grid-cols-3 gap-4">
              {games.slice(0, 6).map((game) => (
                <div key={game.id} className="glass rounded-xl p-4 hover:bg-white/5 transition-colors">
                  <div className="flex items-center justify-between mb-2">
                    <span className="font-medium text-white truncate">{game.name}</span>
                    {game.hasPassword && <span className="text-sm">🔒</span>}
                  </div>
                  <p className="text-sm text-octgn-highlight">{game.gameName}</p>
                  <div className="flex items-center justify-between mt-2 text-xs text-gray-500">
                    <span>{game.hostUser.username}</span>
                    <Badge variant={game.status === 'Staging' ? 'success' : 'warning'}>
                      {game.status}
                    </Badge>
                  </div>
                </div>
              ))}
              
              {games.length > 6 && (
                <button 
                  onClick={() => navigate('/play')}
                  className="glass rounded-xl p-4 flex items-center justify-center text-octgn-highlight hover:bg-white/5 transition-colors"
                >
                  +{games.length - 6} more games
                </button>
              )}
            </div>
          )}
        </section>

        {/* Features Grid */}
        <section>
          <h2 className="text-xl font-bold text-white mb-4 flex items-center">
            <span className="w-1 h-6 bg-octgn-warning rounded-full mr-3" />
            Features
          </h2>
          <div className="grid grid-cols-4 gap-4">
            {[
              { icon: '🌍', title: 'Cross-Platform', desc: 'Windows, macOS, Linux' },
              { icon: '⚡', title: 'Fast', desc: 'Optimized performance' },
              { icon: '🔒', title: 'Secure', desc: 'Encrypted connections' },
              { icon: '🎨', title: 'Modern UI', desc: 'Sleek dark theme' },
              { icon: '📦', title: 'Game Library', desc: '100+ card games' },
              { icon: '🃏', title: 'Deck Builder', desc: 'Powerful editor' },
              { icon: '💬', title: 'In-Game Chat', desc: 'Text & voice' },
              { icon: '💾', title: 'Cloud Sync', desc: 'Save anywhere' },
            ].map((feature) => (
              <div
                key={feature.title}
                className="glass rounded-xl p-4 text-center hover:bg-white/5 transition-colors"
              >
                <span className="text-3xl mb-2 block">{feature.icon}</span>
                <h3 className="font-semibold text-white text-sm mb-1">
                  {feature.title}
                </h3>
                <p className="text-xs text-gray-500">{feature.desc}</p>
              </div>
            ))}
          </div>
        </section>

        {/* Footer */}
        <footer className="text-center py-8 border-t border-octgn-accent/30">
          <div className="flex items-center justify-center space-x-6 text-sm text-gray-500">
            <span>OCTGN Electron v3.5.0</span>
            <span>•</span>
            <a href="https://github.com/octgn/OCTGN" target="_blank" rel="noopener noreferrer" className="hover:text-octgn-highlight transition-colors">
              GitHub
            </a>
            <span>•</span>
            <a href="https://www.octgn.net" target="_blank" rel="noopener noreferrer" className="hover:text-octgn-highlight transition-colors">
              Website
            </a>
            <span>•</span>
            <a href="https://discord.gg/octgn" target="_blank" rel="noopener noreferrer" className="hover:text-octgn-highlight transition-colors">
              Discord
            </a>
          </div>
        </footer>
      </div>
    </div>
  );
}
