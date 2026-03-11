import React, { useState, useCallback, useEffect, useRef } from 'react';
import { clsx } from 'clsx';
import GlassPanel from './GlassPanel';
import Button from './Button';
import Input from './Input';
import { useLobbyStore } from '../stores/lobby-store';

interface CreateGameDialogProps {
  onClose: () => void;
  gameDefinitions: GameDefOption[];
}

interface GameDefOption {
  id: string;
  name: string;
  version: string;
  maxPlayers: number;
}

const CreateGameDialog: React.FC<CreateGameDialogProps> = ({
  onClose,
  gameDefinitions,
}) => {
  const [selectedGameId, setSelectedGameId] = useState('');
  const [gameName, setGameName] = useState('');
  const [password, setPassword] = useState('');
  const [playerCount, setPlayerCount] = useState(2);
  const [spectatorsAllowed, setSpectatorsAllowed] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const overlayRef = useRef<HTMLDivElement>(null);
  const hostGame = useLobbyStore((s) => s.hostGame);

  const selectedGame = gameDefinitions.find((g) => g.id === selectedGameId);
  const maxPlayers = selectedGame?.maxPlayers ?? 8;

  // Reset form when dialog mounts
  useEffect(() => {
    setSelectedGameId(gameDefinitions[0]?.id ?? '');
    setGameName('');
    setPassword('');
    setPlayerCount(2);
    setSpectatorsAllowed(false);
    setError(null);
  }, [gameDefinitions]);

  // Close on escape key
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onClose]);

  const handleOverlayClick = useCallback(
    (e: React.MouseEvent) => {
      if (e.target === overlayRef.current) onClose();
    },
    [onClose],
  );

  const handleSubmit = useCallback(
    async (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      if (!selectedGameId) {
        setError('Please select a game.');
        return;
      }
      if (!gameName.trim()) {
        setError('Please enter a game name.');
        return;
      }

      setIsSubmitting(true);
      setError(null);

      try {
        await hostGame({
          gameId: selectedGameId,
          name: gameName.trim(),
          password: password || undefined,
          maxPlayers: playerCount,
          spectators: spectatorsAllowed,
        });
        onClose();
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to host game');
      } finally {
        setIsSubmitting(false);
      }
    },
    [selectedGameId, gameName, password, playerCount, spectatorsAllowed, hostGame, onClose],
  );

  return (
    <div
      ref={overlayRef}
      onClick={handleOverlayClick}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm animate-fade-in"
    >
      <GlassPanel
        variant="heavy"
        padding="none"
        glow="blue"
        className={clsx(
          'w-full max-w-lg mx-4 animate-slide-up',
          'border border-octgn-border/40',
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-octgn-border/30">
          <h2 className="font-display text-lg font-semibold tracking-wide text-octgn-text">
            Host New Game
          </h2>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-lg text-octgn-text-dim hover:text-octgn-text hover:bg-white/5 transition-colors"
          >
            <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
              <path d="M4 4l8 8M12 4l-8 8" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {/* Game Definition Selector */}
          <div className="w-full">
            <label className="block text-[11px] font-medium text-octgn-text-dim uppercase tracking-wider mb-1.5">
              Game
            </label>
            <div className="relative">
              <select
                value={selectedGameId}
                onChange={(e) => {
                  setSelectedGameId(e.target.value);
                  const game = gameDefinitions.find((g) => g.id === e.target.value);
                  if (game && playerCount > game.maxPlayers) {
                    setPlayerCount(game.maxPlayers);
                  }
                }}
                className={clsx(
                  'w-full h-11 px-4 text-sm rounded-lg appearance-none cursor-pointer',
                  'bg-octgn-surface border border-octgn-border/60 text-octgn-text',
                  'outline-none transition-all duration-200',
                  'focus:border-octgn-primary/70 focus:shadow-[0_0_12px_rgba(59,130,246,0.2)]',
                )}
              >
                {gameDefinitions.length === 0 && (
                  <option value="" disabled>
                    No games installed
                  </option>
                )}
                {gameDefinitions.map((gd) => (
                  <option key={gd.id} value={gd.id}>
                    {gd.name} (v{gd.version})
                  </option>
                ))}
              </select>
              <ChevronDownIcon className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-octgn-text-dim pointer-events-none" />
            </div>
          </div>

          {/* Game Name */}
          <Input
            label="Game Name"
            value={gameName}
            onChange={(e) => setGameName(e.target.value)}
            maxLength={60}
            autoFocus
          />

          {/* Password */}
          <Input
            label="Password (optional)"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            maxLength={32}
          />

          {/* Player Count */}
          <div>
            <label className="block text-[11px] font-medium text-octgn-text-dim uppercase tracking-wider mb-1.5">
              Max Players
            </label>
            <div className="flex items-center gap-2">
              {Array.from({ length: Math.min(maxPlayers, 8) }, (_, i) => i + 1)
                .filter((n) => n >= 2)
                .map((n) => (
                  <button
                    key={n}
                    type="button"
                    onClick={() => setPlayerCount(n)}
                    className={clsx(
                      'w-10 h-10 rounded-lg text-sm font-medium transition-all duration-200 border',
                      playerCount === n
                        ? 'bg-octgn-primary/20 text-octgn-primary border-octgn-primary/50 shadow-[0_0_10px_rgba(59,130,246,0.2)]'
                        : 'bg-octgn-surface text-octgn-text-muted border-octgn-border/40 hover:bg-white/5 hover:text-octgn-text',
                    )}
                  >
                    {n}
                  </button>
                ))}
            </div>
          </div>

          {/* Spectators Toggle */}
          <div className="flex items-center justify-between py-1">
            <div>
              <p className="text-sm text-octgn-text">Allow Spectators</p>
              <p className="text-xs text-octgn-text-dim mt-0.5">
                Others can watch the game without playing
              </p>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={spectatorsAllowed}
              onClick={() => setSpectatorsAllowed(!spectatorsAllowed)}
              className={clsx(
                'relative w-10 h-5 rounded-full transition-colors duration-200 shrink-0 ml-4',
                spectatorsAllowed ? 'bg-octgn-primary' : 'bg-octgn-border',
              )}
            >
              <span
                className={clsx(
                  'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform duration-200',
                  spectatorsAllowed ? 'translate-x-5' : 'translate-x-0.5',
                )}
              />
            </button>
          </div>

          {/* Error */}
          {error && (
            <p className="text-xs text-octgn-danger animate-fade-in">{error}</p>
          )}

          {/* Actions */}
          <div className="flex items-center justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="ghost"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              variant="primary"
              loading={isSubmitting}
              icon={
                <svg className="w-4 h-4" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M8 1v14M1 8h14" />
                </svg>
              }
            >
              Host Game
            </Button>
          </div>
        </form>
      </GlassPanel>
    </div>
  );
};

function ChevronDownIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M4 6l4 4 4-4" />
    </svg>
  );
}

export default CreateGameDialog;
