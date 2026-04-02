import { useState, useEffect } from 'react';
import Modal from './Modal';
import Button from './Button';
import { useInstalledGames, GamePackage } from '../services/GameFeedService';

export interface HostOptions {
  gameId: string;
  gameName: string;
  port: number;
  name: string;
  password?: string;
  twoSidedTable: boolean;
  allowSpectators: boolean;
  muteSpectators: boolean;
  allowCardList: boolean;
}

interface HostGameModalProps {
  isOpen: boolean;
  onClose: () => void;
  onHost: (options: HostOptions) => void;
}

export default function HostGameModal({
  isOpen,
  onClose,
  onHost,
}: HostGameModalProps) {
  const { games: installedGames, install } = useInstalledGames();
  
  const [gameId, setGameId] = useState('');
  const [selectedGame, setSelectedGame] = useState<GamePackage | null>(null);
  const [port, setPort] = useState(8888);
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [twoSidedTable, setTwoSidedTable] = useState(true);
  const [allowSpectators, setAllowSpectators] = useState(true);
  const [muteSpectators, setMuteSpectators] = useState(false);
  const [allowCardList, setAllowCardList] = useState(true);
  const [isHosting, setIsHosting] = useState(false);

  // Reset form when modal opens
  useEffect(() => {
    if (isOpen) {
      setGameId('');
      setSelectedGame(null);
      setPort(8888);
      setName('');
      setPassword('');
      setTwoSidedTable(true);
      setAllowSpectators(true);
      setMuteSpectators(false);
      setAllowCardList(true);
    }
  }, [isOpen]);

  // Update selected game when gameId changes
  useEffect(() => {
    if (gameId) {
      const game = installedGames.find((g) => g.id === gameId);
      setSelectedGame(game || null);
    } else {
      setSelectedGame(null);
    }
  }, [gameId, installedGames]);

  const handleHost = async () => {
    if (!selectedGame) {
      alert('Please select a game');
      return;
    }

    setIsHosting(true);

    try {
      onHost({
        gameId: selectedGame.id,
        gameName: selectedGame.name,
        port,
        name: name || `${selectedGame.name} - ${new Date().toLocaleTimeString()}`,
        password: password || undefined,
        twoSidedTable,
        allowSpectators,
        muteSpectators,
        allowCardList,
      });

      onClose();
    } finally {
      setIsHosting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Host a Game"
      size="lg"
    >
      <div className="space-y-6">
        {/* Game Selection */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Select Game
          </label>
          
          {installedGames.length === 0 ? (
            <div className="glass rounded-xl p-4 text-center">
              <p className="text-gray-400 mb-3">No games installed</p>
              <Button variant="secondary" onClick={() => window.location.href = '/games'}>
                Browse Games
              </Button>
            </div>
          ) : (
            <select
              value={gameId}
              onChange={(e) => setGameId(e.target.value)}
              className="input w-full"
            >
              <option value="">Select a game...</option>
              {installedGames.map((game) => (
                <option key={game.id} value={game.id}>
                  {game.name} (v{game.version})
                </option>
              ))}
            </select>
          )}

          {/* Selected Game Preview */}
          {selectedGame && (
            <div className="mt-4 glass rounded-xl p-4 flex items-center space-x-4">
              <div className="w-16 h-16 rounded-lg bg-octgn-accent/30 flex items-center justify-center">
                {selectedGame.iconUrl ? (
                  <img
                    src={selectedGame.iconUrl}
                    alt={selectedGame.name}
                    className="w-12 h-12 object-contain"
                  />
                ) : (
                  <span className="text-3xl">🃏</span>
                )}
              </div>
              <div className="flex-1">
                <h4 className="font-bold text-white">{selectedGame.name}</h4>
                <p className="text-sm text-gray-400">v{selectedGame.version}</p>
                {selectedGame.authors && (
                  <p className="text-xs text-gray-500">
                    by {selectedGame.authors.join(', ')}
                  </p>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Game Name */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Game Name (optional)
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder={selectedGame ? `${selectedGame.name} Game` : 'My Game'}
            className="input w-full"
          />
          <p className="text-xs text-gray-500 mt-1">
            This is the name other players will see in the game browser.
          </p>
        </div>

        {/* Port */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Server Port
          </label>
          <input
            type="number"
            value={port}
            onChange={(e) => setPort(parseInt(e.target.value, 10) || 8888)}
            min={1024}
            max={65535}
            className="input w-full"
          />
          <p className="text-xs text-gray-500 mt-1">
            Default is 8888. Make sure this port is open on your firewall.
          </p>
        </div>

        {/* Password */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-2">
            Password (optional)
          </label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Leave empty for public game"
            className="input w-full"
          />
        </div>

        {/* Settings */}
        <div className="glass rounded-xl p-4 space-y-3">
          <h4 className="font-medium text-white mb-3">Game Settings</h4>
          
          <label className="flex items-center justify-between">
            <span className="text-sm text-gray-300">Two-sided table</span>
            <input
              type="checkbox"
              checked={twoSidedTable}
              onChange={(e) => setTwoSidedTable(e.target.checked)}
              className="w-5 h-5 accent-octgn-highlight"
            />
          </label>

          <label className="flex items-center justify-between">
            <span className="text-sm text-gray-300">Allow spectators</span>
            <input
              type="checkbox"
              checked={allowSpectators}
              onChange={(e) => setAllowSpectators(e.target.checked)}
              className="w-5 h-5 accent-octgn-highlight"
            />
          </label>

          <label className={`flex items-center justify-between ${!allowSpectators ? 'opacity-50' : ''}`}>
            <span className="text-sm text-gray-300">Mute spectators</span>
            <input
              type="checkbox"
              checked={muteSpectators}
              onChange={(e) => setMuteSpectators(e.target.checked)}
              disabled={!allowSpectators}
              className="w-5 h-5 accent-octgn-highlight"
            />
          </label>

          <label className="flex items-center justify-between">
            <span className="text-sm text-gray-300">Allow card list view</span>
            <input
              type="checkbox"
              checked={allowCardList}
              onChange={(e) => setAllowCardList(e.target.checked)}
              className="w-5 h-5 accent-octgn-highlight"
            />
          </label>
        </div>

        {/* Actions */}
        <div className="flex justify-end space-x-3 pt-4 border-t border-octgn-accent/30">
          <Button variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={handleHost}
            loading={isHosting}
            disabled={!selectedGame}
          >
            Start Hosting
          </Button>
        </div>
      </div>
    </Modal>
  );
}
