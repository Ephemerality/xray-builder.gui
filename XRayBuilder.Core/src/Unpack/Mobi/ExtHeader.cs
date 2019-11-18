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
using MiscUtil.IO;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Unpack.Mobi
{
    public sealed class ExtHeader
    {
        public string Identifier { get; set; }
        public int HeaderLength { get; set; }
        public int RecordCount { get; set; }

        private readonly List<ExtHRecord> _recordList = new List<ExtHRecord>();

        public ExtHeader(EndianBinaryReader reader)
        {
            Identifier = Encoding.UTF8.GetString(reader.ReadBytes(4)).Trim('\0');
            if (Identifier != "EXTH")
                throw new UnpackException($"Invalid EXTH identifier: {Identifier}");
            HeaderLength = reader.ReadInt32();
            RecordCount = reader.ReadInt32();
            for (var i = 0; i < RecordCount; i++)
                _recordList.Add(new ExtHRecord(reader));
            reader.Seek(GetPaddingSize(DataSize), SeekOrigin.Current); // Skip padding bytes
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
            if (paddingSize != 0)
                paddingSize = 4 - paddingSize;

            return paddingSize;
        }

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
           // fs.Seek(rec.RecordOffset, SeekOrigin.Begin);
            fs.Write(newValue, 0, newValue.Length);
        }
    }
}
