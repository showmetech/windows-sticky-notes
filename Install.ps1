param([switch]$NoLaunch,[switch]$DesktopShortcut)
$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot 'app'
$target = Join-Path $env:LOCALAPPDATA 'Programs\WindowsStickyNotes'
$exe = Join-Path $target 'WindowsStickyNotes.exe'
$osArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
$buildInfo = Get-Content -Raw (Join-Path $source 'build-info.json') | ConvertFrom-Json
if ($osArch -eq 'Arm64' -and $buildInfo.architecture -ne 'arm64') { throw 'Use o pacote ARM64 neste computador.' }
if ($osArch -eq 'X64' -and $buildInfo.architecture -ne 'x64') { throw 'Use o pacote x64 neste computador.' }
if ([Environment]::OSVersion.Version.Build -lt 22000) { throw 'Este aplicativo requer Windows 11.' }
if (!(Test-Path -LiteralPath (Join-Path $source 'WindowsStickyNotes.exe'))) { throw 'O pacote está incompleto. Extraia o ZIP inteiro antes de instalar.' }
if (Test-Path -LiteralPath $exe) {
    Start-Process -FilePath $exe -ArgumentList '--quit' -WindowStyle Hidden -Wait
    $running = Get-Process WindowsStickyNotes -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
    foreach ($process in $running) { Wait-Process -Id $process.Id -Timeout 12 -ErrorAction Stop }
    $backup = $target + '.backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Copy-Item -LiteralPath $target -Destination $backup -Recurse
}
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path (Join-Path $source '*') -Destination $target -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.ps1') -Destination (Join-Path $target 'Uninstall.ps1') -Force
$run = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
New-Item -Path $run -Force | Out-Null
Set-ItemProperty -Path $run -Name 'WindowsStickyNotes' -Value ('"' + $exe + '" --startup')
$shell = New-Object -ComObject WScript.Shell
$shortcutPath = Join-Path ([Environment]::GetFolderPath('Programs')) 'Windows Sticky Notes.lnk'
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = $target
$iconPath = Join-Path $target ('Assets\Notes-'+$buildInfo.version+'.ico')
$shortcut.IconLocation = $iconPath
$shortcut.Description = 'Notas coloridas na área de trabalho. Acesso pela bandeja do Windows.'
$shortcut.Save()
$desktopPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Windows Sticky Notes.lnk'
if ($DesktopShortcut -or (Test-Path -LiteralPath $desktopPath)) {
 if ((Test-Path -LiteralPath $desktopPath) -and $shell.CreateShortcut($desktopPath).TargetPath -ne $exe) { throw 'O atalho da área de trabalho com esse nome pertence a outro aplicativo. Ele foi preservado.' }
 $desktop = $shell.CreateShortcut($desktopPath)
 $desktop.TargetPath = $exe
 $desktop.WorkingDirectory = $target
 $desktop.IconLocation = $iconPath
 $desktop.Description = $shortcut.Description
 $desktop.Save()
}
$uninstall = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\WindowsStickyNotes'
New-Item -Path $uninstall -Force | Out-Null
$properties = @{
 DisplayName='Windows Sticky Notes'; DisplayVersion=([System.Diagnostics.FileVersionInfo]::GetVersionInfo($exe).ProductVersion.Split('+')[0]); Publisher='Bruno Ayres Martinez';
 InstallLocation=$target; DisplayIcon=$iconPath;
 UninstallString=('powershell.exe -NoProfile -File "' + (Join-Path $target 'Uninstall.ps1') + '"')
}
foreach ($key in $properties.Keys) { Set-ItemProperty -Path $uninstall -Name $key -Value $properties[$key] }
Set-ItemProperty -Path $uninstall -Name NoModify -Value 1 -Type DWord
Set-ItemProperty -Path $uninstall -Name NoRepair -Value 1 -Type DWord
if (!$NoLaunch) { Start-Process -FilePath $exe -WindowStyle Hidden }
Write-Output ('Instalado em: ' + $target)
Write-Output 'Inicialização automática ativada. Notas e imagens são salvas em LocalAppData\Bruno\WindowsStickyNotes.'
