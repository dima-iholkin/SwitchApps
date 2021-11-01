using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using Serilog;
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

        public SwitchAppsInstaller()
        {
            InstallerHelper.InitializeStaticLogger(InstallerHelper.InstalledDir);

            InitializeComponent();

            List<RegistryItem> registryItems = new List<RegistryItem>
            {
                SwitchAppsRegistryItems.ThumbnailPreviewSize,
                SwitchAppsRegistryItems.ThumbnailPreviewDelay,
                SwitchAppsRegistryItems.MsOfficeAdPopup
            };
            _registryMaker = new RegistryMaker(registryItems);

            _taskSchedulerMaker = new TaskSchedulerMaker();
        }



        public override void Commit(IDictionary savedState)
        {
            _registryMaker.BackupRegistry();
            Log.Logger.Verbose("Finished registry backup.");

            _registryMaker.ModifyRegistry();
            Log.Logger.Verbose("Finished registry modification.");

            _taskSchedulerMaker.CreateTaskSchedulerTask().Run();
            Log.Logger.Verbose("Created scheduled task.");

            base.Commit(savedState);
        }



        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            _taskSchedulerMaker.DeleteTaskSchedulerTask();
            Log.Logger.Verbose("Deleted scheduled task.");

            _registryMaker.RestoreRegistry();
            Log.Logger.Verbose("Finished registry restore.");

            SoftwareSubkey.Instance.DeleteSubKeyTree("SwitchApps");
            Log.Logger.Verbose("Deleted registry subtree SwitchApps.");

            base.OnBeforeUninstall(savedState);
        }
    }
}