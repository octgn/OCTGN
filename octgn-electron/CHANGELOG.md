# Changelog

All notable changes to the OCTGN Electron client will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.5.0] - 2026-02-27

### Added
- Initial cross-platform Electron release
- Full React + TypeScript + Tailwind CSS implementation
- Real OCTGN.net API integration
  - Login with username/password
  - Session management
  - Browse hosted games
  - User statistics
- Game feed service with MyGet integration
  - Official game feed: `octgngames`
  - Community game feed: `octgngamedirectory`
  - Install/uninstall games
- Deck editor
  - Create and edit decks
  - Save/load .o8d format
  - Card database search
  - Section management
  - Export to text
- Game table
  - Canvas-based rendering
  - Pan and zoom controls
  - Card drag and drop
  - Context menus
  - Keyboard shortcuts
  - Player hands
  - Chat system
  - Turn indicators
- Local play mode
  - Offline gameplay
  - Demo cards
  - No server required
- Authentication flow
  - Login page
  - Protected routes
  - Session persistence
- Binary protocol implementation
  - 110+ protocol methods
  - Full OCTGN server compatibility
- WebSocket bridge
  - Renderer-main process communication
  - Real-time game updates
- Game server
  - Host local games
  - LAN discovery
  - Player management
- Comprehensive UI components
  - 20+ reusable components
  - Dark theme with glassmorphism
  - Smooth animations
  - Loading states
  - Error boundaries
- Electron IPC handlers
  - File operations
  - Game installation
  - Deck management
  - Server control
- Distributable builds
  - Windows (NSIS + portable)
  - macOS (DMG + ZIP)
  - Linux (AppImage + DEB)

### Technical Details
- Built with Electron 28
- React 18.2
- TypeScript 5.3
- Tailwind CSS 3.3
- Vite 5.0
- Zustand for state management

### Components
- Layout - Main application shell
- GameCanvas - Canvas-based game table
- Card, CardPile, CardZoom - Card display
- Button, Modal, Badge - UI primitives
- PlayerList, TurnIndicator, CounterPanel - Game UI
- HostGameModal, JoinGameModal - Game management
- LoadingScreen, ErrorBoundary - Utility

### Pages
- HomePage - Dashboard with stats and games
- LoginPage - Authentication
- PlayPage - Browse and join games
- GameTablePage - Online gameplay
- LocalPlayPage - Offline gameplay
- DeckEditorPage - Deck building
- GamesPage - Game library
- SettingsPage - Configuration

### Hooks
- useGameClient - WebSocket game client
- useCardDrag - Card drag and drop
- useCardSelection - Selection management
- useKeyboardShortcuts - Keyboard input

### Services
- OctgnApiService - OCTGN.net API client
- GameFeedService - MyGet feed integration
- CardImageService - Image loading/caching
- GameDiscoveryService - LAN discovery

### Stores
- authStore - Authentication state
- gameStore - Game state

### Utilities
- deckParser - Deck file parsing
- soundManager - Audio playback
- TableRenderer - Canvas rendering
- GameStateSerializer - State persistence
- BinaryProtocol - Wire protocol

## [Unreleased]

### Planned
- Card image loading from game packages
- Python script support
- Plugin system
- Spectator mode
- Replay system
- Game history
- Tournament support
- Voice chat integration
- Custom themes
- Keyboard shortcut customization
- Multi-window support
- Tablet mode
- Accessibility improvements

---

For changes to the original OCTGN, see the main repository changelog.
