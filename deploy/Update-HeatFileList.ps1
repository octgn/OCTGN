param(
    [string]$Configuration = "Debug",
    [string]$WixToolPath = "",
    [switch]$Force
)

<#
.SYNOPSIS
    Updates the Heat-generated file list while preserving existing GUIDs.

.DESCRIPTION
    This script uses WiX Heat.exe to generate a new file list from the built application,
    but preserves existing component GUIDs to maintain upgrade compatibility.

.PARAMETER Configuration
    The build configuration (Debug or Release). Default is Debug.

.PARAMETER WixToolPath
    Path to WiX tools directory. If not specified, will attempt to find automatically.

.PARAMETER Force
    Force regeneration even if target files are newer than source files.

.EXAMPLE
    .\Update-HeatFileList.ps1 -Configuration Release
    .\Update-HeatFileList.ps1 -Configuration Debug -Force
#>

# Set error handling
$ErrorActionPreference = "Stop"

# Get script directory and set paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootPath = Split-Path -Parent $ScriptDir
$InstallerLibPath = Join-Path $RootPath "octgnFX\Octgn.InstallerLib"
$OctgnBinPath = Join-Path $RootPath "octgnFX\Octgn\bin\$Configuration"
$HeatOutputFile = Join-Path $InstallerLibPath "HeatGeneratedFileList.wxs"
$XsltTransformFile = Join-Path $InstallerLibPath "HeatGeneratedFileList.xslt"
$TempHeatFile = Join-Path $InstallerLibPath "HeatGeneratedFileList.temp.wxs"

Write-Host "OCTGN Heat File List Generator" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green
Write-Host "Configuration: $Configuration"
Write-Host "Root Path: $RootPath"
Write-Host "Octgn Bin Path: $OctgnBinPath"
Write-Host "Heat Output: $HeatOutputFile"
Write-Host ""

# Check if source directory exists
if (-not (Test-Path $OctgnBinPath)) {
    Write-Error "Source directory not found: $OctgnBinPath`nPlease build the Octgn project first in $Configuration configuration."
}

# Check if we need to update (unless Force is specified)
if (-not $Force -and (Test-Path $HeatOutputFile)) {
    Write-Host "Analyzing if regeneration is needed..." -ForegroundColor Yellow    # Get current file list from bin directory (filtered like XSLT does)
    $CurrentFiles = @{}
    $FilteredFiles = @{}
    Get-ChildItem $OctgnBinPath -Recurse -File | ForEach-Object {
        $RelativePath = $_.FullName.Substring($OctgnBinPath.Length + 1)
        
        # Apply same filters as XSLT transform
        $shouldInclude = $true
        $filterReason = ""
        
        # Filter out .pdb files
        if ($_.Extension -eq ".pdb") {
            $shouldInclude = $false
            $filterReason = "PDB file"
        }
        
        # Filter out .xml files  
        if ($_.Extension -eq ".xml") {
            $shouldInclude = $false
            $filterReason = "XML file"
        }
        
        # Filter out data.path files
        if ($_.Name -eq "data.path") {
            $shouldInclude = $false
            $filterReason = "data.path file"
        }
        
        # Filter out files in Logs directories
        if ($RelativePath -match "\\Logs\\|^Logs\\") {
            $shouldInclude = $false
            $filterReason = "Logs directory"
        }
        
        if ($shouldInclude) {
            $CurrentFiles[$RelativePath] = @{
                Size = $_.Length
                LastWrite = $_.LastWriteTime
                Hash = (Get-FileHash $_.FullName -Algorithm MD5).Hash
            }
        } else {
            $FilteredFiles[$RelativePath] = $filterReason
        }
    }
    
    # Show filtering summary
    if ($FilteredFiles.Count -gt 0) {
        Write-Host "Filtered out $($FilteredFiles.Count) files (same as XSLT transform):" -ForegroundColor Gray
        $FilteredFiles.GetEnumerator() | Group-Object Value | ForEach-Object {
            Write-Host "  $($_.Name): $($_.Count) files" -ForegroundColor Gray
        }
        Write-Host ""
    }
    
    # Parse existing Heat file to get tracked files
    $ExistingFiles = @{}
    $NeedsUpdate = $false
    $Reasons = @()
      try {
        [xml]$ExistingXml = Get-Content $HeatOutputFile -Encoding UTF8
        
        # Create namespace manager for XPath queries
        $nsManager = New-Object System.Xml.XmlNamespaceManager($ExistingXml.NameTable)
        $nsManager.AddNamespace("wix", "http://schemas.microsoft.com/wix/2006/wi")
        
        $FileElements = $ExistingXml.SelectNodes("//wix:File[@Source]", $nsManager)
        
        foreach ($FileElement in $FileElements) {
            $SourcePath = $FileElement.GetAttribute("Source")
            if ($SourcePath -match '\$\(var\.HarvestPath\)\\(.+)$') {
                $RelativePath = $Matches[1]
                $ExistingFiles[$RelativePath] = $true
            }
        }
        
        # Check for new files
        $NewFiles = @()
        foreach ($FilePath in $CurrentFiles.Keys) {
            if (-not $ExistingFiles.ContainsKey($FilePath)) {
                $NewFiles += $FilePath
            }
        }
        
        # Check for removed files
        $RemovedFiles = @()
        foreach ($FilePath in $ExistingFiles.Keys) {
            if (-not $CurrentFiles.ContainsKey($FilePath)) {
                $RemovedFiles += $FilePath
            }
        }
          # Check for modified files (size or timestamp changes)
        $ModifiedFiles = @()
        foreach ($FilePath in $CurrentFiles.Keys) {
            if ($ExistingFiles.ContainsKey($FilePath)) {
                $CurrentFile = $CurrentFiles[$FilePath]
                
                # Check if file is significantly newer (more than 10 seconds difference)
                $ExistingFileTime = (Get-Item $HeatOutputFile).LastWriteTime
                if ($CurrentFile.LastWrite -gt $ExistingFileTime.AddSeconds(10)) {
                    $ModifiedFiles += $FilePath
                }
            }
        }
        
        # Determine if update is needed
        if ($NewFiles.Count -gt 0) {
            $NeedsUpdate = $true
            $Reasons += "New files detected: $($NewFiles.Count) files"
        }
        
        if ($RemovedFiles.Count -gt 0) {
            $NeedsUpdate = $true
            $Reasons += "Removed files detected: $($RemovedFiles.Count) files"
        }
        
        if ($ModifiedFiles.Count -gt 0) {
            $NeedsUpdate = $true
            $Reasons += "Modified files detected: $($ModifiedFiles.Count) files"
        }
        
        # Show detailed analysis
        Write-Host "Analysis Results:" -ForegroundColor Cyan
        Write-Host "  Current files in bin: $($CurrentFiles.Count)" -ForegroundColor White
        Write-Host "  Files in Heat list: $($ExistingFiles.Count)" -ForegroundColor White
        Write-Host "  New files: $($NewFiles.Count)" -ForegroundColor $(if ($NewFiles.Count -gt 0) { "Green" } else { "Gray" })
        Write-Host "  Removed files: $($RemovedFiles.Count)" -ForegroundColor $(if ($RemovedFiles.Count -gt 0) { "Yellow" } else { "Gray" })
        Write-Host "  Modified files: $($ModifiedFiles.Count)" -ForegroundColor $(if ($ModifiedFiles.Count -gt 0) { "Yellow" } else { "Gray" })
        
        if ($NewFiles.Count -gt 0) {
            Write-Host "  New files:" -ForegroundColor Green
            $NewFiles | ForEach-Object { Write-Host "    + $_" -ForegroundColor Green }
        }
        
        if ($RemovedFiles.Count -gt 0) {
            Write-Host "  Removed files:" -ForegroundColor Yellow
            $RemovedFiles | ForEach-Object { Write-Host "    - $_" -ForegroundColor Yellow }
        }
        
        if ($ModifiedFiles.Count -gt 0 -and $ModifiedFiles.Count -le 10) {
            Write-Host "  Modified files:" -ForegroundColor Yellow
            $ModifiedFiles | ForEach-Object { Write-Host "    * $_" -ForegroundColor Yellow }
        } elseif ($ModifiedFiles.Count -gt 10) {
            Write-Host "  Modified files: (showing first 10 of $($ModifiedFiles.Count))" -ForegroundColor Yellow
            $ModifiedFiles[0..9] | ForEach-Object { Write-Host "    * $_" -ForegroundColor Yellow }
            Write-Host "    ... and $($ModifiedFiles.Count - 10) more" -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "Could not parse existing Heat file: $($_.Exception.Message)" -ForegroundColor Yellow
        $NeedsUpdate = $true
        $Reasons += "Cannot parse existing Heat file"
    }
    
    if (-not $NeedsUpdate) {
        Write-Host ""
        Write-Host "Heat file list is up to date - no changes detected." -ForegroundColor Green
        Write-Host "Use -Force to regenerate anyway." -ForegroundColor Gray
        exit 0
    } else {
        Write-Host ""
        Write-Host "Update needed because:" -ForegroundColor Yellow
        $Reasons | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
        Write-Host ""
    }
}

# Find WiX tools
if ([string]::IsNullOrEmpty($WixToolPath)) {
    Write-Host "Searching for WiX tools..." -ForegroundColor Yellow
    
    $PossiblePaths = @(
        "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin",
        "${env:ProgramFiles}\WiX Toolset v3.11\bin",
        "${env:ProgramFiles(x86)}\WiX Toolset v3.10\bin",
        "${env:ProgramFiles}\WiX Toolset v3.10\bin",
        "${env:ProgramFiles(x86)}\WiX Toolset v3.14\bin",
        "${env:ProgramFiles}\WiX Toolset v3.14\bin",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Microsoft\WiX\v3.x",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\MSBuild\Microsoft\WiX\v3.x",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Microsoft\WiX\v3.x",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Microsoft\WiX\v3.x",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Microsoft\WiX\v3.x",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\WiX\v3.x"
    )
    
    # Also check PATH environment variable
    $PathDirs = $env:PATH -split ';'
    foreach ($PathDir in $PathDirs) {
        if ($PathDir -and (Test-Path (Join-Path $PathDir "heat.exe") -ErrorAction SilentlyContinue)) {
            $WixToolPath = $PathDir
            break
        }
    }
    
    # Check predefined paths if not found in PATH
    if ([string]::IsNullOrEmpty($WixToolPath)) {
        foreach ($Path in $PossiblePaths) {
            Write-Host "  Checking: $Path" -ForegroundColor Gray
            if (Test-Path $Path -ErrorAction SilentlyContinue) {
                if (Test-Path (Join-Path $Path "heat.exe") -ErrorAction SilentlyContinue) {
                    $WixToolPath = $Path
                    Write-Host "  Found WiX tools at: $Path" -ForegroundColor Green
                    break
                }
            }
        }
    }
}

if ([string]::IsNullOrEmpty($WixToolPath)) {
    Write-Error @"
Cannot find WiX tools (heat.exe). Please either:
1. Install WiX Toolset v3.11 or later from https://wixtoolset.org/
2. Add WiX tools to your PATH environment variable
3. Use the -WixToolPath parameter to specify the location

Example: .\Update-HeatFileList.ps1 -WixToolPath "C:\Program Files (x86)\WiX Toolset v3.11\bin"
"@
}

$HeatExe = Join-Path $WixToolPath "heat.exe"
if (-not (Test-Path $HeatExe)) {
    Write-Error "heat.exe not found at: $HeatExe"
}

Write-Host "Using WiX tools from: $WixToolPath"

# Extract existing GUIDs from current file
$ExistingGuids = @{}
if (Test-Path $HeatOutputFile) {
    Write-Host "Extracting existing GUIDs..." -ForegroundColor Yellow
    
    try {
        [xml]$ExistingXml = Get-Content $HeatOutputFile -Encoding UTF8
        
        # Create namespace manager for XPath queries
        $nsManager = New-Object System.Xml.XmlNamespaceManager($ExistingXml.NameTable)
        $nsManager.AddNamespace("wix", "http://schemas.microsoft.com/wix/2006/wi")
        
        $ExistingComponents = $ExistingXml.SelectNodes("//wix:Component[@Id and @Guid and @Guid != '*']", $nsManager)
        
        foreach ($Component in $ExistingComponents) {
            $ComponentId = $Component.GetAttribute("Id")
            $Guid = $Component.GetAttribute("Guid")
            if ($Guid -and $Guid -ne "*" -and $Guid -ne "") {
                $ExistingGuids[$ComponentId] = $Guid
            }
        }
        
        Write-Host "Found $($ExistingGuids.Count) existing GUIDs to preserve"
    } catch {
        Write-Host "Warning: Could not extract existing GUIDs: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "Proceeding with new GUID generation..." -ForegroundColor Yellow
    }
}

# Generate new heat file
Write-Host "Running Heat.exe..." -ForegroundColor Yellow

$HeatArgs = @(
    "dir", $OctgnBinPath,
    "-var", "var.HarvestPath",
    "-out", $TempHeatFile,
    "-cg", "O__HeatGenerated",
    "-dr", "INSTALLDIR",
    "-ag",
    "-sfrag",
    "-sreg",
    "-srd",
    "-gg"
)

if (Test-Path $XsltTransformFile) {
    $HeatArgs += @("-t", $XsltTransformFile)
}

$Process = Start-Process -FilePath $HeatExe -ArgumentList $HeatArgs -Wait -PassThru -NoNewWindow
if ($Process.ExitCode -ne 0) {
    Write-Error "Heat.exe failed with exit code $($Process.ExitCode)"
}

# Read the generated file and restore GUIDs
Write-Host "Restoring existing GUIDs..." -ForegroundColor Yellow

[xml]$NewXml = Get-Content $TempHeatFile -Encoding UTF8

# Create namespace manager for the new XML
$nsManager = New-Object System.Xml.XmlNamespaceManager($NewXml.NameTable)
$nsManager.AddNamespace("wix", "http://schemas.microsoft.com/wix/2006/wi")

$NewComponents = $NewXml.SelectNodes("//wix:Component[@Id]", $nsManager)

$RestoredCount = 0
$NewCount = 0

foreach ($Component in $NewComponents) {
    $ComponentId = $Component.GetAttribute("Id")
    
    if ($ExistingGuids.ContainsKey($ComponentId)) {
        # Restore existing GUID
        $Component.SetAttribute("Guid", $ExistingGuids[$ComponentId])
        $RestoredCount++
    } else {
        # Generate new GUID for new components
        $NewGuid = [System.Guid]::NewGuid().ToString("B").ToUpper()
        $Component.SetAttribute("Guid", $NewGuid)
        $NewCount++
    }
}

# Save the final file
$NewXml.Save($HeatOutputFile)

# Clean up temp file
Remove-Item $TempHeatFile -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Heat file list updated successfully!" -ForegroundColor Green
Write-Host "- Restored GUIDs: $RestoredCount" -ForegroundColor Green
Write-Host "- New GUIDs: $NewCount" -ForegroundColor Green
Write-Host "- Output file: $HeatOutputFile" -ForegroundColor Green

# Show summary of changes
if ($NewCount -gt 0) {
    Write-Host ""
    Write-Host "New components detected:" -ForegroundColor Cyan
    
    # Create namespace manager for final summary
    $nsManager = New-Object System.Xml.XmlNamespaceManager($NewXml.NameTable)
    $nsManager.AddNamespace("wix", "http://schemas.microsoft.com/wix/2006/wi")
    
    $NewComponents = $NewXml.SelectNodes("//wix:Component[@Id]", $nsManager)
    foreach ($Component in $NewComponents) {
        $ComponentId = $Component.GetAttribute("Id")
        if (-not $ExistingGuids.ContainsKey($ComponentId)) {
            $FileElement = $Component.SelectSingleNode("wix:File", $nsManager)
            if ($FileElement) {
                $SourcePath = $FileElement.GetAttribute("Source")
                $FileName = Split-Path $SourcePath -Leaf
                Write-Host "  - $FileName" -ForegroundColor Cyan
            }
        }
    }
}

Write-Host ""
Write-Host "Remember to commit the updated HeatGeneratedFileList.wxs file to source control." -ForegroundColor Yellow
