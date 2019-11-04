using System.Collections.Generic;

namespace XRayBuilder.Core.XRay.Model
{
    public sealed class Excerpt
    {
        public int Id { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string Image { get; set; } = "";
        public readonly List<int> RelatedEntities = new List<int>();
        //public int go_to = -1; unused but in the db
        public int Highlights { get; set; }
        public bool Notable { get; set; }
    }
}