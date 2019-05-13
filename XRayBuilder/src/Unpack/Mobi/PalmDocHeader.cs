/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.IO;

namespace XRayBuilderGUI.Unpack.Mobi
{
    public class PalmDOCHeader
    {
        private byte[] _compression = new byte[2];
        private byte[] _textLength = new byte[4];
        private byte[] _recordCount = new byte[2];
        private byte[] _recordSize = new byte[2];
        private byte[] _encryptionType = new byte[2];

        public PalmDOCHeader(FileStream fs)
        {
            fs.Read(_compression, 0, _compression.Length);
            fs.Seek(2, SeekOrigin.Current);
            fs.Read(_textLength, 0, _textLength.Length);
            fs.Read(_recordCount, 0, _recordCount.Length);
            fs.Read(_recordSize, 0, _recordSize.Length);
            fs.Read(_encryptionType, 0, _encryptionType.Length);
            fs.Seek(2, SeekOrigin.Current);
        }

        public ushort Compression => BitConverter.ToUInt16(Functions.CheckBytes(_compression), 0);

        public uint TextLength => BitConverter.ToUInt32(Functions.CheckBytes(_textLength), 0);

        public ushort RecordCount => BitConverter.ToUInt16(Functions.CheckBytes(_recordCount), 0);

        public ushort RecordSize => BitConverter.ToUInt16(Functions.CheckBytes(_recordSize), 0);

        public ushort EncryptionType => BitConverter.ToUInt16(Functions.CheckBytes(_encryptionType), 0);
    }
}
