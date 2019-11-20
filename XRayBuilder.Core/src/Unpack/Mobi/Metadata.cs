/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;
using XRayBuilder.Core.Libraries.IO.Extensions;
using XRayBuilder.Core.Unpack.Mobi.Decompress;

namespace XRayBuilder.Core.Unpack.Mobi
{
    public sealed class Metadata : IMetadata
    {
        private PDBHeader _pdb;
        private PalmDocHeader _pdh;
        private MobiHead _mobiHeader;
        private PalmDocHeader _palmDocHeaderKf8;
        private MobiHead _mobiHeadKf8;
        private int _startRecord = 1;
        private List<byte[]> _headerRecords;

        private PalmDocHeader _activePdh;
        private MobiHead _activeMobiHeader;

        public Metadata(FileStream fs)
        {
            Initialize(fs);
        }

        internal Metadata()
        { }

        private void Initialize(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            _pdb = new PDBHeader(fs);
            _pdh = _activePdh = new PalmDocHeader(fs);
            _mobiHeader = _activeMobiHeader = new MobiHead(fs, _pdb.MobiHeaderSize)
            {
                HeaderRecordIndex = 0
            };
            // Use ASIN of the first book in the mobi
            var coverOffset = _mobiHeader.ExtHeader.CoverOffset;
            var firstImage = -1;

            byte[] ReadRecord(int index)
            {
                fs.Seek(_pdb.RecordMetadata[index].DataOffset, SeekOrigin.Begin);

                return index < _pdb.RecordMetadata.Length - 1
                    ? fs.ReadBytes((int) (_pdb.RecordMetadata[index + 1].DataOffset - _pdb.RecordMetadata[index].DataOffset))
                    : fs.ReadToEnd();
            }

            // Gather and start storing all header records from first book
            _headerRecords = new List<byte[]>(_pdb.NumRecords);
            for (var i = 0; i < _pdh.RecordCount; i++)
                _headerRecords.Add(ReadRecord(i));

            // Start at end of first book records, search for a second (KF8) and use it instead (for combo books)
            // Gather remaining records
            for (int i = _pdh.RecordCount; i < _pdb.NumRecords; i++)
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
                    _palmDocHeaderKf8 = _activePdh = new PalmDocHeader(fs);
                    _mobiHeadKf8 = _activeMobiHeader = new MobiHead(fs, _pdb.MobiHeaderSize)
                    {
                        HeaderRecordIndex = i
                    };
                }
            }
        }

        public void Save(Stream stream)
        {
            var writer = new EndianBinaryWriter(EndianBitConverter.Big, stream);
            // PDB header
            writer.Write(_pdb.HeaderBytes());

            // TODO Remove exth from mobihead and keep it separate
            // Dump all records
            foreach (var record in _headerRecords)
                writer.Write(record);
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

        public bool IsAzw3 => _activeMobiHeader?.Version >= 8;

        public string Asin => _activeMobiHeader.ExtHeader.Asin != ""
            ? _activeMobiHeader.ExtHeader.Asin
            : _activeMobiHeader.ExtHeader.Asin2;

        public string DbName => _pdb.DBName;

        public string UniqueId => _activeMobiHeader.UniqueId.ToString();
        public bool CanModify => true;

        public string Author => _activeMobiHeader.ExtHeader.Author;

        public string Title => _activeMobiHeader.FullName != ""
            ? _activeMobiHeader.FullName
            : _activeMobiHeader.ExtHeader.UpdatedTitle;

        public long RawMlSize => _activePdh.TextLength;

        public Image CoverImage { get; private set; }

        public string CdeContentType => _activeMobiHeader.ExtHeader.CdeType;

        public void UpdateCdeContentType()
            => UpdateOrCreateExtHeaderRecord(501, Encoding.UTF8.GetBytes("EBOK"));

        private void UpdateOrCreateExtHeaderRecord(int recordType, byte[] recordData)
        {
            foreach (var mobiHeader in new[] { _mobiHeader, _mobiHeadKf8 }.Where(header => header != null))
            {
                var rec = mobiHeader.ExtHeader.RecordList.FirstOrDefault(r => r.RecordType == recordType);
                if (rec == null)
                {
                    rec = new ExtHRecord
                    {
                        RecordData = recordData,
                        RecordType = recordType
                    };
                    mobiHeader.ExtHeader.RecordList.Add(rec);
                }
                else
                    rec.RecordData = recordData;
            }

            RebuildMobiHeaders();
        }

        public void SetAsin(string asin)
            => UpdateOrCreateExtHeaderRecord(113, Encoding.ASCII.GetBytes(asin));

        private void RebuildMobiHeaders()
        {
            _headerRecords[0] = _pdh.HeaderBytes().Concat(_mobiHeader.RecordBytes()).ToArray();
            if (_mobiHeadKf8 != null)
                _headerRecords[_mobiHeadKf8.HeaderRecordIndex] = _palmDocHeaderKf8.HeaderBytes().Concat(_mobiHeadKf8.RecordBytes()).ToArray();

            var offset = _pdb.HeaderBytes().Length;
            for (var i = 0; i < _pdb.RecordMetadata.Length; i++)
            {
                _pdb.RecordMetadata[i].DataOffset = (uint) offset;
                offset += _headerRecords[i].Length;
            }
        }

        public bool RawMlSupported { get; } = true;

        /// <summary>
        /// Throws <see cref="EncryptedBookException"/> if DRM is enabled.
        /// </summary>
        public void CheckDrm()
        {
            if (_activePdh.EncryptionType != 0)
                throw new EncryptedBookException();
        }

        public void SaveRawMl(string path)
            => File.WriteAllBytes(path, GetRawMl());

        public Stream GetRawMlStream()
            => new MemoryStream(GetRawMl());

        public byte[] GetRawMl()
        {
            CheckDrm();

            // TODO Convert to Factory pattern
            var decomp = _activePdh.Compression switch
            {
                1 => (IDecompressor) new UncompressedReader(),
                2 => new PalmDocReader(),
                17480 => new HuffCdicReader(),
                _ => throw new UnpackException("Unknown compression type " + _activePdh.Compression + ".")
            };

            decomp.Initialize(_mobiHeader, _pdb, _headerRecords);
            var rawMl = new byte[0];
            var endRecord = _startRecord + _activePdh.RecordCount - 1;
            for (var i = _startRecord; i <= endRecord; i++)
            {
                var headerBuffer = TrimTrailingDataEntries(_headerRecords[i]);
                var result = decomp.Unpack(headerBuffer);
                var buffer = new byte[rawMl.Length + result.Length];
                Buffer.BlockCopy(rawMl, 0, buffer, 0, rawMl.Length);
                Buffer.BlockCopy(result, 0, buffer, rawMl.Length, result.Length);
                rawMl = buffer;
            }

            return rawMl;
        }

        // TODO Could be more efficient
        private byte[] TrimTrailingDataEntries(byte[] data)
        {
            for (var i = 0; i < _mobiHeader.Trailers; i++)
            {
                var num = GetSizeOfTrailingDataEntry(data);
                var temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }
            if (_mobiHeader.Multibyte)
            {
                var num = (data[data.Length - 1] & 3) + 1;
                var temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }

            return data;
        }

        private static int GetSizeOfTrailingDataEntry(byte[] data)
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
}
