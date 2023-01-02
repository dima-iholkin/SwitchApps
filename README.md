# <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Flag_of_Ukraine.svg/1920px-Flag_of_Ukraine.svg.png" width="32" alt="Ukrainian flag"> SwitchApps utility for Windows 10/11



A new behavior for `Alt + Tab` and `Alt + Shift + Tab` keyboard shortcuts for Windows 10/11. It uses the current Taskbar order to switch between apps.

![SwitchApps demo GIF](/../assets/readme/demo.gif?raw=true "SwitchApps demo GIF")  



## Install instructions

* Recommended to **unpin every app** from the Taskbar, because it interferes with the intended user experience of switching between open apps through the Taskbar. Better to use a diffenent approach to open common apps, for example pin to the Start Menu or create shortcuts on the Desktop.

1. Download and install [the latest SwitchApps release](https://github.com/dima-iholkin/SwitchApps/releases/latest), choose the installer that best suits you.

2. For Windows 10 use [7+ Taskbar Tweaker](https://rammichael.com/7-taskbar-tweaker) to disable the Taskbar app grouping.  
Choose the **Run at startup** option during the installation.  
![7+ Taskbar Tweaker the Run at startup option screenshot](/_docs/_assets/04_7tt_autostart.png?raw=true)  
Choose the **Don't group apps** option in the app.  
![7+ Taskbar Tweaker the Don't group apps option screenshot](/../assets/readme/7tt.png?raw=true)  

* For Windows 11 you can try this [Windhawk mod](https://windhawk.net/mods/taskbar-grouping) or another solution, if they exist, to disable the Taskbar app grouping.

3. Restart the computer for all changes to apply.

4. Open some apps and try pressing `Alt + Tab` and `Alt + Shift + Tab`.



## Limitations:

* Rarely the SwitchApps may send an `Enter` keypress to the active app, which may result in some horrible consequences, so be mindful.

* SwitchApps may not work well during an extreme CPU load or the utility's cold memory state. It will resolve by itself.

* Some games and especially the full-screen mode games may conflict with the utility. You can stop the utility during the gameplay from the Start Menu.  
![Stop SwitchApps from the Start Menu screenshot](/_docs/_assets/02_StartMenu.png?raw=true)

* Windows 11's Microsoft Edge seems to ignore the utility, so an `Alt + Tab` keypress there triggers the default Windows 11 behavior.



## Get Support and Contribute

Please create an issue or discussion, if you noticed a bug or have questions.



## License

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright 2021 © <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.
