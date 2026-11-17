param([Parameter(Mandatory=$true)][string]$AppPath,[string]$Report)
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$ticket=[Guid]::NewGuid().ToString('N')
$root=Join-Path ([IO.Path]::GetTempPath()) ('StickyDeploymentTests-'+$ticket)
$homePath=Join-Path $root 'Usuário Teste com espaços'
$registry='HKCU:\Software\WindowsStickyNotesDeploymentTests\'+$ticket
$package=Join-Path $root 'Pacote de instalação'
$programs=Join-Path $homePath 'Menu Iniciar';$desktop=Join-Path $homePath 'Área de Trabalho'
$local=Join-Path $homePath 'Local';$target=Join-Path $local 'Programs\WindowsStickyNotes'
$checks=@();$previousLocal=$env:LOCALAPPDATA
function Check([string]$Name,[bool]$Passed){$script:checks+=@{name=$Name;passed=$Passed};if(!$Passed){throw $Name}}
try{
 foreach($path in @($package,$programs,$desktop,$local)){New-Item -ItemType Directory -Path $path -Force|Out-Null}
 Copy-Item -LiteralPath (Resolve-Path $AppPath).Path -Destination (Join-Path $package 'app') -Recurse
 foreach($name in @('Install.ps1','Uninstall.ps1')){
  $text=[IO.File]::ReadAllText((Join-Path $repo $name))
  # Only the fixture copy redirects the Windows profile folders and registry hive.
  $text=$text.Replace("[Environment]::GetFolderPath('Programs')",("'"+$programs.Replace("'","''")+"'")).Replace("[Environment]::GetFolderPath('Desktop')",("'"+$desktop.Replace("'","''")+"'")).Replace('HKCU:\',$registry+'\')
  [IO.File]::WriteAllText((Join-Path $package $name),$text,[Text.UTF8Encoding]::new($true))
 }
 $env:LOCALAPPDATA=$local
 & (Join-Path $package 'Install.ps1') -NoLaunch -DesktopShortcut|Out-Null
 $exe=Join-Path $target 'WindowsStickyNotes.exe';$run=$registry+'\Software\Microsoft\Windows\CurrentVersion\Run'
 $link=Join-Path $programs 'Windows Sticky Notes.lnk';$shell=New-Object -ComObject WScript.Shell
 Check 'Fresh install works in a Unicode profile with spaces' ((Test-Path $exe) -and $shell.CreateShortcut($link).TargetPath -eq $exe)
 Check 'Fresh install enables only its own startup entry' ((Get-ItemProperty $run).WindowsStickyNotes -eq ('"'+$exe+'" --startup'))
 $notes=Join-Path $local 'Bruno\WindowsStickyNotes';New-Item -ItemType Directory -Path $notes -Force|Out-Null
 $human=Join-Path $notes 'human-note.txt';[IO.File]::WriteAllText($human,'Dados sintéticos preservados');$noteHash=(Get-FileHash $human).Hash
 Remove-ItemProperty -LiteralPath $run -Name WindowsStickyNotes
 & (Join-Path $package 'Install.ps1') -NoLaunch -DesktopShortcut|Out-Null
 Check 'Update preserves disabled startup preference' (!(Get-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -ErrorAction SilentlyContinue).WindowsStickyNotes)
 Check 'Update preserves user data' ((Get-FileHash $human).Hash -eq $noteHash)
 Check 'Update retains one current versioned icon' (@(Get-ChildItem (Join-Path $target 'Assets') -Filter 'Notes-*.ico').Count -eq 1)
 $hash=(Get-FileHash $exe).Hash
 $normal=[IO.File]::ReadAllText((Join-Path $package 'Install.ps1'))
 $fault=$normal.Replace(' if(!$NoLaunch){',' Set-ItemProperty -LiteralPath $uninstall -Name SyntheticUnexpectedValue -Value new;throw "Synthetic activation failure";if(!$NoLaunch){')
 [IO.File]::WriteAllText((Join-Path $package 'Install.ps1'),$fault,[Text.UTF8Encoding]::new($true))
 $failed=$false;try{& (Join-Path $package 'Install.ps1') -NoLaunch|Out-Null}catch{$failed=$true}
 [IO.File]::WriteAllText((Join-Path $package 'Install.ps1'),$normal,[Text.UTF8Encoding]::new($true))
 Check 'Failed activation restores previous installation and preferences' ($failed -and (Get-FileHash $exe).Hash -eq $hash -and !(Get-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -ErrorAction SilentlyContinue).WindowsStickyNotes)
 Check 'Rollback removes registry values introduced by failed update' (!(Get-ItemProperty -LiteralPath ($registry+'\Software\Microsoft\Windows\CurrentVersion\Uninstall\WindowsStickyNotes') -Name SyntheticUnexpectedValue -ErrorAction SilentlyContinue).SyntheticUnexpectedValue)
 Check 'No abandoned staging folders remain' (@(Get-ChildItem (Split-Path $target) -Directory -Filter 'WindowsStickyNotes.staging-*').Count -eq 0)
 $foreign=$shell.CreateShortcut($link);$foreign.TargetPath=Join-Path $env:WINDIR 'notepad.exe';$foreign.Save()
 $failed=$false;try{& (Join-Path $package 'Install.ps1') -NoLaunch|Out-Null}catch{$failed=$true}
 Check 'Foreign Start menu shortcut blocks update before changes' ($failed -and (Get-FileHash $exe).Hash -eq $hash -and $shell.CreateShortcut($link).TargetPath -ne $exe)
 $ownTray=$registry+'\Control Panel\NotifyIconSettings\123';$foreignTray=$registry+'\Control Panel\NotifyIconSettings\456'
 foreach($key in @($ownTray,$foreignTray)){New-Item -Path $key -Force|Out-Null;Set-ItemProperty -LiteralPath $key -Name IconGuid -Value 'c2f81dae-61aa-46ce-a2a8-2dc643296760'}
 Set-ItemProperty -LiteralPath $ownTray -Name ExecutablePath -Value $exe
 Set-ItemProperty -LiteralPath $foreignTray -Name ExecutablePath -Value (Join-Path $env:WINDIR 'notepad.exe')
 & (Join-Path $target 'Uninstall.ps1')|Out-Null
 Check 'Uninstall removes only its own tray history with a backup' (!(Test-Path -LiteralPath $ownTray) -and (Test-Path -LiteralPath $foreignTray) -and @(Get-ChildItem (Join-Path $local 'Bruno\WindowsStickyNotes-backups') -Filter 'tray-*.json').Count -eq 1)
 Check 'Uninstall removes its executable and Google app config' (!(Test-Path $exe) -and !(Test-Path (Join-Path $target 'Assets\google-oauth.json')))
 Check 'Uninstall preserves user data and foreign shortcut' ((Get-FileHash $human).Hash -eq $noteHash -and (Test-Path $link) -and $shell.CreateShortcut($link).TargetPath -ne $exe)
}finally{
 $env:LOCALAPPDATA=$previousLocal
 if($registry -match '^HKCU:\\Software\\WindowsStickyNotesDeploymentTests\\[a-f0-9]{32}$' -and (Test-Path -LiteralPath $registry)){Remove-Item -LiteralPath $registry -Recurse -Force}
 $result=@{passed=@($checks|Where-Object passed).Count;failed=@($checks|Where-Object{!$_.passed}).Count;root=$root;checks=$checks;isolatedRegistryRemoved=!(Test-Path -LiteralPath $registry)}
 if($Report){$destination=[IO.Path]::GetFullPath($Report);New-Item -ItemType Directory -Path (Split-Path $destination) -Force|Out-Null;$result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $destination -Encoding utf8}
 $result|ConvertTo-Json -Depth 5
}
