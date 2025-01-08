# Read "ExecuteName" and "Version" from plugin.json
$pluginJson = Get-Content -Raw plugin.json | ConvertFrom-Json
$baseName = [System.IO.Path]::GetFileNameWithoutExtension($pluginJson.ExecuteFilename)
$version = $pluginJson.Version
$platforms = @("x64", "arm64")
$excludedFiles = @(
	"PowerToys.Common.UI.dll"
	"PowerToys.Common.UI.pdb"
	"PowerToys.ManagedCommon.dll"
	"PowerToys.ManagedCommon.pdb"
	"PowerToys.Settings.UI.Lib.dll"
	"PowerToys.Settings.UI.Lib.pdb"
	"Wox.Infrastructure.dll"
	"Wox.Infrastructure.pdb"
	"Wox.Plugin.dll"
	"Wox.Plugin.pdb"
)

foreach ($platform in $platforms) {
	$sourceDir = "bin\$platform\Release\net8.0-windows"
	$destinationDir = "bin\$platform\Release\JohnnyDecimal"
	$zipPath = "$baseName-$version-$platform.zip"
	Remove-Item -ErrorAction SilentlyContinue -Path $destinationDir -Recurse -Force
	Remove-Item -ErrorAction SilentlyContinue -Path $zipPath -Force
	Copy-Item -Path $sourceDir -Destination $destinationDir -Recurse -Force
	Get-ChildItem -Path $destinationDir -Recurse -Include $excludedFiles | Remove-Item -Force
	Compress-Archive -Path $destinationDir -DestinationPath $zipPath -Force
}
