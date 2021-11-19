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
            link.SetDescription("Start SwitchApps");
            link.SetPath(@"C:\Windows\System32\schtasks.exe");
            link.SetArguments(@"/Run /TN ""\SwitchApps\SwitchApps autostart"""
            );
            link.SetIconLocation(exePath, 0);
            // Save shortcut:
            IPersistFile file = (IPersistFile)link;
            file.Save(Path.Combine(appStartMenuDir, "Start SwitchApps.lnk"), false);

            IShellLink link2 = (IShellLink)new ShellLink();
            // Shortcut information:
            link2.SetDescription("Stop SwitchApps");
            link2.SetPath(@"C:\Windows\System32\schtasks.exe");
            link2.SetArguments(@"/End /TN ""\SwitchApps\SwitchApps autostart""");
            link2.SetIconLocation(exePath, 0);
            // Save shortcut:
            IPersistFile file2 = (IPersistFile)link2;
            file2.Save(Path.Combine(appStartMenuDir, "Stop SwitchApps.lnk"), false);

            IShellLink link3 = (IShellLink)new ShellLink();
            // Shortcut information:
            link3.SetDescription("Enable autostart SwitchApps");
            link3.SetPath(Path.Combine(
                InstallerHelper.InstalledDir,
                "Enable autostart.bat"
            ));
            // Save shortcut:
            IPersistFile file3 = (IPersistFile)link3;
            file3.Save(Path.Combine(appStartMenuDir, "Enable autostart.lnk"), false);
            using (var fs = new FileStream(
                Path.Combine(appStartMenuDir, "Enable autostart.lnk"),
                FileMode.Open,
                FileAccess.ReadWrite
            ))
            {
                fs.Seek(21, SeekOrigin.Begin);
                fs.WriteByte(0x22);
            }

            IShellLink link4 = (IShellLink)new ShellLink();
            // Shortcut information:
            link4.SetDescription("Disable autostart SwitchApps");
            link4.SetPath(Path.Combine(
                InstallerHelper.InstalledDir,
                "Disable autostart.bat"
            ));
            // Save shortcut:
            IPersistFile file4 = (IPersistFile)link4;
            file4.Save(Path.Combine(appStartMenuDir, "Disable autostart.lnk"), false);
            using (var fs = new FileStream(
                Path.Combine(appStartMenuDir, "Disable autostart.lnk"),
                FileMode.Open,
                FileAccess.ReadWrite
            ))
            {
                fs.Seek(21, SeekOrigin.Begin);
                fs.WriteByte(0x22);
            }

            IShellLink link5 = (IShellLink)new ShellLink();
            // Shortcut information:
            link5.SetDescription("Uninstall SwitchApps");
            link5.SetPath(Path.Combine(
                InstallerHelper.InstalledDir,
                "Uninstall.bat"
            ));
            string uninstallIconPath = Path.Combine(
                InstallerHelper.InstalledDir,
                "Icon_Uninstall.ico"
            );
            link5.SetIconLocation(uninstallIconPath, 0);
            // Save shortcut:
            IPersistFile file5 = (IPersistFile)link5;
            file5.Save(Path.Combine(appStartMenuDir, "Uninstall.lnk"), false);
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