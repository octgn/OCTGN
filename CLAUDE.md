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
