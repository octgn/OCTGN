<#
.SYNOPSIS
    Launches the OCTGN Electron cross-platform client on Windows.
    
.DESCRIPTION
    This script sets up and runs the OCTGN Electron client.
    It will install dependencies if needed and then launch the app.
    
.PARAMETER Production
    Launch the production build instead of development mode.
    
.PARAMETER Install
    Force reinstall dependencies.
    
.PARAMETER Build
    Build the application before launching.
    
.PARAMETER Help
    Show this help message.
    
.EXAMPLE
    .\launch.ps1
    Launches in development mode.
    
.EXAMPLE
    .\launch.ps1 -Production
    Builds and launches the production version.
    
.EXAMPLE
    .\launch.ps1 -Install
    Reinstalls all dependencies.
#>

param(
    [switch]$Production,
    [switch]$Install,
    [switch]$Build,
    [switch]$Help
)

# Colors for output
function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Magenta
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Magenta
    Write-Host ""
}

function Write-Success {
    param([string]$Text)
    Write-Host "✓ $Text" -ForegroundColor Green
}

function Write-Info {
    param([string]$Text)
    Write-Host "→ $Text" -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Text)
    Write-Host "⚠ $Text" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Text)
    Write-Host "✗ $Text" -ForegroundColor Red
}

# Show help
if ($Help) {
    Get-Help $MyInvocation.MyCommand.Path -Detailed
    exit 0
}

# Header
Write-Header "OCTGN Electron Launcher"
Write-Host "Cross-platform card gaming" -ForegroundColor Gray
Write-Host ""

# Script location
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ElectronDir = Join-Path $ScriptDir "octgn-electron"

# Check if octgn-electron directory exists
if (-not (Test-Path $ElectronDir)) {
    Write-Error "octgn-electron directory not found at: $ElectronDir"
    Write-Info "Make sure you're running this script from the OCTGN repository root."
    exit 1
}

Write-Info "Electron directory: $ElectronDir"

# Check for Node.js
Write-Info "Checking for Node.js..."
try {
    $nodeVersion = node --version
    Write-Success "Node.js $nodeVersion found"
} catch {
    Write-Error "Node.js not found. Please install Node.js 18+ from https://nodejs.org/"
    exit 1
}

# Check for npm
Write-Info "Checking for npm..."
try {
    $npmVersion = npm --version
    Write-Success "npm $npmVersion found"
} catch {
    Write-Error "npm not found. Please install Node.js 18+ from https://nodejs.org/"
    exit 1
}

# Navigate to electron directory
Set-Location $ElectronDir

# Install dependencies if needed or requested
$NodeModulesPath = Join-Path $ElectronDir "node_modules"
if ($Install -or -not (Test-Path $NodeModulesPath)) {
    Write-Header "Installing Dependencies"
    
    if ($Install) {
        Write-Info "Forcing reinstall..."
        Remove-Item -Recurse -Force $NodeModulesPath -ErrorAction SilentlyContinue
    } else {
        Write-Info "First run, installing dependencies..."
    }
    
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install dependencies"
        exit 1
    }
    Write-Success "Dependencies installed"
}

# Build if requested or for production
if ($Build -or $Production) {
    Write-Header "Building Application"
    
    Write-Info "Building renderer..."
    npm run build:renderer
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build renderer"
        exit 1
    }
    
    Write-Info "Building main process..."
    npm run build:main
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build main process"
        exit 1
    }
    
    Write-Success "Build complete"
}

# Launch
Write-Header "Launching OCTGN"

if ($Production) {
    Write-Info "Starting production build..."
    npm run start
} else {
    Write-Info "Starting development server..."
    Write-Host ""
    Write-Host "Development mode:" -ForegroundColor Yellow
    Write-Host "  - Hot reload enabled" -ForegroundColor Gray
    Write-Host "  - DevTools available (Ctrl+Shift+I)" -ForegroundColor Gray
    Write-Host "  - Press Ctrl+C to stop" -ForegroundColor Gray
    Write-Host ""
    npm run dev
}

# Return to original directory
Set-Location $ScriptDir
