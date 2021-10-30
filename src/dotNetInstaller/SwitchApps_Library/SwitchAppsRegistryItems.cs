using SwitchApps.Library.Registry;



namespace SwitchApps.Library
{


    public static class SwitchAppsRegistryItems
    {
        public static readonly RegistryItem ThumbnailPreviewSize = new RegistryItem(
            @"Microsoft\Windows\CurrentVersion\Explorer\Taskband",
            "MinThumbSizePx",
            null,
            800,
            null
        );

        public static readonly RegistryItem ThumbnailPreviewDelay = new RegistryItem(
            @"Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "ExtendedUIHoverTime",
            null,
            0,
            null
        );

        public static readonly RegistryItem MsOfficeAdPopup = new RegistryItem(
            @"Classes\ms-officeapp\Shell\Open\Command",
            null,
            "MsOfficeAdPopup",
            "rundll32",
            null
        );
    }
}