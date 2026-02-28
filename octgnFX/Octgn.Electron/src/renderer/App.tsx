import React, { useEffect, useMemo, useState } from 'react';
import TitleBar from './components/TitleBar';
import ToastContainer from './components/ToastContainer';
import ErrorBoundary from './components/ErrorBoundary';
import LoginPage from './pages/LoginPage';
import LobbyPage from './pages/LobbyPage';
import GamePage from './pages/GamePage';
import DeckBuilderPage from './pages/DeckBuilderPage';
import SettingsPage from './pages/SettingsPage';
import ProfilePage from './pages/ProfilePage';
import GamesPage from './pages/GamesPage';
import { useAppStore } from './stores/app-store';
import { useAuthStore } from './stores/auth-store';
import { useGameStore } from './stores/game-store';
import type { LoginResult, GameState } from '../shared/types';

type Page = 'login' | 'lobby' | 'game' | 'deck-builder' | 'settings' | 'profile' | 'games';

const App: React.FC = () => {
  const currentPage = useAppStore((s) => s.currentPage) as Page;
  const navigate = useAppStore((s) => s.navigate);
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const init = async () => {
      try {
        const appState: { session: LoginResult; gameState: GameState | null } =
          await window.octgn.getAppState();

        if (cancelled) return;

        if (appState.session.success && appState.session.user && appState.session.session) {
          // Restore auth state
          useAuthStore.setState({
            user: appState.session.user,
            session: appState.session.session,
          });

          if (appState.gameState) {
            // Restore game state and navigate to game
            useGameStore.setState({
              gameState: appState.gameState,
              isConnected: true,
            });
            navigate('game');
            // Subscribe to ongoing updates
            const unsub = useGameStore.getState().subscribe();
            // Store cleanup in case component unmounts (unlikely for App)
            return unsub;
          } else {
            navigate('lobby');
          }
        }
        // else: no session, stay on login page
      } catch {
        // getAppState not available or failed — stay on login
      } finally {
        if (!cancelled) {
          setIsInitializing(false);
        }
      }
    };

    init();

    return () => {
      cancelled = true;
    };
  }, []);

  const page = useMemo(() => {
    switch (currentPage) {
      case 'login':
        return <LoginPage />;
      case 'lobby':
        return <LobbyPage />;
      case 'game':
        return <GamePage />;
      case 'deck-builder':
        return <DeckBuilderPage />;
      case 'settings':
        return <SettingsPage />;
      case 'profile':
        return <ProfilePage />;
      case 'games':
        return <GamesPage />;
      default:
        return <LoginPage />;
    }
  }, [currentPage]);

  if (isInitializing) {
    return (
      <div className="flex flex-col h-screen w-screen bg-octgn-bg overflow-hidden">
        <TitleBar />
        <main className="flex-1 flex items-center justify-center">
          <div className="text-gray-400 text-sm">Loading...</div>
        </main>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen w-screen bg-octgn-bg overflow-hidden">
      <TitleBar />
      <main className="flex-1 overflow-hidden">
        <ErrorBoundary key={currentPage} onNavigate={navigate}>
          {page}
        </ErrorBoundary>
      </main>
      <ToastContainer />
    </div>
  );
};

export default App;
