using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.Unpack.KFX;
using XRayBuilder.Core.Unpack.Mobi;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Logic.Parsing;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic.Terms
{
    public sealed class TermsService : ITermsService
    {
        private readonly IXRayBuilderConfig _config;
        private readonly Encoding _encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);
        private readonly string _punctuationMarks;
        private readonly string _quotes;

        public TermsService(IXRayBuilderConfig config)
        {
            _config = config;
            var dashesEllipsis = _encoding.GetString(Encoding.UTF8.GetBytes("(-|\u2010|\u2011|\u2012|\u2013|\u2014|\u2015|\u2026|&#8211;|&#8212;|&#8217;|&#8218;|&#8230;)"));
            var apostrophes = _encoding.GetString(Encoding.UTF8.GetBytes("('|\u2019|\u0060|\u00B4)"));
            _quotes = _encoding.GetString(Encoding.UTF8.GetBytes("(\"|\u2018|\u2019|\u201A|\u201B|\u201C|\u201D|\u201E|\u201F)"));
            _punctuationMarks = string.Format(@"({0}s|{0})?{1}?[!\.?,""\);:]*{0}*{1}*{2}*", apostrophes, _quotes, dashesEllipsis);
        }

        // todo extractor factory
        /// <summary>
        /// Extract terms from the given db.
        /// </summary>
        /// <param name="xrayDb">Connection to any db containing the proper dataset.</param>
        /// <param name="singleUse">If set, will close the connection when complete.</param>
        public IEnumerable<Term> ExtractTermsNew(DbConnection xrayDb, bool singleUse)
        {
            if (xrayDb.State != ConnectionState.Open)
                xrayDb.Open();

            using var command = xrayDb.CreateCommand();
            command.CommandText = @"
                SELECT entity.id,entity.label,entity.type,entity.count,entity_description.text,string.text as sourcetxt FROM entity
                LEFT JOIN entity_description ON entity.id = entity_description.entity
                LEFT JOIN source ON entity_description.source = source.id
                LEFT JOIN string ON source.label = string.id AND string.language = 'en'
                WHERE entity.has_info_card = '1'";
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var type = Convert.ToInt32(reader["type"]);

                var newTerm = new Term
                {
                    Id = Convert.ToInt32(reader["id"]),
                    TermName = (string)reader["label"],
                    Type = type == 1 ? "character" : "topic",
                    Desc = (string)reader["text"],
                    DescSrc = reader["sourcetxt"] == DBNull.Value ? "" : (string)reader["sourcetxt"],
                    MatchCase = type == 1 // characters should match case by default
                };

                // Real locations aren't needed for extracting terms for preview or XML saving, but need count
                var i = Convert.ToInt32(reader["count"]);
                for (; i > 0; i--)
                    newTerm.Occurrences.Add(new Occurrence());

                // TODO: Should probably also confirm whether this URL exists or not
                if (newTerm.DescSrc == "Wikipedia")
                    newTerm.DescUrl = $@"http://en.wikipedia.org/wiki/{newTerm.TermName.Replace(" ", "_")}";

                yield return newTerm;
            }

            if (singleUse)
                xrayDb.Close();
        }

        /// <summary>
        /// Extract terms from the old JSON X-Ray format
        /// </summary>
        public IEnumerable<Term> ExtractTermsOld(string path)
        {
            string readContents;
            using (var streamReader = new StreamReader(path, Encoding.UTF8))
                readContents = streamReader.ReadToEnd();

            var xray = JObject.Parse(readContents);
            return xray["terms"] == null
                ? Enumerable.Empty<Term>()
                : xray["terms"].Children().Select(token => token.ToObject<Term>());
        }

        /// <summary>
        /// Downloads terms from the <paramref name="dataSource"/> and saves them to <paramref name="outFile"/>
        /// </summary>
        public async Task DownloadAndSaveAsync(ISecondarySource dataSource, string dataUrl, string outFile, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken token = default)
        {
            var terms = (await dataSource.GetTermsAsync(dataUrl, asin, tld, includeTopics, progress, token)).ToArray();
            if (terms.Length == 0)
                throw new Exception($"No terms were found on {dataSource.Name}");
            XmlUtil.SerializeToFile(terms, outFile);
        }

        public IEnumerable<Term> ReadTermsFromTxt(string txtFile)
        {
            using var streamReader = new StreamReader(txtFile, Encoding.UTF8);
            var termId = 1;
            var lineCount = 1;
            while (!streamReader.EndOfStream)
            {
                var type = streamReader.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(type))
                    continue;
                lineCount++;
                if (type != "character" && type != "topic")
                    throw new Exception($"Error: Invalid term type \"{type}\" on line {lineCount}");

                yield return new Term
                {
                    Type = type,
                    TermName = streamReader.ReadLine(),
                    Desc = streamReader.ReadLine(),
                    MatchCase = type == "character",
                    DescSrc = "shelfari",
                    Id = termId++
                };

                lineCount += 2;
            }
        }

        public HashSet<Occurrence> FindOccurrences(IMetadata metadata, Term term, Paragraph paragraph)
        {
            if (!term.Match)
                return new HashSet<Occurrence>();

            return metadata switch
            {
                Metadata _ => FindOccurrencesLegacy(term, paragraph),
                KfxContainer _ => FindOccurrences(term, paragraph),
                _ => throw new NotSupportedException()
            };
        }

        private HashSet<Occurrence> FindOccurrences(Term term, Paragraph paragraph)
        {
            // If the aliases are not supposed to be in regex format, escape them
            var aliases = term.RegexAliases
                ? term.Aliases
                : term.Aliases.Select(Regex.Escape);

            var searchList = new[] {term.TermName}.Concat(aliases).ToArray();

            //Search content for character name and aliases, respecting the case setting
            var regexOptions = term.MatchCase || term.RegexAliases
                ? RegexOptions.None
                : RegexOptions.IgnoreCase;

            return searchList
                .Select(search => Regex.Matches(paragraph.ContentText, $@"{_quotes}?\b{search}{_punctuationMarks}", regexOptions))
#if NETFRAMEWORK
                .SelectMany(matches => matches.Cast<Match>())
#else
                .SelectMany(matches => matches)
#endif
                .Select(match => new Occurrence
                {
                    Excerpt = new IndexLength(paragraph.Location, paragraph.Length),
                    Highlight = new IndexLength(match.Index, match.Length)
                })
                .ToHashSet();
        }

        private HashSet<Occurrence> FindOccurrencesLegacy(Term term, Paragraph paragraph)
        {
            // Convert from UTF8 string to default-encoded representation
            var search = term.Aliases.Select(alias => _encoding.GetString(Encoding.UTF8.GetBytes(alias))).ToList();
            if (!term.RegexAliases)
            {
                search.Add(term.TermName);
                search = search.OrderByDescending(s => s.Length).ToList();
            }

            var occurrences = new HashSet<Occurrence>();
            //Search html for character name and aliases
            foreach (var s in search)
            {
                var matches = Regex.Matches(paragraph.ContentHtml!, $@"{_quotes}?\b{s}{_punctuationMarks}", term.MatchCase || term.RegexAliases ? RegexOptions.None : RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    occurrences.Add(new Occurrence
                    {
                        Excerpt = new IndexLength(paragraph.Location, paragraph.Length),
                        Highlight = new IndexLength(match.Index, match.Length)
                    });
                }
            }

            //If normal search fails, use regexp to search in case there is some wacky html nested in term
            //Regexp may be less than ideal for parsing HTML but seems to work ok so far in these small paragraphs
            //Also search in soft hyphen-less text if option is set to do so
            if (occurrences.Any())
            {
                foreach (var s in search)
                {
                    var patterns = new List<string>();
                    const string patternHtml = "(?:<[^>]*>)*";
                    //Match HTML tags -- provided there's nothing malformed
                    const string patternSoftHypen = "(\u00C2\u00AD|&shy;|&#173;|&#xad;|&#0173;|&#x00AD;)*";
                    var pattern = string.Format("{0}{1}{0}{2}",
                        patternHtml,
                        string.Join(patternHtml + patternSoftHypen, term.RegexAliases ? s.ToCharArray() : Regex.Unescape(s).ToCharArray()),
                        _punctuationMarks);
                    patterns.Add(pattern);
                    foreach (var pat in patterns)
                    {
                        MatchCollection matches;
                        if (term.MatchCase || term.RegexAliases)
                            matches = Regex.Matches(paragraph.ContentHtml!, pat);
                        else
                            matches = Regex.Matches(paragraph.ContentHtml!, pat, RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                            occurrences.Add(new Occurrence
                            {
                                Excerpt = new IndexLength(paragraph.Location, paragraph.Length),
                                Highlight = new IndexLength(match.Index, match.Length)
                            });
                    }
                }
            }
            else
                return occurrences;

            // Shortening is only useful for the old format
            if (!_config.UseNewVersion && _config.ShortenExcerptsLegacy)
                occurrences = ShortenHighlightsInParagraph(paragraph.ContentHtml, occurrences).ToHashSet();

            return occurrences;
        }

        private IEnumerable<Occurrence> ShortenHighlightsInParagraph(string paragraph, IEnumerable<Occurrence> occurrences)
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
                    var at = paragraph.LastIndexOfAny(new[] {'.', '?', '!'}, start);
                    if (at > -1)
                    {
                        start = at - 1;
                        if (occurrence.Highlight.Index + occurrence.Highlight.Length + 1 - at - 2 <= lengthLimit)
                        {
                            newLoc = occurrence.Excerpt.Index + at + 2;
                            newLenQuote = paragraph.Length - at - 2;
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