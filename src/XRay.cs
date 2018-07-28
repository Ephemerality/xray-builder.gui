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
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using XRayBuilderGUI.DataSources;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilderGUI
{
    public class XRay
    {
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
        private bool unattended;
        private bool skipShelfari;
        private int locOffset;
        private List<NotableClip> notableClips;
        private int foundNotables;

        private bool enableEdit = Properties.Settings.Default.enableEdit;
        private readonly DataSource dataSource;

        public delegate DialogResult SafeShowDelegate(string msg, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton def);

        #region CommonTitles
        string[] CommonTitles = new[] { "Mr", "Mrs", "Ms", "Miss", "Dr", "Herr", "Monsieur", "Hr", "Frau",
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

        public XRay()
        {
        }

        public XRay(string shelfari, DataSource dataSource)
        {
            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "https://" + shelfari;
            dataUrl = shelfari;
            this.dataSource = dataSource;
        }

        public XRay(string shelfari, string db, string guid, string asin, DataSource dataSource,
            int locOffset = 0, string aliaspath = "", bool unattended = false)
        {
            if (shelfari == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");

            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "https://" + shelfari;
            dataUrl = shelfari;
            databaseName = db;
            Guid = guid;
            this.asin = asin;
            this.locOffset = locOffset;
            _aliasPath = aliaspath;
            this.unattended = unattended;
            this.dataSource = dataSource;
        }

        public XRay(string xml, string db, string guid, string asin, DataSource dataSource,
            int locOffset = 0, string aliaspath = "")
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
            skipShelfari = true;
        }

        public string AliasPath
        {
            set => _aliasPath = value;
            get => string.IsNullOrEmpty(_aliasPath) ? Environment.CurrentDirectory + @"\ext\" + asin + ".aliases" : _aliasPath;
        }

        public string Guid
        {
            set => Functions.ConvertGuid(value);
            get => _guid;
        }

        public async Task<int> SaveXml(string outfile, IProgressBar progress, CancellationToken token)
        {
            try
            {
                Terms = await dataSource.GetTerms(dataUrl, progress, token);
            }
            catch (OperationCanceledException)
            {
                return 2;
            }
            if (Terms.Count == 0)
                return 1;
            Logger.Log(@"Exporting terms...");
            Functions.Save(Terms, outfile);
            return 0;
        }

        public override string ToString()
        {
            //Insert a version tag of the current program version so you know which version built it.
            //Will be ignored by the Kindle.
            Version dd = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string xrayversion = dd.Major.ToString() + "." + dd.Minor.ToString() + dd.Build.ToString();
            //Insert creation date... seems useful?
            string date = DateTime.Now.ToString("MM/dd/yy HH:mm:ss");
            //If there are no chapters built (someone only ran create X-Ray), just use the default version
            if (_chapters.Count > 0)
                return
                    String.Format(
                        @"{{""asin"":""{0}"",""guid"":""{1}:{2}"",""version"":""{3}"",""xrayversion"":""{8}"",""created"":""{9}"",""terms"":[{4}],""chapters"":[{5}],""assets"":{{}},""srl"":{6},""erl"":{7}}}",
                        asin, databaseName, Guid, version, string.Join(",", Terms),
                        string.Join(",", _chapters), _srl, _erl, xrayversion, date);
            else
            {
                return
                    String.Format(
                        @"{{""asin"":""{0}"",""guid"":""{1}:{2}"",""version"":""{3}"",""xrayversion"":""{5}"",""created"":""{6}"",""terms"":[{4}],""chapters"":[{{""name"":null,""start"":1,""end"":9999999}}]}}",
                        asin, databaseName, Guid, version, string.Join(",", Terms), xrayversion, date);
            }
        }

        //Add string creation for new XRAY.ASIN.previewData file
        public string getPreviewData()
        {
            string preview = @"{{""numImages"":0,""numTerms"":{0},""previewImages"":""[]"",""excerptIds"":[],""numPeople"":{1}}}";
            preview = String.Format(preview, Terms.Count(t => t.Type == "topic"), Terms.Count(t => t.Type == "character"));
            return preview;
        }

        public string XRayName(bool android = false) =>
            android ? $"XRAY.{asin}.{databaseName}_{Guid}.db" : $"XRAY.entities.{asin}.asc";

        // TODO: Completely remove the need to pass in a message box handler
        public async Task<int> CreateXray(IProgressBar progress, CancellationToken token)
        {
            //Download Shelfari info if not skipping
            if (skipShelfari)
            {
                if (!File.Exists(xmlFile))
                {
                    Logger.Log("An error occurred opening file (" + xmlFile + ")");
                    return 1;
                }
                Logger.Log("Loading terms from file...");
                string filetype = Path.GetExtension(xmlFile);
                if (filetype == ".xml")
                    Terms = Functions.DeserializeList<Term>(xmlFile);
                else if (filetype == ".txt")
                {
                    if (LoadTermsFromTxt(xmlFile) > 0)
                    {
                        Logger.Log("An error occurred loading from text file.");
                        return 1;
                    }
                }
                else
                {
                    Logger.Log("Error: Bad file type \"" + filetype + "\"");
                    return 1;
                }
                if (Terms == null || Terms.Count == 0) return 1;
            }
            else
            {
                try
                {
                    Terms = await dataSource.GetTerms(dataUrl, progress, token);
                    Logger.Log("Downloading notable clips...");
                    notableClips = await dataSource.GetNotableClips(dataUrl, token, null, progress);
                }
                catch (OperationCanceledException)
                {
                    return 2;
                }
                if (Terms.Count == 0)
                {
                    Logger.Log("Error: No terms found on " + dataSource.Name + ".");
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
            bool aliasesDownloaded = false;
            // TODO: Review this download process
            //if ((!File.Exists(AliasPath) || Properties.Settings.Default.overwriteAliases) && Properties.Settings.Default.downloadAliases)
            //{
            //    aliasesDownloaded = await AttemptAliasDownload();
            //}

            if (!aliasesDownloaded && (!File.Exists(AliasPath) || Properties.Settings.Default.overwriteAliases))
            {
                SaveCharacters(AliasPath);
                Logger.Log($"Characters exported to {AliasPath} for adding aliases.");
            }

            if (skipShelfari)
                Logger.Log(String.Format("{0} {1} found in file:", Terms.Count, Terms.Count > 1 ? "Terms" : "Term"));
            else
                Logger.Log(String.Format("{0} {1} found on {2}:", Terms.Count, Terms.Count > 1 ? "Terms" : "Term", dataSource.Name));
            StringBuilder str = new StringBuilder(Terms.Count * 32); // Assume that most names will be less than 32 chars
            int termId = 1;
            foreach (Term t in Terms)
            {
                str.Append(t.TermName).Append(", ");
                t.Id = termId++;
            }
            Logger.Log(str.ToString());
        }

        //public async Task<bool> AttemptAliasDownload()
        //{
        //    try
        //    {
        //        string aliases = await HttpDownloader.GetPageHtmlAsync("https://www.revensoftware.com/xray/aliases/" + asin);
        //        StreamWriter fs = new StreamWriter(AliasPath, false, Encoding.UTF8);
        //        fs.Write(aliases);
        //        fs.Close();
        //        Logger.Log("Found and downloaded pre-made aliases file.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!ex.Message.Contains("(404) Not Found"))
        //            Logger.Log("No pre-made aliases available for this book.");
        //        else
        //            Logger.Log("An error occurred downloading aliases: " + ex.Message + "\r\n" + ex.StackTrace);
        //    }

        //    return false;
        //}

        public void HandleChapters(long mlLen, HtmlDocument doc, string rawMl)
        {
            //Similar to aliases, if chapters definition exists, load it. Otherwise, attempt to build it from the book
            string chapterFile = Environment.CurrentDirectory + @"\ext\" + asin + ".chapters";
            if (File.Exists(chapterFile) && !Properties.Settings.Default.overwriteChapters)
            {
                if (LoadChapters())
                    Logger.Log($"Chapters read from {chapterFile}.\r\nDelete this file if you want chapters built automatically.");
                else
                    Logger.Log($"An error occurred reading chapters from {chapterFile}.\r\nFile is missing or not formatted correctly.");
            }
            else
            {
                try
                {
                    SearchChapters(doc, rawMl);
                }
                catch (Exception ex)
                {
                    Logger.Log("Error searching for chapters: " + ex.Message);
                }
                //Built chapters list is saved for manual editing
                if (_chapters.Count > 0)
                {
                    SaveChapters();
                    Logger.Log($"Chapters exported to {chapterFile} for manual editing.");
                }
                else
                    Logger.Log($"No chapters detected.\r\nYou can create a file at {chapterFile} if you want to define chapters manually.");
            }

            if (!unattended && enableEdit)
                if (DialogResult.Yes ==
                    safeShow("Would you like to open the chapters file in notepad for editing?", "Chapters",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(chapterFile);
                    _chapters.Clear();
                    if (LoadChapters())
                        Logger.Log("Reloaded chapters from edited file.");
                    else
                        Logger.Log($"An error occurred reloading chapters from {chapterFile}.\r\nFile is missing or not formatted correctly.");
                }

            //If no chapters were found, add a default chapter that spans the entire book
            //Define srl and erl so "progress bar" shows up correctly
            if (_chapters.Count == 0)
            {
                _chapters.Add(new Chapter("", 1, mlLen));
                _srl = 1;
                _erl = mlLen;
            }
            else
            {
                //Run through all chapters and take the highest value, in case some chapters can be defined in individual chapters and parts.
                //EG. Part 1 includes chapters 1-6, Part 2 includes chapters 7-12.
                _srl = _chapters[0].start;
                Logger.Log("Found chapters:");
                foreach (Chapter c in _chapters)
                {
                    if (c.End > _erl) _erl = c.End;
                    Logger.Log($"{c.name} | start: {c.start} | end: {c.End}");
                }
            }
        }

        public int ExpandFromRawMl(string rawMl, SafeShowDelegate safeShow, IProgressBar progress, CancellationToken token, bool ignoreSoftHypen = false, bool shortEx = true)
        {
            // If there is an apostrophe, attempt to match 's at the end of the term
            // Match end of word, then search for any lingering punctuation
            string apostrophes = Encoding.Default.GetString(Encoding.UTF8.GetBytes("('|\u2019|\u0060|\u00B4)")); // '\u2019\u0060\u00B4
            string quotes = Encoding.Default.GetString(Encoding.UTF8.GetBytes("(\"|\u2018|\u2019|\u201A|\u201B|\u201C|\u201D|\u201E|\u201F)"));
            string dashesEllipsis = Encoding.Default.GetString(Encoding.UTF8.GetBytes("(-|\u2010|\u2011|\u2012|\u2013|\u2014|\u2015|\u2026|&#8211;|&#8212;|&#8217;|&#8218;|&#8230;)")); //U+2010 to U+2015 and U+2026
            string punctuationMarks = String.Format(@"({0}s|{0})?{1}?[!\.?,""\);:]*{0}*{1}*{2}*", apostrophes, quotes, dashesEllipsis);

            int excerptId = 0;
            HtmlDocument web = new HtmlDocument();
            string readContents;
            using (StreamReader streamReader = new StreamReader(rawMl, Encoding.Default))
            {
                readContents = streamReader.ReadToEnd();
            }
            web.LoadHtml(readContents);

            HandleChapters(new FileInfo(rawMl).Length, web, readContents);
            
            Logger.Log("Scanning book content...");
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            //Iterate over all paragraphs in book
            HtmlNodeCollection nodes = web.DocumentNode.SelectNodes("//p")
                ?? web.DocumentNode.SelectNodes("//div[@class='paragraph']")
                ?? web.DocumentNode.SelectNodes("//div[@class='p-indent']");
            if (nodes == null)
            {
                nodes = web.DocumentNode.SelectNodes("//div");
                Logger.Log("Warning: Could not locate paragraphs normally (p elements or divs of class 'paragraph').\r\n" +
                    "Searching all book contents (all divs), which may produce odd results.");
            }
            if (nodes == null)
                throw new Exception("Could not locate any paragraphs in this book.\r\n" +
                    "Report this error along with a copy of the book to improve parsing.");
            progress?.Set(0, nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                HtmlNode node = nodes[i];
                if (node.FirstChild == null) continue; //If the inner HTML is just empty, skip the paragraph!
                int lenQuote = node.InnerHtml.Length;
                int location = node.FirstChild.StreamPosition;
                if (location < 0)
                {
                    Logger.Log("An error occurred locating the paragraph within the book content.");
                    return 1;
                }
                if (location < _srl || location > _erl) continue; //Skip paragraph if outside chapter range
                string noSoftHypen = "";
                if (ignoreSoftHypen)
                {
                    noSoftHypen = node.InnerText;
                    noSoftHypen = noSoftHypen.Replace("\u00C2\u00AD", "");
                    noSoftHypen = noSoftHypen.Replace("&shy;", "");
                    noSoftHypen = noSoftHypen.Replace("&#xad;", "");
                    noSoftHypen = noSoftHypen.Replace("&#173;", "");
                    noSoftHypen = noSoftHypen.Replace("&#0173;", "");
                }
                foreach (Term character in Terms)
                {
                    //Search for character name and aliases in the html-less text. If failed, try in the HTML for rare situations.
                    //TODO: Improve location searching as IndexOf will not work if book length exceeds 2,147,483,647...
                    //If soft hyphen ignoring is turned on, also search hyphen-less text.
                    if (!character.Match) continue;
                    bool termFound = false;
                    List<string> search = new List<string>(character.Aliases.Count);
                    foreach (string alias in character.Aliases)
                    {
                        search.Add(Encoding.Default.GetString(Encoding.UTF8.GetBytes(alias)));
                    }
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
                        List<int> locHighlight = new List<int>();
                        List<int> lenHighlight = new List<int>();
                        //Search html for character name and aliases
                        foreach (string s in search)
                        {
                            MatchCollection matches = Regex.Matches(node.InnerHtml, quotes + @"?\b" + s + punctuationMarks, character.MatchCase || character.RegexAliases ? RegexOptions.None : RegexOptions.IgnoreCase);
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
                            foreach (string s in search)
                            {
                                List<string> patterns = new List<string>();
                                string patternHTML = "(?:<[^>]*>)*";
                                //Match HTML tags -- provided there's nothing malformed
                                string patternSoftHypen = "(\u00C2\u00AD|&shy;|&#173;|&#xad;|&#0173;|&#x00AD;)*";
                                var pattern = String.Format("{0}{1}{0}{2}", patternHTML,
                                    string.Join(patternHTML + patternSoftHypen, character.RegexAliases ? s.ToCharArray() : Regex.Unescape(s).ToCharArray()), punctuationMarks);
                                patterns.Add(pattern);
                                foreach (string pat in patterns)
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
                            Logger.Log(
                                String.Format(
                                    "An error occurred while searching for start of highlight.\r\nWas looking for (or one of the aliases of): {0}\r\nSearching in: {1}",
                                    character.TermName, node.InnerHtml));
                            continue;
                        }

                        //If an excerpt is too long, the X-Ray reader cuts it off.
                        //If the location of the highlighted word (character name) within the excerpt is far enough in to get cut off,
                        //this section attempts to shorted the excerpt by locating the start of a sentence that is just far enough away from the highlight.
                        //The length is determined by the space the excerpt takes up rather than its actual length... so 135 is just a guess based on what I've seen.
                        int lengthLimit = 135;
                        for (int j = 0; j < locHighlight.Count; j++)
                        {
                            if (shortEx && locHighlight[j] + lenHighlight[j] > lengthLimit)
                            {
                                int start = locHighlight[j];
                                int at = 0;
                                long newLoc = -1;
                                int newLenQuote = 0;
                                int newLocHighlight = 0;

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
                                    character.Locs.Add(String.Format("[{0},{1},{2},{3}]", newLoc + locOffset, newLenQuote,
                                        newLocHighlight, lenHighlight[j]));
                                    locHighlight.RemoveAt(j);
                                    lenHighlight.RemoveAt(j--);
                                }
                            }
                        }

                        for (int j = 0; j < locHighlight.Count; j++)
                        {
                            character.Locs.Add(String.Format("[{0},{1},{2},{3}]", location + locOffset, lenQuote,
                                locHighlight[j], lenHighlight[j])); // For old format
                            character.Occurrences.Add(new[] { location + locOffset + locHighlight[j], lenHighlight[j] }); // For new format
                        }
                        List<Excerpt> exCheck = excerpts.Where(t => t.start.Equals(location + locOffset)).ToList();
                        if (exCheck.Count > 0)
                        {
                            if (!exCheck[0].related_entities.Contains(character.Id))
                                exCheck[0].related_entities.Add(character.Id);
                        }
                        else
                        {
                            Excerpt newExcerpt = new Excerpt(excerptId++, location + locOffset, lenQuote);
                            newExcerpt.related_entities.Add(character.Id);
                            excerpts.Add(newExcerpt);
                        }
                    }
                }

                // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                if (Properties.Settings.Default.useNewVersion && notableClips != null)
                {
                    foreach (var quote in notableClips)
                    {
                        int index = node.InnerText.IndexOf(quote.Text, StringComparison.Ordinal);
                        if (index > -1)
                        {
                            // See if an excerpt already exists at this location
                            Excerpt excerpt = excerpts.FirstOrDefault(e => e.start == index);
                            if (excerpt == null)
                            {
                                excerpt = new Excerpt(excerptId++, index, quote.Text.Length);
                                excerpt.related_entities.Add(0); // Mark the excerpt as notable
                                                                 // TODO: also add other related entities
                                excerpt.notable = true;
                                excerpt.highlights = quote.Likes;
                                excerpts.Add(excerpt);
                            }
                            else
                                excerpt.related_entities.Add(0);
                            foundNotables++;
                        }
                    }
                }
                progress?.Add(1);
            }

            timer.Stop();
            Logger.Log("Scan time: " + timer.Elapsed);
            //output list of terms with no locs
            foreach (Term t in Terms)
            {
                if (t.Match && t.Locs.Count == 0)
                    Logger.Log($"No locations were found for the term \"{t.TermName}\".\r\nYou should add aliases for this term using the book or rawml as a reference.");
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
            HtmlDocument tocDoc = new HtmlDocument();
            HtmlNode toc = bookDoc.DocumentNode.SelectSingleNode(
                    "//reference[translate(@title,'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='TABLE OF CONTENTS']");
            _chapters.Clear();
            //Find table of contents, using case-insensitive search
            if (toc != null)
            {
                int tocloc = Convert.ToInt32(leadingZerosRegex.Replace(toc.GetAttributeValue("filepos", ""), ""));
                tocHtml = rawML.Substring(tocloc, rawML.IndexOf("<mbp:pagebreak/>", tocloc + 1, StringComparison.Ordinal) - tocloc);
                tocDoc = new HtmlDocument();
                tocDoc.LoadHtml(tocHtml);
                var tocNodes = tocDoc.DocumentNode.SelectNodes("//a");
                foreach (HtmlNode chapter in tocNodes)
                {
                    if (chapter.InnerHtml == "") continue;
                    int filepos = Convert.ToInt32(leadingZerosRegex.Replace(chapter.GetAttributeValue("filepos", "0"), ""));
                    if (_chapters.Count > 0)
                    {
                        _chapters[_chapters.Count - 1].End = filepos;
                        if (_chapters[_chapters.Count - 1].start > filepos)
                            _chapters.RemoveAt(_chapters.Count - 1); //remove broken chapters
                    }
                    _chapters.Add(new Chapter(chapter.InnerText, filepos, rawML.Length));
                }
            }

            //Search again, looking for Calibre's 'new' mobi ToC format
            if (_chapters.Count == 0)
            {
                try
                {
                    int index = rawML.LastIndexOf(">Table of Contents<");
                    index = rawML.IndexOf("<p ", index);
                    int breakIndex = rawML.IndexOf("<div class=\"mbp_pagebreak\"", index);
                    if (breakIndex == -1)
                        breakIndex = rawML.IndexOf("div class=\"mbppagebreak\"", index);
                    tocHtml = rawML.Substring(index, breakIndex - index);
                    tocDoc.LoadHtml(tocHtml);
                    var tocNodes = tocDoc.DocumentNode.SelectNodes("//p");
                    // Search for each chapter heading, ignore any misses (user can go and add any that are missing if necessary)
                    foreach (HtmlNode chap in tocNodes)
                    {
                        index = rawML.IndexOf(chap.InnerText);
                        if (index > -1)
                        {
                            if (_chapters.Count > 0)
                                _chapters[_chapters.Count - 1].End = index;
                            _chapters.Add(new Chapter(chap.InnerText, index, rawML.Length));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error searching for Calibre chapters: " + ex.Message);
                }
            }

            //Try a broad search for chapterish names just for fun
            if (_chapters.Count == 0)
            {
                // TODO: Expand on the chapter matching pattern concept
                const string chapterPattern = @"((?:chapter|book|section|part|capitulo)\s+.*)|((?:prolog|prologue|epilogue)(?:\s+|$).*)|((?:one|two|three|four|five|six|seven|eight|nine|ten)(?:\s+|$).*)";
                const string xpath = "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5]";
                var chapterNodes = bookDoc.DocumentNode.SelectNodes("//a")
                    .Where(div => div.GetAttributeValue("class", "") == "chapter" || Regex.IsMatch(div.InnerText, chapterPattern, RegexOptions.IgnoreCase))
                    .ToList();
                var headingNodes = bookDoc.DocumentNode.SelectNodes(xpath).ToList();
                if (headingNodes.Count > chapterNodes.Count)
                    chapterNodes = headingNodes;
                foreach (HtmlNode chap in chapterNodes)
                {
                    if (chap.InnerText.ContainsIgnorecase("Table of Contents")) continue;
                    int index = rawML.IndexOf(chap.InnerHtml) + chap.InnerHtml.IndexOf(chap.InnerText);
                    if (index > -1)
                    {
                        if (_chapters.Count > 0)
                            _chapters[_chapters.Count - 1].End = index;
                        _chapters.Add(new Chapter(chap.InnerText, index, rawML.Length));
                    }
                }
            }
        }

        // TODO: Make async
        public int PopulateDb(SQLiteConnection db, IProgressBar progress, CancellationToken token)
        {
            StringBuilder sql = new StringBuilder(Terms.Count * 256);
            int personCount = 0;
            int termCount = 0;
            SQLiteCommand command = new SQLiteCommand("update string set text=@text where id=15", db);
            command.Parameters.AddWithValue("text", dataUrl);
            command.ExecuteNonQuery();
            command.Dispose();
            Logger.Log("Updating database with terms, descriptions, and excerpts...");
            //Write all entities and occurrences
            Logger.Log($"Writing {Terms.Count} terms...");
            progress?.Set(0, Terms.Count);
            foreach (Term t in Terms)
            {
                token.ThrowIfCancellationRequested();
                if (t.Type == "character") personCount++;
                else if (t.Type == "topic") termCount++;
                command = new SQLiteCommand(String.Format("insert into entity (id, label, loc_label, type, count, has_info_card) values ({0}, @label, null, {1}, {2}, 1);",
                    t.Id, t.Type == "character" ? 1 : 2, t.Occurrences.Count), db);
                command.Parameters.AddWithValue("label", t.TermName);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new SQLiteCommand(String.Format("insert into entity_description (text, source_wildcard, source, entity) values (@text, @source_wildcard, {0}, {1});",
                    t.DescSrc == "shelfari" ? 2 : 4, t.Id), db);
                command.Parameters.AddWithValue("text", t.Desc == "" ? "No description available." : t.Desc);
                command.Parameters.AddWithValue("source_wildcard", t.TermName);
                command.ExecuteNonQuery();
                command.Dispose();

                sql.Clear();
                foreach (int[] loc in t.Occurrences)
                    sql.AppendFormat("insert into occurrence (entity, start, length) values ({0}, {1}, {2});\n",
                        t.Id, loc[0], loc[1]);
                command = new SQLiteCommand(sql.ToString(), db);
                command.ExecuteNonQuery();
                command.Dispose();
                progress?.Add(1);
            }
            //Write excerpts and entity_excerpt table
            Logger.Log(String.Format("Writing {0} excerpts...", excerpts.Count));
            sql.Clear();
            command = new SQLiteCommand("insert into excerpt (id, start, length, image, related_entities, goto) values (@id, @start, @length, @image, @rel_ent, null);", db);
            progress?.Set(0, excerpts.Count);
            foreach (Excerpt e in excerpts)
            {
                token.ThrowIfCancellationRequested();
                command.Parameters.AddWithValue("id", e.id);
                command.Parameters.AddWithValue("start", e.start);
                command.Parameters.AddWithValue("length", e.length);
                command.Parameters.AddWithValue("image", e.image);
                command.Parameters.AddWithValue("rel_ent", String.Join(",", e.related_entities.Where(en => en != 0).ToArray())); // don't write 0 (notable flag)
                command.ExecuteNonQuery();
                foreach (int ent in e.related_entities)
                {
                    if (ent != 0) // skip notable flag
                        sql.AppendFormat("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", ent, e.id);
                }
                progress?.Add(1);
            }
            command.Dispose();
            // create links to notable clips in order of popularity
            var notablesOnly = excerpts.Where(ex => ex.notable).OrderByDescending(ex => ex.highlights);
            foreach (Excerpt notable in notablesOnly)
                sql.AppendFormat("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", 0, notable.id);
            // Populate some more notable clips if not enough were found, 
            // TODO: Add a config value in settings for this amount
            if (foundNotables <= 20 && foundNotables + excerpts.Count <= 20)
                excerpts.ForEach(ex =>
                    {
                        if (!ex.notable)
                            sql.AppendFormat("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", 0, ex.id);
                    });
            else if (foundNotables <= 20)
            {
                Random rand = new Random();
                List<Excerpt> eligible = excerpts.Where(ex => !ex.notable).ToList();
                while (foundNotables <= 20 && eligible.Count > 0)
                {
                    Excerpt randEx = eligible.ElementAt(rand.Next(eligible.Count));
                    sql.AppendFormat("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", 0, randEx.id);
                    eligible.Remove(randEx);
                    foundNotables++;
                }
            }
            token.ThrowIfCancellationRequested();
            Logger.Log("Writing entity excerpt table...");
            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();
            token.ThrowIfCancellationRequested();
            Logger.Log("Writing top mentions...");
            List<int> sorted =
                Terms.Where(t => t.Type.Equals("character"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList();
            sql.Clear();
            sql.AppendFormat("update type set top_mentioned_entities='{0}' where id=1;\n",
                String.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            sorted =
                Terms.Where(t => t.Type.Equals("topic"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList();
            sql.AppendFormat("update type set top_mentioned_entities='{0}' where id=2;",
                String.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();

            token.ThrowIfCancellationRequested();
            Logger.Log("Writing metadata...");

            sql.Clear();
            sql.AppendFormat(
                "insert into book_metadata (srl, erl, has_images, has_excerpts, show_spoilers_default, num_people, num_terms, num_images, preview_images) "
                + "values ({0}, {1}, 0, 1, 0, {2}, {3}, 0, null);", _srl, _erl, personCount, termCount);

            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();
            return 0;
        }

        private int LoadTermsFromTxt(string txtfile)
        {
            if (!File.Exists(txtfile)) return 1;
            using (StreamReader streamReader = new StreamReader(txtfile, Encoding.UTF8))
            {
                int termId = 1;
                int lineCount = 1;
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
                            Logger.Log("Error: Invalid term type \"" + temp + "\" on line " + lineCount);
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
                        Logger.Log("An error occurred reading from txt file: " + ex.Message + "\r\n" + ex.StackTrace);
                        return 1;
                    }
                }
            }
            return 0;
        }

        private class Excerpt
        {
            public int id;
            public int start;
            public int length;
            public string image = "";
            public List<int> related_entities = new List<int>();
            //public int go_to = -1; unused but in the db
            public int highlights;
            public bool notable;

            public Excerpt(int id, int start, int length)
            {
                this.id = id;
                this.start = start;
                this.length = length;
            }
        }

        public class Chapter
        {
            public string name;
            public long start;
            public long End;

            public Chapter()
            {
                name = "";
                start = 1;
                End = 9999999;
            }

            public Chapter(string name, long start, long end)
            {
                this.name = name;
                this.start = start;
                End = end;
            }

            public override string ToString()
            {
                return String.Format(@"{{""name"":{0},""start"":{1},""end"":{2}}}",
                    (name == "" ? "null" : "\"" + name + "\""), start, End);
            }
        }

        public class Term
        {
            public string Type = "";

            [XmlElement("name")] public string TermName = "";

            public string Desc = "";

            [XmlElement("src")] public string DescSrc = "";

            [XmlElement("url")] public string DescUrl = "";

            [XmlIgnore] public List<string> Aliases = new List<string>();

            [JsonIgnore]
            [XmlIgnore]
            public List<string> Locs = new List<string>();

            [XmlIgnore] public List<string> Assets = new List<string> { "" };

            [XmlIgnore] public int Id = -1;

            [XmlIgnore] public List<int[]> Occurrences = new List<int[]>();

            public bool MatchCase;

            public bool Match = true;

            /// <summary>
            /// Determines if the aliases are in Regex format
            /// </summary>
            public bool RegexAliases;

            public Term()
            {
            }

            public Term(string type)
            {
                Type = type;
            }

            public override string ToString()
            {
                //Note that the Amazon X-Ray files declare an "assets" var for each term, but I have not seen one that actually uses them to contain anything
                if (Locs.Count > 0)
                    return
                        String.Format(
                            @"{{""type"":""{0}"",""term"":""{1}"",""desc"":""{2}"",""descSrc"":""{3}"",""descUrl"":""{4}"",""locs"":[{5}]}}",
                            Type, TermName, Desc, DescSrc, DescUrl, string.Join(",", Locs));
                else
                {
                    return
                        String.Format(
                            @"{{""type"":""{0}"",""term"":""{1}"",""desc"":""{2}"",""descSrc"":""{3}"",""descUrl"":""{4}"",""locs"":[[100,100,100,6]]}}",
                            Type, TermName, Desc, DescSrc, DescUrl);
                }
            }
        }

        public void SaveChapters()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\ext\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\ext\");
            using (
                StreamWriter streamWriter =
                    new StreamWriter(Environment.CurrentDirectory + @"\ext\" + asin + ".chapters", false,
                        Encoding.UTF8))
            {
                foreach (Chapter c in _chapters)
                    streamWriter.WriteLine(c.name + "|" + c.start + "|" + c.End);
            }
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
                    string[] tmp = streamReader.ReadLine()?.Split('|');
                    if (tmp?.Length != 3) return false; //Malformed chapters file
                    if (tmp[0] == "" || tmp[0].Substring(0, 1) == "#") continue;
                    _chapters.Add(new Chapter(tmp[0], Convert.ToInt32(tmp[1]), Convert.ToInt64(tmp[2])));
                }
            }
            return true;
        }

        public void SaveCharacters(string aliasFile)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\ext\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\ext\");

            //Try to load custom common titles from BaseSplitIgnore.txt
            try
            {
                using (StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + @"\dist\BaseSplitIgnore.txt", Encoding.UTF8))
                {
                    var CustomSplitIgnore = streamReader.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.None)
                        .Where(r => !r.StartsWith("//")).ToArray();
                    if (CustomSplitIgnore.Length >= 1)
                    {
                        CommonTitles = CustomSplitIgnore;
                    }
                    Logger.Log("Splitting aliases using custom common titles file...");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while opening the BaseSplitIgnore.txt file.\r\n" +
                    "Ensure you extracted it to the same directory as the program.\r\n" +
                    ex.Message + "\r\nUsing built in default terms...");
            }

            //Try to remove common titles from aliases
            using (var streamWriter = new StreamWriter(aliasFile, false, Encoding.UTF8))
            {
                List<string> aliasCheck = new List<string>();
                foreach (var c in Terms)
                {
                    if (c.Type == "character" && c.TermName.Contains(" "))
                    {
                        try
                        {
                            if (Properties.Settings.Default.splitAliases)
                            {
                                string splitName = "";
                                string titleTrimmed;
                                List<string> aliasList = new List<string>();
                                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                                string pattern = @"( ?(" + string.Join("|", CommonTitles) +
                                    ")\\.? )|(^[A-Z]\\. )|( [A-Z]\\.)|(\")|(\u201C)|(\u201D)|(,)|(')";

                                Regex regex = new Regex(pattern);
                                Match matchCheck = Regex.Match(c.TermName, pattern);
                                if (matchCheck.Success)
                                {
                                    titleTrimmed = c.TermName;
                                    foreach (Match match in regex.Matches(titleTrimmed))
                                    {
                                        titleTrimmed = titleTrimmed.Replace(match.Value, String.Empty);
                                    }
                                    foreach (Match match in regex.Matches(titleTrimmed))
                                    {
                                        titleTrimmed = titleTrimmed.Replace(match.Value, String.Empty);
                                    }
                                    aliasList.Add(titleTrimmed);
                                }
                                else
                                    titleTrimmed = c.TermName;

                                titleTrimmed = Regex.Replace(titleTrimmed, @"\s+", " ");
                                titleTrimmed = Regex.Replace(titleTrimmed, @"( ?V?I{0,3}$)", String.Empty);
                                titleTrimmed = Regex.Replace(titleTrimmed, @"(\(aka )", "(");

                                Match bracketedName = Regex.Match(titleTrimmed, @"(.*)(\()(.*)(\))");
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
                                    foreach (string word in words)
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
                                    foreach (string word in aliasList)
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
                            Logger.Log("An error occurred while splitting the aliases.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }
                    else
                        streamWriter.WriteLine(c.TermName + "|");
                }
            }
        }

        public void LoadAliases(string aliasFile = null)
        {
            var d = new Dictionary<string, string[]>();
            aliasFile = aliasFile ?? AliasPath;
            if (!File.Exists(aliasFile)) return;
            using (var streamReader = new StreamReader(aliasFile, Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    string input = streamReader.ReadLine();
                    string[] temp = input?.Split('|');
                    if (temp == null || temp.Length <= 1 || temp[0] == "" || temp[0].StartsWith("#")) continue;
                    string[] temp2 = input.Substring(input.IndexOf('|') + 1).Split(',');
                    //Check for misplaced pipe character in aliases
                    if (temp2[0] != "" && temp2.Any(r => Regex.Match(@"\|", r).Success))
                    {
                        Logger.Log("An error occurred parsing the alias file. Ignoring term: " + temp[0] + " aliases.\r\nCheck the file is in the correct format: Character Name|Alias1,Alias2,Etc");
                        continue;
                    }
                    if (temp2.Length == 0 || temp2[0] == "") continue;
                    if (d.ContainsKey(temp[0]))
                        Logger.Log("Duplicate alias of " + temp[0] + " found. Ignoring the duplicate.");
                    else
                        d.Add(temp[0], temp2);
                }
            }
            for (int i = 0; i < Terms.Count; i++)
            {
                Term t = Terms[i];
                if (d.ContainsKey(t.TermName))
                {
                    if (t.Aliases.Count > 0)
                    {
                        // If aliases exist (loaded from Goodreads), remove any duplicates and add them in the order from the aliases file
                        // Otherwise, the website would take precedence and that could be bad?
                        foreach (string alias in d[t.TermName])
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
                        t.Aliases = new List<string>(d[t.TermName]);
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
            using (var streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                streamWriter.Write(ToString());
        }

        public void SaveToFileNew(string path, IProgressBar progress, CancellationToken token)
        {
            SQLiteConnection.CreateFile(path);
            using (SQLiteConnection m_dbConnection = new SQLiteConnection($"Data Source={path};Version=3;"))
            {
                m_dbConnection.Open();
                string sql;
                try
                {
                    using (StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + @"\dist\BaseDB.sql", Encoding.UTF8))
                    {
                        sql = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    throw new IOException("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
                SQLiteCommand command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", m_dbConnection);
                Logger.Log("Building new X-Ray database. May take a few minutes...");
                command.ExecuteNonQuery();
                command.Dispose();
                command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", m_dbConnection);
                command.ExecuteNonQuery();
                command.Dispose();
                Logger.Log("Done building initial database. Populating with info from source X-Ray...");
                try
                {
                    PopulateDb(m_dbConnection, progress, token);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                        Logger.Log("Building canceled.");
                    else
                        throw;
                }
                Logger.Log("Updating indices...");
                sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                      + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                      + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC); COMMIT;";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        public void SavePreviewToFile(string path)
        {
            using (var streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                streamWriter.Write(getPreviewData());
        }
    }

    public class XRayJsonDef
    {
        public string asin;
        public string guid;
        public string version;
        public string xrayversion;
        public string created;
        public List<XRay.Term> terms;
        public IList<XRay.Chapter> chapters;
        public string assets;
        public int srl;
        public int erl;
    }

    public static partial class ExtensionMethods
    {
        //http://stackoverflow.com/questions/166855/c-sharp-preg-replace
        public static string PregReplace(this string input, string[] pattern, string[] replacements)
        {
            if (replacements.Length != pattern.Length)
                throw new ArgumentException("Replacement and pattern arrays must be balanced");

            for (var i = 0; i < pattern.Length; i++)
            {
                input = Regex.Replace(input, pattern[i], replacements[i]);
            }
            return input;
        }

        //http://stackoverflow.com/questions/444798/case-insensitive-containsstring
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsIgnorecase(this string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
