@echo off

schtasks /Change /TN "\SwitchApps\SwitchApps autostart" /Enable && echo Autostart enabled.

timeout 10