using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using Microsoft.Win32;
using Serilog.Core;



namespace SwitchApps.Library
{


    [RunInstaller(true)]
    public partial class SwitchAppsInstaller : Installer, IDisposable
    {
        private string _installedDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SwitchApps"
        );

        private List<RegistryItem> registryItemsToEdit = new List<RegistryItem>
        {
            SwitchAppsRegistryItems.ThumbnailPreviewSize,
            SwitchAppsRegistryItems.ThumbnailPreviewDelay,
            SwitchAppsRegistryItems.MsOfficeAdPopup
        };

        private string _loginUsername;
        private string _loginSID;

        private RegistryKey _usersSoftwareSubkey;
        private RegistryKey _backupsSubkey;

        private RegistryMaker _registryMaker;

        private TaskSchedulerMaker _taskSchedulerMaker;

        private Logger _logger;



        public SwitchAppsInstaller()
        {
            _logger = InstallerHelper.InitializeLogger(_installedDir);

            _logger.Verbose("Installer started.");

            InitializeComponent();

            _loginUsername = InstallerHelper.GetLoginUsername();

            _loginSID = InstallerHelper.GetLoginSID(_loginUsername);

            _usersSoftwareSubkey = Registry.Users
                .CreateSubKey(_loginSID)
                .CreateSubKey("SOFTWARE");

            _backupsSubkey = _usersSoftwareSubkey
                .CreateSubKey(@"SwitchApps\Backup");

            _registryMaker = new RegistryMaker(
                _usersSoftwareSubkey,
                _backupsSubkey,
                registryItemsToEdit,
                _logger
            );

            _taskSchedulerMaker = new TaskSchedulerMaker(
                _installedDir,
                _loginUsername,
                _logger
            );
        }



        public override void Commit(IDictionary savedState)
        {
            _logger.Verbose("{MethodName} started.", nameof(Commit));

            base.Commit(savedState);

            _logger.Verbose("Start registry backup.");

            _registryMaker.BackupRegistry();

            _logger.Verbose("Finish registry backup.");

            _logger.Verbose("Start registry modification.");

            _registryMaker.ModifyRegistry();

            _logger.Verbose("Finish registry modification.");

            _taskSchedulerMaker.CreateTaskSchedulerTaskAndRun();

            _logger.Verbose("{MethodName} finished.", nameof(Commit));
        }



        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            _logger.Verbose("{MethodName} started.", nameof(OnBeforeUninstall));

            _taskSchedulerMaker.DeleteTaskSchedulerTask();

            base.OnBeforeUninstall(savedState);

            _logger.Verbose("Start registry restore.");

            _registryMaker.RestoreRegistry();

            _logger.Verbose("Finish registry restore.");

            _usersSoftwareSubkey.DeleteSubKeyTree("SwitchApps");

            _logger.Verbose("Deleted the registry subtree SwitchApps.");

            _logger.Verbose("{MethodName} finished.", nameof(OnBeforeUninstall));
        }
    }
}