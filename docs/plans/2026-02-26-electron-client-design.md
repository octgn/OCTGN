# OCTGN Electron Cross-Platform Client - Design Document

## Problem
OCTGN only runs on Windows (WPF/.NET Framework). Users on Linux and Mac cannot play.

## Solution
A cross-platform Electron client using React + Tailwind that connects to the existing OCTGN server infrastructure. No .NET runtime required on the client side.

## Architecture

### Why Pure TypeScript (No .NET on Client)
- Most .NET projects target .NET Framework 4.7/4.8 (Windows-only)
- Only Octgn.Communication and Octgn.Online are .NET Standard 2.0
- Bundling .NET 8 runtime with Electron adds ~100MB+ and complexity
- The game protocol is well-defined in Protocol.xml - portable to TypeScript
- The REST API is standard HTTP - trivial in TypeScript
- Server code stays 100% untouched = zero regression risk

### Component Architecture

```
┌─────────────────────────────────────────┐
│              Electron App               │
│  ┌───────────────────────────────────┐  │
│  │     React + Tailwind Frontend     │  │
│  │  ┌─────────┐ ┌────────────────┐  │  │
│  │  │  Login   │ │  Game Lobby    │  │  │
│  │  └─────────┘ └────────────────┘  │  │
│  │  ┌─────────┐ ┌────────────────┐  │  │
│  │  │  Deck   │ │  Game Table    │  │  │
│  │  │ Builder │ │  (Canvas/WebGL)│  │  │
│  │  └─────────┘ └────────────────┘  │  │
│  └──────────────┬────────────────────┘  │
│                 │ IPC                    │
│  ┌──────────────┴────────────────────┐  │
│  │       Electron Main Process       │  │
│  │  ┌──────────┐ ┌───────────────┐   │  │
│  │  │ OCTGN    │ │ Game Protocol │   │  │
│  │  │ REST API │ │ Client (TCP)  │   │  │
│  │  └──────────┘ └───────────────┘   │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
          │                    │
          ▼                    ▼
   ┌──────────────┐  ┌──────────────────┐
   │ octgn.net    │  │ Game Server      │
   │ REST API     │  │ (Octgn.Server)   │
   │ (Auth/Lobby) │  │ TCP Binary Proto │
   └──────────────┘  └──────────────────┘
```

### Key Components

1. **OCTGN API Client** (TypeScript)
   - Login/session management via octgn.net REST API
   - Game listing, hosting, joining
   - User profiles

2. **Game Protocol Client** (TypeScript)
   - TCP connection to game servers
   - Binary protocol matching Protocol.xml specification
   - Message serialization/deserialization
   - Handshake, game state sync, card operations

3. **Game Renderer** (React + Canvas/WebGL)
   - Card rendering with images
   - Game table with drag-drop
   - Player areas (hand, graveyard, library, etc.)
   - Zoom/pan/rotate support

4. **Deck Builder** (React)
   - Card search and filtering
   - Deck sections management
   - Import/export deck files (.o8d format)

### Data Flow
- Auth: Electron main → HTTPS → octgn.net API
- Lobby: Electron main → HTTPS → octgn.net API (poll every 15s)
- Game: Electron main → TCP → Game Server (persistent connection)
- UI updates: Main process → IPC → Renderer process → React state

### Game Protocol (from Protocol.xml)
Key message types to implement:
- Connection: Hello, Welcome, NewPlayer, Leave
- Cards: LoadDeck, CreateCard, MoveCard, MoveCardAt, Peek, Untarget
- Game flow: NextTurn, SetPhase, StopTurn
- Communication: Chat, Print, Whisper
- State: Counter, Random, Settings, PlayerSettings
- Groups: CreateGroup, MoveGroup

### File Structure
```
octgnFX/Octgn.Electron/
├── package.json
├── electron-builder.yml
├── tsconfig.json
├── tailwind.config.js
├── vitest.config.ts
├── src/
│   ├── main/           # Electron main process
│   │   ├── index.ts
│   │   ├── api/        # OCTGN REST API client
│   │   ├── protocol/   # Game binary protocol
│   │   └── ipc/        # IPC handlers
│   ├── renderer/       # React frontend
│   │   ├── App.tsx
│   │   ├── components/
│   │   ├── pages/
│   │   ├── hooks/
│   │   └── stores/
│   └── shared/         # Shared types
│       └── types/
├── tests/
│   ├── main/
│   └── renderer/
└── resources/          # Icons, assets
```

## Build & Distribution
- electron-builder for cross-platform packaging
- GitHub Actions for CI/CD
- Separate installer from the Windows WPF version
- Auto-update via electron-updater

## What Gets Reused
- Server infrastructure: 100% (zero changes)
- Game protocol spec: Protocol.xml drives TypeScript implementation
- REST API contracts: Same endpoints
- Game definitions: Same .o8g format parsed in TypeScript
- Card images: Same image paths/URLs

## What Gets Reimplemented
- UI layer: React replaces WPF XAML
- Protocol client: TypeScript replaces Octgn.Communication
- Game renderer: Canvas/WebGL replaces WPF controls
- Local data management: IndexedDB/filesystem replaces FileDB
