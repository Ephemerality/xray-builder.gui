using Newtonsoft.Json;

namespace XRayBuilder.Core.Extras.Artifacts
{
    public sealed class AuthorProfile
    {
        [JsonProperty("u")]
        public Author[] Authors { get; set; }

        [JsonProperty("a")]
        public string Asin { get; set; }

        [JsonProperty("d")]
        public long CreationDate { get; set; }

        [JsonProperty("o")]
        public BookData[] OtherBooks { get; set; }

        public sealed class Author
        {
            [JsonProperty("y")]
            public int ImageHeight { get; set; }

            [JsonProperty("l")]
            public string[] OtherBookAsins { get; set; }

            [JsonProperty("n")]
            public string Name { get; set; }

            [JsonProperty("a")]
            public string Asin { get; set; }

            [JsonProperty("b")]
            public string Bio { get; set; }

            [JsonProperty("i")]
            public string Picture { get; set; }
        }

        public sealed class BookData
        {
            [JsonProperty("e")]
            public int E { get; set; }

            [JsonProperty("a")]
            public string Asin { get; set; }

            [JsonProperty("t")]
            public string Title { get; set; }
        }
    }
}
