import { Routes, Route } from 'react-router-dom';
import { useEffect, useState, Suspense, lazy } from 'react';
import { useGameStore } from './stores/gameStore';
import { Layout, LoadingScreen, ErrorBoundary } from './components';

// Lazy load pages for better performance
const HomePage = lazy(() => import('./pages/HomePage'));
const GameTablePage = lazy(() => import('./pages/GameTablePage'));
const DeckEditorPage = lazy(() => import('./pages/DeckEditorPage'));
const GamesPage = lazy(() => import('./pages/GamesPage'));
const SettingsPage = lazy(() => import('./pages/SettingsPage'));

function App() {
  const { initialize } = useGameStore();
  const [isInitializing, setIsInitializing] = useState(true);
  const [initProgress, setInitProgress] = useState(0);

  useEffect(() => {
    const init = async () => {
      setInitProgress(20);
      
      // Initialize game store
      initialize();
      setInitProgress(40);
      
      // Simulate loading (would be real checks in production)
      await new Promise((r) => setTimeout(r, 200));
      setInitProgress(60);
      
      // Check for Electron API
      if (!window.electronAPI) {
        console.log('Running in browser mode (no Electron API)');
      }
      setInitProgress(80);
      
      // Ready
      await new Promise((r) => setTimeout(r, 100));
      setInitProgress(100);
      
      setIsInitializing(false);
    };

    init();
  }, [initialize]);

  if (isInitializing) {
    return <LoadingScreen message="Starting OCTGN" progress={initProgress} />;
  }

  return (
    <ErrorBoundary>
      <Layout>
        <Suspense fallback={<LoadingScreen message="Loading page..." />}>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/play/:gameId?" element={<GameTablePage />} />
            <Route path="/deckeditor" element={<DeckEditorPage />} />
            <Route path="/games" element={<GamesPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Routes>
        </Suspense>
      </Layout>
    </ErrorBoundary>
  );
}

export default App;
