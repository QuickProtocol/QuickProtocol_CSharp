using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.Utils
{
    public class ByteUtils
    {
        public static byte[] HexDecode(string data)
        {
            var buffer = new byte[data.Length / 2];
            for (var i = 0; i < data.Length; i += 2)
            {
                buffer[i / 2] = byte.Parse(data.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return buffer;
        }

        /// <summary>
        /// 字节数组 -> 整型数字(4字节)(大端字节序)
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="startIndex">起始字节序号(可选)</param>
        /// <returns></returns>
        public static int B2I_BE(byte[] buffer, int startIndex = 0)
        {
            //如果是小端字节序，则交换
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer, startIndex, sizeof(int));

            var ret = BitConverter.ToInt32(buffer, startIndex);

            //如果是小端字节序，则交换
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer, startIndex, sizeof(int));

            return ret;
        }

        /// <summary>
        /// 字节数组 -> 整型数字(4字节)(小端字节序)
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="startIndex">起始字节序号(可选)</param>
        /// <returns></returns>
        public static int B2I_LE(byte[] buffer, int startIndex = 0)
        {
            //如果是大端字节序，则交换
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buffer, startIndex, sizeof(int));

            var ret = BitConverter.ToInt32(buffer, startIndex);

            //如果是大端字节序，则交换
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buffer, startIndex, sizeof(int));

            return ret;
        }
    }
}
