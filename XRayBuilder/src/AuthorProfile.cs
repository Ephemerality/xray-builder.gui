using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilderGUI.DataSources;
using XRayBuilderGUI.DataSources.Amazon;

namespace XRayBuilderGUI
{
    public static class AuthorProfile
    {
        // TODO: Review this...
        public static async Task<Response> GenerateAsync(Request request, ILogger logger, CancellationToken cancellationToken = default)
        {
            AuthorSearchResults searchResults = null;
            // Attempt to download from the alternate site, if present. If it fails in some way, try .com
            // If the .com search crashes, it will crash back to the caller in frmMain
            try
            {
                searchResults = await Amazon.SearchAuthor(request.Book, request.Settings.AmazonTld, logger, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Log("Error searching Amazon." + request.Settings.AmazonTld + ": " + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                if (searchResults == null)
                {
                    logger.Log(string.Format("Failed to find {0} on Amazon." + request.Settings.AmazonTld, request.Book.Author));
                    if (request.Settings.AmazonTld != "com")
                    {
                        logger.Log("Trying again with Amazon.com.");
                        request.Settings.AmazonTld = "com";
                        searchResults = await Amazon.SearchAuthor(request.Book, request.Settings.AmazonTld, logger, cancellationToken);
                    }
                }
            }
            if (searchResults == null)
                return null; // Already logged error in search function

            var authorAsin = searchResults.AuthorAsin;

            if (Properties.Settings.Default.saveHtml)
            {
                try
                {
                    logger.Log("Saving author's Amazon webpage...");
                    File.WriteAllText(Environment.CurrentDirectory + string.Format(@"\dmp\{0}.authorpageHtml.txt", request.Book.Asin),
                        searchResults.AuthorHtmlDoc.DocumentNode.InnerHtml);
                }
                catch (Exception ex)
                {
                    logger.Log(string.Format("An error occurred saving authorpageHtml.txt: {0}", ex.Message));
                }
            }

            // Try to find author's biography

            string ReadBio(string file)
            {
                try
                {
                    var fileText = Functions.ReadFromFile(file);
                    if (string.IsNullOrEmpty(fileText))
                        logger.Log("Found biography file, but it is empty!\r\n" + file);
                    else
                        logger.Log("Using biography from " + file + ".");

                    return fileText;
                }
                catch (Exception ex)
                {
                    logger.Log("An error occurred while opening " + file + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }

                return null;
            }

            // TODO: Separate out biography stuff
            string biography = null;
            var bioFile = Environment.CurrentDirectory + @"\ext\" + authorAsin + ".bio";
            var readFromFile = false;
            if (request.Settings.SaveBio && File.Exists(bioFile))
            {
                biography = ReadBio(bioFile);
                if (string.IsNullOrEmpty(biography))
                    return null;
                readFromFile = true;
            }

            if (string.IsNullOrEmpty(biography))
            {
                HtmlNode bio = null;
                try
                {
                    bio = Amazon.GetBioNode(searchResults, request.Settings.AmazonTld);
                }
                catch (FormatChangedException)
                {
                    logger.Log("Warning: Amazon biography format changed or no biography available for this author.", LogLevel.Warn);
                }
                //Trim authour biography to less than 1000 characters and/or replace more problematic characters.
                if (bio?.InnerText.Trim().Length > 0)
                {
                    if (bio.InnerText.Length > 1000)
                    {
                        var lastPunc = bio.InnerText.LastIndexOfAny(new [] { '.', '!', '?' });
                        var lastSpace = bio.InnerText.LastIndexOf(' ');
                        if (lastPunc > lastSpace)
                            biography = bio.InnerText.Substring(0, lastPunc + 1);
                        else
                            biography = bio.InnerText.Substring(0, lastSpace) + '\u2026';
                    }
                    else
                        biography = bio.InnerText;

                    biography = biography.Clean();
                    if (request.Settings.SaveBio)
                        File.WriteAllText(bioFile, biography);
                    logger.Log("Author biography found on Amazon!");
                }
            }

            var message = biography == null
                ? "No author biography found on Amazon!\r\nWould you like to create one?"
                : readFromFile
                    ? "Would you like to edit the existing biography?"
                    : "Author biography found on Amazon! Would you like to edit it?";

            // TODO: No dialogs here

            if (request.Settings.EditBiography
                && System.Windows.Forms.DialogResult.Yes ==
                System.Windows.Forms.MessageBox.Show(
                    message, "Biography",
                    System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question,
                    System.Windows.Forms.MessageBoxDefaultButton.Button2))
            {
                if (!File.Exists(bioFile))
                    File.WriteAllText(bioFile, string.Empty);
                Functions.RunNotepad(bioFile);
                biography = ReadBio(bioFile);
            }

            if (string.IsNullOrEmpty(biography))
            {
                biography = "No author biography found locally or on Amazon!";
                logger.Log("An error occurred finding the author biography.");
            }

            if (request.Settings.SaveBio)
            {
                if (!File.Exists(bioFile))
                {
                    try
                    {
                        logger.Log("Saving biography to " + bioFile);
                        using (var streamWriter = new StreamWriter(bioFile, false, System.Text.Encoding.UTF8))
                        {
                            streamWriter.Write(biography);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log("An error occurred while writing biography.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        return null;
                    }
                }
                if (System.Windows.Forms.DialogResult.Yes == System.Windows.Forms.MessageBox.Show("Would you like to open the biography file in notepad for editing?", "Biography",
                   System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button2))
                {
                    Functions.RunNotepad(bioFile);
                    biography = ReadBio(bioFile);
                    if (string.IsNullOrEmpty(biography))
                        return null;
                }
            }

            // Try to download Author image
            var imageXpath = Amazon.GetAuthorImageNode(searchResults, request.Settings.AmazonTld);
            var authorImageUrl = Regex.Replace(imageXpath.GetAttributeValue("src", ""), @"_.*?_\.", string.Empty);

            // cleanup to match retail file image links
            if (authorImageUrl.Contains(@"https://images-na.ssl-images-amazon"))
                authorImageUrl = authorImageUrl.Replace(@"https://images-na.ssl-images-amazon", @"http://ecx.images-amazon");

            request.Book.AuthorImageUrl = authorImageUrl;

            Bitmap ApAuthorImage = null;
            try
            {
                logger.Log("Downloading author image...");
                ApAuthorImage = await HttpClient.GetImageAsync(authorImageUrl, cancellationToken);
                logger.Log("Grayscale base64-encoded author image created!");
            }
            catch (Exception ex)
            {
                logger.Log(string.Format("An error occurred downloading the author image: {0}", ex.Message));
            }

            logger.Log("Gathering author's other books...");

            var bookList = Amazon.GetAuthorBooks(searchResults, request.Book.Title, request.Book.Author, request.Settings.AmazonTld);
            if (bookList == null || !bookList.Any())
                bookList = Amazon.GetAuthorBooksNew(searchResults, request.Book.Title, request.Book.Author, request.Settings.AmazonTld);
            var bookBag = new ConcurrentBag<BookInfo>();
            if (bookList != null)
            {
                logger.Log("Gathering metadata for other books...");
                await bookList.ParallelForEachAsync(async book =>
                {
                    // TODO: retry a couple times if one fails maybe
                    try
                    {
                        //Gather book desc, image url, etc, if using new format
                        if (request.Settings.UseNewVersion)
                            await book.GetAmazonInfo(book.AmazonUrl, cancellationToken);
                        bookBag.Add(book);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(string.Format("An error occurred gathering metadata for other books: {0}\r\nURL: {1}\r\nBook: {2}", ex.Message, book.AmazonUrl, book.Title));
                        throw;
                    }
                }, cancellationToken);
            }
            else
            {
                logger.Log("Unable to find other books by this author. If there should be some, check the Amazon URL to ensure it is correct.");
            }

            logger.Log("Writing Author Profile to file...");

            return new Response
            {
                Asin = authorAsin,
                Name = request.Book.Author,
                OtherBooks = bookBag.ToArray(),
                Biography = biography,
                Image = ApAuthorImage,
                ImageUrl = authorImageUrl,
                AmazonTld = request.Settings.AmazonTld
            };
        }

        public class Settings
        {
            public string AmazonTld { get; set; }
            public bool UseNewVersion { get; set; }
            public bool SaveBio { get; set; }
            public bool EditBiography { get; set; }
        }

        public class Request
        {
            public BookInfo Book { get; set; }
            public Settings Settings { get; set; }

        }

        public class Response
        {
            public string Asin { get; set; }
            public string Name { get; set; }
            public string Biography { get; set; }
            public Bitmap Image { get; set; }
            public string ImageUrl { get; set; }
            public BookInfo[] OtherBooks { get; set; }
            public string AmazonTld { get; set; }
        }

        public static Model.Artifacts.AuthorProfile CreateAp(Response response, string bookAsin)
        {
            var authorOtherBooks = response.OtherBooks.Select(book => new Model.Artifacts.AuthorProfile.Book
            {
                E = 1,
                Asin = book.Asin,
                Title = book.Title
            }).ToArray();

            return new Model.Artifacts.AuthorProfile
            {
                Asin = bookAsin,
                CreationDate = Functions.UnixTimestampSeconds(),
                OtherBooks = authorOtherBooks,
                Authors = new[]
                {
                    new Model.Artifacts.AuthorProfile.Author
                    {
                        Asin = response.Asin,
                        Bio = response.Biography,
                        ImageHeight = response.Image.Height,
                        Name = response.Name,
                        OtherBookAsins = response.OtherBooks.Select(book => book.Asin).ToArray(),
                        Picture = Functions.ImageToBase64(response.Image, ImageFormat.Jpeg)
                    }
                }
            };
        }
    }
}