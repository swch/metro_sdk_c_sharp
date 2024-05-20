<# --- Do not modify this file. Changes will be lost as this file is refreshed from the Alkami.MicroServices.Installer nuget packages every build ---  #>

function Get-ChocoDirectory 
{
    $scriptDirectory = Get-ScriptDirectory;
    $packageRoot = Split-Path -Parent $scriptDirectory;

    return (Split-Path -Parent $packageRoot)

}

function Get-ScriptDirectory
{
    return (Split-Path -Parent $PSCommandPath)    
}

function InstallMicroService([string] $directoryName)
{
	try 
	{ 
		$scriptDirectory = Get-ScriptDirectory;
		$chocoDirectory = Get-ChocoDirectory;
		$chocoInstallDirectory = Join-Path $chocoDirectory $directoryName;
		$installScriptPath = Join-Path $chocoInstallDirectory "\tools\Install.ps1";
	
		Write-Host "Calling Alkami.MicroServices.Choco.Installer.Database Installer script from $installScriptPath...";
		. $installScriptPath $scriptDirectory
		Write-Host "Done.";

	} 
	catch 
	{
		Write-Host "The install of the service contained in [$($serviceExecutableName)] failed because of error: $($_.Exception.Message)";
		throw 
	}
}

function BeforeModifyMicroService([string] $directoryName)
{
	
	try 
	{ 
		$scriptDirectory = Get-ScriptDirectory;
		$chocoDirectory = Get-ChocoDirectory;
		$chocoInstallDirectory = Join-Path $chocoDirectory $directoryName;
		$beforeModifyScriptPath = Join-Path $chocoInstallDirectory "\tools\BeforeModify.ps1";
		
		Write-Host "Calling Alkami.MicroServices.Choco.Installer.Database BeforeModify script from $beforeModifyScriptPath...";
		. $beforeModifyScriptPath $scriptDirectory
		Write-Host "Done.";
	} 
	catch 
	{
		Write-Host "The beforeModify of the service contained in [$($serviceExecutableName)] failed because of error: $($_.Exception.Message)";
		throw 
	}
}

function UninstallMicroService([string] $directoryName)
{
	try 
	{ 
		$scriptDirectory = Get-ScriptDirectory;
		$chocoDirectory = Get-ChocoDirectory;
		$chocoInstallDirectory = Join-Path $chocoDirectory $directoryName;
		$uninstallScriptPath = Join-Path $chocoInstallDirectory "\tools\Uninstall.ps1";
		
		Write-Host "Calling Alkami.MicroServices.Choco.Installer.Database Uninstall script from $uninstallScriptPath...";
		. $uninstallScriptPath $scriptDirectory
		Write-Host "Done.";
	} 
	catch 
	{
		Write-Host "The uninstall of the service contained in [$($serviceExecutableName)] failed because of error: $($_.Exception.Message)";
		throw 
	}
}

