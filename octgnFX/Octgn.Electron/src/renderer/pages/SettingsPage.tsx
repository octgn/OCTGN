import React, { useState } from 'react';
import { clsx } from 'clsx';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import { useAppStore } from '../stores/app-store';
import { useAuthStore } from '../stores/auth-store';

type SettingsTab = 'account' | 'game' | 'display' | 'audio';

interface ToggleProps {
  label: string;
  description?: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
}

const Toggle: React.FC<ToggleProps> = ({ label, description, checked, onChange }) => (
  <div className="flex items-center justify-between py-3">
    <div>
      <p className="text-sm text-octgn-text">{label}</p>
      {description && <p className="text-xs text-octgn-text-dim mt-0.5">{description}</p>}
    </div>
    <button
      role="switch"
      aria-checked={checked}
      onClick={() => onChange(!checked)}
      className={clsx(
        'relative w-10 h-5 rounded-full transition-colors duration-200 shrink-0 ml-4',
        checked ? 'bg-octgn-primary' : 'bg-octgn-border'
      )}
    >
      <span
        className={clsx(
          'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform duration-200',
          checked ? 'translate-x-5' : 'translate-x-0.5'
        )}
      />
    </button>
  </div>
);

interface SelectFieldProps {
  label: string;
  value: string;
  options: { value: string; label: string }[];
  onChange: (value: string) => void;
}

const SelectField: React.FC<SelectFieldProps> = ({ label, value, options, onChange }) => (
  <div className="flex items-center justify-between py-3">
    <p className="text-sm text-octgn-text">{label}</p>
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text outline-none focus:border-octgn-primary/50 transition-colors min-w-[140px]"
    >
      {options.map((opt) => (
        <option key={opt.value} value={opt.value}>
          {opt.label}
        </option>
      ))}
    </select>
  </div>
);

interface SliderFieldProps {
  label: string;
  value: number;
  min: number;
  max: number;
  onChange: (value: number) => void;
}

const SliderField: React.FC<SliderFieldProps> = ({ label, value, min, max, onChange }) => (
  <div className="py-3">
    <div className="flex items-center justify-between mb-2">
      <p className="text-sm text-octgn-text">{label}</p>
      <span className="text-xs font-mono text-octgn-text-muted">{value}%</span>
    </div>
    <input
      type="range"
      min={min}
      max={max}
      value={value}
      onChange={(e) => onChange(Number(e.target.value))}
      className="w-full h-1 rounded-full bg-octgn-border appearance-none cursor-pointer [&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-3.5 [&::-webkit-slider-thumb]:h-3.5 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-octgn-primary [&::-webkit-slider-thumb]:shadow-[0_0_6px_rgba(59,130,246,0.5)]"
    />
  </div>
);

const SettingsPage: React.FC = () => {
  const navigate = useAppStore((s) => s.navigate);
  const user = useAuthStore((s) => s.user);

  const [activeTab, setActiveTab] = useState<SettingsTab>('account');

  // Settings state (local for now, will be persisted via store later)
  const [settings, setSettings] = useState({
    // Game
    autoConfirmActions: false,
    enableAnimations: true,
    showCardPreviews: true,
    cardPreviewSize: 'medium',
    // Display
    uiScale: '100',
    theme: 'dark',
    reducedMotion: false,
    showFps: false,
    // Audio
    masterVolume: 80,
    sfxVolume: 70,
    musicVolume: 40,
    muteOnMinimize: true,
    chatSounds: true,
  });

  const update = <K extends keyof typeof settings>(key: K, value: (typeof settings)[K]) => {
    setSettings((prev) => ({ ...prev, [key]: value }));
  };

  const tabs: { id: SettingsTab; label: string }[] = [
    { id: 'account', label: 'Account' },
    { id: 'game', label: 'Game' },
    { id: 'display', label: 'Display' },
    { id: 'audio', label: 'Audio' },
  ];

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
          Settings
        </h2>
      </div>

      <div className="flex-1 flex min-h-0">
        {/* Tabs sidebar */}
        <div className="w-48 shrink-0 border-r border-octgn-border/30 bg-octgn-surface/30 p-2">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={clsx(
                'w-full text-left px-3 py-2 rounded-lg text-sm transition-all duration-150',
                activeTab === tab.id
                  ? 'bg-octgn-primary/10 text-octgn-primary font-medium'
                  : 'text-octgn-text-muted hover:bg-white/5 hover:text-octgn-text'
              )}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          <div className="max-w-xl">
            {activeTab === 'account' && (
              <div className="space-y-6 animate-fade-in">
                <SectionTitle>Account Information</SectionTitle>
                <GlassPanel variant="light" padding="md" glow="none">
                  <div className="flex items-center gap-4">
                    <div className="w-16 h-16 rounded-full bg-gradient-to-br from-octgn-primary/40 to-octgn-accent/40 flex items-center justify-center text-xl font-bold text-octgn-text uppercase">
                      {user?.username?.charAt(0) || '?'}
                    </div>
                    <div>
                      <p className="text-lg font-semibold text-octgn-text">{user?.username || 'Not signed in'}</p>
                      {user?.isSubscriber && (
                        <span className="inline-block mt-0.5 px-2 py-0.5 rounded-full text-[10px] font-semibold text-octgn-gold bg-octgn-gold/10 border border-octgn-gold/30">
                          SUBSCRIBER
                        </span>
                      )}
                    </div>
                  </div>
                </GlassPanel>

                <Divider />
                <SectionTitle>Actions</SectionTitle>
                <div className="space-y-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => window.open('https://www.octgn.net/Account', '_blank')}
                  >
                    Manage Account Online
                  </Button>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => useAuthStore.getState().logout()}
                  >
                    Sign Out
                  </Button>
                </div>
              </div>
            )}

            {activeTab === 'game' && (
              <div className="animate-fade-in">
                <SectionTitle>Gameplay</SectionTitle>
                <div className="divide-y divide-octgn-border/20">
                  <Toggle
                    label="Auto-confirm actions"
                    description="Skip confirmation dialogs for common actions"
                    checked={settings.autoConfirmActions}
                    onChange={(v) => update('autoConfirmActions', v)}
                  />
                  <Toggle
                    label="Enable animations"
                    description="Card movement and flip animations"
                    checked={settings.enableAnimations}
                    onChange={(v) => update('enableAnimations', v)}
                  />
                  <Toggle
                    label="Show card previews on hover"
                    description="Display a larger card preview when hovering"
                    checked={settings.showCardPreviews}
                    onChange={(v) => update('showCardPreviews', v)}
                  />
                  <SelectField
                    label="Card preview size"
                    value={settings.cardPreviewSize}
                    options={[
                      { value: 'small', label: 'Small' },
                      { value: 'medium', label: 'Medium' },
                      { value: 'large', label: 'Large' },
                    ]}
                    onChange={(v) => update('cardPreviewSize', v)}
                  />
                </div>
              </div>
            )}

            {activeTab === 'display' && (
              <div className="animate-fade-in">
                <SectionTitle>Interface</SectionTitle>
                <div className="divide-y divide-octgn-border/20">
                  <SelectField
                    label="UI Scale"
                    value={settings.uiScale}
                    options={[
                      { value: '75', label: '75%' },
                      { value: '90', label: '90%' },
                      { value: '100', label: '100%' },
                      { value: '110', label: '110%' },
                      { value: '125', label: '125%' },
                    ]}
                    onChange={(v) => update('uiScale', v)}
                  />
                  <Toggle
                    label="Reduced motion"
                    description="Disable most UI animations"
                    checked={settings.reducedMotion}
                    onChange={(v) => update('reducedMotion', v)}
                  />
                  <Toggle
                    label="Show FPS counter"
                    description="Display frame rate in the corner"
                    checked={settings.showFps}
                    onChange={(v) => update('showFps', v)}
                  />
                </div>
              </div>
            )}

            {activeTab === 'audio' && (
              <div className="animate-fade-in">
                <SectionTitle>Volume</SectionTitle>
                <div className="space-y-1">
                  <SliderField
                    label="Master Volume"
                    value={settings.masterVolume}
                    min={0}
                    max={100}
                    onChange={(v) => update('masterVolume', v)}
                  />
                  <SliderField
                    label="Sound Effects"
                    value={settings.sfxVolume}
                    min={0}
                    max={100}
                    onChange={(v) => update('sfxVolume', v)}
                  />
                  <SliderField
                    label="Music"
                    value={settings.musicVolume}
                    min={0}
                    max={100}
                    onChange={(v) => update('musicVolume', v)}
                  />
                </div>

                <Divider />
                <SectionTitle>Behavior</SectionTitle>
                <div className="divide-y divide-octgn-border/20">
                  <Toggle
                    label="Mute when minimized"
                    checked={settings.muteOnMinimize}
                    onChange={(v) => update('muteOnMinimize', v)}
                  />
                  <Toggle
                    label="Chat notification sounds"
                    checked={settings.chatSounds}
                    onChange={(v) => update('chatSounds', v)}
                  />
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

/* ── Small helper components ───────────────────────────────────────── */

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="text-[11px] font-semibold text-octgn-text-dim uppercase tracking-widest mb-3">
      {children}
    </h3>
  );
}

function Divider() {
  return <div className="my-6 border-t border-octgn-border/20" />;
}

export default SettingsPage;
