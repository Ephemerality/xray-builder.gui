using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using Newtonsoft.Json;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Primitives.Util;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Model.Exceptions;

namespace XRayBuilder.Core.Extras.EndActions
{
    public sealed class EndActionsArtifactService : IEndActionsArtifactService
    {
        private readonly ILogger _logger;
        private readonly Artifacts.EndActions _baseEndActions;

        public EndActionsArtifactService(ILogger logger)
        {
            _logger = logger;

            try
            {
                var template = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}dist/BaseEndActions.json", Encoding.UTF8);
                _baseEndActions = JsonConvert.DeserializeObject<Artifacts.EndActions>(template);
            }
            catch (FileNotFoundException)
            {
                throw new InitializationException("Unable to find dist/BaseEndActions.json, make sure it has been extracted!");
            }
            catch (Exception e)
            {
                throw new InitializationException($"An error occurred while loading dist/BaseEndActions.json (make sure any new versions have been extracted!)\r\n{e.Message}\r\n{e.StackTrace}", e);
            }
        }

        public string GenerateNew(Request request)
        {
            var endActions = _baseEndActions.Clone();

            endActions.BookInfo = new Extras.Artifacts.EndActions.BookInformation
            {
                Asin = request.BookAsin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAATAAA",
                ImageUrl = request.BookImageUrl,
                EmbeddedID = $"{request.BookDatabaseName}:{request.BookGuid}",
                FictionStatus = "unknown",
                Erl = -1 //request.BookErl
            };
            endActions.Data.FollowSubscriptions = new Extras.Artifacts.EndActions.AuthorSubscriptions
            {
                Subscriptions = new[]
                {
                    new Subscription
                    {
                        Asin = request.AuthorAsin,
                        Name = request.Author,
                        ImageUrl = request.AuthorImageUrl
                    }
                }
            };
            endActions.Data.AuthorSubscriptions = endActions.Data.FollowSubscriptions;
            endActions.Data.NextBook = Extensions.BookInfoToBook(request.BookSeriesInfo?.Next, false);
            endActions.Data.PublicSharedRating = new Extras.Artifacts.EndActions.Rating
            {
                Class = "publicSharedRating",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                Value = Math.Round(request.BookAmazonRating ?? 0.0, 1)
            };
            endActions.Data.CustomerProfile = new Extras.Artifacts.EndActions.CustomerProfile
            {
                PenName = request.UserPenName,
                RealName = request.UserRealName
            };
            endActions.Data.Rating = endActions.Data.PublicSharedRating;
            endActions.Data.AuthorBios = new AuthorBios
            {
                Authors = new[]
                {
                    new Author
                    {
                        // TODO: Check mismatched fields from curbook and authorprofile
                        Asin = request.AuthorAsin,
                        Name = request.Author,
                        Bio = request.AuthorBiography,
                        ImageUrl = request.AuthorImageUrl
                    }
                }
            };
            endActions.Data.AuthorBiosBSE = new AuthorBios
            {
                Authors = new[]
                {
                    new Author
                    {
                        Asin = request.AuthorAsin,
                        Name = request.Author,
                        Bio = request.AuthorBiography,
                        ImageUrl = request.AuthorImageUrl
                    }
                }
            };
            endActions.Data.AuthorRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = request.AuthorOtherBooks
                    .Select(bk => Extensions.BookInfoToBook(bk, true))
                    .ToArray()
            };

            endActions.Data.CustomersWhoBoughtRecs = new Recs
            {
                Class = "featuredRecommendationList",
                Recommendations = request.CustomerAlsoBought
                    .Where(b => request.AuthorOtherBooks.All(a => a.Asin != b.Asin && !b.Title.ToLower().Contains(a.Title.ToLower())))
                    .Select(bk => Extensions.BookInfoToBook(bk, true))
                    .ToArray()
            };

            endActions.BottomSheetEnabled = false;

            try
            {
                return Functions.ExpandUnicode(JsonConvert.SerializeObject(endActions));
            }
            catch (Exception ex)
            {
                _logger.Log($"An error occurred creating the EndActions: {ex.Message}\r\n{ex.StackTrace}");
            }

            return null;
        }

        public string GenerateOld(Request request)
        {
            var dt = DateTime.Now.ToString("s");
            var tz = DateTime.Now.ToString("zzz");
            using var stringWriter = new XmlUtil.Utf8StringWriter();
            using var writer = new XmlTextWriter(stringWriter);
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            writer.WriteStartElement("endaction");
            writer.WriteAttributeString("version", "0");
            writer.WriteAttributeString("guid", $"{request.BookDatabaseName}:{request.BookGuid}");
            writer.WriteAttributeString("key", request.BookAsin);
            writer.WriteAttributeString("type", "EBOK");
            writer.WriteAttributeString("timestamp", $"{dt}{tz}");
            writer.WriteElementString("treatment", "d");
            writer.WriteStartElement("currentBook");
            writer.WriteElementString("imageUrl", request.BookImageUrl);
            writer.WriteElementString("asin", request.BookAsin);
            writer.WriteElementString("hasSample", "false");
            writer.WriteEndElement();
            writer.WriteStartElement("customerProfile");
            writer.WriteElementString("penName", request.UserPenName);
            writer.WriteElementString("realName", request.UserRealName);
            writer.WriteEndElement();
            writer.WriteStartElement("recs");
            writer.WriteAttributeString("type", "author");
            for (var i = 0; i < Math.Min(request.AuthorOtherBooks.Length, 5); i++)
            {
                writer.WriteStartElement("rec");
                writer.WriteAttributeString("hasSample", "false");
                writer.WriteAttributeString("asin", request.AuthorOtherBooks[i].Asin);
                writer.WriteElementString("title", request.AuthorOtherBooks[i].Title);
                writer.WriteElementString("author", request.Author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("recs");
            writer.WriteAttributeString("type", "purchase");
            for (var i = 0; i < Math.Min(request.CustomerAlsoBought.Length, 5); i++)
            {
                writer.WriteStartElement("rec");
                writer.WriteAttributeString("hasSample", "false");
                writer.WriteAttributeString("asin", request.CustomerAlsoBought[i].Asin);
                writer.WriteElementString("title", request.CustomerAlsoBought[i].Title);
                writer.WriteElementString("author", request.CustomerAlsoBought[i].Author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteElementString("booksMentionedPosition", "2");
            writer.WriteEndElement();
            writer.Flush();

            return stringWriter.ToString();
        }

        public sealed class Request
        {
            public Request([NotNull] string bookAsin, [CanBeNull] string bookImageUrl,
                [NotNull] string bookDatabaseName, [NotNull] string bookGuid, long bookErl,
                double? bookAmazonRating, [CanBeNull] SeriesInfo bookSeriesInfo, [NotNull] string author, [NotNull] string authorAsin,
                [CanBeNull] string authorImageUrl, [CanBeNull] string authorBiography,
                [NotNull] BookInfo[] authorOtherBooks, [CanBeNull] string userPenName,
                [CanBeNull] string userRealName, BookInfo[] customerAlsoBought)
            {
                BookAsin = bookAsin;
                BookImageUrl = bookImageUrl;
                BookDatabaseName = bookDatabaseName;
                BookGuid = bookGuid;
                BookErl = bookErl;
                BookAmazonRating = bookAmazonRating;
                BookSeriesInfo = bookSeriesInfo;
                Author = author;
                AuthorAsin = authorAsin;
                AuthorImageUrl = authorImageUrl;
                AuthorBiography = authorBiography;
                AuthorOtherBooks = authorOtherBooks;
                UserPenName = userPenName;
                UserRealName = userRealName;
                CustomerAlsoBought = customerAlsoBought;
            }

            [NotNull]
            public string BookAsin { get; }
            [CanBeNull]
            public string BookImageUrl { get; }
            [NotNull]
            public string BookDatabaseName { get; }
            [NotNull]
            public string BookGuid { get; }
            public long BookErl { get; }
            public double? BookAmazonRating { get; }
            [CanBeNull]
            public SeriesInfo BookSeriesInfo { get; }
            [NotNull]
            public string Author { get; }
            [NotNull]
            public string AuthorAsin { get; }
            [CanBeNull]
            public string AuthorImageUrl { get; }
            [CanBeNull]
            public string AuthorBiography { get; }
            [NotNull]
            public BookInfo[] AuthorOtherBooks { get; }
            [CanBeNull]
            public string UserPenName { get; }
            [CanBeNull]
            public string UserRealName { get; }
            public BookInfo[] CustomerAlsoBought { get; }
        }
    }
}