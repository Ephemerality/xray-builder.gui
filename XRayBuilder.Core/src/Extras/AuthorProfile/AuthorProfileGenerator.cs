using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Images.Extensions;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Extras.AuthorProfile
{
    public sealed class AuthorProfileGenerator : IAuthorProfileGenerator
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IAmazonClient _amazonClient;

        public AuthorProfileGenerator(IHttpClient httpClient, ILogger logger, IAmazonClient amazonClient)
        {
            _httpClient = httpClient;
            _logger = logger;
            _amazonClient = amazonClient;
        }

        // TODO: Review this...
        public async Task<Response> GenerateAsync(Request request, Func<string, bool> editBioCallback, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            AuthorSearchResults searchResults = null;
            // Attempt to download from the alternate site, if present. If it fails in some way, try .com
            // If the .com search crashes, it will crash back to the caller in frmMain
            try
            {
                searchResults = await _amazonClient.SearchAuthor(request.Book.Author, request.Settings.AmazonTld, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Log($"Error searching Amazon.{request.Settings.AmazonTld}: {ex.Message}\r\n{ex.StackTrace}");
            }
            finally
            {
                if (searchResults == null)
                {
                    _logger.Log($"Failed to find {request.Book.Author} on Amazon.{request.Settings.AmazonTld}");
                    if (request.Settings.AmazonTld != "com")
                    {
                        _logger.Log("Trying again with Amazon.com.");
                        request.Settings.AmazonTld = "com";
                        searchResults = await _amazonClient.SearchAuthor(request.Book.Author, request.Settings.AmazonTld, cancellationToken);
                    }
                }
            }

            if (searchResults == null)
                return null; // Already logged error in search function

            // Filter out any results that are the same title but not the same asin
            searchResults.Books = searchResults.Books
                .Where(book => !book.Title.ToLower().Contains(request.Book.Title.ToLower()) && book.Asin != request.Book.Asin)
                .ToArray();

            var authorAsin = searchResults.Asin;

            //todo re-implement saving in a nicer way
//            if (Properties.Settings.Default.saveHtml)
//            {
//                try
//                {
//                    _logger.Log("Saving author's Amazon webpage...");
//                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\dmp\{0}.authorpageHtml.txt", request.Book.Asin),
//                        searchResults.AuthorHtmlDoc.DocumentNode.InnerHtml);
//                }
//                catch (Exception ex)
//                {
//                    _logger.Log(string.Format("An error occurred saving authorpageHtml.txt: {0}", ex.Message));
//                }
//            }

            // TODO: Separate out biography stuff
            // Try to find author's biography
            string biography = null;
            var bioFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext", $"{authorAsin}.bio");
            var readFromFile = false;
            var newBioFile = false;
            string ReadBio(string file)
            {
                try
                {
                    var fileText = Functions.ReadFromFile(file);
                    if (string.IsNullOrEmpty(fileText))
                        _logger.Log($"Found biography file, but it is empty!\r\n{file}");
                    else if(!string.Equals(biography, fileText))
                        _logger.Log($"Using biography from {file}.");

                    // todo fix this
                    if (fileText != null && fileText.Contains("No author biography found locally or on Amazon!"))
                        _logger.Log($"Warning: Local biography file contains an empty default biography.{Environment.NewLine}Delete {file} and try again");

                    return fileText;
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred while opening {file}\r\n{ex.Message}\r\n{ex.StackTrace}");
                }

                return null;
            }

            string TrimBio(string bio)
            {
                try
                {
                    //Trim author biography to less than 1000 characters and/or replace more problematic characters.
                    if (string.IsNullOrWhiteSpace(bio))
                        return null;

                    if (bio.Length > 1000)
                    {
                        // todo culture invariant
                        var lastPunc = bio.LastIndexOfAny(new[] {'.', '!', '?'});
                        var lastSpace = bio.LastIndexOf(' ');

                        bio = lastPunc > lastSpace
                            ? bio.Substring(0, lastPunc + 1)
                            : $"{bio.Substring(0, lastSpace)}{'\u2026'}";
                    }

                    return bio.Clean();
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred while trimming the biography\r\n{ex.Message}\r\n{ex.StackTrace}");
                }

                return bio;
            }

            if (searchResults.Biography == null && !File.Exists(bioFile))
            {
                if (request.Settings.AmazonTld != "com")
                {
                    _logger.Log(@"Searching for biography on Amazon.com…");
                    request.Settings.AmazonTld = "com";
                    var tempSearchResults = await _amazonClient.SearchAuthor(request.Book.Author, request.Settings.AmazonTld, cancellationToken, false);
                    if (tempSearchResults?.Biography != null)
                        searchResults.Biography = tempSearchResults.Biography;
                }
            }

            if (searchResults.Biography != null)
                biography = searchResults.Biography;

            // Parse author details from books goodreads page if missing from Amazon
            if (searchResults.Biography == null || searchResults.ImageUrl == null || searchResults.ImageUrl.Contains("placeholder") && !File.Exists(bioFile))
            {
                _logger.Log("Attempting to fill missing author information from Goodreads…");
                var goodreads = new SecondarySourceGoodreads(_logger, _httpClient, _amazonClient, null);
                var grAuthor = await goodreads.GetAuthorAsync(request.Book.DataUrl, cancellationToken);
                if (grAuthor != null)
                {
                    searchResults.Biography ??= grAuthor.Biography;
                    if (searchResults.ImageUrl == null || searchResults.ImageUrl.Contains("placeholder"))
                        searchResults.ImageUrl = grAuthor.ImageUrl;
                }
                biography = searchResults.Biography;
            }

            if (File.Exists(bioFile) && request.Settings.SaveBio)
            {
                biography = ReadBio(bioFile);
                // if it's null, there was an error. if it's just empty, we'll parse it out instead
                if (biography == null)
                    return null;
                if (!string.IsNullOrEmpty(biography))
                    readFromFile = true;
            }

            if (!string.IsNullOrEmpty(biography) && (request.Settings.SaveBio || request.Settings.EditBiography))
            {
                if(!readFromFile)
                    biography = TrimBio(biography);

                if (!File.Exists(bioFile) && !string.IsNullOrEmpty(biography))
                {
                    File.WriteAllText(bioFile, biography);
                    newBioFile = true;
                    _logger.Log(@"Author biography found!");
                }
            }

            var message = biography == null
                ? $"No author biography found on Amazon!{Environment.NewLine}Would you like to create one?"
                : readFromFile
                    ? "Would you like to edit the existing biography?"
                    : $"Author biography found on Amazon!{Environment.NewLine}Would you like to edit it?";

            if (editBioCallback != null && editBioCallback(message))
            {
                if (!File.Exists(bioFile))
                    File.WriteAllText(bioFile, string.Empty);
                Functions.RunNotepad(bioFile);
                biography = ReadBio(bioFile);
            }

            if (string.IsNullOrEmpty(biography))
            {
                biography = "No author biography found locally or on Amazon!";
                _logger.Log("An error occurred finding the author biography.");
            }

            if (request.Settings.SaveBio)
            {
                if (!File.Exists(bioFile))
                {
                    try
                    {
                        _logger.Log($"Saving biography to {bioFile}");
                        using var streamWriter = new StreamWriter(bioFile, false, System.Text.Encoding.UTF8);
                        await streamWriter.WriteAsync(biography);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"An error occurred while writing biography.\r\n{ex.Message}\r\n{ex.StackTrace}");
                        return null;
                    }
                }

                if (newBioFile)
                {
                    _logger.Log(@"New biography file opened in notepad for editing…");
                    Functions.RunNotepad(bioFile);
                    biography = ReadBio(bioFile);
                    if (string.IsNullOrEmpty(biography))
                        return null;
                    searchResults.Biography = biography;
                }

                if (!readFromFile && editBioCallback != null && editBioCallback("Would you like to open the biography file in notepad for editing?"))
                {
                    Functions.RunNotepad(bioFile);
                    biography = ReadBio(bioFile);
                    if (string.IsNullOrEmpty(biography))
                        return null;
                }
            }

            searchResults.Biography = biography;

            // Try to download Author image
            request.Book.AuthorImageUrl = searchResults.ImageUrl;

            Bitmap ApAuthorImage = null;
            try
            {
                _logger.Log("Downloading author image…");
                ApAuthorImage = await _httpClient.GetImageAsync(request.Book.AuthorImageUrl, cancellationToken: cancellationToken);
                _logger.Log("Grayscale base64-encoded author image created!");
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred downloading the author image: {ex.Message}");
            }

            var bookBag = new ConcurrentBag<BookInfo>();
            if (searchResults.Books != null && request.Settings.UseNewVersion)
            {
                if (searchResults.Books.Length != 0)
                {
                    // todo pluralize
                    _logger.Log(searchResults.Books.Length > 1
                        ? $"Gathering metadata for {searchResults.Books.Length} other books by {request.Book.Author}…"
                        : $"Gathering metadata for another book by {request.Book.Author}…");
                }
                try
                {
                    progress?.Set(0, searchResults.Books.Length);
                    await _amazonClient
                        .EnhanceBookInfos(searchResults.Books, cancellationToken)
                        .ForEachAsync(book =>
                        {
                            bookBag.Add(book);
                            progress?.Add(1);
                        }, cancellationToken);
                    progress?.Set(0, 0);
                    _logger.Log("Metadata gathering complete!");
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred gathering metadata for other books: {ex.Message}");
                    throw;
                }
            }
            else
            {
                _logger.Log($"Unable to find other books by {request.Book.Author}. If there should be some, check the Amazon URL to ensure it is correct.");
            }

            _logger.Log("Writing Author Profile to file…");

            return new Response
            {
                Asin = authorAsin,
                Name = request.Book.Author,
                OtherBooks = bookBag.ToArray(),
                Biography = biography,
                Image = ApAuthorImage,
                ImageUrl = searchResults.ImageUrl
            };
        }

        public sealed class Settings
        {
            public string AmazonTld { get; set; }
            public bool UseNewVersion { get; set; }
            public bool SaveBio { get; set; }
            public bool EditBiography { get; set; }
        }

        public sealed class Request
        {
            public BookInfo Book { get; set; }
            public Settings Settings { get; set; }

        }

        public sealed class Response
        {
            public string Asin { get; set; }
            public string Name { get; set; }
            public string Biography { get; set; }
            public Bitmap Image { get; set; }
            public string ImageUrl { get; set; }
            public BookInfo[] OtherBooks { get; set; }
        }

        public static Artifacts.AuthorProfile CreateAp(Response response, string bookAsin)
        {
            var authorOtherBooks = response.OtherBooks.Select(book => new Artifacts.AuthorProfile.BookData
            {
                E = 1,
                Asin = book.Asin,
                Title = book.Title
            }).ToArray();

            return new Artifacts.AuthorProfile
            {
                Asin = bookAsin,
                CreationDate = Functions.UnixTimestampSeconds(),
                OtherBooks = authorOtherBooks,
                Authors = new[]
                {
                    new Artifacts.AuthorProfile.Author
                    {
                        Asin = response.Asin,
                        Bio = response.Biography,
                        ImageHeight = response.Image?.Height ?? 0,
                        Name = response.Name,
                        OtherBookAsins = response.OtherBooks.Select(book => book.Asin).ToArray(),
                        Picture = response.Image?.ToBase64(ImageFormat.Jpeg)
                    }
                }
            };
        }
    }
}