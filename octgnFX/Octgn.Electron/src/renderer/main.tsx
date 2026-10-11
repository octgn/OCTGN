import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import GameApp from './GameApp';
import './index.css';

const container = document.getElementById('root');
if (!container) {
  throw new Error('Root element not found');
}

// Game windows are opened with ?window=game query param
const isGameWindow = new URLSearchParams(window.location.search).get('window') === 'game';

const root = createRoot(container);
root.render(
  <React.StrictMode>
    {isGameWindow ? <GameApp /> : <App />}
  </React.StrictMode>
);
