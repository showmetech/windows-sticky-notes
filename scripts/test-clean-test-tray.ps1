$ErrorActionPreference='Stop'
$ticket=[Guid]::NewGuid().ToString('N')
$root='HKCU:\Software\WindowsStickyNotesCleanupTests\'+$ticket
$files=Join-Path ([IO.Path]::GetTempPath()) ('StickyTrayCleanupTests-'+$ticket)
$exe=Join-Path ([Environment]::GetFolderPath('ProgramFiles')) 'SMT-Tests\WindowsStickyNotes.exe'
$fixture=Join-Path $files 'isolated fixture'
$sha=[Security.Cryptography.SHA256]::Create()
try{$hash=$sha.ComputeHash([Text.Encoding]::UTF8.GetBytes([IO.Path]::GetFullPath($fixture).ToUpperInvariant()))}finally{$sha.Dispose()}
$first=New-Object byte[] 16;[Array]::Copy($hash,$first,16);$guid=(New-Object Guid (,$first)).ToString()
$scriptPath=Join-Path $PSScriptRoot 'clean-test-tray.ps1'
$checks=0
function Assert([bool]$ok,[string]$name){if(!$ok){throw $name};$script:checks++}
function Entry([string]$name,[string]$id,[string]$path){$key=Join-Path $root $name;New-Item -Path $key -Force|Out-Null;Set-ItemProperty -LiteralPath $key -Name IconGuid -Value ('{'+$id+'}');Set-ItemProperty -LiteralPath $key -Name ExecutablePath -Value $path;Set-ItemProperty -LiteralPath $key -Name IconSnapshot -Value ([byte[]]@(1,2,3))}
try{
 Entry '1' $guid $exe
 Entry '2' 'c2f81dae-61aa-46ce-a2a8-2dc643296760' $exe
 Entry '3' $guid (Join-Path $files 'Foreign.exe')
 Entry '4' ([Guid]::NewGuid().ToString()) $exe
 Entry '5' $guid '{6D809377-6AF0-444B-8957-A3773F02200E}\SMT-Tests\WindowsStickyNotes.exe'
 Entry '6' $guid '{00000000-0000-0000-0000-000000000000}\WindowsStickyNotes.exe'
 function Get-CimInstance {param($Filter);[pscustomobject]@{CommandLine=$fixture}}
 try{
  $blocked=$false;try{& (Join-Path $PSScriptRoot 'clean-test-tray.ps1') -Executable $exe -FixtureRoot $fixture -BackupDirectory $files -HistoryRoot $root|Out-Null}catch{$blocked=$true}
  Assert ($blocked -and (Test-Path (Join-Path $root '1'))) 'A running test blocks cleanup before changes'
 }finally{Remove-Item function:Get-CimInstance}
 $result=(& $scriptPath -Executable $exe -FixtureRoot $fixture -BackupDirectory $files -HistoryRoot $root)|ConvertFrom-Json
 Assert ($result.removed -eq 2) 'Only both owned path forms are removed'
 Assert (!(Test-Path (Join-Path $root '1')) -and !(Test-Path (Join-Path $root '5'))) 'Direct and known-folder paths handled'
 foreach($name in @('2','3','4','6')){Assert (Test-Path (Join-Path $root $name)) ('Foreign/production entry preserved: '+$name)}
 Assert (@(Get-ChildItem -LiteralPath $files -Filter '*.json').Count -eq 2) 'Each removal has a readable backup'
 $again=(& $scriptPath -Executable $exe -FixtureRoot $fixture -BackupDirectory $files -HistoryRoot $root)|ConvertFrom-Json
 Assert ($again.removed -eq 0) 'Repeated cleanup is idempotent'
 Entry '1' $guid $exe
 $blocked=$false;try{& $scriptPath -Executable $exe -FixtureRoot $fixture -BackupDirectory $files -HistoryRoot $root|Out-Null}catch{$blocked=$true}
 Assert ($blocked -and (Test-Path (Join-Path $root '1'))) 'Existing backup is never overwritten or followed by deletion'
 $blocked=$false;try{& $scriptPath -Executable $exe -FixtureRoot $fixture -BackupDirectory $files -HistoryRoot 'HKCU:\Software\Unrelated'|Out-Null}catch{$blocked=$true}
 Assert $blocked 'Unrelated registry roots are rejected'
 $unleased=Join-Path $files 'unleased-ui'
 $blocked=$false;try{& (Join-Path $PSScriptRoot 'verify-installed-ui.ps1') -Executable $exe -EvidenceDirectory $unleased|Out-Null}catch{$blocked=$_.Exception.Message -like 'Coordinate exclusive*'}
 Assert ($blocked -and !(Test-Path -LiteralPath $unleased)) 'UI verification needs a coordinated lease before file/process actions'
 @{passed=$checks;failed=0;isolatedRegistry=$root;syntheticBackups=$files}|ConvertTo-Json
}finally{
 if($root -match '^HKCU:\\Software\\WindowsStickyNotesCleanupTests\\[a-f0-9]{32}$' -and (Test-Path -LiteralPath $root)){Remove-Item -LiteralPath $root -Recurse -Force}
}
