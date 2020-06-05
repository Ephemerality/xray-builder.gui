using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.Logic
{
    public interface IDirectoryService
    {
        string GetDirectory(string author, string title, string asin, string bookFilename, bool create);
        string GetFilename(FileType fileType, string asin, string databaseName, string guid);
        string GetFilePath(FileType fileType, IMetadata metadata, string bookFilename, bool create);
        string GetFilePath(FileType fileType, string author, string title, string asin, string bookFilename, string databaseName, string guid, bool create);
    }
}