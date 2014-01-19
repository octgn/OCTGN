param($key, $istest)
try
{
	$nuspecFile = "Octgn.Online.StandAloneServer.nuspec"
	$relativeNugetPath = "../../packages/NuGet.CommandLine.2.7.3/tools/NuGet.exe";
	$relativeNuspecPath = "Octgn.Online.StandAloneServer.nuspec";
	$feed = "https://www.myget.org/F/octgn/api/v2/package";

	# Put us in the parent folder of this script
	$Invocation = (Get-Variable MyInvocation -Scope 0).Value
	$wd = New-Object System.Uri(Split-Path $Invocation.MyCommand.Path)
	Push-Location $wd.LocalPath
	[Environment]::CurrentDirectory = $PWD

	if($istest -eq $true)
	{
		Write-Host("Is in test mode")
		Set-Content $nuspecFile (gc $nuspecFile).replace("bin\Release\*.*","bin\Release_Test\*.*")
	}
	else
	{	
		Write-Host("NOT in test mode")	
		Set-Content $nuspecFile (gc $nuspecFile).replace("bin\Release_Test\*.*","bin\Release\*.*")
	}

	# Grab the nuget exe path
	$nugetPath = (Resolve-Path $relativeNugetPath)
	Write-Host ("Nuget Path: " + $nugetPath)

	# Create nuget command line expression
	$packExpression = '&"' + $nugetPath+ '" pack "' + $relativeNuspecPath + '"'
	Write-Host ("Pack Expression: " + $packExpression)

	# Create nuget deploy expression
	$deployExpression = '&"' + $nugetPath+ '" push *.nupkg ' + $key + ' -Source ' + $feed
	Write-Host ("Deploy Expression: " + $deployExpression)

	
	# Pack project
	Write-Host ("Packing Project")
	Invoke-Expression $packExpression

	Write-Host ("Deploying Project")
	Invoke-Expression $deployExpression

	Write-Host "Finished"
}
catch
{
	Write-Host "Failed to deploy: " $_
	exit 1
}
finally
{
	#delete nupkg files
	Get-ChildItem –Path *.nupkg -Force | 
		foreach ($_) {remove-item $_.fullname}

	Pop-Location
	[Environment]::CurrentDirectory = $PWD
}