﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilderGUI
{
    public class AuthorProfile
    {
        private static Properties.Settings settings = Properties.Settings.Default;

        private string ApPath = "";
        private BookInfo curBook;
        private string TLD;
        
        private Bitmap ApAuthorImage = null;

        public string ApTitle = null;
        public string ApSubTitle = null;
        public string BioTrimmed = "";
        public List<BookInfo> otherBooks = new List<BookInfo>();
        public string authorImageUrl = "";
        public string authorAsin = "";

        public string EaSubTitle = null;

        public AuthorProfile(BookInfo nBook, string TLD)
        {
            curBook = nBook;
            this.TLD = TLD;
        }

        // TODO: Review this...
        public async Task<bool> Generate()
        {
            string outputDir;
            try
            {
                if (settings.android)
                {
                    outputDir = settings.outDir + @"\Android\" + curBook.asin;
                    Directory.CreateDirectory(outputDir);
                }
                else
                    outputDir = settings.useSubDirectories ? Functions.GetBookOutputDirectory(curBook.author, curBook.sidecarName) : settings.outDir;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred creating output directory: " + ex.Message + "\r\nFiles will be placed in the default output directory.");
                outputDir = settings.outDir;
            }
            ApPath = outputDir + @"\AuthorProfile.profile." + curBook.asin + ".asc";

            if (!Properties.Settings.Default.overwrite && File.Exists(ApPath))
            {
                Logger.Log("AuthorProfile file already exists... Skipping!\r\n" +
                         "Please review the settings page if you want to overwite any existing files.");
                return false;
            }
            
            //Process GUID. If in decimal form, convert to hex.
            if (Regex.IsMatch(curBook.guid, "/[a-zA-Z]/"))
                curBook.guid = curBook.guid.ToUpper();
            else
            {
                long guidDec;
                long.TryParse(curBook.guid, out guidDec);
                curBook.guid = guidDec.ToString("X");
            }
            if (curBook.guid == "0")
            {
                Logger.Log("An error occurred while converting the GUID.");
                return false;
            }

            DataSources.AuthorSearchResults searchResults = null;
            // Attempt to download from the alternate site, if present. If it fails in some way, try .com
            // If the .com search crashes, it will crash back to the caller in frmMain
            try
            {
                searchResults = await DataSources.Amazon.SearchAuthor(curBook, TLD);
            }
            catch (Exception ex)
            {
                Logger.Log("Error searching Amazon." + TLD + ": " + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                if (searchResults == null)
                {
                    Logger.Log(String.Format("Failed to find {0} on Amazon." + TLD, curBook.author));
                    if (TLD != "com")
                    {
                        Logger.Log("Trying again with Amazon.com.");
                        TLD = "com";
                        searchResults = await DataSources.Amazon.SearchAuthor(curBook, TLD);
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
                    File.WriteAllText(Environment.CurrentDirectory + String.Format(@"\dmp\{0}.authorpageHtml.txt", curBook.asin),
                        searchResults.authorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("An error occurred saving authorpageHtml.txt: {0}", ex.Message));
                }
            }

            // Try to find author's biography
            string bioFile = Environment.CurrentDirectory + @"\ext\" + authorAsin + ".bio";
            if (settings.saveBio && File.Exists(bioFile))
            {
                if (!readBio(bioFile)) return false;
            }
            if (BioTrimmed == "")
            {
                HtmlNode bio = DataSources.Amazon.GetBioNode(searchResults, TLD);
                //Trim authour biography to less than 1000 characters and/or replace more problematic characters.
                if (bio == null || bio.InnerText.Trim().Length != 0)
                {
                    if (bio.InnerText.Length > 1000)
                    {
                        int lastPunc = bio.InnerText.LastIndexOfAny(new char[] { '.', '!', '?' });
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
                    BioTrimmed = Functions.CleanString(BioTrimmed);
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
            if (settings.saveBio)
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
            HtmlNode imageXpath = DataSources.Amazon.GetAuthorImageNode(searchResults, TLD);
            authorImageUrl = Regex.Replace(imageXpath.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

            // cleanup to match retail file image links
            if (authorImageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                authorImageUrl = authorImageUrl.Replace(@"https://images-na.ssl-images-amazon", @"https://ecx.images-amazon");

            curBook.authorImageUrl = authorImageUrl;

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
            List<BookInfo> bookList = DataSources.Amazon.GetAuthorBooks(searchResults, curBook.title, curBook.author, TLD);
            if (bookList != null)
            {
                Logger.Log("Gathering metadata for other books...");
                foreach (BookInfo book in bookList)
                {
                    try
                    {
                        //Gather book desc, image url, etc, if using new format
                        if (settings.useNewVersion)
                            await book.GetAmazonInfo(book.amazonUrl);
                        otherBooks.Add(book);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(String.Format("An error occurred gathering metadata for other books: {0}\r\nURL: {1}\r\nBook: {2}", ex.Message, book.amazonUrl, book.title));
                        return false;
                    }
                }
            }
            else
            {
                Logger.Log("Unable to find other books by this author. If there should be some, check the Amazon URL to ensure it is correct.");
            }

            Logger.Log("Writing Author Profile to file...");

            //Create list of Asin numbers and titles
            List<string> authorsOtherBookList = new List<string>();
            foreach (BookInfo bk in otherBooks)
            {
                authorsOtherBookList.Add(String.Format(@"{{""e"":1,""a"":""{0}"",""t"":""{1}""}}",
                    bk.asin, bk.title));
            }

            //Create finalAuthorProfile.profile.ASIN.asc
            int unixTimestamp = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            try
            {
                string base64Image = Functions.ImageToBase64(ApAuthorImage, ImageFormat.Jpeg);
                string authorProfileOutput = @"{""u"":[{""y"":" + ApAuthorImage.Height + @",""l"":[""" +
                                          string.Join(@""",""", otherBooks.Select(book => book.asin).ToArray()) + @"""],""n"":""" +
                                          curBook.author + @""",""a"":""" + authorAsin + @""",""b"":""" + BioTrimmed +
                                          @""",""i"":""" + base64Image + @"""}],""a"":""" +
                                          String.Format(@"{0}"",""d"":{1},""o"":[", curBook.asin, unixTimestamp) +
                                          string.Join(",", authorsOtherBookList.ToArray()) + "]}";
                File.WriteAllText(ApPath, authorProfileOutput);
                Logger.Log("Author Profile file created successfully!\r\nSaved to " + ApPath);
            }
            catch (Exception ex)
            {
                Logger.Log("An error occurred while writing the Author Profile file: " + ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }

            ApTitle = "About " + curBook.author;
            ApSubTitle = "Kindle Books By " + curBook.author;
            EaSubTitle = "More Books By " + curBook.author;
            return true;
        }

        public string ToJSON()
        {
            string template = @"{{""class"":""authorBio"",""asin"":""{0}"",""name"":""{1}"",""bio"":""{2}"",""imageUrl"":""{3}""}}";
            return Functions.ExpandUnicode(String.Format(template, authorAsin, curBook.author, BioTrimmed, authorImageUrl));
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
    }
}