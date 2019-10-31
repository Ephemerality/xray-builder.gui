/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using XRayBuilderGUI.Unpack.Mobi.Decompress;

namespace XRayBuilderGUI.Unpack.Mobi
{
    public sealed class Metadata : IMetadata
    {
        private PDBHeader _pdb;
        private PalmDOCHeader _pdh;
        private MobiHead _mobiHeader;
        private int _startRecord = 1;
        private List<byte[]> _headerRecords;

        public Metadata(FileStream fs)
        {
            Initialize(fs);
        }

        private void Initialize(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            _pdb = new PDBHeader(fs);
            _pdh = new PalmDOCHeader(fs);
            _mobiHeader = new MobiHead(fs, _pdb.MobiHeaderSize);
            // Use ASIN of the first book in the mobi
            var coverOffset = _mobiHeader.exthHeader.CoverOffset;
            var firstImage = -1;

            byte[] ReadRecord(int index)
            {
                fs.Seek(_pdb._recInfo[index].RecordDataOffset, SeekOrigin.Begin);
                var recSize = _pdb._recInfo[index + 1].RecordDataOffset - _pdb._recInfo[index].RecordDataOffset;
                var buffer = new byte[recSize];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }

            // Gather and start storing all header records from first book
            _headerRecords = new List<byte[]>(_pdb.NumRecords);
            for (var i = 0; i < _pdh.RecordCount; i++)
                _headerRecords.Add(ReadRecord(i));

            // Start at end of first book records, search for a second (KF8) and use it instead (for combo books)
            // Gather remaining records
            for (int i = _pdh.RecordCount; i < _pdb.NumRecords - 1; i++)
            {
                var buffer = ReadRecord(i);

                _headerRecords.Add(buffer);

                if (buffer.Length < 8)
                    continue;

                var imgtype = coverOffset == -1 ? "" : GetImageType(buffer);
                if (imgtype != "")
                {
                    if (firstImage == -1)
                        firstImage = i;
                    if (i != firstImage + coverOffset)
                        continue;
                    using var ms = new MemoryStream(buffer);
                    CoverImage = new Bitmap(ms);
                }
                else if (Encoding.ASCII.GetString(buffer, 0, 8) == "BOUNDARY")
                {
                    _startRecord = i + 2;
                    _pdh = new PalmDOCHeader(fs);
                    _mobiHeader = new MobiHead(fs, _pdb.MobiHeaderSize);
                    break;
                }
            }
        }

        public void Dispose()
        {
            CoverImage?.Dispose();
        }

        private static string GetImageType(byte[] data)
        {
            if ((data[6] == 'J' && data[7] == 'F' && data[8] == 'I' && data[9] == 'F')
                || (data[6] == 'E' && data[7] == 'x' && data[8] == 'i' && data[9] == 'f')
                || (data[0] == 0xFF && data[1] == 0xD8 && data[data.Length - 2] == 0xFF && data[data.Length - 1] == 0xD9))
                return "jpeg";
            if (data[0] == 0x89 && data[1] == 'P' && data[2] == 'N' && data[3] == 'G')
                return "png";
            return "";
        }

        public string Asin => _mobiHeader.exthHeader.ASIN != "" ? _mobiHeader.exthHeader.ASIN : _mobiHeader.exthHeader.ASIN2;

        public string DbName => _pdb.DBName;

        public string UniqueId => _mobiHeader.UniqueID.ToString();

        public string Author => _mobiHeader.exthHeader.Author;

        public string Title => _mobiHeader.FullName != "" ? _mobiHeader.FullName : _mobiHeader.exthHeader.UpdatedTitle;

        public long RawMlSize => _pdh.TextLength;

        public Image CoverImage { get; private set; }

        public string CdeContentType => _mobiHeader.exthHeader.CDEType;

        public void UpdateCdeContentType(FileStream fs) => _mobiHeader.exthHeader.UpdateCdeContentType(fs);
        public bool RawMlSupported { get; } = true;

        /// <summary>
        /// Throws <see cref="EncryptedBookException"/> if DRM is enabled.
        /// </summary>
        public void CheckDrm()
        {
            if (_pdh.EncryptionType != 0)
                throw new EncryptedBookException();
        }

        public void SaveRawMl(string path)
        {
            File.WriteAllBytes(path, GetRawMl());
        }

        public Stream GetRawMlStream()
        {
            return new MemoryStream(GetRawMl());
        }

        public byte[] GetRawMl()
        {
            CheckDrm();

            // TODO Convert to Factory pattern
            var decomp = _pdh.Compression switch
            {
                1 => (IDecompressor) new UncompressedReader(),
                2 => new PalmDocReader(),
                17480 => new HuffCdicReader(),
                _ => throw new UnpackException("Unknown compression type " + _pdh.Compression + ".")
            };

            decomp.Initialize(_mobiHeader, _pdb, _headerRecords);
            var rawMl = new byte[0];
            var endRecord = _startRecord + _pdh.RecordCount -1;
            for (var i = _startRecord; i <= endRecord; i++)
            {
                var buffer = _headerRecords[i];
                buffer = TrimTrailingDataEntries(buffer);
                var result = decomp.Unpack(buffer);
                buffer = new byte[rawMl.Length + result.Length];
                Buffer.BlockCopy(rawMl, 0, buffer, 0, rawMl.Length);
                Buffer.BlockCopy(result, 0, buffer, rawMl.Length, result.Length);
                rawMl = buffer;
            }
            return rawMl;
        }

        private byte[] TrimTrailingDataEntries(byte[] data)
        {
            for (var i = 0; i < _mobiHeader.trailers; i++)
            {
                var num = getSizeOfTrailingDataEntry(data);
                var temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }
            if (_mobiHeader.multibyte)
            {
                var num = (data[data.Length - 1] & 3) + 1;
                var temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }

            return data;
        }

        private static int getSizeOfTrailingDataEntry(byte[] data)
        {
            var num = 0;
            for (var i = data.Length - 4; i < data.Length; i++)
            {
                if ((data[i] & 0x80) == 0x80)
                    num = 0;
                num = (num << 7) | (data[i] & 0x7f);
            }
            return num;
        }
    }

    public sealed class EncryptedBookException : Exception
    {
        public EncryptedBookException() : base("-This book has DRM (it is encrypted). X-Ray Builder will only work on books that do not have DRM.") { }
    }

    public sealed class UnpackException : Exception
    {
        public UnpackException(string message) : base(message) { }
    }
}
