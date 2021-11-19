function SetRegistryValues {
  [CmdletBinding()]
  param (
    [Parameter()]
    [int] $MinThumbSizePx,
    [int] $ExtendedUIHoverTime,
    [string] $MsOfficeAdPopup
  )
  if ($PSBoundParameters.ContainsKey('MinThumbSizePx') -eq $True) {
    $Path = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Taskband"
    $Name = "MinThumbSizePx"
    Set-ItemProperty -Path $Path -Name $Name -Value $MinThumbSizePx -Type DWord
  }

  if ($PSBoundParameters.ContainsKey('ExtendedUIHoverTime') -eq $True) {
    $Path = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
    $Name = "ExtendedUIHoverTime"
    Set-ItemProperty -Path $Path -Name $Name -Value $ExtendedUIHoverTime -Type DWord
  }

  if ($PSBoundParameters.ContainsKey('MsOfficeAdPopup') -eq $True) {
    $Path = "HKCU:\SOFTWARE\Classes\ms-officeapp\Shell\Open\Command"
    $Name = '(default)'
    if ((Test-Path $Path) -eq $False) {
      New-Item -Path $Path -Force
    }
    Set-ItemProperty -Path $Path -Name $Name -Value $MsOfficeAdPopup -Type String
  }
}