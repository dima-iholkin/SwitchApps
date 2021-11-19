function RemoveRegistryValues {
  [CmdletBinding()]
  param (
    [Parameter()]
    [switch] $MinThumbSizePx,
    [switch] $ExtendedUIHoverTime,
    [switch] $MsOfficeAdPopup
  )
  if ($PSBoundParameters.ContainsKey('MinThumbSizePx') -eq $True) {
    $Path = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Taskband"
    $Name = "MinThumbSizePx"
    Remove-ItemProperty -Path $Path -Name $Name
  }

  if ($PSBoundParameters.ContainsKey('ExtendedUIHoverTime') -eq $True) {
    $Path = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
    $Name = "ExtendedUIHoverTime"
    Remove-ItemProperty -Path $Path -Name $Name
  }

  if ($PSBoundParameters.ContainsKey('MsOfficeAdPopup') -eq $True) {
    $Path = "HKCU:\SOFTWARE\Classes\ms-officeapp\Shell\Open\Command"
    $Name = '(default)'
    if ((Test-Path $Path) -eq $True) {
      Remove-Item -Path $Path
    }
    New-Item -Path $Path -Force
  }
}