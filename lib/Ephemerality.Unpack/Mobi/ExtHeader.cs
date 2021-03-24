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
using Ephemerality.Unpack.Exceptions;
using Ephemerality.Unpack.Extensions;
using JetBrains.Annotations;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace Ephemerality.Unpack.Mobi
{
    public sealed class ExtHeader
    {
        public readonly List<ExtHRecord> RecordList = new List<ExtHRecord>();

        public ExtHeader(EndianBinaryReader reader)
        {
            var identifier = Encoding.UTF8.GetString(reader.ReadBytes(4)).Trim('\0');
            if (identifier != "EXTH")
                throw new UnpackException($"Invalid EXTH identifier: {identifier}");
            // Skip reading header length
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            var recordCount = reader.ReadInt32();
            for (var i = 0; i < recordCount; i++)
                RecordList.Add(new ExtHRecord(reader));
            reader.Seek(GetPaddingSize(DataSize), SeekOrigin.Current); // Skip padding bytes
        }

        public byte[] HeaderBytes()
        {
            using var writer = new EndianBinaryWriter(EndianBitConverter.Big, new MemoryStream());
            writer.Write("EXTH".ToCharArray());
            writer.Write(HeaderLength);
            writer.Write(RecordList.Count);
            foreach (var record in RecordList)
                record.Write(writer);

            var paddingBytes = new byte[GetPaddingSize(DataSize)];
            writer.Write(paddingBytes);

            return ((MemoryStream)writer.BaseStream).ToArray();
        }

        private int DataSize => RecordList.Sum(rec => rec.Size);

        /// <summary>
        /// Retrieve the header's full size - records plus the identifier (4), this length (4), and record count (4)
        /// </summary>
        public int HeaderLength => DataSize + 12;

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

        public string Author => GetTextRecordByType(100);

        public string Description => GetTextRecordByType(103);

        public string Asin => GetTextRecordByType(113);

        public string CdeType => GetTextRecordByType(501);

        public string UpdatedTitle => GetTextRecordByType(503);

        public string Asin2 => GetTextRecordByType(504);

        public string Isbn => GetTextRecordByType(104);

        public int CoverOffset => BitConverter.ToInt32(GetRecordBytesByType(201)?.BigEndian() ?? new byte[] { 255, 255, 255, 255 }, 0);

        [CanBeNull]
        private byte[] GetRecordBytesByType(int recType)
        {
            byte[] record = null;
            foreach (var rec in RecordList.Where(rec => rec.RecordType == recType))
            {
                record = new byte[rec.RecordData.Length];
                Buffer.BlockCopy(rec.RecordData, 0, record, 0, rec.RecordData.Length);
                break;
            }

            return record;
        }

        private string GetTextRecordByType(int recType)
        {
            var record = string.Empty;
            foreach (var rec in RecordList.Where(rec => rec.RecordType == recType))
            {
                record = Encoding.UTF8.GetString(rec.RecordData);
                break;
            }

            return record;
        }
    }
}
