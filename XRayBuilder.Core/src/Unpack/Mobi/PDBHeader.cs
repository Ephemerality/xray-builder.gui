/* Credit to the following for ideas/code in this file:
 * Limey for the C# Metadata reader (http://www.mobileread.com/forums/showthread.php?t=185565)
 * KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)
 * MobileRead Wiki (http://wiki.mobileread.com/wiki/MOBI)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Unpack.Mobi
{
    public class PDBHeader
    {
        private byte[] _dbName = new byte[32];
        private byte[] _numRecords = new byte[2];
        public List<RecordInfo> _recInfo = new List<RecordInfo>();

        public PDBHeader(FileStream fs)
        {
            fs.Read(_dbName, 0, 32);
            fs.Seek(44, SeekOrigin.Current);
            fs.Read(_numRecords, 0, 2);
            var numRecords = NumRecords;
            for (var i = 0; i < numRecords; i++)
            {
                _recInfo.Add(new RecordInfo(fs));
            }
            fs.Seek(2, SeekOrigin.Current);
        }

        public string DBName => Encoding.UTF8.GetString(_dbName).Trim('\0');

        public int NumRecords => BitConverter.ToInt16(_numRecords.BigEndian(), 0);

        public uint MobiHeaderSize
        {
            get
            {
                if (_recInfo.Count > 1)
                    return _recInfo[1].RecordDataOffset - _recInfo[0].RecordDataOffset;
                return 0;
            }
        }
    }

    public class RecordInfo
    {
        private byte[] _recordDataOffset = new byte[4];
        private byte _recordAttributes;
        private byte[] _uniqueID = new byte[3];

        public RecordInfo(FileStream fs)
        {
            fs.Read(_recordDataOffset, 0, _recordDataOffset.Length);
            _recordAttributes = (byte)fs.ReadByte();
            fs.Read(_uniqueID, 0, _uniqueID.Length);
        }

        public uint RecordDataOffset => BitConverter.ToUInt32(_recordDataOffset.BigEndian(), 0);
    }
}
