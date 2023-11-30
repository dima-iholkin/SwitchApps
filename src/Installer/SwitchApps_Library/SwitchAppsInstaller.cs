using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using Serilog;
using Serilog.Core;
using SwitchApps.Library._Helpers;
using SwitchApps.Library.Registry;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.Registry.Singletons;
using SwitchApps.Library.StartMenu;
using SwitchApps.Library.TaskScheduler;
using SwitchApps_Library.MenuAnimation;

namespace SwitchApps.Library
{
    [RunInstaller(true)]
    public partial class SwitchAppsInstaller : Installer, IDisposable
    {
        // Init:

        public SwitchAppsInstaller()
        {
            InstallerHelper.InitializeStaticLogger(InstallerHelper.InstalledDir);
            this._logger = (Logger)Log.Logger;

            InitializeComponent();

            List<RegistryItem> managedRegistryItems = new List<RegistryItem>
            {
                SwitchAppsRegistryItems.ThumbnailPreviewSize,
                SwitchAppsRegistryItems.ThumbnailPreviewDelay,
                SwitchAppsRegistryItems.MsOfficeAdPopup
            };

            this._registryMaker = new RegistryMaker(managedRegistryItems);
            this._taskSchedulerMaker = new TaskSchedulerMaker();
            this._startMenuMaker = new StartMenuMaker();
            this._menuAnimationManager = new MenuAnimationManager(_logger);
        }

        private readonly Logger _logger;
        private readonly RegistryMaker _registryMaker;
        private readonly TaskSchedulerMaker _taskSchedulerMaker;
        private readonly StartMenuMaker _startMenuMaker;
        private readonly MenuAnimationManager _menuAnimationManager;

        // Using the hooks:

        protected override void OnAfterInstall(IDictionary savedState)
        {
            try
            {
                this._registryMaker.BackupRegistry();
                this._logger.Information("Finished the registry backup.");

                this._menuAnimationManager.BackupSetting();
                this._logger.Information("Finished the MenuAnimation backup.");

                this._registryMaker.ModifyRegistry();
                this._logger.Information("Finished the registry modification.");

                this._menuAnimationManager.ModifySetting();
                this._logger.Information("Finished the MenuAnimation modification.");

                this._taskSchedulerMaker.CreateTask()
                    .Run();
                this._logger.Information("Created the scheduled task.");

                this._startMenuMaker.CreateShortcuts();
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Installer caught an error. Installation canceled.");
                throw;
            }

            base.OnAfterInstall(savedState);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            try
            {
                this._taskSchedulerMaker.DeleteTask();
                this._logger.Information("Deleted the scheduled task subtree SwitchApps.");

                this._registryMaker.RestoreRegistry();
                this._logger.Information("Finished the registry restore.");

                this._menuAnimationManager.RestoreSetting();
                this._logger.Information("Finished the MenuAnimation restore.");

                SoftwareSubkey.Instance.DeleteSubKeyTree("SwitchApps");
                this._logger.Information("Deleted the backup registry subtree SwitchApps.");

                this._startMenuMaker.DeleteShortcuts();
            }
            catch (Exception ex)
            {
                // No op.
                this._logger.Error(ex, "Uninstaller caught an error. Uninstall with finish regardless.");

                // Meaning if there are errors in my code, it should still perform the uninstall successfully.
                // As it would be a complete disaster to not being able to uninstall an app.
            }
            finally
            {
                base.OnBeforeUninstall(savedState);
            }
        }
    }
}