# Launch the OCTGN Electron client (Windows).
#
# Usage:
#   .\launch-electron.ps1              Dev mode (Vite hot-reload + Electron)
#   .\launch-electron.ps1 -Production  Build and run from compiled output
#   .\launch-electron.ps1 -Install     Force npm install first

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
    # Always npm install on Windows if .cmd stubs are missing (e.g. deps installed from WSL)
    $binDir = Join-Path (Get-Location) 'node_modules' '.bin'
    $needsInstall = $Install -or
        -not (Test-Path 'node_modules') -or
        -not (Test-Path (Join-Path $binDir 'tsc.cmd'))

    if ($needsInstall) {
        Write-Host '[1/3] Installing dependencies...' -ForegroundColor Cyan
        npm install
        if ($LASTEXITCODE -ne 0) { throw 'npm install failed' }
    }

    # Helper: run a local .cmd bin stub directly
    function Invoke-Bin {
        param([string]$Tool, [string[]]$ToolArgs)
        $cmd = Join-Path $binDir "$Tool.cmd"
        if (-not (Test-Path $cmd)) { throw "$Tool not found at $cmd" }
        & $cmd @ToolArgs
        if ($LASTEXITCODE -ne 0) { throw "$Tool failed (exit code $LASTEXITCODE)" }
    }

    if ($Production -or $Build) {
        Write-Host '[2/3] Building...' -ForegroundColor Cyan
        Invoke-Bin 'tsc' @('-p', 'tsconfig.main.json')
        Invoke-Bin 'vite' @('build')
    }

    if ($Production) {
        Write-Host '[3/3] Starting OCTGN (production)...' -ForegroundColor Green
        Invoke-Bin 'electron' @('dist/main/index.js')
    }
    else {
        Write-Host '[2/3] Building main process...' -ForegroundColor Cyan
        Invoke-Bin 'tsc' @('-p', 'tsconfig.main.json')

        Write-Host '[3/3] Starting OCTGN (dev mode)...' -ForegroundColor Green

        # Start Vite dev server as a background process
        $viteBin = Join-Path $binDir 'vite.cmd'
        $viteProc = Start-Process -FilePath $viteBin -ArgumentList 'dev' `
            -WorkingDirectory (Get-Location).Path `
            -WindowStyle Hidden -PassThru

        try {
            # Poll until Vite is listening on port 5173
            $ready = $false
            for ($i = 0; $i -lt 30; $i++) {
                Start-Sleep -Seconds 1
                try {
                    $tcp = New-Object System.Net.Sockets.TcpClient
                    $tcp.Connect('127.0.0.1', 5173)
                    $tcp.Close()
                    $ready = $true
                    break
                }
                catch {
                    Write-Host "     Waiting for Vite... ($($i+1)s)" -ForegroundColor DarkGray -NoNewline
                    Write-Host "`r" -NoNewline
                }
            }

            if (-not $ready) {
                throw 'Vite dev server did not start within 30 seconds'
            }

            Write-Host '     Vite is ready, launching Electron...        ' -ForegroundColor Green
            $env:NODE_ENV = 'development'
            $electronBin = Join-Path $binDir 'electron.cmd'
            & $electronBin dist/main/index.js
        }
        finally {
            # Kill Vite and its child tree when Electron exits
            if ($viteProc -and -not $viteProc.HasExited) {
                taskkill /PID $viteProc.Id /T /F 2>$null
            }
        }
    }
}
finally {
    Pop-Location
}
