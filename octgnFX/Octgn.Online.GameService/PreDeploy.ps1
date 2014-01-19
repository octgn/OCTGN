try
{
	Write-Host "Looking for process Octgn.Online.GameService"
	$ProcessActive = Get-Process Octgn.Online.GameService -ErrorAction SilentlyContinue
	if($ProcessActive -eq $null)
	{
		Write-host "Process not running."
		return
	}
	else
	{
		Write-host "Process running, trying to kill"
		stop-process -name Octgn.Online.GameService -force
		Write-Host "Process killed successfully"
	}
}
catch
{
	Write-Host "Failed PreDeploy: " $_
	exit 1
}