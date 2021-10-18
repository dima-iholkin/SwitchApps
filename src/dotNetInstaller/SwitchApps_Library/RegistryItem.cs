using System;
using Microsoft.Win32;



namespace SwitchApps_Library
{
    internal class RegistryItem
    {

        public string Path { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                if (IsDefault)
                {
                    return null;
                }
                else
                {
                    return _name;
                }
            } 
            set
            {
                _name = value;
            }
        }

        public string CustomName { get; set; }

        public string IsPresent_Name
        {
            get
            {
                if (IsDefault)
                {
                    return CustomName + "_Present";
                }
                else
                {
                    return Name + "_Present";
                }
            }
        }

        public RegistryValueKind ValueKind { get; set; }

        public object NewValue { get; set; }

        public bool IsDefault { get; set; }
    }
}