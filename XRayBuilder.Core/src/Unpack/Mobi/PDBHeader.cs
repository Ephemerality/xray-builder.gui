/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.IO;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Unpack.Mobi
{
    public sealed class PdbHeader
    {
        private readonly byte[] _dbName = new byte[32];
        private readonly byte[] _attributeBits = new byte[2];
        private readonly byte[] _version = new byte[2];
        private readonly byte[] _creationDate = new byte[4];
        private readonly byte[] _modificationDate = new byte[4];
        private readonly byte[] _lastBackupDate = new byte[4];
        private readonly byte[] _modificationNumber = new byte[4];
        private readonly byte[] _appInfoId = new byte[4];
        private readonly byte[] _sortInfoId = new byte[4];
        private readonly byte[] _type = new byte[4];
        private readonly byte[] _creator = new byte[4];
        private readonly byte[] _uniqueSeed = new byte[4];
        private readonly byte[] _nextRecordListId = new byte[4];
        private readonly byte[] _numRecords = new byte[2];
        private readonly byte[] _unknown = new byte[2];

        public readonly RecordInfo[] RecordMetadata;

        public PdbHeader(FileStream fs)
        {
            fs.Read(_dbName, 0, 32);
            fs.Read(_attributeBits, 0, 2);
            fs.Read(_version, 0, 2);
            fs.Read(_creationDate, 0, 4);
            fs.Read(_modificationDate, 0, 4);
            fs.Read(_lastBackupDate, 0, 4);
            fs.Read(_modificationNumber, 0, 4);
            fs.Read(_appInfoId, 0, 4);
            fs.Read(_sortInfoId, 0, 4);
            fs.Read(_type, 0, 4);
            fs.Read(_creator, 0, 4);
            fs.Read(_uniqueSeed, 0, 4);
            fs.Read(_nextRecordListId, 0, 4);
            fs.Read(_numRecords, 0, 2);

            var numRecords = NumRecords;
            RecordMetadata = new RecordInfo[numRecords];
            for (var i = 0; i < numRecords; i++)
                RecordMetadata[i] = new RecordInfo(new EndianBinaryReader(EndianBitConverter.Big, fs));
            fs.Read(_unknown, 0, 2);
        }

        public string DbName => Encoding.UTF8.GetString(_dbName).Trim('\0');

        public int NumRecords => BitConverter.ToInt16(_numRecords.BigEndian(), 0);

        public uint MobiHeaderSize
        {
            get
            {
                if (RecordMetadata.Length > 1)
                    return RecordMetadata[1].DataOffset - RecordMetadata[0].DataOffset;
                return 0;
            }
        }

        public byte[] HeaderBytes()
        {
            var header = new MemoryStream();
            header.Write(_dbName, 0, _dbName.Length);
            header.Write(_attributeBits, 0, _attributeBits.Length);
            header.Write(_version, 0, _version.Length);
            header.Write(_creationDate, 0, _creationDate.Length);
            header.Write(_modificationDate, 0, _modificationDate.Length);
            header.Write(_lastBackupDate, 0, _lastBackupDate.Length);
            header.Write(_modificationNumber, 0, _modificationNumber.Length);
            header.Write(_appInfoId, 0, _appInfoId.Length);
            header.Write(_sortInfoId, 0, _sortInfoId.Length);
            header.Write(_type, 0, _type.Length);
            header.Write(_creator, 0, _creator.Length);
            header.Write(_uniqueSeed, 0, _uniqueSeed.Length);
            header.Write(_nextRecordListId, 0, _nextRecordListId.Length);
            header.Write(_numRecords, 0, _numRecords.Length);
            var numRecords = NumRecords;
            for (var i = 0; i < numRecords; i++)
            {
                using var byteStream = new MemoryStream();
                var writer = new EndianBinaryWriter(EndianBitConverter.Big, byteStream);
                RecordMetadata[i].Write(writer);
                header.Write(byteStream.ToArray(), 0, (int) byteStream.Length);
            }
            header.Write(_unknown, 0, _unknown.Length);

            return header.ToArray();
        }
    }

    public sealed class RecordInfo
    {
        public uint DataOffset { get; set; }
        public byte Attributes { get; set; }
        public byte[] UniqueId { get; set; }

        public RecordInfo(EndianBinaryReader reader)
        {
            DataOffset = reader.ReadUInt32();
            Attributes = reader.ReadByte();
            UniqueId = reader.ReadBytes(3);
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(DataOffset);
            writer.Write(Attributes);
            writer.Write(UniqueId);
        }
    }
}
