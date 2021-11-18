function BuildDependencies {
  . ./_helpers.ps1

  BuildUninstallBat

  DisableOutOfProcBuild

  BuildExe -Platform x64 -Mod Normal
}