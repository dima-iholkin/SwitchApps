﻿using System;
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
            using (var itemSubkey = softwareSubkey.CreateSubKey(ri.MainEntryPath))
            {
                return itemSubkey.GetValue(ri.MainEntryName);
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
            using (var itemSubkey = softwareSubkey.CreateSubKey(ri.MainEntryPath))
            {
                itemSubkey.DeleteValue(
                    ri.MainEntryName,
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
            bool mainEntryWasPresent = registryItem.GetWasPresentBackupEntryValue(
                backupSubkey,
                logger
            );

            //object _mainEntryWasPresentObj = backupSubkey.GetValue(registryItem.GetBackupEntry_WasPresentName);
            //if (_mainEntryWasPresentObj == null)
            //{
            //    return false;
            //}
            //// If the backup entry not fount.

            //int _mainEntryWasPresentInt;
            //try
            //{
            //    _mainEntryWasPresentInt = (int)_mainEntryWasPresentObj;
            //}
            //catch (InvalidCastException)
            //{
            //    logger.Warning(
            //        "{RegistryEntryName} IsPresent backup entry wasn't an Integer with value {BackupEntryValue}.",
            //        registryItem.MainEntryName,
            //        _mainEntryWasPresentObj.ToString()
            //    );

            //    throw new BackupRegistryRecordCorruptedException();
            //}

            //bool mainEntryWasPresent;
            //try
            //{
            //    mainEntryWasPresent = _mainEntryWasPresentInt.ConvertToBool();
            //}
            //catch (ArgumentOutOfRangeException)
            //{
            //    logger.Warning(
            //            "{RegistryEntryName} IsPresent backup entry was not 0 or 1 with value {BackupEntryValue}.",
            //            registryItem.MainEntryName,
            //            _mainEntryWasPresentInt
            //        );

            //    throw new BackupRegistryRecordCorruptedException();
            //}

            if (mainEntryWasPresent)
            {
                object backupEntryValue = backupSubkey.GetValue(registryItem.MainEntryName);

                if (backupEntryValue == null)
                {
                    logger.Warning(
                        "{RegistryEntryName} Value backup entry was not found.",
                        registryItem.MainEntryName
                    );

                    throw new BackupRegistryRecordCorruptedException();
                }
            }

            return true;
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
            logger.Verbose("{RegistryItem_Name} backup registry corruption detected.", registryItem.BackupEntryName);

            object mainValue = registryItem.GetMainValue(softwareSubkey);

            bool mainValueEqualsDesired = registryItem.MainValueEqualsDesired(softwareSubkey);

            logger.Verbose(
                "{RegistryItem_Name} main value {MainValue} equals the desired value: {MainValueEqualsDesired}.",
                registryItem.BackupEntryName,
                mainValue,
                mainValueEqualsDesired
            );

            if (mainValueEqualsDesired)
            {
                registryItem.RestoreDefaultValue(softwareSubkey);

                logger.Verbose(
                    "{RegistryItem_Name} defaults restored - name deleted.",
                    registryItem.BackupEntryName
                );
            }
            else
            {
                logger.Verbose("{RegistryItem_Name} main value left untouched.", registryItem.BackupEntryName);

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
            using (RegistryKey itemSubkey = softwareSubkey.OpenSubKey(ri.MainEntryPath))
            {
                object value = itemSubkey?.GetValue(ri.MainEntryName);

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
                backupSubkey.SetValue(ri.GetBackupEntry_WasPresentName, 0, RegistryValueKind.DWord);

                logger.Verbose(
                    "{RegistryItem_Name} not present in the main registry. 0 written into the backup registry.",
                    ri.BackupEntryName
                );

                backupSubkey.DeleteValue(
                    ri.BackupEntryName,
                    throwOnMissingValue: false
                );

                logger.Verbose(
                    "{RegistryItem_Name} deleted from the backup registry.",
                    ri.BackupEntryName
                );
                // If the Name exists inside the backup registry, remove it.
            }
            else
            {
                backupSubkey.SetValue(ri.GetBackupEntry_WasPresentName, 1, RegistryValueKind.DWord);

                logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    ri.BackupEntryName
                );

                backupSubkey.SetValue(ri.BackupEntryName, backupValue, ri.ValueKind);

                logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    ri.BackupEntryName,
                    backupValue
                );
            }
        }



        public static bool GetWasPresentBackupEntryValue(
            this RegistryItem ri,
            RegistryKey backupSubkey,
            Logger logger
        )
        {


            int _mainEntryWasPresentInt;
            try
            {
                _mainEntryWasPresentInt = (int)_mainEntryWasPresentObj;
            }
            catch (InvalidCastException)
            {
                logger.Warning(
                    "{RegistryEntryName} IsPresent backup entry wasn't an Integer with value {BackupEntryValue}.",
                    ri.MainEntryName,
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
                    ri.MainEntryName,
                    _mainEntryWasPresentInt
                );

                throw new BackupRegistryRecordCorruptedException();
            }

            return mainEntryWasPresent;
        }



        public static bool WasPresentBackupEntryExists(
            this RegistryItem ri,
            RegistryKey backupSubkey
        )
        {
            object entryValue = backupSubkey.GetValue(ri.GetBackupEntry_WasPresentName);

            if (entryValue != null)
            {
                return true;
            }
            else
            {
                return false;
                // If the backup entry not fount.
            }
        }
    }
}