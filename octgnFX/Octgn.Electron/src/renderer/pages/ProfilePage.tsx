import React from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import { useAppStore } from '../stores/app-store';
import { useAuthStore } from '../stores/auth-store';

interface StatCardProps {
  label: string;
  value: string | number;
  icon: React.ReactNode;
}

const StatCard: React.FC<StatCardProps> = ({ label, value, icon }) => (
  <GlassPanel variant="light" padding="md" glow="none" className="flex items-center gap-3">
    <div className="w-10 h-10 rounded-lg bg-octgn-primary/10 border border-octgn-primary/20 flex items-center justify-center text-octgn-primary shrink-0">
      {icon}
    </div>
    <div>
      <p className="text-lg font-semibold text-octgn-text leading-tight">{value}</p>
      <p className="text-[11px] text-octgn-text-dim uppercase tracking-wider">{label}</p>
    </div>
  </GlassPanel>
);

interface RecentGameEntry {
  id: string;
  gameName: string;
  opponentName: string;
  result: 'win' | 'loss' | 'draw';
  date: string;
}

interface FavoriteGameEntry {
  id: string;
  name: string;
  gamesPlayed: number;
  iconUrl?: string;
}

// Placeholder data -- will be replaced by real API calls when the backend is ready
const MOCK_RECENT_GAMES: RecentGameEntry[] = [
  { id: '1', gameName: 'Android: Netrunner', opponentName: 'CyberJack', result: 'win', date: '2026-02-25' },
  { id: '2', gameName: 'Magic: The Gathering', opponentName: 'PlanesWalker99', result: 'loss', date: '2026-02-24' },
  { id: '3', gameName: 'Star Wars: Destiny', opponentName: 'DarkSide42', result: 'win', date: '2026-02-23' },
  { id: '4', gameName: 'Android: Netrunner', opponentName: 'RunnerX', result: 'draw', date: '2026-02-22' },
  { id: '5', gameName: 'Legend of the Five Rings', opponentName: 'SamuraiLord', result: 'win', date: '2026-02-20' },
];

const MOCK_FAVORITE_GAMES: FavoriteGameEntry[] = [
  { id: '1', name: 'Android: Netrunner', gamesPlayed: 142 },
  { id: '2', name: 'Magic: The Gathering', gamesPlayed: 87 },
  { id: '3', name: 'Star Wars: Destiny', gamesPlayed: 34 },
];

const resultStyles: Record<string, string> = {
  win: 'bg-octgn-success/15 text-octgn-success border border-octgn-success/30',
  loss: 'bg-octgn-danger/15 text-octgn-danger border border-octgn-danger/30',
  draw: 'bg-octgn-warning/15 text-octgn-warning border border-octgn-warning/30',
};

const ProfilePage: React.FC = () => {
  const navigate = useAppStore((s) => s.navigate);
  const user = useAuthStore((s) => s.user);

  const memberSince = 'January 2024';
  const gamesPlayed = 263;
  const winRate = 58;

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="flex items-center gap-3 px-5 py-3 border-b border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm">
        <Button variant="ghost" size="sm" onClick={() => navigate('lobby')}>
          <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
            <path d="M10 2L4 8l6 6" />
          </svg>
          Back
        </Button>
        <div className="w-px h-5 bg-octgn-border/40" />
        <h2 className="font-display text-base font-semibold tracking-wide text-octgn-text">
          Profile
        </h2>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-6">
        <div className="max-w-3xl mx-auto space-y-6 animate-fade-in">
          {/* Profile Card */}
          <GlassPanel variant="default" padding="lg" glow="blue" className="border border-octgn-border/30">
            <div className="flex items-center gap-5">
              {/* Avatar */}
              <div className="relative shrink-0">
                <div className="w-20 h-20 rounded-full bg-gradient-to-br from-octgn-primary/50 to-octgn-accent/50 flex items-center justify-center text-3xl font-bold text-octgn-text uppercase shadow-[0_0_30px_rgba(59,130,246,0.15)]">
                  {user?.username?.charAt(0) || '?'}
                </div>
                {user?.isSubscriber && (
                  <div className="absolute -bottom-1 -right-1 w-6 h-6 rounded-full bg-octgn-gold/20 border border-octgn-gold/40 flex items-center justify-center">
                    <svg className="w-3.5 h-3.5 text-octgn-gold" viewBox="0 0 16 16" fill="currentColor">
                      <path d="M8 1l2.35 4.76 5.25.77-3.8 3.7.9 5.24L8 13.27l-4.7 2.47.9-5.24-3.8-3.7 5.25-.77L8 1z" />
                    </svg>
                  </div>
                )}
              </div>

              {/* User info */}
              <div className="flex-1 min-w-0">
                <h3 className="text-xl font-semibold text-octgn-text truncate">
                  {user?.username || 'Guest'}
                </h3>
                {user?.isSubscriber && (
                  <span className="inline-block mt-1 px-2.5 py-0.5 rounded-full text-[10px] font-semibold text-octgn-gold bg-octgn-gold/10 border border-octgn-gold/30 tracking-wide">
                    SUBSCRIBER
                  </span>
                )}
                <p className="text-xs text-octgn-text-dim mt-1.5">
                  Member since {memberSince}
                </p>
              </div>

              {/* Edit profile (placeholder) */}
              <Button variant="ghost" size="sm" onClick={() => navigate('settings')}>
                <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3">
                  <circle cx="8" cy="8" r="2.5" />
                  <path d="M8 1v1.5M8 13.5V15M1 8h1.5M13.5 8H15M2.9 2.9l1.05 1.05M12.05 12.05l1.05 1.05M13.1 2.9l-1.05 1.05M3.95 12.05L2.9 13.1" strokeLinecap="round" />
                </svg>
                Settings
              </Button>
            </div>
          </GlassPanel>

          {/* Stats Grid */}
          <div className="grid grid-cols-3 gap-4">
            <StatCard
              label="Games Played"
              value={gamesPlayed}
              icon={
                <svg className="w-5 h-5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
                  <rect x="1" y="4" width="14" height="9" rx="2" />
                  <path d="M5 7v3M3.5 8.5h3M10.5 7.5h.01M12.5 9.5h.01" />
                </svg>
              }
            />
            <StatCard
              label="Win Rate"
              value={`${winRate}%`}
              icon={
                <svg className="w-5 h-5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M4 14V8M8 14V4M12 14V2" />
                </svg>
              }
            />
            <StatCard
              label="Member Since"
              value={memberSince}
              icon={
                <svg className="w-5 h-5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
                  <rect x="2" y="3" width="12" height="11" rx="1.5" />
                  <path d="M2 6.5h12M5 1.5v3M11 1.5v3" />
                </svg>
              }
            />
          </div>

          {/* Two-column layout for lists */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Recent Games */}
            <div>
              <SectionTitle>Recent Games</SectionTitle>
              <GlassPanel variant="light" padding="none" glow="none" className="border border-octgn-border/20 divide-y divide-octgn-border/20">
                {MOCK_RECENT_GAMES.map((game) => (
                  <div key={game.id} className="flex items-center gap-3 px-4 py-3 hover:bg-white/[0.02] transition-colors">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm text-octgn-text truncate">{game.gameName}</p>
                      <p className="text-xs text-octgn-text-dim mt-0.5">
                        vs {game.opponentName}
                        <span className="mx-1.5 text-octgn-border">|</span>
                        {game.date}
                      </p>
                    </div>
                    <span
                      className={clsx(
                        'px-2 py-0.5 rounded-full text-[10px] font-medium uppercase tracking-wide',
                        resultStyles[game.result],
                      )}
                    >
                      {game.result}
                    </span>
                  </div>
                ))}
              </GlassPanel>
            </div>

            {/* Favorite Games */}
            <div>
              <SectionTitle>Favorite Games</SectionTitle>
              <GlassPanel variant="light" padding="none" glow="none" className="border border-octgn-border/20 divide-y divide-octgn-border/20">
                {MOCK_FAVORITE_GAMES.map((game, index) => (
                  <div key={game.id} className="flex items-center gap-3 px-4 py-3 hover:bg-white/[0.02] transition-colors">
                    {/* Rank badge */}
                    <div
                      className={clsx(
                        'w-7 h-7 rounded-lg flex items-center justify-center text-xs font-bold shrink-0',
                        index === 0
                          ? 'bg-octgn-gold/15 text-octgn-gold border border-octgn-gold/30'
                          : 'bg-octgn-surface text-octgn-text-dim border border-octgn-border/30',
                      )}
                    >
                      {index + 1}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm text-octgn-text truncate">{game.name}</p>
                      <p className="text-xs text-octgn-text-dim mt-0.5">
                        {game.gamesPlayed} games played
                      </p>
                    </div>
                    <div className="text-xs text-octgn-text-muted font-mono">
                      {game.gamesPlayed}
                    </div>
                  </div>
                ))}
              </GlassPanel>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="text-[11px] font-semibold text-octgn-text-dim uppercase tracking-widest mb-3">
      {children}
    </h3>
  );
}

export default ProfilePage;
