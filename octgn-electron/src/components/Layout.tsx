import { NavLink, Outlet } from 'react-router-dom';
import { useGameStore } from '../stores/gameStore';

const navItems = [
  { path: '/', label: 'Home', icon: '🏠' },
  { path: '/games', label: 'Games', icon: '📦' },
  { path: '/deckeditor', label: 'Deck Editor', icon: '🃏' },
  { path: '/play/local', label: 'Play', icon: '🎮' },
  { path: '/settings', label: 'Settings', icon: '⚙️' },
];

export default function Layout({ children }: { children?: React.ReactNode }) {
  const { connected, playerName } = useGameStore();

  return (
    <div className="h-screen flex bg-octgn-dark">
      {/* Sidebar - glassmorphism style */}
      <aside className="w-64 glass-dark flex flex-col border-r border-octgn-accent/30">
        {/* Logo area with glow */}
        <div className="p-6 border-b border-octgn-accent/30">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center shadow-glow">
              <span className="text-2xl">🃏</span>
            </div>
            <div>
              <h1 className="text-xl font-bold text-gradient">OCTGN</h1>
              <p className="text-xs text-gray-500">Electron Edition</p>
            </div>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `nav-item ${isActive ? 'nav-item-active' : ''}`
              }
            >
              <span className="text-xl">{item.icon}</span>
              <span className="font-medium">{item.label}</span>
            </NavLink>
          ))}
        </nav>

        {/* Connection Status */}
        <div className="p-4 border-t border-octgn-accent/30">
          <div className="glass rounded-xl p-3">
            <div className="flex items-center space-x-3">
              <div
                className={`status-dot ${
                  connected ? 'status-online' : 'status-offline'
                }`}
              />
              <div className="flex-1">
                <p className="text-sm font-medium text-white">
                  {playerName || 'Offline'}
                </p>
                <p className="text-xs text-gray-500">
                  {connected ? 'Connected' : 'Disconnected'}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Version */}
        <div className="p-4 text-center">
          <p className="text-xs text-gray-600">v3.5.0 • Cross-Platform</p>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Top Bar */}
        <header className="h-14 glass-dark border-b border-octgn-accent/30 flex items-center justify-between px-6">
          <div className="flex items-center space-x-4">
            <h2 className="text-lg font-semibold text-white">OCTGN</h2>
          </div>
          
          <div className="flex items-center space-x-3">
            {connected && (
              <span className="badge-success">
                <span className="status-online mr-2" />
                Online
              </span>
            )}
          </div>
        </header>

        {/* Page Content */}
        <div className="flex-1 overflow-hidden bg-octgn-dark">
          {children || <Outlet />}
        </div>
      </main>
    </div>
  );
}
