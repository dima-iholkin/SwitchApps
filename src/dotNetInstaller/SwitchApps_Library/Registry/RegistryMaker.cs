using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Extensions;
using SwitchApps.Library.Registry.Model;



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
                bool makeBackup = ri.DecideToMakeBackupOrNot();
                _logger.Verbose(
                    "{EntryName} will backup: {MakeBackup}.",
                    ri.BackupEntryName,
                    makeBackup
                );

                if (makeBackup == true)
                {
                    ri.CreateBackup();
                }
            });
        }



        public void ModifyRegistry()
        {
            _registryItemsToEdit.ForEach(ri =>
            {
                ri.SetMainEntry(ri.DesiredValue);
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
                bool makeRestore = ri.DecideToMakeRestoreOrNot();
                if (makeRestore == false)
                {
                    _logger.Verbose("{RegistryItem_Name} main registry value left as is.", ri.BackupEntryName);
                    return;
                }

                RestoreSource restoreSource = ri.DecideToRestoreFromSystemDefaultOrFromBackup();
                switch (restoreSource)
                {
                    case RestoreSource.SystemDefault:
                        ri.RestoreFromSystemDefaultValue();
                        break;
                    case RestoreSource.Backup:
                        ri.RestoreFromBackupValue();
                        break;
                    default:
                        throw new Exception($"Unexpected value of {nameof(restoreSource)}.");
                }
            });
        }
    }
}