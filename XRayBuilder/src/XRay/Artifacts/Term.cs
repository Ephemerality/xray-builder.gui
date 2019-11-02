using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace XRayBuilderGUI.XRay.Artifacts
{
    public class Term
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

        [XmlIgnore]
        [JsonIgnore]
        public List<string> Aliases { get; set; } = new List<string>();

        [XmlIgnore]
        [JsonProperty("locs")]
        // todo int[][]
        public List<string> Locs { get; set; } = new List<string>();

        [XmlIgnore]
        [JsonIgnore]
        public List<string> Assets { get; set; } = new List<string>();

        [XmlIgnore]
        [JsonIgnore]
        public int Id { get; set; } = -1;

        [XmlIgnore]
        [JsonIgnore]
        public List<int[]> Occurrences { get; set; } = new List<int[]>();

        [JsonIgnore]
        public bool MatchCase { get; set; }

        [JsonIgnore]
        public bool Match { get; set; } = true;

        /// <summary>
        /// Determines if the aliases are in Regex format
        /// </summary>
        [JsonIgnore]
        public bool RegexAliases;

        public override string ToString()
        {
            //Note that the Amazon X-Ray files declare an "assets" var for each term, but I have not seen one that actually uses them to contain anything
            if (Locs.Count > 0)
                return
                    string.Format(
                        @"{{""type"":""{0}"",""term"":""{1}"",""desc"":""{2}"",""descSrc"":""{3}"",""descUrl"":""{4}"",""locs"":[{5}]}}",
                        Type, TermName, Desc, DescSrc, DescUrl, string.Join(",", Locs));

            return
                string.Format(
                    @"{{""type"":""{0}"",""term"":""{1}"",""desc"":""{2}"",""descSrc"":""{3}"",""descUrl"":""{4}"",""locs"":[[100,100,100,6]]}}",
                    Type, TermName, Desc, DescSrc, DescUrl);
        }
    }
}