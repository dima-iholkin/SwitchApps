using System;
using Microsoft.Win32;



namespace SwitchApps.Library.Registry._Helpers
{


    public static class RegistryHelper
    {
        public static bool ValuesEqual(
            object obj1,
            object obj2,
            RegistryValueKind registryValueKind
        )
        {
            switch (registryValueKind)
            {
                case RegistryValueKind.String:
                    return obj1.ToString() == obj2.ToString();
                case RegistryValueKind.DWord:
                    return (int)obj1 == (int)obj2;
                default:
                    throw new ArgumentException("Expected only String and DWord values.");
            }
        }



        public static void RestoreDefaultValue(
            RegistryItem registryItem,
            RegistryKey _softwareKey
        )
        {
            _softwareKey
                .CreateSubKey(registryItem.Path)
                .DeleteValue(registryItem.Name);
        }
    }
}