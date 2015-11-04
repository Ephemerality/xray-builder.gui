// This file contains code ported over from KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XRayBuilderGUI.Unpack
{
    public abstract class Decompressor
    {
        public abstract byte[] unpack(byte[] data, uint unpackedSize);
    }

    class UncompressedReader : Decompressor
    {
        public override byte[] unpack(byte[] data, uint unpackedSize)
        {
            return data;
        }
    }

    class PalmDOCReader : Decompressor
    {
        public override byte[] unpack(byte[] data, uint unpackedSize)
        {
            int p = 0;
            byte[] o = new byte[unpackedSize];
            int op = 0;
            while (p < data.Length)
            {
                int c = (int)data[p++];
                if (c >= 1 && c <= 8)
                {
                    for (int i = 1; i <= c; i++)
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
                    int m = (c >> 3) & 0x07ff;
                    int n = (c & 7) + 3;
                    if (m > n)
                    {
                        Array.Copy(o, op - m, o, op, n);
                        op += n;
                    }
                    else
                    {
                        for (int i = 0; i < n; i++)
                        {
                            Array.Copy(o, op - m, o, op, 1);
                            op++;
                        }
                    }
                }
            }
            byte[] temp = new byte[op];
            Array.Copy(o, temp, op);
            return temp;
        }
    }
}
