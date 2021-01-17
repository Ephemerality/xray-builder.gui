using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Language.Pluralization;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape) return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        public async Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            var startActions = JsonUtil.DeserializeFile<StartActions>(inputFile);

            ilOtherBooks.Images.Clear();
            lvOtherBooks.Clear();

            if (startActions.Data.SeriesPosition != null)
            {
                var seriesInfo = startActions.Data.SeriesPosition;
                lblSeries.Text = $@"This is book {seriesInfo.PositionInSeries} of {seriesInfo.TotalInSeries} in {seriesInfo.SeriesName}";
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
                    lblSeries.Left = 85;
                    lblSeries.Width = 345;
                    pbPreviousCover.Visible = true;
                    lblPreviousHeading.Visible = true;
                    lblPreviousTitle.Visible = true;
                }
            }
            else
            {
                lblSeries.Text = @"This book is not part of a series…";
                pbPreviousCover.Image = Resources.missing_cover_small;
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
                    pbRating.Image = (Image)Resources.ResourceManager.GetObject($"STAR{Math.Floor((decimal) bookDescription.AmazonRating)}");
                lblVotes.Text = $@"({PluralUtil.Pluralize($"{bookDescription.NumberOfReviews ?? 0:vote}")})";
            }

            var author = startActions.Data.AuthorBios?.Authors?.FirstOrDefault();
            if (author != null)
            {
                var imageUrl = author.ImageUrl;
                if (!string.IsNullOrEmpty(imageUrl))
                    pbAuthorImage.Image = await _httpClient.GetImageAsync(imageUrl, false, cancellationToken);
                lblBiography.Text = author.Bio;
            }

            if (startActions.Data.AuthorRecs != null || startActions.Data.AuthorFeaturedRecs != null)
            {
                var recommendations = startActions.Data.AuthorRecs ?? startActions.Data.AuthorFeaturedRecs;
                foreach (var rec in recommendations.Recommendations)
                {
                    var imageUrl = rec.ImageUrl;
                    if (!string.IsNullOrEmpty(imageUrl))
                        ilOtherBooks.Images.Add(await _httpClient.GetImageAsync(imageUrl, false, cancellationToken));
                }
                ListViewItem_SetSpacing(lvOtherBooks, 60 + 12, 90 + 12);
                for (var i = 0; i < ilOtherBooks.Images.Count; i++)
                {
                    var item = new ListViewItem {ImageIndex = i};
                    lvOtherBooks.Items.Add(item);
                }
            }

            if (startActions.Data.ReadingTime != null)
            {
                lblReadingTime.Text = $@"{startActions.Data.ReadingTime.Hours} hours and {startActions.Data.ReadingTime.Minutes} minutes to read";
                if (startActions.Data.ReadingPages != null)
                    lblReadingTime.Text = $@"{lblReadingTime.Text} ({startActions.Data.ReadingPages.PagesInBook} pages)";
            }

            if (startActions.Data.PreviousBookInTheSeries != null)
            {
                lblPreviousTitle.Text = startActions.Data.PreviousBookInTheSeries.Title;
                var imageUrl = startActions.Data.PreviousBookInTheSeries.ImageUrl;
                if (!string.IsNullOrEmpty(imageUrl))
                    pbPreviousCover.Image = await _httpClient.GetImageAsync(imageUrl, false, cancellationToken);
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }

        #region PREVENT LISTVIEW ICON SELECTION

        private void lvOtherBooks_ItemSelectionChanged(object sender,
            ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        #endregion

        #region SET LISTVIEW ICON SPACING

        // http://qdevblog.blogspot.ch/2011/11/c-listview-item-spacing.html
        private int MakeLong(short lowPart, short highPart)
        {
            return (int) ((ushort) lowPart | (uint) (highPart << 16));
        }

        private void ListViewItem_SetSpacing(ListView listview, short leftPadding, short topPadding)
        {
            const int lvmFirst = 0x1000;
            const int lvmSetIconSpacing = lvmFirst + 53;
            NativeMethods.SendMessage(listview.Handle, lvmSetIconSpacing, IntPtr.Zero,
                (IntPtr) MakeLong(leftPadding, topPadding));
        }

        #endregion
    }
}
