$ErrorActionPreference = 'Stop'
$target = Join-Path $env:LOCALAPPDATA 'Programs\WindowsStickyNotes'
if ([IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\') -ne [IO.Path]::GetFullPath($target).TrimEnd('\')) { throw 'Execute a cópia do desinstalador que fica na pasta instalada do aplicativo.' }
$exe = Join-Path $target 'WindowsStickyNotes.exe'
if (Test-Path -LiteralPath $exe) {
 Start-Process -FilePath $exe -ArgumentList '--quit' -WindowStyle Hidden -Wait
 $running = Get-Process WindowsStickyNotes -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
 foreach ($process in $running) { Wait-Process -Id $process.Id -Timeout 12 -ErrorAction Stop }
}
$run='HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
if((Get-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -ErrorAction SilentlyContinue).WindowsStickyNotes -eq ('"'+$exe+'" --startup')){Remove-ItemProperty -LiteralPath $run -Name WindowsStickyNotes}
$startPath=Join-Path ([Environment]::GetFolderPath('Programs')) 'Windows Sticky Notes.lnk'
if(Test-Path -LiteralPath $startPath){$start=(New-Object -ComObject WScript.Shell).CreateShortcut($startPath);if($start.TargetPath -eq $exe){Remove-Item -LiteralPath $startPath}}
$desktopPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Windows Sticky Notes.lnk'
if (Test-Path -LiteralPath $desktopPath) {
 $desktop = (New-Object -ComObject WScript.Shell).CreateShortcut($desktopPath)
 if ($desktop.TargetPath -eq $exe) { Remove-Item -LiteralPath $desktopPath }
}
$registration='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\WindowsStickyNotes'
if((Get-ItemProperty -LiteralPath $registration -ErrorAction SilentlyContinue).InstallLocation -eq $target){Remove-Item -LiteralPath $registration}
# Remove only this installed executable's stable tray identity; preserve its snapshot in backup.
$history='HKCU:\Control Panel\NotifyIconSettings'
if(Test-Path -LiteralPath $history){
 if(!('StickyKnownFolder' -as [type])){Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public static class StickyKnownFolder{[DllImport("shell32.dll")]static extern int SHGetKnownFolderPath(ref Guid g,uint f,IntPtr t,out IntPtr p);public static string Resolve(string s){if(String.IsNullOrEmpty(s)||!s.StartsWith("{"))return Environment.ExpandEnvironmentVariables(s??"");int end=s.IndexOf("}");Guid g;if(end<0||!Guid.TryParse(s.Substring(0,end+1),out g))return s;IntPtr p=IntPtr.Zero;try{if(SHGetKnownFolderPath(ref g,0,IntPtr.Zero,out p)!=0)return s;return System.IO.Path.Combine(Marshal.PtrToStringUni(p),s.Substring(end+1).TrimStart((char)92));}finally{if(p!=IntPtr.Zero)Marshal.FreeCoTaskMem(p);}}}'}
 foreach($entry in Get-ChildItem -LiteralPath $history){
  $item=Get-ItemProperty -LiteralPath $entry.PSPath
  if($item.IconGuid -and $item.IconGuid.Trim('{}') -eq 'c2f81dae-61aa-46ce-a2a8-2dc643296760' -and [StickyKnownFolder]::Resolve($item.ExecutablePath) -eq $exe){
   $saved=Join-Path $env:LOCALAPPDATA ('Bruno\WindowsStickyNotes-backups\tray-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+$entry.PSChildName+'.json')
   New-Item -ItemType Directory -Path (Split-Path $saved) -Force|Out-Null
   $values=@($entry.GetValueNames()|ForEach-Object{@{name=$_;kind=$entry.GetValueKind($_).ToString();value=$entry.GetValue($_)}})
   @{key=$entry.Name;values=$values}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $saved -Encoding utf8
   if(!(Test-Path -LiteralPath $saved)){throw 'Backup da bandeja não confirmado.'}
   Remove-Item -LiteralPath $entry.PSPath
  }
 }
}
foreach ($relative in @('Assets\Notes.ico','Assets\google-oauth.json','WindowsStickyNotes.exe','WindowsStickyNotes.pdb','build-info.json','Uninstall.ps1')) {
 $file = Join-Path $target $relative
 if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file -Force }
}
Get-ChildItem -LiteralPath (Join-Path $target 'Assets') -Filter 'Notes-*.ico' -File -ErrorAction SilentlyContinue | Where-Object Name -Match '^Notes-\d+\.\d+\.\d+\.ico$' | ForEach-Object { Remove-Item -LiteralPath $_.FullName }
foreach ($folder in @((Join-Path $target 'Assets'),$target)) {
 if ((Test-Path -LiteralPath $folder) -and !(Get-ChildItem -LiteralPath $folder -Force)) { Remove-Item -LiteralPath $folder }
}
Write-Output 'Aplicativo removido. Suas notas e imagens foram preservadas em LocalAppData\Bruno\WindowsStickyNotes.'
