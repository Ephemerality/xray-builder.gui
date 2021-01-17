using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleInjector;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Logic;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmBookInfo : Form
    {
        private readonly IAmazonClient _amazonClient;
        private readonly CancellationTokenSource _cancelTokens = new CancellationTokenSource();
        private readonly Container _diContainer;
        private readonly IHttpClient _httpClient;
        private readonly Settings _settings = Settings.Default;

        private string _bioFile = "";
        private ISecondarySource _dataSource;

        public IMetadata Book;
        public Image Cover;

        public frmBookInfo(
            IHttpClient httpClient,
            Container diContainer,
            IAmazonClient amazonClient)
        {
            InitializeComponent();
            _diContainer = diContainer;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape) return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        private void CheckStatus()
        {
            Cursor = Cursors.Default;
            foreach (var c in Controls.OfType<TextBox>())
                c.BackColor = string.IsNullOrEmpty(c.Text) ? Color.FromArgb(255, 235, 235) : Color.White;

            if (Controls.OfType<TextBox>().Any(t => string.IsNullOrEmpty(t.Text)))
            {
                SetStatus(@"Use the search buttons to update book information…");
                btnOK.Enabled = false;
            }
            else
            {
                SetStatus(@"Book information complete!");
                btnOK.Enabled = true;
            }
        }

        private void frmBookInfo_Load(object sender, EventArgs e)
        {
            //txtGoodreadsUrl.DataBindings.Add("Text", Data, "GoodreadsUrl", true,
            //    DataSourceUpdateMode.OnPropertyChanged);
            //txtAuthorUrl.DataBindings.Add("Text", Data, "AuthorUrl", true, DataSourceUpdateMode.OnPropertyChanged);
            //txtBio.DataBindings.Add("Text", Data, "AuthorBio", true, DataSourceUpdateMode.OnPropertyChanged);
            //cbXraySource.DataBindings.Add("SelectedItem", Data, "XraySource", true,
            //    DataSourceUpdateMode.OnPropertyChanged);
            //txtBookUrl.DataBindings.Add("Text", Data, "BookUrl", true, DataSourceUpdateMode.OnPropertyChanged);

            var ds = (SecondaryDataSourceFactory.Enum) Enum.Parse(typeof(SecondaryDataSourceFactory.Enum),
                _settings.dataSource);
            _dataSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(ds);

            ActiveControl = btnCancel;
            Text = $@"{Book.Title} by {Book.Author}";
            pbCover.Image = Cover;
            txtBookUrl.Text = $@"https://www.amazon.{_settings.amazonTLD}/dp/{Book.Asin}";

            CheckStatus();
        }

        private async void btnGoodreadsSearch_Click(object sender, EventArgs e)
        {
            await SearchGoodreads();
        }

        private async Task SearchGoodreads()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var bookSearchService = _diContainer.GetInstance<IBookSearchService>();
                SetStatus(@$"Searching {_dataSource.Name}…");
                var books = await bookSearchService.SearchSecondarySourceAsync(_dataSource, Book, _cancelTokens.Token);
                if (books.Length <= 0)
                {
                    SetStatus(@$"Unable to find {Book.Title} on {_dataSource.Name}…");
                    CheckStatus();
                    return;
                }

                string bookUrl;
                if (books.Length == 1)
                {
                    bookUrl = books[0].DataUrl;
                }
                else
                {
                    foreach (var book in books.Where(book => !string.IsNullOrEmpty(book.ImageUrl)))
                        try
                        {
                            book.CoverImage = await _httpClient.GetImageAsync(book.ImageUrl,
                                cancellationToken: _cancelTokens.Token);
                        }
                        catch (Exception)
                        {
                            SetStatus(@"Failed to download cover image.");
                        }

                    SetStatus($@"Warning: Multiple results returned from {_dataSource.Name}…");
                    var frmG = new frmGR(books, _dataSource);
                    frmG.ShowDialog();
                    bookUrl = books[frmG.cbResults.SelectedIndex].DataUrl;
                }
            }
            catch (Exception)
            {
                CheckStatus();
            }

            CheckStatus();
        }

        private void SetStatus(string text)
        {
            StatusLabel.Text = text;
            statusStrip.Refresh();
        }

        private async void btnAuthorUrlSearch_Click(object sender, EventArgs e)
        {
            await SearchAuthor();
        }

        private async Task SearchAuthor()
        {
            AuthorSearchResults searchResults = null;
            try
            {
                Cursor = Cursors.WaitCursor;
                SetStatus($@"Searching Amazon.{_settings.amazonTLD}…");
                searchResults = await _amazonClient.SearchAuthor(Book.Author, Book.Asin, _settings.amazonTLD,
                    _cancelTokens.Token, false);
            }
            catch (Exception)
            {
                SetStatus($@"Error searching Amazon.{_settings.amazonTLD}.");
                CheckStatus();
            }
            finally
            {
                if (searchResults == null)
                {
                    SetStatus($"Failed to find {Book.Author} on Amazon.{_settings.amazonTLD}.");
                    if (_settings.amazonTLD != "com")
                    {
                        SetStatus("Trying again with Amazon.com.");
                        searchResults = await _amazonClient.SearchAuthor(Book.Author, Book.Asin, "com",
                            _cancelTokens.Token, false);
                    }
                }
            }

            if (searchResults == null)
            {
                SetStatus($"Failed to find {Book.Author} on Amazon.");
                CheckStatus();
                return;
            }

            CheckStatus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            File.WriteAllText(_bioFile, txtBio.Text);
            Close();
        }

        private void txtAuthorUrl_TextChanged(object sender, EventArgs e)
        {
            _bioFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext",
                $"{_amazonClient.ParseAsin(txtAuthorUrl.Text)}.bio");
        }

        private void OpenLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"An error occured opening this link:" +
                                Environment.NewLine + $@"{ex.Message}",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAuthorUrlLink_Click(object sender, EventArgs e)
        {
            OpenLink(txtAuthorUrl.Text);
        }

        private void btnBookUrlLink_Click(object sender, EventArgs e)
        {
            OpenLink(txtBookUrl.Text);
        }

        private void btnGoodreadsUrlLink_Click(object sender, EventArgs e)
        {
            OpenLink(txtGoodreadsUrl.Text);
        }

        private void cbXraySource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbXraySource.SelectedItem.ToString() == "XML") txtXMLFile.Enabled = true;
        }
    }
}