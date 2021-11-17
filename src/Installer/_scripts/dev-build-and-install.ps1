function BuildAndInstall {
  . .\_helpers.ps1; BuildInstaller

  . .\_helpers.ps1; RunInstaller
}



BuildAndInstall;