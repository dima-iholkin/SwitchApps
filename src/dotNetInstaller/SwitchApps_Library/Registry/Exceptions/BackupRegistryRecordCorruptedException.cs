using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



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