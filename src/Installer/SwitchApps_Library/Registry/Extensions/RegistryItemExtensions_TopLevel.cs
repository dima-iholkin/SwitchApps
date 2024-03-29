﻿using System;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;



namespace SwitchApps.Library.Registry.Extensions
{


    public static class RegistryItemExtensions_TopLevel
    {
        private static readonly Logger _logger = (Logger)Log.Logger;



        public static bool DecideToMakeBackupOrNot(this RegistryItem ri)
        {
            try
            {
                bool? wasPresentValue = ri.GetBackupWasPresentValue();

                if (wasPresentValue == null)
                {
                    return true;
                    // If the backup entry wasn't found, make a backup.
                }

                return false;
                // If the backup entry was found, don't make a new backup. 
            }
            catch (BackupRegistryRecordCorruptedException)
            {
                RegistryItemValue mainValue;
                try
                {
                    mainValue = ri.GetMainValue();
                }
                catch (Exception)
                {
                    return false;
                    // If cannot parse the current main value, then don't make a backup,
                    // as I cannot backup an unparsable value.
                }

                bool mainValueEqualsDesired = ri.DesiredValue.ValueEquals(mainValue);
                if (mainValueEqualsDesired)
                {
                    ri.RestoreFromSystemDefault();
                }
                // If the current backup is corrupted,
                // and the current value appears to be set by this installer:
                // 1. Reset the main entry to the system default.

                return true;
                // 2. Make the backup of the current main entry.
                // It will be either the system default, if the (1) executes,
                // or the current main value as it was before the installer started.
            }
        }



        public static bool DecideToMakeRestoreOrNot(this RegistryItem ri)
        {
            try
            {
                RegistryItemValue mainValue = ri.GetMainValue();
                bool mainValueEqualsDesired = ri.DesiredValue.ValueEquals(mainValue);
                _logger.Information(
                    "{EntryName} main registry value {MainValue} "
                        + "equals the desired value {DesiredValue}: {MainValueEqualsDesired}.",
                    ri.BackupEntryName,
                    mainValue.Value,
                    ri.DesiredValue.Value,
                    mainValueEqualsDesired
                );

                if (mainValueEqualsDesired)
                {
                    return true;
                    // Make a restore,
                    // as the current main value appears to be set by this installer.
                }

                return false;
                // If the current main value appears to be changed by somebody else,
                // leave it as is.
            }
            catch (Exception)
            {
                return false;
                // If cannot parse the current main value,
                // leave it as is.
            }
        }



        public static RestoreSource DecideToMakeRestoreFromSystemDefaultOrFromBackup(this RegistryItem ri)
        {
            bool? wasPresent = ri.GetBackupWasPresentValue();
            if (wasPresent == null)
            {
                _logger.Warning(
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
                    _logger.Warning(
                        "{EntryName} WasPresent entry present, but Value entry not found." +
                            "Therefore the backup is corrupted. Restore from the system defaults.",
                        ri.BackupEntryName
                    );
                    return RestoreSource.SystemDefault;
                }
            }

            _logger.Information(
                "{EntryName} will be restored from the backup.",
                ri.BackupEntryName
            );
            return RestoreSource.Backup;
        }



        public static void RestoreFromBackup(this RegistryItem registryItem)
        {
            registryItem.SetMainEntry(registryItem.GetBackupValue());
            _logger.Information(
                "{EntryName} restored to value {BackupValue}.",
                registryItem.BackupEntryName,
                registryItem.GetBackupValue()
            );
        }



        public static void RestoreFromSystemDefault(this RegistryItem ri)
        {
            using (var itemSubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
            {
                if (ri.SystemDefaultValue == null)
                {
                    itemSubkey.DeleteValue(
                        ri.MainEntryName,
                        throwOnMissingValue: false
                    );
                    _logger.Information(
                        "{EntryName} deleted from the main registry.",
                        ri.BackupEntryName
                    );
                    return;
                }

                itemSubkey.SetValue(
                    ri.MainEntryName,
                    ri.SystemDefaultValue.Value
                );
                _logger.Information(
                    "{EntryName} restored to value {SystemDefaultValue}.",
                    ri.BackupEntryName,
                    ri.SystemDefaultValue.Value
                );
            }
        }



        public static void CreateBackup(this RegistryItem ri)
        {
            using (RegistryKey itemSubkey = SoftwareSubkey.Instance.OpenSubKey(ri.MainEntryPath))
            {
                object _valueObj = itemSubkey?.GetValue(ri.MainEntryName);
                RegistryItemValue value = _valueObj.ParseRegistryObjectToRegistryItemValue();

                ri.SetBackupEntry(value);
            }
        }
    }
}