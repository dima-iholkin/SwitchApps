using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Serilog.Core;
using SwitchApps.Library.Registry._Helpers;
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
                bool backupCurrentValue;
                try
                {
                    backupCurrentValue = CheckIfPresentInBackupRegistry(ri);
                }
                catch (BackupRegistryRecordCorruptedException)
                {
                    // If main value equals to desired, change it to null (the default value for these registry records).

                    bool mainValueEqualsDesired = ri.MainValueEqualsDesired(_softwareKey);

                    if (ri.MainValueEqualsDesiredOrNull(_softwareKey))
                    {
                        //ResetMainRegistryRecord(ri);

                        // Backup that the value is null.
                    }


                    backupCurrentValue = true;
                }

                _logger.Verbose(
                    "{RegistryItem_Name} backup required: {BackupRequired}",
                    ri.Name,
                    backupCurrentValue
                );

                if (backupCurrentValue == false)
                {
                    BackupRegistryItem(ri);
                }
            });
        }



        private bool CheckIfPresentInBackupRegistry(RegistryItem registryItem)
        {
            var isPresent = _backupKey.GetValue(registryItem.IsPresent_Name);

            if (isPresent == null)
            {
                return false;
            }

            if ((int)isPresent == 0)
            {
                return true;
            }

            if ((int)isPresent == 1)
            {
                var backupValue = _backupKey.GetValue(registryItem.Name);

                if (backupValue == null)
                {
                    throw new BackupRegistryRecordCorruptedException();
                }
                else
                {
                    return true;
                }
            }

            throw new BackupRegistryRecordCorruptedException();
            // Normally isPresent value can be only 0, 1 or null (when not fould).
            // Meaning if the code got here, something probably had become incorrect
            // and needs reinitializing.
        }



        private void ResetMainRegistryRecord(RegistryItem registryItem)
        {
            _logger.Verbose("{RegistryItem_Name} backup registry corruption detected.", registryItem.CustomName);

            object mainValue = registryItem.GetMainValue(_softwareKey);

            bool mainValueEqualsDesired = registryItem.MainValueEqualsDesired(_softwareKey);

            _logger.Verbose(
                "{RegistryItem_Name} main value {MainValue} equals the desired value: {MainValueEqualsDesired}.",
                registryItem.CustomName,
                mainValue,
                mainValueEqualsDesired
            );

            if (mainValueEqualsDesired)
            {
                registryItem.RestoreDefaultValue(_softwareKey);

                _logger.Verbose(
                    "{RegistryItem_Name} defaults restored - name deleted.",
                    registryItem.CustomName
                );
            }
            else
            {
                _logger.Verbose("{RegistryItem_Name} main value left untouched.", registryItem.CustomName);

                return;
            }
        }



        private void BackupRegistryItem(RegistryItem registryItem)
        {
            using (RegistryKey itemSubkey = _softwareKey.OpenSubKey(registryItem.Path))
            {
                object value = itemSubkey?.GetValue(registryItem.Name);

                BackupRegistryItem(registryItem, value);
            }
        }



        private void BackupRegistryItem(RegistryItem registryItem, object backupValue)
        {
            if (backupValue is null)
            {
                _backupKey.SetValue(registryItem.IsPresent_Name, 0, RegistryValueKind.DWord);
            
                _logger.Verbose(
                    "{RegistryItem_Name} not present in the main registry. 0 written into the backup registry.",
                    registryItem.CustomName
                );

                _backupKey.DeleteValue(
                    registryItem.CustomName,
                    throwOnMissingValue: false
                );
                
                _logger.Verbose(
                    "{RegistryItem_Name} deleted from the backup registry.",
                    registryItem.CustomName
                );
                // If the Name exists inside the backup registry, remove it.
            }
            else
            {
                _backupKey.SetValue(registryItem.IsPresent_Name, 1, RegistryValueKind.DWord);
                
                _logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    registryItem.CustomName
                );

                _backupKey.SetValue(registryItem.CustomName, backupValue, registryItem.ValueKind);
                
                _logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    registryItem.CustomName,
                    backupValue
                );
            }
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

            bool mainValueEqualsDesired = RegistryHelper.ValuesEqual(
                mainValue,
                registryItem.DesiredValue,
                registryItem.ValueKind
            );

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
                RegistryHelper.RestoreDefaultValue(
                    registryItem,
                    _softwareKey
                );

                _logger.Verbose(
                    "{RegistryItem_Name} was deleted in the main registry.",
                    registryItem.CustomName
                );
            }
        }
    }
}