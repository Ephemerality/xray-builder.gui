using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dasync.Collections;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilderGUI.Extensions;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI.Preview
{
    public partial class frmPreviewEA : Form, IPreviewForm
    {
        private readonly IHttpClient _httpClient;

        private readonly Settings _settings = Settings.Default;
        private string _authorUrl;
        private string _asin;

        private const int MaxImageSize = 750;

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

        #region PREVENT LISTVIEW ICON SELECTION

        private void lvCustomersWhoBoughtRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        private void lvAuthorRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        #endregion

        public frmPreviewEA(IHttpClient httpClient)
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
            try
            {
                var endActions = JsonUtil.DeserializeFile<EndActions>(inputFile);

                var author = endActions.Data.AuthorSubscriptions.Subscriptions?.FirstOrDefault();
                if (author != null)
                {
                    _authorUrl =
                        $"https://www.amazon.{_settings.amazonTLD}/{author.Name.Replace(" ", "-")}/e/{author.Asin}";
                    lblAuthor.Text = author.Name;
                    lblAuthorRecs.Text = $@"More by {author.Name}";
                    if (!string.IsNullOrEmpty(author.ImageUrl))
                        pbAuthor.Image = (await _httpClient.GetImageAsync(author.ImageUrl, MaxImageSize, true, cancellationToken)).ToBitmap();
                }

                if (endActions.Data.PublicSharedRating != null)
                {
                    pbRating.Image =
                        (Image)Resources.ResourceManager.GetObject($"STAR{Math.Floor(endActions.Data.PublicSharedRating.Value)}");
                }
                else
                {
                    pbRating.Image =
                        (Image)Resources.ResourceManager.GetObject("STAR0");
                }

                if (endActions.Data.CustomerProfile != null)
                {
                    lblUpdate.Text = $@"Update on Amazon (as {endActions.Data.CustomerProfile.PenName}) and Goodreads";
                }

                ilauthorRecs.Images.Clear();
                lvAuthorRecs.Items.Clear();
                ilcustomersWhoBoughtRecs.Images.Clear();
                lvCustomersWhoBoughtRecs.Items.Clear();

                if (endActions.Data.NextBook != null)
                {
                    var nextBook = endActions.Data.NextBook;
                    lblNextTitle.Visible = true;
                    lblNextAuthor.Visible = true;
                    linkStore.Visible = true;
                    lblNextTitle.Text = nextBook.Title;
                    lblNextAuthor.Text = nextBook.Authors.FirstOrDefault();
                    _asin = nextBook.Asin;
                    if (!string.IsNullOrEmpty(nextBook.ImageUrl))
                        pbNextCover.Image = (await _httpClient.GetImageAsync(nextBook.ImageUrl, MaxImageSize, false, cancellationToken)).ToBitmap();
                }
                else
                {
                    lblNextTitle.Visible = false;
                    lblNextAuthor.Visible = false;
                    linkStore.Visible = false;

                    txtNotInSeries.Location = new Point(457, 63);
                    txtNotInSeries.Visible = true;
                }

                if (endActions.Data.AuthorRecs?.Recommendations != null)
                    await PopulateImagesFromBooks(lvAuthorRecs, ilauthorRecs, endActions.Data.AuthorRecs.Recommendations, cancellationToken);
                if (endActions.Data.CustomersWhoBoughtRecs?.Recommendations != null)
                    await PopulateImagesFromBooks(lvCustomersWhoBoughtRecs, ilcustomersWhoBoughtRecs, endActions.Data.CustomersWhoBoughtRecs.Recommendations, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new AggregateException("Invalid EndActions file!", ex);
            }
        }

        private async Task PopulateImagesFromBooks(ListView listView, ImageList imageList, IEnumerable<Book> books, CancellationToken cancellationToken = default)
        {
            ListViewItem_SetSpacing(listView, 60 + 10, 90 + 10);

            var urls = books.Select(book => book.ImageUrl);
            var images = await _httpClient.GetImages(urls, MaxImageSize, false, cancellationToken).ToArrayAsync(cancellationToken);

            var i = 0;
            foreach (var image in images)
            {
                var item = new ListViewItem
                {
                    ImageIndex = i++
                };

                listView.Items.Add(item);
                imageList.Images.Add(image.ToBitmap());
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
            Dispose(true);
        }

        private void btnFollow_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_authorUrl)) return;
            Process.Start(_authorUrl);
        }

        private void linkStore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_asin))
                Process.Start($"http://www.amazon.{Settings.Default.amazonTLD}/dp/{_asin}");
        }
    }
}