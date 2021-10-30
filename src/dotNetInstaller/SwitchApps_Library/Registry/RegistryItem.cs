using System;
using OneOf;



namespace SwitchApps.Library.Registry
{


    public class RegistryItem
    {
        public RegistryItem(
            string mainEntryPath,
            string mainEntryName,
            string backupEntryName,
            OneOf<int, string> desiredValue,
            OneOf<int, string> systemDefaultValue
        )
        {
            MainEntryPath = mainEntryPath;

            if (mainEntryName != null && backupEntryName == null)
            {
                MainEntryName = mainEntryName;
            }
            else if (mainEntryName == null && backupEntryName != null)
            {
                BackupEntryName = backupEntryName;
            }
            else if (mainEntryName != null && backupEntryName != null)
            {
                throw new ArgumentException(
                    $"Both {nameof(mainEntryName)} and {nameof(backupEntryName)} cannot be specified.");
            }
            else if (mainEntryName == null && backupEntryName == null)
            {
                throw new ArgumentException(
                    $"One of {nameof(mainEntryName)} and {nameof(backupEntryName)} must be specified.");
            }
            else
            {
                throw new Exception("Something went wrong in the application code.");
            }

            DesiredValue = desiredValue;

            SystemDefaultValue = systemDefaultValue;
        }



        public string MainEntryPath { get; }

        public string MainEntryName { get; }

        public string BackupEntryName { get; }

        public OneOf<int, string> DesiredValue { get; }

        public OneOf<int, string> SystemDefaultValue { get; }



        public bool IsDefaultEntry
        {
            get
            {
                if (MainEntryName != null)
                {
                    return true;
                }
                else if (MainEntryName == null)
                {
                    return false;
                }
                else
                {
                    throw new Exception("Something went wrong in the application code.");
                }
            }
        }



        public string GetBackupEntry_WasPresentName
        {
            get
            {
                if (IsDefaultEntry)
                {
                    return BackupEntryName + "_WasPresent";
                }
                else
                {
                    return MainEntryName + "_WasPresent";
                }
            }
        }
    }
}