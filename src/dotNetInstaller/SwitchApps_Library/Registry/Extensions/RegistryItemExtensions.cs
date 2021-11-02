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
            object _valueObj = BackupSubkey.Instance.GetValue(ri.BackupEntryWasPresentName);
            if (_valueObj == null)
            {
                return null;
                // If the backup entry is not found.
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
            BackupSubkey.Instance.DeleteValue(ri.BackupEntryName);
            BackupSubkey.Instance.DeleteValue(ri.BackupEntryWasPresentName);
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



        public static bool ConvertToBool(this int _int)
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



        public static RegistryItemValue ConvertToRegistryItemValue(this object valueObj)
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