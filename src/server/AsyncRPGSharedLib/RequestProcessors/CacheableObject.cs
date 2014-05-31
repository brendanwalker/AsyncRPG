using System;
using AsyncRPGSharedLib.Database;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public abstract class CacheableObject
    {
        public bool IsDirty { get; set; }

        public abstract void WriteDirtyObjectToDatabase(AsyncRPGDataContext db_context);
    }
}
