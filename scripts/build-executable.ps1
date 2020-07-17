function BuildExe {
  cd ..

  $exeFile = "C:\Program Files\AutoHotKey\Compiler\Ahk2Exe.exe" 

  $dir = (Get-Location).Path

  $inputFile = $dir + "\src\SwitchApps.ahk"



  if (Test-Path build) {
    
  }
  else {
    mkdir build | Out-Null
  }

  $outputFile = $dir + "\build\SwitchApps.exe"



  & $exeFile /in $inputFile /out $outputFile



  cd .\scripts\
}

BuildExe;