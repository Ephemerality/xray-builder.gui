using System.Collections.Generic;
using Newtonsoft.Json;

namespace XRayBuilderGUI.DataSources.Model
{
    public class NextBookResult
    {
        [JsonProperty("error")]
        public ErrorPayload Error { get; set; }

        [JsonProperty("nextBook")]
        public Book NextBook { get; set; }
        public class ErrorPayload
        {

            [JsonProperty("errorCode")]
            public string ErrorCode { get; set; }

            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }
        }

        public class Author
        {
            [JsonProperty("authorName")]
            public string AuthorName { get; set; }

            [JsonProperty("languageTag")]
            public string LanguageTag { get; set; }
        }

        public class Title
        {
            [JsonProperty("languageTag")]
            public string LanguageTag { get; set; }

            [JsonProperty("titleName")]
            public string TitleName { get; set; }
        }

        public class Book
        {
            [JsonProperty("asin")]
            public string Asin { get; set; }

            [JsonProperty("authors")]
            public IList<Author> Authors { get; set; }

            [JsonProperty("showAtPercentRead")]
            public double ShowAtPercentRead { get; set; }

            [JsonProperty("title")]
            public Title Title { get; set; }
        }
    }
}
