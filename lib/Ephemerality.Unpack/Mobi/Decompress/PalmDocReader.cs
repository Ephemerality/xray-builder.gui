using System;

namespace Ephemerality.Unpack.Mobi.Decompress
{
    public sealed class PalmDocReader : Decompressor
    {
        public override byte[] Unpack(byte[] data)
        {
            var p = 0;
            var o = new byte[4096];
            var op = 0;
            while (p < data.Length)
            {
                int c = data[p++];
                if (c >= 1 && c <= 8)
                {
                    for (var i = 1; i <= c; i++)
                    {
                        o[op++] = data[p++];
                    }
                }
                else if (c < 128)
                    o[op++] = (byte)c;
                else if (c >= 192)
                {
                    o[op++] = (byte)' ';
                    o[op++] = (byte)(c ^ 128);
                }
                else if (p < data.Length)
                {
                    c = (c << 8) | data[p++];
                    var m = (c >> 3) & 0x07ff;
                    var n = (c & 7) + 3;
                    if (m > n)
                    {
                        Array.Copy(o, op - m, o, op, n);
                        op += n;
                    }
                    else
                    {
                        for (var i = 0; i < n; i++)
                        {
                            Array.Copy(o, op - m, o, op, 1);
                            op++;
                        }
                    }
                }
            }
            var temp = new byte[op];
            Array.Copy(o, temp, op);
            return temp;
        }
    }
}