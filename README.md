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

## Installation

1. install AutoHotKey with UI-Access. (without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps, like Task Manager, HWInfo and others) [a picture of the installer menu and a URL to the AHK docs on the subject]
2. start the SwitchApps.ahk script (with “with UI-Access”) [a picture of the Windows 10 menu]
3. make each app window separate (with 7+ Taskbar Tweaker) [a picture of the selected settings and a URL to the app]
4. to stop the Office ad popping up, when pressing Alt+Shift+Tab with AHK started. [a URL to the solution and maybe a script]
5. to increase the thumbnail app preview size. (example: I personally use the value of 800; and it certainly doesn’t scale to this size - I’m maxxing it out for my config, your results may be different, you may experiment with it.) [a URL to the solution, an image of how it looks and maybe a PS script]
6. to autostart the SwitchApps.ahk with UI-Access. [figure out the best way to do it]
7. to change thumbnail delay to 0. [a URL to the solution and maybe a PS script]

## Recomendations

* If you’re using a multi-monitor setup: use the Windows’ taskbar setting: show apps only on the display they’re are on. [show the settings screenshot]

## Known issues

1. the shortcut will not trigger in the very privileged apps, like an antivirus app (so the standard `Alt+Tab` behaviour will trigger).
2. occasionally it may not trigger or send an `Enter` or some other weird bit to Windows.
3. it’s not perfect; it’s built on top of a few native things.
not a lot can be improved from the author’s perspective; it doesn’t have access to the UI information.

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
