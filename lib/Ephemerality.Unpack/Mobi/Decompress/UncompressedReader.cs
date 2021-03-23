
namespace Ephemerality.Unpack.Mobi.Decompress
{
    public sealed class UncompressedReader : Decompressor
    {
        public override byte[] Unpack(byte[] data) => data;
    }
}