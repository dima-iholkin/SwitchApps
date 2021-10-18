using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;



namespace SwitchApps_Library
{


    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
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
            base.OnBeforeInstall(savedState);

            //this.LogSomeInfoIntoFile();

            this.BackupRegValues();

            this.ModifyRegValues();



            //savedState.

            //MessageBox.Show("lol");

            //const string userRoot = "HKEY_CURRENT_USER";
            //const string subkey = "Software\\SwitchApps02";
            //const string keyName = userRoot + "\\" + subkey;
            //string value = Registry.GetValue(keyName, "TestValue", "no value").;
            //MessageBox.Show(value);

            //Installer1.SetRegistry();

            //this.CreateProcess();
        }



        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            base.OnBeforeUninstall(savedState);

            RestoreRegValues();

            _usersSoftwareSubkey.DeleteSubKeyTree("SwitchApps");
        }



        private void BackupRegValues()
        {
            this._BackupRegValue(_thumbnailPreviewSize);

            this._BackupRegValue(_thumbnailPreviewDelay);

            this._BackupRegValue(_msOfficeAdPopup);
        }



        private void _BackupRegValue(RegistryItem registryItem)
        {
            var key = _usersSoftwareSubkey
                .OpenSubKey(registryItem.Path);

            object value = key?.GetValue(registryItem.Name);

            if (value is null)
            {
                _backupsSubkey.SetValue(registryItem.IsPresent_Name, 0, RegistryValueKind.DWord);

                if (_backupsSubkey.GetValue(registryItem.Name) != null)
                {
                    _backupsSubkey.DeleteValue(registryItem.Name);
                }
                // If the Name exists inside the backup Key, remove it.
            }
            else
            {
                _backupsSubkey.SetValue(registryItem.IsPresent_Name, 1, RegistryValueKind.DWord);

                if (registryItem.IsDefault)
                {
                    _backupsSubkey.SetValue(registryItem.CustomName, value, registryItem.ValueKind);
                }
                else
                {
                    _backupsSubkey.SetValue(registryItem.Name, value, registryItem.ValueKind);
                }
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
        }



        private void RestoreRegValues()
        {
            _RestoreRegValue(_thumbnailPreviewSize);

            _RestoreRegValue(_thumbnailPreviewDelay);

            _RestoreRegValue(_msOfficeAdPopup);
        }



        private void _RestoreRegValue(RegistryItem registryItem)
        {
            int isPresent_int = (int)_backupsSubkey.GetValue(registryItem.IsPresent_Name);

            bool isPresent;
            switch (isPresent_int)
            {
                case 1:
                    isPresent = true;
                    break;
                case 0:
                    isPresent = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unexpected backup registry isPresent value.");
            }

            if (isPresent)
            {
                object backupValue;
                if (registryItem.IsDefault)
                {
                    backupValue = _backupsSubkey.GetValue(registryItem.CustomName);
                }
                else
                {
                    backupValue = _backupsSubkey.GetValue(registryItem.Name);
                }

                object currentValue = _usersSoftwareSubkey
                    .CreateSubKey(registryItem.Path)
                    .GetValue(registryItem.Name);

                bool valueIsEqual;
                if (registryItem.ValueKind == RegistryValueKind.DWord)
                {
                    valueIsEqual = (int)currentValue == (int)backupValue;
                }
                else if (registryItem.ValueKind == RegistryValueKind.String)
                {
                    valueIsEqual = currentValue.ToString() == backupValue.ToString();
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Unexpected RegistryValueKind for {registryItem.Name}.");
                }

                if (valueIsEqual)
                {
                    _usersSoftwareSubkey
                        .CreateSubKey(registryItem.Path)
                        .SetValue(registryItem.Name, backupValue, registryItem.ValueKind);
                }
            }
            else
            {
                object currentValue = _usersSoftwareSubkey
                    .CreateSubKey(registryItem.Path)
                    .GetValue(registryItem.Name);

                if (currentValue == null)
                {
                    _usersSoftwareSubkey
                        .CreateSubKey(registryItem.Path)
                        .DeleteValue(registryItem.Name);
                }
            }
        }



        /*
        <Component Id = "MsOfficeAdPopup_Reg" Guid="*" Permanent="yes">
            <!--<Condition>
                $(var.ModifySystemRegistry) = "true"
            </Condition>-->

            <RegistryKey Root = "HKCU"
                            Key="Software\Classes\ms-officeapp\Shell\Open\Command"
                            ForceCreateOnInstall="yes"
                            ForceDeleteOnUninstall="no" >
                <RegistryValue Type = "string" Value="rundll32" />
            </RegistryKey>
        </Component>

        <Component Id = "ThumbnailPreviewSize_Reg" Guid="*" Permanent="yes">
        <!--<Condition>
            $(var.ModifySystemRegistry) = "true"
        </Condition>-->

        <RegistryKey Root = "HKCU"
                        Key="Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband"
                        ForceCreateOnInstall="yes"
                        ForceDeleteOnUninstall="no" >
            <RegistryValue Name = "MinThumbSizePx" Type="integer" Value="800" />
        </RegistryKey>
        </Component>

        <Component Id = "ThumbnailPreviewDelay_Reg" Guid="*" Permanent="yes">
        <!--<Condition>
            $(var.ModifySystemRegistry) = "true"
        </Condition>-->

        <RegistryKey Root = "HKCU"
                        Key="Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
                        ForceCreateOnInstall="yes"
                        ForceDeleteOnUninstall="no" >
            <RegistryValue Name = "ExtendedUIHoverTime" Type="integer" Value="0" />
        </RegistryKey>
        </Component>
        <!--This code make changes to the System Registry values on Install.-->
        */


        private void LogSomeInfoIntoFile()
        {
            string path = @"C:\_temp\01\Log.txt";
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

            var softwareSubkey = Registry.Users
                .OpenSubKey(loginSID)
                .OpenSubKey("SOFTWARE");

            var testValue = softwareSubkey
                .OpenSubKey("SwitchApps02")
                .GetValue("TestValue");
            streamWriter.WriteLine("TestValue: " + testValue);

            var thumbnailPreviewSize = softwareSubkey
                .OpenSubKey(@"Microsoft\Windows\CurrentVersion\Explorer\Taskband")
                .GetValue("MinThumbSizePx");
            streamWriter.WriteLine("ThumbnailPreviewSize: " + thumbnailPreviewSize);

            streamWriter.WriteLine();

            streamWriter.Close();
        }







        /*
        private static void SetRegistry()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\SwitchApps01\\Backup01";
            const string keyName = userRoot + "\\" + subkey;

            Registry.SetValue(keyName, "TestValue", 1000);
        }
        */


        /*
        private void CreateProcess()
        {
            //MessageBox.Show(WindowsIdentity.GetCurrent().User.ToString());

            //ProcessStartInfo process = new ProcessStartInfo("cmd dir");
            //process.UseShellExecute = true;
            //process.WorkingDirectory
            //Process.Start(process);

            ProcessStartInfo process = new ProcessStartInfo("Installer.EditRegistry.exe");
            //process.Verb = "runas";
            process.UseShellExecute = true;
            process.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SwitchApps";
            //MessageBox.Show(process.WorkingDirectory);
            Process.Start(process);
        }
        */
    }
}