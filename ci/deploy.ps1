$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path
Import-Module $ScriptDir\functions.psm1

function StopService([string] $ApplicationName){
	CheckIsNullOrEmptyString -Value $ApplicationName -ArgName 'ApplicationName'

	Write-Host "Stop $ApplicationName service"
	$stopCommand = "Get-Service -$ApplicationName -ErrorAction Ignore | Stop-Service"
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
    git clone http://gitlab.stada.ru/cba/common/sccba.git ${CI_PROJECT_DIR}/sccba
    Remove-Item '..\..\..\common\*' -Force -Recurse -ErrorAction Continue
    Move-Item -Path ${CI_PROJECT_DIR}/sccba -Destination '../../../Common' -ErrorAction Continue -Verbose -Force
    
}

function StartService([string] $ApplicationName){
	CheckIsNullOrEmptyString -Value $ApplicationName -ArgName 'ApplicationName'

	Write-Host "Start $ApplicationPoolName pool"
	$startCommand = "Get-Service $ApplicationName | Start-Service"
	$tryStatus = TryManyWithTimeout -Command $startCommand -Attempts 3
	if( -not $tryStatus ){
		throw 'Start service failed';
	}
}

[string] $SourceFolder = $args[0]
[string] $DestinationFolder = 'C:\_WORKERS\WorkerHrEmail'
[string] $ApplicationName = 'WorkerHrEmail'

# StopPool -ApplicationPoolName $ApplicationPoolName
# BuildLib
# Deploy -SourceFolder $SourceFolder -DestinationFolder $DestinationFolder
# StartPool -ApplicationPoolName $ApplicationPoolName
Write-Host "Running on $(Environment::MachineName ..."
Write-Host $CI_BUILDS_DIR
Write-Host $CI_JOB_NAME
Write-Host $CI_PROJECT_DIR
Write-Host $CI_PROJECT_PATH