$ErrorActionPreference = 'Stop'
$target = Join-Path $env:LOCALAPPDATA 'Programs\WindowsStickyNotes'
if ([IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\') -ne [IO.Path]::GetFullPath($target).TrimEnd('\')) { throw 'Execute a cópia do desinstalador que fica na pasta instalada do aplicativo.' }
$exe = Join-Path $target 'WindowsStickyNotes.exe'
if (Test-Path -LiteralPath $exe) {
 Start-Process -FilePath $exe -ArgumentList '--quit' -WindowStyle Hidden -Wait
 $running = Get-Process WindowsStickyNotes -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
 foreach ($process in $running) { Wait-Process -Id $process.Id -Timeout 12 -ErrorAction Stop }
}
Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name WindowsStickyNotes -ErrorAction SilentlyContinue
Remove-Item -LiteralPath (Join-Path ([Environment]::GetFolderPath('Programs')) 'Windows Sticky Notes.lnk') -ErrorAction SilentlyContinue
$desktopPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Windows Sticky Notes.lnk'
if (Test-Path -LiteralPath $desktopPath) {
 $desktop = (New-Object -ComObject WScript.Shell).CreateShortcut($desktopPath)
 if ($desktop.TargetPath -eq $exe) { Remove-Item -LiteralPath $desktopPath }
}
Remove-Item -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\WindowsStickyNotes' -ErrorAction SilentlyContinue
foreach ($relative in @('Assets\Notes.ico','WindowsStickyNotes.exe','WindowsStickyNotes.pdb','build-info.json','Uninstall.ps1')) {
 $file = Join-Path $target $relative
 if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file -Force }
}
Get-ChildItem -LiteralPath (Join-Path $target 'Assets') -Filter 'Notes-*.ico' -File -ErrorAction SilentlyContinue | Where-Object Name -Match '^Notes-\d+\.\d+\.\d+\.ico$' | ForEach-Object { Remove-Item -LiteralPath $_.FullName }
foreach ($folder in @((Join-Path $target 'Assets'),$target)) {
 if ((Test-Path -LiteralPath $folder) -and !(Get-ChildItem -LiteralPath $folder -Force)) { Remove-Item -LiteralPath $folder }
}
Write-Output 'Aplicativo removido. Suas notas e imagens foram preservadas em LocalAppData\Bruno\WindowsStickyNotes.'
