using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Utilities
{
    public static class GuidUtilities
    {
        public static Guid ToGuid(this int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static int ToInt(this Guid value)
        {
            byte[] gb = value.ToByteArray();
            int i = BitConverter.ToInt32(gb, 0);
            return i;
        }
    }
}
