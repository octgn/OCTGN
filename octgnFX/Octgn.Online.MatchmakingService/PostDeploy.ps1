try
{
    #$path= "C:\\Users\\Kelly\\Programming\\OCTGN\\octgnFX\\Octgn.Online.MatchmakingService\\bin\\Debug\\Octgn.Online.MatchmakingService.exe"
	$path = $OctopusParameters["OctopusOriginalPackageDirectoryPath"] + "/Octgn.Online.MatchmakingService.exe"
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