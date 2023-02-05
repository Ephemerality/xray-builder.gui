using Newtonsoft.Json;

namespace XRayBuilder.Core.DataSources.Secondary.Model
{
    public sealed class GoodreadsCharacter
    {
        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }
    }
}