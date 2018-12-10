/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace XRayBuilderGUI.Unpack.Mobi
{
    public sealed class Metadata : IDisposable, IMetadata
    {
        private PDBHeader _pdb;
        private PalmDOCHeader _pdh;
        private MobiHead _mobiHeader;
        private int _startRecord = 1;
        private readonly FileStream _fs;

        public Metadata(string file)
        {
            var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            _fs = fs;
            Initialize(fs);
        }

        private void Initialize(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            _pdb = new PDBHeader(fs);
            _pdh = new PalmDOCHeader(fs);
            _mobiHeader = new MobiHead(fs, _pdb.MobiHeaderSize);
            // Use ASIN of the first book in the mobi
            int coverOffset = _mobiHeader.exthHeader.CoverOffset;
            int firstImage = -1;
            // Start at end of first book records, search for a second (KF8) and use it instead (for combo books)
            for (int i = _pdh.RecordCount; i < _pdb.NumRecords - 1; i++)
            {
                uint recSize = _pdb._recInfo[i + 1].RecordDataOffset - _pdb._recInfo[i].RecordDataOffset;
                if (recSize < 8) continue;
                byte[] buffer = new byte[recSize];
                fs.Seek(_pdb._recInfo[i].RecordDataOffset, SeekOrigin.Begin);
                fs.Read(buffer, 0, buffer.Length);
                string imgtype = coverOffset == -1 ? "" : GetImageType(buffer);
                if (imgtype != "")
                {
                    if (firstImage == -1) firstImage = i;
                    if (i == firstImage + coverOffset)
                    {
                        using (MemoryStream ms = new MemoryStream(buffer))
                            CoverImage = new Bitmap(ms);
                    }
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
            _fs?.Dispose();
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

            Decompressor decomp;
            switch (_pdh.Compression)
            {
                case (1):
                    decomp = new UncompressedReader();
                    break;
                case (2):
                    decomp = new PalmDOCReader();
                    break;
                case (17480):
                    HUFFCDICReader reader = new HUFFCDICReader();
                    try
                    {
                        int recOffset = (int)_mobiHeader.HuffmanRecordOffset;
                        byte[] huffSect = new byte[_pdb._recInfo[recOffset + 1].RecordDataOffset - _pdb._recInfo[recOffset].RecordDataOffset];
                        _fs.Seek(_pdb._recInfo[recOffset].RecordDataOffset, SeekOrigin.Begin);
                        _fs.Read(huffSect, 0, huffSect.Length);
                        reader.loadHuff(huffSect);
                        int recCount = (int)_mobiHeader.HuffmanRecordCount;
                        for (int i = 1; i < recCount; i++)
                        {
                            huffSect = new byte[_pdb._recInfo[recOffset + i + 1].RecordDataOffset - _pdb._recInfo[recOffset + i].RecordDataOffset];
                            _fs.Seek(_pdb._recInfo[recOffset + i].RecordDataOffset, SeekOrigin.Begin);
                            _fs.Read(huffSect, 0, huffSect.Length);
                            reader.loadCdic(huffSect);
                        }
                    } catch (Exception ex)
                    {
                        throw new UnpackException("Error in HUFF/CDIC decompression: " + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    decomp = reader;
                    break;
                default:
                    throw new UnpackException("Unknown compression type " + _pdh.Compression + ".");
            }
            byte[] rawMl = new byte[0];
            int endRecord = _startRecord + _pdh.RecordCount -1;
            for (int i = _startRecord; i <= endRecord; i++)
            {
                byte[] buffer = new byte[_pdb._recInfo[i + 1].RecordDataOffset - _pdb._recInfo[i].RecordDataOffset];
                _fs.Seek(_pdb._recInfo[i].RecordDataOffset, SeekOrigin.Begin);
                _fs.Read(buffer, 0, buffer.Length);
                buffer = trimTrailingDataEntries(buffer);
                byte[] result = decomp.unpack(buffer);
                buffer = new byte[rawMl.Length + result.Length];
                Buffer.BlockCopy(rawMl, 0, buffer, 0, rawMl.Length);
                Buffer.BlockCopy(result, 0, buffer, rawMl.Length, result.Length);
                rawMl = buffer;
            }
            return rawMl;
        }

        private byte[] trimTrailingDataEntries(byte[] data)
        {
            for (int i = 0; i < _mobiHeader.trailers; i++)
            {
                int num = getSizeOfTrailingDataEntry(data);
                byte[] temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }
            if (_mobiHeader.multibyte)
            {
                int num = (data[data.Length - 1] & 3) + 1;
                byte[] temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }
            return data;
        }

        private int getSizeOfTrailingDataEntry(byte[] data)
        {
            int num = 0;
            for (int i = data.Length - 4; i < data.Length; i++)
            {
                if ((data[i] & 0x80) == 0x80)
                    num = 0;
                num = (num << 7) | (data[i] & 0x7f);
            }
            return num;
        }
    }

    public class EncryptedBookException : Exception
    {
        public EncryptedBookException() : base("-This book has DRM (it is encrypted). X-Ray Builder will only work on books that do not have DRM.") { }
    }

    public class UnpackException : Exception
    {
        public UnpackException(string message) : base(message) { }
    }
}
