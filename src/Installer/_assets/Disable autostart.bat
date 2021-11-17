@echo off

schtasks /Change /TN "\SwitchApps\SwitchApps autostart" /Disable && echo Autostart disabled.

timeout 10