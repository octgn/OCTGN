import React, { useMemo } from 'react';
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

type Page = 'login' | 'lobby' | 'game' | 'deck-builder' | 'settings' | 'profile' | 'games';

const App: React.FC = () => {
  const currentPage = useAppStore((s) => s.currentPage) as Page;
  const navigate = useAppStore((s) => s.navigate);

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
