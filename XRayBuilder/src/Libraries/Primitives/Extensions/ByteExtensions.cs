using System;
using System.Security.Cryptography;

namespace XRayBuilderGUI.Libraries.Primitives.Extensions
{
    public static class ByteExtensions
    {
        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/14333437#14333437
        /// <summary>
        /// Convert a byte array to hex string quickly
        /// </summary>
        public static string ToHexString(this byte[] bytes)
        {
            var c = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }

            return new string(c);
        }

        public static byte[] Sha1(this byte[] bytes)
        {
            using (var sha1 = new SHA1Managed())
                return sha1.ComputeHash(bytes);
        }

        // Stolen from http://www.mobileread.com/forums/showthread.php?t=185565
        /// <summary>
        /// Returns <paramref name="bytes"/> in BigEndian form, reversing if necessary
        /// </summary>
        public static byte[] BigEndian(this byte[] bytes)
        {
            if (bytes == null)
                return null;

            if (!BitConverter.IsLittleEndian)
                return bytes;

            var buffer = (byte[]) bytes.Clone();
            Array.Reverse(buffer);

            return buffer;
        }
    }
}