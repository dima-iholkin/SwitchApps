﻿using Microsoft.Win32;
using SwitchApps.Library.Registry;



namespace SwitchApps.Library
{


    public static class SwitchAppsRegistryItems
    {
        public static readonly RegistryItem ThumbnailPreviewSize = new RegistryItem
        {
            Path = @"Microsoft\Windows\CurrentVersion\Explorer\Taskband",
            Name = "MinThumbSizePx",
            ValueKind = RegistryValueKind.DWord,
            DesiredValue = 800
        };

        public static readonly RegistryItem ThumbnailPreviewDelay = new RegistryItem
        {
            Path = @"Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            Name = "ExtendedUIHoverTime",
            ValueKind = RegistryValueKind.DWord,
            DesiredValue = 0
        };

        public static readonly RegistryItem MsOfficeAdPopup = new RegistryItem
        {
            Path = @"Classes\ms-officeapp\Shell\Open\Command",
            CustomName = "MsOfficeAdPopup",
            IsDefault = true,
            ValueKind = RegistryValueKind.String,
            DesiredValue = "rundll32"
        };
    }
}