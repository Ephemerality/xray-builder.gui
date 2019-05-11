// This file contains code ported over from KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)

using System;
using System.Collections.Generic;
using System.Linq;

namespace XRayBuilderGUI.Unpack.Mobi
{
    public interface Decompressor
    {
        byte[] unpack(byte[] data);
    }

    public class UncompressedReader : Decompressor
    {
        public byte[] unpack(byte[] data)
        {
            return data;
        }
    }

    public class PalmDOCReader : Decompressor
    {
        public byte[] unpack(byte[] data)
        {
            int p = 0;
            byte[] o = new byte[4096];
            int op = 0;
            while (p < data.Length)
            {
                int c = data[p++];
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

    class HUFFCDICReader : Decompressor
    {
        private List<DictRecord> _dict1 = new List<DictRecord>(256);
        private List<uint> _dict2 = new List<uint>(64);
        private List<UInt64> _maxcode = new List<UInt64>();
        private List<uint> _mincode = new List<uint>();
        List<Slice> dictionary = new List<Slice>();

        public void loadHuff(byte[] data)
        {
            if (!data.Take(8).SequenceEqual(new byte[] { 72, 85, 70, 70, 0, 0, 0, 24 }))
                throw new Exception("Invalid HUFF header.");
            byte[] temp4 = new byte[4];
            Array.Copy(data, 8, temp4, 0, 4);
            uint off1 = BitConverter.ToUInt32(Functions.CheckBytes(temp4), 0);
            Array.Copy(data, 12, temp4, 0, 4);
            uint off2 = BitConverter.ToUInt32(Functions.CheckBytes(temp4), 0);

            for (int i = 0; i < 256; i++)
            {
                Array.Copy(data, off1 + (i * 4), temp4, 0, 4);
                _dict1.Add(dictUnpack(BitConverter.ToUInt32(Functions.CheckBytes(temp4), 0)));
            }
            for (int i = 0; i < 64; i++)
            {
                Array.Copy(data, off2 + (i * 4), temp4, 0, 4);
                _dict2.Add(BitConverter.ToUInt32(Functions.CheckBytes(temp4), 0));
            }
            int count = 1;
            _mincode.Add(0);
            _maxcode.Add(0);
            for (int i = 0; i < _dict2.Count; i += 2)
            {
                uint item = _dict2[i];
                _mincode.Add(item << (32 - count++));
            }
            count = 1;
            for (int i = 1; i < _dict2.Count; i += 2)
            {
                uint item = _dict2[i];
                _maxcode.Add(((item + 1) << (32 - count++)) - 1);
            }
        }

        private DictRecord dictUnpack(uint v)
        {
            uint codelen = v & 0x1f;
            uint term = v & 0x80;
            UInt64 maxcode = v >> 8;
            if (codelen == 0) throw new Exception("Invalid codelen value.");
            if (codelen <= 8 && term == 0) throw new Exception("Invalid term value.");
            maxcode = ((maxcode + 1) << (int)(32 - codelen)) - 1;
            return new DictRecord(codelen, term, maxcode);
        }

        public void loadCdic(byte[] data)
        {
            if (!data.Take(8).SequenceEqual(new byte[] { 67, 68, 73, 67, 0, 0, 0, 16 }))
                throw new Exception("Invalid CDIC header.");
            byte[] temp4 = new byte[4];
            Array.Copy(data, 8, temp4, 0, 4);
            int phrases = (int)BitConverter.ToUInt32(Functions.CheckBytes(temp4), 0);
            Array.Copy(data, 12, temp4, 0, 4);
            int bits = (int)BitConverter.ToUInt32(Functions.CheckBytes(temp4), 0);
            int n = Math.Min(1 << bits, phrases - dictionary.Count);
            for (int i = 0; i < n; i++)
            {
                byte[] temp2 = new byte[2];
                Array.Copy(data, 16 + (i * 2), temp2, 0, 2);
                ushort offset = BitConverter.ToUInt16(Functions.CheckBytes(temp2), 0);
                dictionary.Add(getSlice(data, offset));
            }
        }

        private Slice getSlice(byte[] data, ushort offset)
        {
            byte[] temp2 = new byte[2];
            Array.Copy(data, 16 + offset, temp2, 0, 2);
            ushort blen = BitConverter.ToUInt16(Functions.CheckBytes(temp2), 0);
            byte[] slice = new byte[blen & 0x7fff];
            Array.Copy(data, 18 + offset, slice, 0, slice.Length);
            return new Slice(slice, blen & 0x8000);
        }

        public byte[] unpack(byte[] data)
        {
            byte[] o = new byte[4096];
            byte[] temp8 = new byte[8];
            int op = 0;
            int bitsleft = data.Length * 8;
            data = data.Concat(new byte[8]).ToArray();
            int pos = 0;
            Array.Copy(data, pos, temp8, 0, 8);
            UInt64 x = BitConverter.ToUInt64(Functions.CheckBytes(temp8), 0);
            int n = 32;
            while (true)
            {
                if (n <= 0)
                {
                    pos += 4;
                    Array.Copy(data, pos, temp8, 0, 8);
                    x = BitConverter.ToUInt64(Functions.CheckBytes(temp8), 0);
                    n += 32;
                }
                UInt64 code = (x >> n) & ((1L << 32) - 1);
                DictRecord rec = _dict1[(int)(code >> 24)];
                int codelen = (int)rec.codelen;
                UInt64 maxcode = rec.maxcode;
                if (rec.term == 0)
                {
                    while (code < _mincode[codelen])
                        codelen++;
                    maxcode = _maxcode[codelen];
                }
                n -= codelen;
                bitsleft -= codelen;
                if (bitsleft < 0)
                    break;
                int r = (int)((maxcode - code) >> (32 - codelen));
                Slice slice = dictionary[r];
                if (slice.flag == 0)
                {
                    byte[] newSlice = unpack(slice.slice);
                    slice = new Slice(newSlice, 1);
                    dictionary[r] = slice;
                }
                Array.Copy(slice.slice, 0, o, op, slice.slice.Length);
                op += slice.slice.Length;
            }
            byte[] temp = new byte[op];
            Array.Copy(o, temp, op);
            return temp;
        }

        private class Slice
        {
            public byte[] slice;
            public int flag;

            public Slice(byte[] s, int f)
            {
                slice = s;
                flag = f;
            }
        }

        private class DictRecord
        {
            public uint codelen;
            public uint term;
            public UInt64 maxcode;

            public DictRecord(uint c, uint t, UInt64 m)
            {
                codelen = c;
                term = t;
                maxcode = m;
            }
        }
    }
}
