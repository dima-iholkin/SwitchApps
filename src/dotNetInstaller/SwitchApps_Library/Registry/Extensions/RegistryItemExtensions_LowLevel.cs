using System;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;



namespace SwitchApps.Library.Registry.Extensions
{


    public static class RegistryItemExtensions_LowLevel
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
                return _mainValueObj.ParseRegistryObjectToRegistryItemValue();
            }
            catch (InvalidCastException ex)
            {
                _logger.Error(
                    ex,
                    "{EntryName} unexpected value {MainValue} from the main entry.",
                    ri.BackupEntryName,
                    _mainValueObj
                );
                throw;
            }
        }



        public static bool? GetBackupWasPresentValue(this RegistryItem ri)
        {
            object _valueObj = BackupSubkey.Instance.GetValue(ri.BackupEntryWasPresentName);
            if (_valueObj == null)
            {
                return null;
                // Null means the entry was not found.
            }

            try
            {
                int _valueInt = (int)_valueObj;
                return _valueInt.ParseBackupWasPresentIntToBool();
                // True, false means the value of the entry.
            }
            catch (InvalidCastException)
            {
                _logger.Warning(
                    "{EntryName} WasPresent entry's expected value's type is Int32, not type {ValueObjType}.",
                    ri.BackupEntryName,
                    _valueObj.GetType()
                );
                throw new BackupRegistryRecordCorruptedException();
                // If the _valueObj cannot be converted to int.
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
                // If the _valueInt cannot be converted to bool.
            }
        }
        // Null means the entry was not found.
        // True, false means the value of the entry.
        // BackupRegistryRecordCorruptedException can be thrown.



        public static RegistryItemValue GetBackupValue(this RegistryItem ri)
        {
            try
            {
                return BackupSubkey.Instance
                    .GetValue(ri.BackupEntryName)
                    .ParseRegistryObjectToRegistryItemValue();
            }
            catch (Exception)
            {
                throw new BackupRegistryRecordCorruptedException();
            }
        }



        public static void SetMainEntry(
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



        public static void SetBackupEntry(
            this RegistryItem ri,
            RegistryItemValue value
        )
        {
            if (value == null)
            {
                BackupSubkey.Instance.SetValue(
                    ri.BackupEntryWasPresentName,
                    0,
                    RegistryValueKind.DWord
                );

                BackupSubkey.Instance.DeleteValue(ri.BackupEntryName);
                // If WasPresent is false(0), then the value entry must not exist.

                return;
            }

            BackupSubkey.Instance.SetValue(
                ri.BackupEntryWasPresentName,
                value.Value,
                RegistryValueKind.DWord
            );
            BackupSubkey.Instance.SetValue(
                ri.BackupEntryName,
                value.Value,
                ri.ValueKind
            );
        }



        public static void DeleteBackupEntries(this RegistryItem ri)
        {
            BackupSubkey.Instance.DeleteValue(
                ri.BackupEntryName,
                throwOnMissingValue: false
            );
            BackupSubkey.Instance.DeleteValue(
                ri.BackupEntryWasPresentName,
                throwOnMissingValue: false
            );
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



        public static bool ParseBackupWasPresentIntToBool(this int _int)
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



        public static RegistryItemValue ParseRegistryObjectToRegistryItemValue(this object valueObj)
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