# SwitchApps

Maybe a better way to switch apps (Alt+Tab) for Windows 10.

This solution overrides the `Alt+Tab` and `Alt+Shift+Tab` shortcut behaviour.
Based on the Windows 10's `Win+T` shortcut.

When the AHK script is stopped, the standard `Alt+Tab` behaviour of Windows 10 resumes.

> The author was annoyed with the Windows 10 `Alt+Tab` behaviour of the shortcut and the other options,
and wanted a behaviour similar to:  
     * switching the tabs in Chrome with: `Ctrl+PageUp` and `Ctrl+PageDown`,  
     * switching the desktops in Windows 10 with: `Ctrl+Win+LeftArrow` and `Ctrl+Win+RightArrow`.

## Demo

This all is done by using just `Alt+Tab` and `Alt+Shift+Tab`:  
<img src="../assets/readme/demo.gif" width="600" title="a demonstation of the solution">

## Table of Contents

* [Installation](#installation)
* [Additional setup (optional)](#additional-setup-optional)
* [Recomendations](#recomendations)
* [Known issues](#known-issues)
* [Support](#support)
* [Contribute](#contribute)
* [Donate](#donate)
* [License](#license)

## Installation

1. Install [AutoHotKey](https://www.autohotkey.com) **with UI-Access** (version 1.1.32 or a newer v1).  
<img src="../assets/readme/ahk-setup.png" width="200" title="AutoHotKey installer">  

> Without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps (Task Manager, HWInfo and others).

2. Download the `SwitchApps.ahk` [script](https://github.com/dima-iholkin/SwitchApps/releases/latest).

3. Start the downloaded `SwitchApps.ahk` script **with UI-Access**.  
<img src="../assets/readme/ahk-start.png" width="200" title="AutoHotKey script start">  

## Additional setup (optional)

* Increase the thumbnail preview size with [this solution](https://winaero.com/blog/change-taskbar-thumbnail-size-windows-10/). 

> I used the value of 800 to maximize the thumbnail size.  
> It certainly doesn’t scale to this size on my config - you may experiment with the value.

* Change the thumbnail preview delay to 0 with [this solution](https://www.tenforums.com/tutorials/21005-change-delay-time-show-taskbar-thumbnails-windows-10-a.html).

* Stop an Office ad pop up, when `Alt+Shift+Tab` is pressed, with [this solution](https://www.howtogeek.com/445318/how-to-remap-the-office-key-on-your-keyboard/):  
disable the `Office key` from opening the Office stuff, by adding to the registry

```powershell
REG ADD HKCU\Software\Classes\ms-officeapp\Shell\Open\Command /t REG_SZ /d rundll32
``` 

* Make each app icon on the taskbar separate with [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker):  
in settings: Grouping > **Don't group**.  
<img src="../assets/readme/7tt.png" width="200" title="7+ Taskbar Tweaker settings">  

* Autostart the `SwitchApps.ahk` with UI-Access.  
[TODO:]

## Recomendations

* If you’re using a multi-monitor setup:  
show the apps only on the taskbar of the same display:  
Taskbar settings > Show taskbar buttons on > Taskbar where window is open  
<img src="../assets/readme/taskbar-settings.png" width="200" title="Taskbar settings">  

* Recommendation to unpin all the pinned apps from the taskbar, maybe move them to Start or Desktop.

## Known issues

* The shortcut will not trigger in the very privileged apps, like an antivirus app (so the standard `Alt+Tab` behaviour will trigger).

* Sometimes it may not trigger or send an `Enter` or some other weird bit to Windows.

## Future plans

No plans to unlock any new functionality.

## Support

You can open an issue or contact me at:  
[TODO: email, some real-time messenger...]

## Contribute

Probably you should contact me first.  
Make PRs, open issues, do the usual stuff - I'm a rookie here.

## Donate

Maybe you're comfortable supporting the author:  
[TODO:]

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
