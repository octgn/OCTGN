# Heat File List Generator

Scripts to update the WiX installer file list while preserving component GUIDs.

## Quick Start

```cmd
# Build your app first
msbuild octgnFX\Octgn\Octgn.csproj /p:Configuration=Debug

# Update heat file list
deploy\update-heat-filelist.bat

# Build installer
msbuild octgnFX\Octgn.Installer\Octgn.Installer.wixproj
```

## Scripts

- **`update-heat-filelist.bat`** - Simple wrapper (use this)
- **`Update-HeatFileList.ps1`** - PowerShell script that does the work

## Why Use This?

- ✅ Preserves existing component GUIDs (upgrade compatibility)
- ✅ Only regenerates when files actually change
- ✅ Shows exactly what's new/removed/modified
- ✅ Filters out .pdb/.xml files like the installer does

## Options

```cmd
update-heat-filelist.bat Debug    # Debug config
update-heat-filelist.bat Release  # Release config  
update-heat-filelist.bat -Force   # Force regeneration
```

Run this whenever you add/remove files from your application.
