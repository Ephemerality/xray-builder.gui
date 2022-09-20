/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.IO;
using Ephemerality.Unpack.Extensions;

namespace Ephemerality.Unpack.Mobi
{
    public sealed class PalmDocHeader
    {
        private readonly byte[] _compression;
        private readonly byte[] _unused;
        private readonly byte[] _textLength;
        private readonly byte[] _recordCount;
        private readonly byte[] _recordSize;
        private readonly byte[] _encryptionType;
        private readonly byte[] _unknown;

        public PalmDocHeader(Stream fs)
        {
            _compression = fs.ReadBytes(2);
            _unused = fs.ReadBytes(2);
            _textLength = fs.ReadBytes(4);
            _recordCount = fs.ReadBytes(2);
            _recordSize = fs.ReadBytes(2);
            _encryptionType = fs.ReadBytes(2);
            _unknown = fs.ReadBytes(2);
        }

        public byte[] HeaderBytes()
        {
            using var header = new MemoryStream();
            header.Write(_compression, 0, _compression.Length);
            header.Write(_unused, 0, _unused.Length);
            header.Write(_textLength, 0, _textLength.Length);
            header.Write(_recordCount, 0, _recordCount.Length);
            header.Write(_recordSize, 0, _recordSize.Length);
            header.Write(_encryptionType, 0, _encryptionType.Length);
            header.Write(_unknown, 0, _unknown.Length);

            return header.ToArray();
        }

        public ushort Compression => BitConverter.ToUInt16(_compression.BigEndian(), 0);

        public uint TextLength => BitConverter.ToUInt32(_textLength.BigEndian(), 0);

        public ushort RecordCount => BitConverter.ToUInt16(_recordCount.BigEndian(), 0);

        public ushort RecordSize => BitConverter.ToUInt16(_recordSize.BigEndian(), 0);

        public ushort EncryptionType => BitConverter.ToUInt16(_encryptionType.BigEndian(), 0);
    }
}
