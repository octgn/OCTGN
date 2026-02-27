import { useState } from 'react';
import { useGameStore } from '../stores/gameStore';

export default function SettingsPage() {
  const { playerName } = useGameStore();
  const [name, setName] = useState(playerName);
  const [theme, setTheme] = useState('dark');
  const [cardBack, setCardBack] = useState('default');
  const [sounds, setSounds] = useState(true);
  const [notifications, setNotifications] = useState(true);

  const handleSave = () => {
    localStorage.setItem('playerName', name);
    // TODO: Save other settings
    alert('Settings saved!');
  };

  return (
    <div className="p-8">
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold text-white mb-8">Settings</h1>

        {/* Profile Section */}
        <section className="panel mb-6">
          <h2 className="text-xl font-bold text-white mb-4">Profile</h2>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Display Name
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="input w-full"
                placeholder="Enter your display name"
              />
            </div>
          </div>
        </section>

        {/* Appearance Section */}
        <section className="panel mb-6">
          <h2 className="text-xl font-bold text-white mb-4">Appearance</h2>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Theme
              </label>
              <select
                value={theme}
                onChange={(e) => setTheme(e.target.value)}
                className="input w-full"
              >
                <option value="dark">Dark (Default)</option>
                <option value="light">Light</option>
                <option value="midnight">Midnight Blue</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Card Back
              </label>
              <select
                value={cardBack}
                onChange={(e) => setCardBack(e.target.value)}
                className="input w-full"
              >
                <option value="default">Default Blue</option>
                <option value="red">Red</option>
                <option value="green">Green</option>
                <option value="black">Black</option>
                <option value="custom">Custom...</option>
              </select>
            </div>
          </div>
        </section>

        {/* Audio Section */}
        <section className="panel mb-6">
          <h2 className="text-xl font-bold text-white mb-4">Audio</h2>

          <div className="space-y-4">
            <label className="flex items-center justify-between">
              <span className="text-gray-300">Sound Effects</span>
              <input
                type="checkbox"
                checked={sounds}
                onChange={(e) => setSounds(e.target.checked)}
                className="w-5 h-5"
              />
            </label>

            <label className="flex items-center justify-between">
              <span className="text-gray-300">Notifications</span>
              <input
                type="checkbox"
                checked={notifications}
                onChange={(e) => setNotifications(e.target.checked)}
                className="w-5 h-5"
              />
            </label>
          </div>
        </section>

        {/* Network Section */}
        <section className="panel mb-6">
          <h2 className="text-xl font-bold text-white mb-4">Network</h2>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Default Port
              </label>
              <input
                type="number"
                defaultValue="8888"
                className="input w-full"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">
                Broadcast Port
              </label>
              <input
                type="number"
                defaultValue="21234"
                className="input w-full"
              />
            </div>
          </div>
        </section>

        {/* About Section */}
        <section className="panel mb-6">
          <h2 className="text-xl font-bold text-white mb-4">About</h2>
          <div className="text-gray-400 space-y-2">
            <p>OCTGN Electron Client</p>
            <p>Version 3.5.0</p>
            <p>Platform: {window.electronAPI?.platform || 'Web'}</p>
            <p className="mt-4 text-sm">
              Built with Electron, React, and Tailwind CSS
            </p>
          </div>
        </section>

        {/* Save Button */}
        <button onClick={handleSave} className="btn btn-primary w-full">
          Save Settings
        </button>
      </div>
    </div>
  );
}
