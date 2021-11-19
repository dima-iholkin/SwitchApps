$ErrorActionPreference = "Stop"



function DebugCurrentDir {
  Get-Location

  $PSScriptRoot
}



function GetInstallerProductCode {
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $projectFile = $installerDir + "\SwitchApps_Installer\SwitchApps_Installer.vdproj" # src\Installer\SwitchApps_Installer\SwitchApps_Installer.vdproj
  
  $productCodeLines = Select-String -Path $projectFile -Pattern "ProductCode" | Select-String -Pattern "8:{"
  $productCode = ($productCodeLines -split { $_ -eq "{" -or $_ -eq "}" })[1]

  return $productCode
}



function BuildUninstallBat {
  $productCode = GetInstallerProductCode

  $fileContent = @'
@echo off
msiexec /x {
'@ + $productCode + "}"

  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $buildDir = $installerDir + "\_build" # src\Installer\_build
  $uninstallFile = $buildDir + "\Uninstall.bat" # src\Installer\_build\Uninstall.bat

  Set-Content -Path $uninstallFile -Value $fileContent
}




enum Platform {
  x64
  x86
}



enum Mod {
  Normal
  NoUngroupMod
}



function DisableOutOfProcBuild() {
  $vsInstallPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
  $disableOutOfProcBuildFile = $vsInstallPath + "\Common7\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe"
  
  Push-Location $vsInstallPath
  & $disableOutOfProcBuildFile
  Pop-Location
}



function BuildNormalInstaller {
  BuildUninstallBat

  DisableOutOfProcBuild
  
  BuildExeAndInstaller -Platform x64 -Mod Normal
}




function BuildAllInstallers {
  BuildUninstallBat

  DisableOutOfProcBuild
  
  BuildExeAndInstaller -Platform x86 -Mod Normal
  BuildExeAndInstaller -Platform x86 -Mod NoUngroupMod
  BuildExeAndInstaller -Platform x64 -Mod NoUngroupMod
  BuildExeAndInstaller -Platform x64 -Mod Normal
}



function BuildExeAndInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod
  )
  BuildExe -Platform $Platform -Mod $Mod
  BuildInstaller -Platform $Platform -Mod $Mod
}



function BuildExe {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod
  )
  $ahkCompilerPath = "C:\Program Files\AutoHotKey\Compiler"
  $exeFile = $ahkCompilerPath + "\Ahk2Exe.exe"
  
  $binPlatform
  switch ($Platform) {
    x64 { $binPlatform = "\Unicode 64-bit.bin" }
    x86 { $binPlatform = "\Unicode 32-bit.bin" }
    Default { throw "Unexpected platform argument." }
  }
  $binFile = $ahkCompilerPath + $binPlatform

  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $assetsDir = $installerDir + "\_assets" # src\Installer\_assets
  $buildDir = $installerDir + "\_build" # src\Installer\_build
  $rootDir = Split-Path -Path $installerDir -Parent # src\

  $ahkSourceFile = $rootDir + "\SwitchApps.ahk"
  Copy-Item -Path $ahkSourceFile -Destination $buildDir -Force
  $copiedAhkFile = $buildDir + "\SwitchApps.ahk"

  switch ($Mod) {
    Normal { }
    NoUngroupMod { 
      (Get-Content -path $copiedAhkFile -Raw) -replace "Send {Enter}", "Send {Up} `n Send {Enter}" | Set-Content -Path $copiedAhkFile
    }
    Default { throw "Unexpected mod argument." }
  }

  $outputFile = $buildDir + "\SwitchApps.exe"
  $iconFile = $assetsDir + "\Icon_SwitchApps.ico"

  & $exeFile /in $copiedAhkFile /out $outputFile /icon $iconFile /bin $binFile
}



function BuildInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod
  )
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $projectFile = $installerDir + "\SwitchApps_Installer\SwitchApps_Installer.vdproj" # src\Installer\SwitchApps_Installer\SwitchApps_Installer.vdproj
  $solutionFile = $installerDir + "\SwitchApps.sln"

  switch ($Platform) {
    x64 { }
    x86 {
      (Get-Content -path $projectFile -Raw) -replace '"TargetPlatform" = "3:1"', '"TargetPlatform" = "3:0"' | Set-Content -Path $projectFile
      (Get-Content -path $projectFile -Raw) -replace 'ProgramFiles64Folder', 'ProgramFilesFolder' | Set-Content -Path $projectFile
    }
    Default { throw "Unexpected platform argument." }
  }

  $devenvFile = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"
  Write-Output "devenv.exe started."
  # $toLogOrNot = "/Out " + $installerDir + "\SwitchApps_Installer\Debug\log.txt"
  $toLogOrNot = ""
  Start-Process -FilePath $devenvFile -ArgumentList ($solutionFile + " /rebuild Debug $toLogOrNot") -Wait
  Write-Output "devenv.exe finished."

  switch ($Platform) {
    x64 { }
    x86 {
      (Get-Content -path $projectFile -Raw) -replace '"TargetPlatform" = "3:0"', '"TargetPlatform" = "3:1"' | Set-Content -Path $projectFile
      (Get-Content -path $projectFile -Raw) -replace 'ProgramFilesFolder', 'ProgramFiles64Folder' | Set-Content -Path $projectFile
    }
    Default { throw "Unexpected platform argument." }
  }

  $installerFile = $installerDir + "\SwitchApps_Installer\Debug\SwitchApps.msi" # \src\dotNetInstaller\SwitchApps_Installer\Debug\SwitchApps.msi
  $buildDir = $installerDir + "\_build" # \src\dotNetInstaller\_build

  $installerPlatform
  switch ($Platform) {
    x64 { $installerPlatform = "_x64" }
    x86 { $installerPlatform = "_x86" }
    Default { throw "Unexpected platform argument." }
  }
  $installerMod
  switch ($Mod) {
    Normal { $installerMod = "" }
    NoUngroupMod { $installerMod = "_NoUngroupMod" }
    Default { throw "Unexpected mod argument." }
  }

  Copy-Item -Path $installerFile -Destination ($buildDir + "\SwitchApps" + $installerPlatform + $installerMod + ".msi") -Force
}