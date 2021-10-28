using System;
using Microsoft.Win32;



namespace SwitchApps.Library.Registry.Extensions
{


    public static class RegistryItemExtensions
    {
        public static object GetMainValue(
            this RegistryItem ri,
            RegistryKey softwareSubkey
        )
        {
            using (var itemSubkey = softwareSubkey.CreateSubKey(ri.Path))
            {
                return itemSubkey.GetValue(ri.Name);
            }
        }



        public static bool MainValueEqualsDesired(
            this RegistryItem ri,
            RegistryKey softwareSubkey
        )
        {
            object mainValue = ri.GetMainValue(softwareSubkey);

            return mainValue.ValueEquals(
                ri.DesiredValue,
                ri.ValueKind
            );
        }



        public static bool MainValueEqualsDesired(
            this RegistryItem ri,
            object mainValue
        )
        {
            return mainValue.ValueEquals(
                ri.DesiredValue,
                ri.ValueKind
            );
        }



        public static bool MainValueEqualsDesiredOrNull(
            this RegistryItem ri,
            RegistryKey softwareSubkey
        )
        {
            object mainValue = ri.GetMainValue(softwareSubkey);

            if (mainValue == null)
            {
                return true;
            }

            return mainValue.ValueEquals(
                ri.DesiredValue,
                ri.ValueKind
            );
        }



        public static void RestoreDefaultValue(
            this RegistryItem ri,
            RegistryKey softwareSubkey
        )
        {
            using (var itemSubkey = softwareSubkey.CreateSubKey(ri.Path))
            {
                itemSubkey.DeleteValue(
                    ri.Name,
                    throwOnMissingValue: false
                );
            }
        }



        public static bool ValueEquals(
            this object obj1,
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
    }
}