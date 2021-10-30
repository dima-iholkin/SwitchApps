using System;
using Microsoft.Win32;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;

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



        public static bool MainValueEqualsDesiredValue(
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



        public static bool BackupEntryExists(
            this RegistryItem registryItem,
            RegistryKey backupSubkey,
            Logger logger
        )
        {
            object _mainEntryWasPresentObj = backupSubkey.GetValue(registryItem.IsPresent_Name);
            if (_mainEntryWasPresentObj == null)
            {
                return false;
            }
            // If the backup entry not fount.

            int _mainEntryWasPresentInt;
            try
            {
                _mainEntryWasPresentInt = (int)_mainEntryWasPresentObj;
            }
            catch (InvalidCastException)
            {
                logger.Warning(
                    "{RegistryEntryName} IsPresent backup entry wasn't an Integer with value {BackupEntryValue}.",
                    registryItem.Name,
                    _mainEntryWasPresentObj.ToString()
                );

                throw new BackupRegistryRecordCorruptedException();
            }

            bool mainEntryWasPresent;
            try
            {
                mainEntryWasPresent = _mainEntryWasPresentInt.ConvertToBool();
            }
            catch (ArgumentOutOfRangeException)
            {
                logger.Warning(
                        "{RegistryEntryName} IsPresent backup entry was not 0 or 1 with value {BackupEntryValue}.",
                        registryItem.Name,
                        _mainEntryWasPresentInt
                    );

                throw new BackupRegistryRecordCorruptedException();
            }

            if (mainEntryWasPresent)
            {
                object backupValue = backupSubkey.GetValue(registryItem.Name);
                if (backupValue == null)
                {
                    logger.Warning(
                        "{RegistryEntryName} Value backup entry was not found.",
                        registryItem.Name,
                        _mainEntryWasPresentObj.ToString()
                    );

                    throw new BackupRegistryRecordCorruptedException();
                }
            }

            return mainEntryWasPresent;
        }



        private static bool ConvertToBool(this int _int)
        {
            switch (_int)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
                    // Normally isPresent value can be only 0, 1 or null (when not fould).
                    // Meaning if the code got here, something probably had become incorrect.
            }
        }



        public static void ResetMainEntryToSystemDefaultValue(
            this RegistryItem registryItem,
            RegistryKey softwareSubkey,
            Logger logger
        )
        {
            logger.Verbose("{RegistryItem_Name} backup registry corruption detected.", registryItem.CustomName);

            object mainValue = registryItem.GetMainValue(softwareSubkey);

            bool mainValueEqualsDesired = registryItem.MainValueEqualsDesired(softwareSubkey);

            logger.Verbose(
                "{RegistryItem_Name} main value {MainValue} equals the desired value: {MainValueEqualsDesired}.",
                registryItem.CustomName,
                mainValue,
                mainValueEqualsDesired
            );

            if (mainValueEqualsDesired)
            {
                registryItem.RestoreDefaultValue(softwareSubkey);

                logger.Verbose(
                    "{RegistryItem_Name} defaults restored - name deleted.",
                    registryItem.CustomName
                );
            }
            else
            {
                logger.Verbose("{RegistryItem_Name} main value left untouched.", registryItem.CustomName);

                return;
            }
        }



        public static void CreateBackupEntry(
            this RegistryItem ri,
            RegistryKey softwareSubkey,
            RegistryKey backupSubkey,
            Logger logger
        )
        {
            using (RegistryKey itemSubkey = softwareSubkey.OpenSubKey(ri.Path))
            {
                object value = itemSubkey?.GetValue(ri.Name);

                ri.CreateBackupEntry(
                    value,
                    backupSubkey,
                    logger
                );
            }
        }



        public static void CreateBackupEntry(
            this RegistryItem ri,
            object backupValue,
            RegistryKey backupSubkey,
            Logger logger
        )
        {
            if (backupValue == null)
            {
                backupSubkey.SetValue(ri.IsPresent_Name, 0, RegistryValueKind.DWord);

                logger.Verbose(
                    "{RegistryItem_Name} not present in the main registry. 0 written into the backup registry.",
                    ri.CustomName
                );

                backupSubkey.DeleteValue(
                    ri.CustomName,
                    throwOnMissingValue: false
                );

                logger.Verbose(
                    "{RegistryItem_Name} deleted from the backup registry.",
                    ri.CustomName
                );
                // If the Name exists inside the backup registry, remove it.
            }
            else
            {
                backupSubkey.SetValue(ri.IsPresent_Name, 1, RegistryValueKind.DWord);

                logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    ri.CustomName
                );

                backupSubkey.SetValue(ri.CustomName, backupValue, ri.ValueKind);

                logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    ri.CustomName,
                    backupValue
                );
            }
        }
    }
}