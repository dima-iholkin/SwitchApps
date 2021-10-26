using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog.Core;

namespace SwitchApps_Library
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
                BackupRegistryItem(ri);
            });
        }



        private void BackupRegistryItem(RegistryItem registryItem)
        {
            var key = _softwareKey.OpenSubKey(registryItem.Path);

            object value = key?.GetValue(registryItem.Name);

            if (value is null)
            {
                _backupKey.SetValue(registryItem.IsPresent_Name, 0, RegistryValueKind.DWord);
                _logger.Verbose(
                    "{RegistryItem_Name} not present in the registry. 0 written into the backup registry.",
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

                _backupKey.SetValue(registryItem.CustomName, value, registryItem.ValueKind);
                _logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    registryItem.CustomName,
                    value
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
            bool DesiredValueWasUnchanged;
            object currentValue = _softwareKey
                .CreateSubKey(registryItem.Path)
                .GetValue(registryItem.Name);

            if (registryItem.ValueKind == RegistryValueKind.DWord)
            {
                DesiredValueWasUnchanged = (int)currentValue == (int)registryItem.DesiredValue;
            }
            else if (registryItem.ValueKind == RegistryValueKind.String)
            {
                DesiredValueWasUnchanged = currentValue.ToString() == registryItem.DesiredValue.ToString();
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Unexpected RegistryValueKind for {registryItem.Name}.");
            }

            _logger.Verbose(
                "{RegistryItem_Name} current value {CurrentValue} and desired value {DesiredValue}. " +
                    "Desired value wasn't changed: {DesiredValueWasntChanged}.",
                registryItem.CustomName,
                currentValue,
                registryItem.DesiredValue,
                DesiredValueWasUnchanged
            );

            if (DesiredValueWasUnchanged == false)
            {
                _logger.Verbose("{RegistryItem_Name} left as is.", registryItem.CustomName);

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
                _softwareKey
                    .CreateSubKey(registryItem.Path)
                    .DeleteValue(registryItem.Name);

                _logger.Verbose(
                    "{RegistryItem_Name} name was deleted in the main registry.",
                    registryItem.CustomName
                );
            }
        }
    }
}