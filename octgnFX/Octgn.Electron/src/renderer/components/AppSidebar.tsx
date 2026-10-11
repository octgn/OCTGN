import React from 'react';
import { clsx } from 'clsx';
import Button from './Button';
import { useAuthStore } from '../stores/auth-store';
import { useAppStore } from '../stores/app-store';
import type { AppPage } from '../stores/app-store';

interface NavItem {
  id: AppPage;
  label: string;
  icon: React.FC;
}

const NAV_ITEMS: NavItem[] = [
  { id: 'lobby', label: 'Game Lobby', icon: GamepadIcon },
  { id: 'games', label: 'Games', icon: PuzzleIcon },
  { id: 'deck-builder', label: 'Deck Builder', icon: LayersIcon },
  { id: 'profile', label: 'Profile', icon: ProfileIcon },
  { id: 'settings', label: 'Settings', icon: GearIcon },
];

interface AppSidebarProps {
  activePage: AppPage;
}

const AppSidebar: React.FC<AppSidebarProps> = ({ activePage }) => {
  const user = useAuthStore((s) => s.user);
  const navigate = useAppStore((s) => s.navigate);

  return (
    <aside className="w-56 shrink-0 flex flex-col border-r border-octgn-border/40 bg-octgn-surface/50">
      {/* User info */}
      <div className="p-4 border-b border-octgn-border/30">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-octgn-primary/40 to-octgn-accent/40 flex items-center justify-center text-sm font-bold text-octgn-text uppercase">
            {user?.username?.charAt(0) || '?'}
          </div>
          <div className="min-w-0">
            <p className="text-sm font-semibold text-octgn-text truncate">
              {user?.username || 'Guest'}
            </p>
            {user?.isSubscriber && (
              <span className="text-[10px] font-medium text-octgn-gold tracking-wide">
                SUBSCRIBER
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 p-2 space-y-0.5">
        {NAV_ITEMS.map((item) => (
          <button
            key={item.id}
            onClick={() => navigate(item.id)}
            className={clsx(
              'w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-all duration-150',
              item.id === activePage
                ? 'bg-octgn-primary/10 text-octgn-primary'
                : 'text-octgn-text-muted hover:bg-white/5 hover:text-octgn-text'
            )}
          >
            <item.icon />
            {item.label}
          </button>
        ))}
      </nav>

      {/* Sign out */}
      <div className="p-3 border-t border-octgn-border/30">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => useAuthStore.getState().logout()}
          className="w-full justify-start text-octgn-text-dim"
        >
          Sign Out
        </Button>
      </div>
    </aside>
  );
};

function GamepadIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <rect x="1" y="4" width="14" height="9" rx="2" />
      <path d="M5 7v3M3.5 8.5h3M10.5 7.5h.01M12.5 9.5h.01" />
    </svg>
  );
}

function PuzzleIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <path d="M6 2h4v2c.6 0 1 .4 1 1s-.4 1-1 1v2H6V8c-.6 0-1-.4-1-1s.4-1 1-1V2z" />
      <path d="M10 6h2v4h-2c0 .6-.4 1-1 1s-1-.4-1-1H6V6h2" />
      <path d="M6 10H4V6h2" />
      <path d="M4 10v2h8v-2" />
    </svg>
  );
}

function LayersIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <path d="M8 1.5L1.5 5.5 8 9.5l6.5-4L8 1.5z" />
      <path d="M1.5 8l6.5 4 6.5-4" />
      <path d="M1.5 10.5l6.5 4 6.5-4" />
    </svg>
  );
}

function GearIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3">
      <circle cx="8" cy="8" r="2.5" />
      <path d="M8 1v1.5M8 13.5V15M1 8h1.5M13.5 8H15M2.9 2.9l1.05 1.05M12.05 12.05l1.05 1.05M13.1 2.9l-1.05 1.05M3.95 12.05L2.9 13.1" strokeLinecap="round" />
    </svg>
  );
}

function ProfileIcon() {
  return (
    <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="8" cy="5" r="3" />
      <path d="M2 14c0-3.3 2.7-5 6-5s6 1.7 6 5" />
    </svg>
  );
}

export default AppSidebar;
