using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace SwitchApps_Library
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            //throw new Exception("loooool");

            base.OnBeforeInstall(savedState);

            //MessageBox.Show("lol");

            //const string userRoot = "HKEY_CURRENT_USER";
            //const string subkey = "Software\\SwitchApps02";
            //const string keyName = userRoot + "\\" + subkey;
            //string value = Registry.GetValue(keyName, "TestValue", "no value").;
            //MessageBox.Show(value);

            //Installer1.SetRegistry();
        }

        private static void SetRegistry()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\SwitchApps01\\Backup01";
            const string keyName = userRoot + "\\" + subkey;

            Registry.SetValue(keyName, "TestValue", 1000);
        }
    }
}