function Push-Changes($version)
{
	$path = (Resolve-Path ".\octgnFX\Octgn.Online.GameService")
	Invoke-Expression "git add `"$path/*`""
	Invoke-Expression "git commit -m `"Auto generated commit for Octgn.Online.GameService version $version`""
	Invoke-Expression "git tag -a $version -m `"Auto generated commit for Octgn.Online.GameService version $version`""
	Invoke-Expression "git push origin HEAD:test"
	Invoke-Expression "git push --tags origin"
}

function Get-AssemblyInfoVersion($path)
{
	$rp = (Resolve-Path $path)
	# Get the line containing the AssemblyVersion custom attribute
	$attr = (get-content "$rp" | select-string "AssemblyVersion").ToString()

	# Parse the attribute to get the 3 digit version
	$s = $attr.IndexOf("`"")+1
	$e = $attr.LastIndexOf("`"")
	$version = $attr.Substring($s,$e-$s)
	return $version
}

try
{
	$versionString = Get-AssemblyInfoVersion ".\octgnFX\Octgn.Online.GameService\Properties\AssemblyInfo.cs"
	$version = New-Object System.Version($versionString)
	Push-Changes $version
}
catch
{
	Write-Host "Failed PostBuild: " $_
	exit 1
}