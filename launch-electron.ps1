#!/usr/bin/env pwsh
# Launch the OCTGN Electron client.
#
# Usage:
#   ./launch-electron.ps1              Dev mode (Vite hot-reload + Electron)
#   ./launch-electron.ps1 -Production  Build and run from compiled output
#   ./launch-electron.ps1 -Install     Force npm install first

param(
    [switch]$Production,
    [switch]$Build,
    [switch]$Install
)

$ErrorActionPreference = 'Stop'
$electronDir = Join-Path $PSScriptRoot 'octgnFX' 'Octgn.Electron'

if (-not (Test-Path $electronDir)) {
    Write-Error "Electron project not found at $electronDir"
    return
}

Push-Location $electronDir
try {
    # Install deps if requested or node_modules missing
    if ($Install -or -not (Test-Path 'node_modules')) {
        Write-Host '[1/3] Installing dependencies...' -ForegroundColor Cyan
        npm install
        if ($LASTEXITCODE -ne 0) { throw 'npm install failed' }
    }

    if ($Production -or $Build) {
        Write-Host '[2/3] Building...' -ForegroundColor Cyan
        npx tsc -p tsconfig.main.json
        if ($LASTEXITCODE -ne 0) { throw 'Main process build failed' }
        npx vite build
        if ($LASTEXITCODE -ne 0) { throw 'Renderer build failed' }
    }

    if ($Production) {
        Write-Host '[3/3] Starting OCTGN (production)...' -ForegroundColor Green
        npx electron dist/main/index.js
    }
    else {
        Write-Host '[2/3] Building main process...' -ForegroundColor Cyan
        npx tsc -p tsconfig.main.json
        if ($LASTEXITCODE -ne 0) { throw 'Main process build failed' }

        Write-Host '[3/3] Starting OCTGN (dev mode)...' -ForegroundColor Green
        Write-Host '     Vite dev server will start on http://localhost:5173' -ForegroundColor DarkGray
        Write-Host '     Electron will launch once the server is ready' -ForegroundColor DarkGray
        Write-Host ''

        # Start Vite in background, poll until ready, then launch Electron
        $viteJob = Start-Job -ScriptBlock {
            param($dir)
            Set-Location $dir
            npx vite dev 2>&1
        } -ArgumentList (Get-Location).Path

        # Wait for Vite to be ready
        $ready = $false
        for ($i = 0; $i -lt 30; $i++) {
            Start-Sleep -Seconds 1
            try {
                $null = Invoke-WebRequest -Uri 'http://localhost:5173' -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
                $ready = $true
                break
            }
            catch { }
        }

        if (-not $ready) {
            Stop-Job $viteJob -ErrorAction SilentlyContinue
            Remove-Job $viteJob -Force -ErrorAction SilentlyContinue
            throw 'Vite dev server did not start within 30 seconds'
        }

        Write-Host '     Vite is ready, launching Electron...' -ForegroundColor Green
        try {
            npx electron dist/main/index.js
        }
        finally {
            Stop-Job $viteJob -ErrorAction SilentlyContinue
            Remove-Job $viteJob -Force -ErrorAction SilentlyContinue
        }
    }
}
finally {
    Pop-Location
}
