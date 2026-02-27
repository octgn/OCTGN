import { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';
import { useGameStore } from '../stores/gameStore';

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const { connected, playerName, playerCount } = useGameStore();

  return (
    <div className="flex h-screen bg-octgn-dark">
      {/* Sidebar */}
      <aside className="w-64 bg-octgn-primary flex flex-col">
        {/* Logo */}
        <div className="p-4 border-b border-octgn-accent">
          <h1 className="text-2xl font-bold text-octgn-highlight">OCTGN</h1>
          <p className="text-xs text-gray-400">Cross-Platform Edition</p>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-4">
          <ul className="space-y-2">
            <li>
              <NavLink
                to="/"
                className={({ isActive }) =>
                  `block px-4 py-2 rounded-lg transition-colors ${
                    isActive
                      ? 'bg-octgn-accent text-white'
                      : 'text-gray-300 hover:bg-octgn-accent/50'
                  }`
                }
              >
                🏠 Home
              </NavLink>
            </li>
            <li>
              <NavLink
                to="/games"
                className={({ isActive }) =>
                  `block px-4 py-2 rounded-lg transition-colors ${
                    isActive
                      ? 'bg-octgn-accent text-white'
                      : 'text-gray-300 hover:bg-octgn-accent/50'
                  }`
                }
              >
                🎮 Games
              </NavLink>
            </li>
            <li>
              <NavLink
                to="/deckeditor"
                className={({ isActive }) =>
                  `block px-4 py-2 rounded-lg transition-colors ${
                    isActive
                      ? 'bg-octgn-accent text-white'
                      : 'text-gray-300 hover:bg-octgn-accent/50'
                  }`
                }
              >
                📋 Deck Editor
              </NavLink>
            </li>
            <li>
              <NavLink
                to="/settings"
                className={({ isActive }) =>
                  `block px-4 py-2 rounded-lg transition-colors ${
                    isActive
                      ? 'bg-octgn-accent text-white'
                      : 'text-gray-300 hover:bg-octgn-accent/50'
                  }`
                }
              >
                ⚙️ Settings
              </NavLink>
            </li>
          </ul>
        </nav>

        {/* Status Bar */}
        <div className="p-4 border-t border-octgn-accent">
          <div className="flex items-center space-x-2">
            <div
              className={`w-2 h-2 rounded-full ${
                connected ? 'bg-green-500' : 'bg-red-500'
              }`}
            />
            <span className="text-xs text-gray-400">
              {connected ? 'Connected' : 'Offline'}
            </span>
          </div>
          {playerName && (
            <p className="text-sm text-gray-300 mt-1">{playerName}</p>
          )}
          {playerCount > 0 && (
            <p className="text-xs text-gray-500">{playerCount} players online</p>
          )}
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-auto">{children}</main>
    </div>
  );
}
