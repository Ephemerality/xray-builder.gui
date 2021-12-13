using System;
using System.Collections.Generic;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay
{
    public sealed class XRay
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string DataUrl { get; set; }
        public string DatabaseName { get; set; }
        public string Guid { get; set; }
        public string Asin { get; set; }
        public List<Term> Terms { get; set; } = new List<Term>(100);
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public List<Excerpt> Excerpts { get; } = new List<Excerpt>();
        public long Srl { get; set; }
        public long Erl { get; set; }
        public bool Unattended { get; set; }
        public List<NotableClip> NotableClips { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
