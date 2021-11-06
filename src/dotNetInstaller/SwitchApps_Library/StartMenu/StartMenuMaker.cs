using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using SwitchApps.Library._Helpers;
using SwitchApps.Library.StartMenu._Helpers;



namespace SwitchApps.Library.StartMenu
{


    public class StartMenuMaker
    {
        public StartMenuMaker() { }



        public void CreateShortcuts()
        {
            string appStartMenuDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "SwitchApps"
            );
            Directory.CreateDirectory(appStartMenuDir);

            string exePath = Path.Combine(
                InstallerHelper.InstalledDir,
                "SwitchApps.exe"
            );

            IShellLink link = (IShellLink)new ShellLink();
            // Shortcut information:
            link.SetDescription("SwitchApps start");
            link.SetPath(@"C:\Windows\System32\schtasks.exe");
            link.SetArguments(@"/run /tn ""\SwitchApps\SwitchApps autostart""");
            link.SetIconLocation(exePath, 0);
            // Save shortcut:
            IPersistFile file = (IPersistFile)link;
            file.Save(Path.Combine(appStartMenuDir, "SwitchApps start.lnk"), false);

            IShellLink link2 = (IShellLink)new ShellLink();
            // Shortcut information:
            link2.SetDescription("SwitchApps stop");
            link2.SetPath(@"C:\Windows\System32\schtasks.exe");
            link2.SetArguments(@"/end /tn ""\SwitchApps\SwitchApps autostart""");
            link2.SetIconLocation(exePath, 0);
            // Save shortcut:
            IPersistFile file2 = (IPersistFile)link2;
            file2.Save(Path.Combine(appStartMenuDir, "SwitchApps stop.lnk"), false);
        }


        public void DeleteShortcuts()
        {
            string appStartMenuDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "SwitchApps"
            );
            Directory.Delete(
                appStartMenuDir,
                recursive: true
            );
        }
    }
}