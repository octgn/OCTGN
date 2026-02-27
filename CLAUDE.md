# OCTGN Electron App

## Stack
Electron 34, React 19, Zustand 5, Vite 6, TypeScript 5.7, Tailwind 3.4, Vitest 3

## Launch
From repo root (`F:\Source\OCTGN`):
```powershell
.\launch-electron.ps1          # Dev mode (Vite hot-reload + Electron)
.\launch-electron.ps1 -Production  # Built/production mode
```

## Interacting with the Running Electron App (CDP)

The Playwright MCP tool connects to a **browser**, not to an Electron BrowserWindow. To control the actual Electron app directly, use **Chrome DevTools Protocol (CDP)**:

### 1. Launch with CDP enabled
Use `launch-cdp.ps1` from repo root — it builds, starts Vite, and launches Electron with `--remote-debugging-port=9222`:
```powershell
pwsh -File F:\Source\OCTGN\launch-cdp.ps1
```
The script exits after launching — it doesn't stay running. Wait ~5-8 seconds for Electron to be ready, then check CDP with `curl -s http://127.0.0.1:9222/json/version`.

### 2. Use the debug tools
Reusable CDP scripts live in `tools/`. See `tools/README.md` for full docs. Common workflow:
```bash
node tools/cdp-state.mjs               # Check current page, user, games
node tools/cdp-login.mjs               # Trigger login (if on login page)
node tools/cdp-join.mjs                # List lobby games
node tools/cdp-join.mjs chess --spectate  # Join by game type name
node tools/cdp-cards.mjs               # Dump all card data
node tools/cdp-check-images.mjs        # Check which card images loaded/failed
node tools/cdp-screenshot.mjs out.png  # Screenshot the Electron renderer
node tools/cdp-console.mjs "expr"      # Eval arbitrary JS in renderer
node tools/tail-log.mjs 50 IMAGE       # Tail log with filter keyword
pwsh -File tools/capture-wpf.ps1 out.png  # Screenshot WPF OCTGN window
```

### 3. Why not Playwright MCP directly?
- Playwright MCP opens its own Chromium browser — it does NOT connect to the Electron window
- Navigating to `http://localhost:5173` in Playwright MCP renders the same HTML but without `window.octgn` (the Electron preload API), so IPC calls (login, credentials, clipboard, etc.) won't work
- CDP connects to the **actual Electron BrowserWindow** with the real preload script, real IPC, and real main process

### 4. Quick verification
```bash
curl -s http://127.0.0.1:9222/json/version   # Check CDP is running
curl -s http://127.0.0.1:9222/json            # List page targets
```

## Testing

### Unit/integration tests (Vitest)
```bash
cd octgnFX/Octgn.Electron
npm test           # Run all tests (~600 tests)
npm run test:ui    # Vitest UI
```

### E2E tests (Playwright)
```bash
npx playwright test              # Run all e2e tests
npx playwright test --ui         # Playwright UI
```
E2E tests use the Vite dev server with `window.octgn` mocked via `page.addInitScript()`. They do NOT require the Electron app to be running.

## Key Directories
- `src/main/` — Electron main process (IPC handlers, auth, games, protocol)
- `src/main/api/game-service.ts` — Game protocol handler, card creation, state management (large file, ~1200 lines)
- `src/main/games/` — Game definitions: card-resolver, image-resolver, set-parser, definition-parser, game-store, game-installer
- `src/main/asset-protocol.ts` — `octgn-asset://` protocol handler (serves card images + game files from disk)
- `src/renderer/` — React app (pages, components, stores)
- `src/renderer/components/CardComponent.tsx` — Card rendering (image, placeholder, rotation, markers)
- `src/renderer/components/GameBoard.tsx` — Table, board, card layout, zoom/pan
- `src/shared/` — Shared types and constants
- `tests/` — Vitest unit/integration tests
- `e2e/` — Playwright e2e tests
- `tools/` — Reusable debug scripts (CDP + WPF capture). See `tools/README.md`

## Card Image Resolution Pipeline

Understanding how cards get their images is critical for debugging rendering issues:

1. **Game protocol** sends card with `definitionId` (a GUID) but NO set info
2. **CardResolver** (`src/main/games/card-resolver.ts`) scans `Sets/*/set.xml` files from the GameDatabase dir, builds a `cardId → CardDefinition` cache. Each CardDefinition has `{ id, name, setId, properties }`
3. **GameService** creates Card objects: looks up `cardResolver.resolve(definitionId)` to get name + setId, then calls `imageResolver.buildAssetUrl(gameId, setId, cardId)` to build an `octgn-asset://card/{gameId}/{setId}/{cardId}` URL
4. **Renderer** renders `<img src="octgn-asset://...">`
5. **Asset protocol handler** (`src/main/asset-protocol.ts`) intercepts the request, calls `imageResolver.resolveCardImage()` to find the file on disk, serves it via `net.fetch(file://...)`
6. **If definitions load after cards arrive** (async timing): `updateCardNamesAndImages()` retroactively patches all existing cards with proper names and image URLs

**Image search paths** (ImageResolver checks in order):
- `%APPDATA%/OCTGN/ImageDatabase/{gameId}/Sets/{setId}/Cards/{cardId}.{png|jpg|bmp}`
- `%LOCALAPPDATA%/Programs/OCTGN/Data/ImageDatabase/...`
- `%LOCALAPPDATA%/Octgn/ImageDatabase/...`
- Proxy fallback: `.../Cards/Proxies/{cardId}.png`

**Key detail**: ImageDatabase is SEPARATE from GameDatabase. GameDatabase has definition XML files; ImageDatabase has the actual `.png` card images. They're in different directories.

## WPF Client Reference

The WPF OCTGN client (`Octgn.JodsEngine`) is often running alongside during development. Use `tools/capture-wpf.ps1` to screenshot it for visual comparison. The WPF client's process name is `Octgn.JodsEngine` (the game engine window) or `OCTGN` (the main launcher).

**How the WPF client resolves card images** (for reference when matching behavior):
- Uses `Card.ImageUri` property (a filename string, not a full path)
- Searches `{Set.ImagePackUri}/{ImageUri}.*` — each set has its own image directory
- When multiple sets define the same card ID: `game.Sets().SelectMany(x => x.Cards).FirstOrDefault(y => y.Id == id)` — **first set wins**
- Fallback chain: real image → cached proxy → generated proxy (via `proxydef.xml` template)

## Gotchas & Lessons Learned

### Main process changes require rebuild + restart
Renderer changes hot-reload via Vite, but **main process and preload do NOT**. After changing anything in `src/main/`:
1. Run `npm run build:main` (or relaunch via `launch-cdp.ps1` which rebuilds automatically)
2. Restart the Electron process (`Stop-Process -Name electron; pwsh -File launch-cdp.ps1`)

### Auto-login may need a click after restart
After restarting Electron, the app lands on the login page with saved credentials pre-filled but doesn't always auto-submit. Use `node tools/cdp-login.mjs` to trigger it, or click the button manually.

### Electron IPC drops `undefined` arguments
When calling `ipcRenderer.invoke(channel, arg1, undefined, arg3)`, Electron serializes `undefined` in a way that can shift positional arguments. Always use explicit defaults before sending: `password ?? ''`, `spectator ?? false`. This caused a bug where the spectator flag was silently lost.

### OCTGN API game status can be stale
The OCTGN lobby API sometimes reports `status: 1` (GameReady) for games that are actually in progress. The server then kicks you with "This game is already started". Don't assume the API status is always correct — show spectate options for any game with `spectators: true`, not just InProgress games.

### Duplicate card IDs across sets
Some games (like Chess) have multiple sets that define the same card IDs with different images. CardResolver uses first-set-wins (matching WPF's `FirstOrDefault`). If card images look wrong, check whether the wrong set is being selected. The set order depends on filesystem `readdir` order, which is typically alphabetical.

### Preload changes require `npm run build:main`
The preload script is bundled by esbuild, NOT by Vite. Changes to `src/main/preload.ts` won't take effect until you run `npm run build:main` (or relaunch via `launch-cdp.ps1` which rebuilds). Renderer changes hot-reload via Vite, but main process and preload do not.

### Mocking `fs/promises` in Vitest is unreliable
`vi.mock('fs/promises')` often fails to apply to source modules that import from it — the mock applies to the test file's import but not to the module under test. **Use dependency injection instead**: pass an IO interface to the constructor (see `CardResolver` and `CardResolverIO` for the pattern). This is more reliable and avoids async factory / `importOriginal` headaches.

### JSDoc comments with `*/` in glob paths break esbuild
A comment like `/** Scans Sets/*/set.xml */` will be parsed as closing the JSDoc comment at `*/`. Rephrase to avoid literal `*/` in comments: `/** Scans set.xml files from each set directory */`.

### OCTGN data paths — GameDatabase vs ImageDatabase
- **GameDatabase** (definition XML, set.xml): `%LOCALAPPDATA%\Programs\OCTGN\Data\GameDatabase` (newer) or `%LOCALAPPDATA%\Octgn\GameDatabase` (legacy). See `game-store.ts`.
- **ImageDatabase** (card .png/.jpg images): `%LOCALAPPDATA%\Programs\OCTGN\Data\ImageDatabase` or `%APPDATA%\OCTGN\ImageDatabase`. See `image-resolver.ts`.
- These are SEPARATE directories. A game can have definitions in GameDatabase but images in a completely different ImageDatabase path.

### Game protocol group ID encoding
OCTGN group IDs encode the group index in the low byte (1-based). To get the group array index: `(groupId & 0xFF) - 1`. The high bytes encode the player. The `definition.xml` `<player>` element lists groups in order matching these indices.

### Bash shell on Windows eats PowerShell `$` variables
When running `pwsh -Command "..."` from bash, PowerShell variables like `$proc` get interpreted as bash variables and disappear. Either write a `.ps1` file and use `pwsh -File`, or use a heredoc (`pwsh -File - <<'PS1'`).

### Zustand stores are exposed on window for debugging
All stores are available at runtime: `window.__appStore`, `window.__authStore`, `window.__lobbyStore`, `window.__gameStore`. Use `getState()` to read, e.g. `window.__gameStore.getState().gameState.table.cards.length`.
