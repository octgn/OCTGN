$nugetRelativePath = "../packages/NuGet.CommandLine.2.5.0/tools/NuGet.exe"
$projFile = "Octgn.Core.csproj"
$buildTarget = "Release"
try
{
        ## Working Directory Stuff
        $Invocation = (Get-Variable MyInvocation -Scope 0).Value
        $wd = New-Object System.Uri(Split-Path $Invocation.MyCommand.Path)
		[System.IO.Directory]::SetCurrentDirectory($wd.LocalPath)
        Set-Location $wd.LocalPath
        Write-Host -NoNewLine "WD: " 
		[Environment]::CurrentDirectory

        ##Nuget Path
        $nugetPath = New-Object System.Uri($wd,$nugetRelativePath)

		Write-Host -NoNewLine "Nuget Path: "
		$nugetPath.LocalPath

        $packExpression = '&' + '"' + $nugetPath.LocalPath + '"' + " pack " + $projFile + " -Build -IncludeReferencedProjects -Prop Configuration=" + $buildTarget 
		Write-Host -NoNewLine "Call: "
		$packExpression
		
        ##Build & pack the project
        Write-Output "Building & Packing Project"
        Invoke-Expression $packExpression
        if($LASTEXITCODE -ne 0){
                Write-Error "Build Failed."
                exit 1
        }
        else{
                Write-Output "Build Succeeded"
        }

        #Deploy package
        Write-Output "Deploying..."
        $pushExpression =  '&' + '"' + $nugetPath.LocalPath + '"' + " push *.nupkg"
        Invoke-Expression $pushExpression
        Write-Output "Deployed"

        #delete nupkg files
        Get-ChildItem –Path *.nupkg -Force | 
                foreach ($_) {remove-item $_.fullname}
}
catch
{
        Write-Host "Failed to deploy: " $_
}