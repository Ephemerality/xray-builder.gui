// Credit for the majority of code in this file goes to Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)

using System;
using System.IO;
using System.Text;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Unpack.Mobi
{
    public sealed class MobiHead
    {
        private readonly byte[] _identifier = new byte[4];
        private readonly byte[] _headerLength = new byte[4];
        private readonly byte[] _mobiType = new byte[4];
        private readonly byte[] _textEncoding = new byte[4];
        private readonly byte[] _uniqueId = new byte[4];
        private readonly byte[] _fileVersion = new byte[4];
        private readonly byte[] _orthographicIndex = new byte[4];
        private readonly byte[] _inflectionIndex = new byte[4];
        private readonly byte[] _indexNames = new byte[4];
        private readonly byte[] _indexKeys = new byte[4];
        private readonly byte[] _extraIndex0 = new byte[4];
        private readonly byte[] _extraIndex1 = new byte[4];
        private readonly byte[] _extraIndex2 = new byte[4];
        private readonly byte[] _extraIndex3 = new byte[4];
        private readonly byte[] _extraIndex4 = new byte[4];
        private readonly byte[] _extraIndex5 = new byte[4];
        private readonly byte[] _firstNonBookIndex = new byte[4];
        private readonly byte[] _fullNameOffset = new byte[4];
        private readonly byte[] _fullNameLength = new byte[4];
        private readonly byte[] _locale = new byte[4];
        private readonly byte[] _inputLanguage = new byte[4];
        private readonly byte[] _outputLanguage = new byte[4];
        private readonly byte[] _minVersion = new byte[4];
        private readonly byte[] _firstImageIndex = new byte[4];
        private readonly byte[] _huffmanRecordOffset = new byte[4];
        private readonly byte[] _huffmanRecordCount = new byte[4];
        private readonly byte[] _huffmanTableOffset = new byte[4];
        private readonly byte[] _huffmanTableLength = new byte[4];
        private readonly byte[] _exthFlags = new byte[4];
        private readonly byte[] _restOfMobiHeader;
        public readonly ExtHeader ExtHeader;

        private readonly byte[] _remainder;
        private readonly byte[] _fullName;

        public bool Multibyte;
        public int Trailers;

        public MobiHead(FileStream fs, uint mobiHeaderSize)
        {
            fs.Read(_identifier, 0, _identifier.Length);
            if (IdentifierAsString != "MOBI")
                throw new UnpackException("Did not get expected MOBI identifier");

            fs.Read(_headerLength, 0, _headerLength.Length);
            _restOfMobiHeader = new byte[HeaderLength + 16 - 132];

            fs.Read(_mobiType, 0, _mobiType.Length);
            fs.Read(_textEncoding, 0, _textEncoding.Length);
            fs.Read(_uniqueId, 0, _uniqueId.Length);
            Array.Reverse(_uniqueId);
            fs.Read(_fileVersion, 0, _fileVersion.Length);
            fs.Read(_orthographicIndex, 0, _orthographicIndex.Length);
            fs.Read(_inflectionIndex, 0, _inflectionIndex.Length);
            fs.Read(_indexNames, 0, _indexNames.Length);
            fs.Read(_indexKeys, 0, _indexKeys.Length);
            fs.Read(_extraIndex0, 0, _extraIndex0.Length);
            fs.Read(_extraIndex1, 0, _extraIndex1.Length);
            fs.Read(_extraIndex2, 0, _extraIndex2.Length);
            fs.Read(_extraIndex3, 0, _extraIndex3.Length);
            fs.Read(_extraIndex4, 0, _extraIndex4.Length);
            fs.Read(_extraIndex5, 0, _extraIndex5.Length);
            fs.Read(_firstNonBookIndex, 0, _firstNonBookIndex.Length);
            fs.Read(_fullNameOffset, 0, _fullNameOffset.Length);
            fs.Read(_fullNameLength, 0, _fullNameLength.Length);
            fs.Read(_locale, 0, _locale.Length);
            fs.Read(_inputLanguage, 0, _inputLanguage.Length);
            fs.Read(_outputLanguage, 0, _outputLanguage.Length);
            fs.Read(_minVersion, 0, _minVersion.Length);
            fs.Read(_firstImageIndex, 0, _firstImageIndex.Length);
            fs.Read(_huffmanRecordOffset, 0, _huffmanRecordOffset.Length);
            fs.Read(_huffmanRecordCount, 0, _huffmanRecordCount.Length);
            fs.Read(_huffmanTableOffset, 0, _huffmanTableOffset.Length);
            fs.Read(_huffmanTableLength, 0, _huffmanTableLength.Length);
            fs.Read(_exthFlags, 0, _exthFlags.Length);

            //If bit 6 (0x40) is set, then there's an EXTH record
            var exthExists = (BitConverter.ToUInt32(_exthFlags.BigEndian(), 0) & 0x40) != 0;

            fs.Read(_restOfMobiHeader, 0, _restOfMobiHeader.Length);

            if (exthExists)
                ExtHeader = new ExtHeader(fs);
            else
                throw new UnpackException("No EXT Header found. Ensure this book was processed with Calibre then try again.");

            // If applicable, read mbh flags regarding trailing bytes in record data
            if (MinVersion >= 5 && HeaderLength >= 228)
            {
                var tempFlags = new byte[2];
                Array.Copy(_restOfMobiHeader, 110, tempFlags, 0, 2);
                var mbhFlags = BitConverter.ToUInt16(tempFlags.BigEndian(), 0);
                Multibyte = Convert.ToBoolean(mbhFlags & 1);
                while (mbhFlags > 1)
                {
                    if ((mbhFlags & 2) == 2)
                        Trailers++;
                    mbhFlags >>= 1;
                }
            }

            var currentOffset = 132 + _restOfMobiHeader.Length + ExthHeaderSize;
            _remainder = new byte[(int)(mobiHeaderSize - currentOffset)];
            fs.Read(_remainder, 0, _remainder.Length);

            var fullNameIndexInRemainder = BitConverter.ToInt32(_fullNameOffset.BigEndian(), 0) - currentOffset;
            var fullNameLen = BitConverter.ToInt32(_fullNameLength.BigEndian(), 0);
            _fullName = new byte[fullNameLen];

            if (fullNameIndexInRemainder >= 0 &&
                fullNameIndexInRemainder < _remainder.Length &&
                fullNameIndexInRemainder + fullNameLen <= _remainder.Length && fullNameLen > 0)
            {
                Array.Copy(_remainder, fullNameIndexInRemainder, _fullName, 0, fullNameLen);
            }
        }

        public int ExthHeaderSize => ExtHeader?.Size ?? 0;

        public string FullName => Encoding.UTF8.GetString(_fullName).Trim('\0');

        public string IdentifierAsString => Encoding.ASCII.GetString(_identifier).Trim('\0');

        public uint HeaderLength => BitConverter.ToUInt32(_headerLength.BigEndian(), 0);

        public uint MobiType => BitConverter.ToUInt32(_mobiType.BigEndian(), 0);

        public string MobiTypeAsString
        {
            get
            {
                return MobiType switch
                {
                    2 => "Mobipocket Book",
                    3 => "PalmDoc Book",
                    4 => "Audio",
                    257 => "News",
                    258 => "News Feed",
                    259 => "News Magazine",
                    513 => "PICS",
                    514 => "WORD",
                    515 => "XLS",
                    516 => "PPT",
                    517 => "TEXT",
                    518 => "HTML",
                    _ => $"Unknown {nameof(MobiType)} - {MobiType}"
                };
            }
        }

        public uint UniqueId => BitConverter.ToUInt32(_uniqueId, 0);

        public uint FileVersion => BitConverter.ToUInt32(_fileVersion, 0);

        public uint IndexKeys => BitConverter.ToUInt32(_indexKeys, 0);

        public uint FirstNonBookIndex => BitConverter.ToUInt32(_firstNonBookIndex.BigEndian(), 0);

        public uint FullNameOffset => BitConverter.ToUInt32(_fullNameOffset.BigEndian(), 0);

        public uint FullNameLength => BitConverter.ToUInt32(_fullNameLength.BigEndian(), 0);

        public uint MinVersion => BitConverter.ToUInt32(_minVersion.BigEndian(), 0);

        public uint HuffmanRecordOffset => BitConverter.ToUInt32(_huffmanRecordOffset.BigEndian(), 0);

        public uint HuffmanRecordCount => BitConverter.ToUInt32(_huffmanRecordCount.BigEndian(), 0);

        public uint HuffmanTableOffset => BitConverter.ToUInt32(_huffmanTableOffset.BigEndian(), 0);

        public uint HuffmanTableLength => BitConverter.ToUInt32(_huffmanTableLength.BigEndian(), 0);
    }
}
