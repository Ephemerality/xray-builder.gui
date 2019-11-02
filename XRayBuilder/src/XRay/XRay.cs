/*  Builds an X-Ray file to be used on the Amazon Kindle
*   Original xray builder by shinew, http://www.mobileread.com/forums/showthread.php?t=157770 , http://www.xunwang.me/xray/
*
*   Copyright (C) 2014 Ephemerality <Nick Niemi - ephemeral.vilification@gmail.com>
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.

*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.

*   You should have received a copy of the GNU General Public License
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

// HTMLAgilityPack from http://htmlagilitypack.codeplex.com

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Libraries.Primitives.Extensions;
using XRayBuilderGUI.Libraries.Progress;
using XRayBuilderGUI.XRay.Artifacts;
using XRayBuilderGUI.XRay.Logic;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI.XRay
{
    // TODO: Anywhere JSON is used, serialization should be done rather than text formatting...
    public partial class XRay
    {
        private readonly ILogger _logger;

        private string dataUrl = "";
        private string xmlFile = "";
        private string databaseName = "";
        private string _guid = "";
        private string asin = "";
        private string version = "1";
        private string _aliasPath;
        public List<Term> Terms = new List<Term>(100);
        private List<Chapter> _chapters = new List<Chapter>();
        private List<Excerpt> excerpts = new List<Excerpt>();
        private long _srl;
        private long _erl;
        public bool unattended;
        private bool skipShelfari;
        private int locOffset;
        private List<NotableClip> notableClips;
        private int foundNotables;
        public DateTime? CreatedAt { get; set; }

        private bool enableEdit = Properties.Settings.Default.enableEdit;
        private readonly ISecondarySource dataSource;
        private readonly IAliasesService _aliasesService;

        public delegate DialogResult SafeShowDelegate(string msg, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton def);

        // TODO: Do something about this
        #region CommonTitles
        string[] CommonTitles = { "Mr", "Mrs", "Ms", "Miss", "Dr", "Herr", "Monsieur", "Hr", "Frau",
            "A V M", "Admiraal", "Admiral", "Alderman", "Alhaji", "Ambassador", "Baron", "Barones", "Brig",
            "Brigadier", "Brother", "Canon", "Capt", "Captain", "Cardinal", "Cdr", "Chief", "Cik", "Cmdr", "Col",
            "Colonel", "Commandant", "Commander", "Commissioner", "Commodore", "Comte", "Comtessa", "Congressman",
            "Conseiller", "Consul", "Conte", "Contessa", "Corporal", "Councillor", "Count", "Countess", "Air Cdre",
            "Air Commodore", "Air Marshal", "Air Vice Marshal", "Brig Gen", "Brig General", "Brigadier General",
            "Crown Prince", "Crown Princess", "Dame", "Datin", "Dato", "Datuk", "Datuk Seri", "Deacon", "Deaconess",
            "Dean", "Dhr", "Dipl Ing", "Doctor", "Dott", "Dott Sa", "Dr", "Dr Ing", "Dra", "Drs", "Embajador",
            "Embajadora", "En", "Encik", "Eng", "Eur Ing", "Exma Sra", "Exmo Sr", "Father", "First Lieutient",
            "First Officer", "Flt Lieut", "Flying Officer", "Fr", "Frau", "Fraulein", "Fru", "Gen", "Generaal",
            "General", "Governor", "Graaf", "Gravin", "Group Captain", "Grp Capt", "H E Dr", "H H", "H M", "H R H",
            "Hajah", "Haji", "Hajim", "Her Highness", "Her Majesty", "Herr", "High Chief", "His Highness",
            "His Holiness", "His Majesty", "Hon", "Hr", "Hra", "Ing", "Ir", "Jonkheer", "Judge", "Justice",
            "Khun Ying", "Kolonel", "Lady", "Lcda", "Lic", "Lieut", "Lieut Cdr", "Lieut Col", "Lieut Gen", "Lord",
            "Madame", "Mademoiselle", "Maj Gen", "Major", "Master", "Mevrouw", "Miss", "Mlle", "Mme", "Monsieur",
            "Monsignor", "Mstr", "Nti", "Pastor", "President", "Prince", "Princess", "Princesse", "Prinses", "Prof",
            "Prof Dr", "Prof Sir", "Professor", "Puan", "Puan Sri", "Rabbi", "Rear Admiral", "Rev", "Rev Canon",
            "Rev Dr", "Rev Mother", "Reverend", "Rva", "Senator", "Sergeant", "Sheikh", "Sheikha", "Sig", "Sig Na",
            "Sig Ra", "Sir", "Sister", "Sqn Ldr", "Sr", "Sr D", "Sra", "Srta", "Sultan", "Tan Sri", "Tan Sri Dato",
            "Tengku", "Teuku", "Than Puying", "The Hon Dr", "The Hon Justice", "The Hon Miss", "The Hon Mr",
            "The Hon Mrs", "The Hon Ms", "The Hon Sir", "The Very Rev", "Toh Puan", "Tun", "Vice Admiral",
            "Viscount", "Viscountess", "Wg Cdr", "Jr", "Sr", "Sheriff", "Special Agent", "Detective", "Lt" };
        #endregion

        public XRay(string shelfari, ISecondarySource dataSource, ILogger logger, IAliasesService aliasesService)
        {
            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "https://" + shelfari;
            dataUrl = shelfari;
            this.dataSource = dataSource;
            _logger = logger;
            _aliasesService = aliasesService;
        }

        public XRay(string shelfari, string db, string guid, string asin, ISecondarySource dataSource, ILogger logger, IAliasesService aliasesService, int locOffset = 0, string aliaspath = "", bool unattended = false)
        {
            if (shelfari == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");

            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "https://" + shelfari;
            dataUrl = shelfari;
            databaseName = db;
            if (guid != null)
                Guid = guid;
            this.asin = asin;
            this.locOffset = locOffset;
            _aliasPath = aliaspath;
            this.unattended = unattended;
            this.dataSource = dataSource;
            _logger = logger;
            _aliasesService = aliasesService;
        }

        public XRay(string xml, string db, string guid, string asin, ISecondarySource dataSource, ILogger logger, IAliasesService aliasesService, int locOffset = 0, string aliaspath = "")
        {
            if (xml == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");
            xmlFile = xml;
            databaseName = db;
            Guid = guid;
            this.asin = asin;
            this.locOffset = locOffset;
            _aliasPath = aliaspath;
            unattended = false;
            this.dataSource = dataSource;
            _logger = logger;
            _aliasesService = aliasesService;
            skipShelfari = true;
        }

        public string AliasPath
        {
            set => _aliasPath = value;
            // TODO directory service to handle default paths
            get => string.IsNullOrEmpty(_aliasPath) ? Environment.CurrentDirectory + @"\ext\" + asin + ".aliases" : _aliasPath;
        }

        public string Guid
        {
            private set => _guid = Functions.ConvertGuid(value);
            get => _guid;
        }

        public async Task<int> SaveXml(string outfile, IProgressBar progress, CancellationToken token = default)
        {
            try
            {
                Terms = (await dataSource.GetTermsAsync(dataUrl, progress, token)).ToList();
            }
            catch (OperationCanceledException)
            {
                return 2;
            }
            if (Terms.Count == 0)
                return 1;
            _logger.Log(@"Exporting terms...");
            Functions.Save(Terms, outfile);
            return 0;
        }

        public override string ToString()
        {
            //Insert a version tag of the current program version so you know which version built it.
            //Will be ignored by the Kindle.
            var dd = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var xrayversion = $"{dd.Major}.{dd.Minor}{dd.Build}";
            //Insert creation date... seems useful?
            CreatedAt ??= DateTime.Now;
            //If there are no chapters built (someone only ran create X-Ray), just use the default version
            if (_chapters.Count > 0)
                return
                    string.Format(
                        @"{{""asin"":""{0}"",""guid"":""{1}:{2}"",""version"":""{3}"",""xrayversion"":""{8}"",""created"":""{9}"",""terms"":[{4}],""chapters"":[{5}],""assets"":{{}},""srl"":{6},""erl"":{7}}}",
                        asin, databaseName, Guid, version, string.Join(",", Terms),
                        string.Join(",", _chapters), _srl, _erl, xrayversion, CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            return
                string.Format(
                    @"{{""asin"":""{0}"",""guid"":""{1}:{2}"",""version"":""{3}"",""xrayversion"":""{5}"",""created"":""{6}"",""terms"":[{4}],""chapters"":[{{""name"":null,""start"":1,""end"":9999999}}]}}",
                    asin, databaseName, Guid, version, string.Join(",", Terms), xrayversion, CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        //Add string creation for new XRAY.ASIN.previewData file
        public string getPreviewData()
        {
            var preview = @"{{""numImages"":0,""numTerms"":{0},""previewImages"":""[]"",""excerptIds"":[],""numPeople"":{1}}}";
            preview = string.Format(preview, Terms.Count(t => t.Type == "topic"), Terms.Count(t => t.Type == "character"));
            return preview;
        }

        public string XRayName(bool android = false) =>
            android
                ? $"XRAY.{asin}.{(databaseName == null ? "" : $"{databaseName}_")}{Guid ?? ""}.db"
                : $"XRAY.entities.{asin}.asc";

        // TODO: Change return values to exceptions instead
        public async Task<int> CreateXray(IProgressBar progress, CancellationToken token = default)
        {
            //Download Shelfari info if not skipping
            if (skipShelfari)
            {
                if (!File.Exists(xmlFile))
                {
                    _logger.Log("An error occurred opening file (" + xmlFile + ")");
                    return 1;
                }
                _logger.Log("Loading terms from file...");
                var filetype = Path.GetExtension(xmlFile);
                if (filetype == ".xml")
                    Terms = Functions.DeserializeList<Term>(xmlFile);
                else if (filetype == ".txt")
                {
                    if (LoadTermsFromTxt(xmlFile) > 0)
                    {
                        _logger.Log("An error occurred loading from text file.");
                        return 1;
                    }
                }
                else
                {
                    _logger.Log("Error: Bad file type \"" + filetype + "\"");
                    return 1;
                }
                if (Terms == null || Terms.Count == 0) return 1;
            }
            else
            {
                try
                {
                    Terms = (await dataSource.GetTermsAsync(dataUrl, progress, token)).ToList();
                    _logger.Log("Downloading notable clips...");
                    notableClips = (await dataSource.GetNotableClipsAsync(dataUrl, null, progress, token))?.ToList();
                }
                catch (OperationCanceledException)
                {
                    return 2;
                }
                if (Terms.Count == 0)
                {
                    _logger.Log("Error: No terms found on " + dataSource.Name + ".");
                    return 1;
                }
            }

            return 0;
        }

        public void ExportAndDisplayTerms()
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

            if (!aliasesDownloaded && (!File.Exists(AliasPath) || Properties.Settings.Default.overwriteAliases))
            {
                SaveCharacters(AliasPath);
                _logger.Log($"Characters exported to {AliasPath} for adding aliases.");
            }

            if (skipShelfari)
                _logger.Log(string.Format("{0} {1} found in file:", Terms.Count, Terms.Count > 1 ? "Terms" : "Term"));
            else
                _logger.Log(string.Format("{0} {1} found on {2}:", Terms.Count, Terms.Count > 1 ? "Terms" : "Term", dataSource.Name));
            var str = new StringBuilder(Terms.Count * 32); // Assume that most names will be less than 32 chars
            var termId = 1;
            foreach (var t in Terms)
            {
                str.Append(t.TermName).Append(", ");
                t.Id = termId++;
            }
            _logger.Log(str.ToString());
        }

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

        public void HandleChapters(long mlLen, HtmlDocument doc, string rawMl, SafeShowDelegate safeShow)
        {
            //Similar to aliases, if chapters definition exists, load it. Otherwise, attempt to build it from the book
            var chapterFile = Environment.CurrentDirectory + @"\ext\" + asin + ".chapters";
            if (File.Exists(chapterFile) && !Properties.Settings.Default.overwriteChapters)
            {
                if (LoadChapters())
                    _logger.Log($"Chapters read from {chapterFile}.\r\nDelete this file if you want chapters built automatically.");
                else
                    _logger.Log($"An error occurred reading chapters from {chapterFile}.\r\nFile is missing or not formatted correctly.");
            }
            else
            {
                try
                {
                    SearchChapters(doc, rawMl);
                }
                catch (Exception ex)
                {
                    _logger.Log("Error searching for chapters: " + ex.Message);
                }
                //Built chapters list is saved for manual editing
                if (_chapters.Count > 0)
                {
                    SaveChapters();
                    _logger.Log($"Chapters exported to {chapterFile} for manual editing.");
                }
                else
                    _logger.Log($"No chapters detected.\r\nYou can create a file at {chapterFile} if you want to define chapters manually.");
            }

            if (!unattended && enableEdit)
                if (DialogResult.Yes ==
                    safeShow("Would you like to open the chapters file in notepad for editing?", "Chapters",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(chapterFile);
                    _chapters.Clear();
                    if (LoadChapters())
                        _logger.Log("Reloaded chapters from edited file.");
                    else
                        _logger.Log($"An error occurred reloading chapters from {chapterFile}.\r\nFile is missing or not formatted correctly.");
                }

            //If no chapters were found, add a default chapter that spans the entire book
            //Define srl and erl so "progress bar" shows up correctly
            if (_chapters.Count == 0)
            {
                _chapters.Add(new Chapter
                {
                    Name = "",
                    Start = 1,
                    End = mlLen
                });
                _srl = 1;
                _erl = mlLen;
            }
            else
            {
                //Run through all chapters and take the highest value, in case some chapters can be defined in individual chapters and parts.
                //EG. Part 1 includes chapters 1-6, Part 2 includes chapters 7-12.
                _srl = _chapters[0].Start;
                _logger.Log("Found chapters:");
                foreach (var c in _chapters)
                {
                    if (c.End > _erl) _erl = c.End;
                    _logger.Log($"{c.Name} | start: {c.Start} | end: {c.End}");
                }
            }
        }

        public int ExpandFromRawMl(Stream rawMlStream, SafeShowDelegate safeShow, IProgressBar progress, CancellationToken token, bool ignoreSoftHypen = false, bool shortEx = true)
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
                HandleChapters(rawMlStream.Length, utf8Doc, readContents, safeShow);
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
                {
                    _logger.Log("An error occurred locating the paragraph within the book content.");
                    return 1;
                }
                if (location < _srl || location > _erl) continue; //Skip paragraph if outside chapter range
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
                foreach (var character in Terms)
                {
                    //Search for character name and aliases in the html-less text. If failed, try in the HTML for rare situations.
                    //TODO: Improve location searching as IndexOf will not work if book length exceeds 2,147,483,647...
                    //If soft hyphen ignoring is turned on, also search hyphen-less text.
                    if (!character.Match) continue;
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
                    if (termFound)
                    {
                        var locHighlight = new List<int>();
                        var lenHighlight = new List<int>();
                        //Search html for character name and aliases
                        foreach (var s in search)
                        {
                            var matches = Regex.Matches(node.InnerHtml, quotes + @"?\b" + s + punctuationMarks, character.MatchCase || character.RegexAliases ? RegexOptions.None : RegexOptions.IgnoreCase);
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
                                var patternHTML = "(?:<[^>]*>)*";
                                //Match HTML tags -- provided there's nothing malformed
                                var patternSoftHypen = "(\u00C2\u00AD|&shy;|&#173;|&#xad;|&#0173;|&#x00AD;)*";
                                var pattern = string.Format("{0}{1}{0}{2}", patternHTML,
                                    string.Join(patternHTML + patternSoftHypen, character.RegexAliases ? s.ToCharArray() : Regex.Unescape(s).ToCharArray()), punctuationMarks);
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
                            _logger.Log(
                                string.Format(
                                    "An error occurred while searching for start of highlight.\r\nWas looking for (or one of the aliases of): {0}\r\nSearching in: {1}",
                                    character.TermName, node.InnerHtml));
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
                                    character.Locs.Add(string.Format("[{0},{1},{2},{3}]", newLoc + locOffset, newLenQuote,
                                        newLocHighlight, lenHighlight[j]));
                                    locHighlight.RemoveAt(j);
                                    lenHighlight.RemoveAt(j--);
                                }
                            }
                        }

                        for (var j = 0; j < locHighlight.Count; j++)
                        {
                            character.Locs.Add(string.Format("[{0},{1},{2},{3}]", location + locOffset, lenQuote,
                                locHighlight[j], lenHighlight[j])); // For old format
                            character.Occurrences.Add(new[] { location + locOffset + locHighlight[j], lenHighlight[j] }); // For new format
                        }
                        var exCheck = excerpts.Where(t => t.Start.Equals(location + locOffset)).ToArray();
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
                                Start = location + locOffset,
                                Length = lenQuote
                            };
                            newExcerpt.RelatedEntities.Add(character.Id);
                            excerpts.Add(newExcerpt);
                        }
                    }
                }

                // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                if (Properties.Settings.Default.useNewVersion && notableClips != null)
                {
                    foreach (var quote in notableClips)
                    {
                        var index = node.InnerText.IndexOf(quote.Text, StringComparison.Ordinal);
                        if (index > -1)
                        {
                            // See if an excerpt already exists at this location
                            var excerpt = excerpts.FirstOrDefault(e => e.Start == index);
                            if (excerpt == null)
                            {
                                if (Properties.Settings.Default.skipNoLikes && quote.Likes == 0
                                    || quote.Text.Length < Properties.Settings.Default.minClipLen)
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
                                excerpts.Add(excerpt);
                            }
                            else
                                excerpt.RelatedEntities.Add(0);

                            foundNotables++;
                        }
                    }
                }
                progress?.Add(1);
            }

            timer.Stop();
            _logger.Log("Scan time: " + timer.Elapsed);
            //output list of terms with no locs
            foreach (var t in Terms)
            {
                if (t.Match && t.Locs.Count == 0)
                    _logger.Log($"No locations were found for the term \"{t.TermName}\".\r\nYou should add aliases for this term using the book or rawml as a reference.");
            }
            return 0;
        }

        /// <summary>
        /// Searches for a Table of Contents within the book and adds the chapters to _chapters.
        /// </summary>
        /// <param name="bookDoc">Book's HTML</param>
        /// <param name="rawML">Path to the book's rawML file</param>
        private void SearchChapters(HtmlDocument bookDoc, string rawML)
        {
            var leadingZerosRegex = new Regex(@"^0+(?=\d)", RegexOptions.Compiled);
            string tocHtml;
            var tocDoc = new HtmlDocument();
            var toc = bookDoc.DocumentNode.SelectSingleNode(
                    "//reference[translate(@title,'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='TABLE OF CONTENTS']");
            _chapters.Clear();
            //Find table of contents, using case-insensitive search
            if (toc != null)
            {
                var tocloc = Convert.ToInt32(leadingZerosRegex.Replace(toc.GetAttributeValue("filepos", ""), ""));
                tocHtml = rawML.Substring(tocloc, rawML.IndexOf("<mbp:pagebreak/>", tocloc + 1, StringComparison.Ordinal) - tocloc);
                tocDoc = new HtmlDocument();
                tocDoc.LoadHtml(tocHtml);
                var tocNodes = tocDoc.DocumentNode.SelectNodes("//a");
                foreach (var chapter in tocNodes)
                {
                    if (chapter.InnerHtml == "") continue;
                    var filepos = Convert.ToInt32(leadingZerosRegex.Replace(chapter.GetAttributeValue("filepos", "0"), ""));
                    if (_chapters.Count > 0)
                    {
                        _chapters[_chapters.Count - 1].End = filepos;
                        if (_chapters[_chapters.Count - 1].Start > filepos)
                            _chapters.RemoveAt(_chapters.Count - 1); //remove broken chapters
                    }
                    _chapters.Add(new Chapter
                    {
                        Name = chapter.InnerText,
                        Start = filepos,
                        End = rawML.Length
                    });
                }
            }

            //Search again, looking for Calibre's 'new' mobi ToC format
            if (_chapters.Count == 0)
            {
                try
                {
                    var index = rawML.LastIndexOf(">Table of Contents<");
                    if (index >= 0)
                    {
                        index = rawML.IndexOf("<p ", index);
                        var breakIndex = rawML.IndexOf("<div class=\"mbp_pagebreak\"", index);
                        if (breakIndex == -1)
                            breakIndex = rawML.IndexOf("div class=\"mbppagebreak\"", index);
                        tocHtml = rawML.Substring(index, breakIndex - index);
                        tocDoc.LoadHtml(tocHtml);
                        var tocNodes = tocDoc.DocumentNode.SelectNodes("//p");
                        // Search for each chapter heading, ignore any misses (user can go and add any that are missing if necessary)
                        foreach (var chap in tocNodes)
                        {
                            index = rawML.IndexOf(chap.InnerText);
                            if (index > -1)
                            {
                                if (_chapters.Count > 0)
                                    _chapters[_chapters.Count - 1].End = index;
                                _chapters.Add(new Chapter
                                {
                                    Name = chap.InnerText,
                                    Start = index,
                                    End = rawML.Length
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log("Error searching for Calibre chapters: " + ex.Message);
                }
            }

            // Try searching for Calibre's toc2 nodes
            if (_chapters.Count == 0)
            {
                var tocNodes = bookDoc.DocumentNode.SelectNodes("//p[@class='toc2']")?.ToArray() ?? new HtmlNode[0];
                foreach (var node in tocNodes)
                {
                    var position = node.StreamPosition;
                    if (_chapters.Count > 0)
                        _chapters[_chapters.Count - 1].End = position;
                    _chapters.Add(new Chapter
                    {
                        Name = node.InnerText,
                        Start = position,
                        End = rawML.Length
                    });
                }
            }

            //Try a broad search for chapterish names just for fun
            if (_chapters.Count == 0)
            {
                // TODO: Expand on the chapter matching pattern concept
                const string chapterPattern = @"((?:chapter|book|section|part|capitulo)\s+.*)|((?:prolog|prologue|epilogue)(?:\s+|$).*)|((?:one|two|three|four|five|six|seven|eight|nine|ten)(?:\s+|$).*)";
                const string xpath = "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5]";
                var chapterNodes = bookDoc.DocumentNode.SelectNodes("//a")
                    ?.Where(div => div.GetAttributeValue("class", "") == "chapter" || Regex.IsMatch(div.InnerText, chapterPattern, RegexOptions.IgnoreCase))
                    .ToList();
                if (chapterNodes == null)
                    return;
                var headingNodes = bookDoc.DocumentNode.SelectNodes(xpath).ToList();
                if (headingNodes.Count > chapterNodes.Count)
                    chapterNodes = headingNodes;
                foreach (var chap in chapterNodes)
                {
                    if (chap.InnerText.ContainsIgnorecase("Table of Contents")) continue;
                    var index = rawML.IndexOf(chap.InnerHtml) + chap.InnerHtml.IndexOf(chap.InnerText);
                    if (index > -1)
                    {
                        if (_chapters.Count > 0)
                            _chapters[_chapters.Count - 1].End = index;
                        _chapters.Add(new Chapter
                        {
                            Name = chap.InnerText,
                            Start = index,
                            End = rawML.Length
                        });
                    }
                }
            }
        }

        public void PopulateDb(SQLiteConnection db, IProgressBar progress, CancellationToken token)
        {
            var sql = new StringBuilder(Terms.Count * 256);
            var personCount = 0;
            var termCount = 0;
            var command = new SQLiteCommand($"update string set text='{dataUrl}' where id=15", db);
            command.ExecuteNonQuery();

            _logger.Log("Updating database with terms, descriptions, and excerpts...");
            //Write all entities and occurrences
            _logger.Log($"Writing {Terms.Count} terms...");
            progress?.Set(0, Terms.Count);
            command = new SQLiteCommand("insert into entity (id, label, loc_label, type, count, has_info_card) values (@id, @label, null, @type, @count, 1)", db);
            var command2 = new SQLiteCommand("insert into entity_description (text, source_wildcard, source, entity) values (@text, @source_wildcard, @source, @entity)", db);
            var command3 = new SQLiteCommand("insert into occurrence (entity, start, length) values (@entity, @start, @length)", db);
            foreach (var t in Terms)
            {
                token.ThrowIfCancellationRequested();
                if (t.Type == "character") personCount++;
                else if (t.Type == "topic") termCount++;
                command.Parameters.Add("@id", DbType.Int32).Value = t.Id;
                command.Parameters.Add("@label", DbType.String).Value = t.TermName;
                command.Parameters.Add("@type", DbType.Int32).Value = t.Type == "character" ? 1 : 2;
                command.Parameters.Add("@count", DbType.Int32).Value = t.Occurrences.Count;
                command.ExecuteNonQuery();

                command2.Parameters.Add("@text", DbType.String).Value = t.Desc == "" ? "No description available." : t.Desc;
                command2.Parameters.Add("@source_wildcard", DbType.String).Value = t.TermName;
                command2.Parameters.Add("@source", DbType.Int32).Value = t.DescSrc == "shelfari" ? 4 : 6;
                command2.Parameters.Add("@entity", DbType.Int32).Value = t.Id;
                command2.ExecuteNonQuery();

                foreach (var loc in t.Occurrences)
                {
                    command3.Parameters.Add("@entity", DbType.Int32).Value = t.Id;
                    command3.Parameters.Add("@start", DbType.Int32).Value = loc[0];
                    command3.Parameters.Add("@length", DbType.Int32).Value = loc[1];
                    command3.ExecuteNonQuery();
                }
                progress?.Add(1);
            }

            //Write excerpts and entity_excerpt table
            _logger.Log($"Writing {excerpts.Count} excerpts...");
            command.CommandText = "insert into excerpt (id, start, length, image, related_entities, goto) values (@id, @start, @length, @image, @rel_ent, null);";
            command.Parameters.Clear();
            command2.CommandText = "insert into entity_excerpt (entity, excerpt) values (@entityId, @excerptId)";
            command2.Parameters.Clear();
            progress?.Set(0, excerpts.Count);
            foreach (var e in excerpts)
            {
                token.ThrowIfCancellationRequested();
                command.Parameters.Add("id", DbType.Int32).Value = e.Id;
                command.Parameters.Add("start", DbType.Int32).Value = e.Start;
                command.Parameters.Add("length", DbType.Int32).Value = e.Length;
                command.Parameters.Add("image", DbType.String).Value = e.Image;
                command.Parameters.Add("rel_ent", DbType.String).Value = string.Join(",", e.RelatedEntities.Where(en => en != 0).ToArray()); // don't write 0 (notable flag)
                command.ExecuteNonQuery();
                foreach (var ent in e.RelatedEntities)
                {
                    token.ThrowIfCancellationRequested();
                    if (ent == 0) continue; // skip notable flag
                    command2.Parameters.Add("@entityId", DbType.Int32).Value = ent;
                    command2.Parameters.Add("@excerptId", DbType.Int32).Value = e.Id;
                    command2.ExecuteNonQuery();
                }
                progress?.Add(1);
            }

            // create links to notable clips in order of popularity
            _logger.Log("Adding notable clips...");
            command.Parameters.Clear();
            var notablesOnly = excerpts.Where(ex => ex.Notable).OrderByDescending(ex => ex.Highlights);
            foreach (var notable in notablesOnly)
            {
                command.CommandText = $"insert into entity_excerpt (entity, excerpt) values (0, {notable.Id})";
                command.ExecuteNonQuery();
            }

            // Populate some more clips if not enough were found initially
            // TODO: Add a config value in settings for this amount
            var toAdd = new List<Excerpt>(20);
            if (foundNotables <= 20 && foundNotables + excerpts.Count <= 20)
                toAdd.AddRange(excerpts);
            else if (foundNotables <= 20)
            {
                var rand = new Random();
                var eligible = excerpts.Where(ex => !ex.Notable).ToList();
                while (foundNotables <= 20 && eligible.Count > 0)
                {
                    var randEx = eligible.ElementAt(rand.Next(eligible.Count));
                    toAdd.Add(randEx);
                    eligible.Remove(randEx);
                    foundNotables++;
                }
            }
            foreach (var excerpt in toAdd)
            {
                command.CommandText = $"insert into entity_excerpt (entity, excerpt) values (0, {excerpt.Id})";
                command.ExecuteNonQuery();
            }
            command.Dispose();

            token.ThrowIfCancellationRequested();
            _logger.Log("Writing top mentions...");
            var sorted =
                Terms.Where(t => t.Type.Equals("character"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList();
            sql.Clear();
            sql.AppendFormat("update type set top_mentioned_entities='{0}' where id=1;\n",
                string.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            sorted =
                Terms.Where(t => t.Type.Equals("topic"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList();
            sql.AppendFormat("update type set top_mentioned_entities='{0}' where id=2;",
                string.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();

            token.ThrowIfCancellationRequested();
            _logger.Log("Writing metadata...");

            sql.Clear();
            sql.AppendFormat(
                "insert into book_metadata (srl, erl, has_images, has_excerpts, show_spoilers_default, num_people, num_terms, num_images, preview_images) "
                + "values ({0}, {1}, 0, 1, 0, {2}, {3}, 0, null);", _srl, _erl, personCount, termCount);

            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        private int LoadTermsFromTxt(string txtfile)
        {
            if (!File.Exists(txtfile)) return 1;
            using (var streamReader = new StreamReader(txtfile, Encoding.UTF8))
            {
                var termId = 1;
                var lineCount = 1;
                Terms.Clear();
                while (!streamReader.EndOfStream)
                {
                    try
                    {
                        var temp = streamReader.ReadLine()?.ToLower(); //type
                        if (string.IsNullOrEmpty(temp)) continue;
                        lineCount++;
                        if (temp != "character" && temp != "topic")
                        {
                            _logger.Log("Error: Invalid term type \"" + temp + "\" on line " + lineCount);
                            return 1;
                        }
                        Terms.Add(new Term
                        {
                            Type = temp,
                            TermName = streamReader.ReadLine(),
                            Desc = streamReader.ReadLine(),
                            MatchCase = temp == "character",
                            DescSrc = "shelfari",
                            Id = termId++
                        });
                        lineCount += 2;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("An error occurred reading from txt file: " + ex.Message + "\r\n" + ex.StackTrace);
                        return 1;
                    }
                }
            }
            return 0;
        }

        public void SaveChapters()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\ext\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\ext\");
            using var streamWriter =
                new StreamWriter(Environment.CurrentDirectory + @"\ext\" + asin + ".chapters", false,
                    Encoding.UTF8);
            foreach (var c in _chapters)
                streamWriter.WriteLine(c.Name + "|" + c.Start + "|" + c.End);
        }

        public bool LoadChapters()
        {
            _chapters = new List<Chapter>();
            if (!File.Exists(Environment.CurrentDirectory + @"\ext\" + asin + ".chapters")) return false;
            using (
                var streamReader =
                    new StreamReader(Environment.CurrentDirectory + @"\ext\" + asin + ".chapters", Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    var tmp = streamReader.ReadLine()?.Split('|');
                    if (tmp?.Length != 3) return false; //Malformed chapters file
                    if (tmp[0] == "" || tmp[0].Substring(0, 1) == "#") continue;
                    _chapters.Add(new Chapter
                    {
                        Name = tmp[0],
                        Start = Convert.ToInt32(tmp[1]),
                        End = Convert.ToInt64(tmp[2])
                    });
                }
            }
            return true;
        }

        public void SaveCharacters(string aliasFile)
        {
            // todo service should handle this
            if (!Directory.Exists(Environment.CurrentDirectory + @"\ext\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\ext\");

            // todo these should probably already be loaded at this point
            //Try to load custom common titles from BaseSplitIgnore.txt
            try
            {
                using var streamReader = new StreamReader(Environment.CurrentDirectory + @"\dist\BaseSplitIgnore.txt", Encoding.UTF8);
                var CustomSplitIgnore = streamReader.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.None)
                    .Where(r => !r.StartsWith("//")).ToArray();
                if (CustomSplitIgnore.Length >= 1)
                {
                    CommonTitles = CustomSplitIgnore;
                }
                _logger.Log("Splitting aliases using custom common titles file...");
            }
            catch (Exception ex)
            {
                _logger.Log("An error occurred while opening the BaseSplitIgnore.txt file.\r\n" +
                    "Ensure you extracted it to the same directory as the program.\r\n" +
                    ex.Message + "\r\nUsing built in default terms...");
            }

            //Try to remove common titles from aliases
            using var streamWriter = new StreamWriter(aliasFile, false, Encoding.UTF8);
            var aliasCheck = new List<string>();
            foreach (var c in Terms)
            {
                if (c.Type == "character" && c.TermName.Contains(" "))
                {
                    try
                    {
                        if (Properties.Settings.Default.splitAliases)
                        {
                            var splitName = "";
                            string titleTrimmed;
                            var aliasList = new List<string>();
                            var textInfo = new CultureInfo("en-US", false).TextInfo;

                            var pattern = @"( ?(" + string.Join("|", CommonTitles) +
                                          ")\\.? )|(^[A-Z]\\. )|( [A-Z]\\.)|(\")|(\u201C)|(\u201D)|(,)|(')";

                            var regex = new Regex(pattern);
                            var matchCheck = Regex.Match(c.TermName, pattern);
                            if (matchCheck.Success)
                            {
                                titleTrimmed = c.TermName;
                                foreach (Match match in regex.Matches(titleTrimmed))
                                {
                                    titleTrimmed = titleTrimmed.Replace(match.Value, string.Empty);
                                }
                                foreach (Match match in regex.Matches(titleTrimmed))
                                {
                                    titleTrimmed = titleTrimmed.Replace(match.Value, string.Empty);
                                }
                                aliasList.Add(titleTrimmed);
                            }
                            else
                                titleTrimmed = c.TermName;

                            titleTrimmed = Regex.Replace(titleTrimmed, @"\s+", " ");
                            titleTrimmed = Regex.Replace(titleTrimmed, @"( ?V?I{0,3}$)", string.Empty);
                            titleTrimmed = Regex.Replace(titleTrimmed, @"(\(aka )", "(");

                            var bracketedName = Regex.Match(titleTrimmed, @"(.*)(\()(.*)(\))");
                            if (bracketedName.Success)
                            {
                                aliasList.Add(bracketedName.Groups[3].Value);
                                aliasList.Add(bracketedName.Groups[1].Value.TrimEnd());
                                titleTrimmed = titleTrimmed.Replace(bracketedName.Groups[2].Value, "")
                                    .Replace(bracketedName.Groups[4].Value, "");
                            }

                            if (titleTrimmed.Contains(" "))
                            {
                                titleTrimmed = titleTrimmed.Replace(" &amp;", "").Replace(" &", "");
                                var words = titleTrimmed.Split(' ');
                                foreach (var word in words)
                                {
                                    if (word.ToUpper() == word)
                                        aliasList.Add(textInfo.ToTitleCase(word.ToLower()));
                                    else
                                        aliasList.Add(word);
                                }
                            }
                            if (aliasList.Count > 0)
                            {
                                aliasList.Sort((a, b) => b.Length.CompareTo(a.Length));
                                foreach (var word in aliasList)
                                {
                                    if (aliasCheck.Any(str => str.Equals(word)))
                                        continue;
                                    aliasCheck.Add(word);
                                    splitName += word + ",";
                                }
                                if (splitName.LastIndexOf(",") != -1)
                                {
                                    streamWriter.WriteLine(c.TermName + "|" + splitName.Substring(0, splitName.LastIndexOf(",")));
                                }
                                else
                                    streamWriter.WriteLine(c.TermName + "|");
                            }
                        }
                        else
                            streamWriter.WriteLine(c.TermName + "|");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("An error occurred while splitting the aliases.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
                else
                    streamWriter.WriteLine(c.TermName + "|");
            }
        }

        public void LoadAliases(string aliasFile = null)
        {
            aliasFile ??= AliasPath;
            if (!File.Exists(aliasFile)) return;

            var aliasesByTermName = _aliasesService.LoadAliases(aliasFile);

            for (var i = 0; i < Terms.Count; i++)
            {
                var t = Terms[i];
                if (aliasesByTermName.ContainsKey(t.TermName))
                {
                    if (t.Aliases.Count > 0)
                    {
                        // If aliases exist (loaded from Goodreads), remove any duplicates and add them in the order from the aliases file
                        // Otherwise, the website would take precedence and that could be bad?
                        foreach (var alias in aliasesByTermName[t.TermName])
                        {
                            if (t.Aliases.Contains(alias))
                            {
                                t.Aliases.Remove(alias);
                                t.Aliases.Add(alias);
                            }
                            else
                                t.Aliases.Add(alias);
                        }
                    }
                    else
                        t.Aliases = new List<string>(aliasesByTermName[t.TermName]);
                    // If first alias is "/c", character searches will be case-sensitive
                    // If it is /d, delete this character
                    // If /n, will not match excerpts but will leave character in X-Ray
                    // If /r, character's aliases (and ONLY the aliases) will be processed as Regular Expressions (case-sensitive unless specified in regex)
                    if (t.Aliases[0] == "/c")
                    {
                        t.MatchCase = true;
                        t.Aliases.Remove("/c");
                    }
                    else if (t.Aliases[0] == "/d")
                    {
                        Terms.Remove(t);
                        i--;
                    }
                    else if (t.Aliases[0] == "/n")
                    {
                        t.Match = false;
                        t.Aliases.Remove("/n");
                    }
                    else if (t.Aliases[0] == "/r")
                    {
                        t.RegexAliases = true;
                        t.Aliases.Remove("/r");
                    }
                }
            }
        }

        public void SaveToFileOld(string path)
        {
            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            streamWriter.Write(ToString());
        }

        public void SaveToFileNew(string path, IProgressBar progress, CancellationToken token)
        {
            SQLiteConnection.CreateFile(path);
            using var m_dbConnection = new SQLiteConnection($"Data Source={path};Version=3;");
            m_dbConnection.Open();
            string sql;
            try
            {
                using var streamReader = new StreamReader(Environment.CurrentDirectory + @"\dist\BaseDB.sql", Encoding.UTF8);
                sql = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            var command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", m_dbConnection);
            _logger.Log("Building new X-Ray database. May take a few minutes...");
            command.ExecuteNonQuery();
            command.Dispose();
            command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", m_dbConnection);
            command.ExecuteNonQuery();
            command.Dispose();
            _logger.Log("Done building initial database. Populating with info from source X-Ray...");
            PopulateDb(m_dbConnection, progress, token);
            _logger.Log("Updating indices...");
            sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                  + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                  + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC); COMMIT;";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        public void SavePreviewToFile(string path)
        {
            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            streamWriter.Write(getPreviewData());
        }
    }
}
