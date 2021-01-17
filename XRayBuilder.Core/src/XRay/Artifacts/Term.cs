using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Artifacts
{
    public sealed class Term : INotifyPropertyChanged
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
        public List<string> Aliases
        {
            get => _aliases;
            set
            {
                _aliases = value;
                OnPropertyChanged();
            }
        }

        private List<string> _aliases = new();

        [XmlIgnore]
        [JsonIgnore]
        public int Id { get; set; } = -1;

        [XmlIgnore]
        [JsonIgnore]
        public HashSet<Occurrence> Occurrences
        {
            get => _occurrences;
            set
            {
                _occurrences = value;
                OnPropertyChanged();
            }
        }
        private HashSet<Occurrence> _occurrences = new();

        [JsonIgnore]
        public bool MatchCase { get; set; }

        [JsonIgnore]
        public bool Match { get; set; } = true;

        /// <summary>
        /// Determines if the aliases are in Regex format
        /// </summary>
        [JsonIgnore]
        public bool RegexAliases { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}