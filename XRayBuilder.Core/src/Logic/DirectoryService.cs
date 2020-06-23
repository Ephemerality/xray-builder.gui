using System;
using System.IO;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.Logic
{
    public sealed class DirectoryService : IDirectoryService
    {
        private readonly ILogger _logger;
        private readonly XRayBuilderConfig _config;

        public DirectoryService(ILogger logger, XRayBuilderConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public string GetArtifactFilename(ArtifactType artifactType, string asin, string databaseName, string guid)
            => artifactType switch
            {
                ArtifactType.XRay => _config.BuildForAndroid
                    ? $"XRAY.{asin}.{(databaseName == null ? "" : $"{databaseName}_")}{guid ?? ""}.db"
                    : $"XRAY.entities.{asin}.asc",
                ArtifactType.XRayPreview => $"XRAY.{asin}.previewData",
                ArtifactType.AuthorProfile => $"AuthorProfile.profile.{asin}.asc",
                ArtifactType.EndActions => $"EndActions.data.{asin}.asc",
                ArtifactType.StartActions => $"StartActions.data.{asin}.asc",
                _ => ""
            };

        // TODO Maybe just pass a whole IMetadata instead?
        public string GetArtifactPath(ArtifactType artifactType, string author, string title, string asin, string bookFilename, string databaseName, string guid, bool create)
            => Path.Combine(GetDirectory(author, title, asin, bookFilename, create), GetArtifactFilename(artifactType, asin, databaseName, guid));

        public string GetArtifactPath(ArtifactType artifactType, IMetadata metadata, string bookFilename, bool create)
            => Path.Combine(GetDirectory(metadata.Author, metadata.Title, metadata.Asin, bookFilename, create), GetArtifactFilename(artifactType, metadata.Asin, metadata.DbName, metadata.Guid));

        public string GetDirectory(string author, string title, string asin, string bookFilename, bool create)
        {
            var outputDir = "";

            if (_config.BuildForAndroid)
                outputDir = $@"{_config.BaseOutputDirectory}\Android\{asin}";
            else if (!_config.UseSubdirectories)
                outputDir = _config.BaseOutputDirectory;

            if (!Functions.ValidateFilename(author, title))
                _logger.Log("Warning: The author and/or title metadata fields contain invalid characters.\r\nThe book's output directory may not match what your Kindle is expecting.");

            if (string.IsNullOrEmpty(outputDir))
                outputDir = Functions.GetBookOutputDirectory(author, title, create, _config.BaseOutputDirectory);

            if (_config.OutputToSidecar)
                outputDir = Path.Combine(outputDir, $"{bookFilename}.sdr");

            if (!create)
                return outputDir;

            try
            {
                Directory.CreateDirectory(outputDir);
                return outputDir;
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred creating directory: {ex.Message}\r\nFiles will be placed in the default output directory.");
                return _config.BaseOutputDirectory;
            }
        }
    }

    public enum ArtifactType
    {
        XRay = 1,
        XRayPreview,
        AuthorProfile,
        EndActions,
        StartActions
    }
}