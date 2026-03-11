import React, { useEffect, useState } from 'react';
import TitleBar from './components/TitleBar';
import ToastContainer from './components/ToastContainer';
import ErrorBoundary from './components/ErrorBoundary';
import GamePage from './pages/GamePage';
import { useGameStore } from './stores/game-store';

/**
 * Game-only app shell rendered in game BrowserWindows (?window=game).
 * No login, no page routing, no auth — just the game.
 */
function useGameWindowTitle() {
  const gameState = useGameStore((s) => s.gameState);
  useEffect(() => {
    const gameName = gameState?.gameName || 'Game';
    let status: string;
    if (!gameState) {
      status = 'Connecting';
    } else if (gameState.isSpectator) {
      status = 'Spectating';
    } else if (!gameState.isStarted) {
      status = 'In Lobby';
    } else {
      status = 'In Game';
    }
    document.title = `${gameName} - ${status} - OCTGN`;
  }, [gameState?.gameName, gameState?.isStarted, gameState?.isSpectator, !!gameState]);
}

const GameApp: React.FC = () => {
  const subscribe = useGameStore((s) => s.subscribe);
  const [ready, setReady] = useState(false);

  useGameWindowTitle();

  useEffect(() => {
    // Subscribe to game state updates from the main process
    const unsubscribe = subscribe();
    setReady(true);
    return unsubscribe;
  }, [subscribe]);

  if (!ready) {
    return (
      <div className="flex flex-col h-screen w-screen bg-octgn-bg overflow-hidden">
        <TitleBar isGameWindow />
        <main className="flex-1 flex items-center justify-center">
          <div className="text-gray-400 text-sm">Connecting to game...</div>
        </main>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen w-screen bg-octgn-bg overflow-hidden">
      <TitleBar isGameWindow />
      <main className="flex-1 overflow-hidden">
        <ErrorBoundary>
          <GamePage isGameWindow />
        </ErrorBoundary>
      </main>
      <ToastContainer />
    </div>
  );
};

export default GameApp;
