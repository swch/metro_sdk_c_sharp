<# --- Do not modify this file. Changes will be lost as this file is refreshed from the Alkami.MicroServices.Installer nuget packages every build ---  #>

function Get-ScriptDirectory
{
    return (Split-Path -Parent $PSCommandPath)    
}

$scriptDirectory = Get-ScriptDirectory;
$libraryScriptPath = Join-Path $scriptDirectory "\library.ps1";
. $libraryScriptPath 

InstallMicroService "\Alkami.MicroServices.Choco.Installer.Logic";

