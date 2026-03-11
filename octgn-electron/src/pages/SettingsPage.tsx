import { useState } from 'react';
import { Button, Badge } from '../components';

const SETTINGS_SECTIONS = [
  { id: 'general', label: 'General', icon: '⚙️' },
  { id: 'appearance', label: 'Appearance', icon: '🎨' },
  { id: 'audio', label: 'Audio', icon: '🔊' },
  { id: 'network', label: 'Network', icon: '🌐' },
  { id: 'gameplay', label: 'Gameplay', icon: '🎮' },
  { id: 'shortcuts', label: 'Shortcuts', icon: '⌨️' },
  { id: 'about', label: 'About', icon: 'ℹ️' },
];

export default function SettingsPage() {
  const [activeSection, setActiveSection] = useState('general');

  return (
    <div className="h-full flex">
      {/* Sidebar */}
      <div className="w-64 border-r border-octgn-accent/30 bg-octgn-primary/30">
        <div className="p-6">
          <h1 className="text-2xl font-bold text-white mb-2">Settings</h1>
          <p className="text-sm text-gray-500">Configure your experience</p>
        </div>

        <nav className="p-3 space-y-1">
          {SETTINGS_SECTIONS.map((section) => (
            <button
              key={section.id}
              onClick={() => setActiveSection(section.id)}
              className={`
                w-full flex items-center space-x-3 px-4 py-3 rounded-lg transition-all
                ${activeSection === section.id
                  ? 'bg-octgn-highlight/20 text-octgn-highlight border-l-2 border-octgn-highlight'
                  : 'text-gray-400 hover:text-white hover:bg-white/5'
                }
              `}
            >
              <span className="text-xl">{section.icon}</span>
              <span className="font-medium">{section.label}</span>
            </button>
          ))}
        </nav>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-8">
        <div className="max-w-2xl">
          {activeSection === 'general' && <GeneralSettings />}
          {activeSection === 'appearance' && <AppearanceSettings />}
          {activeSection === 'audio' && <AudioSettings />}
          {activeSection === 'network' && <NetworkSettings />}
          {activeSection === 'gameplay' && <GameplaySettings />}
          {activeSection === 'shortcuts' && <ShortcutSettings />}
          {activeSection === 'about' && <AboutSection />}
        </div>
      </div>
    </div>
  );
}

function GeneralSettings() {
  return (
    <div className="space-y-8">
      <Section title="Profile">
        <div className="flex items-start space-x-4">
          <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center text-4xl shadow-glow">
            👤
          </div>
          <div className="flex-1 space-y-4">
            <div>
              <label className="text-sm text-gray-400 mb-1 block">Display Name</label>
              <input type="text" className="input w-full" defaultValue="Player" />
            </div>
          </div>
        </div>
      </Section>

      <Section title="Language">
        <Select
          options={[
            { value: 'en', label: 'English' },
            { value: 'es', label: 'Español' },
            { value: 'fr', label: 'Français' },
            { value: 'de', label: 'Deutsch' },
            { value: 'ja', label: '日本語' },
          ]}
          defaultValue="en"
        />
      </Section>

      <Section title="Startup">
        <Toggle label="Launch at system startup" defaultChecked />
        <Toggle label="Show in system tray" defaultChecked />
        <Toggle label="Check for updates automatically" defaultChecked />
      </Section>

      <Section title="Data">
        <div className="flex space-x-3">
          <Button variant="secondary">Export Settings</Button>
          <Button variant="secondary">Import Settings</Button>
          <Button variant="danger">Reset to Defaults</Button>
        </div>
      </Section>
    </div>
  );
}

function AppearanceSettings() {
  return (
    <div className="space-y-8">
      <Section title="Theme">
        <div className="grid grid-cols-3 gap-4">
          {[
            { id: 'dark', name: 'Dark', preview: '#171717' },
            { id: 'midnight', name: 'Midnight', preview: '#0a0a0a' },
            { id: 'purple', name: 'Purple Haze', preview: '#1a1a2e' },
          ].map((theme) => (
            <button
              key={theme.id}
              className="card p-4 text-left hover:ring-2 hover:ring-octgn-highlight transition-all"
            >
              <div
                className="h-20 rounded-lg mb-3"
                style={{ backgroundColor: theme.preview }}
              />
              <p className="font-medium text-white">{theme.name}</p>
            </button>
          ))}
        </div>
      </Section>

      <Section title="Accent Color">
        <div className="flex space-x-3">
          {['#9370DB', '#6886D4', '#50C878', '#FFD700', '#DC143C', '#4FC3F7'].map((color) => (
            <button
              key={color}
              className="w-10 h-10 rounded-full transition-transform hover:scale-110"
              style={{ backgroundColor: color }}
            />
          ))}
        </div>
      </Section>

      <Section title="Display">
        <Toggle label="Enable animations" defaultChecked />
        <Toggle label="Show card previews on hover" defaultChecked />
        <Toggle label="Glassmorphism effects" defaultChecked />
      </Section>

      <Section title="Table">
        <div className="space-y-4">
          <div>
            <label className="text-sm text-gray-400 mb-2 block">Grid Style</label>
            <Select
              options={[
                { value: 'subtle', label: 'Subtle' },
                { value: 'visible', label: 'Visible' },
                { value: 'none', label: 'None' },
              ]}
              defaultValue="subtle"
            />
          </div>
        </div>
      </Section>
    </div>
  );
}

function AudioSettings() {
  return (
    <div className="space-y-8">
      <Section title="Master Volume">
        <div className="flex items-center space-x-4">
          <span className="text-xl">🔈</span>
          <input
            type="range"
            min="0"
            max="100"
            defaultValue="80"
            className="flex-1 accent-octgn-highlight"
          />
          <span className="text-xl">🔊</span>
          <span className="text-white font-mono w-12 text-right">80%</span>
        </div>
      </Section>

      <Section title="Sound Effects">
        <Toggle label="Card sounds" defaultChecked />
        <Toggle label="UI sounds" defaultChecked />
        <Toggle label="Chat notification sounds" defaultChecked />
        <Toggle label="Turn change sounds" defaultChecked />
      </Section>

      <Section title="Voice Chat">
        <Toggle label="Enable voice chat" />
        <div className="flex items-center space-x-3 mt-4">
          <Select
            options={[
              { value: 'default', label: 'Default Microphone' },
            ]}
            defaultValue="default"
            className="flex-1"
          />
          <Button variant="secondary">Test</Button>
        </div>
      </Section>
    </div>
  );
}

function NetworkSettings() {
  return (
    <div className="space-y-8">
      <Section title="Connection">
        <div>
          <label className="text-sm text-gray-400 mb-1 block">Default Server</label>
          <input type="text" className="input w-full" placeholder="localhost:8888" />
        </div>
        <div className="flex items-center justify-between p-4 rounded-lg bg-octgn-dark/50">
          <div>
            <p className="text-white font-medium">Connection Status</p>
            <p className="text-sm text-gray-500">Not connected</p>
          </div>
          <Badge variant="warning">Offline</Badge>
        </div>
      </Section>

      <Section title="LAN Discovery">
        <Toggle label="Enable LAN game discovery" defaultChecked />
        <div>
          <label className="text-sm text-gray-400 mb-1 block">Broadcast Port</label>
          <input type="number" className="input w-full" defaultValue="21234" />
        </div>
      </Section>

      <Section title="Proxy">
        <Toggle label="Use proxy server" />
        <input type="text" className="input w-full mt-4" placeholder="http://proxy:8080" />
      </Section>
    </div>
  );
}

function GameplaySettings() {
  return (
    <div className="space-y-8">
      <Section title="Cards">
        <Toggle label="Always show card names" defaultChecked />
        <Toggle label="Show card markers" defaultChecked />
        <Toggle label="Enable card targeting" defaultChecked />
        <Toggle label="Show opponent's card faces" />
      </Section>

      <Section title="Animations">
        <Toggle label="Card flip animation" defaultChecked />
        <Toggle label="Card move animation" defaultChecked />
        <Toggle label="Shuffle animation" defaultChecked />
      </Section>

      <Section title="Auto-actions">
        <Toggle label="Auto-pass priority" />
        <Toggle label="Auto-save game state" defaultChecked />
      </Section>
    </div>
  );
}

function ShortcutSettings() {
  const shortcuts = [
    { key: 'F', action: 'Flip selected cards' },
    { key: 'R', action: 'Rotate selected cards' },
    { key: 'Delete', action: 'Delete selected cards' },
    { key: 'H', action: 'Toggle hand visibility' },
    { key: 'C', action: 'Toggle chat visibility' },
    { key: 'Ctrl+S', action: 'Save game' },
    { key: 'Ctrl+O', action: 'Load game' },
    { key: 'Ctrl++', action: 'Zoom in' },
    { key: 'Ctrl+-', action: 'Zoom out' },
    { key: 'Escape', action: 'Clear selection' },
  ];

  return (
    <div className="space-y-8">
      <Section title="Keyboard Shortcuts">
        <div className="space-y-2">
          {shortcuts.map((shortcut) => (
            <div
              key={shortcut.key}
              className="flex items-center justify-between p-3 rounded-lg bg-octgn-dark/50 hover:bg-octgn-accent/20 transition-colors"
            >
              <span className="text-gray-300">{shortcut.action}</span>
              <kbd className="px-3 py-1.5 rounded-lg bg-octgn-accent/30 text-white font-mono text-sm">
                {shortcut.key}
              </kbd>
            </div>
          ))}
        </div>
      </Section>
    </div>
  );
}

function AboutSection() {
  return (
    <div className="space-y-8">
      <div className="text-center py-8">
        <div className="w-24 h-24 rounded-2xl bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center mx-auto mb-6 shadow-glow">
          <span className="text-5xl">🃏</span>
        </div>
        <h2 className="text-3xl font-bold text-gradient mb-2">OCTGN</h2>
        <p className="text-gray-400 mb-1">Electron Edition</p>
        <p className="text-gray-500 text-sm">Version 3.5.0 (Cross-Platform)</p>
      </div>

      <Section title="System Information">
        <div className="space-y-2 text-sm">
          <InfoRow label="Platform" value={navigator.platform} />
          <InfoRow label="Electron" value="28.0.0" />
          <InfoRow label="Chrome" value="120.0.0" />
          <InfoRow label="Node.js" value="20.10.0" />
        </div>
      </Section>

      <Section title="Links">
        <div className="space-y-2">
          <a href="#" className="flex items-center justify-between p-3 rounded-lg bg-octgn-dark/50 hover:bg-octgn-accent/20 transition-colors">
            <span className="text-gray-300">GitHub Repository</span>
            <span className="text-octgn-highlight">↗</span>
          </a>
          <a href="#" className="flex items-center justify-between p-3 rounded-lg bg-octgn-dark/50 hover:bg-octgn-accent/20 transition-colors">
            <span className="text-gray-300">Documentation</span>
            <span className="text-octgn-highlight">↗</span>
          </a>
          <a href="#" className="flex items-center justify-between p-3 rounded-lg bg-octgn-dark/50 hover:bg-octgn-accent/20 transition-colors">
            <span className="text-gray-300">Discord Community</span>
            <span className="text-octgn-highlight">↗</span>
          </a>
        </div>
      </Section>

      <div className="text-center text-gray-500 text-sm">
        <p>Built with ❤️ by the OCTGN community</p>
        <p className="mt-1">Released under the AGPL-3.0 License</p>
      </div>
    </div>
  );
}

// Helper components
function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div>
      <h3 className="text-lg font-semibold text-white mb-4">{title}</h3>
      <div className="space-y-4">{children}</div>
    </div>
  );
}

function Toggle({ label, defaultChecked }: { label: string; defaultChecked?: boolean }) {
  const [checked, setChecked] = useState(defaultChecked);

  return (
    <div className="flex items-center justify-between">
      <span className="text-gray-300">{label}</span>
      <button
        onClick={() => setChecked(!checked)}
        className={`
          w-12 h-7 rounded-full transition-colors relative
          ${checked ? 'bg-octgn-highlight' : 'bg-octgn-accent'}
        `}
      >
        <div
          className={`
            absolute top-1 w-5 h-5 rounded-full bg-white shadow-lg transition-transform
            ${checked ? 'translate-x-6' : 'translate-x-1'}
          `}
        />
      </button>
    </div>
  );
}

function Select({
  options,
  defaultValue,
  className = '',
}: {
  options: Array<{ value: string; label: string }>;
  defaultValue?: string;
  className?: string;
}) {
  return (
    <select className={`input ${className}`} defaultValue={defaultValue}>
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between p-3 rounded-lg bg-octgn-dark/50">
      <span className="text-gray-400">{label}</span>
      <span className="text-white font-mono">{value}</span>
    </div>
  );
}
