using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Unpack.KFX;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    /// <summary>
    /// Handles adding locations and highlights to an X-Ray.
    /// Supports UTF8 instead of the 1252 encoding
    /// </summary>
    public class KfxXrayService
    {
        private readonly ILogger _logger;

        private const string Apostrophes = "('|\u2019|\u0060|\u00B4)";
        private const string Quotes = "(\"|\u2018|\u2019|\u201A|\u201B|\u201C|\u201D|\u201E|\u201F)";
        private const string DashesEllipsis = "(-|\u2010|\u2011|\u2012|\u2013|\u2014|\u2015|\u2026|&#8211;|&#8212;|&#8217;|&#8218;|&#8230;)";
        private readonly string _punctuationMarks = string.Format(@"({0}s|{0})?{1}?[!\.?,""\);:]*{0}*{1}*{2}*", Apostrophes, Quotes, DashesEllipsis);

        public KfxXrayService(ILogger logger)
        {
            _logger = logger;
        }

        public void AddLocations(XRay xray,
            KfxContainer kfx,
            bool skipNoLikes,
            int minClipLen,
            IProgressBar progress,
            CancellationToken token)
        {
            _logger.Log("Scanning book content...");
            var contentChunks = kfx.GetContentChunks();
            var offset = 0;
            var excerptId = 0;
            progress?.Set(0, contentChunks.Count);
            foreach (var contentChunk in contentChunks)
            {
                token.ThrowIfCancellationRequested();

                if (contentChunk.ContentText != null)
                {
                    foreach (var character in xray.Terms.Where(term => term.Match))
                    {
                        // If the aliases are regex-based, search for those first
                        if (character.RegexAliases && !character.Aliases.Any(pattern => Regex.IsMatch(contentChunk.ContentText, pattern)))
                            continue;

                        // Search for the term's name and its aliases in the chunk, respecting the case setting
                        var searchList = new[] {character.TermName}.Concat(character.Aliases).ToArray();
                        if ((!character.MatchCase || !searchList.Any(contentChunk.ContentText.Contains))
                            && (character.MatchCase || !searchList.Any(contentChunk.ContentText.ContainsIgnorecase)))
                        {
                            continue;
                        }

                        //Search content for character name and aliases
                        var regexOptions = character.MatchCase || character.RegexAliases
                            ? RegexOptions.None
                            : RegexOptions.IgnoreCase;
                        var currentOffset = offset;
                        var highlights = searchList
                            .Select(search => Regex.Matches(contentChunk.ContentText, $@"{Quotes}?\b{search}{_punctuationMarks}", regexOptions))
                            .SelectMany(matches => matches.Cast<Match>())
                            .ToLookup(match => currentOffset + match.Index, match => match.Length);

                        if (highlights.Count == 0)
                        {
                            _logger.Log($"An error occurred while searching for start of highlight.\r\nWas looking for (or one of the aliases of): {character.TermName}\r\nSearching in: {contentChunk.ContentText}");
                            continue;
                        }

                        var highlightOccurrences = highlights.SelectMany(highlightGroup => highlightGroup.Select(highlight => new[] {highlightGroup.Key, highlight}));
                        if (highlightOccurrences.Any(asdasd => asdasd.Contains(507347)))
                            break;
                        character.Occurrences.AddRange(highlightOccurrences);

                        // Check excerpts
                        var exCheck = xray.Excerpts.Where(t => t.Start.Equals(offset)).ToArray();
                        if (exCheck.Length > 0)
                        {
                            if (!exCheck[0].RelatedEntities.Contains(character.Id))
                                exCheck[0].RelatedEntities.Add(character.Id);
                        }
                        else
                        {
                            var newExcerpt = new Excerpt
                            {
                                Id = excerptId++,
                                Start = offset,
                                Length = contentChunk.Length
                            };
                            newExcerpt.RelatedEntities.Add(character.Id);
                            xray.Excerpts.Add(newExcerpt);
                        }
                    }

                    // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                    if (xray.NotableClips != null)
                    {
                        foreach (var quote in xray.NotableClips)
                        {
                            var index = contentChunk.ContentText.IndexOf(quote.Text, StringComparison.Ordinal);
                            if (index <= -1)
                                continue;

                            // See if an excerpt already exists at this location
                            var excerpt = xray.Excerpts.FirstOrDefault(e => e.Start == index);
                            if (excerpt == null)
                            {
                                if (skipNoLikes && quote.Likes == 0
                                    || quote.Text.Length < minClipLen)
                                    continue;
                                excerpt = new Excerpt
                                {
                                    Id = excerptId++,
                                    Start = offset,
                                    Length = contentChunk.Length,
                                    Notable = true,
                                    Highlights = quote.Likes
                                };
                                excerpt.RelatedEntities.Add(0); // Mark the excerpt as notable
                                // TODO: also add other related entities
                                xray.Excerpts.Add(excerpt);
                            }
                            else
                                excerpt.RelatedEntities.Add(0);

                            xray.FoundNotables++;
                        }
                    }

                    progress?.Add(1);
                }

                offset += contentChunk.Length;
            }

            foreach (var term in xray.Terms.Where(t => t.Match && t.Locs.Count == 0))
            {
                _logger.Log($"No locations were found for the term \"{term.TermName}\".\r\nYou should add aliases for this term using the book or rawml as a reference.");
            }
        }
    }
}