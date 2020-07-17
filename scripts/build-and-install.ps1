function BuildAndInstall {
  . .\build-installer.ps1 BuildInstaller

  . .\run-installer.ps1 RunInstaller
}



BuildAndInstall;