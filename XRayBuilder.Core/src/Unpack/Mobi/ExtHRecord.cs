using System.Text;
using MiscUtil.IO;

namespace XRayBuilder.Core.Unpack.Mobi
{
    public sealed class ExtHRecord
    {
        public int RecordType { get; set; }
        public int RecordLength { get; set; }
        public byte[] RecordData { get; set; }

        public ExtHRecord(EndianBinaryReader reader)
        {
            RecordType = reader.ReadInt32();
            RecordLength = reader.ReadInt32();

            if (RecordLength < 8)
                throw new UnpackException("Invalid EXTH record length");

            RecordData = reader.ReadBytes((int) RecordLength - 8);
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(RecordType);
            writer.Write(RecordLength);
            writer.Write(RecordData);
        }

        public override string ToString() => Encoding.UTF8.GetString(RecordData);

        public int Size => RecordData.Length + 8;
    }
}