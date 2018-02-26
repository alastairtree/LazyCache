Set-StrictMode -Version latest
$ErrorActionPreference = "Stop"


# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ("Error executing command {0}" -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

$config = "release"

Exec { dotnet restore }

Exec { dotnet build --configuration $config --no-restore }

Get-ChildItem .\**\*.csproj -Recurse |  Where-Object { $_.Name -match ".*Test(s)?.csproj$"} | ForEach-Object { 
    Exec { dotnet test $_.FullName --configuration $config --no-build --no-restore }
}

if (Get-Command "Push-AppveyorArtifact" -errorAction SilentlyContinue)
{
    Get-ChildItem .\*\bin\$config\*.nupkg -Recurse | ForEach-Object { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
}