# SwitchApps

> Maybe a better way to switch apps (Alt+Tab) for Windows 10.

This solution overrides the `Alt+Tab` and `Alt+Shift+Tab` shortcut behaviour.
Based on the Windows 10's `Win+T` shortcut.

When the AHK script is stopped, the standard `Alt+Tab` behaviour of Windows 10 will be resumed.

**how it works**  
![demonstation](../assets/readme/demo.gif)

## Table of Contents

* [Basic set up](#basic-set-up)
* [Additional set up (Optional)](#additional-set-up-optional)
* [Recomendations](#recomendations)
* [Known issues](#known-issues)
* [License](#license)

## Basic set up

* Install [AutoHotKey](https://www.autohotkey.com) **with UI-Access** (version 1.1.32 or a newer v1).  
[a picture of the installer menu]  

> Without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps (Task Manager, HWInfo and others).

* Download the `SwitchApps.ahk` script.  
[a link to the releases](/release/latest)

* Start the downloaded `SwitchApps.ahk` script **with UI-Access**.  
[a picture of the Windows 10 menu]

## Additional set up (Optional)

* Separate each app icon on the taskbar (to not group the icons) with [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker):  
Grouping > **Don't group**.  
![7+ Taskbar Tweaker settings](../assets/readme/7-Taskbar-Tweaker.png)

* Stop an Office ad pop up, when `Alt+Shift+Tab` is pressed, with [this solution](https://www.howtogeek.com/445318/how-to-remap-the-office-key-on-your-keyboard/):  
disable the `Office key` from opening the Office stuff, by adding to the registry

```powershell
REG ADD HKCU\Software\Classes\ms-officeapp\Shell\Open\Command /t REG_SZ /d rundll32
```

* Increase the thumbnail preview size with [this solution](https://winaero.com/blog/change-taskbar-thumbnail-size-windows-10/).  

> I used the value of 800 to maximize the thumbnail size.  
> It certainly doesn’t scale to this size on my config - you may experiment with the value.

* Change the thumbnail preview delay to 0 with [this solution](https://www.tenforums.com/tutorials/21005-change-delay-time-show-taskbar-thumbnails-windows-10-a.html).

* Autostart the `SwitchApps.ahk` with UI-Access.  
[figure out the best way to do it]

## Recomendations

* If you’re using a multi-monitor setup, show the apps only on the taskbar of the same display:  
Taskbar settings > Show taskbar buttons on > Taskbar where window is open
[Taskbar settings](../assets/readme/Taskbar-settings.png)

## Known issues

* The shortcut will not trigger in the very privileged apps, like an antivirus app (so the standard `Alt+Tab` behaviour will trigger).

* Occasionally it may not trigger or send an `Enter` or some other weird bit to Windows.

* It’s not perfect; it’s built on top of a few native things.

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
