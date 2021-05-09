using System.IO;

namespace Ephemerality.Unpack.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Read <paramref name="count"/> bytes from <paramref name="stream"/> after seeking to <paramref name="offset"/> from <paramref name="origin"/>
        /// </summary>
        public static byte[] ReadBytes(this Stream stream, int offset, int count, SeekOrigin origin)
        {
            stream.Seek(offset, origin);
            return stream.ReadBytes(count);
        }

        public static byte[] ReadBytes(this Stream stream, int count)
        {
            var buffer = new byte[count];

            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read == 0)
                    throw new EndOfStreamException();

                offset += read;
            }

            return buffer;
        }

        public static byte[] ReadToEnd(this Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}