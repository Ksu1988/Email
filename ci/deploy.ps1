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

function BuildLib(){
	Write-Host "Build SCCBA lib"
	New-Item -ItemType Directory -Force -Path '..\..\..\Common'
    git clone http://gitlab.stada.ru/cba/common/sccba.git $env:CI_PROJECT_DIR/sccba
    Remove-Item '..\..\..\common\*' -Force -Recurse -ErrorAction Continue
    Move-Item -Path $env:CI_PROJECT_DIR/sccba -Destination '../../../Common' -ErrorAction Continue -Verbose -Force
    
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
[string] $DestinationFolder = $arg[1]
[string] $ApplicationName = $arg[2]

write-host "This script is a file in the repository that is called by .gitlab-ci.yml"
write-host "Running in project $env:CI_PROJECT_NAME with results at $env:CI_JOB_URL ($env:CI_JOB_URL)."

StopService -ApplicationName $ApplicationName
BuildLib
Deploy -SourceFolder $SourceFolder -DestinationFolder $DestinationFolder
StartService -ApplicationName $ApplicationName
