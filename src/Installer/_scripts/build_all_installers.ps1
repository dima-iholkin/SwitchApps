try
{
  # Set the root path:
  Write-Host "Scripts root path: $PSScriptRoot"
  . "$PSScriptRoot\_helpers.ps1"
  # Build all installers:
  BuildAllInstallers
}
catch
{
    Write-Error $_.Exception.ToString()
    Read-Host -Prompt "The above error occurred. Press Enter to exit."
}