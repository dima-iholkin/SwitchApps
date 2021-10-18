using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;



namespace Installer.EditRegistry
{


    internal class Program
    {
        static void Main(string[] args)
        {
            MessageBox.Show(Environment.UserName);
            MessageBox.Show(WindowsIdentity.GetCurrent().User.ToString());

            //var a1 = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
            //var a2 = a1.OpenSubKey()

            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\SwitchApps02";
            const string keyName = userRoot + "\\" + subkey;
            object value = Registry.GetValue(keyName, "TestValue", "no value");
            //Registry.GetValue()
            if (value == null)
            {
                MessageBox.Show("value is null.");
                return;
            }

            MessageBox.Show(value.ToString());
        }
    }
}