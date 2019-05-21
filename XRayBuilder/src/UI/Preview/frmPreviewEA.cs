using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilderGUI.Model.Artifacts;

namespace XRayBuilderGUI.UI.Preview
{
    public partial class frmPreviewEA : Form, IPreviewForm
    {
        private readonly IHttpClient _httpClient;

        #region SET LISTVIEW ICON SPACING

        // http://qdevblog.blogspot.ch/2011/11/c-listview-item-spacing.html
        private int MakeLong(short lowPart, short highPart)
        {
            return (int) (((ushort) lowPart) | (uint) (highPart << 16));
        }

        private void ListViewItem_SetSpacing(ListView listview, short leftPadding, short topPadding)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            NativeMethods.SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr) MakeLong(leftPadding, topPadding));
        }

        #endregion

        public frmPreviewEA(IHttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }

        #region PREVENT LISTVIEW ICON SELECTION

        private void lvcustomersWhoBoughtRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        private void lvauthorRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        #endregion

        public async Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            try
            {
                var endActions = Functions.JsonDeserializeFile<Model.Artifacts.EndActions>(inputFile);

                ilauthorRecs.Images.Clear();
                lvAuthorRecs.Items.Clear();
                ilcustomersWhoBoughtRecs.Images.Clear();
                lvCustomersWhoBoughtRecs.Items.Clear();

                if (endActions.Data.NextBook != null)
                {
                    var nextBook = endActions.Data.NextBook;
                    lblNextTitle.Text = nextBook.Title;
                    lblNextAuthor.Text = nextBook.Authors.FirstOrDefault() ?? "";
                    if (!string.IsNullOrEmpty(nextBook.ImageUrl))
                        pbNextCover.Image = await _httpClient.GetImageAsync(nextBook.ImageUrl, true, cancellationToken);
                }
                else
                {
                    pbNextCover.Visible = false;
                    lblNextTitle.Visible = false;
                    lblNextAuthor.Visible = false;
                    lblNotInSeries.Visible = true;
                }

                await PopulateImagesFromBooks(lvAuthorRecs, ilauthorRecs, endActions.Data.AuthorRecs.Recommendations, cancellationToken);
                await PopulateImagesFromBooks(lvAuthorRecs, ilauthorRecs, endActions.Data.CustomersWhoBoughtRecs.Recommendations, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new AggregateException("Invalid EndActions file!", ex);
            }
        }

        private async Task PopulateImagesFromBooks(ListView listView, ImageList imageList, IEnumerable<Book> books, CancellationToken cancellationToken = default)
        {
            ListViewItem_SetSpacing(listView, 60 + 7, 90 + 7);

            var urls = books.Select(book => book.ImageUrl);
            var greyscaleImages = await _httpClient.GetImages(urls, true).ToArrayAsync(cancellationToken);

            var i = 0;
            foreach (var greyscaleImage in greyscaleImages)
            {
                var item = new ListViewItem
                {
                    ImageIndex = i++
                };

                listView.Items.Add(item);
                imageList.Images.Add(greyscaleImage);
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
            Dispose(true);
        }
    }
}