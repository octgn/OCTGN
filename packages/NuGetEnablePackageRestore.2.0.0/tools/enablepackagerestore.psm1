$toolsDir = (Split-Path -parent $MyInvocation.MyCommand.Definition)
$nugetLocalAppDir = Join-Path $env:LocalAppData 'NuGet'
$backupDir = Join-Path $nugetLocalAppDir '.nugetbkp'
$nugetFixDir = Join-Path $toolsDir 'nuget'

function Install-NuGetEnablePackageRestoreFix {
  $solutionFolder = $PWD
  $nugetPRDir = Join-Path $solutionFolder '.nuget'

  Write-Host "Backing up current .nuget folder to  `'$backupDir`'"
  Copy-Item $nugetPRDir $backupDir -recurse -force
  
  Write-Host "Fixing .nuget folder with contents of `'$nugetFixDir`'"
  Copy-Item $nugetFixDir\*.* $nugetPRDir -recurse -force
  
@"
NuGetPackageRestore has been fixed.
"@ | write-host
}

function Remove-NuGetEnablePackageRestoreFix {
  $solutionFolder = $PWD
  $nugetPRDir = Join-Path $solutionFolder '.nuget'
  
  Write-Host "Setting current .nuget folder to  `'$backupDir`'"
  Copy-Item $backupDir\*.* $nugetPRDir  -recurse -force
  
@"
NuGetPackageRestore is back to the way it was before.
"@ | write-host
}

export-modulemember -function Install-NuGetEnablePackageRestoreFix;
export-modulemember -function Remove-NuGetEnablePackageRestoreFix;