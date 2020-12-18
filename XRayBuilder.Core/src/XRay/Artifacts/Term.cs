using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Artifacts
{
    public sealed class Term
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [XmlElement("name")]
        [JsonProperty("term")]
        public string TermName { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [XmlElement("src")]
        [JsonProperty("descSrc")]
        public string DescSrc { get; set; }

        [XmlElement("url")]
        [JsonProperty("descUrl")]
        public string DescUrl { get; set; }

        [JsonIgnore]
        public List<string> Aliases { get; set; } = new List<string>();

        [XmlIgnore]
        [JsonIgnore]
        public int Id { get; set; } = -1;

        [XmlIgnore]
        [JsonIgnore]
        public List<Occurrence> Occurrences { get; set; } = new();

        [JsonIgnore]
        public bool MatchCase { get; set; }

        [JsonIgnore]
        public bool Match { get; set; } = true;

        /// <summary>
        /// Determines if the aliases are in Regex format
        /// </summary>
        [JsonIgnore]
        public bool RegexAliases;
    }
}