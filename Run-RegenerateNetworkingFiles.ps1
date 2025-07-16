# PowerShell script to regenerate all networking T4 templates in OCTGN
# This script must be run whenever Protocol.xml is modified

Write-Host "OCTGN Networking Files Regeneration Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Function to get BOM bytes if present
function Get-FileBOMBytes {
    param(
        [string]$Path
    )
    if (-not (Test-Path $Path)) {
        return @()
    }
    [byte[]]$all = [System.IO.File]::ReadAllBytes($Path)
    if ($all.Length -ge 3 -and $all[0] -eq 0xEF -and $all[1] -eq 0xBB -and $all[2] -eq 0xBF) {
        return 0xEF,0xBB,0xBF
    }
    return @()
}

# Function to prepend BOM if it was present before
function Prepend-BOMIfMissing {
    param(
        [string]$Path,
        [byte[]]$BOM
    )
    if (-not (Test-Path $Path) -or $BOM.Count -eq 0) {
        return
    }
    [byte[]]$newBytes = [System.IO.File]::ReadAllBytes($Path)
    if ($newBytes.Length -ge 3 -and $newBytes[0] -eq $BOM[0] -and $newBytes[1] -eq $BOM[1] -and $newBytes[2] -eq $BOM[2]) {
        return
    }
    [byte[]]$combined = New-Object byte[] ($BOM.Length + $newBytes.Length)
    [Array]::Copy($BOM, 0, $combined, 0, $BOM.Length)
    [Array]::Copy($newBytes, 0, $combined, $BOM.Length, $newBytes.Length)
    [System.IO.File]::WriteAllBytes($Path, $combined)
}

# Check if dotnet t4 tool is installed
Write-Host "Checking for dotnet t4 tool..." -ForegroundColor Yellow
try {
    $t4Command = Get-Command t4 -ErrorAction Stop
    Write-Host "✓ dotnet t4 tool found at: $($t4Command.Source)" -ForegroundColor Green
} catch {
    Write-Host "✗ dotnet t4 tool not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install the dotnet t4 tool first:" -ForegroundColor Yellow
    Write-Host "  dotnet tool install -g dotnet-t4" -ForegroundColor White
    Write-Host ""
    Write-Host "Or install it locally to this project:" -ForegroundColor Yellow
    Write-Host "  dotnet tool install dotnet-t4" -ForegroundColor White
    exit 1
}

# Verify Protocol.xml exists
$protocolFile = "octgnFX\Octgn.Server\Protocol.xml"
if (-not (Test-Path $protocolFile)) {
    Write-Host "✗ Protocol.xml not found at: $protocolFile" -ForegroundColor Red
    Write-Host "Please run this script from the OCTGN root directory." -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Protocol.xml found" -ForegroundColor Green
Write-Host ""

# Define networking T4 templates to regenerate
$networkingTemplates = @(
    @{
        Directory = "octgnFX\Octgn.JodsEngine\Networking"
        Templates = @(
            @{ Template = "BinaryStubs.tt"; Output = "BinaryStubs.cs" },
            @{ Template = "BinaryParser.tt"; Output = "BinaryParser.cs" },
            @{ Template = "IServerCalls.tt"; Output = "IServerCalls.cs" }
        )
    },
    @{
        Directory = "octgnFX\Octgn.Server"
        Templates = @(
            @{ Template = "BinaryStubs.tt"; Output = "BinaryStubs.cs" },
            @{ Template = "BinaryParser.tt"; Output = "BinaryParser.cs" },
            @{ Template = "IClientCalls.tt"; Output = "IClientCalls.cs" },
            @{ Template = "Broadcaster.tt"; Output = "Broadcaster.cs" }
        )
    }
)

$totalProcessed = 0
$totalErrors = 0

# Process each template group
foreach ($templateGroup in $networkingTemplates) {
    $dir = $templateGroup.Directory

    Write-Host "Processing networking templates in: $dir" -ForegroundColor Cyan

    if (-not (Test-Path $dir)) {
        Write-Host "  ✗ Directory not found: $dir" -ForegroundColor Red
        $totalErrors++
        continue
    }

    Push-Location $dir
    try {
        foreach ($template in $templateGroup.Templates) {
            $templateFile = $template.Template
            $outputFile = $template.Output

            if (-not (Test-Path $templateFile)) {
                Write-Host "  ✗ Template not found: $templateFile" -ForegroundColor Red
                $totalErrors++
                continue
            }

            # Build full path to output file
            $fullOutputPath = Join-Path -Path (Get-Location) -ChildPath $outputFile

            # Capture existing BOM
            $bom = Get-FileBOMBytes -Path $fullOutputPath

            Write-Host "  Generating $outputFile from $templateFile..." -NoNewline

            try {
                $result = & t4 -o $outputFile $templateFile 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host " ✓" -ForegroundColor Green
                    $totalProcessed++

                    # Re-add BOM if needed
                    Prepend-BOMIfMissing -Path $fullOutputPath -BOM $bom
                } else {
                    Write-Host " ✗" -ForegroundColor Red
                    Write-Host "    Error: $result" -ForegroundColor Red
                    $totalErrors++
                }
            } catch {
                Write-Host " ✗" -ForegroundColor Red
                Write-Host "    Exception: $_" -ForegroundColor Red
                $totalErrors++
            }
        }
    } finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Templates processed: $totalProcessed" -ForegroundColor Green
if ($totalErrors -gt 0) {
    Write-Host "  Errors: $totalErrors" -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠️  Some templates failed to generate. Please check the errors above." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "  Errors: 0" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ All networking templates regenerated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANT: The following files were regenerated and should be committed:" -ForegroundColor Yellow
    Write-Host "  - octgnFX/Octgn.JodsEngine/Networking/BinaryStubs.cs" -ForegroundColor White
    Write-Host "  - octgnFX/Octgn.JodsEngine/Networking/BinaryParser.cs" -ForegroundColor White
    Write-Host "  - octgnFX/Octgn.JodsEngine/Networking/IServerCalls.cs" -ForegroundColor White
    Write-Host "  - octgnFX/Octgn.Server/BinaryStubs.cs" -ForegroundColor White
    Write-Host "  - octgnFX/Octgn.Server/BinaryParser.cs" -ForegroundColor White
    Write-Host "  - octgnFX/Octgn.Server/IClientCalls.cs" -ForegroundColor White
    Write-Host "  - octgnFX/Octgn.Server/Broadcaster.cs" -ForegroundColor White
}