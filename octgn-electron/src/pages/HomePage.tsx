import { useNavigate } from 'react-router-dom';
import { Button, Badge } from '../components';

export default function HomePage() {
  const navigate = useNavigate();

  const quickActions = [
    {
      icon: '🎮',
      title: 'Host Game',
      description: 'Start a new game and invite players',
      path: '/play',
      color: 'from-octgn-highlight to-octgn-blue',
    },
    {
      icon: '🔍',
      title: 'Join Game',
      description: 'Find and join an existing game',
      path: '/play',
      color: 'from-octgn-blue to-blue-600',
    },
    {
      icon: '🃏',
      title: 'Deck Editor',
      description: 'Build and manage your decks',
      path: '/deckeditor',
      color: 'from-green-600 to-emerald-600',
    },
    {
      icon: '📦',
      title: 'Games Library',
      description: 'Browse and install card games',
      path: '/games',
      color: 'from-orange-600 to-amber-600',
    },
  ];

  const recentActivity = [
    { type: 'game', message: 'Played Magic: The Gathering', time: '2 hours ago' },
    { type: 'deck', message: 'Edited "Red Deck Wins"', time: 'Yesterday' },
    { type: 'install', message: 'Installed Netrunner', time: '3 days ago' },
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
            Welcome to OCTGN
          </h1>
          <p className="text-xl text-gray-400 max-w-2xl mx-auto">
            The ultimate cross-platform card and tabletop gaming experience
          </p>
        </div>

        {/* Quick Actions */}
        <section>
          <h2 className="text-xl font-bold text-white mb-4 flex items-center">
            <span className="w-1 h-6 bg-octgn-highlight rounded-full mr-3" />
            Quick Actions
          </h2>
          <div className="grid grid-cols-2 gap-4">
            {quickActions.map((action) => (
              <button
                key={action.path}
                onClick={() => navigate(action.path)}
                className="group relative overflow-hidden rounded-2xl p-6 text-left transition-all duration-300 hover:-translate-y-1"
              >
                {/* Gradient background */}
                <div className={`absolute inset-0 bg-gradient-to-br ${action.color} opacity-80 group-hover:opacity-100 transition-opacity`} />
                
                {/* Shine effect */}
                <div className="absolute inset-0 shine" />
                
                {/* Content */}
                <div className="relative z-10">
                  <div className="flex items-start justify-between">
                    <span className="text-5xl mb-4 drop-shadow-lg">{action.icon}</span>
                    <span className="text-white/50 group-hover:text-white/80 transition-colors">
                      →
                    </span>
                  </div>
                  <h3 className="text-2xl font-bold text-white mb-2 drop-shadow-md">
                    {action.title}
                  </h3>
                  <p className="text-white/80 text-sm">{action.description}</p>
                </div>
              </button>
            ))}
          </div>
        </section>

        {/* Stats & Activity */}
        <div className="grid grid-cols-3 gap-6">
          {/* Stats */}
          <section className="col-span-2">
            <h2 className="text-xl font-bold text-white mb-4 flex items-center">
              <span className="w-1 h-6 bg-octgn-blue rounded-full mr-3" />
              Statistics
            </h2>
            <div className="grid grid-cols-3 gap-4">
              {[
                { label: 'Games Played', value: '0', icon: '🎮' },
                { label: 'Decks Built', value: '0', icon: '🃏' },
                { label: 'Hours Played', value: '0h', icon: '⏱️' },
              ].map((stat) => (
                <div key={stat.label} className="glass rounded-xl p-4 text-center">
                  <span className="text-3xl mb-2 block">{stat.icon}</span>
                  <p className="text-2xl font-bold text-white">{stat.value}</p>
                  <p className="text-sm text-gray-400">{stat.label}</p>
                </div>
              ))}
            </div>
          </section>

          {/* Recent Activity */}
          <section>
            <h2 className="text-xl font-bold text-white mb-4 flex items-center">
              <span className="w-1 h-6 bg-octgn-success rounded-full mr-3" />
              Recent
            </h2>
            <div className="glass rounded-xl p-4 space-y-3">
              {recentActivity.map((activity, i) => (
                <div
                  key={i}
                  className="flex items-start space-x-3 pb-3 border-b border-octgn-accent/30 last:border-0 last:pb-0"
                >
                  <span className="text-xl">
                    {activity.type === 'game' && '🎮'}
                    {activity.type === 'deck' && '🃏'}
                    {activity.type === 'install' && '📦'}
                  </span>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm text-white truncate">{activity.message}</p>
                    <p className="text-xs text-gray-500">{activity.time}</p>
                  </div>
                </div>
              ))}
            </div>
          </section>
        </div>

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
            <a href="#" className="hover:text-octgn-highlight transition-colors">
              GitHub
            </a>
            <span>•</span>
            <a href="#" className="hover:text-octgn-highlight transition-colors">
              Documentation
            </a>
            <span>•</span>
            <a href="#" className="hover:text-octgn-highlight transition-colors">
              Discord
            </a>
          </div>
        </footer>
      </div>
    </div>
  );
}
