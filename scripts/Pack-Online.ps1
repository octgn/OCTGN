#requires -Version 7.3
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [Parameter(Mandatory = $true)][ValidatePattern('^\d+\.\d+\.\d+(\.\d+)?$')][string]$Version,
    [string]$PackageSource = 'https://api.nuget.org/v3/index.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$PSNativeCommandArgumentPassing = 'Standard'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repositoryRoot 'octgnFX/Octgn.Online/Octgn.Online.csproj'
[xml]$projectXml = Get-Content -Raw -LiteralPath $project
$communicationVersion = [string]$projectXml.Project.PropertyGroup[0].ReleasedCommunicationVersion
$communicationAssemblyVersion = [Version]::Parse($communicationVersion + '.0')
if (-not [IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path (Get-Location).ProviderPath $OutputDirectory
}
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
$sourceRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'octgnFX'))
if ($outputRoot.Equals($sourceRoot, [StringComparison]::OrdinalIgnoreCase) -or
    $outputRoot.StartsWith($sourceRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'OutputDirectory must stay outside the octgnFX source tree so generated consumer files cannot enter later builds.'
}
if (Test-Path -LiteralPath $outputRoot) { throw 'OutputDirectory must be a new directory so package validation uses a fresh cache.' }
New-Item -ItemType Directory -Path $outputRoot | Out-Null
$buildRoot = Join-Path $outputRoot 'build'
$packageRoot = Join-Path $outputRoot 'packages'
$cacheRoot = Join-Path $outputRoot 'cache'
$consumerRoot = Join-Path $outputRoot 'consumer'
New-Item -ItemType Directory -Path $buildRoot, $consumerRoot | Out-Null
$intermediateRoot = (Join-Path $buildRoot 'obj') + [IO.Path]::DirectorySeparatorChar
$binaryRoot = (Join-Path $buildRoot 'bin') + [IO.Path]::DirectorySeparatorChar
$auditProperties = @()
if ($PackageSource -notmatch '^[a-z][a-z0-9+.-]*://') {
    $auditProperties = @('-p:NuGetAudit=false')
    if (-not [IO.Path]::IsPathRooted($PackageSource)) {
        $PackageSource = Join-Path (Get-Location).ProviderPath $PackageSource
    }
    $PackageSource = [IO.Path]::GetFullPath($PackageSource)
}
$sourceXml = [Security.SecurityElement]::Escape($PackageSource)
$packageRootXml = [Security.SecurityElement]::Escape($packageRoot)

function Invoke-Dotnet([string[]]$Arguments, [string]$LogName) {
    & dotnet @Arguments 2>&1 | Tee-Object -FilePath (Join-Path $outputRoot $LogName) | Write-Host
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) failed; see $LogName" }
}

$restoreConfig = Join-Path $outputRoot 'restore.config'
@"
<configuration><packageSources><clear /><add key="public" value="$sourceXml" /></packageSources></configuration>
"@ | Set-Content -LiteralPath $restoreConfig -Encoding UTF8
$properties = @(
    '-p:UseReleasedCommunication=true',
    "-p:Version=$Version", "-p:AssemblyVersion=$Version", "-p:FileVersion=$Version",
    "-p:BaseIntermediateOutputPath=$intermediateRoot", "-p:MSBuildProjectExtensionsPath=$intermediateRoot",
    "-p:BaseOutputPath=$binaryRoot", '-p:GeneratePackageOnBuild=false', '-p:UseSharedCompilation=false'
) + $auditProperties
Invoke-Dotnet (@('restore', $project, '--packages', $cacheRoot, '--configfile', $restoreConfig) + $properties) 'restore.log'
Invoke-Dotnet (@('pack', $project, '--no-restore', '-m:1', '-o', $packageRoot) + $properties) 'pack.log'

$packages = @(Get-ChildItem -LiteralPath $packageRoot -Filter 'Octgn.Online.*.nupkg')
if ($packages.Count -ne 1) { throw 'Expected exactly one Octgn.Online package.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($packages[0].FullName)
try {
    $entry = $archive.GetEntry('Octgn.Online.nuspec')
    $reader = [IO.StreamReader]::new($entry.Open())
    try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
} finally { $archive.Dispose() }
$communication = @($nuspec.package.metadata.dependencies.group.dependency | Where-Object id -eq 'Octgn.Communication')
if ($communication.Count -ne 1 -or $communication[0].version -ne $communicationVersion) {
    throw "The package must depend on published Octgn.Communication $communicationVersion, independently of the release version."
}
$packageVersion = [string]$nuspec.package.metadata.version
$nuspec.OuterXml | Set-Content -LiteralPath (Join-Path $outputRoot 'Octgn.Online.nuspec') -Encoding UTF8

# Source mapping selects the local Online candidate even if its version already
# exists on nuget.org. All dependency packages still come from the selected feed.
$consumerConfig = Join-Path $outputRoot 'consumer.config'
@"
<configuration>
  <packageSources><clear /><add key="candidate" value="$packageRootXml" /><add key="public" value="$sourceXml" /></packageSources>
  <packageSourceMapping>
    <packageSource key="candidate"><package pattern="Octgn.Online" /></packageSource>
    <packageSource key="public"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content -LiteralPath $consumerConfig -Encoding UTF8
$consumerProject = Join-Path $consumerRoot 'Consumer.csproj'
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><PackageReference Include="Octgn.Online" Version="$packageVersion" /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $consumerProject -Encoding UTF8
@'
using System;
using System.Linq;
using Newtonsoft.Json;
using Octgn.Communication;
using Octgn.Online;
using Octgn.Online.Hosting;
using Octgn.Site.Api.Models;

var expected = new Version("__COMMUNICATION_ASSEMBLY_VERSION__");
var reference = typeof(HostedGame).Assembly.GetReferencedAssemblies().Single(name => name.Name == "Octgn.Communication");
if (reference.Version != expected || typeof(User).Assembly.GetName().Version != expected)
    throw new Exception("Package metadata, compiled reference, and loaded Communication assembly must agree.");
var user = new User("test-user", "Test User");
var game = new HostedGame { Id = Guid.NewGuid(), HostUser = user, Name = "Package validation" };
var restored = HostedGame.Deserialize(HostedGame.Serialize(game));
if (restored.Id != game.Id || restored.HostUser.Id != user.Id || restored.HostUser.DisplayName != user.DisplayName)
    throw new Exception("HostedGame/Communication.User round trip failed.");
var apiUser = new ApiUser { Id = 17, UserName = "Test User" };
if (JsonConvert.DeserializeObject<ApiUser>(JsonConvert.SerializeObject(apiUser)).Id != apiUser.Id)
    throw new Exception("Shared API DTO round trip failed.");
if (!DefaultHandshaker.Validate("session", "user", "device", out _))
    throw new Exception("Handshake validation failed.");
Console.WriteLine("PASS: nuspec/compiled/loaded Communication " + expected + " agree; HostedGame/User and API DTO round trips; handshake validation.");
'@.Replace('__COMMUNICATION_ASSEMBLY_VERSION__', $communicationAssemblyVersion.ToString()) | Set-Content -LiteralPath (Join-Path $consumerRoot 'Program.cs') -Encoding UTF8
Invoke-Dotnet (@('restore', $consumerProject, '--packages', (Join-Path $outputRoot 'consumer-cache'), '--configfile', $consumerConfig) + $auditProperties) 'consumer-restore.log'
$assets = Get-Content -Raw -LiteralPath (Join-Path $consumerRoot 'obj/project.assets.json') | ConvertFrom-Json
if (@($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'project' }).Count -ne 0) {
    throw 'The downstream consumer must restore packages only, without project substitutions.'
}
Invoke-Dotnet @('run', '--project', $consumerProject, '--no-restore', '-p:UseSharedCompilation=false') 'consumer-run.log'
Write-Host "Validated local package: $($packages[0].FullName). Nothing was uploaded."
