using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGSharedLib.Utility
{
    public class EnumUtilities
    {
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
