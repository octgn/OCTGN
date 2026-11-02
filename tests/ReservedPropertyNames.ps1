$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$builder = Join-Path $root 'octgnFX/o8build/bin/Release/o8build.exe'
$template = Join-Path $root 'octgnFX/Octgn.GameStudio/Resources/Templates/Basic-Game/Game Plugin'
$temp = Join-Path ([IO.Path]::GetTempPath()) ('octgn-validator-' + [guid]::NewGuid())
try {
    Copy-Item $template $temp -Recurse
    $definition = Join-Path $temp 'definition.xml'
    $original = Get-Content $definition -Raw
    [void][Reflection.Assembly]::LoadFrom($builder)
    foreach ($name in @('type', 'Type', 'TYPE', 'name', 'ID', 'Alternate', 'SIZE', 'set')) {
        $xml = $original.Replace('name="Text" type="String"', "name=`"$name`" type=`"String`"")
        [IO.File]::WriteAllText($definition, $xml)
        $errorMessage = $null
        try { ([o8build.GameValidator]::new($temp)).VerifyDefPaths() }
        catch { $errorMessage = $_.Exception.ToString() }
        if ($name -in @('type', 'Type', 'TYPE')) {
            if ($errorMessage) { throw "Expected $name to be accepted: $errorMessage" }
        } elseif ($errorMessage -notmatch "Property '$name' is a reserved name") {
            throw "Expected $name to be rejected as reserved; got: $errorMessage"
        }
        Write-Host "PASS $name"
    }
} finally {
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force }
}
