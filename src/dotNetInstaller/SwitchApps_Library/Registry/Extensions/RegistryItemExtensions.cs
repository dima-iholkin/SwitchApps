using System;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;



namespace SwitchApps.Library.Registry.Extensions
{


    public static class RegistryItemExtensions
    {
        private static readonly Logger _logger = (Logger)Log.Logger;



        public static RegistryItemValue GetMainValue(this RegistryItem ri)
        {
            object _mainValueObj;
            using (RegistryKey itemSubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
            {
                _mainValueObj = itemSubkey.GetValue(ri.MainEntryName);
            };
            // Using CreateSubKey() instead of OpenSubKey() just to avoid a possible null result.

            try
            {
                return _mainValueObj.ConvertToRegistryItemValue();
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException(
                    $"Unexpected value {_mainValueObj} from the {nameof(ri.BackupEntryName)} main entry.",
                    ex
                );
            }
        }



        public static bool? GetBackupWasPresentValue(this RegistryItem ri)
        {
            object _valueObj = BackupSubkey.Instance.GetValue(ri.GetBackupEntry_WasPresentName);
            if (_valueObj == null)
            {
                return null;
                // If the backup entry is not fount.
            }

            try
            {
                int _valueInt = (int)_valueObj;
                return _valueInt.ConvertToBool();
            }
            catch (InvalidCastException)
            {
                _logger.Warning(
                    "{EntryName} WasPresent entry's expected value's type is Int32, not type {ValueObjType}.",
                    ri.BackupEntryName,
                    _valueObj.GetType()
                );
                throw new BackupRegistryRecordCorruptedException();
                // If the valueObj cannot be converted to int.
            }
            catch (ArgumentOutOfRangeException)
            {
                _logger.Warning(
                    "{EntryName} WasPresent entry's expected values are 0 or 1, not {ValueInt}." +
                        "the actual value was .",
                    ri.BackupEntryName,
                    (int)_valueObj
                );
                throw new BackupRegistryRecordCorruptedException();
                // if the valueInt cannot be converted to bool.
            }
        }



        public static RegistryItemValue GetBackupValue(this RegistryItem ri)
        {
            try
            {
                return BackupSubkey.Instance
                    .GetValue(ri.BackupEntryName)
                    .ConvertToRegistryItemValue();
            }
            catch (Exception)
            {
                throw new BackupRegistryRecordCorruptedException();
            }
        }



        public static void EditMainEntry(
            this RegistryItem ri,
            RegistryItemValue value
        )
        {
            using (var entrySubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
            {
                if (value == null)
                {
                    entrySubkey.DeleteValue(ri.MainEntryName);
                    return;
                }

                entrySubkey.SetValue(ri.MainEntryName, value.Value);
            }
        }



        public static bool ValueEquals(
            this RegistryItemValue obj1,
            RegistryItemValue obj2
        )
        {
            if (obj1.Value.GetType() != obj2.Value.GetType())
            {
                throw new Exception("Incompatible types compared.");
            }

            return obj1.Equals(obj2);
        }



        public static void RestoreFromBackupValue(this RegistryItem registryItem)
        {
            RegistryItemValue mainValue = registryItem.GetMainValue();
            if (mainValue == null)
            {
                _logger.Verbose(
                    "{EntryName} main value null left as is.",
                    registryItem.BackupEntryName
                );
                return;
                // If the current main value is not found,
                // it must had been changed after the install by somebody,
                // therefore leave it as is.
            }

            bool mainValueEqualsDesired = registryItem.DesiredValue.ValueEquals(mainValue);
            _logger.Verbose(
                "{EntryName} main value {MainValue} equals the desired value {DesiredValue} " +
                    ": {MainValueEqualsDesired}.",
                registryItem.BackupEntryName,
                mainValue,
                registryItem.DesiredValue,
                mainValueEqualsDesired
            );

            if (mainValueEqualsDesired)
            {
                registryItem.EditMainEntry(registryItem.GetBackupValue());
                _logger.Verbose(
                    "{EntryName} restored from backup value {BackupValue} restored.",
                    registryItem.BackupEntryName,
                    registryItem.GetBackupValue()
                );
            }
            else
            {
                _logger.Verbose("{EntryName} main value {MainValue} left as is.", registryItem.BackupEntryName);
                return;
            }
        }



        private static void RestoreFromSystemDefaultValue(this RegistryItem ri)
        {
            using (var itemSubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
            {
                if (ri.SystemDefaultValue == null)
                {
                    itemSubkey.DeleteValue(
                        ri.MainEntryName,
                        throwOnMissingValue: false
                    );

                    return;
                }

                itemSubkey.SetValue(ri.MainEntryName, ri.SystemDefaultValue.Value);
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
                _logger.Verbose(
                    "{RegistryItem_Name} not present in the main registry. 0 written into the backup registry.",
                    ri.BackupEntryName
                );

                BackupSubkey.Instance.DeleteValue(
                    ri.BackupEntryName,
                    throwOnMissingValue: false
                );
                _logger.Verbose(
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
                _logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    ri.BackupEntryName
                );

                BackupSubkey.Instance.SetValue(
                    ri.BackupEntryName,
                    backupValue,
                    ri.GetValueKind()
                );
                _logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    ri.BackupEntryName,
                    backupValue
                );
            }
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



        private static RegistryItemValue ConvertToRegistryItemValue(this object valueObj)
        {
            switch (valueObj)
            {
                case String s:
                    return s;
                case Int32 i:
                    return i;
                case null:
                    return null;
                default:
                    throw new InvalidCastException(
                        $"Unexpected value {valueObj}. Expected int, string or null."
                    );
            };
        }
    }
}