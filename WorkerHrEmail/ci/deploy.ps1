$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module $ScriptDir\functions.psm1

function StopService([string] $ApplicationName){
	CheckIsNullOrEmptyString -Value $ApplicationName -ArgName 'ApplicationName'

	Write-Host "Stop $ApplicationName service"
	$stopCommand = "Stop-Service -Name $ApplicationName -PassThru"
	$tryStatus = TryManyWithTimeout -Command $stopCommand -Attempts 3
	if( -not $tryStatus ){
		throw 'Stop service failed';
	}
}

function Deploy([string] $SourceFolder, [string] $DestinationFolder){
	CheckIsNullOrEmptyString -Value $SourceFolder -ArgName 'SourceFolder'
	CheckIsNullOrEmptyString -Value $DestinationFolder -ArgName 'DestinationFolder'

	if( $SourceFolder -eq $DestinationFolder){
		throw '$SourceFolder and $DestinationFolder are equal';
	}

	Write-Host "Remove old items from $DestinationFolder\*"
	Remove-Item -Path "$DestinationFolder\*" -Force -Verbose -Recurse

	Write-Host "Copy built items from $SourceFolder\* to $DestinationFolder\*"
	Copy-Item -Path "$SourceFolder\*" -Destination $DestinationFolder -Force -Verbose -Recurse
}

function StartService([string] $ApplicationName){
	CheckIsNullOrEmptyString -Value $ApplicationName -ArgName 'ApplicationName'

	Write-Host "Start $ApplicationName pool"
	$startCommand = "Start-Service -Name $ApplicationName -PassThru"
	$tryStatus = TryManyWithTimeout -Command $startCommand -Attempts 3
	if( -not $tryStatus ){
		throw 'Start service failed';
	}
}

[string] $SourceFolder = $args[0]
[string] $DestinationFolder = $args[1]
[string] $ApplicationName = $args[2]

write-host "This script is a file in the repository that is called by .gitlab-ci.yml"
write-host "Running in project $env:CI_PROJECT_NAME with results at $env:CI_JOB_URL ($env:CI_JOB_URL)."

StopService -ApplicationName $ApplicationName
Deploy -SourceFolder $SourceFolder -DestinationFolder $DestinationFolder
StartService -ApplicationName $ApplicationName
