# OCTGN Electron Debug Tools

Reusable scripts for debugging the Electron app via CDP (Chrome DevTools Protocol) and comparing against the WPF client.

## Prerequisites
- Electron app running with CDP: `pwsh -File launch-cdp.ps1` from repo root
- Playwright installed: `npm install` (already in devDependencies)

## CDP Scripts (Node.js)

| Script | Description |
|--------|-------------|
| `cdp-connect.mjs` | Shared connection helper — imported by other scripts |
| `cdp-state.mjs` | Dump app state: page, user, lobby games, game summary |
| `cdp-cards.mjs` | Dump all cards in the active game (table + player groups) |
| `cdp-screenshot.mjs [file]` | Screenshot the Electron renderer |
| `cdp-join.mjs [id\|name] [--spectate]` | Join a game (no args = list games) |
| `cdp-console.mjs "expr"` | Evaluate arbitrary JS in the renderer |
| `cdp-check-images.mjs` | Check image loading status for all cards |
| `cdp-login.mjs` | Trigger login if on login page (uses saved credentials) |
| `cdp-watch.mjs [secs] [--no-screenshots]` | Watch a live game for state changes |
| `tail-log.mjs [lines] [filter]` | Tail the Electron log file |

## PowerShell Scripts

| Script | Description |
|--------|-------------|
| `capture-wpf.ps1 [file]` | Screenshot the WPF OCTGN game window |

## Examples

```bash
# Check what page the app is on
node tools/cdp-state.mjs

# List lobby games
node tools/cdp-join.mjs

# Join a Chess game as spectator
node tools/cdp-join.mjs chess --spectate

# Dump all card data
node tools/cdp-cards.mjs

# Check which images are loading
node tools/cdp-check-images.mjs

# Screenshot Electron app
node tools/cdp-screenshot.mjs my-screenshot.png

# Screenshot WPF client
pwsh -File tools/capture-wpf.ps1 wpf-screenshot.png

# Check recent RESOLVER log entries
node tools/tail-log.mjs 50 RESOLVER

# Check IMAGE resolution log entries
node tools/tail-log.mjs 50 IMAGE

# Trigger login (when on login page with saved credentials)
node tools/cdp-login.mjs

# Watch a live game for 60 seconds, logging state changes
node tools/cdp-watch.mjs 60

# Watch without saving screenshots
node tools/cdp-watch.mjs 120 --no-screenshots
```
