namespace Ephemerality.Unpack.KFX
{
    public sealed class KfxContainerInfo
    {
        public KfxHeader Header { get; set; }
        public string ContainerId { get; set; }
        public int ChunkSize { get; set; }
        public int CompressionType { get; set; }
        public int DrmScheme { get; set; }
        public string KfxGenApplicationVersion { get; set; }
        public string KfxGenPackageVersion { get; set; }
        public YjContainer.ContainerFormat ContainerFormat { get; set; }
    }
}