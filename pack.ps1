$src = "dist\PotatoPlease-Steam-Coop-v1.0.0"
$dst = "dist\PotatoPlease-Steam-Coop-v1.0.1"

if (Test-Path $dst) { Remove-Item -Recurse -Force $dst }
Copy-Item -Recurse $src $dst

Copy-Item "dist\Installer\PotatoCoopInstaller.exe" "$dst\PotatoCoopInstaller.exe" -Force
Copy-Item "dist\Installer\Install.bat" "$dst\Install.bat" -Force
Copy-Item "Mods\PotatoCoop.dll" "$dst\Mods\PotatoCoop.dll" -Force

$pluginsDir = "$dst\PotatoPlease_Data\Plugins\x86_64"
if (-not (Test-Path $pluginsDir)) { New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null }
Copy-Item "$dst\steam_api64.dll" "$pluginsDir\steam_api64.dll" -Force

$zipDst = "$PSScriptRoot\dist\PotatoPlease-Steam-Coop-v1.0.1.zip"
if (Test-Path $zipDst) { Remove-Item -Force $zipDst }

Compress-Archive -Path "$dst\*" -DestinationPath $zipDst -Force
Write-Output "Successfully created $zipDst"
