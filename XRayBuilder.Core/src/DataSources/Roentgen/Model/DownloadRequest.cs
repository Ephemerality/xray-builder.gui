namespace XRayBuilder.Core.DataSources.Roentgen.Model
{
    public sealed class DownloadRequest
    {
        public string Asin { get; set; }
        public TypeEnum Type { get; set; }
        public string Region { get; set; }

        public enum TypeEnum
        {
            Terms,
            EndActions,
            AuthorProfile
        }
    }
}