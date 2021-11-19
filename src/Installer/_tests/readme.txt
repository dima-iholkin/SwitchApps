Start with this on a test VM:
0. Open a PowerShell window.

1. cd to wherever you have the test scripts:
> cd "C:\_temp"

2. Allow the current PowerShell session to run scripts:
> Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process

3. Run the scripts, for example:
> . .\GetCurrentRegistryValues.ps1
> . .\SetRegistryValues.ps1; SetRegistryValues -MinThumbSizePx 400 -ExtendedUIHoverTime 50 -MsOfficeAdPopup "rundll32"
> . .\RemoveRegistryValues.ps1; RemoveRegistryValues -MinThumbSizePx -ExtendedUIHoverTime -MsOfficeAdPopup