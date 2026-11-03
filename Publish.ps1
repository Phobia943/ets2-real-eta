param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'artifacts'))
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'EtsRealEta.csproj'
$out = [IO.Path]::GetFullPath($OutputDirectory)
$bundle = Join-Path $out 'ETS2-Fahrzeit-v0.1.0-win-x64'
New-Item -ItemType Directory -Path $bundle -Force | Out-Null
dotnet publish $project -c Release -r win-x64 --self-contained true -p:RuntimeFrameworkVersion=10.0.9 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o $bundle
if ($LASTEXITCODE -ne 0) { throw 'Build fehlgeschlagen.' }
foreach ($name in @('README.md','LICENSE','THIRD-PARTY-NOTICES.txt','DOTNET-LICENSE.txt','DOTNET-THIRD-PARTY-NOTICES.txt','WINDOWSDESKTOP-THIRD-PARTY-NOTICES.txt')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $bundle
}
Copy-Item -LiteralPath (Join-Path $bundle 'ETS2-Fahrzeit.exe') -Destination $out
$zip = Join-Path $out 'ETS2-Fahrzeit-v0.1.0-win-x64.zip'
Compress-Archive -Path (Join-Path $bundle '*') -DestinationPath $zip -Force
$hashLines = foreach ($name in @('ETS2-Fahrzeit.exe','ETS2-Fahrzeit-v0.1.0-win-x64.zip')) {
    $hash = Get-FileHash -LiteralPath (Join-Path $out $name) -Algorithm SHA256
    '{0}  {1}' -f $hash.Hash.ToLowerInvariant(), $name
}
$hashLines | Set-Content -LiteralPath (Join-Path $out 'SHA256SUMS.txt') -Encoding ascii
Get-ChildItem -LiteralPath $out | Select-Object Name,Length
