import React, { useState, useCallback, useRef, useEffect } from 'react';
import { clsx } from 'clsx';
import Button from './Button';
import GlassPanel from './GlassPanel';
import type { GameState, Player, ChatMessage } from '../../shared/types';
import { useGameStore } from '../stores/game-store';
import { useAppStore } from '../stores/app-store';

interface PreGameLobbyProps {
  gameState: GameState;
}

const PreGameLobby: React.FC<PreGameLobbyProps> = ({ gameState }) => {
  const sendChat = useGameStore((s) => s.sendChat);
  const updateSettings = useGameStore((s) => s.updateSettings);
  const updatePlayerSettings = useGameStore((s) => s.updatePlayerSettings);
  const bootPlayer = useGameStore((s) => s.bootPlayer);
  const startGame = useGameStore((s) => s.startGame);
  const leaveGame = useGameStore((s) => s.leaveGame);
  const navigate = useAppStore((s) => s.navigate);

  const [chatInput, setChatInput] = useState('');
  const [settingsOpen, setSettingsOpen] = useState(true);
  const chatEndRef = useRef<HTMLDivElement>(null);

  const localPlayer = gameState.players.find(p => p.id === gameState.localPlayerId);
  const isHost = localPlayer?.isHost ?? false;
  const isSpectator = gameState.isSpectator;
  const connectionStatus = gameState.connectionStatus ?? 'connected';

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [gameState.chatMessages?.length]);

  const handleSendChat = useCallback(() => {
    if (!chatInput.trim()) return;
    sendChat(chatInput.trim());
    setChatInput('');
  }, [chatInput, sendChat]);

  const handleLeave = useCallback(async () => {
    await leaveGame();
    navigate('lobby');
  }, [leaveGame, navigate]);

  const handleToggleSide = useCallback((player: Player) => {
    const newSide = !player.invertedTable;
    updatePlayerSettings(player.id, newSide, player.isSpectator);
  }, [updatePlayerSettings]);

  const handleToggleSpectator = useCallback((player: Player) => {
    const newSpectator = !player.isSpectator;
    updatePlayerSettings(player.id, player.invertedTable ?? false, newSpectator);
  }, [updatePlayerSettings]);

  const handleBoot = useCallback((playerId: number) => {
    bootPlayer(playerId);
  }, [bootPlayer]);

  const handleToggleTwoSided = useCallback(() => {
    updateSettings(
      !gameState.useTwoSidedTable,
      gameState.allowSpectators ?? false,
      gameState.muteSpectators ?? false,
      gameState.allowCardList ?? false,
    );
  }, [updateSettings, gameState]);

  const handleToggleAllowSpectators = useCallback(() => {
    updateSettings(
      gameState.useTwoSidedTable ?? false,
      !gameState.allowSpectators,
      gameState.muteSpectators ?? false,
      gameState.allowCardList ?? false,
    );
  }, [updateSettings, gameState]);

  const handleToggleMuteSpectators = useCallback(() => {
    updateSettings(
      gameState.useTwoSidedTable ?? false,
      gameState.allowSpectators ?? false,
      !gameState.muteSpectators,
      gameState.allowCardList ?? false,
    );
  }, [updateSettings, gameState]);

  return (
    <div className="flex items-start sm:items-center justify-center h-full overflow-y-auto bg-octgn-bg bg-hero-pattern">
      <div className="w-full max-w-4xl mx-auto p-3 sm:p-6 animate-fade-in">
        {/* Header */}
        <div className="flex flex-wrap items-center gap-2 sm:gap-4 mb-4 sm:mb-6">
          <div className="flex items-center gap-2">
            <span className={clsx(
              'w-2.5 h-2.5 rounded-full shrink-0',
              connectionStatus === 'connected' && 'bg-octgn-success',
              connectionStatus === 'disconnected' && 'bg-octgn-danger',
              connectionStatus === 'reconnecting' && 'bg-octgn-warning animate-pulse',
            )} />
            <h1 className="font-display text-base sm:text-xl font-bold text-octgn-text tracking-wider">
              {gameState.gameName || 'Game Lobby'}
            </h1>
          </div>
          {gameState.gameDefinition && (
            <span className="text-xs text-octgn-text-dim hidden sm:inline">
              {gameState.gameDefinition.name}
            </span>
          )}
          {isSpectator && (
            <span className="px-2 py-0.5 rounded-full bg-octgn-accent/20 border border-octgn-accent/40 text-[10px] font-bold text-octgn-accent tracking-widest uppercase">
              Spectator
            </span>
          )}
          <div className="flex-1" />
          <span className="text-xs text-octgn-text-dim">
            {gameState.players.length} player{gameState.players.length !== 1 ? 's' : ''}
          </span>
        </div>

        {/* Two-column on md+, stacked on mobile */}
        <div className="flex flex-col md:flex-row gap-3 sm:gap-4 min-h-0 md:h-[480px]">
          {/* Left column: Players + Settings */}
          <div className="flex-[3] flex flex-col gap-3 sm:gap-4 min-w-0">
            {/* Player list */}
            <GlassPanel variant="dark" padding="md" className="flex-1 overflow-y-auto max-h-[300px] md:max-h-none">
              <h2 className="text-xs font-semibold text-octgn-text-muted tracking-wide uppercase mb-3">
                Players
              </h2>
              <div className="space-y-2">
                {gameState.players.map((player, index) => (
                  <div
                    key={player.id}
                    className="flex items-center gap-2 sm:gap-3 p-2 sm:p-3 rounded-lg bg-octgn-surface/60 border border-octgn-border/20 animate-slide-up"
                    style={{ animationDelay: `${index * 50}ms` }}
                  >
                    {/* Color dot */}
                    <div
                      className="w-3 h-3 rounded-full shrink-0"
                      style={{ backgroundColor: player.color || '#6b7280' }}
                    />

                    {/* Name + badges */}
                    <div className="flex-1 min-w-0">
                      <div className="flex flex-wrap items-center gap-1 sm:gap-2">
                        <span
                          className="text-xs sm:text-sm font-medium truncate"
                          style={{ color: player.color || '#f9fafb' }}
                        >
                          {player.name}
                        </span>
                        {player.isHost && (
                          <span className="px-1 sm:px-1.5 py-0.5 rounded text-[8px] sm:text-[9px] font-bold bg-octgn-gold/20 text-octgn-gold border border-octgn-gold/30 tracking-wider">
                            HOST
                          </span>
                        )}
                        {player.id === gameState.localPlayerId && (
                          <span className="text-[8px] sm:text-[9px] text-octgn-primary font-bold">(You)</span>
                        )}
                        {player.isSpectator && (
                          <span className="text-[8px] sm:text-[9px] text-octgn-accent">(Spectator)</span>
                        )}
                      </div>
                    </div>

                    {/* Side toggle (when two-sided table is on) */}
                    {gameState.useTwoSidedTable && !player.isSpectator && (
                      <button
                        onClick={() => handleToggleSide(player)}
                        disabled={!isHost && player.id !== gameState.localPlayerId}
                        className={clsx(
                          'px-2 py-1 rounded text-[10px] font-bold tracking-wider transition-all shrink-0',
                          !player.invertedTable
                            ? 'bg-octgn-primary/30 text-octgn-primary border border-octgn-primary/40'
                            : 'bg-octgn-accent/30 text-octgn-accent border border-octgn-accent/40',
                          'disabled:opacity-40 disabled:cursor-not-allowed',
                        )}
                      >
                        Side {player.invertedTable ? 'B' : 'A'}
                      </button>
                    )}

                    {/* Spectator toggle */}
                    {(gameState.allowSpectators && (isHost || player.id === gameState.localPlayerId)) && (
                      <button
                        onClick={() => handleToggleSpectator(player)}
                        className={clsx(
                          'px-2 py-1 rounded text-[10px] font-medium transition-all shrink-0',
                          player.isSpectator
                            ? 'bg-octgn-accent/20 text-octgn-accent border border-octgn-accent/30 hover:bg-octgn-accent/30'
                            : 'bg-octgn-surface text-octgn-text-dim border border-octgn-border/30 hover:bg-octgn-surface-light',
                        )}
                      >
                        {player.isSpectator ? 'Spectator' : 'Player'}
                      </button>
                    )}

                    {/* Kick button (host only, not self) */}
                    {isHost && player.id !== gameState.localPlayerId && (
                      <button
                        onClick={() => handleBoot(player.id)}
                        className="p-1 rounded text-octgn-text-dim hover:text-octgn-danger hover:bg-octgn-danger/10 transition-colors shrink-0"
                        title="Kick player"
                      >
                        <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
                          <path d="M4 4l8 8M12 4l-8 8" />
                        </svg>
                      </button>
                    )}
                  </div>
                ))}
              </div>
            </GlassPanel>

            {/* Settings panel (host only) */}
            {isHost && (
              <GlassPanel variant="dark" padding="md">
                <button
                  onClick={() => setSettingsOpen(v => !v)}
                  className="flex items-center gap-2 w-full text-left"
                >
                  <svg
                    className={clsx('w-3 h-3 text-octgn-text-dim transition-transform', settingsOpen && 'rotate-90')}
                    viewBox="0 0 16 16" fill="currentColor"
                  >
                    <path d="M6 4l4 4-4 4z" />
                  </svg>
                  <span className="text-xs font-semibold text-octgn-text-muted tracking-wide uppercase">
                    Game Settings
                  </span>
                </button>
                {settingsOpen && (
                  <div className="mt-3 space-y-2.5">
                    <ToggleSetting
                      label="Two-Sided Table"
                      checked={gameState.useTwoSidedTable ?? false}
                      onChange={handleToggleTwoSided}
                    />
                    <ToggleSetting
                      label="Allow Spectators"
                      checked={gameState.allowSpectators ?? false}
                      onChange={handleToggleAllowSpectators}
                    />
                    <ToggleSetting
                      label="Mute Spectators"
                      checked={gameState.muteSpectators ?? false}
                      onChange={handleToggleMuteSpectators}
                    />
                  </div>
                )}
              </GlassPanel>
            )}
          </div>

          {/* Right column: Chat */}
          <div className="flex-[2] flex flex-col min-w-0 h-[250px] md:h-auto">
            <GlassPanel variant="dark" padding="none" className="flex-1 flex flex-col min-h-0">
              <div className="px-3 py-2 border-b border-octgn-border/30">
                <span className="text-xs font-semibold text-octgn-text-muted tracking-wide uppercase">
                  Chat
                </span>
              </div>

              <div className="flex-1 overflow-y-auto p-3 space-y-1.5 text-xs">
                {gameState.chatMessages.map((msg: ChatMessage) => (
                  <div
                    key={msg.id}
                    className={clsx(
                      'leading-relaxed break-words',
                      msg.isSystem ? 'text-octgn-text-dim italic' : 'text-octgn-text'
                    )}
                  >
                    {!msg.isSystem && (
                      <span
                        className="font-semibold mr-1"
                        style={{ color: msg.color || '#9ca3af' }}
                      >
                        {msg.playerName}:
                      </span>
                    )}
                    {msg.message}
                  </div>
                ))}
                <div ref={chatEndRef} />
              </div>

              <div className="p-2 border-t border-octgn-border/30">
                <div className="flex gap-1.5">
                  <input
                    type="text"
                    value={chatInput}
                    onChange={(e) => setChatInput(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSendChat()}
                    placeholder="Send a message..."
                    className="flex-1 h-8 sm:h-7 px-2 text-xs rounded bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-colors"
                  />
                  <Button variant="primary" size="sm" onClick={handleSendChat} className="h-8 sm:h-7 px-2">
                    <svg className="w-3 h-3" viewBox="0 0 16 16" fill="currentColor">
                      <path d="M1.5 1.5l13 6.5-13 6.5V9l8-1-8-1V1.5z" />
                    </svg>
                  </Button>
                </div>
              </div>
            </GlassPanel>
          </div>
        </div>

        {/* Bottom bar */}
        <div className="flex flex-col-reverse sm:flex-row items-center justify-between gap-3 mt-4 sm:mt-6">
          <Button variant="ghost" size="sm" onClick={handleLeave} className="w-full sm:w-auto">
            Leave Game
          </Button>

          <div className="flex items-center gap-4 w-full sm:w-auto justify-center sm:justify-end">
            {isHost ? (
              <Button
                variant="primary"
                size="lg"
                onClick={() => startGame()}
                className="animate-glow-pulse font-display tracking-wider w-full sm:w-auto"
              >
                Start Game
              </Button>
            ) : (
              <div className="flex items-center gap-2">
                <div className="w-2 h-2 rounded-full bg-octgn-primary animate-pulse" />
                <span className="text-sm text-octgn-text-muted">
                  Waiting for host to start...
                </span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

// ─── Toggle Setting Component ────────────────────────────────────────────────

interface ToggleSettingProps {
  label: string;
  checked: boolean;
  onChange: () => void;
}

const ToggleSetting: React.FC<ToggleSettingProps> = ({ label, checked, onChange }) => (
  <div className="flex items-center justify-between">
    <span className="text-xs text-octgn-text">{label}</span>
    <button
      onClick={onChange}
      className={clsx(
        'relative w-9 h-5 rounded-full transition-colors',
        checked ? 'bg-octgn-primary' : 'bg-octgn-surface-light border border-octgn-border/50',
      )}
    >
      <span
        className={clsx(
          'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform',
          checked ? 'translate-x-4' : 'translate-x-0.5',
        )}
      />
    </button>
  </div>
);

export default PreGameLobby;
