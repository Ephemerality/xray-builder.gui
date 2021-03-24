// Credit for the majority of code in this file goes to Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)

using System;
using System.IO;
using System.Text;
using Ephemerality.Unpack.Exceptions;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace Ephemerality.Unpack.Mobi
{
    public sealed class MobiHead
    {
        /// <summary>
        /// For internal implementation, the record to which this header belongs (always 0 for regular mobi, higher for KF8)
        /// </summary>
        public int HeaderRecordIndex { get; set; }

        public string Identifier { get; set; }
        public int HeaderLength { get; set; }
        public int MobiType { get; set; }
        public short CryptoType { get; set; }
        public ushort TextEncoding { get; set; }
        public uint UniqueId { get; set; }
        public int Version { get; set; }
        public int OrthographicIndex { get; set; }
        public int InflectionIndex { get; set; }
        public int IndexNames { get; set; }
        public int IndexKeys { get; set; }
        public int ExtraIndex0 { get; set; }
        public int ExtraIndex1 { get; set; }
        public int ExtraIndex2 { get; set; }
        public int ExtraIndex3 { get; set; }
        public int ExtraIndex4 { get; set; }
        public int ExtraIndex5 { get; set; }
        public int FirstNonBookIndex { get; set; }
        public int FullNameLength { get; set; }
        /// <summary>
        /// Book locale code.
        /// Low byte is main language 09 = English, next byte is dialect, 08 = British, 04 = US.
        /// Thus US English is 1033, UK English is 2057.
        /// </summary>
        public int Locale { get; set; }
        public int InputLanguage { get; set; }
        public int OutputLanguage { get; set; }
        public int MinVersion { get; set; }
        public int FirstImageIndex { get; set; }
        public int HuffmanRecordOffset { get; set; }
        public int HuffmanRecordCount { get; set; }
        public int HuffmanTableOffset { get; set; }
        public int HuffmanTableLength { get; set; }
        public int ExtHFlags { get; set; }
        private byte[] Unknown1 { get; }
        public int DrmOffset { get; set; }
        public int DrmCount { get; set; }
        public int DrmSize { get; set; }
        public int DrmFlags { get; set; }
        private byte[] Unknown2 { get; }
        public short FirstContentRecordNumber { get; set; }
        public short LastContentRecordNumber { get; set; }
        public int Unknown3 { get; set; }
        public int FcisRecordNumber { get; set; }
        public int Unknown4 { get; set; }
        public int FlisRecordNumber { get; set; }
        public int Unknown5 { get; set; }
        public long Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
        public int Unknown10 { get; set; }
        /// <summary>
        /// A set of binary flags, some of which indicate extra data at the end of each text block.
        /// This only seems to be valid for Mobipocket format version 5 and 6 (and higher?), when the header length is 228 (0xE4) or 232 (0xE8).
        /// bit 1 (0x1): extra multibyte bytes - size
        /// bit 2 (0x2): TBS indexing description of this HTML record - size
        /// bit 3 (0x4): uncrossable breaks - size
        /// Setting bit 2 (0x2) disables [guide][reference type = "start"] functionality.
        /// </summary>
        public int MbhFlags { get; set; }
        /// <summary>
        /// If not 0xFFFFFFFF, the record number of the first INDX record created from an ncx file.
        /// </summary>
        public int IndxRecordOffset { get; set; }
        private byte[] _fullName { get; }

        private readonly byte[] _restOfMobiHeader;
        public readonly ExtHeader ExtHeader;

        private readonly int _remainderLength;

        public bool Multibyte { get; }
        public int Trailers { get; }

        public byte[] RecordBytes()
        {
            using var writer = new EndianBinaryWriter(EndianBitConverter.Big, new MemoryStream());
            var extHeaderBytes = ExtHeader.HeaderBytes();
            var fullNameOffset = 16 + Identifier.Length + 112 + Unknown1.Length + 16 + Unknown2.Length + 56 + _restOfMobiHeader.Length + extHeaderBytes.Length;

            writer.Write(Encoding.ASCII.GetBytes(Identifier));
            writer.Write(HeaderLength);
            writer.Write(MobiType);
            writer.Write(CryptoType);
            writer.Write(TextEncoding);
            writer.Write(UniqueId);
            writer.Write(Version);
            writer.Write(OrthographicIndex);
            writer.Write(InflectionIndex);
            writer.Write(IndexNames);
            writer.Write(IndexKeys);
            writer.Write(ExtraIndex0);
            writer.Write(ExtraIndex1);
            writer.Write(ExtraIndex2);
            writer.Write(ExtraIndex3);
            writer.Write(ExtraIndex4);
            writer.Write(ExtraIndex5);
            writer.Write(FirstNonBookIndex);
            writer.Write(fullNameOffset);
            writer.Write(FullNameLength);
            writer.Write(Locale);
            writer.Write(InputLanguage);
            writer.Write(OutputLanguage);
            writer.Write(MinVersion);
            writer.Write(FirstImageIndex);
            writer.Write(HuffmanRecordOffset);
            writer.Write(HuffmanRecordCount);
            writer.Write(HuffmanTableOffset);
            writer.Write(HuffmanTableLength);
            writer.Write(ExtHFlags);
            writer.Write(Unknown1);
            writer.Write(DrmOffset);
            writer.Write(DrmCount);
            writer.Write(DrmSize);
            writer.Write(DrmFlags);
            writer.Write(Unknown2);
            writer.Write(FirstContentRecordNumber);
            writer.Write(LastContentRecordNumber);
            writer.Write(Unknown3);
            writer.Write(FcisRecordNumber);
            writer.Write(Unknown4);
            writer.Write(FlisRecordNumber);
            writer.Write(Unknown5);
            writer.Write(Unknown6);
            writer.Write(Unknown7);
            writer.Write(Unknown8);
            writer.Write(Unknown9);
            writer.Write(Unknown10);
            writer.Write(MbhFlags);
            writer.Write(IndxRecordOffset);
            writer.Write(_restOfMobiHeader);

            writer.Write(extHeaderBytes);

            if (_fullName != null)
                writer.Write(_fullName);

            writer.Write(new byte[_remainderLength]);

            return ((MemoryStream) writer.BaseStream).ToArray();
        }

        public MobiHead(Stream fs, uint mobiHeaderSize)
        {
            var reader = new EndianBinaryReader(EndianBitConverter.Big, fs);
            Identifier = Encoding.UTF8.GetString(reader.ReadBytes(4)).Trim('\0');
            if (Identifier != "MOBI")
                throw new UnpackException("Did not get expected MOBI identifier");

            HeaderLength = reader.ReadInt32();
            MobiType = reader.ReadInt32();
            CryptoType = reader.ReadInt16();
            TextEncoding = reader.ReadUInt16();
            UniqueId = reader.ReadUInt32();
            Version = reader.ReadInt32();
            OrthographicIndex = reader.ReadInt32();
            InflectionIndex = reader.ReadInt32();
            IndexNames = reader.ReadInt32();
            IndexKeys = reader.ReadInt32();
            ExtraIndex0 = reader.ReadInt32();
            ExtraIndex1 = reader.ReadInt32();
            ExtraIndex2 = reader.ReadInt32();
            ExtraIndex3 = reader.ReadInt32();
            ExtraIndex4 = reader.ReadInt32();
            ExtraIndex5 = reader.ReadInt32();
            FirstNonBookIndex = reader.ReadInt32();
            var fullNameOffset = reader.ReadInt32();
            FullNameLength = reader.ReadInt32();
            Locale = reader.ReadInt32();
            InputLanguage = reader.ReadInt32();
            OutputLanguage = reader.ReadInt32();
            MinVersion = reader.ReadInt32();
            FirstImageIndex = reader.ReadInt32();
            HuffmanRecordOffset = reader.ReadInt32();
            HuffmanRecordCount = reader.ReadInt32();
            HuffmanTableOffset = reader.ReadInt32();
            HuffmanTableLength = reader.ReadInt32();
            ExtHFlags = reader.ReadInt32();
            Unknown1 = reader.ReadBytesOrThrow(32);
            DrmOffset = reader.ReadInt32();
            DrmCount = reader.ReadInt32();
            DrmSize = reader.ReadInt32();
            DrmFlags = reader.ReadInt32();
            Unknown2 = reader.ReadBytes(12);
            FirstContentRecordNumber = reader.ReadInt16();
            LastContentRecordNumber = reader.ReadInt16();
            Unknown3 = reader.ReadInt32();
            FcisRecordNumber = reader.ReadInt32();
            Unknown4 = reader.ReadInt32();
            FlisRecordNumber = reader.ReadInt32();
            Unknown5 = reader.ReadInt32();
            Unknown6 = reader.ReadInt64();
            Unknown7 = reader.ReadInt32();
            Unknown8 = reader.ReadInt32();
            Unknown9 = reader.ReadInt32();
            Unknown10 = reader.ReadInt32();
            MbhFlags = reader.ReadInt32();
            IndxRecordOffset = reader.ReadInt32();

            // If anything is left, read it and save it
            // (length + 16 extra that are usually at the end and unknown (in newer mobi versions) - the 248 we've read in already)
            var bytesLeftInHeader = HeaderLength + 16 - 248;
            _restOfMobiHeader = bytesLeftInHeader > 0
                ? reader.ReadBytes(bytesLeftInHeader)
                : new byte[0];

            //If bit 6 (0x40) is set, then there's an EXTH record
            if ((ExtHFlags & 0x40) != 0)
                ExtHeader = new ExtHeader(reader);
            else
                throw new UnpackException("No EXT Header found. Ensure this book was processed with Calibre then try again.");

            // If applicable, read mbh flags regarding trailing bytes in record data
            if (MinVersion >= 5 && HeaderLength >= 228)
            {
                var mbhFlags = MbhFlags;
                Multibyte = Convert.ToBoolean(mbhFlags & 1);
                while (mbhFlags > 1)
                {
                    if ((mbhFlags & 2) == 2)
                        Trailers++;
                    mbhFlags >>= 1;
                }
            }

            var currentOffset = 248 + _restOfMobiHeader.Length + ExthHeaderSize;
            _remainderLength = (int) (mobiHeaderSize - currentOffset);
            var remainder = reader.ReadBytes(_remainderLength);

            var fullNameIndexInRemainder = fullNameOffset - currentOffset;
            if (fullNameIndexInRemainder >= 0 &&
                fullNameIndexInRemainder < remainder.Length &&
                fullNameIndexInRemainder + FullNameLength <= remainder.Length && FullNameLength > 0)
            {
                _fullName = new byte[FullNameLength];
                Array.Copy(remainder, fullNameIndexInRemainder, _fullName, 0, FullNameLength);
                _remainderLength -= FullNameLength;
            }
        }

        public int ExthHeaderSize => ExtHeader?.Size ?? 0;
        public string FullName => Encoding.UTF8.GetString(_fullName).Trim('\0');

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
    }
}
