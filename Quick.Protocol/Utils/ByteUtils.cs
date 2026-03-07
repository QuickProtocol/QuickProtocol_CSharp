using System;
using System.Runtime.InteropServices;

namespace Quick.Protocol.Utils
{
    public class ByteUtils
    {
        public static byte[] HexDecode(string data) => Convert.FromHexString(data);

        public static void HexDecode(string data, Memory<byte> memory)
        {
            var span = memory.Span;
            var charSpan = data.AsSpan();
            for (var i = 0; i < data.Length; i += 2)
            {
                span[i / 2] = byte.Parse(charSpan.Slice(0, 2), System.Globalization.NumberStyles.HexNumber);
                charSpan = charSpan.Slice(2);
            }
        }

        /// <summary>
        /// 字节数组 -> 整型数字(4字节)(大端字节序)
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="startIndex">起始字节序号(可选)</param>
        /// <returns></returns>
        public static int B2I_BE(byte[] buffer, int startIndex = 0)
        {
            var ret = BitConverter.ToInt32(buffer, startIndex);
            //如果是小端字节序，则交换
            if (BitConverter.IsLittleEndian)
            {
                var intSpan = MemoryMarshal.CreateSpan(ref ret, 1);
                var byteSpan = MemoryMarshal.AsBytes(intSpan);
                byteSpan.Reverse();
            }
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
            var ret = BitConverter.ToInt32(buffer, startIndex);
            //如果是大端字节序，则交换
            if (!BitConverter.IsLittleEndian)
            {
                var intSpan = MemoryMarshal.CreateSpan(ref ret, 1);
                var byteSpan = MemoryMarshal.AsBytes(intSpan);
                byteSpan.Reverse();
            }
            return ret;
        }
    }
}
