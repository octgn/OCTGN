try
{
    #$path= "C:\\Users\\Kelly\\Programming\\OCTGN\\octgnFX\\Octgn.Online.MatchmakingService\\bin\\Debug\\Octgn.Online.MatchmakingService.exe"
	$path = $OctopusParameters["OctopusOriginalPackageDirectoryPath"] + "\\Octgn.Online.MatchmakingService.exe"
	Write-Host ("Exe Path: " + $path)

	Write-Host "Starting Process"
	Start-Process -FilePath $path
	Write-Host "Process Started"
}
catch
{
	Write-Host "Failed PostDeploy: " $_
	exit 1
}