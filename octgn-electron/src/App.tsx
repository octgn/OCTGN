import { Routes, Route, Navigate } from 'react-router-dom';
import { useEffect, useState, Suspense, lazy } from 'react';
import { useGameStore } from './stores/gameStore';
import { useAuthStore } from './stores/authStore';
import { Layout, LoadingScreen, ErrorBoundary } from './components';

// Lazy load pages for better performance
const HomePage = lazy(() => import('./pages/HomePage'));
const LoginPage = lazy(() => import('./pages/LoginPage'));
const GameTablePage = lazy(() => import('./pages/GameTablePage'));
const DeckEditorPage = lazy(() => import('./pages/DeckEditorPage'));
const GamesPage = lazy(() => import('./pages/GamesPage'));
const SettingsPage = lazy(() => import('./pages/SettingsPage'));
const PlayPage = lazy(() => import('./pages/PlayPage'));

// Protected route wrapper
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  return <>{children}</>;
}

function App() {
  const { initialize } = useGameStore();
  const { isAuthenticated, checkSession, session } = useAuthStore();
  const [isInitializing, setIsInitializing] = useState(true);
  const [initProgress, setInitProgress] = useState(0);

  useEffect(() => {
    const init = async () => {
      setInitProgress(10);
      
      // Initialize game store
      initialize();
      setInitProgress(20);
      
      // Check for stored session
      if (session) {
        setInitProgress(30);
        const valid = await checkSession();
        console.log('Session valid:', valid);
      }
      setInitProgress(50);
      
      // Check for Electron API
      if (!window.electronAPI) {
        console.log('Running in browser mode (no Electron API)');
      }
      setInitProgress(70);
      
      // Load game definitions (in production would load from feed)
      await new Promise((r) => setTimeout(r, 100));
      setInitProgress(90);
      
      // Ready
      setInitProgress(100);
      await new Promise((r) => setTimeout(r, 100));
      
      setIsInitializing(false);
    };

    init();
  }, [initialize, checkSession, session]);

  if (isInitializing) {
    return <LoadingScreen message="Starting OCTGN" progress={initProgress} />;
  }

  return (
    <ErrorBoundary>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/play/local" element={
          <Layout>
            <Suspense fallback={<LoadingScreen message="Loading game..." />}>
              <GameTablePage />
            </Suspense>
          </Layout>
        } />
        
        {/* Offline deck editor (no auth required) */}
        <Route path="/deckeditor" element={
          <Layout>
            <Suspense fallback={<LoadingScreen message="Loading deck editor..." />}>
              <DeckEditorPage />
            </Suspense>
          </Layout>
        } />
        
        {/* Protected routes */}
        <Route path="/" element={
          <ProtectedRoute>
            <Layout>
              <Suspense fallback={<LoadingScreen message="Loading..." />}>
                <HomePage />
              </Suspense>
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/play" element={
          <ProtectedRoute>
            <Layout>
              <Suspense fallback={<LoadingScreen message="Loading..." />}>
                <PlayPage />
              </Suspense>
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/play/:gameId" element={
          <ProtectedRoute>
            <Layout>
              <Suspense fallback={<LoadingScreen message="Loading game..." />}>
                <GameTablePage />
              </Suspense>
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/games" element={
          <ProtectedRoute>
            <Layout>
              <Suspense fallback={<LoadingScreen message="Loading games..." />}>
                <GamesPage />
              </Suspense>
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/settings" element={
          <ProtectedRoute>
            <Layout>
              <Suspense fallback={<LoadingScreen message="Loading settings..." />}>
                <SettingsPage />
              </Suspense>
            </Layout>
          </ProtectedRoute>
        } />
        
        {/* Catch-all redirect */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </ErrorBoundary>
  );
}

export default App;
