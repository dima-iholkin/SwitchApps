using Microsoft.Win32;



namespace SwitchApps_Library
{
    internal class RegistryItem
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Present_Name
        {
            get
            {
                return Name + "_Present";
            }
        }
        public RegistryValueKind ValueKind { get; set; }
        public object NewValue { get; set; }
    }
}