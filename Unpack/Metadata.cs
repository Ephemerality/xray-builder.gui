/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace XRayBuilderGUI.Unpack
{
    class Metadata
    {
        public PDBHeader PDB;
        public PalmDOCHeader PDH;
        public MobiHead mobiHeader;

        public Metadata(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            PDB = new PDBHeader(fs);
            PDH = new PalmDOCHeader(fs);
            mobiHeader = new MobiHead(fs, PDB.MobiHeaderSize);
        }

        public byte[] getRawML(FileStream fs)
        {
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
                    throw new Exception("HUFF/CDIC compression not supported yet. Please use the KindleUnpack option in settings.");
                default:
                    throw new Exception("Unknown compression type " + PDH.Compression + ".");
            }
            byte[] rawML = new byte[0];
            for (int i = 1; i <= PDH.RecordCount; i++)
            {
                byte[] buffer = new byte[PDB._recInfo[i + 1].RecordDataOffset - PDB._recInfo[i].RecordDataOffset];
                fs.Seek(PDB._recInfo[i].RecordDataOffset, SeekOrigin.Begin);
                fs.Read(buffer, 0, buffer.Length);
                buffer = trimTrailingDataEntries(buffer);
                byte[] result = decomp.unpack(buffer, PDB.MobiHeaderSize);
                rawML = rawML.Concat(result).ToArray();
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
