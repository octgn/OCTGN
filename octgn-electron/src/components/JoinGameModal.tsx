import { useState } from 'react';
import Modal from './Modal';
import Button from './Button';
import { HostedGame } from '../services/OctgnApiService';

interface JoinGameModalProps {
  isOpen: boolean;
  game: HostedGame | null;
  onClose: () => void;
  onJoin: (game: HostedGame, password?: string) => void;
}

export default function JoinGameModal({
  isOpen,
  game,
  onClose,
  onJoin,
}: JoinGameModalProps) {
  const [password, setPassword] = useState('');
  const [isJoining, setIsJoining] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleJoin = async () => {
    if (!game) return;

    if (game.hasPassword && !password) {
      setError('Password is required for this game');
      return;
    }

    setIsJoining(true);
    setError(null);

    try {
      onJoin(game, password || undefined);
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to join game');
    } finally {
      setIsJoining(false);
    }
  };

  // Reset when modal opens/closes
  const handleOpenChange = (open: boolean) => {
    if (!open) {
      setPassword('');
      setError(null);
      onClose();
    }
  };

  if (!game) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={() => handleOpenChange(false)}
      title="Join Game"
      size="md"
    >
      <div className="space-y-6">
        {/* Game Info */}
        <div className="glass rounded-xl p-4">
          <div className="flex items-start space-x-4">
            {/* Game Icon */}
            <div className="w-20 h-20 rounded-xl bg-gradient-to-br from-octgn-highlight/30 to-octgn-blue/30 flex items-center justify-center flex-shrink-0">
              {game.gameIconUrl ? (
                <img
                  src={game.gameIconUrl}
                  alt={game.gameName}
                  className="w-14 h-14 object-contain"
                />
              ) : (
                <span className="text-4xl">🃏</span>
              )}
            </div>

            <div className="flex-1 min-w-0">
              <h3 className="text-xl font-bold text-white truncate">
                {game.name}
              </h3>
              <p className="text-octgn-highlight font-medium">{game.gameName}</p>
              
              <div className="flex flex-wrap items-center gap-2 mt-2">
                {game.hasPassword && (
                  <span className="text-xs px-2 py-1 rounded bg-yellow-500/20 text-yellow-400">
                    🔒 Password Required
                  </span>
                )}
                {game.spectators && (
                  <span className="text-xs px-2 py-1 rounded bg-blue-500/20 text-blue-400">
                    👁️ Spectators Allowed
                  </span>
                )}
                <span className="text-xs px-2 py-1 rounded bg-octgn-accent/30 text-gray-400">
                  v{game.gameVersion}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Host Info */}
        <div className="flex items-center justify-between p-3 rounded-lg bg-octgn-dark/50">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center text-white font-bold">
              {game.hostUser.username[0].toUpperCase()}
            </div>
            <div>
              <p className="font-medium text-white">{game.hostUser.username}</p>
              <p className="text-xs text-gray-500">Host</p>
            </div>
          </div>
          
          <div className="text-right">
            <p className={`text-sm font-medium ${
              game.status === 'Staging' ? 'text-octgn-success' : 'text-octgn-warning'
            }`}>
              {game.status === 'Staging' ? 'Waiting for players' : 'Game in progress'}
            </p>
            <p className="text-xs text-gray-500">
              {game.status === 'Staging' ? 'You can join' : 'You can spectate'}
            </p>
          </div>
        </div>

        {/* Password Input */}
        {game.hasPassword && (
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                setError(null);
              }}
              placeholder="Enter the game password"
              className="input w-full"
              autoFocus
            />
            {error && (
              <p className="text-sm text-red-400 mt-1">{error}</p>
            )}
          </div>
        )}

        {/* Connection Info */}
        <div className="text-xs text-gray-500 space-y-1">
          <p>Server: {game.hostAddress}:{game.port || 8888}</p>
          <p>Game ID: {game.gameId}</p>
          <p>Created: {new Date(game.dateCreated).toLocaleString()}</p>
        </div>

        {/* Actions */}
        <div className="flex justify-end space-x-3 pt-4 border-t border-octgn-accent/30">
          <Button variant="ghost" onClick={() => handleOpenChange(false)}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={handleJoin}
            loading={isJoining}
            disabled={game.hasPassword && !password}
          >
            {game.status === 'Staging' ? 'Join Game' : 'Spectate'}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
