using Newtonsoft.Json;

namespace XRayBuilderGUI.XRay.Artifacts
{
    public sealed class PreviewData
    {
        [JsonProperty("numImages")]
        public int NumImages { get; set; }

        [JsonProperty("numTerms")]
        public int NumTerms { get; set; }

        [JsonProperty("previewImages")]
        public string PreviewImages { get; set; }

        [JsonProperty("excerptIds")]
        public string[] ExcerptIds { get; set; }

        [JsonProperty("numPeople")]
        public int NumPeople { get; set; }
    }
}