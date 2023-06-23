$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module $ScriptDir\functions.psm1

[string] $ServiceName = [Environment]::GetEnvironmentVariable($args[0])

$stopCommand = "Stop-Service -Name $ServiceName;"
TryManyWithTimeout -Command $stopCommand -Attempts 3

$startCommand = "Start-Service -Name $ServiceName;"
TryManyWithTimeout -Command $startCommand -Attempts 3
