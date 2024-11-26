# Read "ExecuteName" and "Version" from plugin.json
$pluginJson = Get-Content -Raw plugin.json | ConvertFrom-Json
$baseName = [System.IO.Path]::GetFileNameWithoutExtension($pluginJson.ExecuteFilename)
$version = $pluginJson.Version
$platforms = @("x64", "arm64")
$excludedFiles = @(
	"PowerToys.Common.UI.dll"
	"PowerToys.ManagedCommon.dll"
	"PowerToys.Settings.UI.Lib.dll"
	"Wox.Infrastructure.dll"
	"Wox.Plugin.dll"
)

foreach ($platform in $platforms) {
	$sourceDir = "bin\$platform\Release\net8.0-windows"
	$destinationDir = "bin\$platform\Release\JohnnyDecimal"
	$zipPath = "$baseName-$version-$platform.zip"
	Remove-Item -ErrorAction SilentlyContinue -Path $destinationDir -Recurse -Force
	Remove-Item -ErrorAction SilentlyContinue -Path $zipPath -Force
	Copy-Item -Path $sourceDir -Destination $destinationDir -Recurse -Force
	foreach ($file in $excludedFiles) {
		Remove-Item -ErrorAction SilentlyContinue -Path "$destinationDir\$file" -Force
	}
	Compress-Archive -Path $destinationDir -DestinationPath $zipPath -Force
}
