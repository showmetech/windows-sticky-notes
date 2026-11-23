param([switch]$NoLaunch,[switch]$DesktopShortcut)
$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'app'
$target=[IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'Programs\WindowsStickyNotes'))
$exe=Join-Path $target 'WindowsStickyNotes.exe'
$run='HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$uninstall='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\WindowsStickyNotes'
$buildInfo=Get-Content -LiteralPath (Join-Path $source 'build-info.json') -Raw | ConvertFrom-Json
$osArch=[System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
if($osArch -notin @('X64','Arm64')){throw 'Arquitetura não suportada. Use Windows 11 x64 ou ARM64.'}
if($buildInfo.architecture -ne $osArch.ToLowerInvariant()){throw ('Use o pacote '+$osArch+' neste computador.')}
if([Environment]::OSVersion.Version.Build -lt 22000){throw 'Este aplicativo requer Windows 11.'}
if($buildInfo.version -notmatch '^\d+\.\d+\.\d+$'){throw 'Versão inválida no pacote.'}
$iconRelative='Assets\Notes-'+$buildInfo.version+'.ico'
foreach($name in @('WindowsStickyNotes.exe','Assets\Notes.ico',$iconRelative)) {if(!(Test-Path -LiteralPath (Join-Path $source $name) -PathType Leaf)){throw 'Pacote incompleto. Extraia o ZIP inteiro antes de instalar.'}}
$shell=New-Object -ComObject WScript.Shell
$shortcutPath=Join-Path ([Environment]::GetFolderPath('Programs')) 'Windows Sticky Notes.lnk'
$desktopPath=Join-Path ([Environment]::GetFolderPath('Desktop')) 'Windows Sticky Notes.lnk'
$links=@($shortcutPath);if($DesktopShortcut -or (Test-Path -LiteralPath $desktopPath)){$links+=$desktopPath}
foreach($link in $links){if((Test-Path -LiteralPath $link) -and $shell.CreateShortcut($link).TargetPath -ne $exe){throw ('Atalho de outro aplicativo preservado: '+$link)}}
if((Test-Path -LiteralPath $uninstall) -and (Get-ItemProperty -LiteralPath $uninstall).InstallLocation -ne $target){throw 'Registro de instalação de outro aplicativo preservado.'}
$hadInstallation=Test-Path -LiteralPath $exe
$oldRun=(Get-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -ErrorAction SilentlyContinue).WindowsStickyNotes
$startup=if($hadInstallation){$oldRun -eq ('"'+$exe+'" --startup')}else{$true}
if($oldRun -and $oldRun -ne ('"'+$exe+'" --startup')){throw 'Entrada de inicialização diferente preservada. Confira a instalação anterior.'}
$ticket=[Guid]::NewGuid().ToString('N')
$backupRoot=[IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'Bruno\WindowsStickyNotes-backups'))
$backup=Join-Path $backupRoot ((Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+$ticket)
$stage=$target+'.staging-'+$ticket
if([IO.Path]::GetDirectoryName($stage) -ne [IO.Path]::GetDirectoryName($target) -or [IO.Path]::GetFileName($stage) -notmatch '^WindowsStickyNotes\.staging-[a-f0-9]{32}$'){throw 'Pasta temporária inválida.'}
New-Item -ItemType Directory -Path $backup -Force|Out-Null
$oldRegistration=if(Test-Path -LiteralPath $uninstall){Get-ItemProperty -LiteralPath $uninstall}else{$null}
$linkBackups=@{}
$linksWritten=@()
$movedOld=$false;$activated=$false
try{
 New-Item -ItemType Directory -Path $stage|Out-Null
 foreach($link in $links){if(Test-Path -LiteralPath $link){$saved=Join-Path $backup ('shortcut-'+$linkBackups.Count+'.lnk');Copy-Item -LiteralPath $link -Destination $saved;$linkBackups[$link]=$saved}}
 Copy-Item -Path (Join-Path $source '*') -Destination $stage -Recurse -Force
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.ps1') -Destination (Join-Path $stage 'Uninstall.ps1')
 foreach($file in Get-ChildItem -LiteralPath $source -File -Recurse){$relative=$file.FullName.Substring($source.Length).TrimStart('\');if((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath (Join-Path $stage $relative)).Hash){throw 'Falha ao verificar o pacote preparado.'}}
 if($hadInstallation){Start-Process -FilePath $exe -ArgumentList '--quit' -WindowStyle Hidden -Wait;foreach($process in @(Get-Process WindowsStickyNotes -ErrorAction SilentlyContinue|Where-Object Path -eq $exe)){Wait-Process -Id $process.Id -Timeout 15 -ErrorAction Stop}}
 if(Test-Path -LiteralPath $target){Move-Item -LiteralPath $target -Destination (Join-Path $backup 'app');$movedOld=$true}
 Move-Item -LiteralPath $stage -Destination $target;$activated=$true
 New-Item -Path $run -Force|Out-Null
 if($startup){Set-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -Value ('"'+$exe+'" --startup')}else{Remove-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -ErrorAction SilentlyContinue}
 $icon=Join-Path $target $iconRelative
 foreach($link in $links){$linksWritten+=$link;$shortcut=$shell.CreateShortcut($link);$shortcut.TargetPath=$exe;$shortcut.WorkingDirectory=$target;$shortcut.IconLocation=$icon+',0';$shortcut.Description='Notas coloridas na área de trabalho. Acesso pela bandeja do Windows.';$shortcut.Save()}
 New-Item -Path $uninstall -Force|Out-Null
 $properties=@{DisplayName='SMT Sticky Notes';DisplayVersion=$buildInfo.version;Publisher='Showmetech';InstallLocation=$target;DisplayIcon=$icon;UninstallString=('powershell.exe -NoProfile -File "'+(Join-Path $target 'Uninstall.ps1')+'"')}
 foreach($key in $properties.Keys){Set-ItemProperty -LiteralPath $uninstall -Name $key -Value $properties[$key]}
 Set-ItemProperty -LiteralPath $uninstall -Name NoModify -Value 1 -Type DWord
 Set-ItemProperty -LiteralPath $uninstall -Name NoRepair -Value 1 -Type DWord
 if(!$NoLaunch){$started=Start-Process -FilePath $exe -ArgumentList '--startup' -WindowStyle Hidden -PassThru;Start-Sleep -Milliseconds 1200;if($started.HasExited -and !(Get-Process WindowsStickyNotes -ErrorAction SilentlyContinue|Where-Object Path -eq $exe)){throw 'O aplicativo não iniciou. Restaurando instalação anterior.'}}
 Write-Output ('Instalado em: '+$target)
 Write-Output ('Inicialização automática: '+$startup+'. Dados e preferências existentes preservados.')
}catch{
 $failure=$_
 if($activated){if(Get-Process WindowsStickyNotes -ErrorAction SilentlyContinue|Where-Object Path -eq $exe){Start-Process -FilePath $exe -ArgumentList '--quit' -WindowStyle Hidden -Wait};Move-Item -LiteralPath $target -Destination (Join-Path $backup 'failed-app')}
 if($movedOld){Move-Item -LiteralPath (Join-Path $backup 'app') -Destination $target}
 if($oldRun){Set-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -Value $oldRun}else{Remove-ItemProperty -LiteralPath $run -Name WindowsStickyNotes -ErrorAction SilentlyContinue}
 foreach($link in $linksWritten){if($linkBackups.ContainsKey($link)){Copy-Item -LiteralPath $linkBackups[$link] -Destination $link -Force}elseif((Test-Path -LiteralPath $link) -and $shell.CreateShortcut($link).TargetPath -eq $exe){Remove-Item -LiteralPath $link}}
 if($oldRegistration){New-Item -Path $uninstall -Force|Out-Null;foreach($name in (Get-Item -LiteralPath $uninstall).GetValueNames()){if($oldRegistration.PSObject.Properties.Name -notcontains $name){Remove-ItemProperty -LiteralPath $uninstall -Name $name}};foreach($property in $oldRegistration.PSObject.Properties|Where-Object Name -NotLike 'PS*'){Set-ItemProperty -LiteralPath $uninstall -Name $property.Name -Value $property.Value}}elseif(Test-Path -LiteralPath $uninstall){Remove-Item -LiteralPath $uninstall}
 if($hadInstallation -and !$NoLaunch -and (Test-Path -LiteralPath $exe)){Start-Process -FilePath $exe -ArgumentList '--startup' -WindowStyle Hidden}
 throw $failure
}finally{
 if(Test-Path -LiteralPath $stage){if([IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($stage)) -eq [IO.Path]::GetDirectoryName($target) -and [IO.Path]::GetFileName($stage) -match '^WindowsStickyNotes\.staging-[a-f0-9]{32}$'){Remove-Item -LiteralPath $stage -Recurse -Force}}
}