/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace XRayBuilderGUI.Unpack
{
    public class EXTHHeader
    {
        private byte[] _identifier = new byte[4];
        private byte[] _headerLength = new byte[4];
        private byte[] _recordCount = new byte[4];

        private List<EXTHRecord> recordList = new List<EXTHRecord>();

        public EXTHHeader(FileStream fs)
        {
            fs.Read(_identifier, 0, _identifier.Length);
            if (IdentifierAsString != "EXTH")
                throw new UnpackException("Expected to find EXTH header identifier EXTH but got something else instead");
            fs.Read(_headerLength, 0, _headerLength.Length);
            fs.Read(_recordCount, 0, _recordCount.Length);
            for (int i = 0; i < RecordCount; i++)
            {
                recordList.Add(new EXTHRecord(fs));
            }
            fs.Seek(GetPaddingSize(DataSize), SeekOrigin.Current); // Skip padding bytes
        }

        protected int DataSize
        {
            get
            {
                int size = 0;
                foreach (EXTHRecord rec in recordList)
                {
                    size += rec.Size;
                }

                return size;
            }
        }

        public int Size
        {
            get
            {
                int dataSize = DataSize;
                return 12 + dataSize + GetPaddingSize(dataSize);
            }
        }

        protected int GetPaddingSize(int dataSize)
        {
            int paddingSize = dataSize % 4;
            if (paddingSize != 0) paddingSize = 4 - paddingSize;

            return paddingSize;
        }

        public string IdentifierAsString => Encoding.UTF8.GetString(_identifier).Trim('\0');

        public uint HeaderLength => BitConverter.ToUInt32(Functions.CheckBytes(_headerLength), 0);

        public uint RecordCount => BitConverter.ToUInt32(Functions.CheckBytes(_recordCount), 0);

        public string Author => GetRecordByType(100);

        public string Description => GetRecordByType(103);

        public string ASIN => GetRecordByType(113);

        public string CDEType => GetRecordByType(501);

        public string UpdatedTitle => GetRecordByType(503);

        public string ASIN2 => GetRecordByType(504);

        public int CoverOffset => BitConverter.ToInt32(Functions.CheckBytes(GetRecordBytesByType(201)) ?? new byte[] { 255, 255, 255, 255 }, 0);

        private byte[] GetRecordBytesByType(int recType)
        {
            byte[] record = null;
            foreach (EXTHRecord rec in recordList)
            {
                if (rec.RecordType == recType)
                {
                    record = new byte[rec.RecordData.Length];
                    Buffer.BlockCopy(rec.RecordData, 0, record, 0, rec.RecordData.Length);
                    break;
                }
            }
            return record;
        }

        private string GetRecordByType(int recType)
        {
            string record = string.Empty;
            foreach (EXTHRecord rec in recordList)
            {
                if (rec.RecordType == recType)
                {
                    record = System.Text.Encoding.UTF8.GetString(rec.RecordData);
                    break;
                }
            }
            return record;
        }

        public void UpdateCdeContentType(FileStream fs)
        {
            byte[] newValue = Encoding.UTF8.GetBytes("EBOK");
            var rec = recordList.First(r => r.RecordType == 501);
            if (rec == null)
                throw new UnpackException("Could not find the CDEContentType record (EXTH 501).");
            fs.Seek(rec.recordOffset, SeekOrigin.Begin);
            fs.Write(newValue, 0, newValue.Length);
        }
    }

    class EXTHRecord
    {
        private readonly byte[] _recordType = new byte[4];
        private readonly byte[] _recordLength = new byte[4];
        public long recordOffset;

        public EXTHRecord(Stream fs)
        {
            fs.Read(_recordType, 0, _recordType.Length);
            fs.Read(_recordLength, 0, _recordLength.Length);

            if (RecordLength < 8) throw new UnpackException("Invalid EXTH record length");
            recordOffset = fs.Position;
            RecordData = new byte[RecordLength - 8];
            fs.Read(RecordData, 0, RecordData.Length);
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(RecordData);
        }

        public int DataLength => RecordData.Length;

        public int Size => DataLength + 8;

        public uint RecordLength => BitConverter.ToUInt32(Functions.CheckBytes(_recordLength), 0);

        public uint RecordType => BitConverter.ToUInt32(Functions.CheckBytes(_recordType), 0);

        public byte[] RecordData { get; }
    }
}
