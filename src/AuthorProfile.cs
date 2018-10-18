using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace XRayBuilderGUI
{
    public class AuthorProfile
    {
        public string ApTitle;
        public string ApSubTitle;
        public string BioTrimmed = "";
        public List<BookInfo> otherBooks = new List<BookInfo>();
        public string authorImageUrl = "";
        public string authorAsin = "";

        public string EaSubTitle;

        private readonly BookInfo _curBook;
        private readonly Settings _settings;

        public AuthorProfile(BookInfo curBook, Settings settings)
        {
            _curBook = curBook;
            _settings = settings;
        }

        // TODO: Review this...
        public async Task<bool> Generate()
        {
            string outputDir;
            try
            {
                if (_settings.Android)
                {
                    outputDir = _settings.OutDir + @"\Android\" + _curBook.asin;
                    Directory.CreateDirectory(outputDir);
                }
                else
                    outputDir = _settings.UseSubDirectories ? Functions.GetBookOutputDirectory(_curBook.author, _curBook.sidecarName, true) : _settings.OutDir;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = _settings.OutDir;
            }
            string ApPath = outputDir + @"\AuthorProfile.profile." + _curBook.asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(ApPath))
            {
                Logger.Log("AuthorProfile file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return false;
            }

            DataSources.AuthorSearchResults searchResults = null;
            // Attempt to download from the alternate site, if present. If it fails in some way, try .com
            // If the .com search crashes, it will crash back to the caller in frmMain
            try
            {
                searchResults = await DataSources.Amazon.SearchAuthor(_curBook, _settings.AmazonTld);
            }
            catch (Exception ex)
            {
                Logger.Log("Error searching Amazon." + _settings.AmazonTld + ": " + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                if (searchResults == null)
                {
                    Logger.Log(String.Format("Failed to find {0} on Amazon." + _settings.AmazonTld, _curBook.author));
                    if (_settings.AmazonTld != "com")
                    {
                        Logger.Log("Trying again with Amazon.com.");
                        _settings.AmazonTld = "com";
                        searchResults = await DataSources.Amazon.SearchAuthor(_curBook, _settings.AmazonTld);
                    }
                }
            }
            if (searchResults == null) return false; // Already logged error in search function
            authorAsin = searchResults.authorAsin;

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    Logger.Log("Saving author's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory + String.Format(@"\dmp\{0}.authorpageHtml.txt", _curBook.asin),
                        searchResults.authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("An error occurred saving authorpageHtml.txt: {0}", ex.Message));
                }
            }

            // Try to find author's biography
            string bioFile = Environment.CurrentDirectory + @"\ext\" + authorAsin + ".bio";
            if (_settings.SaveBio && File.Exists(bioFile))
            {
                if (!readBio(bioFile)) return false;
            }
            if (BioTrimmed == "")
            {
                // TODO: Let users edit bio in same style as chapters and aliases
                HtmlNode bio = DataSources.Amazon.GetBioNode(searchResults, _settings.AmazonTld);
                //Trim authour biography to less than 1000 characters and/or replace more problematic characters.
                if (bio?.InnerText.Trim().Length > 0)
                {
                    if (bio.InnerText.Length > 1000)
                    {
                        int lastPunc = bio.InnerText.LastIndexOfAny(new [] { '.', '!', '?' });
                        int lastSpace = bio.InnerText.LastIndexOf(' ');
                        if (lastPunc > lastSpace)
                            BioTrimmed = bio.InnerText.Substring(0, lastPunc + 1);
                        else
                            BioTrimmed = bio.InnerText.Substring(0, lastSpace) + '\u2026';
                    }
                    else
                    {
                        BioTrimmed = bio.InnerText;
                    }
                    BioTrimmed = BioTrimmed.Clean();
                    Logger.Log("Author biography found on Amazon!");
                }
            }
            else
            {
                File.WriteAllText(bioFile, String.Empty);
                if (System.Windows.Forms.DialogResult.Yes ==
                    System.Windows.Forms.MessageBox.Show(
                        "No author biography found on Amazon!\r\nWould you like to create a biography?", "Biography",
                        System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question,
                        System.Windows.Forms.MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(bioFile);
                    if (!readBio(bioFile)) return false;
                }
                else
                {
                    BioTrimmed = "No author biography found on Amazon!";
                    Logger.Log("An error occurred finding the author biography on Amazon.");
                }
            }
            if (_settings.SaveBio)
            {
                if (!File.Exists(bioFile))
                {
                    try
                    {
                        Logger.Log("Saving biography to " + bioFile);
                        using (var streamWriter = new StreamWriter(bioFile, false, System.Text.Encoding.UTF8))
                        {
                            streamWriter.Write(BioTrimmed);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("An error occurred while writing biography.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        return false;
                    }
                }
                if (System.Windows.Forms.DialogResult.Yes == System.Windows.Forms.MessageBox.Show("Would you like to open the biography file in notepad for editing?", "Biography",
                   System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(bioFile);
                    if (!readBio(bioFile)) return false;
                }
            }
            // Try to download Author image
            HtmlNode imageXpath = DataSources.Amazon.GetAuthorImageNode(searchResults, _settings.AmazonTld);
            authorImageUrl = Regex.Replace(imageXpath.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

            // cleanup to match retail file image links
            if (authorImageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                authorImageUrl = authorImageUrl.Replace(@"https://images-na.ssl-images-amazon", @"http://ecx.images-amazon");

            _curBook.authorImageUrl = authorImageUrl;

            Bitmap ApAuthorImage;
            try
            {
                Logger.Log("Downloading author image...");
                ApAuthorImage = await HttpDownloader.GetImage(authorImageUrl);
                Logger.Log("Grayscale base64-encoded author image created!");
            }
            catch (Exception ex)
            {
                Logger.Log(String.Format("An error occurred downloading the author image: {0}", ex.Message));
                return false;
            }

            Logger.Log("Gathering author's other books...");
            var bookList = DataSources.Amazon.GetAuthorBooks(searchResults, _curBook.title, _curBook.author, _settings.AmazonTld)
                ?? DataSources.Amazon.GetAuthorBooksNew(searchResults, _curBook.title, _curBook.author, _settings.AmazonTld);
            if (bookList != null)
            {
                Logger.Log("Gathering metadata for other books...");
                var bookBag = new ConcurrentBag<BookInfo>();
                await bookList.ParallelForEachAsync(async book =>
                {
                    // TODO: retry a couple times if one fails maybe
                    try
                    {
                        //Gather book desc, image url, etc, if using new format
                        if (_settings.UseNewVersion)
                            await book.GetAmazonInfo(book.amazonUrl);
                        bookBag.Add(book);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(String.Format("An error occurred gathering metadata for other books: {0}\r\nURL: {1}\r\nBook: {2}", ex.Message, book.amazonUrl, book.title));
                        throw;
                    }
                });
                otherBooks.AddRange(bookBag);
            }
            else
            {
                Logger.Log("Unable to find other books by this author. If there should be some, check the Amazon URL to ensure it is correct.");
            }

            Logger.Log("Writing Author Profile to file...");

            var authorOtherBooks = otherBooks.Select(book => new Model.AuthorProfile.Book
            {
                E = 1,
                Asin = book.asin,
                Title = book.title
            }).ToArray();

            var ap = new Model.AuthorProfile
            {
                Asin = _curBook.asin,
                CreationDate = Functions.UnixTimestampSeconds(),
                OtherBooks = authorOtherBooks,
                Authors = new []
                {
                    new Model.AuthorProfile.Author
                    {
                        Asin = authorAsin,
                        Bio = BioTrimmed,
                        ImageHeight = ApAuthorImage.Height,
                        Name = _curBook.author,
                        OtherBookAsins = otherBooks.Select(book => book.asin).ToArray(),
                        Picture = Functions.ImageToBase64(ApAuthorImage, ImageFormat.Jpeg)
                    }
                }
            };

            string authorProfileOutput = JsonConvert.SerializeObject(ap);

            try
            {
                File.WriteAllText(ApPath, authorProfileOutput);
                Logger.Log("Author Profile file created successfully!\r\nSaved to " + ApPath);
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while writing the Author Profile file: " + ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }

            ApTitle = "About " + _curBook.author;
            ApSubTitle = "Kindle Books By " + _curBook.author;
            EaSubTitle = "More Books By " + _curBook.author;
            return true;
        }

        public string ToJSON()
        {
            string template = @"{{""class"":""authorBio"",""asin"":""{0}"",""name"":""{1}"",""bio"":""{2}"",""imageUrl"":""{3}""}}";
            return Functions.ExpandUnicode(String.Format(template, authorAsin, _curBook.author, BioTrimmed, authorImageUrl));
        }

        private bool readBio(string bioFile)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(bioFile, System.Text.Encoding.UTF8))
                {
                    BioTrimmed = streamReader.ReadToEnd();
                    if (BioTrimmed == "")
                        Logger.Log("Found biography file, but it is empty!\r\n" + bioFile);
                    else
                        Logger.Log("Using biography from " + bioFile + ".");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while opening " + bioFile + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }
            return true;
        }

        public class Settings
        {
            public string AmazonTld { get; set; }
            public string OutDir { get; set; }
            public bool Android { get; set; }
            public bool UseNewVersion { get; set; }
            public bool UseSubDirectories { get; set; }
            public bool SaveBio { get; set; }
        }
    }
}