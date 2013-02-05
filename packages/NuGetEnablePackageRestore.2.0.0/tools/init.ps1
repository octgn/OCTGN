param($installPath, $toolsPath, $package, $project)

$modules = Get-ChildItem $ToolsPath -Filter *.psm1
$modules | ForEach-Object { import-module -name  $_.FullName }

@"
========================
NuGet Enable Package Restore Fix
========================
To fix package restore:
1. Please enable package restore in Visual Studio FIRST.
2. Run the command: Install-NuGetEnablePackageRestoreFix 
3. Restart Visual Studio.

This should fix that pesky broken build you are experiencing.

To uninstall: 
1. Run the command: Remove-NuGetEnablePackageRestoreFix
2. Uninstall-Package NuGetEnablePackageRestore
3. Restart Visual Studio.
========================
"@ | Write-Host