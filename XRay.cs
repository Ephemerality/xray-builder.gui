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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    public class XRay
    {
        private string shelfariURL = "";
        private string xmlFile = "";
        private string databaseName = "";
        private string _guid = "";
        private string asin = "";
        private string version = "1";
        private string aliaspath = "";
        public List<Term> Terms = new List<Term>(100);
        private List<Chapter> _chapters = new List<Chapter>();
        private List<Excerpt> excerpts = new List<Excerpt>();
        private long _srl;
        private long _erl;
        private bool _shortEx = true;
        private bool useSpoilers;
        private bool unattended;
        private bool skipShelfari;
        private int locOffset;
        private List<string[]> notableShelfariQuotes = new List<string[]>();
        private int foundNotables = 0;

        private bool enableEdit = Properties.Settings.Default.enableEdit;
        private frmMain main;

        #region CommonTitles
        string[] CommonTitles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Dr", "Herr", "Monsieur", "Hr", "Frau",
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

        public XRay(string shelfari, frmMain frm, bool useSpoilers = false)
        {
            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "http://" + shelfari;
            this.shelfariURL = shelfari;
            this.useSpoilers = useSpoilers;
            this.main = frm;
        }

        public XRay(string shelfari, string db, string guid, string asin, frmMain frm, bool useSpoilers = false,
            int locOffset = 0, string aliaspath = "", bool unattended = false)
        {
            if (shelfari == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");

            if (!shelfari.ToLower().StartsWith("http://") && !shelfari.ToLower().StartsWith("https://"))
                shelfari = "http://" + shelfari;
            this.shelfariURL = shelfari;
            this.databaseName = db;
            this._guid = guid;
            this.asin = asin;
            this.useSpoilers = useSpoilers;
            this.locOffset = locOffset;
            this.aliaspath = aliaspath;
            this.unattended = unattended;
            this.main = frm;
        }

        public XRay(string xml, string db, string guid, string asin, frmMain frm, bool useSpoilers = false,
            int locOffset = 0, string aliaspath = "")
        {
            if (xml == "" || db == "" || guid == "" || asin == "")
                throw new ArgumentException("Error initializing X-Ray, one of the required parameters was blank.");
            this.xmlFile = xml;
            this.databaseName = db;
            this._guid = guid;
            this.asin = asin;
            this.useSpoilers = useSpoilers;
            this.locOffset = locOffset;
            this.aliaspath = aliaspath;
            this.unattended = false;
            this.main = frm;
            this.skipShelfari = true;
        }

        public int SaveXml(string outfile)
        {
            if (!GetShelfari())
                return 1;

            Functions.Save<List<Term>>(Terms, outfile);
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
                        asin, databaseName, _guid, version, string.Join<Term>(",", Terms),
                        string.Join<Chapter>(",", _chapters), _srl, _erl, xrayversion, date);
            else
            {
                return
                    String.Format(
                        @"{{""asin"":""{0}"",""guid"":""{1}:{2}"",""version"":""{3}"",""xrayversion"":""{5}"",""created"":""{6}"",""terms"":[{4}],""chapters"":[{{""name"":null,""start"":1,""end"":9999999}}]}}",
                        asin, databaseName, _guid, version, string.Join<Term>(",", Terms), xrayversion, date);
            }
        }

        //Add string creation for new XRAY.ASIN.previewData file
        public string getPreviewData()
        {
            string preview = @"{{""numImages"":0,""numTerms"":{0},""previewImages"":""[]"",""excerptIds"":[],""numPeople"":{1}}}";
            preview = String.Format(preview, Terms.Count(t => t.Type == "topic"), Terms.Count(t => t.Type == "character"));
            return preview;
        }

        public string GetXRayName(bool android = false)
        {
            if (android)
                return String.Format("XRAY.{0}.{1}_{2}.db", asin, databaseName, _guid);
            else
                return "XRAY.entities." + asin + ".asc";
        }

        public int CreateXray()
        {
            //Process GUID. If in decimal form, convert to hex.
            if (Regex.IsMatch(_guid, "/[a-zA-Z]/"))
                _guid = _guid.ToUpper();
            else
            {
                long guidDec;
                long.TryParse(_guid, out guidDec);
                _guid = guidDec.ToString("X");
            }
            if (_guid == "0")
            {
                main.Log("Something bad happened while converting the GUID.");
                return 1;
            }

            //Download Shelfari info if not skipping
            if (skipShelfari)
            {
                if (!File.Exists(xmlFile))
                {
                    main.Log("Error opening file (" + xmlFile + ")");
                    return 1;
                }
                main.Log("Loading terms from file...");
                string filetype = Path.GetExtension(xmlFile);
                if (filetype == ".xml")
                    Terms = Functions.DeserializeList<Term>(xmlFile);
                else if (filetype == ".txt")
                {
                    if (LoadTermsFromTxt(xmlFile) > 0)
                    {
                        main.Log("Error loading from text file.");
                        return 1;
                    }
                }
                else
                {
                    main.Log("Bad file type \"" + filetype + "\"");
                    return 1;
                }
                if (Terms == null || Terms.Count == 0) return 1;
            }
            else if (!GetShelfari())
                return 1;

            //Export list of Shelfari characters to a file to make it easier to create aliases or import the modified aliases if they exist
            //Could potentially just attempt to automate the creation of aliases, but in some cases it is very subjective...
            //For example, Shelfari shows the character "Artemis Fowl II", but in the book he is either referred to as "Artemis Fowl", "Artemis", or even "Arty"
            //Other characters have one name on Shelfari but can have completely different names within the book
            string aliasFile;
            if (aliaspath == "")
                aliasFile = Environment.CurrentDirectory + @"\ext\" + asin + ".aliases";
            else
                aliasFile = aliaspath;
            bool aliasesDownloaded = false;
            if ((!File.Exists(aliasFile) || Properties.Settings.Default.overwriteAliases) && Properties.Settings.Default.downloadAliases)
            {
                try
                {
                    string aliases = HttpDownloader.GetPageHtml("https://www.revensoftware.com/xray/aliases/" + asin);
                    StreamWriter fs = new StreamWriter(aliasFile, false, Encoding.UTF8);
                    fs.Write(aliases);
                    fs.Close();
                    main.Log("Found and downloaded pre-made aliases file.");
                    aliasesDownloaded = true;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("(404) Not Found"))
                        main.Log("No pre-made aliases available for this book.");
                    else
                        main.Log("Error downloading aliases: " + ex.Message);
                }
            }

            if (!aliasesDownloaded && (!File.Exists(aliasFile) || Properties.Settings.Default.overwriteAliases))
            {
                SaveCharacters(aliasFile);
                main.Log(String.Format("Characters exported to {0} for adding aliases.", aliasFile));
            }

            if (skipShelfari)
                main.Log(String.Format("{0} Terms found in file:", Terms.Count));
            else
                main.Log(String.Format("{0} Terms found on Shelfari:", Terms.Count));
            string tmp = "";
            int termId = 1;
            foreach (Term t in Terms)
            {
                tmp += t.TermName + ", ";
                t.Id = termId++;
            }
            main.Log(tmp);

            if (!unattended && enableEdit)
            {
                if (DialogResult.Yes ==
                    MessageBox.Show(main,
                        "Terms have been exported to an alias file or already exist in that file. Would you like to open the file in notepad for editing?\r\n"
                        + "See the MobileRead forum thread (link in Settings) for more information on building aliases.",
                        "Aliases",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(aliasFile);
                }
            }
            //Load the aliases now that we know they exist
            if (!File.Exists(aliasFile))
                main.Log("Aliases file not found.");
            else
            {
                LoadAliases(aliasFile);
                main.Log("Character aliases read from " + aliasFile + ".");
            }
            return 0;
        }

        public int ExpandFromRawMl(string rawMl, bool ignoreSoftHypen = false, bool shortEx = true)
        {
            int excerptId = 0;
            this._shortEx = shortEx;
            HtmlAgilityPack.HtmlDocument web = new HtmlAgilityPack.HtmlDocument();
            string readContents;
            using (StreamReader streamReader = new StreamReader(rawMl, Encoding.Default))
            {
                readContents = streamReader.ReadToEnd();
            }
            web.LoadHtml(readContents);
            //Similar to aliases, if chapters definition exists, load it. Otherwise, attempt to build it from the book
            string chapterFile = Environment.CurrentDirectory + @"\ext\" + asin + ".chapters";
            if (File.Exists(chapterFile) && !Properties.Settings.Default.overwriteChapters)
            {
                if (LoadChapters())
                    main.Log(String.Format("Chapters read from {0}.\r\nDelete this file if you want chapters built automatically.", chapterFile));
                else
                    main.Log(String.Format("Failed to read chapters from {0}.\r\nFile is missing or not formatted correctly.", chapterFile));
            }
            else
            {
                try
                {
                    SearchChapters(web, readContents);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message); //Just ignore errors
                }
                //Built chapters list is saved for manual editing
                if (_chapters.Count > 0)
                {
                    SaveChapters();
                    main.Log(String.Format("Chapters exported to {0} for manual editing.", chapterFile));
                }
                else
                    main.Log(
                        String.Format(
                            "No chapters detected.\r\nYou can create a file at {0} if you want to define chapters manually.",
                            chapterFile));
            }

            if (enableEdit)
                if (DialogResult.Yes ==
                    MessageBox.Show("Would you like to open the chapters file in notepad for editing?", "Chapters",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(chapterFile);
                    _chapters.Clear();
                    if (LoadChapters())
                        main.Log("Reloaded chapters from edited file.");
                    else
                        main.Log(
                            String.Format(
                                "Failed to reload chapters from {0}.\r\nFile is missing or not formatted correctly.",
                                chapterFile));
                }

            //If no chapters were found, add a default chapter that spans the entire book
            //Define srl and erl so "progress bar" shows up correctly
            if (_chapters.Count == 0)
            {
                long len = (new FileInfo(rawMl)).Length;
                _chapters.Add(new Chapter("", 1, len));
                _srl = 1;
                _erl = len;
            }
            else
            {
                //Run through all chapters and take the highest value, in case some chapters can be defined in individual chapters and parts.
                //EG. Part 1 includes chapters 1-6, Part 2 includes chapters 7-12.
                _srl = _chapters[0].start;
                main.Log("Found chapters:");
                foreach (Chapter c in _chapters)
                {
                    if (c.End > _erl) _erl = c.End;
                    main.Log(String.Format("{0} | start: {1} | end: {2}", c.name, c.start, c.End));
                }
            }

            main.Log("Scanning book content...");
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            //Iterate over all paragraphs in book
            HtmlNodeCollection nodes = web.DocumentNode.SelectNodes("//p");
            if (nodes == null)
                nodes = web.DocumentNode.SelectNodes("//div[@class='paragraph']");
            if (nodes == null)
                throw new Exception("Could not locate any paragraphs in this book.\r\n" +
                    "Report this error along with a copy of the book to improve parsing.");
            main.prgBar.Maximum = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (main.Exiting) return 1;
                main.prgBar.Value = (i + 1);
                if (((i + 1)%5) == 0) Application.DoEvents();

                HtmlNode node = nodes[i];
                if (node.FirstChild == null) continue; //If the inner HTML is just empty, skip the paragraph!
                int lenQuote = node.InnerHtml.Length;
                int location = node.FirstChild.StreamPosition;
                if (location < 0)
                {
                    main.Log("There was an error locating the paragraph within the book content.");
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
                    List<string> search = character.Aliases.ToList<string>();
                    if (character.RegEx)
                    {
                        if (search.Any(r => Regex.Match(node.InnerText, r).Success)
                            || search.Any(r => Regex.Match(node.InnerHtml, r).Success)
                            || (ignoreSoftHypen && (search.Any(r => Regex.Match(noSoftHypen, r).Success) || search.Any(r => Regex.Match(noSoftHypen, r).Success))))
                            termFound = true;
                    }
                    else
                    {
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
                        // If there is an apostrophe, attempt to match 's at the end of the term
                        // Match end of word, then search for any lingering punctuation
                        string punctuationMarks = @"(?(')'s?|')?\b[!\.?,""'\);]*";
                        //Search html for the matching term out of all aliases
                        foreach (string s in search)
                        {
                            MatchCollection matches = Regex.Matches(node.InnerHtml, @"\b" + s + punctuationMarks, character.MatchCase || character.RegEx ? RegexOptions.None : RegexOptions.IgnoreCase);
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count > 1)
                                {
                                    if (locHighlight.Contains(match.Groups[1].Index)) continue;
                                    locHighlight.Add(match.Groups[1].Index);
                                    lenHighlight.Add(match.Groups[1].Length);
                                }
                                else
                                {
                                    if (locHighlight.Contains(match.Index)) continue;
                                    locHighlight.Add(match.Index);
                                    lenHighlight.Add(match.Length);
                                }
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
                                string pattern;
                                string patternHTML = "(?:<[^>]*>)*";
                                //Match HTML tags -- provided there's nothing malformed
                                string patternSoftHypen = "(\u00C2\u00AD|&shy;|&#173;|&#xad;|&#0173;|&#x00AD;)*";
                                pattern = String.Format("{0}{1}{0}{2}", patternHTML,
                                    string.Join(patternHTML + patternSoftHypen, character.RegEx ? s.ToCharArray() : Regex.Unescape(s).ToCharArray()), punctuationMarks);
                                if (character.MatchCase)
                                    pattern += "(?=[^a-zA-Z])";
                                patterns.Add(pattern);
                                foreach (string pat in patterns)
                                {
                                    MatchCollection matches;
                                    if (character.MatchCase || character.RegEx)
                                        matches = Regex.Matches(node.InnerHtml, pat);
                                    else
                                        matches = Regex.Matches(node.InnerHtml, pat, RegexOptions.IgnoreCase);
                                    foreach (Match match in matches)
                                    {
                                        if (locHighlight.Contains(match.Index)) continue;
                                        locHighlight.Add(match.Index);
                                        lenHighlight.Add(match.Length);
                                    }
                                }
                            }
                        }
                        if (locHighlight.Count == 0 || locHighlight.Count != lenHighlight.Count) //something went wrong
                        {
                            main.Log(
                                String.Format(
                                    "Something went wrong while searching for start of highlight.\nWas looking for (or one of the aliases of): {0}\nSearching in: {1}",
                                    character.TermName, node.InnerHtml));
                            continue;
                        }

                        //If an excerpt is too long, the X-Ray reader cuts it off.
                        //If the location of the highlighted word (character name) within the excerpt is far enough in to get cut off,
                        //this section attempts to shorted the excerpt by locating the start of a sentence that is just far enough away from the highlight.
                        //The length is determined by the space the excerpt takes up rather than its actual length... so 135 is just a guess based on what I've seen.
                        int lengthLimit = 135;
                        if (shortEx && locHighlight[0] + lenHighlight[0] > lengthLimit)
                        {
                            int start = locHighlight[0];
                            int at = 0;
                            long newLoc = -1;
                            int newLenQuote = 0;
                            int newLocHighlight = 0;

                            while ((start > -1) && (at > -1))
                            {
                                at = node.InnerHtml.LastIndexOfAny(new char[] { '.', '?', '!' }, start);
                                if (at > -1)
                                {
                                    start = at - 1;

                                    if ((locHighlight[0] + lenHighlight[0] + 1 - at - 2) <= lengthLimit)
                                    {
                                        newLoc = location + at + 2;
                                        newLenQuote = lenQuote - at - 2;
                                        newLocHighlight = locHighlight[0] - at - 2;
                                        string newQuote = node.InnerHtml.Substring(at + 2);
                                    }
                                    else break;
                                }
                                else break;
                            }
                            //Only add new locs if shorter excerpt was found
                            if (newLoc >= 0)
                            {
                                character.Locs.Add(String.Format("[{0},{1},{2},{3}]", newLoc + locOffset, newLenQuote,
                                    newLocHighlight, lenHighlight));
                                continue;
                            }
                        }

                        for (int j = 0; j < locHighlight.Count; j++)
                        {
                            character.Locs.Add(String.Format("[{0},{1},{2},{3}]", location + locOffset, lenQuote,
                                locHighlight[j], lenHighlight[j])); // For old format
                            character.Occurrences.Add(new int[] { location + locOffset + locHighlight[j], lenHighlight[j] }); // For new format
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
            }

            // Attempt to match any quotes from Shelfari for Notable Clips, not worried if no matches occur as they will be added later anyway
            if (Properties.Settings.Default.useNewVersion)
            {
                foreach (string[] quote in notableShelfariQuotes)
                {
                    int index = readContents.IndexOf(quote[0]);
                    if (index > -1)
                    {
                        Excerpt excerpt = excerpts.FirstOrDefault(e => e.start == index);
                        if (excerpt == null)
                        {
                            excerpt = new Excerpt(excerptId++, index, quote[0].Length);
                            if (quote[1] != "")
                            {
                                Term foundterm = Terms.FirstOrDefault(t => t.TermName == quote[1]);
                                if (foundterm != null)
                                    excerpt.related_entities.Add(foundterm.Id);
                            }
                            excerpts.Add(excerpt);
                        }
                        foundNotables++;
                        excerpt.related_entities.Add(0);
                    }
                }
            }

            timer.Stop();
            main.Log("Scan time: " + timer.Elapsed);
            //output list of terms with no locs
            foreach (Term t in Terms)
            {
                if (t.Match && t.Locs.Count == 0)
                    main.Log(
                        String.Format(
                            "No locations were found for the term \"{0}\".\r\nYou should add aliases for this term using the book or rawml as a reference.",
                            t.TermName));
            }
            return 0;
        }

        /// <summary>
        /// Searches for a Table of Contents within the book and adds the chapters to _chapters.
        /// </summary>
        /// <param name="bookDoc">Book's HTML</param>
        /// <param name="rawML">Path to the book's rawML file</param>
        private void SearchChapters(HtmlAgilityPack.HtmlDocument bookDoc, string rawML)
        {
            string leadingZeros = @"^0+(?=\d)";
            string tocHtml = "";
            HtmlAgilityPack.HtmlDocument tocDoc = new HtmlAgilityPack.HtmlDocument();
            HtmlNodeCollection tocNodes = null;
            HtmlNode toc = bookDoc.DocumentNode.SelectSingleNode(
                    "//reference[translate(@title,'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='TABLE OF CONTENTS']");
            _chapters.Clear();
            //Find table of contents, using case-insensitive search
            if (toc != null)
            {
                int tocloc = Convert.ToInt32(Regex.Replace(toc.GetAttributeValue("filepos", ""), leadingZeros, ""));
                tocHtml = rawML.Substring(tocloc, rawML.IndexOf("<mbp:pagebreak/>", tocloc + 1) - tocloc);
                tocDoc = new HtmlAgilityPack.HtmlDocument();
                tocDoc.LoadHtml(tocHtml);
                tocNodes = tocDoc.DocumentNode.SelectNodes("//a");
                foreach (HtmlNode chapter in tocNodes)
                {
                    if (chapter.InnerHtml == "") continue;
                    int filepos =
                        Convert.ToInt32(Regex.Replace(chapter.GetAttributeValue("filepos", "0"), leadingZeros, ""));
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
                    tocNodes = tocDoc.DocumentNode.SelectNodes("//p");
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
                    Console.WriteLine(ex.Message);
                }
            }

            //Try a broad search for chapterish names just for fun
            if (_chapters.Count == 0)
            {
                string chapterPattern = @"((?:chapter|book|section|part)\s+.*)|((?:prolog|prologue|epilogue)(?:\s+|$).*)|((?:one|two|three|four|five|six|seven|eight|nine|ten)(?:\s+|$).*)";
                IEnumerable<HtmlAgilityPack.HtmlNode> chapterNodes = bookDoc.DocumentNode.SelectNodes("//a").
                    Where(div => div.GetAttributeValue("class", "") == "chapter" || Regex.IsMatch(div.InnerText, chapterPattern, RegexOptions.IgnoreCase));
                foreach (HtmlAgilityPack.HtmlNode chap in chapterNodes)
                {
                    int index = rawML.IndexOf(chap.InnerText);
                    if (index > -1)
                    {
                        if (_chapters.Count > 0)
                            _chapters[_chapters.Count - 1].End = index;
                        _chapters.Add(new Chapter(chap.InnerText, index, rawML.Length));
                    }
                }
            }
        }

        public int PopulateDb(SQLiteConnection db)
        {
            string sql = "";
            int entity = 1;
            int excerpt = 1;
            int personCount = 0;
            int termCount = 0;
            SQLiteCommand command;
            command = new SQLiteCommand(db);
            command.CommandText = "update string set text=@text where id=15";
            command.Parameters.AddWithValue("text", shelfariURL);
            command.ExecuteNonQuery();
            command.Dispose();
            main.Log("Updating database with terms, descriptions, and excerpts...");
            main.prgBar.Maximum = Terms.Count;
            //Write all entities and occurrences
            main.Log(String.Format("Writing {0} terms...", Terms.Count));
            foreach (Term t in Terms)
            {
                if (main.Exiting) return 1;
                main.prgBar.Value = entity++;
                Application.DoEvents();
                command = new SQLiteCommand(db);
                if (t.Type == "character") personCount++;
                else if (t.Type == "topic") termCount++;
                command.CommandText =
                    String.Format(
                        "insert into entity (id, label, loc_label, type, count, has_info_card) values ({0}, @label, null, {1}, {2}, 1);",
                        t.Id, t.Type == "character" ? 1 : 2, t.Occurrences.Count);
                command.Parameters.AddWithValue("label", t.TermName);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new SQLiteCommand(db);
                command.CommandText =
                    String.Format(
                        "insert into entity_description (text, source_wildcard, source, entity) values (@text, @source_wildcard, {0}, {1});",
                        t.DescSrc == "shelfari" ? 2 : 4, t.Id);
                command.Parameters.AddWithValue("text", t.Desc);
                command.Parameters.AddWithValue("source_wildcard", t.TermName);
                command.ExecuteNonQuery();
                command.Dispose();

                sql = "";
                foreach (int[] loc in t.Occurrences)
                    sql += String.Format("insert into occurrence (entity, start, length) values ({0}, {1}, {2});\n",
                        t.Id, loc[0], loc[1]);
                command = new SQLiteCommand(sql, db);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            //Write excerpts and entity_excerpt table
            main.prgBar.Maximum = excerpts.Count;
            main.Log(String.Format("Writing {0} excerpts...", excerpts.Count));
            sql = "";
            command = new SQLiteCommand(db);
            command.CommandText =
                String.Format(
                    "insert into excerpt (id, start, length, image, related_entities, goto) values (@id, @start, @length, @image, @rel_ent, null);");
            foreach (Excerpt e in excerpts)
            {
                if (main.Exiting) return 1;
                main.prgBar.Value = excerpt++;
                Application.DoEvents();
                command.Parameters.AddWithValue("id", e.id);
                command.Parameters.AddWithValue("start", e.start);
                command.Parameters.AddWithValue("length", e.length);
                command.Parameters.AddWithValue("image", e.image);
                command.Parameters.AddWithValue("rel_ent", String.Join(",", e.related_entities.Where(en => en != 0).ToArray())); // don't write 0 (notable)
                command.ExecuteNonQuery();
                foreach (int ent in e.related_entities)
                {
                    sql += String.Format("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", ent, e.id);
                }
            }
            command.Dispose();
            // Populate some more Notable Clips if not enough were found from Shelfari
            // TODO: Add a config value in settings for this
            if (foundNotables + excerpts.Count <= 20)
                excerpts.ForEach(ex => sql += String.Format("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", 0, ex.id));
            else
            {
                Random rand = new Random();
                while (foundNotables <= 20)
                {
                    Excerpt randEx = excerpts.ElementAt(rand.Next(excerpts.Count));
                    sql += String.Format("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", 0, randEx.id);
                    excerpts.Remove(randEx);
                    foundNotables++;
                }
            }
            main.Log("Writing entity excerpt table...");
            command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
            command.Dispose();
            main.prgBar.Value = main.prgBar.Maximum;
            Application.DoEvents();
            main.Log("Writing top mentions...");
            List<int> sorted =
                Terms.Where<Term>(t => t.Type.Equals("character"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList<int>();
            sql = String.Format("update type set top_mentioned_entities='{0}' where id=1;\n",
                String.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            sorted =
                Terms.Where<Term>(t => t.Type.Equals("topic"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList<int>();
            sql += String.Format("update type set top_mentioned_entities='{0}' where id=2;",
                String.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
            command.Dispose();

            main.Log("Writing metadata...");
            
            sql =
                String.Format(
                    "insert into book_metadata (srl, erl, has_images, has_excerpts, show_spoilers_default, num_people, num_terms, num_images, preview_images) "
                    + "values ({0}, {1}, 0, 1, 0, {2}, {3}, 0, null);", _srl, _erl, personCount, termCount);

            command = new SQLiteCommand(sql, db);
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
                        string temp = streamReader.ReadLine().ToLower(); //type
                        lineCount++;
                        if (temp == "") continue;
                        if (temp != "character" && temp != "topic")
                        {
                            main.Log("Invalid term type \"" + temp + "\" on line " + lineCount);
                            return 1;
                        }
                        Term newTerm = new Term();
                        newTerm.Type = temp;
                        newTerm.TermName = streamReader.ReadLine();
                        newTerm.Desc = streamReader.ReadLine();
                        lineCount += 2;
                        newTerm.MatchCase = temp == "character" ? true : false;
                        newTerm.DescSrc = "shelfari";
                        newTerm.Id = termId++;
                        Terms.Add(newTerm);
                    }
                    catch (Exception ex)
                    {
                        main.Log("Failed to read from txt file: " + ex.Message);
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
            public int go_to = -1;

            public Excerpt(int id, int start, int length)
            {
                this.id = id;
                this.start = start;
                this.length = length;
            }

            public string GetQuery()
            {
                string sql =
                    String.Format(
                        "insert into excerpt (id, start, length, image, related_entities, goto) values ({0}, {1}, {2}, {3}, '{4}', {5});\n",
                        id, start, length, image == "" ? "null" : image, String.Join(",", related_entities),
                        go_to == -1 ? "null" : go_to.ToString());
                foreach (int i in related_entities)
                {
                    sql += String.Format("insert into entity_excerpt (entity, excerpt) values ({0}, {1});\n", i, id);
                }
                return sql;
            }
        }

        private class Chapter
        {
            public string name;
            public long start;
            public long End;

            public Chapter()
            {
                this.name = "";
                this.start = 1;
                this.End = 9999999;
            }

            public Chapter(string name, long start, long end)
            {
                this.name = name;
                this.start = start;
                this.End = end;
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

            [XmlIgnore] public List<string> Locs = new List<string>(1000);

            [XmlIgnore] public List<string> Assets = new List<string> {""};

            [XmlIgnore] public int Id = -1;

            [XmlIgnore] public List<int[]> Occurrences = new List<int[]>();

            public bool MatchCase = false;

            public bool Match = true;

            public bool RegEx = false;

            public Term()
            {
            }

            public Term(string type)
            {
                this.Type = type;
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
                    string[] tmp = streamReader.ReadLine().Split('|');
                    if (tmp.Length != 3) return false; //Malformed chapters file
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

            //Try to remove common titles from aliases
            using (var streamWriter = new StreamWriter(aliasFile, false, Encoding.UTF8))
            {
                List<string> aliasCheck = new List<string>();
                foreach (var c in Terms)
                    if (c.Type == "character" && c.TermName.Contains(" "))
                        try
                        {
                        if (Properties.Settings.Default.splitAliases)
                        {
                            string splitName = "";
                            string titleTrimmed = "";
                            string[] words;
                            List<string> aliasList = new List<string>();
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            
                            string pattern = @"( ?(" + string.Join("|", CommonTitles) +
                                ")\\.? )|(^[A-Z]\\. )|( [A-Z]\\.)|(\")|(“)|(”)|(,)";

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
                                titleTrimmed = titleTrimmed.Replace(" &amp;","").Replace(" &", "");
                                words = titleTrimmed.Split(' ');
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
                            main.Log("An error occurred while splitting the aliases.\r\n" + ex.Message);
                        }
                    else
                        streamWriter.WriteLine(c.TermName + "|");
            }
        }

        public void LoadAliases(string aliasFile)
        {
            var d = new Dictionary<string, string[]>();
            if (!File.Exists(aliasFile)) return;
            using (var streamReader = new StreamReader(aliasFile, Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    string input = streamReader.ReadLine();
                    string[] temp = input.Split('|');
                    if (temp.Length <= 1 || temp[0] == "") continue;
                    else if (temp[0].Substring(0, 1) == "#") continue;
                    string[] temp2 = input.Substring(input.IndexOf('|') + 1).Split(',');
                    if (temp2.Length == 0 || temp2[0] == "") continue;
                    if (d.ContainsKey(temp[0]))
                        main.Log("Duplicate alias of " + temp[0] + " found. Ignoring the duplicate.");
                    else
                        d.Add(temp[0], temp2);
                }
            }
            for (int i = 0; i < Terms.Count; i++)
            {
                Term t = Terms[i];
                if (d.ContainsKey(t.TermName))
                {
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
                        t.RegEx = true;
                        t.Aliases.Remove("/r");
                    }
                }
            }
        }

        public bool GetShelfari()
        {
            //Download HTML of Shelfari URL, try 3 times just in case it fails the first time
            main.Log(String.Format("Downloading Shelfari page... {0}", useSpoilers ? "SHOWING SPOILERS!" : ""));
            main.Log(String.Format("Shelfari URL: {0}", shelfariURL));
            var shelfariHtml = "";
            var tries = 3;
            do
            {
                try
                {
                    //Enable cookies
                    var jar = new CookieContainer();
                    var client = new HttpDownloader(shelfariURL, jar, "", "");

                    if (useSpoilers)
                    {
                        //Grab book ID from url (search for 5 digits between slashes) and create spoiler cookie
                        var bookId = Regex.Match(shelfariURL, @"\/\d{5}").Value.Substring(1, 5);
                        var spoilers = new Cookie("ShelfariBookWikiSession", "", "/", "www.shelfari.com")
                        {
                            Value = "{\"SpoilerShowAll\":true%2C\"SpoilerShowCharacters\":true%2C\"SpoilerBookId\":" +
                                    bookId +
                                    "%2C\"SpoilerShowPSS\":true%2C\"SpoilerShowQuotations\":true%2C\"SpoilerShowParents\":true%2C\"SpoilerShowThemes\":true}"
                        };
                        jar.Add(spoilers);
                    }
                    shelfariHtml = client.GetPage();
                    break;
                }
                catch
                {
                    if (tries <= 0)
                    {
                        main.Log("Failed to connect to Shelfari URL.");
                        return false;
                    }
                }
            }
            while (tries-- > 0);

            //Constants for wiki processing
            Dictionary<string, string> sections = new Dictionary<string, string>
            {
                {"WikiModule_Characters", "character"},
                {"WikiModule_Organizations", "topic"},
                {"WikiModule_Settings", "topic"},
                {"WikiModule_Glossary", "topic"}
            }; //, {"WikiModule_Themes", "topic"} };
            string[] patterns = {@""""};
            //, @"\[\d\]", @"\s*?\(.*\)\s*?" }; //Escape quotes, numbers in brackets, and anything within brackets at all
            string[] replacements = {@"\"""};

            //Parse elements from various headers listed in sections
            HtmlAgilityPack.HtmlDocument shelfariDoc = new HtmlAgilityPack.HtmlDocument();
            shelfariDoc.LoadHtml(shelfariHtml);
            foreach (string header in sections.Keys)
            {
                if (!shelfariHtml.Contains(header)) continue; //Skip section if not found on page
                //Select <li> nodes on page from within the <div id=header> tag, under <ul class=li_6>
                HtmlNodeCollection characterNodes =
                    shelfariDoc.DocumentNode.SelectNodes("//div[@id='" + header + "']//ul[@class='li_6']/li");
                foreach (HtmlNode li in characterNodes)
                {
                    string tmpString = li.InnerText;
                    Term newTerm = new Term(sections[header]); //Create term as either character/topic
                    if (tmpString.Contains(":"))
                    {
                        newTerm.TermName = tmpString.Substring(0, tmpString.IndexOf(":"));
                        newTerm.Desc = tmpString.Substring(tmpString.IndexOf(":") + 1).Replace("&amp;", "&").Trim();
                    }
                    else
                    {
                        newTerm.TermName = tmpString;
                    }
                    //newTerm.TermName = newTerm.TermName.PregReplace(patterns, replacements);
                    //newTerm.Desc = newTerm.Desc.PregReplace(patterns, replacements);
                    newTerm.DescSrc = "shelfari";
                    //Use either the associated shelfari URL of the term or if none exists, use the book's url
                    //Could use a wikipedia page instead as the xray plugin/site does but I decided not to
                    newTerm.DescUrl = (li.InnerHtml.IndexOf("<a href") == 0
                        ? li.InnerHtml.Substring(9, li.InnerHtml.IndexOf("\"", 9) - 9)
                        : shelfariURL);
                    if (header == "WikiModule_Glossary")
                        newTerm.MatchCase = false;
                    //Default glossary terms to be case insensitive when searching through book
                    if (Terms.Select<Term, string>(t => t.TermName).Contains<string>(newTerm.TermName))
                        main.Log("Duplicate term \"" + newTerm.TermName + "\" found. Ignoring this duplicate.");
                    else
                        Terms.Add(newTerm);
                }
            }

            // Scrape quotes to attempt matching in ExpandRawML
            if (Properties.Settings.Default.useNewVersion)
            {
                HtmlNodeCollection quoteNodes = shelfariDoc.DocumentNode.SelectNodes("//div[@id='WikiModule_Quotations']/div/ul[@class='li_6']/li");
                if (quoteNodes != null)
                {
                    foreach (HtmlNode quoteNode in quoteNodes)
                    {
                        HtmlNode node = quoteNode.SelectSingleNode(".//blockquote");
                        if (node == null) continue;
                        string quote = node.InnerText;
                        string character = "";
                        node = quoteNode.SelectSingleNode(".//cite");
                        if (node != null)
                            character = node.InnerText;
                        // Remove quotes (sometimes people put unnecessary quotes in the quote as well)
                        quote = Regex.Replace(quote, "^(&ldquo;){1,2}", "");
                        quote = Regex.Replace(quote, "(&rdquo;){1,2}$", "");
                        notableShelfariQuotes.Add(new string[] {quote, character});
                    }
                }
            }
            return true;
        }
    }

    public static class ExtensionMethods
    {
        //http://stackoverflow.com/questions/166855/c-sharp-preg-replace
        public static String PregReplace(this String input, string[] pattern, string[] replacements)
        {
            if (replacements.Length != pattern.Length)
                throw new ArgumentException("Replacement and Pattern Arrays must be balanced");

            for (var i = 0; i < pattern.Length; i++)
            {
                var s = Regex.IsMatch(input, "\"");
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