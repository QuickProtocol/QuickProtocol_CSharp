using System;
using System.Security.Cryptography;
using System.Text;

namespace Quick.Protocol.Utils
{
    public class CryptographyUtils
    {
        public static string ComputeMD5Hash(string data)
        {
            var buffer = ComputeMD5Hash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(buffer).ToLower();
        }

        public static byte[] ComputeMD5Hash(byte[] data)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(data);
        }
    }
}
