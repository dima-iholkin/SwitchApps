# SwitchApps

A better way to switch apps (Alt+Tab) for Windows 10.

This solution overrides the `Alt+Tab`, `Alt+Shift+Tab` shortcut behaviour.  
When the AHK script is stopped, the standard `Alt+Tab` behaviour of Windows 10 resumes.

The solution is based on the original `Win+T` shortcut.

> The author was annoyed with the Windows10’s switching app behaviours with the `Alt+Tab` shortcut and the other possibilities; and wanted a behaviour similar to:
>
> * switching the tabs in a web browser, in Chrome and Opera it's: `Ctrl+PageUp`, `Ctrl+PageDown`,
> * switching the desktops in WIndows 10 with `Ctrl+Win+LeftArrow`, `Ctrl+Win+RightArrow`.

**how it works**  
![example how it works](https://example.com/example.gif)

## Table of Contents

* [Installation](#installation)
* [Recomendations](#recomendations)
* [Known issues](#known-issues)
* [License](#license)

## Basic set up

* Install [AutoHotKey](https://www.autohotkey.com) (version 1.1.32 or newer v1) **with UI-Access**.  
[a picture of the installer menu]  

> Without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps (Task Manager, HWInfo and others).

* Download the `SwitchApps.ahk` script.  
[a link to the releases]

* Start the downloaded `SwitchApps.ahk` script **with UI-Access**.  
[a picture of the Windows 10 menu]

## Additional set up (Optional)

* To make each app icon on the taskbar separate (to not group the icons):  
install [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker),  
in it's Settings select Grouping > **Don't group**.  
[a picture of the selected settings]

* To stop an Office ad popping up, when `Alt+Shift+Tab` is pressed:  
disable the `Office` key is press from opening an Office app, by modifying the Registry with a PowerShell script [from this article](https://www.howtogeek.com/445318/how-to-remap-the-office-key-on-your-keyboard/):

```powershell
REG ADD HKCU\Software\Classes\ms-officeapp\Shell\Open\Command /t REG_SZ /d rundll32
```

* to increase the thumbnail app preview size.  
[a URL to the solution]  
[an image of how it looks]  
[maybe a PS script]

> Me personally use the value of 800. It certainly doesn’t scale to this size on my config - your results may differ, you can experiment with the value.

* to change thumbnail delay to 0.  
[a URL to the solution and maybe a PS script]

* to autostart the SwitchApps.ahk with UI-Access.  
[figure out the best way to do it]

## Recomendations

* If you’re using a multi-monitor setup: use the Windows’ taskbar setting: show apps only on the display they’re are on.  
[show the settings screenshot]

## Known issues

* The shortcut will not trigger in the very privileged apps, like an antivirus app (so the standard `Alt+Tab` behaviour will trigger).

* Occasionally it may not trigger or send an `Enter` or some other weird bit to Windows.

* It’s not perfect; it’s built on top of a few native things.

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
