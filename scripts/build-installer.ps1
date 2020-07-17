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
  . .\build-executable.ps1 BuildExe



  cd ..\src\installer\

  $wix = (Get-ChildItem Env:WIX).Value

  $candle = $wix + "bin\candle.exe"
  $light = $wix + "bin\light.exe"

  $output = (Get-Location).Path + "\build\"



  $outputFile = $output + "Installer.wixobj"

  Write-Host "Candle:" -ForegroundColor Green

  & $candle -out $outputFile Installer.wxs -dName="SwitchApps_dev" -dModifySystemRegistry="false"

  Write-Host ""
  # Write-Host ""
  # Write-Host ""



  $outputFile = $output + "SwitchApps.msi"

  Write-Host "Light:" -ForegroundColor Green
  Write-Host "Ignored warnings: ICE03, ICE91." -ForegroundColor Yellow

  & $light -out $outputFile ./build/Installer.wixobj -sice:ICE03 -sice:ICE91



  Copy-Item .\build\SwitchApps.msi -Destination ..\..\build\



  CleanUp;



  cd ..\..\scripts\
}

BuildInstaller;