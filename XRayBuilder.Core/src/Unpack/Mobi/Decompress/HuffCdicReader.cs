using System;
using System.Collections.Generic;
using System.Linq;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Unpack.Mobi.Decompress
{
    internal sealed class HuffCdicReader : Decompressor
    {
        private readonly List<DictRecord> _dict1 = new List<DictRecord>(256);
        private readonly List<uint> _dict2 = new List<uint>(64);
        private readonly List<ulong> _maxcode = new List<ulong>();
        private readonly List<uint> _mincode = new List<uint>();
        private readonly List<Slice> _dictionary = new List<Slice>();

        public override void Initialize(MobiHead mobiHeader, PDBHeader pdbHeader, List<byte[]> headerRecords)
        {
            try
            {
                var huffmanRecordOffset = (int) mobiHeader.HuffmanRecordOffset;
                var huffSect = headerRecords[huffmanRecordOffset];
                loadHuff(huffSect);
                var recCount = (int)mobiHeader.HuffmanRecordCount;
                for (var i = 1; i < recCount; i++)
                {
                    huffSect = headerRecords[huffmanRecordOffset + i];
                    LoadCdic(huffSect);
                }
            } catch (Exception ex)
            {
                throw new UnpackException("Error in HUFF/CDIC decompression: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void loadHuff(byte[] data)
        {
            if (!data.Take(8).SequenceEqual(new byte[] { 72, 85, 70, 70, 0, 0, 0, 24 }))
                throw new Exception("Invalid HUFF header.");
            var temp4 = new byte[4];
            Array.Copy(data, 8, temp4, 0, 4);
            var off1 = BitConverter.ToUInt32(temp4.BigEndian(), 0);
            Array.Copy(data, 12, temp4, 0, 4);
            var off2 = BitConverter.ToUInt32(temp4.BigEndian(), 0);

            for (var i = 0; i < 256; i++)
            {
                Array.Copy(data, off1 + (i * 4), temp4, 0, 4);
                _dict1.Add(dictUnpack(BitConverter.ToUInt32(temp4.BigEndian(), 0)));
            }
            for (var i = 0; i < 64; i++)
            {
                Array.Copy(data, off2 + (i * 4), temp4, 0, 4);
                _dict2.Add(BitConverter.ToUInt32(temp4.BigEndian(), 0));
            }
            var count = 1;
            _mincode.Add(0);
            _maxcode.Add(0);
            for (var i = 0; i < _dict2.Count; i += 2)
            {
                var item = _dict2[i];
                _mincode.Add(item << (32 - count++));
            }
            count = 1;
            for (var i = 1; i < _dict2.Count; i += 2)
            {
                var item = _dict2[i];
                _maxcode.Add(((item + 1) << (32 - count++)) - 1);
            }
        }

        private static DictRecord dictUnpack(uint v)
        {
            var codelen = v & 0x1f;
            var term = v & 0x80;
            ulong maxcode = v >> 8;
            if (codelen == 0) throw new Exception("Invalid codelen value.");
            if (codelen <= 8 && term == 0) throw new Exception("Invalid term value.");
            maxcode = ((maxcode + 1) << (int)(32 - codelen)) - 1;
            return new DictRecord
            {
                CodeLen = codelen,
                Term = term,
                MaxCode = maxcode
            };
        }

        private void LoadCdic(byte[] data)
        {
            if (!data.Take(8).SequenceEqual(new byte[] { 67, 68, 73, 67, 0, 0, 0, 16 }))
                throw new Exception("Invalid CDIC header.");
            var temp4 = new byte[4];
            Array.Copy(data, 8, temp4, 0, 4);
            var phrases = (int)BitConverter.ToUInt32(temp4.BigEndian(), 0);
            Array.Copy(data, 12, temp4, 0, 4);
            var bits = (int)BitConverter.ToUInt32(temp4.BigEndian(), 0);
            var n = Math.Min(1 << bits, phrases - _dictionary.Count);
            for (var i = 0; i < n; i++)
            {
                var temp2 = new byte[2];
                Array.Copy(data, 16 + (i * 2), temp2, 0, 2);
                var offset = BitConverter.ToUInt16(temp2.BigEndian(), 0);
                _dictionary.Add(GetSlice(data, offset));
            }
        }

        private Slice GetSlice(byte[] data, ushort offset)
        {
            var temp2 = new byte[2];
            Array.Copy(data, 16 + offset, temp2, 0, 2);
            var blen = BitConverter.ToUInt16(temp2.BigEndian(), 0);
            var slice = new byte[blen & 0x7fff];
            Array.Copy(data, 18 + offset, slice, 0, slice.Length);
            return new Slice
            {
                Data = slice,
                Flag = blen & 0x8000
            };
        }

        public override byte[] Unpack(byte[] data)
        {
            var o = new byte[4096];
            var temp8 = new byte[8];
            var op = 0;
            var bitsleft = data.Length * 8;
            data = data.Concat(new byte[8]).ToArray();
            var pos = 0;
            Array.Copy(data, pos, temp8, 0, 8);
            var x = BitConverter.ToUInt64(temp8.BigEndian(), 0);
            var n = 32;
            while (true)
            {
                if (n <= 0)
                {
                    pos += 4;
                    Array.Copy(data, pos, temp8, 0, 8);
                    x = BitConverter.ToUInt64(temp8.BigEndian(), 0);
                    n += 32;
                }
                var code = (x >> n) & ((1L << 32) - 1);
                var rec = _dict1[(int)(code >> 24)];
                var codelen = (int)rec.CodeLen;
                var maxcode = rec.MaxCode;
                if (rec.Term == 0)
                {
                    while (code < _mincode[codelen])
                        codelen++;
                    maxcode = _maxcode[codelen];
                }
                n -= codelen;
                bitsleft -= codelen;
                if (bitsleft < 0)
                    break;
                var r = (int)((maxcode - code) >> (32 - codelen));
                var slice = _dictionary[r];
                if (slice.Flag == 0)
                {
                    var newSlice = Unpack(slice.Data);
                    slice = new Slice
                    {
                        Data = newSlice,
                        Flag = 1
                    };
                    _dictionary[r] = slice;
                }
                Array.Copy(slice.Data, 0, o, op, slice.Data.Length);
                op += slice.Data.Length;
            }
            var temp = new byte[op];
            Array.Copy(o, temp, op);
            return temp;
        }

        private sealed class Slice
        {
            public byte[] Data { get; set; }
            public int Flag { get; set; }
        }

        private sealed class DictRecord
        {
            public uint CodeLen { get; set; }
            public uint Term { get; set; }
            public ulong MaxCode { get; set; }
        }
    }
}