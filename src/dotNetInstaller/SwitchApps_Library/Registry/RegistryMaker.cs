﻿using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Extensions;



namespace SwitchApps.Library.Registry
{


    public class RegistryMaker
    {
        private readonly RegistryKey _softwareKey;
        private readonly RegistryKey _backupKey;
        private readonly List<RegistryItem> _registryItemsToEdit;

        private readonly Logger _logger;

        public RegistryMaker(
            RegistryKey softwareKey,
            RegistryKey backupKey,
            List<RegistryItem> registryItemsToEdit,
            Logger logger
        )
        {
            _softwareKey = softwareKey;
            _backupKey = backupKey;
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
                       _backupKey,
                       _logger
                   );

                    makeBackup = !backupEntryExists;
                    // Don't make a backup, if the backup entry exists and correct.
                    // Make a backup, if the backup entry doesn't exist.
                }
                catch (BackupRegistryRecordCorruptedException)
                {
                    if (ri.MainValueEqualsDesired(_softwareKey))
                    {
                        ri.ResetMainEntryToSystemDefaultValue(
                            _softwareKey,
                            _logger
                        );
                    }
                    // Reset the main entry, if the current backup entry is corrupted
                    // and the current main entry value equals the desired value setup sets.

                    makeBackup = true;
                    // Make a backup, if the current backup entry is corrupted.
                }

                _logger.Verbose(
                    "{RegistryItem_Name} backup required: {BackupRequired}",
                    ri.Name,
                    makeBackup
                );

                if (makeBackup == false)
                {
                    ri.CreateBackupEntry(
                        _softwareKey,
                        _backupKey,
                        _logger
                    );
                }
            });
        }



        public void ModifyRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                ModifyRegistryItem(ri);
            });
        }



        private void ModifyRegistryItem(RegistryItem registryItem)
        {
            _softwareKey
                .CreateSubKey(registryItem.Path)
                .SetValue(
                    registryItem.Name,
                    registryItem.DesiredValue
                );

            _logger.Verbose(
                "{RegistryItem_Name} value {RegistryItem_Value} written into the main registry.",
                registryItem.CustomName,
                registryItem.DesiredValue
            );
        }



        public void RestoreRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                RestoreRegistryItem(ri);
            });
        }



        private void RestoreRegistryItem(RegistryItem registryItem)
        {
            object mainValue = _softwareKey
                .CreateSubKey(registryItem.Path)
                .GetValue(registryItem.Name);

            bool mainValueEqualsDesired = registryItem.MainValueEqualsDesiredValue(mainValue);

            _logger.Verbose(
                "{RegistryItem_Name} main value {MainValue} equals the desired value: {MainValueEqualsDesired}.",
                registryItem.CustomName,
                mainValue,
                mainValueEqualsDesired
            );

            if (mainValueEqualsDesired == false)
            {
                _logger.Verbose("{RegistryItem_Name} main value left untouched.", registryItem.CustomName);

                return;
            }

            bool wasPresent;
            int wasPresent_int = (int)_backupKey.GetValue(registryItem.IsPresent_Name);
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
                registryItem.CustomName,
                wasPresent
            );

            if (wasPresent)
            {
                object backupValue = _backupKey.GetValue(registryItem.CustomName);

                _softwareKey
                    .CreateSubKey(registryItem.Path)
                    .SetValue(registryItem.Name, backupValue, registryItem.ValueKind);

                _logger.Verbose(
                    "{RegistryItem_Name} value changed to {BackupValue} in the main registry.",
                    registryItem.CustomName,
                    backupValue
                );
            }
            else
            {
                registryItem.ResetMainEntryToSystemDefaultValue(
                    _softwareKey,
                    _logger
                );

                _logger.Verbose(
                    "{RegistryItem_Name} was deleted in the main registry.",
                    registryItem.CustomName
                );
            }
        }
    }
}