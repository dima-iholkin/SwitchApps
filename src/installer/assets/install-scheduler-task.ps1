function Install {
  [CmdletBinding()]
  param (
    [Parameter()]
    [string] $InstallDir,
    [Parameter()]
    [string] $Name,
    [Parameter()]
    [string] $ModifySystemRegistry
  )
  Write-Output "Install started";



  try {
    Unregister-ScheduledTask -TaskPath "\$Name\" -TaskName "$Name autostart" -Confirm:$false -ErrorAction SilentlyContinue
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

  Register-ScheduledTask -TaskPath "$Name" -TaskName "$Name autostart" -InputObject $D
  Write-Output "Scheduler task created."



  # $thumbSize = Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband" -Name MinThumbSizePx
  # Set-ItemProperty -Path "HKCU:\Software\SwitchApps\Backup" -Name ThumbnailPreviewSize_Backup -Value $thumbSize.MinThumbSizePx

  # $thumbDelay = Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name ExtendedUIHoverTime
  # Set-ItemProperty -Path "HKCU:\Software\SwitchApps\Backup" -Name ThumbnailPreviewDelay_Backup -Value $thumbDelay.ExtendedUIHoverTime

  # if (Test-Path -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command") {
  #   $val = Get-ItemProperty -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command"
  #   Set-ItemProperty -Path "HKCU:\Software\SwitchApps\Backup" -Name MsOfficeAdPopup_Backup -Value $val.'(default)'
  # } else {
  #   Set-ItemProperty -Path "HKCU:\Software\SwitchApps\Backup" -Name MsOfficeAdPopup_Backup -Value "false"
  # }
  
  # Write-Output "Registry backup created."
}



function Uninstall {
  [CmdletBinding()]
  param (
    [Parameter()]
    [string] $ThumbSizePreInstall,
    [Parameter()]
    [string] $ThumbDelayPreInstall,
    [Parameter()]
    [string] $MsOfficePopupPreInstall,
    [Parameter()]
    [string] $Name,
    [Parameter()]
    [string] $ModifySystemRegistry
  )
  Write-Output "Uninstall started...";

  $str = "ThumbSize: " + $ThumbSizePreInstall
  $str1 = "ThumbDelay: " + $ThumbDelayPreInstall
  $str2 = "MsOfficePopup: " + $MsOfficePopupPreInstall
  Write-Output $str
  Write-Output $str1
  Write-Output $str2
  # Start-Sleep -Seconds 5


  
  Unregister-ScheduledTask -TaskPath "\$Name\" -TaskName "$Name autostart" -Confirm:$false  
  # Remove the task scheduler task.



  $scheduleObject = New-Object -ComObject Schedule.Service
  $scheduleObject.connect()

  $rootFolder = $scheduleObject.GetFolder("\")

  $appFolder = $rootFolder.GetFolder($Name)
  
  if ($appFolder.GetTasks(0).Count -eq 0) {
    $rootFolder.DeleteFolder($Name, $null)
  }
  # Remove the task scheduler empty folder.

  Write-Output "Scheduler task uninstalled."



  if ($ModifySystemRegistry -eq "true") {
    $ThumbSizePostInstall = 800;
    $ThumbSizeCurrent = (Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband" -Name MinThumbSizePx).MinThumbSizePx
    if ($ThumbSizeCurrent -eq $ThumbSizePostInstall) {
      # if REG value now equals the PostInstall value, meaning nothing has changed it in between...

      $thumbSizeInt = $ThumbSizePreInstall.Remove(0, 3) -as [int]
      Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband" -Name MinThumbSizePx -Value $thumbSizeInt
      # ...then revert the value to the PreInstall one.
    }
    


    $ThumbDelayPostInstall = 0;
    $ThumbDelayCurrent = (Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name ExtendedUIHoverTime).ExtendedUIHoverTime
    if ($ThumbDelayCurrent -eq $ThumbDelayPostInstall) {
      # if REG value now equals the PostInstall value, meaning nothing has changed it in between...

      $thumbDelayInt = $ThumbDelayPreInstall.Remove(0, 3) -as [int]
      Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name ExtendedUIHoverTime -Value $thumbDelayInt
      # ...then revert the value to the PreInstall one.
    }
    
    
    $MsOfficePopupPostInstall = "rundll32";
    $MsOfficePopupCurrent = (Get-ItemProperty -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command").'(default)'
    if ($MsOfficePopupCurrent -eq $MsOfficePopupPostInstall) {
      # if REG value now equals the PostInstall value, meaning nothing has changed it in between...

      if ($MsOfficePopupPreInstall -eq "false") {
        Remove-Item -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command"
      }
      else {
        Set-ItemProperty -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command" -Name '(default)' -Value $MsOfficePopupPreInstall
      }
      # ...then revert the value to the PreInstall one.
    }
    
    Write-Output "Registry changes reverted."
  }
  else {
    Write-Output "ModifySystemRegistry = false"
  }
}



# Install;