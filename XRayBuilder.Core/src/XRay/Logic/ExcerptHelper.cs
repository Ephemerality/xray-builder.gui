using System;
using System.Collections.Generic;
using System.Linq;
using XRayBuilder.Core.DataSources.Secondary.Model;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    public static class ExcerptHelper
    {
        // todo just take paragraph instead of indexlength
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
                    Id = excerpts.Any()
                        ? excerpts.Max(excerpt => excerpt.Id) + 1
                        : 1,
                    Start = excerptLocation.Index,
                    Length = excerptLocation.Length
                };
                newExcerpt.RelatedEntities.Add(characterId);
                excerpts.Add(newExcerpt);
            }
        }

        public static void ProcessNotablesForParagraph(string paragraph, int offset, IEnumerable<NotableClip> notableClips, List<Excerpt> excerpts, bool skipNoLikes, int minClipLength)
        {
            foreach (var quote in notableClips)
            {
                var index = paragraph.IndexOf(quote.Text, StringComparison.Ordinal);
                if (index <= -1)
                    continue;

                // See if an excerpt already exists at this location
                var excerpt = excerpts.FirstOrDefault(e => e.Start == index);
                if (excerpt == null)
                {
                    if (skipNoLikes && quote.Likes == 0 || quote.Text.Length < minClipLength)
                        continue;
                    excerpt = new Excerpt
                    {
                        Id = excerpts.Max(e => e.Id) + 1,
                        Start = offset + index,
                        Length = paragraph.Length,
                        Notable = true,
                        Highlights = quote.Likes
                    };
                    excerpt.RelatedEntities.Add(0); // Mark the excerpt as notable
                    // TODO: also add other related entities
                    excerpts.Add(excerpt);
                }
                else
                {
                    excerpt.Notable = true;
                    excerpt.RelatedEntities.Add(0);
                }
            }
        }
    }
}