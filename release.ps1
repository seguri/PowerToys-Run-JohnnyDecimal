# Read "Version" from plugin.json
$pluginJson = Get-Content -Raw plugin.json | ConvertFrom-Json
$version = $pluginJson.Version

$sourceDir = "bin\x64\Release\net8.0-windows"
$destinationDir = "bin\x64\Release\JohnnyDecimal"
$zipPath = "Community.PowerToys.Run.Plugin.JohnnyDecimal-$version.zip"

$excludedFiles = @(
	"PowerToys.Common.UI.dll"
	"PowerToys.ManagedCommon.dll"
	"PowerToys.Settings.UI.Lib.dll"
	"Wox.Infrastructure.dll"
	"Wox.Plugin.dll"
)

Remove-Item -ErrorAction SilentlyContinue -Path $destinationDir -Recurse -Force
Remove-Item -ErrorAction SilentlyContinue -Path $zipPath -Force
Copy-Item -Path $sourceDir -Destination $destinationDir -Recurse -Force
foreach ($file in $excludedFiles) {
	Remove-Item -ErrorAction SilentlyContinue -Path "$destinationDir\$file" -Force
}
Compress-Archive -Path $destinationDir -DestinationPath $zipPath -Force