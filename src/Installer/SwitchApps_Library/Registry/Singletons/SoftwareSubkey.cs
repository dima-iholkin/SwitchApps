using Microsoft.Win32;
using SwitchApps.Library._Helpers;



namespace SwitchApps.Library.Registry.Singletons
{


    public static class SoftwareSubkey
    {
        private static RegistryKey _instance = null;

        public static RegistryKey Instance
        {
            get
            {
                if (_instance == null)
                {
                    string loginSID = InstallerHelper.LoginSID;

                    _instance = Microsoft.Win32.Registry.Users
                        .CreateSubKey(loginSID)
                        .CreateSubKey("SOFTWARE");
                }

                return _instance;
            }
        }
    }
}