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
    [string] $ThumbSize,
    [Parameter()]
    [string] $ThumbDelay,
    [Parameter()]
    [string] $MsOfficePopup
  )
  Write-Output "Uninstall started...";

  $str = "ThumbSize: " + $ThumbSize
  $str1 = "ThumbDelay: " + $ThumbDelay
  $str2 = "MsOfficePopup: " + $MsOfficePopup
  Write-Output $str
  Write-Output $str1
  Write-Output $str2
  Start-Sleep -Seconds 5


  
  Unregister-ScheduledTask -TaskPath "\SwitchApps\" -TaskName "SwitchApps autostart" -Confirm:$false  

  Write-Output "Scheduler task uninstalled."



  $thumbSizeInt = $ThumbSize.Remove(0, 3) -as [int]
  Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband" -Name MinThumbSizePx -Value $thumbSizeInt

  $thumbDelayInt = $ThumbDelay.Remove(0, 3) -as [int]
  Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name ExtendedUIHoverTime -Value $thumbDelayInt

  if ($MsOfficePopup -eq "false") {
    Remove-Item -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command"
  } else {
    Set-ItemProperty -Path "HKCU:\Software\Classes\ms-officeapp\Shell\Open\Command"  -Value $MsOfficePopup
  }

  Write-Output "Registry changes reverted."
}



# Install;