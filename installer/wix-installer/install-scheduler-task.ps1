function Install {
  [CmdletBinding()]
  param (
      [Parameter()]
      [String]
      $InstallDir
  )
  Write-Output "Install started";



  try {
    Unregister-ScheduledTask -TaskPath "\SwitchApps\" -TaskName "SwitchApps autostart" -Confirm:$false -ErrorAction SilentlyContinue
  }
  catch {
    Write-Output "Unable to delete the task."
  }
  # Write-Output "Continue..."



  # $A = New-ScheduledTaskAction -Execute "C:\MySoftware\Projects\SwitchApps\installer\assets\SwitchApps.exe"
  $PathToExe = $InstallDir + "\SwitchApps.exe"
  $A = New-ScheduledTaskAction -Execute $PathToExe



  $user = [System.Security.Principal.WindowsIdentity]::GetCurrent();

  $P = New-ScheduledTaskPrincipal -UserId $user.User.Value -RunLevel Highest

  $T = New-ScheduledTaskTrigger -AtLogon -User $user.Name



  $S = New-ScheduledTaskSettingsSet -Priority 0 -AllowStartIfOnBatteries
  $S.ExecutionTimeLimit = 'PT0S';



  $D = New-ScheduledTask -Action $A -Principal $P -Trigger $T -Settings $S



  Register-ScheduledTask -TaskPath "SwitchApps" -TaskName "SwitchApps autostart" -InputObject $D
}



function Uninstall {
  Write-Output "Uninstall started";

  Unregister-ScheduledTask -TaskPath "\SwitchApps\" -TaskName "SwitchApps autostart" -Confirm:$false  
}



# Install;