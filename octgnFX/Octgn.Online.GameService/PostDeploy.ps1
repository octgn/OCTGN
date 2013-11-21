try
{
	$path = $OctopusParameters["OctopusOriginalPackageDirectoryPath"] + "/Octgn.Online.GameService.exe"
	Write-Host ("Exe Path: " + $path)

	$exp = '&' + 'Start-Process "' + $path + '"'
	Write-Host ("Exp: " + $exp)

	Write-Host "Starting Process"
	Invoke-Expression $exp
	Write-Host "Process Started"
}
catch
{
	Write-Host "Failed PostDeploy: " $_
	exit 1
}