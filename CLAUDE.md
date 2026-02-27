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

### 2. Connect via Playwright CDP
Write a Node script (`.mjs`) in the Electron project dir and use `chromium.connectOverCDP`:
```js
import { chromium } from 'playwright';
const browser = await chromium.connectOverCDP('http://127.0.0.1:9222');
const pages = browser.contexts()[0].pages();
const octgn = pages.find(p => p.url().includes('5173'));
// Now use octgn like a normal Playwright page:
await octgn.screenshot({ path: 'screenshot.png' });
await octgn.getByTestId('login-submit').click();
```
Run with: `node cdp-demo.mjs`

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
npm test           # Run all tests
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
- `src/renderer/` — React app (pages, components, stores)
- `src/shared/` — Shared types and constants
- `tests/` — Vitest unit/integration tests
- `e2e/` — Playwright e2e tests

## Gotchas & Lessons Learned

### Electron IPC drops `undefined` arguments
When calling `ipcRenderer.invoke(channel, arg1, undefined, arg3)`, Electron serializes `undefined` in a way that can shift positional arguments. Always use explicit defaults before sending: `password ?? ''`, `spectator ?? false`. This caused a bug where the spectator flag was silently lost.

### OCTGN API game status can be stale
The OCTGN lobby API sometimes reports `status: 1` (GameReady) for games that are actually in progress. The server then kicks you with "This game is already started". Don't assume the API status is always correct — show spectate options for any game with `spectators: true`, not just InProgress games.

### Preload changes require `npm run build:main`
The preload script is bundled by esbuild, NOT by Vite. Changes to `src/main/preload.ts` won't take effect until you run `npm run build:main` (or relaunch via `launch-cdp.ps1` which rebuilds). Renderer changes hot-reload via Vite, but main process and preload do not.

### Mocking `fs/promises` in Vitest is unreliable
`vi.mock('fs/promises')` often fails to apply to source modules that import from it — the mock applies to the test file's import but not to the module under test. **Use dependency injection instead**: pass an IO interface to the constructor (see `CardResolver` and `CardResolverIO` for the pattern). This is more reliable and avoids async factory / `importOriginal` headaches.

### JSDoc comments with `*/` in glob paths break esbuild
A comment like `/** Scans Sets/*/set.xml */` will be parsed as closing the JSDoc comment at `*/`. Rephrase to avoid literal `*/` in comments: `/** Scans set.xml files from each set directory */`.

### OCTGN GameDatabase paths
The legacy OCTGN install uses `%LOCALAPPDATA%\Octgn\GameDatabase`. Newer installs use `%LOCALAPPDATA%\Programs\OCTGN\Data\GameDatabase`. Always check both paths (see `game-store.ts`).

### Game protocol group ID encoding
OCTGN group IDs encode the group index in the low byte (1-based). To get the group array index: `(groupId & 0xFF) - 1`. The high bytes encode the player. The `definition.xml` `<player>` element lists groups in order matching these indices.
