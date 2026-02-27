# OCTGN Electron Client

Cross-platform Electron client for OCTGN (Online Card and Tabletop Gaming Network).

## Features

- 🎮 **Game Table**: Interactive canvas-based game table
- 🃏 **Deck Editor**: Build and manage your decks
- 📦 **Game Library**: Browse and install card games
- 💬 **Chat**: In-game chat support
- 🌍 **Cross-Platform**: Windows, macOS, and Linux

## Development

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Package for distribution
npm run dist
```

## Architecture

- **Frontend**: Electron + React + Tailwind CSS
- **Backend**: Node.js game server (binary protocol compatible)
- **Rendering**: Canvas 2D for game table

## Protocol

This client implements the OCTGN binary protocol for compatibility with existing clients.

## License

AGPL-3.0
