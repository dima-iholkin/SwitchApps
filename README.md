# SwitchApps

Maybe a better way to switch apps `[Alt+Tab]` for Windows 10.

This solution overrides the `Alt+Tab` and `Alt+Shift+Tab` shortcut behaviour.
Based on the Windows 10's `Win+T` shortcut.

When the AHK script is stopped, the standard `Alt+Tab` behaviour of Windows 10 resumes.

> The author was annoyed with the Windows 10 `Alt+Tab` behaviour of the shortcut and the other options,
and wanted a behaviour similar to:  
     * switching the tabs in Chrome with: `Ctrl+PageUp` and `Ctrl+PageDown`,  
     * switching the desktops in Windows 10 with: `Ctrl+Win+LeftArrow` and `Ctrl+Win+RightArrow`.

## Demo

The new behaviour of `Alt+Tab` and `Alt+Shift+Tab`:  
<img src="../assets/readme/demo.gif" width="600" title="the new behaviour of Alt+Tab and Alt+Shift+Tab">

## Table of Contents

  - [Install (start here)](#install-start-here)
  - [Additional setup (optional)](#additional-setup-optional)
  - [Known issues](#known-issues)
  - [Get Support and Contribute](#get-support-and-contribute)
  - [License](#license)

## Install (start here)

1. Install [AutoHotKey](https://www.autohotkey.com) **with UI-Access** (version 1.1.32 or a newer v1).  
> Without the UI-Access turned on, the shortcut won’t trigger in the more privileged apps (Task Manager, HWInfo and others).  

<img src="../assets/readme/ahk-setup.png" width="200" title="AutoHotKey installer">  


2. Download the `SwitchApps.ahk` [script](https://github.com/dima-iholkin/SwitchApps/releases/latest).

3. Start the downloaded `SwitchApps.ahk` script **with UI-Access**.  
<img src="../assets/readme/ahk-start.png" width="200" title="AutoHotKey script start">  

4. Try using `Alt+Tab`.  
> If you use `Alt+Shift+Tab` now, you'll probably get an MS Office pop up (see the solution to this below).

5. Enjoy.

## Additional setup (optional)

:exclamation: **The steps here can brick your OS.  
Don't do it if you're not experienced. You are the person responsible if you do it.**

---

* Increase the thumbnail preview size with [this solution](https://winaero.com/blog/change-taskbar-thumbnail-size-windows-10/). 

> I used the value of 800 to maximize the thumbnail size.  
> It certainly doesn’t scale to this size on my config - you may experiment with the value.

---

* Change the thumbnail preview delay to 0 with [this solution](https://www.tenforums.com/tutorials/21005-change-delay-time-show-taskbar-thumbnails-windows-10-a.html).

---

* Stop an MS Office ad pop up, when `Alt+Shift+Tab` is pressed, with [this solution](https://www.howtogeek.com/445318/how-to-remap-the-office-key-on-your-keyboard/):  
disable the `Office key` from opening the Office stuff, by adding to the registry

```powershell
REG ADD HKCU\Software\Classes\ms-officeapp\Shell\Open\Command /t REG_SZ /d rundll32
```

---

* Make each app icon on the taskbar separate with [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker):  
in the settings > Grouping: `Don't group`.  
<img src="../assets/readme/7tt.png" width="200" title="7+ Taskbar Tweaker settings">  

---

* To autostart the `SwitchApps.ahk` with UI-Access:  
1. Add to the Desktop a SwitchApps.bat file with contents:

```bat
start "C:\Program Files\AutoHotkey\AutoHotkeyU64_UIA.exe" "C:\[the location of the script file]\SwitchApps.ahk"
```

2. in **Task Scheduler** > **Create Task...** :  
   General tab: 
   * Name: > `[username] start SwitchApps`,
   * check `Run only when user is logged on`.
   Trigger tab: **New Trigger...** > Begin the task: `At log on`,  
   Actions tab: Action: `Start a program` > Program/script: `C:\Users\[username]\Desktop\SwitchApps.bat` or the other location of the `SwitchApps.bat` file,  
   Conditions tab: uncheck `Start the task only if the computer is on AC power`,  
   Settings tab: 
   * check `Allow task to be run on demand`, 
   * uncheck `Stop the task if it runs longer than:`,
   * uncheck `If the runnign task does not end when requested, force it to stop`,
   * in the bottom dropdown choose `Do not start a new instance`.

---

* If you’re using a multi-monitor setup:  
to show the apps on the taskbar of the same display only:  
Taskbar settings > Show taskbar buttons on: `Taskbar where window is open`  
<img src="../assets/readme/taskbar-settings.png" width="200" title="Taskbar settings">  

---

* Recommended to unpin all the pinned apps from the taskbar, maybe move them to Start or Desktop.

## Known issues

* The script will not trigger in the very privileged apps, like an antivirus app (the standard Windows 10 `Alt+Tab` shortcut would be run).

* Sometimes the script may not trigger. Sometimes it may send an `Enter` keypress to Windows.

## Get Support and Contribute

To get the support: You can open an issue or contact me.  

To contribute: Please contact me first. Then do the usual GitHub stuff.

## License

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2020 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
