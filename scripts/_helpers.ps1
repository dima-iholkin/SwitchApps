function RunInstaller {
  Write-Output "RunInstaller..."

  cd ..

  cd .\build\

  . .\SwitchApps_dev.msi

  cd ..\scripts\
}



function BuildExe {
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

  & $exeFile /in $inputFile /out $outputFile /icon $iconFile

  cd .\scripts\
}



function BuildExe_x86 {
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
}



function BuildExe_Win11 {
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
}



function CleanUp {
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
}



function BuildInstaller {
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
}