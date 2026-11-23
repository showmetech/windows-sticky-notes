param(
 [Parameter(Mandatory=$true)][string]$Executable,
 [string]$EvidenceDirectory=(Join-Path ([IO.Path]::GetTempPath()) ('SMTStickyNotes-QA-'+[Guid]::NewGuid().ToString('N'))),
 [switch]$WindowLeaseConfirmed
)
$ErrorActionPreference='Stop'
if(!$WindowLeaseConfirmed){throw 'Coordinate exclusive window access before running this visual fixture'}
$Executable=(Resolve-Path -LiteralPath $Executable).Path
if([IO.Path]::GetFileName($Executable) -ne 'WindowsStickyNotes.exe'){throw 'Use the installed/released executable, never dotnet as tray identity'}
$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
$fixture=Join-Path $EvidenceDirectory 'isolated-fixture'
if(Test-Path -LiteralPath $fixture){throw 'Use a fresh isolated fixture; existing files were preserved'}
New-Item -ItemType Directory -Path $EvidenceDirectory -Force|Out-Null
$report=Join-Path $EvidenceDirectory 'live-tests.json'
$process=Start-Process -FilePath $Executable -ArgumentList @('--data-dir',('"'+$fixture+'"'),'--demo','--verify-live',('"'+$report+'"')) -WindowStyle Hidden -PassThru
if(!$process.WaitForExit(60000)){
 Start-Process -FilePath $Executable -ArgumentList @('--data-dir',('"'+$fixture+'"'),'--quit') -WindowStyle Hidden -Wait
 if(!$process.WaitForExit(15000)){throw 'Fixture remains active; no tray cleanup or forced termination was performed'}
}
# Windows can persist notification history after the tested process has exited.
Start-Sleep -Milliseconds 300
& (Join-Path $PSScriptRoot 'clean-test-tray.ps1') -Executable $Executable -FixtureRoot $fixture -BackupDirectory (Join-Path $EvidenceDirectory 'tray-backups')|Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'tray-cleanup.json') -Encoding UTF8
if(!(Test-Path -LiteralPath $report)){throw 'Visual fixture did not produce a report'}
$result=Get-Content -LiteralPath $report -Raw|ConvertFrom-Json
if($result.failed -ne 0 -or !$result.checks -or $process.ExitCode -ne 0){throw 'Visual fixture failed; inspect its report'}
@{passed=$result.checks.Count;failed=$result.failed;report=$report;scope='Isolated native window checks; no physical tray click or production account test'}|ConvertTo-Json
