using Microsoft.Win32;



namespace SwitchApps.Library.Registry.Singletons
{


    public static class BackupSubkey
    {
        private static RegistryKey _instance = null;

        public static RegistryKey Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = SoftwareSubkey.Instance
                        .CreateSubKey(@"SwitchApps\Backup");
                }

                return _instance;
            }
        }
    }
}