// This file contains code ported over from KindleUnpack (http://www.mobileread.com/forums/showthread.php?t=61986)

using System.Collections.Generic;

namespace XRayBuilder.Core.Unpack.Mobi.Decompress
{
    public interface IDecompressor
    {
        /// <summary>
        /// Initialize the decompressor using any necessary values from the <paramref name="mobiHeader"/>, <paramref name="pdbHeader"/>, and <paramref name="headerRecords"/>
        /// </summary>
        void Initialize(MobiHead mobiHeader, PdbHeader pdbHeader, List<byte[]> headerRecords);
        byte[] Unpack(byte[] data);
    }

    public abstract class Decompressor : IDecompressor
    {
        public virtual void Initialize(MobiHead mobiHeader, PdbHeader pdbHeader, List<byte[]> headerRecords)
        {
        }

        public abstract byte[] Unpack(byte[] data);
    }
}
