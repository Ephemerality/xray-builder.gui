using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Unpack.KFX;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    /// <summary>
    /// Handles adding locations and highlights to an X-Ray.
    /// Supports UTF8 instead of the 1252 encoding
    /// </summary>
    public sealed class KfxXrayService : IKfxXrayService
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

            // Set start and end of content
            // TODO Figure out how to identify the first *actual* bit of content after the TOC
            var last = contentChunks.Last();
            xray.Srl = 1;
            xray.Erl = last.Pid + last.Length - 1;

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
                        // If the aliases are not supposed to be in regex format, escape them
                        var aliases = character.RegexAliases
                            ? character.Aliases
                            : character.Aliases.Select(Regex.Escape);

                        var searchList = new[] {character.TermName}.Concat(aliases).ToArray();

                        //Search content for character name and aliases, respecting the case setting
                        var regexOptions = character.MatchCase || character.RegexAliases
                            ? RegexOptions.None
                            : RegexOptions.IgnoreCase;

                        var currentOffset = offset;
                        var highlights = searchList
                            .Select(search => Regex.Matches(contentChunk.ContentText, $@"{Quotes}?\b{search}{_punctuationMarks}", regexOptions))
                            .SelectMany(matches => matches.Cast<Match>())
                            .ToLookup(match => currentOffset + match.Index, match => match.Length);

                        if (highlights.Count == 0)
                            continue;

                        var highlightOccurrences = highlights.SelectMany(highlightGroup => highlightGroup.Select(highlight => new[] {highlightGroup.Key, highlight}));
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

            var missingOccurrences = xray.Terms
                .Where(term => term.Match && term.Occurrences.Count == 0)
                .Select(term => term.TermName)
                .ToArray();

            if (!missingOccurrences.Any())
                return;

            var termList = string.Join(", ", missingOccurrences);
            _logger.Log($"\r\nNo locations were found for the following terms. You should add aliases for them using the book as a reference:\r\n{termList}\r\n");
        }
    }
}