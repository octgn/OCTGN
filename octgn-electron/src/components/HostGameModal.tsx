import { useState } from 'react';
import Modal from './Modal';
import Button from './Button';

interface HostGameModalProps {
  isOpen: boolean;
  onClose: () => void;
  onHost: (options: HostOptions) => void;
  installedGames: GameInfo[];
}

interface GameInfo {
  id: string;
  name: string;
  version: string;
}

export interface HostOptions {
  gameId: string;
  port: number;
  name: string;
  password?: string;
  twoSidedTable: boolean;
  allowSpectators: boolean;
  muteSpectators: boolean;
  allowCardList: boolean;
}

export default function HostGameModal({
  isOpen,
  onClose,
  onHost,
  installedGames,
}: HostGameModalProps) {
  const [gameId, setGameId] = useState('');
  const [port, setPort] = useState(8888);
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [twoSidedTable, setTwoSidedTable] = useState(true);
  const [allowSpectators, setAllowSpectators] = useState(true);
  const [muteSpectators, setMuteSpectators] = useState(false);
  const [allowCardList, setAllowCardList] = useState(true);

  const handleHost = () => {
    if (!gameId) {
      alert('Please select a game');
      return;
    }

    onHost({
      gameId,
      port,
      name,
      password: password || undefined,
      twoSidedTable,
      allowSpectators,
      muteSpectators,
      allowCardList,
    });

    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Host a Game"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleHost}>
            Start Hosting
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        {/* Game Selection */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Game
          </label>
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
        </div>

        {/* Game Name */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Game Name (optional)
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="My Awesome Game"
            className="input w-full"
          />
        </div>

        {/* Port */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Port
          </label>
          <input
            type="number"
            value={port}
            onChange={(e) => setPort(parseInt(e.target.value, 10) || 8888)}
            className="input w-full"
          />
        </div>

        {/* Password */}
        <div>
          <label className="block text-sm font-medium text-gray-300 mb-1">
            Password (optional)
          </label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Leave empty for no password"
            className="input w-full"
          />
        </div>

        {/* Settings */}
        <div className="space-y-2">
          <label className="flex items-center space-x-3">
            <input
              type="checkbox"
              checked={twoSidedTable}
              onChange={(e) => setTwoSidedTable(e.target.checked)}
              className="w-4 h-4"
            />
            <span className="text-sm text-gray-300">Two-sided table</span>
          </label>

          <label className="flex items-center space-x-3">
            <input
              type="checkbox"
              checked={allowSpectators}
              onChange={(e) => setAllowSpectators(e.target.checked)}
              className="w-4 h-4"
            />
            <span className="text-sm text-gray-300">Allow spectators</span>
          </label>

          <label className="flex items-center space-x-3">
            <input
              type="checkbox"
              checked={muteSpectators}
              onChange={(e) => setMuteSpectators(e.target.checked)}
              className="w-4 h-4"
              disabled={!allowSpectators}
            />
            <span className="text-sm text-gray-300">Mute spectators</span>
          </label>

          <label className="flex items-center space-x-3">
            <input
              type="checkbox"
              checked={allowCardList}
              onChange={(e) => setAllowCardList(e.target.checked)}
              className="w-4 h-4"
            />
            <span className="text-sm text-gray-300">Allow card list view</span>
          </label>
        </div>
      </div>
    </Modal>
  );
}
