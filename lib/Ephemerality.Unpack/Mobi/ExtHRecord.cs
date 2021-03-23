using System.Text;
using MiscUtil.IO;

namespace Ephemerality.Unpack.Mobi
{
    public sealed class ExtHRecord
    {
        public int RecordType { get; set; }
        public byte[] RecordData { get; set; }

        public ExtHRecord() { }

        public ExtHRecord(EndianBinaryReader reader)
        {
            RecordType = reader.ReadInt32();
            var recordLength = reader.ReadInt32();

            if (recordLength < 8)
                throw new UnpackException("Invalid EXTH record length");

            RecordData = reader.ReadBytes(recordLength - 8);
        }

        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(RecordType);
            writer.Write(Size);
            writer.Write(RecordData);
        }

        public override string ToString() => Encoding.UTF8.GetString(RecordData);

        public int Size => RecordData.Length + 8;
    }
}