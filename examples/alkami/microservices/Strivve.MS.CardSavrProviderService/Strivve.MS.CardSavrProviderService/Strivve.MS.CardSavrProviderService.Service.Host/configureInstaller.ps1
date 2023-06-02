<# --- Do not modify this file. Changes will be lost as this file is refreshed from the Alkami.MicroServices.Installer nuget packages every build ---  #>

param($solutionDir, $projectDir, $projectPath)

Write-host "SolutionDir - $solutionDir"
Write-host "ProjectDir - $projectDir"
Write-host "ProjectPath - $projectPath"
Write-host "Find nuget package setup script"

$Path = Join-Path $projectDir.Trim() "packages.config"
$xml = [xml](Get-Content $Path)
$version = $xml.packages.package | ? {$_.id -eq "Alkami.MicroServices.Installer.Logic"} | Select-Object version
$packageDir = Join-Path $solutionDir.Trim() "packages\Alkami.MicroServices.Installer.Logic.$($version.version)"
$setupScript = Join-Path $packageDir.Trim() "tools\setup.ps1"
Write-host "Execute setup.ps1 from nuget package"

Powershell -File "$setupScript"  "$packageDir" "$projectPath"