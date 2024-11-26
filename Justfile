set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

appname := "PowerToys-Run-JohnnyDecimal"
build_dir := clean("./bin/x64/Release/net8.0-windows")
ptrun_dir := clean(env_var('LOCALAPPDATA') / "Microsoft/PowerToys/PowerToys Run")
plugin_dir := clean(ptrun_dir / "Plugins/JohnnyDecimal/")
ptrun_logfile := clean(ptrun_dir / "Logs/0.86.0.0" / datetime('%F') + ".txt")

system-info:
    @echo "This is an {{arch()}} machine,"
    @echo "With {{num_cpus()}} CPUs,"
    @echo "Running on {{os()}} ({{os_family()}})."
    @echo "The executable is at: {{just_executable()}}."
    @echo "The process ID is: {{ just_pid() }}."
    @echo "build_dir     {{ if path_exists(build_dir) == "true" { "✅" } else { "❌" } }}: {{build_dir}}"
    @echo "ptrun_dir     {{ if path_exists(ptrun_dir) == "true" { "✅" } else { "❌" } }}: {{ptrun_dir}}"
    @echo "plugin_dir    {{ if path_exists(plugin_dir) == "true" { "✅" } else { "❌" } }}: {{plugin_dir}}"
    @echo "ptrun_logfile {{ if path_exists(ptrun_logfile) == "true" { "✅" } else { "❌" } }}: {{ptrun_logfile}}"

add-remote:
    git remote add origin git@github.com:seguri/{{appname}}.git

copy:
    try { Stop-Process -ErrorAction Stop -Name "PowerToys"; Start-Sleep -Seconds 1; } catch { $true }
    Remove-Item -Path "{{plugin_dir}}" -Recurse -Force
    Copy-Item -Path "{{build_dir}}" -Destination "{{plugin_dir}}" -Recurse -Force
    Start-Sleep -Seconds 2
    Start-Process "C:\Program Files\PowerToys\PowerToys.exe"

hash:
    Get-ChildItem -Path "*.zip" | Select-Object Name, @{Name = "Hash"; Expression = { ((Get-FileHash -Path $_.FullName -Algorithm SHA256).Hash) }}

repo:
    Start-Process "https://github.com/seguri/{{appname}}"

logs:
    @echo "Log file: {{ptrun_logfile}}"
    Get-Content -Path "{{ptrun_logfile}}" -Tail 10 -Wait