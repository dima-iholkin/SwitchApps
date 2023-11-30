using System;
using Microsoft.Win32;
using Serilog.Core;
using SwitchApps.Library.Registry.Exceptions;
using SwitchApps.Library.Registry.Singletons;

namespace SwitchApps_Library.MenuAnimation
{
    internal class MenuAnimationManager
    {
        private const string BackupEntryName = "MenuAnimation";
        private const bool DefaultValue = true;

        // Init:

        public MenuAnimationManager(Logger logger)
        {
            this._logger = logger;
            this._menuAnimationUtility = new MenuAnimationUtility(logger);
        }

        private readonly Logger _logger;
        private readonly MenuAnimationUtility _menuAnimationUtility;

        // Public methods:

        public void BackupSetting()
        {
            bool? menuAnimationValue = this._menuAnimationUtility.GetMenuAnimation();

            if (menuAnimationValue == null)
            {
                this._logger.Warning("Backup entry {BackupEntryName} not set.", MenuAnimationManager.BackupEntryName);
                return;
            }

            BackupSubkey.Instance.SetValue(
                MenuAnimationManager.BackupEntryName,
                (bool)menuAnimationValue ? 1 : 0,
                RegistryValueKind.DWord
            );

            this._logger.Information("Backup entry {BackupEntryName} value {Value} written into the backup registry.",
                MenuAnimationManager.BackupEntryName,
                (bool)menuAnimationValue ? 1 : 0
            );
        }

        public void ModifySetting()
        {
            this._menuAnimationUtility.SetMenuAnimation(false);

            this._logger.Information("MenuAnimation UI modified to {MenuAnimationUI}", false);
        }

        public void RestoreSetting()
        {
            // Get backup value:

            bool? backupValue;

            try
            {
                object backupObj = BackupSubkey.Instance.GetValue(MenuAnimationManager.BackupEntryName);

                switch (backupObj)
                {
                    case Int32 i:
                        backupValue =
                            i == 1 ? true : false;
                        break;
                    case null:
                        backupValue = null;
                        break;
                    default:
                        throw new InvalidCastException(
                            $"Expected integer or null. Encountered value {backupObj} of type {backupObj.GetType()}."
                        );
                };
            }
            catch (Exception)
            {
                throw new BackupRegistryRecordCorruptedException();
            }

            // Get current value:

            bool? currentValue = this._menuAnimationUtility.GetMenuAnimation();

            // Restore the setting to backup or default:

            if (currentValue == null)
            {
                bool newValue = backupValue.HasValue ? backupValue.Value : MenuAnimationManager.DefaultValue;

                this._menuAnimationUtility.SetMenuAnimation(newValue);

                this._logger.Information("MenuAnimation UI setting set to {MenuAnimationUI}", newValue);
            }
            else if (backupValue == null && currentValue != MenuAnimationManager.DefaultValue)
            {
                this._menuAnimationUtility.SetMenuAnimation(MenuAnimationManager.DefaultValue);

                this._logger.Information("MenuAnimation UI setting set to {MenuAnimationUI}", MenuAnimationManager.DefaultValue);
            }
            else if (backupValue.HasValue && currentValue != backupValue.Value)
            {
                this._menuAnimationUtility.SetMenuAnimation(backupValue.Value);

                this._logger.Information("MenuAnimation UI setting set to {MenuAnimationUI}", backupValue.Value);
            }

            // Delete the backup entry:

            BackupSubkey.Instance.DeleteValue(
                MenuAnimationManager.BackupEntryName,
                throwOnMissingValue: false
            );

            this._logger.Information(
                "Backup entry {BackupEntryName} was deleted from the backup registry.",
                MenuAnimationManager.BackupEntryName
            );
        }
    }
}