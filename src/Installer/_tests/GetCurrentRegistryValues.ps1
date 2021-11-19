function GetCurrentRegistryValues {
  $minThumbSizePx_Path = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Taskband"
  $minThumbSizePx_Name = "MinThumbSizePx"
  $minThumbSizePx_Value = (Get-ItemProperty -Path $minThumbSizePx_Path -Name $minThumbSizePx_Name -ErrorAction SilentlyContinue).MinThumbSizePx
  if ($minThumbSizePx_Value -eq $null) {
    $minThumbSizePx_Value = "null"
  }
  Write-Host ($minThumbSizePx_Path + " : " + $minThumbSizePx_Name + " : " + $minThumbSizePx_Value)

  $extendedUIHoverTime_Path = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
  $extendedUIHoverTime_Name = "ExtendedUIHoverTime"
  $extendedUIHoverTime_Value = (Get-ItemProperty -Path $extendedUIHoverTime_Path -Name $extendedUIHoverTime_Name -ErrorAction SilentlyContinue).ExtendedUIHoverTime
  if ($extendedUIHoverTime_Value -eq $null) {
    $extendedUIHoverTime_Value = "null"
  }
  Write-Host ($extendedUIHoverTime_Path + " : " + $extendedUIHoverTime_Name + " : " + $extendedUIHoverTime_Value)

  $msOfficeAdPopup_Path = "HKCU:\SOFTWARE\Classes\ms-officeapp\Shell\Open\Command"
  $msOfficeAdPopup_Name = '(default)'
  if ((Test-Path $msOfficeAdPopup_Path) -eq $False) {
    New-Item -Path $msOfficeAdPopup_Path -Force
  }
  $msOfficeAdPopup_Value = (Get-ItemProperty -Path $msOfficeAdPopup_Path -Name $msOfficeAdPopup_Name -ErrorAction SilentlyContinue).'(default)'
  if ($msOfficeAdPopup_Value -eq $null) {
    $msOfficeAdPopup_Value = "null"
  }
  Write-Host ($msOfficeAdPopup_Path + " : " + $msOfficeAdPopup_Name + " : " + $msOfficeAdPopup_Value)
}

GetCurrentRegistryValues