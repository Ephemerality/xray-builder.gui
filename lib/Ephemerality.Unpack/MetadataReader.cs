using System;
using System.IO;
using Ephemerality.Unpack.KFX;
using Ephemerality.Unpack.Mobi;

namespace Ephemerality.Unpack
{
    public static class MetadataReader
    {
        public static IMetadata Load(string file)
        {
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);

            IMetadata metadata;
            switch (Path.GetExtension(file))
            {
                case ".azw3":
                case ".mobi":
                    metadata = new MobiMetadata(fs);
                    break;
                case ".kfx":
                    metadata = new KfxContainer(fs);
                    break;
                default:
                    throw new NotSupportedException("Unsupported book format");
            }

            return metadata;
        }
    }
}
