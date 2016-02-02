using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace UrlShortenerService.Extensions
{
    public static class EncodingExtensions
    {
        private static string Base62CodingSpace = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string ToBase62(this byte[] numberInbytes)
        {
            byte[] buffer = new byte[8];
            Buffer.BlockCopy(numberInbytes,0,buffer,0,numberInbytes.Length);

            ulong number = BitConverter.ToUInt64(buffer, 0);

            var alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var n = number;
            ulong basis = 62;
            var ret = "";
            while (n > 0)
            {
                ulong temp = n % basis;
                ret = alphabet[(int)temp] + ret;
                n = (n / basis);

            }
            return ret;
        }
    }
}
