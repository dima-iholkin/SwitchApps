using Microsoft.Win32;



namespace SwitchApps.Library.Registry
{


    public class RegistryItem
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



        private string _customName;
        public string CustomName
        {
            get
            {
                if (IsDefault)
                {
                    return _customName;
                }
                else
                {
                    return Name;
                }
            }
            set
            {
                _customName = value;
            }
        }



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

        public object DesiredValue { get; set; }

        public bool IsDefault { get; set; }
    }
}