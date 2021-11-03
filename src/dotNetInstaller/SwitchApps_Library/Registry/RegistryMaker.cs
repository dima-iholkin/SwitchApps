﻿using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using SwitchApps.Library.Registry.Extensions;
using SwitchApps.Library.Registry.Model;



namespace SwitchApps.Library.Registry
{


    public class RegistryMaker
    {
        private readonly List<RegistryItem> _managedRegistryItems;
        private readonly Logger _logger;

        public RegistryMaker(List<RegistryItem> managedRegistryItems)
        {
            _managedRegistryItems = managedRegistryItems;
            _logger = (Logger)Log.Logger;
        }



        public void BackupRegistry()
        {
            _managedRegistryItems.ForEach(ri =>
            {
                bool makeBackup = ri.DecideToMakeBackupOrNot();
                _logger.Information(
                    "{EntryName} will be backed up: {MakeBackup}.",
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
            _managedRegistryItems.ForEach(ri =>
            {
                ri.SetMainEntry(ri.DesiredValue);
                _logger.Information(
                    "{EntryName} value {DesiredValue} written into the main registry.",
                    ri.BackupEntryName,
                    ri.DesiredValue.Value
                );
            });
        }



        public void RestoreRegistry()
        {
            _managedRegistryItems.ForEach(ri =>
            {
                bool makeRestore = ri.DecideToMakeRestoreOrNot();
                if (makeRestore == false)
                {
                    _logger.Information(
                        "{EntryName} main registry value left as is.", 
                        ri.BackupEntryName
                    );
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
        }
    }
}