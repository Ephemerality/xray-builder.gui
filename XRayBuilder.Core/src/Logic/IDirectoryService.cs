using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.Logic
{
    public interface IDirectoryService
    {
        string GetAliasPath(string asin);
        string GetRawmlPath(string filePath);
        string GetDirectory(string author, string title, string asin, string bookFilename, bool create);
        string GetArtifactFilename(ArtifactType artifactType, string asin, string databaseName, string guid);
        string GetArtifactPath(ArtifactType artifactType, IMetadata metadata, string bookFilename, bool create);
        string GetArtifactPath(ArtifactType artifactType, string author, string title, string asin, string bookFilename, string databaseName, string guid, bool create);
    }
}