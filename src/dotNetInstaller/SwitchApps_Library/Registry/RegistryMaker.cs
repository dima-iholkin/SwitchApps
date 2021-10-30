using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Extensions;



namespace SwitchApps.Library.Registry
{


    public class RegistryMaker
    {
        private readonly RegistryKey _softwareSubkey;
        private readonly RegistryKey _backupSubkey;
        private readonly List<RegistryItem> _registryItemsToEdit;
        private readonly Logger _logger;

        public RegistryMaker(
            RegistryKey softwareKey,
            RegistryKey backupKey,
            List<RegistryItem> registryItemsToEdit,
            Logger logger
        )
        {
            _softwareSubkey = softwareKey;
            _backupSubkey = backupKey;
            _registryItemsToEdit = registryItemsToEdit;
            _logger = logger;
        }



        public void BackupRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                bool makeBackup;
                try
                {
                    bool backupEntryExists = ri.BackupEntryExists(
                       _backupSubkey,
                       _logger
                    );

                    makeBackup = !backupEntryExists;
                    // Don't make a backup, if the backup entry exists and correct.
                    // Make a backup, if the backup entry doesn't exist.
                }
                catch (BackupRegistryRecordCorruptedException)
                {
                    if (ri.MainValueEqualsDesired(_softwareSubkey))
                    {
                        ri.ResetMainEntryToSystemDefaultValue(
                            _softwareSubkey,
                            _logger
                        );
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
                    ri.CreateBackupEntry(
                        _softwareSubkey,
                        _backupSubkey,
                        _logger
                    );
                }
            });
        }



        public void ModifyRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                using (RegistryKey entrySubkey = _softwareSubkey.CreateSubKey(ri.MainEntryPath))
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
                object mainValue = ri.GetMainValue(_softwareSubkey);

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

                ri.GetWasPresentValue()

                bool wasPresent;
                int wasPresent_int = (int)_backupSubkey.GetValue(ri.GetBackupEntry_WasPresentName);
                switch (wasPresent_int)
                {
                    case 1:
                        wasPresent = true;
                        break;
                    case 0:
                        wasPresent = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unexpected backup registry isPresent value.");
                }

                _logger.Verbose(
                    "{RegistryItem_Name} was present in the main registry before the install: {WasPresent}.",
                    ri.BackupEntryName,
                    wasPresent
                );

                if (wasPresent)
                {
                    object backupValue = _backupSubkey.GetValue(ri.BackupEntryName);

                    _softwareSubkey
                        .CreateSubKey(ri.MainEntryPath)
                        .SetValue(ri.MainEntryName, backupValue, ri.ValueKind);

                    _logger.Verbose(
                        "{RegistryItem_Name} value changed to {BackupValue} in the main registry.",
                        ri.BackupEntryName,
                        backupValue
                    );
                }
                else
                {
                    ri.ResetMainEntryToSystemDefaultValue(
                        _softwareSubkey,
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