$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module $ScriptDir\functions.psm1

[string] $ServiceName = [Environment]::GetEnvironmentVariable($args[0])

write-host "Service name $ServiceName"

$stopCommand = "Stop-Service -Name $ServiceName -Force -PassThru;"
TryManyWithTimeout -Command $stopCommand -Attempts 2
