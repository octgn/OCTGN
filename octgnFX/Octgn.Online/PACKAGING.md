# Building an Online package

Normal client and server builds continue to use the pinned Communication source
project. They do not generate an Online package automatically.

An Online package must instead compile against the published
`Octgn.Communication 3.0.150` package. Setting only the nuspec dependency version
would leave a mismatched compiled assembly reference when CI stamps all projects.
Changing every local build to the package also creates duplicate unsigned
Communication assemblies for SDK consumers that reference the source project.

From the repository root, with PowerShell and .NET 8 or newer:

```powershell
./scripts/Pack-Online.ps1 -OutputDirectory ./package-check/run-001 -Version 3.4.445.0
```

Choose a new output directory for each run. The script puts its build output,
intermediate files, package cache, nupkg, logs, and downstream consumer there.
It restores the published dependency, packs only Online, checks the generated
nuspec, then restores and runs a package-only consumer with a separate fresh cache.
The consumer checks that the nuspec, compiled Communication reference, and loaded
Communication assembly agree, plus shared DTO and hosted-game serialization.
No OCTGN service is contacted and nothing is uploaded.

`-PackageSource` can select another package feed for an offline or controlled test.
The default is the public NuGet v3 feed. `-Version` defaults to the Online project's
current version; a real release must choose a new package version.

For a custom packaging pipeline, `UseReleasedCommunication=true` selects the
released dependency. Apply it only to an isolated Online package build, with
separate bin, obj, and restore cache paths, as the script does. Do not pass it to
the complete client solution. Packing without this property fails before the
nupkg is generated.

The existing release pipeline must explicitly adopt this package-building step.
Its version-stamping and upload configuration is not tracked in this repository.
This source change does not repair already published packages or publish a new one.
