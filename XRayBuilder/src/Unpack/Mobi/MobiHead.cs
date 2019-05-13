// Credit for the majority of code in this file goes to Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)

using System;
using System.IO;
using System.Text;

namespace XRayBuilderGUI.Unpack.Mobi
{
    public class MobiHead
    {
        private readonly byte[] identifier = new byte[4];
        private readonly byte[] headerLength = new byte[4];
        private readonly byte[] mobiType = new byte[4];
        private readonly byte[] textEncoding = new byte[4];
        private readonly byte[] uniqueID = new byte[4];
        private readonly byte[] fileVersion = new byte[4];
        private readonly byte[] orthographicIndex = new byte[4];
        private readonly byte[] inflectionIndex = new byte[4];
        private readonly byte[] indexNames = new byte[4];
        private readonly byte[] indexKeys = new byte[4];
        private readonly byte[] extraIndex0 = new byte[4];
        private readonly byte[] extraIndex1 = new byte[4];
        private readonly byte[] extraIndex2 = new byte[4];
        private readonly byte[] extraIndex3 = new byte[4];
        private readonly byte[] extraIndex4 = new byte[4];
        private readonly byte[] extraIndex5 = new byte[4];
        private readonly byte[] firstNonBookIndex = new byte[4];
        private readonly byte[] fullNameOffset = new byte[4];
        private readonly byte[] fullNameLength = new byte[4];
        private readonly byte[] locale = new byte[4];
        private readonly byte[] inputLanguage = new byte[4];
        private readonly byte[] outputLanguage = new byte[4];
        private readonly byte[] minVersion = new byte[4];
        private readonly byte[] firstImageIndex = new byte[4];
        private readonly byte[] huffmanRecordOffset = new byte[4];
        private readonly byte[] huffmanRecordCount = new byte[4];
        private readonly byte[] huffmanTableOffset = new byte[4];
        private readonly byte[] huffmanTableLength = new byte[4];
        private readonly byte[] exthFlags = new byte[4];
        private readonly byte[] restOfMobiHeader;
        public EXTHHeader exthHeader;

        private readonly byte[] remainder;
        private readonly byte[] fullName;

        public bool multibyte;
        public int trailers;

        public MobiHead(FileStream fs, uint mobiHeaderSize)
        {
            fs.Read(identifier, 0, identifier.Length);
            if (IdentifierAsString != "MOBI")
                throw new UnpackException("Did not get expected MOBI identifier");

            fs.Read(headerLength, 0, headerLength.Length);
            restOfMobiHeader = new byte[HeaderLength + 16 - 132];

            fs.Read(mobiType, 0, mobiType.Length);
            fs.Read(textEncoding, 0, textEncoding.Length);
            fs.Read(uniqueID, 0, uniqueID.Length);
            Array.Reverse(uniqueID);
            fs.Read(fileVersion, 0, fileVersion.Length);
            fs.Read(orthographicIndex, 0, orthographicIndex.Length);
            fs.Read(inflectionIndex, 0, inflectionIndex.Length);
            fs.Read(indexNames, 0, indexNames.Length);
            fs.Read(indexKeys, 0, indexKeys.Length);
            fs.Read(extraIndex0, 0, extraIndex0.Length);
            fs.Read(extraIndex1, 0, extraIndex1.Length);
            fs.Read(extraIndex2, 0, extraIndex2.Length);
            fs.Read(extraIndex3, 0, extraIndex3.Length);
            fs.Read(extraIndex4, 0, extraIndex4.Length);
            fs.Read(extraIndex5, 0, extraIndex5.Length);
            fs.Read(firstNonBookIndex, 0, firstNonBookIndex.Length);
            fs.Read(fullNameOffset, 0, fullNameOffset.Length);
            fs.Read(fullNameLength, 0, fullNameLength.Length);
            fs.Read(locale, 0, locale.Length);
            fs.Read(inputLanguage, 0, inputLanguage.Length);
            fs.Read(outputLanguage, 0, outputLanguage.Length);
            fs.Read(minVersion, 0, minVersion.Length);
            fs.Read(firstImageIndex, 0, firstImageIndex.Length);
            fs.Read(huffmanRecordOffset, 0, huffmanRecordOffset.Length);
            fs.Read(huffmanRecordCount, 0, huffmanRecordCount.Length);
            fs.Read(huffmanTableOffset, 0, huffmanTableOffset.Length);
            fs.Read(huffmanTableLength, 0, huffmanTableLength.Length);
            fs.Read(exthFlags, 0, exthFlags.Length);

            //If bit 6 (0x40) is set, then there's an EXTH record
            bool exthExists = (BitConverter.ToUInt32(Functions.CheckBytes(exthFlags), 0) & 0x40) != 0;

            fs.Read(restOfMobiHeader, 0, restOfMobiHeader.Length);

            if (exthExists)
                exthHeader = new EXTHHeader(fs);
            else
                throw new UnpackException("No EXT Header found. Ensure this book was processed with Calibre then try again.");

            // If applicable, read mbh flags regarding trailing bytes in record data
            if (MinVersion >= 5 && HeaderLength >= 228)
            {
                byte[] tempFlags = new byte[2];
                Array.Copy(restOfMobiHeader, 110, tempFlags, 0, 2);
                ushort mbhFlags = BitConverter.ToUInt16(Functions.CheckBytes(tempFlags), 0);
                multibyte = Convert.ToBoolean(mbhFlags & 1);
                while (mbhFlags > 1)
                {
                    if ((mbhFlags & 2) == 2)
                        trailers++;
                    mbhFlags >>= 1;
                }
            }

            int currentOffset = 132 + restOfMobiHeader.Length + ExthHeaderSize;
            remainder = new byte[(int)(mobiHeaderSize - currentOffset)];
            fs.Read(remainder, 0, remainder.Length);

            int fullNameIndexInRemainder = BitConverter.ToInt32(Functions.CheckBytes(fullNameOffset), 0) - currentOffset;
            int fullNameLen = BitConverter.ToInt32(Functions.CheckBytes(fullNameLength), 0);
            fullName = new byte[fullNameLen];

            if (fullNameIndexInRemainder >= 0 &&
                fullNameIndexInRemainder < remainder.Length &&
                fullNameIndexInRemainder + fullNameLen <= remainder.Length && fullNameLen > 0)
            {
                Array.Copy(remainder, fullNameIndexInRemainder, fullName, 0, fullNameLen);
            }
        }

        public int ExthHeaderSize
        {
            get
            {
                if (exthHeader == null)
                    return 0;
                else
                    return exthHeader.Size;
            }

        }

        public string FullName => Encoding.UTF8.GetString(fullName).Trim('\0');

        public string IdentifierAsString => Encoding.ASCII.GetString(identifier).Trim('\0');

        public uint HeaderLength => BitConverter.ToUInt32(Functions.CheckBytes(headerLength), 0);

        public uint MobiType => BitConverter.ToUInt32(Functions.CheckBytes(mobiType), 0);

        public string MobiTypeAsString
        {
            get
            {
                switch (MobiType)
                {
                    case 2: return "Mobipocket Book";
                    case 3: return "PalmDoc Book";
                    case 4: return "Audio";
                    case 257: return "News";
                    case 258: return "News Feed";
                    case 259: return "News Magazine";
                    case 513: return "PICS";
                    case 514: return "WORD";
                    case 515: return "XLS";
                    case 516: return "PPT";
                    case 517: return "TEXT";
                    case 518: return "HTML";
                    default:
                        return string.Format("Unknown {0}", MobiType);
                }
            }
        }

        public uint UniqueID => BitConverter.ToUInt32(uniqueID, 0);

        public uint FileVersion => BitConverter.ToUInt32(fileVersion, 0);

        public uint IndexKeys => BitConverter.ToUInt32(indexKeys, 0);

        public uint FirstNonBookIndex => BitConverter.ToUInt32(Functions.CheckBytes(firstNonBookIndex), 0);

        public uint FullNameOffset => BitConverter.ToUInt32(Functions.CheckBytes(fullNameOffset), 0);

        public uint FullNameLength => BitConverter.ToUInt32(Functions.CheckBytes(fullNameLength), 0);

        public uint MinVersion => BitConverter.ToUInt32(Functions.CheckBytes(minVersion), 0);

        public uint HuffmanRecordOffset => BitConverter.ToUInt32(Functions.CheckBytes(huffmanRecordOffset), 0);

        public uint HuffmanRecordCount => BitConverter.ToUInt32(Functions.CheckBytes(huffmanRecordCount), 0);

        public uint HuffmanTableOffset => BitConverter.ToUInt32(Functions.CheckBytes(huffmanTableOffset), 0);

        public uint HuffmanTableLength => BitConverter.ToUInt32(Functions.CheckBytes(huffmanTableLength), 0);
    }
}
