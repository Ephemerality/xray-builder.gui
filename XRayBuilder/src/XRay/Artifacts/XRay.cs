using Newtonsoft.Json;

namespace XRayBuilderGUI.XRay.Artifacts
{
    public sealed class XRay
    {
        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("xrayversion")]
        public string XRayVersion { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("terms")]
        public Term[] Terms { get; set; }

        [JsonProperty("chapters")]
        public Chapter[] Chapters { get; set; }

        [JsonProperty("assets")]
        public Assets Assets { get; set; } = new Assets();

        [JsonProperty("srl")]
        public long Start { get; set; }

        [JsonProperty("erl")]
        public long End { get; set; }
    }

    public sealed class Assets
    {
    }
}