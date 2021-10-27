using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using Serilog;
using Serilog.Core;



namespace SwitchApps.Library
{


    public static class InstallerHelper
    {
        public static Logger InitializeLogger(string installedDir)
        {
            string logPath = Path.Combine(
                installedDir,
                "log.txt"
            );

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logPath)
                .CreateLogger();
        }



        public static string GetLoginUsername()
        {
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            
            ManagementObjectCollection collection = searcher.Get();
            
            return (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        }
        // Gets the login username, not the account under which this process runs.



        public static string GetLoginSID(string loginUsername)
        {
            NTAccount account = new NTAccount(loginUsername);
            
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            
            return sid.ToString();
        }
        // Gets this login's SID.
    }
}