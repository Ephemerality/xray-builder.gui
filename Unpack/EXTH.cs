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
    class EXTHHeader
    {
        private byte[] _identifier = new byte[4];
        private byte[] _headerLength = new byte[4];
        private byte[] _recordCount = new byte[4];

        private List<EXTHRecord> recordList = new List<EXTHRecord>();
        
        public EXTHHeader(FileStream fs)
        {
            fs.Read(_identifier, 0, _identifier.Length);
            if (IdentifierAsString != "EXTH")
                throw new IOException("Expected to find EXTH header identifier EXTH but got something else instead");
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
                foreach (EXTHRecord rec in this.recordList)
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
                int dataSize = this.DataSize;
                return 12 + dataSize + GetPaddingSize(dataSize);
            }
        }

        protected int GetPaddingSize(int dataSize)
        {
            int paddingSize = dataSize % 4;
            if (paddingSize != 0) paddingSize = 4 - paddingSize;

            return paddingSize;
        }

        public string IdentifierAsString
        {
            get { return Encoding.UTF8.GetString(_identifier).Trim('\0'); }
        }

        public uint HeaderLength
        {
            get { return BitConverter.ToUInt32(Functions.CheckBytes(_headerLength), 0); }
        }

        public uint RecordCount
        {
            get { return BitConverter.ToUInt32(Functions.CheckBytes(_recordCount), 0); }
        }

        public string Author
        {
            get { return GetRecordByType(100); }
        }

        public string Description
        {
            get { return GetRecordByType(103); }
        }

        public string ASIN
        {
            get { return GetRecordByType(113); }
        }

        public string CDEType
        {
            get { return GetRecordByType(501); }
        }

        public string UpdatedTitle
        {
            get { return GetRecordByType(503); }
        }

        public string ASIN2
        {
            get { return GetRecordByType(504); }
        }
        
        private string GetRecordByType(int recType)
        {
            string record = String.Empty;
            foreach (EXTHRecord rec in recordList)
            {
                if (rec.RecordType == recType)
                {
                    record = System.Text.Encoding.UTF8.GetString(rec.RecordData);
                }
            }
            return record;
        }
    }

    class EXTHRecord
    {
        private byte[] _recordType = new byte[4];
        private byte[] _recordLength = new byte[4];
        private byte[] _recordData = null;

        public EXTHRecord(FileStream fs)
        {
            fs.Read(_recordType, 0, _recordType.Length);
            fs.Read(_recordLength, 0, _recordLength.Length);

            if (RecordLength < 8) throw new IOException("Invalid EXTH record length");
            _recordData = new byte[RecordLength - 8];
            fs.Read(_recordData, 0, _recordData.Length);
        }
        
        public int DataLength
        {
            get { return _recordData.Length; }
        }

        public int Size
        {
            get { return DataLength + 8; }
        }

        public uint RecordLength
        {
            get { return BitConverter.ToUInt32(Functions.CheckBytes(_recordLength), 0); }
        }

        public uint RecordType
        {
            get { return BitConverter.ToUInt32(Functions.CheckBytes(_recordType), 0); }
        }

        public byte[] RecordData
        {
            get { return _recordData; }
        }
    }
}
