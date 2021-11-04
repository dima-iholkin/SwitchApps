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
                return BackupSubkey.Instance.GetValue(ri.BackupEntryName)
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
                _logger.Information(
                    "{EntryName} not present in the main registry. "
                        + "WasPresent value 0 written into the backup registry.",
                    ri.BackupEntryName
                );

                BackupSubkey.Instance.DeleteValue(
                    ri.BackupEntryName,
                    throwOnMissingValue: false
                );
                // If the WasPresent value is false (0), then the Value entry must be absent.
                _logger.Information(
                    "{EntryName} Value entry deleted from the backup registry.",
                    ri.BackupEntryName
                );

                return;
            }

            BackupSubkey.Instance.SetValue(
                ri.BackupEntryWasPresentName,
                1,
                RegistryValueKind.DWord
            );
            _logger.Information(
                "{EntryName} present in the main registry. "
                    + "WasPresent value 1 written into the backup registry.",
                ri.BackupEntryName
            );

            BackupSubkey.Instance.SetValue(
                ri.BackupEntryName,
                value.Value,
                ri.ValueKind
            );
            _logger.Verbose(
                "{EntryName} value {MainValue} written into the backup registry.",
                ri.BackupEntryName,
                value
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
            _logger.Information(
                "{EntryName} WasPresent and Value entries deleted from the backup registry.",
                ri.BackupEntryName
            );
        }



        public static bool ValueEquals(
            this RegistryItemValue obj1,
            RegistryItemValue obj2
        )
        {
            if (obj1.Value.GetType() != obj2.Value.GetType())
            {
                throw new Exception("Incompatible type comparison.");
            }

            return obj1.Value.Equals(obj2.Value);
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
                    throw new ArgumentOutOfRangeException("Expected integer 0 or 1.");
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
                        $"Expected integer, string or null. Encountered value {valueObj} of type {valueObj.GetType()}. "
                    );
            };
        }
    }
}