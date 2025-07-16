# PowerShell script to regenerate all networking T4 templates in OCTGN
# This script must be run whenever Protocol.xml is modified

Write-Host "OCTGN Networking Files Regeneration Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Check if dotnet t4 tool is installed
Write-Host "Checking for dotnet t4 tool..." -ForegroundColor Yellow
try {
    $t4Version = & t4 --version 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet t4 tool not found"
    }
    Write-Host "✓ dotnet t4 tool found: $t4Version" -ForegroundColor Green
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
            
            Write-Host "  Generating $outputFile from $templateFile..." -NoNewline
            
            try {
                $result = & t4 -o $outputFile $templateFile 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host " ✓" -ForegroundColor Green
                    $totalProcessed++
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
