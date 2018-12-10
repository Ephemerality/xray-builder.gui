using System;
using System.IO;
using XRayBuilderGUI.Unpack.Mobi;

namespace XRayBuilderGUI.Unpack
{
    public static class MetadataLoader
    {
        public static IMetadata Load(string file)
        {
            var fs = new FileStream(file, FileMode.Open, FileAccess.Read);

            IMetadata metadata = null;
            switch (Path.GetExtension(file))
            {
                case ".azw3":
                case ".mobi":
                    metadata = new Metadata(fs);
                    break;
                case ".kfx":
                    break;
                default:
                    throw new NotSupportedException("Unsupported book format");
            }

            return metadata;
        }
    }
}
