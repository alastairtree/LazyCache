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

Try {

	# Get dependencies from nuget and compile
	Exec { dotnet restore }
	Exec { nuget restore Console.Net461 -SolutionDirectory . }
	Exec { dotnet build --configuration $config --no-restore }

	# Find each test project and run tests. upload results to AppVeyor
	Get-ChildItem .\**\*.csproj -Recurse | 
		Where-Object { $_.Name -match ".*Test.*\.csproj$"} | 
		ForEach-Object { 
		
			Exec { dotnet test $_.FullName --configuration $config --no-build --no-restore --logger:"trx;LogFileName=..\..\test-result.trx" }
	
			$testResults = (Resolve-Path .\test-result*.trx)
			# if on build server upload results to AppVeyor
			if ("${ENV:APPVEYOR_JOB_ID}" -ne "") {
				$wc = New-Object 'System.Net.WebClient'
				$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$($env:APPVEYOR_JOB_ID)", $testResults) 
			}

			Remove-Item $testResults -ErrorAction SilentlyContinue
	}

	# Publish the nupkg artifacts
	if (Get-Command "Push-AppveyorArtifact" -errorAction SilentlyContinue)
	{
		Get-ChildItem .\*\bin\$config\*.nupkg -Recurse | ForEach-Object { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
	}

} Catch {
	$host.SetShouldExit(-1)
	throw
}