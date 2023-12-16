using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Extensions;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;

namespace SwitchApps.Library.Registry
{
    public class RegistryManager
    {
        // Init:

        private readonly List<RegistryItem> _managedRegistryItems;
        private readonly Logger _logger;

        public RegistryManager(List<RegistryItem> managedRegistryItems)
        {
            _managedRegistryItems = managedRegistryItems;
            _logger = (Logger)Log.Logger;
        }

        // Public methods:

        public void BackupRegistry()
        {
            _managedRegistryItems.ForEach(ri =>
            {
                bool makeBackup = ri.DecideToMakeBackupOrNot();

                _logger.Information("{EntryName} will be backed up: {MakeBackup}.", ri.BackupEntryName, makeBackup);

                if (makeBackup == true)
                {
                    ri.CreateBackup();
                }
            });

            this._logger.Information("The registry backup finished.");
        }

        public void ModifyRegistry()
        {
            _managedRegistryItems.ForEach(ri =>
            {
                ri.SetMainEntry(ri.DesiredValue);

                _logger.Information(
                    "{EntryName} value {DesiredValue} written into the main registry.",
                    ri.BackupEntryName,
                    ri.DesiredValue.Value
                );
            });

            this._logger.Information("The registry modification finished.");
        }

        public void RestoreRegistry()
        {
            _managedRegistryItems.ForEach(ri =>
            {
                bool makeRestore = ri.DecideToMakeRestoreOrNot();

                if (makeRestore == false)
                {
                    _logger.Information("{EntryName} main registry value left as is.", ri.BackupEntryName);

                    return;
                }

                RestoreSource restoreSource = ri.DecideToMakeRestoreFromSystemDefaultOrFromBackup();

                switch (restoreSource)
                {
                    case RestoreSource.SystemDefault:
                        ri.RestoreFromSystemDefault();
                        break;
                    case RestoreSource.Backup:
                        ri.RestoreFromBackup();
                        break;
                    default:
                        throw new Exception($"Unexpected value {restoreSource} of {nameof(restoreSource)}.");
                }
            });

            this._logger.Information("The registry restore finished.");
        }

        public void DeleteBackupTree()
        {
            SoftwareSubkey.Instance.DeleteSubKeyTree("SwitchApps");

            this._logger.Information("The backup registry subtree SwitchApps deleted.");
        }
    }
}