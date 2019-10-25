using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace XRayBuilderGUI.XRay
{
    public class Term
    {
        public string Type = "";

        [XmlElement("name")] public string TermName = "";

        public string Desc = "";

        [XmlElement("src")] public string DescSrc = "";

        [XmlElement("url")] public string DescUrl = "";

        [XmlIgnore] public List<string> Aliases = new List<string>();

        [JsonIgnore] [XmlIgnore] public List<string> Locs = new List<string>();

        [XmlIgnore] public List<string> Assets = new List<string> {""};

        [XmlIgnore] public int Id = -1;

        [XmlIgnore] public List<int[]> Occurrences = new List<int[]>();

        public bool MatchCase;

        public bool Match = true;

        /// <summary>
        /// Determines if the aliases are in Regex format
        /// </summary>
        public bool RegexAliases;

        public Term()
        {
        }

        public Term(string type)
        {
            Type = type;
        }

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