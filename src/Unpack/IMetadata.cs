using System.Drawing;
using System.IO;

namespace XRayBuilderGUI.Unpack
{
    public interface IMetadata
    {
        string Asin { get; }
        string Author { get; }
        string CdeContentType { get; }
        Image CoverImage { get; }
        string DbName { get; }
        long RawMlSize { get; }
        string Title { get; }
        string UniqueId { get; }

        void CheckDrm();
        byte[] GetRawMl();
        Stream GetRawMlStream();
        void SaveRawMl(string path);
        void UpdateCdeContentType(FileStream fs);
    }
}