using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Quick.Protocol.Utils
{
    public class CryptographyUtils
    {
        public static String ComputeMD5Hash(String data)
        {
            var buffer = ComputeMD5Hash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }

        public static byte[] ComputeMD5Hash(byte[] data)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(data);
        }
    }
}
