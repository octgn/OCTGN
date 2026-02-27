import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

const navItems = [
  { path: '/', label: 'Home', icon: '🏠', auth: true },
  { path: '/games', label: 'Games', icon: '📦', auth: true },
  { path: '/deckeditor', label: 'Deck Editor', icon: '🃏', auth: false },
  { path: '/play', label: 'Play', icon: '🎮', auth: true },
  { path: '/settings', label: 'Settings', icon: '⚙️', auth: true },
];

export default function Layout({ children }: { children?: React.ReactNode }) {
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuthStore();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const handleLogin = () => {
    navigate('/login');
  };

  return (
    <div className="h-screen flex bg-octgn-dark">
      {/* Sidebar */}
      <aside className="w-64 glass-dark flex flex-col border-r border-octgn-accent/30">
        {/* Logo */}
        <div className="p-6 border-b border-octgn-accent/30">
          <NavLink to="/" className="flex items-center space-x-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center shadow-glow">
              <span className="text-2xl">🃏</span>
            </div>
            <div>
              <h1 className="text-xl font-bold text-gradient">OCTGN</h1>
              <p className="text-xs text-gray-500">Electron Edition</p>
            </div>
          </NavLink>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
          {navItems.map((item) => {
            // Skip auth-required items if not authenticated
            if (item.auth && !isAuthenticated) return null;
            
            return (
              <NavLink
                key={item.path}
                to={item.path}
                end={item.path === '/'}
                className={({ isActive }) =>
                  `nav-item ${isActive ? 'nav-item-active' : ''}`
                }
              >
                <span className="text-xl">{item.icon}</span>
                <span className="font-medium">{item.label}</span>
              </NavLink>
            );
          })}

          {/* Offline play link */}
          {!isAuthenticated && (
            <NavLink
              to="/play/local"
              className={({ isActive }) =>
                `nav-item ${isActive ? 'nav-item-active' : ''}`
              }
            >
              <span className="text-xl">🎮</span>
              <span className="font-medium">Local Game</span>
            </NavLink>
          )}
        </nav>

        {/* User / Auth */}
        <div className="p-4 border-t border-octgn-accent/30">
          {isAuthenticated && user ? (
            <div className="glass rounded-xl p-3">
              <div className="flex items-center space-x-3">
                <div className="w-10 h-10 rounded-full bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center text-white font-bold">
                  {user.username[0].toUpperCase()}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-white truncate">
                    {user.username}
                  </p>
                  <p className="text-xs text-gray-500">Online</p>
                </div>
              </div>
              <button
                onClick={handleLogout}
                className="w-full mt-3 py-1.5 text-xs text-gray-400 hover:text-white transition-colors"
              >
                Sign Out
              </button>
            </div>
          ) : (
            <div className="space-y-2">
              <p className="text-xs text-gray-500 text-center">
                Sign in to play online
              </p>
              <button
                onClick={handleLogin}
                className="w-full btn btn-primary text-sm"
              >
                Sign In
              </button>
            </div>
          )}
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
            {isAuthenticated && (
              <div className="flex items-center space-x-2 px-3 py-1 rounded-full bg-octgn-success/20">
                <div className="w-2 h-2 rounded-full bg-octgn-success animate-pulse" />
                <span className="text-xs text-octgn-success">Connected</span>
              </div>
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
