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
            link.SetDescription("Start SwitchApps utility");
            link.SetPath(@"C:\Windows\System32\schtasks.exe");
            link.SetArguments(@"/Run /TN ""\SwitchApps\SwitchApps autostart"""
            );
            link.SetIconLocation(exePath, 0);
            // Save shortcut:
            IPersistFile file = (IPersistFile)link;
            file.Save(Path.Combine(appStartMenuDir, "Start SwitchApps utility.lnk"), false);

            IShellLink link2 = (IShellLink)new ShellLink();
            // Shortcut information:
            link2.SetDescription("Stop SwitchApps utility");
            link2.SetPath(@"C:\Windows\System32\schtasks.exe");
            link2.SetArguments(@"/End /TN ""\SwitchApps\SwitchApps autostart""");
            link2.SetIconLocation(exePath, 0);
            // Save shortcut:
            IPersistFile file2 = (IPersistFile)link2;
            file2.Save(Path.Combine(appStartMenuDir, "Stop SwitchApps utility.lnk"), false);

            IShellLink link3 = (IShellLink)new ShellLink();
            // Shortcut information:
            link3.SetDescription("Enable SwitchApps autostart");
            link3.SetPath(@"C:\Windows\System32\schtasks.exe");
            link3.SetArguments(@"/Change /TN ""\SwitchApps\SwitchApps autostart"" /Enable");
            link3.SetIconLocation(exePath, 0);
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
            link4.SetDescription("Disable SwitchApps autostart");
            link4.SetPath(@"C:\Windows\System32\schtasks.exe");
            link4.SetArguments(@"/Change /TN ""\SwitchApps\SwitchApps autostart"" /Disable");
            link4.SetIconLocation(exePath, 0);
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