namespace XRayBuilder.Core.DataSources.Roentgen.Model
{
    public sealed class DownloadRequest
    {
        public string Asin { get; set; }
        public TypeEnum Type { get; set; }
        public string RegionTld { get; set; }

        public enum TypeEnum
        {
            Terms,
            EndActions,
            AuthorProfile
        }
    }
}