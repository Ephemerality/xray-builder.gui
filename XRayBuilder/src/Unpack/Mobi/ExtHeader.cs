/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using XRayBuilderGUI.Libraries.Primitives.Extensions;

namespace XRayBuilderGUI.Unpack.Mobi
{
    public sealed class ExtHeader
    {
        private readonly byte[] _identifier = new byte[4];
        private readonly byte[] _headerLength = new byte[4];
        private readonly byte[] _recordCount = new byte[4];

        private readonly List<ExtHRecord> _recordList = new List<ExtHRecord>();

        public ExtHeader(FileStream fs)
        {
            fs.Read(_identifier, 0, _identifier.Length);
            if (IdentifierAsString != "EXTH")
                throw new UnpackException("Expected to find EXTH header identifier EXTH but got something else instead");
            fs.Read(_headerLength, 0, _headerLength.Length);
            fs.Read(_recordCount, 0, _recordCount.Length);
            for (var i = 0; i < RecordCount; i++)
            {
                _recordList.Add(new ExtHRecord(fs));
            }
            fs.Seek(GetPaddingSize(DataSize), SeekOrigin.Current); // Skip padding bytes
        }

        private int DataSize => _recordList.Sum(rec => rec.Size);

        public int Size
        {
            get
            {
                var dataSize = DataSize;
                return 12 + dataSize + GetPaddingSize(dataSize);
            }
        }

        private int GetPaddingSize(int dataSize)
        {
            var paddingSize = dataSize % 4;
            if (paddingSize != 0) paddingSize = 4 - paddingSize;

            return paddingSize;
        }

        public string IdentifierAsString => Encoding.UTF8.GetString(_identifier).Trim('\0');

        public uint HeaderLength => BitConverter.ToUInt32(_headerLength.BigEndian(), 0);

        public uint RecordCount => BitConverter.ToUInt32(_recordCount.BigEndian(), 0);

        public string Author => GetRecordByType(100);

        public string Description => GetRecordByType(103);

        public string Asin => GetRecordByType(113);

        public string CdeType => GetRecordByType(501);

        public string UpdatedTitle => GetRecordByType(503);

        public string Asin2 => GetRecordByType(504);

        public int CoverOffset => BitConverter.ToInt32(GetRecordBytesByType(201)?.BigEndian() ?? new byte[] { 255, 255, 255, 255 }, 0);

        [CanBeNull]
        private byte[] GetRecordBytesByType(int recType)
        {
            byte[] record = null;
            foreach (var rec in _recordList.Where(rec => rec.RecordType == recType))
            {
                record = new byte[rec.RecordData.Length];
                Buffer.BlockCopy(rec.RecordData, 0, record, 0, rec.RecordData.Length);
                break;
            }

            return record;
        }

        private string GetRecordByType(int recType)
        {
            var record = string.Empty;
            foreach (var rec in _recordList.Where(rec => rec.RecordType == recType))
            {
                record = Encoding.UTF8.GetString(rec.RecordData);
                break;
            }

            return record;
        }

        public void UpdateCdeContentType(FileStream fs)
        {
            var newValue = Encoding.UTF8.GetBytes("EBOK");
            var rec = _recordList.First(r => r.RecordType == 501);
            if (rec == null)
                throw new UnpackException("Could not find the CDEContentType record (EXTH 501).");
            fs.Seek(rec.RecordOffset, SeekOrigin.Begin);
            fs.Write(newValue, 0, newValue.Length);
        }
    }

    internal sealed class ExtHRecord
    {
        private readonly byte[] _recordType = new byte[4];
        private readonly byte[] _recordLength = new byte[4];
        public readonly long RecordOffset;

        public ExtHRecord(Stream fs)
        {
            fs.Read(_recordType, 0, _recordType.Length);
            fs.Read(_recordLength, 0, _recordLength.Length);

            if (RecordLength < 8) throw new UnpackException("Invalid EXTH record length");
            RecordOffset = fs.Position;
            RecordData = new byte[RecordLength - 8];
            fs.Read(RecordData, 0, RecordData.Length);
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(RecordData);
        }

        public int DataLength => RecordData.Length;

        public int Size => DataLength + 8;

        public uint RecordLength => BitConverter.ToUInt32(_recordLength.BigEndian(), 0);

        public uint RecordType => BitConverter.ToUInt32(_recordType.BigEndian(), 0);

        public byte[] RecordData { get; }
    }
}
