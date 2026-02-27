import React, { useMemo } from 'react';
import TitleBar from './components/TitleBar';
import LoginPage from './pages/LoginPage';
import LobbyPage from './pages/LobbyPage';
import GamePage from './pages/GamePage';
import DeckBuilderPage from './pages/DeckBuilderPage';
import SettingsPage from './pages/SettingsPage';
import ProfilePage from './pages/ProfilePage';
import { useAppStore } from './stores/app-store';

type Page = 'login' | 'lobby' | 'game' | 'deck-builder' | 'settings' | 'profile';

const App: React.FC = () => {
  const currentPage = useAppStore((s) => s.currentPage) as Page;

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
      default:
        return <LoginPage />;
    }
  }, [currentPage]);

  return (
    <div className="flex flex-col h-screen w-screen bg-octgn-bg overflow-hidden">
      <TitleBar />
      <main className="flex-1 overflow-hidden">{page}</main>
    </div>
  );
};

export default App;
