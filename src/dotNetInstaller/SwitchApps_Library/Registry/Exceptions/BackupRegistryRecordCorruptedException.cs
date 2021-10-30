using System;



namespace SwitchApps.Library.Registry.Exceptions
{


    [Serializable]
    public class BackupRegistryRecordCorruptedException : Exception
    {
        public BackupRegistryRecordCorruptedException() { }

        public BackupRegistryRecordCorruptedException(string message) : base(message) { }
        
        public BackupRegistryRecordCorruptedException(string message, Exception inner) : base(message, inner) { }
        
        protected BackupRegistryRecordCorruptedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context
        ) : base(info, context) { }
    }
}