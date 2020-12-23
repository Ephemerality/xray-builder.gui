using System.Collections.Generic;
using System.Linq;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    public static class ExcerptHelper
    {
        public static void EnhanceOrAddExcerpts(List<Excerpt> excerpts, int characterId, IndexLength excerptLocation)
        {
            var exCheck = excerpts.Where(t => t.Start.Equals(excerptLocation.Index)).ToArray();
            if (exCheck.Length > 0)
            {
                if (!exCheck[0].RelatedEntities.Contains(characterId))
                    exCheck[0].RelatedEntities.Add(characterId);
            }
            else
            {
                var newExcerpt = new Excerpt
                {
                    Id = excerpts.Max(excerpt => excerpt.Id) + 1,
                    Start = excerptLocation.Index,
                    Length = excerptLocation.Length
                };
                newExcerpt.RelatedEntities.Add(characterId);
                excerpts.Add(newExcerpt);
            }
        }
    }
}