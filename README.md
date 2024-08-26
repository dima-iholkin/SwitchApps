# <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Flag_of_Ukraine.svg/1920px-Flag_of_Ukraine.svg.png" width="32" alt="Ukrainian flag"> SwitchApps utility for Windows 10/11

SwitchApps utility for Windows 10/11 changes the behavior of `Alt + Tab` and `Alt + Shift + Tab` keyboard shortcuts, to a behavior based on the order of apps on the Taskbar. 

> This means you finally have a predictable order of switching between apps!

If you're familiar with switching between the tabs in your web browser with keyboard shortcuts `Ctrl + PageUp` and `Ctrl + PageDown` - this is a very similar idea. Under the hood it utilizes the `Win + T` keyboard shortcut behavior.

![Screen capture of SwitchApps usage](/../assets/readme/demo.gif?raw=true "Screen capture of SwitchApps usage")  



## Recommendations

1. Unpin the apps from the Taskbar for the best user experience. 
2. I believe it's best to disable the app grouping behavior on the Taskbar with:
    * [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker) for Windows 10,
    * [Windhawk Mods](https://windhawk.net/mods/taskbar-grouping) for Windows 11.

However you can try it out without any of these changes.



## Install instructions

1. Download and install from [the Releases page](https://github.com/dima-iholkin/SwitchApps/releases/latest).
2. Recommended to disable the app grouping behavior with 7+ Taskbar Tweaker or Windhawk Mods.
3. Restart the computer for all changes to apply.



## Support and Contributions

If you encounter any problems or have questions, please contact me [on LinkedIn](linkedin.com/in/dima-iholkin/) or create an Issue/Discussion on GitHub.



## Technical details

The keyboard shortcut behaviors are implemented with an AutoHotKey script.  
Then this script is packaged into an executable by AutoHotKey tool.  
The installed is created with Microsoft Visual Studio Installer Projects, with custom C# .NET Framework 4.7 code for additional steps during install and uninstall.  

During install:
* a Task Scheduler task for autostart is created,
* a couple of Registry keys are backed up and edited for the best user experience,
* a couple Start Menu shortcuts created to give a user the best control of the utility.

During uninstall:
* the Task Scheduler task will be removed,
* the Registry keys will be restored to the original (backed-up) values,
* the Start Menu shortcuts will be removed.

The installer logs into a file in the installation directory.

```
C:\Users\[username]\AppData\Roaming\SwitchApps
```

The installer requires admin privileges for the best user experience, therefore you may not be able to install the app in some environments due to security concerns.



## License information

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2021-2024 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.