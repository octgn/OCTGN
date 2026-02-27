import { Routes, Route } from 'react-router-dom';
import { useEffect } from 'react';
import { useGameStore } from './stores/gameStore';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import GameTablePage from './pages/GameTablePage';
import DeckEditorPage from './pages/DeckEditorPage';
import GamesPage from './pages/GamesPage';
import SettingsPage from './pages/SettingsPage';

function App() {
  const { initialize } = useGameStore();

  useEffect(() => {
    initialize();
  }, [initialize]);

  return (
    <Layout>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/play/:gameId?" element={<GameTablePage />} />
        <Route path="/deckeditor" element={<DeckEditorPage />} />
        <Route path="/games" element={<GamesPage />} />
        <Route path="/settings" element={<SettingsPage />} />
      </Routes>
    </Layout>
  );
}

export default App;
