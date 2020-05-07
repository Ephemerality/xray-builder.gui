using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Primitives.Util;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Model.Exceptions;

namespace XRayBuilder.Core.Extras.StartActions
{
    public sealed class StartActionsArtifactService : IStartActionsArtifactService
    {
        private readonly Artifacts.StartActions _baseStartActions;

        public StartActionsArtifactService()
        {
            try
            {
                var template = File.ReadAllText($@"{Environment.CurrentDirectory}\dist\BaseStartActions.json", Encoding.UTF8);
                _baseStartActions = JsonConvert.DeserializeObject<Artifacts.StartActions>(template);
            }
            catch (FileNotFoundException)
            {
                throw new InitializationException(@"Unable to find dist\BaseStartActions.json, make sure it has been extracted!");
            }
            catch (Exception e)
            {
                throw new InitializationException($@"An error occurred while loading dist\BaseStartActions.json (make sure any new versions have been extracted!)\r\n{e.Message}\r\n{e.StackTrace}", e);
            }
        }

        public Artifacts.StartActions GenerateStartActions(BookInfo curBook, AuthorProfileGenerator.Response authorProfile)
        {
            var startActions = _baseStartActions.Clone();

            startActions.BookInfo = new Artifacts.StartActions.BookInformation
            {
                Asin = curBook.Asin,
                ContentType = "EBOK",
                Timestamp = Functions.UnixTimestampMilliseconds(),
                RefTagSuffix = "AAAgAAA",
                ImageUrl = curBook.ImageUrl,
                Erl = -1
            };
            if (!string.IsNullOrEmpty(curBook.Series?.Position))
            {
                startActions.Data.SeriesPosition = new Artifacts.StartActions.SeriesPosition
                {
                    PositionInSeries = Convert.ToInt32(double.Parse(curBook.Series.Position)),
                    TotalInSeries = curBook.Series.Total,
                    SeriesName = curBook.Series.Name
                };
            }

            startActions.Data.FollowSubscriptions = new Artifacts.StartActions.AuthorSubscriptions
            {
                Subscriptions = new[]
                {
                    new Subscription
                    {
                        Asin = curBook.AuthorAsin,
                        Name = curBook.Author,
                        ImageUrl = curBook.AuthorImageUrl
                    }
                }
            };
            startActions.Data.AuthorSubscriptions = startActions.Data.FollowSubscriptions;
            startActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMPASSAGES%", $"{curBook.notableClips?.Count ?? 0}");
            startActions.Data.PopularHighlightsText.LocalizedText.Replace("%NUMHIGHLIGHTS%", $"{curBook.notableClips?.Sum(c => c.Likes) ?? 0}");
            startActions.Data.GrokShelfInfo.Asin = curBook.Asin;
            startActions.Data.BookDescription = Extensions.BookInfoToBook(curBook, true);
            startActions.Data.CurrentBook = startActions.Data.BookDescription;
            startActions.Data.AuthorBios = new AuthorBios
            {
                Authors = new[]
                {
                    new Author
                    {
                        // TODO: Check mismatched fields from curbook and authorprofile
                        Asin = authorProfile.Asin,
                        Name = curBook.Author,
                        Bio = authorProfile.Biography,
                        ImageUrl = authorProfile.ImageUrl
                    }
                }
            };
            startActions.Data.AuthorRecs = new Recs
            {
                Class = "recommendationList",
                Recommendations = authorProfile.OtherBooks.Select(bk => Extensions.BookInfoToBook(bk, false)).ToArray()
            };
            startActions.Data.ReadingTime.Hours = curBook.ReadingHours;
            startActions.Data.ReadingTime.Minutes = curBook.ReadingMinutes;
            startActions.Data.ReadingTime.FormattedTime.Replace("%HOURS%", curBook.ReadingHours.ToString());
            startActions.Data.ReadingTime.FormattedTime.Replace("%MINUTES%", curBook.ReadingMinutes.ToString());
            startActions.Data.PreviousBookInTheSeries = Extensions.BookInfoToBook(curBook.Series?.Previous, true);
            startActions.Data.ReadingPages.PagesInBook = curBook.PagesInBook;

            return startActions;
        }
    }
}