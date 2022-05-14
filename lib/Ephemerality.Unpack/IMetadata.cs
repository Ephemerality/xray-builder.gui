using System;
using System.IO;
using Ephemerality.Unpack.Exceptions;
using SixLabors.ImageSharp;

namespace Ephemerality.Unpack
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
        /// <summary>
        /// Converted version of <see cref="UniqueId"/>
        /// </summary>
        string Guid { get; }
        string Isbn { get; }
        int ImageCount { get; }
        /// <summary>
        /// Indicates whether or not the metadata can be modified and saved
        /// </summary>
        bool CanModify { get; }

        /// <summary>
        /// For Mobi/AZW3, throws <see cref="EncryptedBookException"/> if DRM is enabled.
        /// KFX books will throw during load instead.
        /// </summary>
        void CheckDrm();
        /// <summary>
        /// If supported, returns the raw markup from the book
        /// </summary>
        byte[] GetRawMl();
        /// <summary>
        /// If supported, returns a stream containing the raw markup from <see cref="GetRawMl"/>
        /// </summary>
        Stream GetRawMlStream();
        /// <summary>
        /// If supported, saves the raw markup to a file
        /// </summary>
        void SaveRawMl(string path);

        // TODO These only apply to Mobi, shouldn't be forced for KFX
        void UpdateCdeContentType();
        void Save(Stream stream);
        void SetAsin(string asin);
        int? GetPageCount();

        /// <summary>
        /// Indicates whether or not the metadata type supports exporting its raw markup
        /// </summary>
        bool RawMlSupported { get; }
    }
}