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



namespace SwitchApps.Library
{


    [RunInstaller(true)]
    public partial class SwitchAppsInstaller : Installer, IDisposable
    {
        private RegistryMaker _registryMaker;
        private TaskSchedulerMaker _taskSchedulerMaker;
        private readonly Logger _logger;

        public SwitchAppsInstaller()
        {
            InstallerHelper.InitializeStaticLogger(InstallerHelper.InstalledDir);
            _logger = (Logger)Log.Logger;

            InitializeComponent();

            List<RegistryItem> managedRegistryItems = new List<RegistryItem>
            {
                SwitchAppsRegistryItems.ThumbnailPreviewSize,
                SwitchAppsRegistryItems.ThumbnailPreviewDelay,
                SwitchAppsRegistryItems.MsOfficeAdPopup
            };
            _registryMaker = new RegistryMaker(managedRegistryItems);

            _taskSchedulerMaker = new TaskSchedulerMaker();
        }



        public override void Commit(IDictionary savedState)
        {
            try
            {
                _registryMaker.BackupRegistry();
                _logger.Information("Finished the registry backup.");

                _registryMaker.ModifyRegistry();
                _logger.Information("Finished the registry modification.");

                _taskSchedulerMaker.CreateTaskSchedulerTask()
                    .Run();
                _logger.Information("Created the scheduled task.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Installer caught an error. Installation canceled.");
                throw;
            }

            base.Commit(savedState);
        }



        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            try
            {
                _taskSchedulerMaker.DeleteTaskSchedulerTask();
                _logger.Information("Deleted the scheduled task subtree SwitchApps.");

                _registryMaker.RestoreRegistry();
                _logger.Information("Finished the registry restore.");

                SoftwareSubkey.Instance.DeleteSubKeyTree("SwitchApps");
                _logger.Information("Deleted the backup registry subtree SwitchApps.");
            }
            catch (Exception ex)
            {
                // No op.
                _logger.Error(ex, "Uninstaller caught an error. Uninstall with finish regardless.");

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