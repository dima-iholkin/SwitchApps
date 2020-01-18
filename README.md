# SwitchApps

> Maybe a better way to switch apps (Alt+Tab) for Windows 10.

This solution overrides the `Alt+Tab` and `Alt+Shift+Tab` shortcut behaviour.
Based on the Windows 10's `Win+T` shortcut.

When the AHK script is stopped, the standard `Alt+Tab` behaviour of Windows 10 will be resumed.

> The author was annoyed with the Windows 10 `Alt+Tab` behaviour of the shortcut and the other options,
and wanted a behaviour similar to:  
     * switching the tabs in a web browser, like in Chrome and Opera with: `Ctrl+PageUp` and `Ctrl+PageDown`,  
     * switching the desktops in Windows 10 with: `Ctrl+Win+LeftArrow` and `Ctrl+Win+RightArrow`.


## Demo

![a demonstation of the solution](../assets/readme/demo.gif | width=400)

## Table of Contents

* [Installation](#installation)
* [Additional setup (Optional)](#additional-setup-optional)
* [Recomendations](#recomendations)
* [Known issues](#known-issues)
* [License](#license)

## Installation

* Install [AutoHotKey](https://www.autohotkey.com) **with UI-Access** (version 1.1.32 or a newer v1).  
![AutoHotKey installer|100x100,20%](../assets/readme/ahk-setup.png) 

> Without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps (Task Manager, HWInfo and others).

* Download [the SwitchApps.ahk script](https://github.com/dima-iholkin/SwitchApps/releases/latest).

* Start the downloaded `SwitchApps.ahk` script **with UI-Access**.  
![AutoHotKey script start](../assets/readme/ahk-start.png | width=200)  

## Additional setup (optional)

* Separate each app icon on the taskbar (to not group the icons) with [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker):  
Grouping > **Don't group**.  
![7+ Taskbar Tweaker settings](../assets/readme/7tt.png | width=200)

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
![Taskbar settings](../assets/readme/taskbar-settings.png  | width=200)

## Known issues

* The shortcut will not trigger in the very privileged apps, like an antivirus app (so the standard `Alt+Tab` behaviour will trigger).

* Occasionally it may not trigger or send an `Enter` or some other weird bit to Windows.

* It’s not perfect; it’s built on top of a few native things.

## Future plans

## Say hello (and get support)

## Contributing

## Support the author

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
