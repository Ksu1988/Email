function CheckIsNullOrEmptyString([string] $Value, [string] $ArgName){
	if ( [string]::IsNullOrEmpty($value)){
		throw "`$$ArgName is null or empty"
	} 
}

function TryManyWithTimeout([string] $Command,[int] $Attempts, [int] $TimeoutMs=500)
{
    $status = $false;
    for($i=1; $i -le $Attempts; $i++)
    {
        Write-Host "Attemtp $i : $Command"
        try {
            Invoke-Expression $Command;
            
            Write-Host "Successed";
            $status = $true;
            break;
        }
        catch {
            
        }
        Write-Host "Execution for '$Command' failed";
        Start-Sleep -Milliseconds $TimeoutMs;
    }

    return $status;
}
