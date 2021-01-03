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
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Localization.Core;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    [UsedImplicitly]
    public sealed class XRayService : IXRayService
    {
        private readonly ILogger _logger;
        private readonly ChaptersService _chaptersService;
        private readonly IAliasesRepository _aliasesRepository;
        private readonly Encoding _encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);
        private readonly IDirectoryService _directoryService;
        private readonly ITermsService _termsService;

        public XRayService(ILogger logger, ChaptersService chaptersService, IAliasesRepository aliasesRepository, IDirectoryService directoryService, ITermsService termsService)
        {
            _logger = logger;
            _chaptersService = chaptersService;
            _aliasesRepository = aliasesRepository;
            _directoryService = directoryService;
            _termsService = termsService;
        }

        public async Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            string tld,
            bool includeTopics,
            ISecondarySource dataSource,
            IProgressBar progress,
            CancellationToken token = default)
        {
            if (dataLocation == "" && !(dataSource is SecondarySourceRoentgen) || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");

            dataLocation = dataSource.SanitizeDataLocation(dataLocation);

            var terms = (await dataSource.GetTermsAsync(dataLocation, asin, tld, includeTopics, progress, token)).ToList();

            var xray = new XRay
            {
                DatabaseName = string.IsNullOrEmpty(db) ? null : db,
                Guid = Functions.ConvertGuid(guid),
                Asin = asin,
                DataUrl = dataLocation,
                Terms = terms
            };


            if (dataSource.SupportsNotableClips)
            {
                _logger.Log("Downloading notable clips...");
                xray.NotableClips = (await dataSource.GetNotableClipsAsync(dataLocation, null, progress, token))?.ToList();
            }

            if (xray.Terms.Count == 0)
            {
                _logger.Log($"Warning: No terms found on {dataSource.Name}.");
            }

            return xray;
        }

        public void ExportAndDisplayTerms(XRay xray, ISecondarySource dataSource, bool overwriteAliases, bool splitAliases)
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
            var aliasPath = _directoryService.GetAliasPath(xray.Asin);
            if (!aliasesDownloaded && (!File.Exists(aliasPath) || overwriteAliases))
            {
                // overwrite path in case it waas changed within the service
                aliasPath = _aliasesRepository.SaveCharactersToFile(xray.Terms, xray.Asin, splitAliases);
                if (aliasPath != null)
                    _logger.Log($"Characters exported to {aliasPath} for adding aliases.");
            }

            var termsFound = $"{xray.Terms.Count} {(xray.Terms.Count > 1 ? "terms" : "term")} found";
            _logger.Log($"{termsFound} on {dataSource.Name}:");
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
            IMetadata metadata,
            Stream rawMlStream,
            bool useNewVersion,
            bool skipNoLikes,
            int minClipLen,
            bool overwriteChapters,
            Func<bool> editChaptersCallback,
            IProgressBar progress,
            CancellationToken token,
            bool ignoreSoftHypen = false,
            bool shortEx = true)
        {
            var locOffset = metadata.IsAzw3 ? -16 : 0;

            // If there is an apostrophe, attempt to match 's at the end of the term
            // Match end of word, then search for any lingering punctuation
            var apostrophes = _encoding.GetString(Encoding.UTF8.GetBytes("('|\u2019|\u0060|\u00B4)")); // '\u2019\u0060\u00B4
            var quotes = _encoding.GetString(Encoding.UTF8.GetBytes("(\"|\u2018|\u2019|\u201A|\u201B|\u201C|\u201D|\u201E|\u201F)"));
            var dashesEllipsis = _encoding.GetString(Encoding.UTF8.GetBytes("(-|\u2010|\u2011|\u2012|\u2013|\u2014|\u2015|\u2026|&#8211;|&#8212;|&#8217;|&#8218;|&#8230;)")); //U+2010 to U+2015 and U+2026
            var punctuationMarks = string.Format(@"({0}s|{0})?{1}?[!\.?,""\);:]*{0}*{1}*{2}*", apostrophes, quotes, dashesEllipsis);

            var web = new HtmlDocument();
            web.Load(rawMlStream, _encoding);

            // Only load chapters when building the old format
            if (!useNewVersion)
            {
                rawMlStream.Seek(0, SeekOrigin.Begin);
                // TODO: passing stream, doc, and contents probably not necessary)
                using var streamReader = new StreamReader(rawMlStream, Encoding.UTF8);
                var readContents = streamReader.ReadToEnd();
                var utf8Doc = new HtmlDocument();
                utf8Doc.LoadHtml(readContents);

                _chaptersService.HandleChapters(xray, xray.Asin, rawMlStream.Length, utf8Doc, readContents, overwriteChapters, editChaptersCallback);
            }
            else
            {
                // set default ERL to prevent filtering
                xray.Srl = 1;
                xray.Erl = rawMlStream.Length;
            }

            _logger.Log(CoreStrings.ScanningEbookContent);
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            //Iterate over all paragraphs in book
            var nodes = web.DocumentNode.SelectNodes("//p")
                ?? web.DocumentNode.SelectNodes("//div[@class='paragraph']")
                ?? web.DocumentNode.SelectNodes("//div[@class='p-indent']");
            if (nodes == null)
            {
                nodes = web.DocumentNode.SelectNodes("//div");
                _logger.Log($@"{CoreStrings.Warning}: {CoreStrings.CouldNotLocateParagraphsNormally}{Environment.NewLine}{CoreStrings.SearchingAllDivs}");
            }

            if (nodes == null)
                throw new Exception(CoreStrings.CouldNotLocateAnyParagraphs);
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
                    var search = character.Aliases.Select(alias => _encoding.GetString(Encoding.UTF8.GetBytes(alias)))
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
                        search.Add(character.TermName);
                        // Search list should be in descending order by length, even the term name itself
                        search = search.OrderByDescending(s => s.Length).ToList();

                        // TODO consider removing this "termfound" section 'cause it might be redundant and pointless now
                        if ((character.MatchCase && (search.Any(node.InnerText.Contains) || search.Any(node.InnerHtml.Contains)))
                            || (!character.MatchCase && (search.Any(node.InnerText.ContainsIgnorecase) || search.Any(node.InnerHtml.ContainsIgnorecase)))
                                || (ignoreSoftHypen && (character.MatchCase && search.Any(noSoftHypen.Contains))
                                    || (!character.MatchCase && search.Any(noSoftHypen.ContainsIgnorecase))))
                            termFound = true;
                    }

                    if (!termFound)
                        continue;

                    var paragraphInfo = new IndexLength(location + locOffset, lenQuote);
                    var occurrences = _termsService.FindOccurrences(metadata, character, node.InnerHtml, paragraphInfo);
                    if (!occurrences.Any())
                    {
                        // _logger.Log($"An error occurred while searching for start of highlight.\r\nWas looking for (or one of the aliases of): {character.TermName}\r\nSearching in: {node.InnerHtml}");
                        continue;
                    }

                    character.Occurrences.UnionWith(occurrences);

                    ExcerptHelper.EnhanceOrAddExcerpts(xray.Excerpts, character.Id, paragraphInfo);
                }

                // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                if (useNewVersion && xray.NotableClips != null)
                    ExcerptHelper.ProcessNotablesForParagraph(node.InnerText, location, xray.NotableClips, xray.Excerpts, skipNoLikes, minClipLen);

                progress?.Add(1);
            }

            timer.Stop();
            _logger.Log(string.Format(CoreStrings.ScanTime, timer.Elapsed));
            //output list of terms with no occurrences
            foreach (var t in xray.Terms.Where(t => t.Match && t.Occurrences.Count == 0))
            {
                _logger.Log(string.Format(CoreStrings.NoLocationsFoundForTerm, t.TermName));
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

        private IEnumerable<Occurrence> ShortenHighlightsInNode(HtmlNode node, IEnumerable<Occurrence> occurrences)
        {
            //If an excerpt is too long, the X-Ray reader cuts it off (in firmware versions < 5.6).
            //If the location of the highlighted word (character name) within the excerpt is far enough in to get cut off,
            //this section attempts to shorted the excerpt by locating the start of a sentence that is just far enough away from the highlight.
            //The length is determined by the space the excerpt takes up rather than its actual length... so 135 is just a guess based on what I've seen.
            const int lengthLimit = 135;
            foreach (var occurrence in occurrences)
            {
                if (occurrence.Highlight.Index + occurrence.Highlight.Length <= lengthLimit)
                {
                    yield return occurrence;
                    continue;
                }
                var start = occurrence.Highlight.Index;
                var newLoc = -1;
                var newLenQuote = 0;
                var newLocHighlight = 0;

                while (start > -1)
                {
                    var at = node.InnerHtml.LastIndexOfAny(new[] {'.', '?', '!'}, start);
                    if (at > -1)
                    {
                        start = at - 1;
                        if (occurrence.Highlight.Index + occurrence.Highlight.Length + 1 - at - 2 <= lengthLimit)
                        {
                            newLoc = occurrence.Excerpt.Index + at + 2;
                            newLenQuote = node.InnerHtml.Length - at - 2;
                            newLocHighlight = occurrence.Highlight.Index - at - 2;
                        }
                        else
                            break;
                    }
                    else
                        break;
                }

                // Only use new locs if shorter excerpt was found
                yield return newLoc >= 0
                    ? new Occurrence
                    {
                        Excerpt = new IndexLength(newLoc, newLenQuote),
                        Highlight = occurrence.Highlight with { Index = newLocHighlight }
                    }
                    : occurrence;
            }
        }
    }
}