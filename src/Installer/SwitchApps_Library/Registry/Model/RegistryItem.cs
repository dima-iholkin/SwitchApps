using System;
using Microsoft.Win32;



namespace SwitchApps.Library.Registry.Model
{


    public class RegistryItem
    {
        public RegistryItem(
            string mainEntryPath,
            string mainEntryName,
            string backupEntryName,
            RegistryItemValue desiredValue,
            RegistryItemValue systemDefaultValue
        )
        {
            MainEntryPath = mainEntryPath;

            if (mainEntryName != null && backupEntryName == null)
            {
                MainEntryName = mainEntryName;
                // Everything is right.

                BackupEntryName = mainEntryName;
                // BackupEntryName is used for naming the Backup registry entries
                // and for referring to the items. Therefore must be present for every record.
            }
            else if (mainEntryName == null && backupEntryName != null)
            {
                BackupEntryName = backupEntryName;
                // Everything is right.
            }
            else if (
                mainEntryName == null && backupEntryName == null
                || mainEntryName != null && backupEntryName != null
            )
            {
                throw new ArgumentException(
                    $"Exactly one of {nameof(mainEntryName)} and {nameof(backupEntryName)} must be specified.");
                // If the contructor recieves both MainEntryName and BackupEntryName,
                // it's a wrong usage of it.
            }
            else
            {
                throw new Exception("Something went wrong in the application code.");
                // Should never get here.
            }

            DesiredValue = desiredValue;

            SystemDefaultValue = systemDefaultValue;
        }



        public string MainEntryPath { get; }

        public string MainEntryName { get; }

        public string BackupEntryName { get; }

        public RegistryItemValue DesiredValue { get; }

        public RegistryItemValue SystemDefaultValue { get; }



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



        public string BackupEntryWasPresentName
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



        public RegistryValueKind ValueKind
        {
            get
            {
                return DesiredValue.Match(
                    (int _) => RegistryValueKind.DWord,
                    (string _) => RegistryValueKind.String
                );
            }
        }
    }
}