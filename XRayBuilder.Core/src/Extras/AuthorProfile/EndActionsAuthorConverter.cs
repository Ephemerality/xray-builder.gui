using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Extras.AuthorProfile
{
    public sealed class EndActionsAuthorConverter : IEndActionsAuthorConverter
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;

        public EndActionsAuthorConverter(ILogger logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<AuthorProfileGenerator.Response> ConvertAsync(Artifacts.EndActions endActions, CancellationToken cancellationToken)
        {
            var authorBio = endActions.Data.AuthorBios?.Authors.FirstOrDefault();
            var firstAuthorRec = endActions.Data.AuthorRecs?.Recommendations.FirstOrDefault();

            var asin = authorBio?.Asin ?? firstAuthorRec?.Asin;
            var bio = authorBio?.Bio;
            var imageUrl = authorBio?.ImageUrl ?? firstAuthorRec?.ImageUrl;
            var name = authorBio?.Name ?? firstAuthorRec?.Authors.FirstOrDefault();

            var otherBooks = Enumerable.Empty<BookInfo>();
            if (firstAuthorRec != null)
            {
                otherBooks = endActions.Data.AuthorRecs.Recommendations
                    .Select(rec => new BookInfo(rec.Title, rec.Authors.FirstOrDefault(), rec.Asin)
                    {
                        Description = rec.Description,
                        ImageUrl = rec.ImageUrl,
                        AmazonRating = rec.AmazonRating,
                        Reviews = rec.NumberOfReviews ?? 0
                    });
            }

            Bitmap authorImage = null;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    _logger.Log("Downloading author image...");
                    authorImage = await _httpClient.GetImageAsync(imageUrl, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Log($"An error occurred downloading the author image: {ex.Message}");
                }
            }

            return new AuthorProfileGenerator.Response
            {
                Asin = !string.IsNullOrEmpty(asin) ? asin : null,
                Name = !string.IsNullOrEmpty(name) ? name : null,
                Biography = !string.IsNullOrEmpty(bio) ? bio : null,
                ImageUrl = !string.IsNullOrEmpty(imageUrl) ? imageUrl : null,
                OtherBooks = otherBooks.ToArray(),
                Image = authorImage
            };
        }
    }
}