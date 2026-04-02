$ErrorActionPreference = 'Stop'
$dir = Join-Path $PSScriptRoot 'octgnFX' 'Octgn.Electron'
Push-Location $dir

# Build main process + preload
& ./node_modules/.bin/tsc.cmd -p tsconfig.main.json
if ($LASTEXITCODE -ne 0) { throw "tsc failed" }
& ./node_modules/.bin/esbuild.cmd src/main/preload.ts --bundle --platform=node --outfile=dist/main/preload.js --external:electron --format=cjs
if ($LASTEXITCODE -ne 0) { throw "esbuild preload failed" }

# Kill any old vite
Get-Process -Name node -ErrorAction SilentlyContinue | Where-Object {
    $_.CommandLine -match 'vite'
} | Stop-Process -Force -ErrorAction SilentlyContinue

# Start Vite in background
$viteProc = Start-Process -FilePath ./node_modules/.bin/vite.cmd -ArgumentList 'dev' -WindowStyle Hidden -PassThru

# Wait for Vite
for ($i = 0; $i -lt 20; $i++) {
    Start-Sleep -Seconds 1
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $tcp.Connect('127.0.0.1', 5173)
        $tcp.Close()
        Write-Host "Vite ready"
        break
    } catch {}
}

# Launch Electron with CDP
$env:NODE_ENV = 'development'
$electronProc = Start-Process -FilePath ./node_modules/.bin/electron.cmd `
    -ArgumentList 'dist/main/index.js', '--remote-debugging-port=9222' `
    -PassThru

Write-Host "Electron PID: $($electronProc.Id)"
Write-Host "CDP available at http://127.0.0.1:9222"

Pop-Location
