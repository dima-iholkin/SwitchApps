function RunThis {
  # Start-Process powershell -filepath ./SwitchApps_script.ps1 -verb RunAs
  Start-Process powershell -ArgumentList '-noprofile -file SwitchApps_script.ps1' -verb RunAs
  pause
}

# REM start ".\SwitchApps.exe"
# start-process -filepath ./SwitchApps.exe -verb RunAs

# wmic process where name="SwitchApps.exe" CALL setpriority "realtime"


RunThis;