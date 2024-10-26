param (
    [switch]$NoArchive,
    [switch]$UMM,
    [string]$OutputDirectory = $PSScriptRoot
)

Set-Location "$PSScriptRoot"
if ($UMM) {
    $FilesToInclude = "build/UMM/*","LICENSE","Info.json"
} else {
    $FilesToInclude = "build/BepInEx/*","LICENSE"
}

$modInfo = Get-Content -Raw -Path "Info.json" | ConvertFrom-Json
$modId = $modInfo.Id
$modVersion = $modInfo.Version

$DistDir = "$OutputDirectory/dist"
if ($NoArchive) {
    $ZipWorkDir = "$OutputDirectory"
} else {
    $ZipWorkDir = "$DistDir/tmp"
    Remove-Item "$ZipWorkDir" -Force -Recurse
}
$ZipOutDir = "$ZipWorkDir/$modId"

New-Item "$ZipOutDir" -ItemType Directory -Force
Copy-Item -Force -Path $FilesToInclude -Destination "$ZipOutDir"

if (!$NoArchive)
{
    if ($UMM) {
        $FILE_NAME = "$DistDir/${modId}_v$modVersion-UMM.zip"
    } else {
        $FILE_NAME = "$DistDir/${modId}_v$modVersion-BepInEx.zip"
    }
    Compress-Archive -Update -CompressionLevel Fastest -Path "$ZipOutDir/*" -DestinationPath "$FILE_NAME"
}