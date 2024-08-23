function StartAhkScript {
  # Set the directory paths:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $rootDir = Split-Path -Path $installerDir -Parent # src
  # Set the AHK script file path:
  $ahkSourceFile = $rootDir + "\SwitchApps.ahk" # src\SwitchApps.ahk
  # Run the AHK script file:
  start $ahkSourceFile
}

StartAhkScript