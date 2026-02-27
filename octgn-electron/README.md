# OCTGN Electron Client

Cross-platform OCTGN (Online Card and Tabletop Gaming Network) client built with Electron, React, and Tailwind CSS.

![OCTGN Electron](./screenshot.png)

## Features

### 🎮 Full Cross-Platform Support
- **Windows** - NSIS installer + portable
- **macOS** - DMG + ZIP (Intel & Apple Silicon)
- **Linux** - AppImage, DEB, tar.gz

### 🎨 Modern UI
- Dark theme matching original OCTGN aesthetic
- Glassmorphism effects with purple highlights
- Smooth animations and transitions
- Responsive layout

### 🌐 Real OCTGN.net Integration
- Login to real OCTGN.net servers
- Browse hosted games
- Game library with MyGet feeds
- User statistics

### 🃏 Deck Editor
- Create and edit decks
- Save/load to .o8d format
- Card search and filtering
- Section management (Main, Sideboard, Commander)
- Export to text

### 🎲 Game Table
- Canvas-based rendering
- Pan and zoom controls
- Card drag and drop
- Context menus
- Player hands
- Turn indicators
- Chat system

### 💾 Local Play
- Offline mode with demo cards
- No server required
- Perfect for testing

## Installation

### Download Release
Download the latest release for your platform from [GitHub Releases](https://github.com/octgn/OCTGN/releases).

### Build from Source

```bash
# Clone the repository
git clone https://github.com/octgn/OCTGN.git
cd OCTGN/octgn-electron

# Install dependencies
npm install

# Development
npm run dev

# Build for production
npm run build

# Package for distribution
npm run dist          # Current platform
npm run dist:win      # Windows
npm run dist:mac      # macOS
npm run dist:linux    # Linux
```

## Development

### Prerequisites
- Node.js 18+
- npm 9+

### Project Structure

```
octgn-electron/
├── electron/
│   ├── main.ts              # Electron main process
│   ├── preload.ts           # Preload script (IPC bridge)
│   ├── server/              # Game server components
│   │   ├── GameServer.ts    # Local game server
│   │   ├── GameClient.ts    # Game client
│   │   ├── BinaryProtocol.ts # Protocol implementation
│   │   └── WebSocketBridge.ts # Renderer-main bridge
│   └── assets/              # Application assets
├── src/
│   ├── components/          # React components
│   │   ├── Layout.tsx
│   │   ├── GameCanvas.tsx
│   │   ├── Modal.tsx
│   │   └── ...
│   ├── pages/               # Page components
│   │   ├── HomePage.tsx
│   │   ├── PlayPage.tsx
│   │   ├── GameTablePage.tsx
│   │   └── ...
│   ├── stores/              # Zustand state stores
│   ├── services/            # API services
│   ├── hooks/               # Custom React hooks
│   ├── utils/               # Utility functions
│   └── types/               # TypeScript types
├── dist/                    # Build output
└── release/                 # Packaged applications
```

### Available Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server (renderer + main) |
| `npm run dev:renderer` | Start Vite dev server for renderer |
| `npm run dev:main` | Watch and compile main process |
| `npm run build` | Build for production |
| `npm run start` | Run the built application |
| `npm run dist` | Package distributable |
| `npm run pack` | Package without creating installer |

## Technology Stack

- **Electron** - Cross-platform desktop framework
- **React** - UI components
- **TypeScript** - Type safety
- **Tailwind CSS** - Styling
- **Vite** - Build tooling
- **Zustand** - State management
- **WebSocket** - Real-time communication

## OCTGN Compatibility

This client maintains compatibility with the existing OCTGN ecosystem:

- **Binary Protocol** - Full implementation of 110+ protocol methods
- **Game Packages** - Compatible with existing .nupkg game definitions
- **Deck Format** - Standard .o8d XML deck format
- **Network** - Can connect to existing OCTGN servers

## Configuration

### User Data
Application data is stored in:
- **Windows**: `%APPDATA%\octgn-electron\octgn-data\`
- **macOS**: `~/Library/Application Support/octgn-electron/octgn-data/`
- **Linux**: `~/.config/octgn-electron/octgn-data/`

### Data Structure
```
octgn-data/
├── games/        # Installed game packages
│   ├── magic/
│   ├── pokemon/
│   └── ...
└── decks/        # Saved decks
    ├── deck1.o8d
    └── ...
```

### Ports
**OCTGN Standard Ports** (for interoperability with Windows client):
- Game Server: **8888** (host/join games)
- LAN Discovery: **21234** (broadcast for local games)

**Internal Dev Ports** (won't affect interop):
- Vite Dev Server: 32456 (hot reload during development)
- WebSocket Bridge: 32457 (Electron IPC)

## API Integration

### OCTGN.net API
- Base URL: `https://www.octgn.net`
- Login: `POST /api/sessions`
- Games: `GET /api/game`
- Stats: `GET /api/stats/UsersOnlineNow`

### Game Feeds
- Official: `https://www.myget.org/F/octgngames/`
- Community: `https://www.myget.org/f/octgngamedirectory`

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `Ctrl+C` | Copy |
| `Ctrl+V` | Paste |
| `Ctrl+A` | Select All |
| `Delete` | Delete selected |
| `+` / `=` | Zoom in |
| `-` | Zoom out |
| `0` | Reset zoom |
| `C` | Toggle chat |
| `H` | Toggle hand |
| `Escape` | Clear selection |
| `F11` | Toggle fullscreen |

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

AGPL-3.0 - See [LICENSE](../LICENSE) for details.

## Credits

- Original OCTGN - [octgn.net](https://octgn.net)
- Community contributors
- Game developers

## Support

- **Issues**: [GitHub Issues](https://github.com/octgn/OCTGN/issues)
- **Discord**: [OCTGN Community](https://discord.gg/clawd)
- **Wiki**: [Documentation](https://docs.octgn.net)

---

Built with ❤️ by the OCTGN Community
