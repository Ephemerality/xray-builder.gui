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
        private readonly byte[] _compression = new byte[2];
        private readonly byte[] _unused = new byte[2];
        private readonly byte[] _textLength = new byte[4];
        private readonly byte[] _recordCount = new byte[2];
        private readonly byte[] _recordSize = new byte[2];
        private readonly byte[] _encryptionType = new byte[2];
        private readonly byte[] _unknown = new byte[2];

        public PalmDocHeader(Stream fs)
        {
            fs.Read(_compression, 0, _compression.Length);
            fs.Read(_unused, 0, _unused.Length);
            fs.Read(_textLength, 0, _textLength.Length);
            fs.Read(_recordCount, 0, _recordCount.Length);
            fs.Read(_recordSize, 0, _recordSize.Length);
            fs.Read(_encryptionType, 0, _encryptionType.Length);
            fs.Read(_unknown, 0, _unknown.Length);
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
