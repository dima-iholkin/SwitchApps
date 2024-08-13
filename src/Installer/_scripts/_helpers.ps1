$ErrorActionPreference = "Stop"

# Public functions:

function BuildNormalInstaller {
  BuildUninstallBat
  DisableOutOfProcBuild
  BuildExeAndInstaller -Platform x64 -Mod Normal
}

function BuildAllInstallers {
  # Create the "_build" folder:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $buildDir = $installerDir + "\_build" # src\Installer\_build
  New-Item -ItemType Directory -Path $buildDir -Force
  # Build the dependencies and the installers:
  BuildUninstallBat
  DisableOutOfProcBuild
  # RunVS2022
  # UpdateDevenvConfiguration
  BuildExeAndInstaller -Platform x86 -Mod Normal
  BuildExeAndInstaller -Platform x86 -Mod AppGroupingMod
  BuildExeAndInstaller -Platform x64 -Mod AppGroupingMod
  BuildExeAndInstaller -Platform x64 -Mod Normal
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
    [Mod] $Mod
  )
  # Guard clause:
  # if ($PSVersionTable.PSVersion.Major -lt 7) {
  #   throw "The script requires PowerShell 7 or newer.";
  # }
  # Set the AHK "compiler" directory path:
  if (Test-Path -Path "$PSScriptRoot\_autohotkey\Compiler\Ahk2Exe.exe") {
    $ahkCompilerPath = "$PSScriptRoot\_autohotkey\Compiler"
  }
  elseif (Test-Path -Path "C:\Program Files\AutoHotKey\Compiler\Ahk2Exe.exe") {
    $ahkCompilerPath = "C:\Program Files\AutoHotKey\Compiler"
  }
  else {
    throw "AutoHotKey compiler directory not found.";
  }
  if (($Platform -eq "x86") -and ($Mod -eq "Normal")) {
    Write-Output "AutoHotKey compiler directory path: $ahkCompilerPath"
  }
  # Set the "Ahk2Exe.exe" file path:
  $exeFile = $ahkCompilerPath + "\Ahk2Exe.exe"
  # Set the bin platform file path:
  switch ($Platform) {
    x64 { $binPlatform = "\Unicode 64-bit.bin" }
    x86 { $binPlatform = "\Unicode 32-bit.bin" }
    Default { throw "Unexpected bin platform argument." }
  }
  $binFile = $ahkCompilerPath + $binPlatform
  # Copy the base platform AHK file:
  if (Test-Path -Path "$PSScriptRoot\_autohotkey\Compiler\Ahk2Exe.exe") {
    Copy-Item -Path "$binFile" -Destination "$ahkCompilerPath\AutoHotkeySC.bin" -Force
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
  if (Test-Path -Path $outputFile) {
    Write-Output "SwitchApps.exe created"
  }
  else {
    Write-Output "SwitchApps.exe not created"
  }
}

function BuildInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod
  )
  # Set the paths:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $projectFile = $installerDir + "\SwitchApps_Installer\SwitchApps_Installer.vdproj" # src\Installer\SwitchApps_Installer\SwitchApps_Installer.vdproj
  $solutionFile = $installerDir + "\SwitchApps.sln"
  # Modify the project file for x86 platform:
  switch ($Platform) {
    x64 { }
    x86 {
      (Get-Content -path $projectFile -Raw) -replace '"TargetPlatform" = "3:1"', '"TargetPlatform" = "3:0"' | Set-Content -Path $projectFile -Force
      (Get-Content -path $projectFile -Raw) -replace 'ProgramFiles64Folder', 'ProgramFilesFolder' | Set-Content -Path $projectFile -Force
    }
    Default { throw "Unexpected platform argument." }
  }
  # Set the "devenv.exe" file path:
  $vsInstallPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
  $devenvFile = $vsInstallPath + "\Common7\IDE\devenv.com"
  # Guard clause:
  if ((Test-Path -Path $devenvFile) -eq $false) {
    throw "Visual Studio 2022 devenv.exe file not found."
  }
  # Build the installer:
  Write-Output "devenv.exe started: platform $Platform, mod $Mod."
  # $toLogOrNot = "/Out " + $installerDir + "\SwitchApps_Installer\Debug\log.txt"
  $toLogOrNot = ""
  $timeoutReached = $null
  $proc = Start-Process -FilePath $devenvFile -ArgumentList ("$solutionFile /Rebuild Debug") -NoNewWindow -PassThru
  $proc | Wait-Process -Timeout 60 -ErrorAction SilentlyContinue -ErrorVariable timeoutReached
  if ($timeoutReached) {
    # Terminate the process:
    $proc | Stop-Process
    # Retry the build:
    Write-Output "Retrying the build..."
    Start-Process -FilePath $devenvFile -ArgumentList ("$solutionFile /Rebuild Debug") -NoNewWindow -Wait
  }
  Write-Output "devenv.exe finished: platform $Platform, mod $Mod."
  # Revert the project file's modification after an x86 platform run:
  switch ($Platform) {
    x64 { }
    x86 {
      (Get-Content -path $projectFile -Raw) -replace '"TargetPlatform" = "3:0"', '"TargetPlatform" = "3:1"' | Set-Content -Path $projectFile -Force
      (Get-Content -path $projectFile -Raw) -replace 'ProgramFilesFolder', 'ProgramFiles64Folder' | Set-Content -Path $projectFile -Force
    }
    Default { throw "Unexpected platform argument." }
  }
  # Set the paths:
  $installerFile = $installerDir + "\SwitchApps_Installer\Debug\SwitchApps.msi" # \src\Installer\SwitchApps_Installer\Debug\SwitchApps.msi
  $buildDir = $installerDir + "\_build" # \src\Installer\_build
  # Set the installer name suffixes:
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
    [Mod] $Mod
  )
  BuildExe -Platform $Platform -Mod $Mod
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

function UpdateDevenvConfiguration {
  # Set the "devenv.exe" file path:
  $vsInstallPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
  $devenvFile = $vsInstallPath + "\Common7\IDE\devenv.exe"
  # Guard clause:
  if ((Test-Path -Path $devenvFile) -eq $false) {
    throw "Visual Studio 2022 devenv.exe file not found."
  }
  # Run "devenv /updateconfiguration":
  Write-Output 'Run "devenv /updateconfiguration"'
  Start-Process -FilePath $devenvFile -ArgumentList ("/updateconfiguration") -NoNewWindow -Wait
}

function RunVS2022 {
  # Set the paths:
  $scriptsDir = $PSScriptRoot # src\Installer\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # src\Installer
  $solutionFile = $installerDir + "\SwitchApps.sln"
  # $projectFile = $installerDir + "\SwitchApps_Library\SwitchApps.Library.csproj"
  # Set the "devenv.exe" file path:
  $vsInstallPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
  $devenvFile = $vsInstallPath + "\Common7\IDE\devenv.exe"
  # Guard clause:
  if ((Test-Path -Path $devenvFile) -eq $false) {
    throw "Visual Studio 2022 devenv.exe file not found."
  }
  # Run the solution in VS 2022:
  Write-Output 'Run the solution in VS 2022...'
  $proc = Start-Process -FilePath $devenvFile -ArgumentList ("/runexit $solutionFile") -PassThru
  $timeoutReached = $null
  $proc | Wait-Process -Timeout 60 -ErrorAction SilentlyContinue -ErrorVariable timeoutReached
  if ($timeoutReached) {
    # Terminate the process:
    Write-Output "Terminate the VS 2022 process..."
    $proc | Stop-Process
  }
  cd $installerDir
  ls
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