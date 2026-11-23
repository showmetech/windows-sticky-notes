param(
 [Parameter(Mandatory=$true)][string]$Executable,
 [Parameter(Mandatory=$true)][string]$FixtureRoot,
 [Parameter(Mandatory=$true)][string]$BackupDirectory,
 [string]$HistoryRoot='HKCU:\Control Panel\NotifyIconSettings'
)
$ErrorActionPreference='Stop'
$productionGuid='c2f81dae-61aa-46ce-a2a8-2dc643296760'
if($HistoryRoot -ne 'HKCU:\Control Panel\NotifyIconSettings' -and $HistoryRoot -notmatch '^HKCU:\\Software\\WindowsStickyNotesCleanupTests\\[a-f0-9]{32}$'){throw 'Unexpected history root'}
$Executable=[IO.Path]::GetFullPath($Executable)
$FixtureRoot=[IO.Path]::GetFullPath($FixtureRoot)
if([IO.Path]::GetFileName($Executable) -ne 'WindowsStickyNotes.exe'){throw 'Unexpected test executable'}
$sha=[Security.Cryptography.SHA256]::Create()
try{$hash=$sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($FixtureRoot.ToUpperInvariant()))}finally{$sha.Dispose()}
$first=New-Object byte[] 16;[Array]::Copy($hash,$first,16)
$guid=(New-Object Guid (,$first)).ToString()
if($guid -eq $productionGuid){throw 'Production identity cannot be cleaned by this script'}
function Assert-TestExited {
 foreach($process in @(Get-CimInstance Win32_Process -Filter "name='WindowsStickyNotes.exe'")){
  if(!$process.CommandLine){throw 'Unable to establish process ownership'}
  if($process.CommandLine.IndexOf($FixtureRoot,[StringComparison]::OrdinalIgnoreCase) -ge 0){throw 'The test profile is still running'}
 }
}
function Resolve-CachedExecutable([string]$path){
 if([string]::IsNullOrWhiteSpace($path)){return $null}
 $path=[Environment]::ExpandEnvironmentVariables($path)
 foreach($pair in @(@('{6D809377-6AF0-444B-8957-A3773F02200E}','ProgramFiles'),@('{F1B32785-6FBA-4FCF-9D55-7B8E7F157091}','LocalApplicationData'))){
  if($path.StartsWith($pair[0],[StringComparison]::OrdinalIgnoreCase)){$path=Join-Path ([Environment]::GetFolderPath($pair[1])) $path.Substring($pair[0].Length).TrimStart('\');break}
 }
 if($path.StartsWith('{')){return $null}
 try{return [IO.Path]::GetFullPath($path)}catch{return $null}
}
Assert-TestExited
$removed=0
if(Test-Path -LiteralPath $HistoryRoot){
 foreach($entry in @(Get-ChildItem -LiteralPath $HistoryRoot)){
  if($entry.PSChildName -notmatch '^\d+$'){continue}
  $item=Get-ItemProperty -LiteralPath $entry.PSPath
  if(!$item.IconGuid -or $item.IconGuid.Trim('{}') -ne $guid -or (Resolve-CachedExecutable $item.ExecutablePath) -ne $Executable){continue}
  Assert-TestExited
  $fresh=Get-ItemProperty -LiteralPath $entry.PSPath
  if($fresh.IconGuid -ne $item.IconGuid -or $fresh.ExecutablePath -ne $item.ExecutablePath){throw 'Tray entry changed concurrently'}
  New-Item -ItemType Directory -Path $BackupDirectory -Force|Out-Null
  $backup=Join-Path $BackupDirectory ('fixture-tray-'+$guid+'-'+$entry.PSChildName+'.json')
  if(Test-Path -LiteralPath $backup){throw 'Preserve the existing cleanup backup'}
  $values=@($entry.GetValueNames()|ForEach-Object{@{name=$_;kind=$entry.GetValueKind($_).ToString();value=$entry.GetValue($_)}})
  @{key=$entry.Name;fixtureGuid=$guid;values=$values}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $backup -Encoding UTF8
  $saved=Get-Content -LiteralPath $backup -Raw|ConvertFrom-Json
  if($saved.fixtureGuid -ne $guid -or !($saved.values|Where-Object {$_.name -eq 'IconGuid' -and $_.value -eq $fresh.IconGuid})){throw 'Cleanup backup not verified'}
  Remove-Item -LiteralPath $entry.PSPath
  if(Test-Path -LiteralPath $entry.PSPath){throw 'Test tray history remains'}
  $removed++
 }
}
if($removed -gt 0){
 Start-Sleep -Milliseconds 300
 foreach($entry in @(Get-ChildItem -LiteralPath $HistoryRoot)){
  $item=Get-ItemProperty -LiteralPath $entry.PSPath
  if($item.IconGuid -and $item.IconGuid.Trim('{}') -eq $guid -and (Resolve-CachedExecutable $item.ExecutablePath) -eq $Executable){throw 'Windows recreated test history; preserve the backup and investigate before retrying'}
 }
}
@{fixtureGuid=$guid;removed=$removed;productionIdentityPreserved=$true}|ConvertTo-Json
