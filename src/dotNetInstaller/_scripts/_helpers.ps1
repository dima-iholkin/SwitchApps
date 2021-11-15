# function RunInstaller {
#   Write-Output "RunInstaller..."

#   cd ..

#   cd .\build\

#   . .\SwitchApps_dev.msi

#   cd ..\scripts\
# }



$ErrorActionPreference = "Stop"



function DebugMe {
  dir
}




enum Platform {
  x64
  x86
}



enum Mod {
  Normal
  NoUngroupMod
}



function BuildAllInstallers {
  BuildExeAndInstaller -Platform x64 -Mod Normal
  BuildExeAndInstaller -Platform x86 -Mod Normal
  BuildExeAndInstaller -Platform x64 -Mod NoUngroupMod
  BuildExeAndInstaller -Platform x86 -Mod NoUngroupMod
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

  $scriptsDir = (Get-Location).Path # \src\dotNetInstaller\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # \src\dotNetInstaller
  $assetsDir = $installerDir + "\_assets" # \src\dotNetInstaller\_assets
  $buildDir = $installerDir + "\_build" # \src\dotNetInstaller\_build
  $rootDir = Split-Path -Path $installerDir -Parent # \src\

  $ahkSourceFile = $rootDir + "\SwitchApps.ahk"
  Copy-Item -Path $ahkSourceFile -Destination $buildDir -Force
  $copiedAhkFile = $buildDir + "\SwitchApps.ahk"

  switch ($Mod) {
    Normal { }
    NoUngroupMod { 
      (Get-Content -path $copiedAhkFile -Raw) -replace 'Send {Enter}', 'Send {Up} Send {Enter}' | Set-Content -Path $copiedAhkFile
    }
    Default { throw "Unexpected mod argument." }
  }

  $outputFile = $buildDir + "\SwitchApps.exe"
  $iconFile = $assetsDir + "\Icon_SwitchApps.ico"

  & $exeFile /in $copiedAhkFile /out $outputFile /icon $iconFile /bin $binFile

  # cd .\scripts\
}



function BuildInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [Platform] $Platform,
    [Mod] $Mod
  )
  $scriptsDir = (Get-Location).Path # \src\dotNetInstaller\_scripts
  $installerDir = Split-Path -Path $scriptsDir -Parent # \src\dotNetInstaller
  $projectFile = $installerDir + "\SwitchApps_Installer\SwitchApps_Installer.vdproj" # \src\dotNetInstaller\SwitchApps_Installer\SwitchApps_Installer.vdproj
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
  $logFile = $installerDir + "\SwitchApps_Installer\Debug\log.txt"
  Start-Process -FilePath $devenvFile -ArgumentList ($solutionFile + " /rebuild Debug /Out " + $logFile) -Wait
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



<# function BuildExe_x86 {
  cd ..
  
  if (Test-Path build) {
    
  }
  else {
    mkdir build | Out-Null
  }
  
  $exeFile = "C:\Program Files\AutoHotKey\Compiler\Ahk2Exe.exe" 

  $dir = (Get-Location).Path
  $inputFile = $dir + "\src\SwitchApps.ahk"
  $outputFile = $dir + "\build\SwitchApps.exe"
  $iconFile = $dir + "\build\Icon_SwitchApps.ico"
  $baseFile = "C:\Program Files\AutoHotKey\Compiler\Unicode 32-bit.bin"
  # $baseFile = "Unicode 32-bit.bin"

  & $exeFile /in $inputFile /out $outputFile /icon $iconFile /bin $baseFile

  cd .\scripts\
} #>



<# function BuildExe_Win11 {
  cd ..

  if (Test-Path build) {
    # no op.
  }
  else {
    mkdir build | Out-Null
  }

  $exeFile = "C:\Program Files\AutoHotKey\Compiler\Ahk2Exe.exe" 
  
  $dir = (Get-Location).Path
  $inputFile = $dir + "\src\SwitchApps_Win11.ahk"
  $outputFile = $dir + "\build\SwitchApps_Win11.exe"
  $iconFile = $dir + "\build\Icon_SwitchApps.ico"

  & $exeFile /in $inputFile /out $outputFile /icon $iconFile

  cd .\scripts\
} #>



<# function CleanUp {
  # cd ..\src\installer\

  try {
    $ErrorActionPreference = "Stop";
    Remove-Item build -Recurse -Force
    Write-Host '"Build" folder removed.' -ForegroundColor Green
  }
  catch {
    Write-Host '"Build" folder does not exist.'-ForegroundColor Yellow
  }
  finally {
    $ErrorActionPreference = "Continue";
  }
} #>



<# function BuildInstaller {
  [CmdletBinding()]
  param (
    [Parameter()]
    [string] $Stage
  )
  BuildExe;



  cd ..\src\installer\

  $wix = (Get-ChildItem Env:WIX).Value

  $candle = $wix + "bin\candle.exe"
  $light = $wix + "bin\light.exe"
  $wixUiExt = $wix + "bin\WixUIExtension.dll"

  $output = (Get-Location).Path + "\build\"



  $outputFile = $output + "Installer.wixobj"

  Write-Host "Candle:" -ForegroundColor Green

  $guid = (New-Guid).Guid

  if ($Stage -eq "Prod") {
    & $candle -out $outputFile Installer.wxs -dName="SwitchApps" -dModifySystemRegistry="true" -dGuid="$guid"
  }
  else {
    & $candle -out $outputFile Installer.wxs -dName="SwitchApps_dev" -dModifySystemRegistry="false" -dGuid="$guid"
  }

  Write-Host ""
  # Write-Host ""
  # Write-Host ""



  $outputFile
  if ($Stage -eq "Prod") {
    $outputFile = $output + "SwitchApps.msi"
  }
  else {
    $outputFile = $output + "SwitchApps_dev.msi"
  }

  Write-Host "Light:" -ForegroundColor Green
  Write-Host "Ignored warnings: ICE03, ICE91." -ForegroundColor Yellow

  & $light -ext $wixUiExt -out $outputFile ./build/Installer.wixobj -sice:ICE03 -sice:ICE91



  if ($Stage -eq "Prod") {
    Copy-Item .\build\SwitchApps.msi -Destination ..\..\build\
  }
  else {
    Copy-Item .\build\SwitchApps_dev.msi -Destination ..\..\build\
  }



  CleanUp;



  cd ..\..\scripts\
} #>