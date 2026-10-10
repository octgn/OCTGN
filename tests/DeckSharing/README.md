# DeckSharing tests

NUnit 3 tests for `DeckExtensionMethods.Share` (`octgnFX/Octgn/Extentions/DeckExtensionMethods.cs`), built as a .NET Framework 4.8 library.

## What is real and what is a double

- Real, linked from the repo: `DeckExtensionMethods.cs`, `UserMessageException.cs`, `SharedDeckUploadResult.cs`.
- Doubles (`Doubles.cs`): `IDeck`, `Game`, `Save`, `GameManager`, `ApiClient`, `Prefs`, `Program`, log4net.

## Prerequisites

- .NET SDK and the .NET Framework 4.8 targeting pack (reference assemblies are not pulled from NuGet).
- `NUnit.3.13.3` in the packages folder (default `../../packages`, relative to this folder).
- `nunit3-console` (NUnit Console Runner 3.x) on `PATH`.

## Restore, build, run (from the repo root, pwsh)

```pwsh
dotnet restore tests/DeckSharing/DeckSharing.Tests.csproj
dotnet build tests/DeckSharing/DeckSharing.Tests.csproj -c Release --no-restore
nunit3-console tests/DeckSharing/bin/Release/net48/DeckSharing.Tests.dll
```

With an absolute packages folder (override of `PackageRoot`):

```pwsh
$pkgs = 'F:/Source/octgn/packages'   # absolute path to the folder containing NUnit.3.13.3
dotnet build tests/DeckSharing/DeckSharing.Tests.csproj -c Release --no-restore "-p:PackageRoot=$pkgs"
nunit3-console tests/DeckSharing/bin/Release/net48/DeckSharing.Tests.dll
```

## Baseline rerun

Point `ShareSource` at the pre-fix copy of `DeckExtensionMethods.cs`; the temp-file cleanup and logging tests should fail there. `TearDown` deletes only the temp paths captured from `Save`, so a leaking baseline does not leave files behind.

```pwsh
$baseline = 'C:/path/to/baseline/DeckExtensionMethods.cs'
dotnet build tests/DeckSharing/DeckSharing.Tests.csproj -c Release --no-restore "-p:ShareSource=$baseline"
nunit3-console tests/DeckSharing/bin/Release/net48/DeckSharing.Tests.dll
```

## Caveats

- The dependencies are doubles, so these tests check the logic of `Share` only.
- `Share` currently has no callers in the repository, so no call site is exercised.
- There is no validation against the full WPF client or the real server.
- The cleanup-failure test locks the captured temp file with `FileShare.None`, then closes the lock and deletes only that file in `finally`. Temp environment variables and directories are never touched.
