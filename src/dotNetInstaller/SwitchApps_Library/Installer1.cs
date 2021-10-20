using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using Microsoft.Win32.TaskScheduler;
using System.Windows;



namespace SwitchApps_Library
{


    [RunInstaller(true)]
    public partial class Installer1 : Installer, IDisposable
    {
        private Logger _logger;

        private string _installedDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SwitchApps"
        );

        private string _loginUsername;
        private string _loginSID;
        private RegistryKey _usersSoftwareSubkey;
        private RegistryKey _backupsSubkey;

        private readonly RegistryItem _thumbnailPreviewSize = new RegistryItem
        {
            Path = @"Microsoft\Windows\CurrentVersion\Explorer\Taskband",
            Name = "MinThumbSizePx",
            ValueKind = RegistryValueKind.DWord,
            NewValue = 800
        };

        private readonly RegistryItem _thumbnailPreviewDelay = new RegistryItem
        {
            Path = @"Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            Name = "ExtendedUIHoverTime",
            ValueKind = RegistryValueKind.DWord,
            NewValue = 0
        };

        private readonly RegistryItem _msOfficeAdPopup = new RegistryItem
        {
            Path = @"Classes\ms-officeapp\Shell\Open\Command",
            Name = null,
            CustomName = "MsOfficeAdPopup",
            IsDefault = true,
            ValueKind = RegistryValueKind.String,
            NewValue = "rundll32"
        };

        public Installer1()
        {
            string logFile = Path.Combine(_installedDir, "log.txt");

            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logFile)
                .CreateLogger();

            _logger.Verbose("Installer started.");

            InitializeComponent();

            // Get the login username, not the account under which this process runs:
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            _loginUsername = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];

            // Get this login's SID:
            NTAccount f = new NTAccount(_loginUsername);
            SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
            _loginSID = s.ToString();

            _usersSoftwareSubkey = Registry.Users
                .CreateSubKey(_loginSID)
                .CreateSubKey("SOFTWARE");

            _backupsSubkey = _usersSoftwareSubkey
                .CreateSubKey(@"SwitchApps\Backup");
        }



        protected override void OnBeforeInstall(IDictionary savedState)
        {
            _logger.Verbose("{MethodName} method started.", nameof(OnBeforeInstall));

            base.OnBeforeInstall(savedState);

            //this.LogSomeInfoIntoFile();

            /*
            _logger.Verbose("Registry values backup started.");
            this.BackupRegValues();
            _logger.Verbose("Registry values backup finished.");

            _logger.Verbose("Registry values modification started.");
            this.ModifyRegValues();
            _logger.Verbose("Registry values modification finished.");
            */

            _logger.Verbose("{MethodName} method finished.", nameof(OnBeforeInstall));
        }



        protected override void OnAfterInstall(IDictionary savedState)
        {
            _logger.Verbose("{MethodName} method started.", nameof(OnAfterInstall));

            base.OnAfterInstall(savedState);

            this.CreateTaskSchedulerTaskAndRun();

            _logger.Verbose("{MethodName} method finished.", nameof(OnAfterInstall));
        }



        //public override void Uninstall(IDictionary savedState)
        //{
        //    MessageBox.Show("Hello");

        //    base.Uninstall(savedState);
        //}



        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            _logger.Verbose("{MethodName} method started.", nameof(OnBeforeUninstall));

            this.DeleteTaskSchedulerTask();

            base.OnBeforeUninstall(savedState);

            /*
            _logger.Verbose("Registry values restore started.");
            RestoreRegValues();
            _logger.Verbose("Registry values restore finished.");

            _usersSoftwareSubkey.DeleteSubKeyTree("SwitchApps");
            _logger.Verbose("Deleted the main registry's subtree SwitchApps.");
            */

            _logger.Verbose("{MethodName} method finished.", nameof(OnBeforeUninstall));
        }



        private void BackupRegValues()
        {
            this._BackupRegValue(_thumbnailPreviewSize);

            this._BackupRegValue(_thumbnailPreviewDelay);

            this._BackupRegValue(_msOfficeAdPopup);
        }



        private void _BackupRegValue(RegistryItem registryItem)
        {
            var key = _usersSoftwareSubkey.OpenSubKey(registryItem.Path);

            object value = key?.GetValue(registryItem.Name);

            if (value is null)
            {
                _backupsSubkey.SetValue(registryItem.IsPresent_Name, 0, RegistryValueKind.DWord);
                _logger.Verbose(
                    "{RegistryItem_Name} not present in the registry. 0 written into the backup registry.",
                    registryItem.CustomName
                );

                _backupsSubkey.DeleteValue(
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
                _backupsSubkey.SetValue(registryItem.IsPresent_Name, 1, RegistryValueKind.DWord);
                _logger.Verbose(
                    "{RegistryItem_Name} present in the registry. 1 written into the backup registry.",
                    registryItem.CustomName
                );

                _backupsSubkey.SetValue(registryItem.CustomName, value, registryItem.ValueKind);
                _logger.Verbose(
                    "{RegistryItem_Name} value {RegistryItem_Value} written into the backup registry.",
                    registryItem.CustomName,
                    value
                );
            }
        }



        private void ModifyRegValues()
        {
            this._ModifyRegValue(_thumbnailPreviewSize);

            this._ModifyRegValue(_thumbnailPreviewDelay);

            this._ModifyRegValue(_msOfficeAdPopup);
        }



        private void _ModifyRegValue(RegistryItem registryItem)
        {
            this._usersSoftwareSubkey
                .CreateSubKey(registryItem.Path)
                .SetValue(
                    registryItem.Name,
                    registryItem.NewValue
                );
            _logger.Verbose(
                "{RegistryItem_Name} value {RegistryItem_Value} written into the main registry.",
                registryItem.CustomName,
                registryItem.NewValue
            );
        }



        private void CreateTaskSchedulerTaskAndRun()
        {
            TaskDefinition td = TaskService.Instance.NewTask();
            //td.RegistrationInfo.Description = "SwitchApps autostart";
            td.Principal.UserId = _loginUsername;
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.Highest;

            LogonTrigger logonTrigger = new LogonTrigger();
            logonTrigger.UserId = _loginUsername;
            td.Triggers.Add(logonTrigger);

            var pathToExe = Path.Combine(_installedDir, "SwitchApps.exe");
            td.Actions.Add(new ExecAction(pathToExe));
            td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.RealTime;
            td.Settings.AllowDemandStart = true;
            td.Settings.AllowHardTerminate = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

            var task = TaskService.Instance.RootFolder
                .CreateFolder("SwitchApps", exceptionOnExists: false)
                .RegisterTaskDefinition("SwitchApps autostart", td);

            task.Run();
        }



        public void DeleteTaskSchedulerTask()
        {
            TaskService.Instance.RootFolder
                .SubFolders.Where(f => f.Name == "SwitchApps")
                .FirstOrDefault().Tasks.ToList().ForEach(task =>
                {
                    task.Stop();
                    task.Folder.DeleteTask(task.Name);
                });

            TaskService.Instance.RootFolder.DeleteFolder("SwitchApps");
        }



        private void RestoreRegValues()
        {
            _RestoreRegValue(_thumbnailPreviewSize);

            _RestoreRegValue(_thumbnailPreviewDelay);

            _RestoreRegValue(_msOfficeAdPopup);
        }



        private void _RestoreRegValue(RegistryItem registryItem)
        {
            bool DesiredValueWasntChanged;
            object currentValue = _usersSoftwareSubkey
                .CreateSubKey(registryItem.Path)
                .GetValue(registryItem.Name);
            if (registryItem.ValueKind == RegistryValueKind.DWord)
            {
                DesiredValueWasntChanged = (int)currentValue == (int)registryItem.NewValue;
            }
            else if (registryItem.ValueKind == RegistryValueKind.String)
            {
                DesiredValueWasntChanged = currentValue.ToString() == registryItem.NewValue.ToString();
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
                registryItem.NewValue,
                DesiredValueWasntChanged
            );

            if (DesiredValueWasntChanged == false)
            {
                _logger.Verbose("{RegistryItem_Name} left as is.", registryItem.CustomName);

                return;
            }

            bool wasPresent;
            int wasPresent_int = (int)_backupsSubkey.GetValue(registryItem.IsPresent_Name);
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
                object backupValue = _backupsSubkey.GetValue(registryItem.CustomName);

                _usersSoftwareSubkey
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
                _usersSoftwareSubkey
                    .CreateSubKey(registryItem.Path)
                    .DeleteValue(registryItem.Name);
                _logger.Verbose(
                    "{RegistryItem_Name} name was deleted in the main registry.",
                    registryItem.CustomName
                );
            }
        }



        private void LogSomeInfoIntoFile()
        {
            string path = @"C:\_temp\01\Log_manual.txt";
            var streamWriter = File.AppendText(path);

            streamWriter.WriteLine(DateTime.Now);
            streamWriter.WriteLine("Run as account: " + WindowsIdentity.GetCurrent().Name);

            // Get the login username, not the account under which this process runs:
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            string loginUsername = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];

            streamWriter.WriteLine("Login username: " + loginUsername);

            // Get this login's SID:
            NTAccount f = new NTAccount(loginUsername);
            SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
            string loginSID = s.ToString();

            streamWriter.WriteLine("Login SID: " + loginSID);

            // Get the execution directory:
            streamWriter.WriteLine("Current directory: " + Environment.CurrentDirectory);
            streamWriter.WriteLine("Installed directory: " + _installedDir);

            var softwareSubkey = Registry.Users
                .OpenSubKey(loginSID)
                .OpenSubKey("SOFTWARE");

            streamWriter.WriteLine();

            streamWriter.Close();
        }
    }
}