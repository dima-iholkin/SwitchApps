using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Security.Principal;
using Serilog;
using Serilog.Core;
using SwitchApps.Library._Helpers;
using SwitchApps.Library.Registry;
using SwitchApps.Library.Registry.Model;
using SwitchApps.Library.StartMenu;
using SwitchApps.Library.TaskScheduler;
using SwitchApps_Library.MenuAnimation;

namespace SwitchApps.Library
{
    [RunInstaller(true)]
    public partial class SwitchAppsInstaller : Installer, IDisposable
    {
        // Init:

        private readonly Logger _logger;
        private readonly RegistryManager _registryManager;
        private readonly TaskSchedulerManager _taskSchedulerManager;
        private readonly StartMenuManager _startMenuManager;
        private readonly MenuAnimationManager _menuAnimationManager;

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

            this._registryManager = new RegistryManager(managedRegistryItems);
            this._taskSchedulerManager = new TaskSchedulerManager(this._logger);
            this._startMenuManager = new StartMenuManager();
            this._menuAnimationManager = new MenuAnimationManager(this._logger);

            this._logger.Information("Login username: {LoginUsername}.", InstallerHelper.LoginUsername);

            this._logger.Information(
                "WindowsIdentity install username: {WindowsIdentityUsername}.", WindowsIdentity.GetCurrent().Name
            );
        }

        // Implement the hooks:

        protected override void OnAfterInstall(IDictionary savedState)
        {
            try
            {
                this._registryManager.BackupRegistry();
                this._registryManager.ModifyRegistry();

                this._menuAnimationManager.BackupSetting();
                this._menuAnimationManager.ModifySetting();

                this._taskSchedulerManager.CreateTask().Run();

                this._startMenuManager.CreateShortcuts();
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
                this._taskSchedulerManager.DeleteTask();

                this._registryManager.RestoreRegistry();

                this._menuAnimationManager.RestoreSetting();

                this._registryManager.DeleteBackupTree();

                this._startMenuManager.DeleteShortcuts();
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Uninstaller caught an error. Uninstall with finish regardless.");

                // No op.

                // Meaning if there are errors in my code, it should still perform the uninstall successfully.
                // As it would be a complete disaster not being able to uninstall the app.
            }
            finally
            {
                base.OnBeforeUninstall(savedState);
            }
        }
    }
}