$ErrorActionPreference = "Stop"

# Public functions:

function BuildAllInstallers {
  # Create the "_build" folder:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $buildDir = $installerDir + "\_build" # src\Installer\_build
  New-Item -ItemType Directory -Path $buildDir -Force
  # Build the dependencies:
  BuildUninstallBat
  # Configure VS 2022:
  DisableOutOfProcBuild
  # Choose the AHK "Compiler" directory path:
  $ahkCompilerPath = GetAhkCompilerPath
  # Build the executables and installers:
  SetProjectFileToPlatform -Platform x64
  BuildExeAndInstaller -Platform x64 -Mod Normal -AhkCompilerPath $ahkCompilerPath
  BuildExeAndInstaller -Platform x64 -Mod AppGroupingMod -AhkCompilerPath $ahkCompilerPath
  SetProjectFileToPlatform -Platform x86
  BuildExeAndInstaller -Platform x86 -Mod Normal -AhkCompilerPath $ahkCompilerPath
  BuildExeAndInstaller -Platform x86 -Mod AppGroupingMod -AhkCompilerPath $ahkCompilerPath
  SetProjectFileToPlatform -Platform x64
}

# Internal functions:

function BuildUninstallBat {
  # Prepare the product code from the installer project:
  $productCode = GetInstallerProductCode
  $fileContent = @'
@echo off
msiexec /x {
'@ + $productCode + "}"
  # Prepare the "Uninstall.bat" path:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $buildDir = $installerDir + "\_build" # src\Installer\_build
  $uninstallFile = $buildDir + "\Uninstall.bat" # src\Installer\_build\Uninstall.bat
  # Save the file:
  Set-Content -Path $uninstallFile -Value $fileContent
}

function BuildExe {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod,
    [String] $AhkCompilerPath
  )
  # Set the "Ahk2Exe.exe" file path:
  $exeFile = $AhkCompilerPath + "\Ahk2Exe.exe"
  # Set the bin platform file path:
  switch ($Platform) {
    x64 { $binPlatform = "\Unicode 64-bit.bin" }
    x86 { $binPlatform = "\Unicode 32-bit.bin" }
    Default { throw "Unexpected bin platform argument." }
  }
  $binFile = $AhkCompilerPath + $binPlatform
  # Copy the base platform AHK file, if it's the local "_autohotkey" folder:
  if ("$PSScriptRoot\_autohotkey\Compiler" -eq $AhkCompilerPath) {
    Write-Host "Copy $binFile as AutoHotkeySC.bin"
    Copy-Item -Path "$binFile" -Destination "$AhkCompilerPath\AutoHotkeySC.bin" -Force
  }
  # Set the directory paths:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $assetsDir = $installerDir + "\_assets" # src\Installer\_assets
  $buildDir = $installerDir + "\_build" # src\Installer\_build
  $rootDir = Split-Path -Path $installerDir -Parent # src\
  # Copy the AHK script file:
  $ahkSourceFile = $rootDir + "\SwitchApps.ahk"
  Copy-Item -Path $ahkSourceFile -Destination $buildDir -Force
  $copiedAhkFile = $buildDir + "\SwitchApps.ahk"
  # Modify the script file, if the Mod set to AppGroupingMod:
  switch ($Mod) {
    Normal { }
    AppGroupingMod {
      (Get-Content -path $copiedAhkFile -Raw) -replace "Send {Enter}", "Send {Up} `n Send {Enter}" | Set-Content -Path $copiedAhkFile -Force
    }
    Default { throw "Unexpected mod argument." }
  }
  # Set the output file path:
  $outputFile = $buildDir + "\SwitchApps.exe"
  $iconFile = $assetsDir + "\Icon_SwitchApps.ico"
  # Build the executable:
  Start-Process -FilePath $exeFile -Wait -ArgumentList $(
    "/in $copiedAhkFile",
    "/out $outputFile",
    "/icon $iconFile"
  )
}

function BuildInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod
  )
  # Set the paths for build:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $solutionFile = $installerDir + "\SwitchApps.sln" # src\Installer\SwitchApps.sln
  # Set the "devenv.com" file path:
  $vsInstallPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
  $devenvFile = $vsInstallPath + "\Common7\IDE\devenv.com"
  # Guard clause:
  if ((Test-Path -Path $devenvFile) -eq $false) {
    throw "Visual Studio 2022 devenv.com file not found."
  }
  # Build the installer:
  Write-Host "devenv.com started: platform $Platform, mod $Mod."
  Start-Process -FilePath $devenvFile -ArgumentList ("$solutionFile /Rebuild Debug") -NoNewWindow -Wait
  Write-Host "devenv.com finished: platform $Platform, mod $Mod."
  # Set the paths for copy:
  $installerFile = $installerDir + "\SwitchApps_Installer\Debug\SwitchApps.msi" # \src\Installer\SwitchApps_Installer\Debug\SwitchApps.msi
  $buildDir = $installerDir + "\_build" # \src\Installer\_build
  # Choose the installer name suffixes:
  switch ($Platform) {
    x64 { $installerPlatform = "_x64" }
    x86 { $installerPlatform = "_x86" }
    Default { throw "Unexpected platform argument." }
  }
  switch ($Mod) {
    Normal { $installerMod = "" }
    AppGroupingMod { $installerMod = "_AppGroupingMod" }
    Default { throw "Unexpected mod argument." }
  }
  # Copy the installer file to the "build" directory:
  Copy-Item -Path $installerFile -Destination ($buildDir + "\SwitchApps" + $installerPlatform + $installerMod + ".msi") -Force
}

function BuildExeAndInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod,
    [String] $AhkCompilerPath
  )
  BuildExe -Platform $Platform -Mod $Mod -AhkCompilerPath $AhkCompilerPath
  BuildInstaller -Platform $Platform -Mod $Mod
}

function DisableOutOfProcBuild() {
  # Set the paths:
  $vsInstallPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
  $disableOutOfProcBuildFile = $vsInstallPath + "\Common7\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe"
  # Guard clauses:
  if ((Test-Path -Path $vsInstallPath) -eq $false) {
    throw "Visual Studio 2022 vswhere.exe file not found."
  }
  if ((Test-Path -Path $disableOutOfProcBuildFile) -eq $false) {
    throw "Visual Studio 2022 DisableOutOfProcBuild.exe file not found."
  }
  # Run the command:
  Push-Location $vsInstallPath
  & $disableOutOfProcBuildFile
  Pop-Location
}

function GetAhkCompilerPath {
  # Choose the AHK "Compiler" directory path:
  if (Test-Path -Path "$PSScriptRoot\_autohotkey\Compiler\Ahk2Exe.exe") {
    $ahkCompilerPath = "$PSScriptRoot\_autohotkey\Compiler"
  }
  elseif (Test-Path -Path "C:\Program Files\AutoHotKey\Compiler\Ahk2Exe.exe") {
    $ahkCompilerPath = "C:\Program Files\AutoHotKey\Compiler"
  }
  else {
    throw "AutoHotKey compiler directory not found.";
  }
  # Log the AHK "Compiler" directory path once:
  Write-Host "AutoHotKey compiler directory path: $ahkCompilerPath"
  # Return the chosen path:
  return $ahkCompilerPath
}

function GetInstallerProductCode {
  # Set the paths:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $projectFile = $installerDir + "\SwitchApps_Installer\SwitchApps_Installer.vdproj" # src\Installer\SwitchApps_Installer\SwitchApps_Installer.vdproj
  # Parse and return the product code:
  $productCodeLines = Select-String -Path $projectFile -Pattern "ProductCode" | Select-String -Pattern "8:{"
  $productCode = ($productCodeLines -split { $_ -eq "{" -or $_ -eq "}" })[1]
  return $productCode
}

function SetProjectFileToPlatform {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform
  )
  # Set the paths:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $projectFile = $installerDir + "\SwitchApps_Installer\SwitchApps_Installer.vdproj" # src\Installer\SwitchApps_Installer\SwitchApps_Installer.vdproj
  # Modify the project file:
  $fileContents = Get-Content -path $projectFile -Raw
  switch ($Platform) {
    x64 {
      if ($fileContents.Contains('"TargetPlatform" = "3:0"') -or $fileContents.Contains('ProgramFilesFolder')) {
        $fileContents.Replace('"TargetPlatform" = "3:0"', '"TargetPlatform" = "3:1"').Replace('ProgramFilesFolder', 'ProgramFiles64Folder') | Set-Content -Path $projectFile -Force
      }
    }
    x86 {
      if ($fileContents.Contains('"TargetPlatform" = "3:1"') -or $fileContents.Contains('ProgramFiles64Folder')) {
        $fileContents.Replace('"TargetPlatform" = "3:1"', '"TargetPlatform" = "3:0"').Replace('ProgramFiles64Folder', 'ProgramFilesFolder') | Set-Content -Path $projectFile -Force
      }
    }
    Default { throw "Unexpected platform argument." }
  }
}

# Enums:

enum Platform {
  x64
  x86
}

enum Mod {
  Normal
  AppGroupingMod
}

# Debug functions:

function DebugCurrentDir {
  Get-Location
  $PSScriptRoot
}