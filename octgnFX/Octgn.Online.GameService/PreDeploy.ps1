try
{
    #$path= "C:\\Programming\\OCTGN\\octgnFX\\Octgn.Online.GameService\\bin\\Debug\\Octgn.Online.GameService.exe"
	$path = $OctopusParameters["OctopusOriginalPackageDirectoryPath"] + "/Octgn.Online.GameService.exe"
	Write-Host ("Exe Path: " + $path)

	$startInfo = New-Object Diagnostics.ProcessStartInfo
	$startInfo.Filename = $path
	$startInfo.UseShellExecute = $false
	$startInfo.Arguments = "kill";

	Write-Host "Killing Process"
	$Proc = [Diagnostics.Process]::Start($startInfo)
	#$Proc = [Diagnostics.Process]::Start($path, "kill")

	$Proc.WaitForExit()
	Write-Host "Process Killed?"
}
catch
{
	Write-Host "Failed PreDeploy: " $_
	exit 1
}