using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using Serilog;



namespace SwitchApps.Library._Helpers
{


    public static class InstallerHelper
    {
        public static void InitializeStaticLogger(string installedDir)
        {
            string logPath = Path.Combine(
                installedDir,
                "log.txt"
            );

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logPath)
                .CreateLogger();
        }



        private static string _loginUserName = null;

        public static string LoginUsername
        {
            get
            {
                if (_loginUserName == null)
                {
                    ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");

                    ManagementObjectCollection collection = searcher.Get();

                    _loginUserName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
                }

                return _loginUserName;
            }
        }
        // Gets the login username, not the account under which this process runs.



        private static string _loginSID = null;

        public static string LoginSID
        {
            get
            {
                if (_loginSID == null)
                {
                    NTAccount account = new NTAccount(LoginUsername);

                    SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

                    _loginSID = sid.ToString();
                }

                return _loginSID;
            }
        }
        // Gets this login's SID.



        private static string _installedDir = null;

        public static string InstalledDir
        {
            get
            {
                if (_installedDir == null)
                {
                    _installedDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "SwitchApps"
                    );
                }

                return _installedDir;
            }
        }
    }
}