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

namespace XRayBuilderGUI.Unpack
{
    public class Metadata
    {
        public PDBHeader PDB;
        public PalmDOCHeader PDH;
        public MobiHead mobiHeader;
        public Bitmap coverImage;
        private int _startRecord = 1;
        public string rawMLPath = "";
        private string _ASIN = "";

        public Metadata(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            PDB = new PDBHeader(fs);
            PDH = new PalmDOCHeader(fs);
            mobiHeader = new MobiHead(fs, PDB.MobiHeaderSize);
            // Use ASIN of the first book in the mobi
            _ASIN = mobiHeader.exthHeader.ASIN != "" ? mobiHeader.exthHeader.ASIN : mobiHeader.exthHeader.ASIN2;
            int coverOffset = mobiHeader.exthHeader.CoverOffset;
            int firstImage = -1;
            // Start at end of first book records, search for a second (KF8) and use it instead (for combo books)
            for (int i = PDH.RecordCount; i < PDB.NumRecords - 1; i++)
            {
                uint recSize = PDB._recInfo[i + 1].RecordDataOffset - PDB._recInfo[i].RecordDataOffset;
                if (recSize < 8) continue;
                byte[] buffer = new byte[recSize];
                fs.Seek(PDB._recInfo[i].RecordDataOffset, SeekOrigin.Begin);
                fs.Read(buffer, 0, buffer.Length);
                string imgtype = coverOffset == -1 ? "" : get_image_type(buffer);
                if (imgtype != "")
                {
                    if (firstImage == -1) firstImage = i;
                    if (i == firstImage + coverOffset)
                    {
                        using (MemoryStream ms = new MemoryStream(buffer))
                            coverImage = new Bitmap(ms);
                    }
                }
                else if (Encoding.ASCII.GetString(buffer, 0, 8) == "BOUNDARY")
                {
                    _startRecord = i + 2;
                    PDH = new PalmDOCHeader(fs);
                    mobiHeader = new MobiHead(fs, PDB.MobiHeaderSize);
                    break;
                }
            }
        }

        private string get_image_type(byte[] data)
        {
            if ((data[6] == 'J' && data[7] == 'F' && data[8] == 'I' && data[9] == 'F')
                || (data[6] == 'E' && data[7] == 'x' && data[8] == 'i' && data[9] == 'f')
                || (data[0] == 0xFF && data[1] == 0xD8 && data[data.Length - 2] == 0xFF && data[data.Length - 1] == 0xD9))
                return "jpeg";
            return "";
        }

        // Temporary function to mimic old GetMetaData functionality until it can be removed
        // 0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML, 4 = author, 5 = title
        public List<string> getResults()
        {
            List<string> results = new List<string>(6);
            results.Add(ASIN);
            results.Add(UniqueID);
            results.Add(DBName);
            results.Add(rawMLPath);
            results.Add(Author);
            results.Add(Title);
            return results;
        }

        public string ASIN
        {
            get { return _ASIN; }
        }

        public string DBName
        {
            get { return PDB.DBName; }
        }

        public string UniqueID
        {
            get { return mobiHeader.UniqueID.ToString(); }
        }

        public string Author
        {
            get { return mobiHeader.exthHeader.Author; }
        }

        public string Title
        {
            get
            {
                if (mobiHeader.FullName != "")
                    return mobiHeader.FullName;
                else
                    return mobiHeader.exthHeader.UpdatedTitle;
            }
        }

        public long rawMLSize()
        {
            return PDH.TextLength;
        }

        public byte[] getRawML(FileStream fs)
        {
            if (PDH.EncryptionType != 0)
                throw new Exception("This book has DRM (it is encrypted). X-Ray Builder will only work on books that do not have DRM.");

            Decompressor decomp;
            switch (PDH.Compression)
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
                        int recOffset = (int)mobiHeader.HuffmanRecordOffset;
                        byte[] huffSect = new byte[PDB._recInfo[recOffset + 1].RecordDataOffset - PDB._recInfo[recOffset].RecordDataOffset];
                        fs.Seek(PDB._recInfo[recOffset].RecordDataOffset, SeekOrigin.Begin);
                        fs.Read(huffSect, 0, huffSect.Length);
                        reader.loadHuff(huffSect);
                        int recCount = (int)mobiHeader.HuffmanRecordCount;
                        for (int i = 1; i < recCount; i++)
                        {
                            huffSect = new byte[PDB._recInfo[recOffset + i + 1].RecordDataOffset - PDB._recInfo[recOffset + i].RecordDataOffset];
                            fs.Seek(PDB._recInfo[recOffset + i].RecordDataOffset, SeekOrigin.Begin);
                            fs.Read(huffSect, 0, huffSect.Length);
                            reader.loadCdic(huffSect);
                        }
                    } catch (Exception ex)
                    {
                        throw new Exception("Error in HUFF/CDIC decompression: " + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    decomp = reader;
                    break;
                default:
                    throw new Exception("Unknown compression type " + PDH.Compression + ".");
            }
            byte[] rawML = new byte[0];
            int endRecord = _startRecord + PDH.RecordCount -1;
            for (int i = _startRecord; i <= endRecord; i++)
            {
                byte[] buffer = new byte[PDB._recInfo[i + 1].RecordDataOffset - PDB._recInfo[i].RecordDataOffset];
                fs.Seek(PDB._recInfo[i].RecordDataOffset, SeekOrigin.Begin);
                fs.Read(buffer, 0, buffer.Length);
                buffer = trimTrailingDataEntries(buffer);
                byte[] result = decomp.unpack(buffer, PDB.MobiHeaderSize);
                buffer = new byte[rawML.Length + result.Length];
                Buffer.BlockCopy(rawML, 0, buffer, 0, rawML.Length);
                Buffer.BlockCopy(result, 0, buffer, rawML.Length, result.Length);
                rawML = buffer;
            }
            return rawML;
        }

        private byte[] trimTrailingDataEntries(byte[] data)
        {
            for (int i = 0; i < mobiHeader.trailers; i++)
            {
                int num = getSizeOfTrailingDataEntry(data);
                byte[] temp = new byte[data.Length - num];
                Array.Copy(data, temp, temp.Length);
                data = temp;
            }
            if (mobiHeader.multibyte)
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
}
