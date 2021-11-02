using System;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Extensions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;



namespace SwitchApps.Library.Registry
{


    public static class RegistryMakerHelpers
    {
        private static readonly Logger _logger = (Logger)Log.Logger;



        public static bool DecideToMakeBackupOrNot(this RegistryItem ri)
        {
            bool makeBackup = false;

            try
            {
                bool? wasPresentValue = ri.GetBackupWasPresentValue();

                bool backupExists = wasPresentValue.HasValue;
                if (backupExists == false)
                {
                    makeBackup = true;
                    // If the backup entry doesn't exist, make a backup.
                }
            }
            catch (BackupRegistryRecordCorruptedException)
            {
                bool mainValueEqualsDesired = ri.DesiredValue.ValueEquals(ri.GetMainValue());
                if (mainValueEqualsDesired)
                {
                    ri.RestoreFromSystemDefaultValue();
                }
                // Reset the main entry,
                // if the current main value equals the desired value
                // and the current backup entry is corrupted.

                makeBackup = true;
                // Make a backup, if the current backup entry is corrupted.
            }

            return makeBackup;
        }



        public static bool DecideToMakeRestoreOrNot(this RegistryItem ri)
        {
            RegistryItemValue mainValue = ri.GetMainValue();
            bool mainValueEqualsDesired = ri.DesiredValue.ValueEquals(mainValue);
            _logger.Verbose(
                "{EntryName} main registry value: {MainValue} " +
                    "equals the desired value: {MainValueEqualsDesired}.",
                ri.BackupEntryName,
                mainValue,
                mainValueEqualsDesired
            );

            bool makeRestore = false;
            if (mainValueEqualsDesired)
            {
                makeRestore = true;
            }

            return makeRestore;
        }



        public static RestoreSource DecideToRestoreFromSystemDefaultOrFromBackup(this RegistryItem ri)
        {
            bool? wasPresent = ri.GetBackupWasPresentValue();
            if (wasPresent == null)
            {
                _logger.Verbose(
                    "{EntryName} not found in the backup registry. Restore from the system defaults.",
                    ri.BackupEntryName
                );
                return RestoreSource.SystemDefault;
            }

            if (wasPresent == true)
            {
                RegistryItemValue backupValue = ri.GetBackupValue();
                if (backupValue == null)
                {
                    _logger.Verbose(
                        "{EntryName} WasPresent entry present, but Value entry not found." +
                            "Therefore the backup is corrupted. Restore from the system defaults.",
                        ri.BackupEntryName
                    );
                    return RestoreSource.SystemDefault;
                }
            }

            _logger.Verbose(
                "{EntryName} will be restored from the backup.",
                ri.BackupEntryName
            );
            return RestoreSource.Backup;
        }



        public static void RestoreFromBackupValue(this RegistryItem registryItem)
        {
            registryItem.SetMainEntry(registryItem.GetBackupValue());
            _logger.Verbose(
                "{EntryName} restored to value {BackupValue}.",
                registryItem.BackupEntryName,
                registryItem.GetBackupValue()
            );
        }



        public static void RestoreFromSystemDefaultValue(this RegistryItem ri)
        {
            using (var itemSubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
            {
                if (ri.SystemDefaultValue == null)
                {
                    itemSubkey.DeleteValue(
                        ri.MainEntryName,
                        throwOnMissingValue: false
                    );
                    _logger.Verbose(
                        "{EntryName} deleted from the main registry.",
                        ri.BackupEntryName
                    );
                    return;
                }

                itemSubkey.SetValue(
                    ri.MainEntryName,
                    ri.SystemDefaultValue.Value
                );
                _logger.Verbose(
                    "{EntryName} restored to value {SystemDefaultValue}.",
                    ri.BackupEntryName,
                    ri.SystemDefaultValue
                );
            }
        }



        public static void CreateBackup(this RegistryItem ri)
        {
            using (RegistryKey itemSubkey = SoftwareSubkey.Instance.OpenSubKey(ri.MainEntryPath))
            {
                object _valueObj = itemSubkey?.GetValue(ri.MainEntryName);

                RegistryItemValue value = _valueObj.ConvertToRegistryItemValue();

                ri.CreateBackup(value);
            }
        }



        public static void CreateBackup(
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
                    ri.BackupEntryWasPresentName,
                    1,
                    RegistryValueKind.DWord
                );
                _logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    ri.BackupEntryName
                );

                BackupSubkey.Instance.SetValue(
                    ri.BackupEntryName,
                    value,
                    ri.ValueKind
                );
                _logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    ri.BackupEntryName,
                    value
                );
            }
        }
    }
}