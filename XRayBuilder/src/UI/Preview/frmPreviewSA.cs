using System;
using System.Diagnostics;
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
        private const int MaxImageSize = 750;
        private string _asin;

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

            _asin = startActions.BookInfo.Asin;

            if (startActions.Data.SeriesPosition != null)
            {
                var seriesInfo = startActions.Data.SeriesPosition;
                if (seriesInfo.PositionInSeries > 1)
                {
                    lblSeries.Text = $@"This is book {seriesInfo.PositionInSeries} of {seriesInfo.TotalInSeries} in {seriesInfo.SeriesName}";
                    lblPreviousHeading.Visible = true;
                    lblPreviousTitle.Visible = true;
                }
                else
                {
                    lblSeries.Text = @"This book is not part of a series…";
                    pbPreviousCover.Image = Resources.missing_cover_small;
                    lblPreviousHeading.Visible = false;
                    lblPreviousTitle.Visible = false;
                }
            }

            // TODO: Enums or something for language
            var highlights = startActions.Data.PopularHighlightsText?.LocalizedText?.GetOrDefault("en-US");
            if (highlights != null)
            {
                var popularHighlightsText = _regexHighlights.Match(highlights);
                if (popularHighlightsText.Success)
                    lblHighlightsCount.Text = !popularHighlightsText.Groups["text"].Value.StartsWith("0") ? popularHighlightsText.Groups["text"].Value : "No popular highlights were found for this book.";
            }
            else
                lblHighlightsCount.Text = "No popular highlights were found for this book.";

            if (startActions.Data.BookDescription != null)
            {
                var bookDescription = startActions.Data.BookDescription;
                lblTitle.Text = bookDescription.Title;
                lblAuthor.Text = bookDescription.Authors.FirstOrDefault() ?? "";
                lblAboutAuthorName.Text = bookDescription.Authors.FirstOrDefault() ?? "";
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
                    pbAuthorImage.Image = await _httpClient.GetImageAsync(imageUrl, MaxImageSize, false, cancellationToken);
                lblBiography.Text = author.Bio;
            }

            if (startActions.Data.AuthorRecs != null || startActions.Data.AuthorFeaturedRecs != null)
            {
                var recommendations = startActions.Data.AuthorRecs ?? startActions.Data.AuthorFeaturedRecs;
                Width = recommendations.Recommendations.Length < 15 ? 665 : 691;
                foreach (var rec in recommendations.Recommendations)
                {
                    var imageUrl = rec.ImageUrl;
                    if (!string.IsNullOrEmpty(imageUrl))
                        ilOtherBooks.Images.Add(await _httpClient.GetImageAsync(imageUrl, MaxImageSize, false, cancellationToken));
                }
                ListViewItem_SetSpacing(lvOtherBooks, 60 + 10, 90 + 10);
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
                    pbPreviousCover.Image = await _httpClient.GetImageAsync(imageUrl, MaxImageSize, false, cancellationToken);
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

        private void linkStore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_asin))
                Process.Start($"http://www.amazon.{Settings.Default.amazonTLD}/dp/{_asin}");
        }

        private void frmPreviewSA_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }
    }
}
