using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JetBrains.Annotations;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Extensions;
using XRayBuilder.Core.Libraries.Prompt;
using XRayBuilder.Core.Localization.Core;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.XRay.Artifacts;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace XRayBuilder.Core.XRay.Logic.Chapters
{
    public class ChaptersService
    {
        private readonly ILogger _logger;
        private readonly IXRayBuilderConfig _config;

        public ChaptersService(ILogger logger, IXRayBuilderConfig config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Read the chapters or search for them and apply them to the given <param name="xray"></param>
        /// </summary>
        public void HandleChapters(XRay xray, string asin, long mlLen, HtmlDocument doc, string rawMl, [CanBeNull] YesNoPrompt yesNoPrompt, [CanBeNull] EditCallback editCallback)
        {
            //Similar to aliases, if chapters definition exists, load it. Otherwise, attempt to build it from the book
            // todo directory service
            var chapterFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext", $"{asin}.chapters");
            if (File.Exists(chapterFile) && !_config.OverwriteChapters)
            {
                try
                {
                    xray.Chapters = LoadChapters(asin).ToList();
                    _logger.Log($"Chapters read from {chapterFile}.\r\nDelete this file if you want chapters built automatically.");
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred reading chapters from {chapterFile}: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    var chapters = SearchChapters(doc, rawMl);
                    if (chapters != null)
                        xray.Chapters = chapters.ToList();
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error searching for chapters: {ex.Message}");
                }
                //Built chapters list is saved for manual editing
                if (xray.Chapters.Count > 0)
                {
                    SaveChapters(xray);
                    _logger.Log($"Chapters exported to {chapterFile} for manual editing.");
                }
                else
                    _logger.Log($"No chapters detected.\r\nYou can create a file at {chapterFile} if you want to define chapters manually.");
            }

            // TODO Get rid of Unattended from XRay
            if (!xray.Unattended && _config.EnableEdit && yesNoPrompt != null && editCallback != null && yesNoPrompt(CoreStrings.Chapters, CoreStrings.OpenChaptersFile, PromptType.Question) == PromptResultYesNo.Yes && editCallback(chapterFile))
            {
                xray.Chapters.Clear();

                try
                {
                    xray.Chapters = LoadChapters(asin).ToList();
                    _logger.Log("Reloaded chapters from edited file.");
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred reading chapters from {chapterFile}: {ex.Message}");
                }
            }

            //If no chapters were found, add a default chapter that spans the entire book
            //Define srl and erl so "progress bar" shows up correctly
            if (xray.Chapters.Count == 0)
            {
                xray.Chapters.Add(new Chapter
                {
                    Name = "",
                    Start = 1,
                    End = mlLen
                });
                xray.Srl = 1;
                xray.Erl = mlLen;
            }
            else
            {
                //Run through all chapters and take the highest value, in case some chapters can be defined in individual chapters and parts.
                //EG. Part 1 includes chapters 1-6, Part 2 includes chapters 7-12.
                xray.Srl = xray.Chapters[0].Start;
                _logger.Log("Found chapters:");
                foreach (var c in xray.Chapters)
                {
                    if (c.End > xray.Erl)
                        xray.Erl = c.End;
                    _logger.Log($"{c.Name} | start: {c.Start} | end: {c.End}");
                }
            }
        }

        private void SaveChapters(XRay xray)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using var streamWriter = new StreamWriter(Path.Combine(path, $"{xray.Asin}.chapters"), false, Encoding.UTF8);
            foreach (var chapter in xray.Chapters)
                streamWriter.WriteLine($"{chapter.Name}|{chapter.Start}|{chapter.End}");
        }

        private IEnumerable<Chapter> LoadChapters(string asin)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext", $"{asin}.chapters");
            if (!File.Exists(path))
                throw new Exception($"Chapters file does not exist: {path}");
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            while (!streamReader.EndOfStream)
            {
                var tmp = streamReader.ReadLine()?.Split('|');
                if (tmp?.Length != 3)
                    throw new Exception("Malformed chapters file");
                if (tmp[0] == "" || tmp[0].Substring(0, 1) == "#")
                    continue;
                yield return new Chapter
                {
                    Name = tmp[0],
                    Start = Convert.ToInt32(tmp[1]),
                    End = Convert.ToInt64(tmp[2])
                };
            }
        }

        /// <summary>
        /// Searches for a Table of Contents within the book
        /// </summary>
        /// <param name="bookDoc">Book's HTML</param>
        /// <param name="rawMl">Path to the book's rawML file</param>
        // TODO Split out different chapter-finders
        [CanBeNull]
        private IEnumerable<Chapter> SearchChapters(HtmlDocument bookDoc, string rawMl)
        {
            var chapters = new List<Chapter>();

            var leadingZerosRegex = new Regex(@"^0+(?=\d)", RegexOptions.Compiled);
            string tocHtml;
            var tocDoc = new HtmlDocument();
            var toc = bookDoc.DocumentNode.SelectSingleNode(
                    "//reference[translate(@title,'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='TABLE OF CONTENTS']");

            //Find table of contents, using case-insensitive search
            if (toc != null)
            {
                var tocloc = Convert.ToInt32(leadingZerosRegex.Replace(toc.GetAttributeValue("filepos", ""), ""));
                tocHtml = rawMl.Substring(tocloc, rawMl.IndexOf("<mbp:pagebreak/>", tocloc + 1, StringComparison.Ordinal) - tocloc);
                tocDoc = new HtmlDocument();
                tocDoc.LoadHtml(tocHtml);
                var tocNodes = tocDoc.DocumentNode.SelectNodes("//a");
                foreach (var chapter in tocNodes)
                {
                    if (chapter.InnerHtml == "")
                        continue;
                    var filepos = Convert.ToInt32(leadingZerosRegex.Replace(chapter.GetAttributeValue("filepos", "0"), ""));
                    if (chapters.Count > 0)
                    {
                        chapters[chapters.Count - 1].End = filepos;
                        if (chapters[chapters.Count - 1].Start > filepos)
                            chapters.RemoveAt(chapters.Count - 1); //remove broken chapters
                    }
                    chapters.Add(new Chapter
                    {
                        Name = chapter.InnerText,
                        Start = filepos,
                        End = rawMl.Length
                    });
                }
            }

            //Search again, looking for Calibre's 'new' mobi ToC format
            if (chapters.Count == 0)
            {
                try
                {
                    var index = rawMl.LastIndexOf(">Table of Contents<", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        index = rawMl.IndexOf("<p ", index, StringComparison.Ordinal);
                        var breakIndex = rawMl.IndexOf("<div class=\"mbp_pagebreak\"", index, StringComparison.Ordinal);
                        if (breakIndex == -1)
                            breakIndex = rawMl.IndexOf("div class=\"mbppagebreak\"", index, StringComparison.Ordinal);
                        tocHtml = rawMl.Substring(index, breakIndex - index);
                        tocDoc.LoadHtml(tocHtml);
                        var tocNodes = tocDoc.DocumentNode.SelectNodes("//p");
                        // Search for each chapter heading, ignore any misses (user can go and add any that are missing if necessary)
                        foreach (var chap in tocNodes)
                        {
                            index = rawMl.IndexOf(chap.InnerText, StringComparison.Ordinal);
                            if (index > -1)
                            {
                                if (chapters.Count > 0)
                                    chapters[chapters.Count - 1].End = index;
                                chapters.Add(new Chapter
                                {
                                    Name = chap.InnerText,
                                    Start = index,
                                    End = rawMl.Length
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error searching for Calibre chapters: {ex.Message}");
                }
            }

            // Try searching for Calibre's toc2 nodes
            if (chapters.Count == 0)
            {
                var tocNodes = bookDoc.DocumentNode.SelectNodes("//p[@class='toc2']")?.ToArray() ?? new HtmlNode[0];
                foreach (var node in tocNodes)
                {
                    var position = node.StreamPosition;
                    if (chapters.Count > 0)
                        chapters[chapters.Count - 1].End = position;
                    chapters.Add(new Chapter
                    {
                        Name = node.InnerText,
                        Start = position,
                        End = rawMl.Length
                    });
                }
            }

            //Try a broad search for chapterish names just for fun
            if (chapters.Count == 0)
            {
                // TODO: Expand on the chapter matching pattern concept
                const string chapterPattern = @"((?:chapter|book|section|part|capitulo)\s+.*)|((?:prolog|prologue|epilogue)(?:\s+|$).*)|((?:one|two|three|four|five|six|seven|eight|nine|ten)(?:\s+|$).*)";
                const string xpath = "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5]";
                var chapterNodes = bookDoc.DocumentNode.SelectNodes("//a")
                    ?.Where(div => div.GetAttributeValue("class", "") == "chapter" || Regex.IsMatch(div.InnerText, chapterPattern, RegexOptions.IgnoreCase))
                    .ToList();
                if (chapterNodes == null)
                    return null;
                var headingNodes = bookDoc.DocumentNode.SelectNodes(xpath).ToList();
                if (headingNodes.Count > chapterNodes.Count)
                    chapterNodes = headingNodes;
                foreach (var chap in chapterNodes)
                {
                    if (chap.InnerText.ContainsIgnorecase("Table of Contents"))
                        continue;
                    var index = rawMl.IndexOf(chap.InnerHtml, StringComparison.Ordinal) + chap.InnerHtml.IndexOf(chap.InnerText, StringComparison.Ordinal);
                    if (index > -1)
                    {
                        if (chapters.Count > 0)
                            chapters[chapters.Count - 1].End = index;
                        chapters.Add(new Chapter
                        {
                            Name = chap.InnerText,
                            Start = index,
                            End = rawMl.Length
                        });
                    }
                }
            }

            return chapters;
        }
    }
}