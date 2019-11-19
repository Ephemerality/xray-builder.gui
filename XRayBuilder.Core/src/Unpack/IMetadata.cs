using System;
using System.Drawing;
using System.IO;

namespace XRayBuilder.Core.Unpack
{
    public interface IMetadata : IDisposable
    {
        bool IsAzw3 { get; }
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
        void UpdateCdeContentType();
        void Save(Stream stream);

        // Settings (should be moved)
        bool RawMlSupported { get; }
    }
}