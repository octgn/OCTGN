import { useState } from 'react';
import Modal from './Modal';
import Button from './Button';

interface JoinGameModalProps {
  isOpen: boolean;
  onClose: () => void;
  onJoin: (options: JoinOptions) => void;
  recentServers?: ServerInfo[];
}

export interface ServerInfo {
  host: string;
  port: number;
  name?: string;
  lastConnected?: Date;
}

export interface JoinOptions {
  host: string;
  port: number;
  password?: string;
  spectator: boolean;
}

export default function JoinGameModal({
  isOpen,
  onClose,
  onJoin,
  recentServers = [],
}: JoinGameModalProps) {
  const [host, setHost] = useState('');
  const [port, setPort] = useState(8888);
  const [password, setPassword] = useState('');
  const [spectator, setSpectator] = useState(false);

  const handleJoin = () => {
    if (!host) {
      alert('Please enter a server address');
      return;
    }

    onJoin({
      host,
      port,
      password: password || undefined,
      spectator,
    });

    onClose();
  };

  const handleSelectRecent = (server: ServerInfo) => {
    setHost(server.host);
    setPort(server.port);
  };

  const handlePasteFromClipboard = async () => {
    try {
      const text = await navigator.clipboard.readText();
      // Try to parse as "host:port"
      const match = text.match(/^([^:]+):(\d+)$/);
      if (match) {
        setHost(match[1]);
        setPort(parseInt(match[2], 10));
      } else {
        setHost(text.trim());
      }
    } catch (e) {
      console.error('Failed to read clipboard:', e);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Join a Game"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleJoin}>
            Connect
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        {/* Server Address */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Server Address
          </label>
          <div className="flex space-x-2">
            <input
              type="text"
              value={host}
              onChange={(e) => setHost(e.target.value)}
              placeholder="hostname or IP"
              className="input flex-1"
            />
            <input
              type="number"
              value={port}
              onChange={(e) => setPort(parseInt(e.target.value, 10) || 8888)}
              className="input w-24"
            />
            <Button
              variant="secondary"
              onClick={handlePasteFromClipboard}
              title="Paste from clipboard"
            >
              📋
            </Button>
          </div>
        </div>

        {/* Password */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Password (if required)
          </label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter password"
            className="input w-full"
          />
        </div>

        {/* Spectator mode */}
        <label className="flex items-center space-x-3">
          <input
            type="checkbox"
            checked={spectator}
            onChange={(e) => setSpectator(e.target.checked)}
            className="w-4 h-4"
          />
          <span className="text-sm text-gray-300">Join as spectator</span>
        </label>

        {/* Recent Servers */}
        {recentServers.length > 0 && (
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Recent Servers
            </label>
            <div className="space-y-1 max-h-32 overflow-y-auto">
              {recentServers.map((server, index) => (
                <button
                  key={index}
                  onClick={() => handleSelectRecent(server)}
                  className="w-full text-left px-3 py-2 rounded bg-octgn-accent/30 hover:bg-octgn-accent/50 text-sm text-gray-300 transition-colors"
                >
                  <span className="font-mono">{server.host}:{server.port}</span>
                  {server.name && (
                    <span className="ml-2 text-gray-500">({server.name})</span>
                  )}
                </button>
              ))}
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
}
