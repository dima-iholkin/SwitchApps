function RunThisToo {
  taskkill /IM "SwitchApps.exe"
  pause
}

# REM start ".\SwitchApps.exe"
# start-process -filepath ./SwitchApps.exe -verb RunAs

# wmic process where name="SwitchApps.exe" CALL setpriority "realtime"


RunThisToo;