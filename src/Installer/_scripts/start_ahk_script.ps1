function StartAhkScript {
  $scriptsDir = (Get-Location).Path # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $rootDir = Split-Path -Path $installerDir -Parent # src
  
  $ahkSourceFile = $rootDir + "\SwitchApps.ahk" # src\SwitchApps.ahk

  start $ahkSourceFile
}