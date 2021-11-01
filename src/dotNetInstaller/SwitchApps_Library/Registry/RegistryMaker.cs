using System.Collections.Generic;
using Microsoft.Win32;
using Serilog;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Extensions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;



namespace SwitchApps.Library.Registry
{


    public class RegistryMaker
    {
        private readonly List<RegistryItem> _registryItemsToEdit;

        public RegistryMaker(List<RegistryItem> registryItemsToEdit)
        {
            _registryItemsToEdit = registryItemsToEdit;
        }



        public void BackupRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                bool makeBackup;
                try
                {
                    bool? wasPresent = ri.GetWasPresentValue();

                    bool backupEntryExists = wasPresent.HasValue;

                    makeBackup = !backupEntryExists;
                    // Don't make a backup, if the backup entry exists and correct.
                    // Make a backup, if the backup entry doesn't exist.
                }
                catch (BackupRegistryRecordCorruptedException)
                {
                    if (ri.MainValueEqualsDesired(SoftwareSubkey.Instance))
                    {
                        ri.ResetMainEntryToSystemDefaultValue();
                    }
                    // Reset the main entry,
                    // if the current main entry value equals the desired value setup sets
                    // and the current backup entry is corrupted.

                    makeBackup = true;
                    // Make a backup, if the current backup entry is corrupted.
                }

                Log.Logger.Verbose(
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
                object mainValue = ri.GetMainValue(SoftwareSubkey.Instance);

                bool mainValueEqualsDesired = ri.MainValueEqualsDesiredValue(mainValue);

                _logger.Verbose(
                    "{RegistryItem_Name} main value {MainValue} equals the desired value: {MainValueEqualsDesired}.",
                    ri.BackupEntryName,
                    mainValue,
                    mainValueEqualsDesired
                );

                if (mainValueEqualsDesired == false)
                {
                    _logger.Verbose("{RegistryItem_Name} main value left untouched.", ri.BackupEntryName);

                    return;
                }

                bool? wasPresent = ri.GetWasPresentValue(
                    BackupSubkey.Instance,
                    _logger
                );

                if (wasPresent == null)
                {
                    _logger.Verbose(
                        "{BackupEntryName} is not present in the backup registry.",
                        ri.BackupEntryName
                    );
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
                    ri.ResetMainEntryToSystemDefaultValue(
                        SoftwareSubkey.Instance,
                        _logger
                    );

                    _logger.Verbose(
                        "{RegistryItem_Name} was deleted in the main registry.",
                        ri.BackupEntryName
                    );
                }
            });
        }
    }
}