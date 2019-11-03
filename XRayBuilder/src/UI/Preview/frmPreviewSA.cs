using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilderGUI.Extras.Artifacts;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Enumerables.Extensions;
using XRayBuilderGUI.Libraries.Http;
using XRayBuilderGUI.Libraries.Language.Pluralization;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI.Preview
{
    public partial class frmPreviewSA : Form, IPreviewForm
    {
        private readonly IHttpClient _httpClient;

        private readonly Regex _regexHighlights = new Regex(@"(?<text>(?<num>[\d,.]+) passages have been highlighted (?<total>[\d,.]+) times)", RegexOptions.Compiled);

        public frmPreviewSA(IHttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }

        public async Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            var startActions = Functions.JsonDeserializeFile<StartActions>(inputFile);

            ilOtherBooks.Images.Clear();
            dgvOtherBooks.Rows.Clear();

            if (startActions.Data.SeriesPosition != null)
            {
                var seriesInfo = startActions.Data.SeriesPosition;
                lblSeries.Text = $"This is book {seriesInfo.PositionInSeries} of {seriesInfo.TotalInSeries} in {seriesInfo.SeriesName}";
                if (seriesInfo.PositionInSeries == 1)
                {
                    pbPreviousCover.Visible = false;
                    lblPreviousHeading.Visible = false;
                    lblPreviousTitle.Visible = false;
                    lblSeries.Left = 12;
                    lblSeries.Width = 312;
                }
                else
                {
                    lblSeries.Left = 80;
                    lblSeries.Width = 244;
                    pbPreviousCover.Visible = true;
                    lblPreviousHeading.Visible = true;
                    lblPreviousTitle.Visible = true;
                }
            }
            else
            {
                lblSeries.Text = "This book is not part of a series...";
                pbPreviousCover.Image = Resources.missing_image;
                lblPreviousHeading.Visible = false;
                lblPreviousTitle.Visible = false;
            }

            // TODO: Enums or something for language
            var highlights = startActions.Data.PopularHighlightsText?.LocalizedText?.GetOrDefault("en-US");
            if (highlights != null)
            {
                var popularHighlightsText = _regexHighlights.Match(highlights);
                if (popularHighlightsText.Success)
                    lblHighlights.Text = popularHighlightsText.Groups["text"].Value;
            }

            if (startActions.Data.BookDescription != null)
            {
                var bookDescription = startActions.Data.BookDescription;
                lblTitle.Text = bookDescription.Title;
                lblAuthor.Text = bookDescription.Authors.FirstOrDefault() ?? "";
                lblDescription.Text = bookDescription.Description;
                if (bookDescription.AmazonRating.HasValue)
                    pbRating.Image = (Image)Resources.ResourceManager.GetObject($"STAR{bookDescription.AmazonRating}");
                lblVotes.Text = $"({bookDescription.NumberOfReviews ?? 0} {PluralUtil.Pluralize($"{bookDescription.NumberOfReviews ?? 0:vote}")})";
            }

            var author = startActions.Data.AuthorBios?.Authors?.FirstOrDefault();
            if (author != null)
            {
                var imageUrl = author.ImageUrl;
                if (!string.IsNullOrEmpty(imageUrl))
                    pbAuthorImage.Image = await _httpClient.GetImageAsync(imageUrl, true, cancellationToken);
                lblBiography.Text = author.Bio;
            }

            if (startActions.Data.AuthorRecs != null || startActions.Data.AuthorFeaturedRecs != null)
            {
                var recommendations = startActions.Data.AuthorRecs ?? startActions.Data.AuthorFeaturedRecs;
                foreach (var rec in recommendations.Recommendations)
                {
                    var imageUrl = rec.ImageUrl;
                    if (!string.IsNullOrEmpty(imageUrl))
                        ilOtherBooks.Images.Add(await _httpClient.GetImageAsync(imageUrl, true, cancellationToken));
                    dgvOtherBooks.Rows.Add(ilOtherBooks.Images[ilOtherBooks.Images.Count - 1], $"{rec.Title}\n{rec.Authors.FirstOrDefault() ?? ""}");
                }
            }

            if (startActions.Data.ReadingTime != null)
            {
                lblReadingTime.Text = $"{startActions.Data.ReadingTime.Hours} hours and {startActions.Data.ReadingTime.Minutes} minutes to read";
                if (startActions.Data.ReadingPages != null)
                    lblReadingTime.Text = $"{lblReadingTime.Text} ({startActions.Data.ReadingPages.PagesInBook} pages)";
            }

            if (startActions.Data.PreviousBookInTheSeries != null)
            {
                lblPreviousTitle.Text = startActions.Data.PreviousBookInTheSeries.Title;
                var imageUrl = startActions.Data.PreviousBookInTheSeries.ImageUrl;
                if (!string.IsNullOrEmpty(imageUrl))
                    pbPreviousCover.Image = await _httpClient.GetImageAsync(imageUrl, true, cancellationToken);
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }
    }
}
