using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using Microsoft.Win32;
using Serilog.Core;



namespace SwitchApps_Library
{


    [RunInstaller(true)]
    public partial class MyInstaller : Installer, IDisposable
    {
        private string _installedDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SwitchApps"
        );

        private List<RegistryItem> registryItemsToEdit = new List<RegistryItem>
        {
            MyRegistryItems.ThumbnailPreviewSize,
            MyRegistryItems.ThumbnailPreviewDelay,
            MyRegistryItems.MsOfficeAdPopup
        };

        private string _loginUsername;
        private string _loginSID;

        private RegistryKey _usersSoftwareSubkey;
        private RegistryKey _backupsSubkey;

        private RegistryMaker _registryMaker;

        private TaskSchedulerMaker _taskSchedulerMaker;

        private Logger _logger;



        public MyInstaller()
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
            _logger.Verbose("{MethodName} method started.", nameof(Commit));

            base.Commit(savedState);

            _logger.Verbose("Registry values backup started.");

            _registryMaker.BackupRegistry();

            _logger.Verbose("Registry values backup finished.");

            _logger.Verbose("Registry values modification started.");

            _registryMaker.ModifyRegistry();

            _logger.Verbose("Registry values modification finished.");

            _taskSchedulerMaker.CreateTaskSchedulerTaskAndRun();

            _logger.Verbose("{MethodName} method finished.", nameof(Commit));
        }



        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            _logger.Verbose("{MethodName} method started.", nameof(OnBeforeUninstall));

            _taskSchedulerMaker.DeleteTaskSchedulerTask();

            base.OnBeforeUninstall(savedState);

            _logger.Verbose("Registry values restore started.");

            _registryMaker.RestoreRegistry();

            _logger.Verbose("Registry values restore finished.");

            _usersSoftwareSubkey.DeleteSubKeyTree("SwitchApps");

            _logger.Verbose("Deleted the main registry's subtree SwitchApps.");

            _logger.Verbose("{MethodName} method finished.", nameof(OnBeforeUninstall));
        }
    }
}