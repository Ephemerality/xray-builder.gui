using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Localization.Core;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Chapters;
using XRayBuilder.Core.XRay.Logic.Parsing;
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
        private readonly IParagraphsService _paragraphsService;
        private readonly IXRayBuilderConfig _config;

        public XRayService(
            ILogger logger,
            ChaptersService chaptersService,
            IAliasesRepository aliasesRepository,
            IDirectoryService directoryService,
            ITermsService termsService,
            IParagraphsService paragraphsService,
            IXRayBuilderConfig config)
        {
            _logger = logger;
            _chaptersService = chaptersService;
            _aliasesRepository = aliasesRepository;
            _directoryService = directoryService;
            _termsService = termsService;
            _paragraphsService = paragraphsService;
            _config = config;
        }

        public Task<XRay> CreateXRayAsync(string dataLocation, IMetadata metadata, string tld, bool includeTopics, ISecondarySource dataSource, IProgressBar progress, CancellationToken cancellationToken)
            => CreateXRayAsync(dataLocation, metadata.DbName, metadata.UniqueId, metadata.Asin, metadata.Author, metadata.Title, tld, includeTopics, dataSource, progress, cancellationToken);

        public async Task<XRay> CreateXRayAsync(
            string dataLocation,
            string db,
            string guid,
            string asin,
            string author,
            string title,
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
                Terms = terms,
                Author = author,
                Title = title
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
            YesNoPrompt yesNoPrompt,
            EditCallback editCallback,
            IProgressBar progress,
            CancellationToken token)
        {
            // Only load chapters when building the old format
            if (!_config.UseNewVersion)
            {
                rawMlStream.Seek(0, SeekOrigin.Begin);
                // TODO: passing stream, doc, and contents probably not necessary)
                using var streamReader = new StreamReader(rawMlStream, Encoding.UTF8);
                var readContents = streamReader.ReadToEnd();
                var utf8Doc = new HtmlDocument();
                utf8Doc.LoadHtml(readContents);

                _chaptersService.HandleChapters(xray, xray.Asin, rawMlStream.Length, utf8Doc, readContents, yesNoPrompt, editCallback);
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

            var paragraphs = _paragraphsService.GetParagraphs(metadata).ToArray();
            if (!paragraphs.Any())
                throw new Exception(CoreStrings.CouldNotLocateAnyParagraphs);

            progress?.Set(0, paragraphs.Length);
            foreach (var paragraph in paragraphs)
            {
                token.ThrowIfCancellationRequested();

                //Skip paragraph if outside known chapter range or if html is missing (shouldn't be, just a safety check)
                if (paragraph.Location < xray.Srl || paragraph.Location > xray.Erl || paragraph.ContentHtml == null)
                    continue;

                var noSoftHypen = "";
                if (_config.IgnoreSoftHyphen)
                {
                    noSoftHypen = paragraph.ContentText;
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
                        if (search.Any(r => Regex.Match(paragraph.ContentText, r).Success)
                            || search.Any(r => Regex.Match(paragraph.ContentHtml!, r).Success)
                            || (_config.IgnoreSoftHyphen && search.Any(r => Regex.Match(noSoftHypen, r).Success)))
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
                        if ((character.MatchCase && (search.Any(paragraph.ContentText.Contains) || search.Any(paragraph.ContentHtml.Contains)))
                            || (!character.MatchCase && (search.Any(paragraph.ContentText.ContainsIgnorecase) || search.Any(paragraph.ContentHtml.ContainsIgnorecase)))
                                || (_config.IgnoreSoftHyphen && (character.MatchCase && search.Any(noSoftHypen.Contains))
                                    || (!character.MatchCase && search.Any(noSoftHypen.ContainsIgnorecase))))
                            termFound = true;
                    }

                    if (!termFound)
                        continue;

                    var occurrences = _termsService.FindOccurrences(metadata, character, paragraph);
                    if (!occurrences.Any())
                    {
                        // _logger.Log($"An error occurred while searching for start of highlight.\r\nWas looking for (or one of the aliases of): {character.TermName}\r\nSearching in: {node.InnerHtml}");
                        continue;
                    }

                    character.Occurrences.UnionWith(occurrences);

                    ExcerptHelper.EnhanceOrAddExcerpts(xray.Excerpts, character.Id, new IndexLength(paragraph.Location, paragraph.Length));
                }

                // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                if (_config.UseNewVersion && xray.NotableClips != null)
                    ExcerptHelper.ProcessNotablesForParagraph(paragraph.ContentText, paragraph.Location, xray.NotableClips, xray.Excerpts, _config.SkipNoLikes, _config.MinimumClipLength);

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
    }
}