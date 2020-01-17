# SwitchApps

> A better way to switch apps (Alt+Tab) for Windows 10.

This solution overrides the `Alt+Tab` and `Alt+Shift+Tab` shortcut behaviour.

When the AHK script is stopped, the standard `Alt+Tab` behaviour of Windows 10 resumes.

Based on the Windows 10's `Win+T` shortcut.

**how it works**  
![example how it works](https://example.com/example.gif)

## Table of Contents

* [Basic set up](#basic-set-up)
* [Additional set up (Optional)](#additional-set-up-optional)
* [Recomendations](#recomendations)
* [Known issues](#known-issues)
* [License](#license)

## Basic set up

* Install [AutoHotKey](https://www.autohotkey.com) **with UI-Access** (version 1.1.32 or newer v1).  
[a picture of the installer menu]  

> Without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps (Task Manager, HWInfo and others).

* Download the `SwitchApps.ahk` script.  
[a link to the releases]

* Start the downloaded `SwitchApps.ahk` script **with UI-Access**.  
[a picture of the Windows 10 menu]

## Additional set up (Optional)

* Separate each app icon on the taskbar (to not group the icons):  
install [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker),  
in it's Settings select Grouping > **Don't group**.  
[a picture of the selected settings]

* Stop an Office pop up, when `Alt+Shift+Tab` is pressed:  
disable the `Office` key is press from opening an Office app, by modifying the Registry with a PowerShell script [from this article](https://www.howtogeek.com/445318/how-to-remap-the-office-key-on-your-keyboard/):

```powershell
REG ADD HKCU\Software\Classes\ms-officeapp\Shell\Open\Command /t REG_SZ /d rundll32
```

* Increase the thumbnail preview size.  
[a URL to the solution]  
[an image of how it looks]  
[maybe a PS script]

> Me personally use the value of 800. It certainly doesn’t scale to this size on my config - your results may differ, you can experiment with the value.

* Change the thumbnail preview delay to 0.  
[a URL to the solution and maybe a PS script]

* Autostart the `SwitchApps.ahk` with UI-Access.  
[figure out the best way to do it]

## Recomendations

* If you’re using a multi-monitor setup, show the apps in the taskbar of only the display they are in:  
Settings > blah blah 
[show the settings screenshot]

## Known issues

* The shortcut will not trigger in the very privileged apps, like an antivirus app (so the standard `Alt+Tab` behaviour will trigger).

* Occasionally it may not trigger or send an `Enter` or some other weird bit to Windows.

* It’s not perfect; it’s built on top of a few native things.

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
