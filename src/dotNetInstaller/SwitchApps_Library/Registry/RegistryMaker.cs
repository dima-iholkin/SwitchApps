using System.Collections.Generic;
using Microsoft.Win32;
using OneOf;
using OneOf.Types;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Extensions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;



namespace SwitchApps.Library.Registry
{


    public class RegistryMaker
    {
        private readonly List<RegistryItem> _registryItemsToEdit;

        private readonly Logger _logger;

        public RegistryMaker(List<RegistryItem> registryItemsToEdit)
        {
            _registryItemsToEdit = registryItemsToEdit;

            _logger = (Logger)Log.Logger;
        }



        public void BackupRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                bool makeBackup;
                try
                {
                    bool? wasPresent = ri.GetBackupWasPresentValue();

                    bool backupEntryExists = wasPresent.HasValue;

                    makeBackup = !backupEntryExists;
                    // Don't make a backup, if the backup entry exists and correct.
                    // Make a backup, if the backup entry doesn't exist.
                }
                catch (BackupRegistryRecordCorruptedException)
                {
                    if (ri.MainValueEqualsDesired(SoftwareSubkey.Instance))
                    {
                        ri.RestoreFromBackupValue();
                    }
                    // Reset the main entry,
                    // if the current main entry value equals the desired value setup sets
                    // and the current backup entry is corrupted.

                    makeBackup = true;
                    // Make a backup, if the current backup entry is corrupted.
                }
                _logger.Verbose(
                    "{RegistryItem_Name} backup required: {BackupRequired}",
                    ri.MainEntryName,
                    makeBackup
                );

                if (makeBackup == true)
                {
                    ri.CreateBackupEntry();
                }
            });
        }



        public void ModifyRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                using (RegistryKey entrySubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
                {
                    entrySubkey.SetValue(
                        ri.MainEntryName,
                        ri.DesiredValue.Value
                    );
                }
                _logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the main registry.",
                    ri.BackupEntryName,
                    ri.DesiredValue.Value
                );
            });
        }



        public void RestoreRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
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

                if (mainValueEqualsDesired == false)
                {
                    _logger.Verbose("{RegistryItem_Name} main registry value left as is.", ri.BackupEntryName);
                    return;
                }

                bool? wasPresent = ri.GetBackupWasPresentValue();
                if (wasPresent == null)
                {
                    _logger.Verbose(
                        "{BackupEntryName} is not present in the backup registry.",
                        ri.BackupEntryName
                    );
                    return;
                }

                if (wasPresent.Value == true)
                {
                    object backupValue = BackupSubkey.Instance.GetValue(ri.BackupEntryName);
                    using (RegistryKey entrySubkey = SoftwareSubkey.Instance.CreateSubKey(ri.MainEntryPath))
                    {
                        entrySubkey.SetValue(
                            ri.MainEntryName,
                            backupValue,
                            ri.GetValueKind()
                        );
                    }
                    _logger.Verbose(
                        "{RegistryItem_Name} value changed to {BackupValue} in the main registry.",
                        ri.BackupEntryName,
                        backupValue
                    );
                }
                else
                {
                    ri.RestoreFromBackupValue();
                    _logger.Verbose(
                        "{RegistryItem_Name} was deleted in the main registry.",
                        ri.BackupEntryName
                    );
                }
            });
        }
    }
}