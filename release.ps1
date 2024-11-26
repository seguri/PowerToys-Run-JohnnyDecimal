# Read "Version" from plugin.json
$pluginJson = Get-Content -Raw plugin.json | ConvertFrom-Json
$version = $pluginJson.Version

$sourceDir = "bin\x64\Release\net8.0-windows"
$destinationDir = "bin\x64\Release\JohnnyDecimal"
$zipPath = "Community.PowerToys.Run.Plugin.JohnnyDecimal-$version.zip"

try {
	Remove-Item -Path $destinationDir -Recurse -Force
} catch {}
try {
	Remove-Item -Path $zipPath -Force
} catch {}
Copy-Item -Path $sourceDir -Destination $destinationDir -Recurse -Force
Compress-Archive -Path $destinationDir -DestinationPath $zipPath -Force