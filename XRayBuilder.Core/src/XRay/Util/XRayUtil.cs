using System.IO;
using System.Text;

namespace XRayBuilder.Core.XRay.Util
{
    public static class XRayUtil
    {
        public enum XRayVersion
        {
            Invalid = 0,
            Old,
            New
        }

        public static XRayVersion CheckXRayVersion(string path)
        {
            using var reader = new StreamReader(path, Encoding.UTF8);
            var buffer = new char[1];
            var result = reader.Read(buffer, 0, 1);
            if (result < 1)
                return XRayVersion.Invalid;

            return buffer[0] switch
            {
                'S' => XRayVersion.New,
                '{' => XRayVersion.Old,
                _ => XRayVersion.Invalid
            };
        }
    }
}
