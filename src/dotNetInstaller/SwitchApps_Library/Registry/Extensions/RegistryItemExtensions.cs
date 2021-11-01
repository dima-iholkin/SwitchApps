using System;
using Microsoft.Win32;
using OneOf;
using OneOf.Types;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;

namespace SwitchApps.Library.Registry.Extensions
{


    public static class RegistryItemExtensions
    {
        //public static object GetMainValue(
        //    this RegistryItem ri,
        //    RegistryKey softwareSubkey
        //)
        //{
        //    using (var itemSubkey = softwareSubkey.CreateSubKey(ri.MainEntryPath))
        //    {
        //        return itemSubkey.GetValue(ri.MainEntryName);
        //    }
        //}



        public static OneOf<int, string, NotFound> GetMainValue(
            this RegistryItem ri,
            RegistryKey softwareSubkey
        )
        {
            object _mainValueObj;
            using (var itemSubkey = softwareSubkey.CreateSubKey(ri.MainEntryPath))
            {
                _mainValueObj = itemSubkey.GetValue(ri.MainEntryName);
            };

            switch (_mainValueObj)
            {
                case String s:
                    return s;
                case Int32 i:
                    return i;
                case null:
                    return new NotFound();
                default:
                    throw new Exception(
                        $"Unexpected value {_mainValueObj} from the main registry entry {nameof(ri.BackupEntryName)}."
                    );
            };
        }



        public static bool MainValueEqualsDesired(
            this RegistryItem ri,
            RegistryKey softwareSubkey
        )
        {
            object mainValue = ri.GetMainValue(softwareSubkey);

            return mainValue.ValueEquals(
                ri.DesiredValue,
                ri.GetValueKind()
            );
        }



        public static bool MainValueEqualsDesiredValue(
            this RegistryItem ri,
            object mainValue
        )
        {
            return mainValue.ValueEquals(
                ri.DesiredValue,
                ri.GetValueKind()
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
                ri.GetValueKind()
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


        /*
        public static bool BackupEntryExists(
            this RegistryItem registryItem,
            RegistryKey backupSubkey,
            Logger logger
        )
        {
            bool? mainEntryWasPresent = registryItem.GetWasPresentValue(
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

            if (mainEntryWasPresent == null)
            {

            }

            if (mainEntryWasPresent.Value == true)
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
        */


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



        public static void ResetMainEntryToSystemDefaultValue(this RegistryItem registryItem)
        {
            Log.Logger.Verbose("{RegistryItem_Name} backup registry corruption detected.", registryItem.BackupEntryName);

            object mainValue = registryItem.GetMainValue(SoftwareSubkey.Instance);

            bool mainValueEqualsDesired = registryItem.MainValueEqualsDesired(SoftwareSubkey.Instance);

            Log.Logger.Verbose(
                "{RegistryItem_Name} main value {MainValue} equals the desired value: {MainValueEqualsDesired}.",
                registryItem.BackupEntryName,
                mainValue,
                mainValueEqualsDesired
            );

            if (mainValueEqualsDesired)
            {
                registryItem.RestoreDefaultValue(SoftwareSubkey.Instance);

                Log.Logger.Verbose(
                    "{RegistryItem_Name} defaults restored - name deleted.",
                    registryItem.BackupEntryName
                );
            }
            else
            {
                Log.Logger.Verbose("{RegistryItem_Name} main value left untouched.", registryItem.BackupEntryName);

                return;
            }
        }



        public static void CreateBackupEntry(this RegistryItem ri)
        {
            using (RegistryKey itemSubkey = SoftwareSubkey.Instance.OpenSubKey(ri.MainEntryPath))
            {
                object value = itemSubkey?.GetValue(ri.MainEntryName);

                ri.CreateBackupEntry(value);
            }
        }



        public static void CreateBackupEntry(
            this RegistryItem ri,
            object backupValue
        )
        {
            if (backupValue == null)
            {
                BackupSubkey.Instance.SetValue(ri.GetBackupEntry_WasPresentName, 0, RegistryValueKind.DWord);
                Log.Logger.Verbose(
                    "{RegistryItem_Name} not present in the main registry. 0 written into the backup registry.",
                    ri.BackupEntryName
                );

                BackupSubkey.Instance.DeleteValue(
                    ri.BackupEntryName,
                    throwOnMissingValue: false
                );
                Log.Logger.Verbose(
                    "{RegistryItem_Name} deleted from the backup registry.",
                    ri.BackupEntryName
                );
                // If the Name exists inside the backup registry, remove it.
            }
            else
            {
                BackupSubkey.Instance.SetValue(
                    ri.GetBackupEntry_WasPresentName,
                    1,
                    RegistryValueKind.DWord
                );
                Log.Logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    ri.BackupEntryName
                );

                BackupSubkey.Instance.SetValue(
                    ri.BackupEntryName,
                    backupValue,
                    ri.GetValueKind()
                );
                Log.Logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    ri.BackupEntryName,
                    backupValue
                );
            }
        }



        public static bool? GetWasPresentValue(this RegistryItem ri)
        {
            object _wasPresentValueObj = BackupSubkey.Instance
                .GetValue(ri.GetBackupEntry_WasPresentName);

            if (_wasPresentValueObj == null)
            {
                return null;
                // If the backup entry not fount.
            }

            int _wasPresentValueInt;
            try
            {
                _wasPresentValueInt = (int)_wasPresentValueObj;
            }
            catch (InvalidCastException)
            {
                Log.Logger.Warning(
                    "{RegistryEntryName} IsPresent backup entry was not an Integer with value {BackupEntryValue}.",
                    ri.MainEntryName,
                    _wasPresentValueObj.ToString()
                );

                throw new BackupRegistryRecordCorruptedException();
            }

            bool wasPresentValue;
            try
            {
                wasPresentValue = _wasPresentValueInt.ConvertToBool();
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Logger.Warning(
                    "{RegistryEntryName} IsPresent backup entry value was not 0 or 1 with value {wasPresentValue}.",
                    ri.BackupEntryName,
                    _wasPresentValueInt
                );

                throw new BackupRegistryRecordCorruptedException();
            }

            return wasPresentValue;
        }
    }
}