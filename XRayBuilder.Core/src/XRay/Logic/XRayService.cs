using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    [UsedImplicitly]
    public sealed class XRayService : IXRayService
    {
        private readonly ILogger _logger;
        private readonly ChaptersService _chaptersService;
        private readonly IAliasesRepository _aliasesRepository;

        public XRayService(ILogger logger, ChaptersService chaptersService, IAliasesRepository aliasesRepository)
        {
            _logger = logger;
            _chaptersService = chaptersService;
            _aliasesRepository = aliasesRepository;
        }

        public async Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            int locOffset,
            ISecondarySource dataSource,
            IProgressBar progress,
            CancellationToken token = default)
        {
            var xray = new XRay(dataLocation, db, guid, asin, dataSource, locOffset)
            {
                Terms = (await dataSource.GetTermsAsync(dataLocation, progress, token)).ToList()
            };
            if (dataSource.SupportsNotableClips)
            {
                _logger.Log("Downloading notable clips...");
                xray.NotableClips = (await dataSource.GetNotableClipsAsync(dataLocation, null, progress, token))?.ToList();
            }
            if (xray.Terms.Count == 0)
            {
                _logger.Log("Warning: No terms found on " + dataSource.Name + ".");
            }

            return xray;
        }

        // TODO Remove path from here when directory service is done
        public void ExportAndDisplayTerms(XRay xray, string path, bool overwriteAliases, bool splitAliases)
        {
            //Export available terms to a file to make it easier to create aliases or import the modified aliases if they exist
            //Could potentially just attempt to automate the creation of aliases, but in some cases it is very subjective...
            //For example, Shelfari shows the character "Artemis Fowl II", but in the book he is either referred to as "Artemis Fowl", "Artemis", or even "Arty"
            //Other characters have one name on Shelfari but can have completely different names within the book
            var aliasesDownloaded = false;
            // TODO: Review this download process
            //if ((!File.Exists(AliasPath) || Properties.Settings.Default.overwriteAliases) && Properties.Settings.Default.downloadAliases)
            //{
            //    aliasesDownloaded = await AttemptAliasDownload();
            //}

            if (!aliasesDownloaded && (!File.Exists(path) || overwriteAliases))
            {
                _aliasesRepository.SaveCharactersToFile(xray.Terms, xray.Asin, splitAliases);
                _logger.Log($"Characters exported to {path} for adding aliases.");
            }

            if (xray.SkipShelfari)
                _logger.Log(string.Format("{0} {1} found in file:", xray.Terms.Count, xray.Terms.Count > 1 ? "Terms" : "Term"));
            else
                _logger.Log(string.Format("{0} {1} found on {2}:", xray.Terms.Count, xray.Terms.Count > 1 ? "Terms" : "Term", xray.DataSource.Name));
            var str = new StringBuilder(xray.Terms.Count * 32); // Assume that most names will be less than 32 chars
            var termId = 1;
            foreach (var t in xray.Terms)
            {
                str.Append(t.TermName).Append(", ");
                // todo don't set the IDs here...
                t.Id = termId++;
            }
            _logger.Log(str.ToString());
        }

        // TODO split this up, possible return a result instead of modifying xray
        public void ExpandFromRawMl(
            XRay xray,
            Stream rawMlStream,
            bool enableEdit,
            bool useNewVersion,
            bool skipNoLikes,
            int minClipLen,
            bool overwriteChapters,
            SafeShowDelegate safeShow,
            IProgressBar progress,
            CancellationToken token,
            bool ignoreSoftHypen = false,
            bool shortEx = true)
        {
            // If there is an apostrophe, attempt to match 's at the end of the term
            // Match end of word, then search for any lingering punctuation
            var apostrophes = Encoding.Default.GetString(Encoding.UTF8.GetBytes("('|\u2019|\u0060|\u00B4)")); // '\u2019\u0060\u00B4
            var quotes = Encoding.Default.GetString(Encoding.UTF8.GetBytes("(\"|\u2018|\u2019|\u201A|\u201B|\u201C|\u201D|\u201E|\u201F)"));
            var dashesEllipsis = Encoding.Default.GetString(Encoding.UTF8.GetBytes("(-|\u2010|\u2011|\u2012|\u2013|\u2014|\u2015|\u2026|&#8211;|&#8212;|&#8217;|&#8218;|&#8230;)")); //U+2010 to U+2015 and U+2026
            var punctuationMarks = string.Format(@"({0}s|{0})?{1}?[!\.?,""\);:]*{0}*{1}*{2}*", apostrophes, quotes, dashesEllipsis);

            var excerptId = 0;
            var web = new HtmlDocument();
            web.Load(rawMlStream, Encoding.Default);

            rawMlStream.Seek(0, SeekOrigin.Begin);
            // TODO: passing stream, doc, and contents probably not necessary)
            using (var streamReader = new StreamReader(rawMlStream, Encoding.UTF8))
            {
                var readContents = streamReader.ReadToEnd();
                var utf8Doc = new HtmlDocument();
                utf8Doc.LoadHtml(readContents);
                _chaptersService.HandleChapters(xray, xray.Asin, rawMlStream.Length, utf8Doc, readContents, overwriteChapters, safeShow, xray.Unattended, enableEdit);
            }

            _logger.Log("Scanning book content...");
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            //Iterate over all paragraphs in book
            var nodes = web.DocumentNode.SelectNodes("//p")
                ?? web.DocumentNode.SelectNodes("//div[@class='paragraph']")
                ?? web.DocumentNode.SelectNodes("//div[@class='p-indent']");
            if (nodes == null)
            {
                nodes = web.DocumentNode.SelectNodes("//div");
                _logger.Log("Warning: Could not locate paragraphs normally (p elements or divs of class 'paragraph').\r\n" +
                    "Searching all book contents (all divs), which may produce odd results.");
            }
            if (nodes == null)
                throw new Exception("Could not locate any paragraphs in this book.\r\n" +
                    "Report this error along with a copy of the book to improve parsing.");
            progress?.Set(0, nodes.Count);
            for (var i = 0; i < nodes.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var node = nodes[i];
                if (node.FirstChild == null) continue; //If the inner HTML is just empty, skip the paragraph!
                var lenQuote = node.InnerHtml.Length;
                var location = node.FirstChild.StreamPosition;
                if (location < 0)
                    throw new Exception($"Unable to locate paragraph {i} within the book content.");

                //Skip paragraph if outside chapter range
                if (location < xray.Srl || location > xray.Erl)
                    continue;
                var noSoftHypen = "";
                if (ignoreSoftHypen)
                {
                    noSoftHypen = node.InnerText;
                    noSoftHypen = noSoftHypen.Replace("\u00C2\u00AD", "");
                    noSoftHypen = noSoftHypen.Replace("&shy;", "");
                    noSoftHypen = noSoftHypen.Replace("&#xad;", "");
                    noSoftHypen = noSoftHypen.Replace("&#173;", "");
                    noSoftHypen = noSoftHypen.Replace("&#0173;", "");
                }
                foreach (var character in xray.Terms)
                {
                    //Search for character name and aliases in the html-less text. If failed, try in the HTML for rare situations.
                    //TODO: Improve location searching as IndexOf will not work if book length exceeds 2,147,483,647...
                    //If soft hyphen ignoring is turned on, also search hyphen-less text.
                    if (!character.Match)
                        continue;
                    var termFound = false;
                    // Convert from UTF8 string to default-encoded representation
                    var search = character.Aliases.Select(alias => Encoding.Default.GetString(Encoding.UTF8.GetBytes(alias)))
                        .ToList();
                    if (character.RegexAliases)
                    {
                        if (search.Any(r => Regex.Match(node.InnerText, r).Success)
                            || search.Any(r => Regex.Match(node.InnerHtml, r).Success)
                            || (ignoreSoftHypen && search.Any(r => Regex.Match(noSoftHypen, r).Success)))
                            termFound = true;
                    }
                    else
                    {
                        // Search for character name and aliases
                        // If there is an apostrophe, attempt to match 's at the end of the term
                        // Match end of word, then search for any lingering punctuation
                        search.Insert(0, character.TermName);
                        if ((character.MatchCase && (search.Any(node.InnerText.Contains) || search.Any(node.InnerHtml.Contains)))
                            || (!character.MatchCase && (search.Any(node.InnerText.ContainsIgnorecase) || search.Any(node.InnerHtml.ContainsIgnorecase)))
                                || (ignoreSoftHypen && (character.MatchCase && search.Any(noSoftHypen.Contains))
                                    || (!character.MatchCase && search.Any(noSoftHypen.ContainsIgnorecase))))
                            termFound = true;
                    }

                    if (!termFound)
                        continue;

                    var locHighlight = new List<int>();
                    var lenHighlight = new List<int>();
                    //Search html for character name and aliases
                    foreach (var s in search)
                    {
                        var matches = Regex.Matches(node.InnerHtml, $@"{quotes}?\b{s}{punctuationMarks}", character.MatchCase || character.RegexAliases ? RegexOptions.None : RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                        {
                            if (locHighlight.Contains(match.Index) && lenHighlight.Contains(match.Length))
                                continue;
                            locHighlight.Add(match.Index);
                            lenHighlight.Add(match.Length);
                        }
                    }
                    //If normal search fails, use regexp to search in case there is some wacky html nested in term
                    //Regexp may be less than ideal for parsing HTML but seems to work ok so far in these small paragraphs
                    //Also search in soft hyphen-less text if option is set to do so
                    if (locHighlight.Count == 0)
                    {
                        foreach (var s in search)
                        {
                            var patterns = new List<string>();
                            const string patternHtml = "(?:<[^>]*>)*";
                            //Match HTML tags -- provided there's nothing malformed
                            const string patternSoftHypen = "(\u00C2\u00AD|&shy;|&#173;|&#xad;|&#0173;|&#x00AD;)*";
                            var pattern = string.Format("{0}{1}{0}{2}",
                                patternHtml,
                                string.Join(patternHtml + patternSoftHypen, character.RegexAliases ? s.ToCharArray() : Regex.Unescape(s).ToCharArray()),
                                punctuationMarks);
                            patterns.Add(pattern);
                            foreach (var pat in patterns)
                            {
                                MatchCollection matches;
                                if (character.MatchCase || character.RegexAliases)
                                    matches = Regex.Matches(node.InnerHtml, pat);
                                else
                                    matches = Regex.Matches(node.InnerHtml, pat, RegexOptions.IgnoreCase);
                                foreach (Match match in matches)
                                {
                                    if (locHighlight.Contains(match.Index) && lenHighlight.Contains(match.Length))
                                        continue;
                                    locHighlight.Add(match.Index);
                                    lenHighlight.Add(match.Length);
                                }
                            }
                        }
                    }
                    if (locHighlight.Count == 0 || locHighlight.Count != lenHighlight.Count) //something went wrong
                    {
                        _logger.Log($"An error occurred while searching for start of highlight.\r\nWas looking for (or one of the aliases of): {character.TermName}\r\nSearching in: {node.InnerHtml}");
                        continue;
                    }

                    //If an excerpt is too long, the X-Ray reader cuts it off.
                    //If the location of the highlighted word (character name) within the excerpt is far enough in to get cut off,
                    //this section attempts to shorted the excerpt by locating the start of a sentence that is just far enough away from the highlight.
                    //The length is determined by the space the excerpt takes up rather than its actual length... so 135 is just a guess based on what I've seen.
                    var lengthLimit = 135;
                    for (var j = 0; j < locHighlight.Count; j++)
                    {
                        if (shortEx && locHighlight[j] + lenHighlight[j] > lengthLimit)
                        {
                            var start = locHighlight[j];
                            var at = 0;
                            long newLoc = -1;
                            var newLenQuote = 0;
                            var newLocHighlight = 0;

                            while ((start > -1) && (at > -1))
                            {
                                at = node.InnerHtml.LastIndexOfAny(new[] { '.', '?', '!' }, start);
                                if (at > -1)
                                {
                                    start = at - 1;
                                    if ((locHighlight[j] + lenHighlight[j] + 1 - at - 2) <= lengthLimit)
                                    {
                                        newLoc = location + at + 2;
                                        newLenQuote = lenQuote - at - 2;
                                        newLocHighlight = locHighlight[j] - at - 2;
                                        //string newQuote = node.InnerHtml.Substring(at + 2);
                                    }
                                    else break;
                                }
                                else break;
                            }
                            //Only add new locs if shorter excerpt was found
                            if (newLoc >= 0)
                            {
                                character.Locs.Add(new []
                                {
                                    newLoc + xray.LocOffset,
                                    newLenQuote,
                                    newLocHighlight,
                                    lenHighlight[j]
                                });
                                locHighlight.RemoveAt(j);
                                lenHighlight.RemoveAt(j--);
                            }
                        }
                    }

                    for (var j = 0; j < locHighlight.Count; j++)
                    {
                        // For old format
                        character.Locs.Add(new long[]
                        {
                            location + xray.LocOffset,
                            lenQuote,
                            locHighlight[j],
                            lenHighlight[j]
                        });
                        // For new format
                        character.Occurrences.Add(new[] { location + xray.LocOffset + locHighlight[j], lenHighlight[j] });
                    }
                    var exCheck = xray.Excerpts.Where(t => t.Start.Equals(location + xray.LocOffset)).ToArray();
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
                            Start = location + xray.LocOffset,
                            Length = lenQuote
                        };
                        newExcerpt.RelatedEntities.Add(character.Id);
                        xray.Excerpts.Add(newExcerpt);
                    }
                }

                // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                if (useNewVersion && xray.NotableClips != null)
                {
                    foreach (var quote in xray.NotableClips)
                    {
                        var index = node.InnerText.IndexOf(quote.Text, StringComparison.Ordinal);
                        if (index > -1)
                        {
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
                                    Start = location,
                                    Length = node.InnerHtml.Length,
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
                }
                progress?.Add(1);
            }

            timer.Stop();
            _logger.Log($"Scan time: {timer.Elapsed}");
            //output list of terms with no locs
            foreach (var t in xray.Terms.Where(t => t.Match && t.Locs.Count == 0))
            {
                _logger.Log($"No locations were found for the term \"{t.TermName}\".\r\nYou should add aliases for this term using the book or rawml as a reference.");
            }
        }

        // TODO: Redo this later maybe
        //public async Task<bool> AttemptAliasDownload()
        //{
        //    try
        //    {
        //        string aliases = await HttpDownloader.GetPageHtmlAsync("https://www.revensoftware.com/xray/aliases/" + asin);
        //        StreamWriter fs = new StreamWriter(AliasPath, false, Encoding.UTF8);
        //        fs.Write(aliases);
        //        fs.Close();
        //        _logger.Log("Found and downloaded pre-made aliases file.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!ex.Message.Contains("(404) Not Found"))
        //            _logger.Log("No pre-made aliases available for this book.");
        //        else
        //            _logger.Log("An error occurred downloading aliases: " + ex.Message + "\r\n" + ex.StackTrace);
        //    }

        //    return false;
        //}
    }
}