using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ephemerality.Unpack;
using SimpleInjector;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Logic;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilderGUI.Localization.Main;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmBookInfo : Form
    {
        private readonly IAmazonClient _amazonClient;
        private readonly CancellationTokenSource _cancelTokens = new();
        private readonly Container _diContainer;
        private readonly IHttpClient _httpClient;
        private readonly Settings _settings = Settings.Default;

        private string _bioFile = "";
        private ISecondarySource _dataSource;
        private IMetadata _metadata;
        private Image _cover;

        public DialogData Result { get; private set; }

        private readonly ToolTip _tooltip = new();
        private string secondaryDataSourceUrl;

        public frmBookInfo(IHttpClient httpClient, Container diContainer, IAmazonClient amazonClient)
        {
            InitializeComponent();
            _diContainer = diContainer;
            _httpClient = httpClient;
            _amazonClient = amazonClient;
        }

        public void Setup(IMetadata metadata, Image coverImage, DialogData dialogData)
        {
            _metadata = metadata;
            _cover = coverImage;
            txtAuthorUrl.Text = dialogData.AuthorUrl;
            txtBio.Text = dialogData.AuthorBiography;

            txtDataProviderUrl.Text = dialogData.SecondarySourceUrl;
            secondaryDataSourceUrl = dialogData.SecondarySourceUrl;
            cmbSecondaryDataSource.Text = Settings.Default.dataSource;

            _tooltip.SetToolTip(btnAuthorUrlSearch, $"Search for {_metadata.Author}\r\non Amazon.{_settings.amazonTLD}.");
            _tooltip.SetToolTip(btnBookUrlSearch, $"Search for {_metadata.Title}\r\non Amazon.{_settings.amazonTLD}.");
            _tooltip.SetToolTip(btnAuthorUrlSearch, $"Search for {_metadata.Author}\r\non Amazon.{_settings.amazonTLD}.");
            _tooltip.SetToolTip(cmbSecondaryDataSource, MainStrings.SecondarySourceTooltip);

            foreach (var button in Controls.OfType<Button>())
            {
                if (button.Name.Contains("UrlLink"))
                    _tooltip.SetToolTip(button, "Open this link in your\r\ndefault browser.");
            }
        }

        public sealed record DialogData(string SecondarySource, string SecondarySourceUrl, string AuthorUrl, string AuthorBiography);

        private void CheckStatus()
        {
            Cursor = Cursors.Default;
            foreach (var c in Controls.OfType<TextBox>())
            {
                c.BackColor = string.IsNullOrEmpty(c.Text)
                    ? Color.FromArgb(255, 235, 235)
                    : Color.White;
            }

            if (Controls.OfType<TextBox>().Any(t => string.IsNullOrEmpty(t.Text)))
            {
                SetStatus("Use the search buttons to update book information…");
                btnOK.Enabled = false;
            }
            else
            {
                SetStatus("Book information complete! Click OK.");
                btnOK.Enabled = true;
            }
        }

        private void frmBookInfo_Load(object sender, EventArgs e)
        {
            // Safety check
            if (_metadata == null)
                throw new InvalidOperationException("Cannot load form before Setup is called");

            var dataSource = (SecondaryDataSourceFactory.Enum) Enum.Parse(typeof(SecondaryDataSourceFactory.Enum), _settings.dataSource);
            _dataSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(dataSource);

            ActiveControl = btnCancel;
            Text = $@"{_metadata.Title} by {_metadata.Author}";
            pbCover.Image = _cover;
            txtBookUrl.Text = $@"https://www.amazon.{_settings.amazonTLD}/dp/{_metadata.Asin}";

            CheckStatus();
        }

        private async void btnDataProviderSearch_Click(object sender, EventArgs e)
        {
            await SearchDataProvider();
        }

        private async Task SearchDataProvider()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var bookSearchService = _diContainer.GetInstance<IBookSearchService>();
                SetStatus($"Searching {_dataSource.Name}…");
                var books = await bookSearchService.SearchSecondarySourceAsync(_dataSource, _metadata, _cancelTokens.Token);
                if (books.Length <= 0)
                {
                    SetStatus($"Unable to find {_metadata.Title} on {_dataSource.Name}…");
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
                            SetStatus($"Failed to download cover image for {book.Title} by {book.Author}.");
                        }

                    SetStatus($"Warning: Multiple results returned from {_dataSource.Name}…");
                    var frmG = new frmGR(books, _dataSource);
                    frmG.ShowDialog();
                    bookUrl = books[frmG.cbResults.SelectedIndex].DataUrl;
                }

                if (!string.IsNullOrEmpty(bookUrl))
                    txtDataProviderUrl.Text = bookUrl;
            }
            catch (Exception)
            {
                // Ignored?
            }
            finally
            {
                CheckStatus();
            }
        }

        private void SetStatus(string text)
        {
            StatusLabel.Text = text;
            statusStrip.Refresh();
        }

        private async void btnAuthorUrlSearch_Click(object sender, EventArgs e)
        {
            await SearchAuthor();
            CheckStatus();
        }

        private async Task SearchAuthor()
        {
            AuthorSearchResults searchResults = null;
            try
            {
                Cursor = Cursors.WaitCursor;
                SetStatus($@"Searching Amazon.{_settings.amazonTLD}…");
                searchResults = await _amazonClient.SearchAuthor(_metadata.Author, _settings.amazonTLD, _cancelTokens.Token, false);
            }
            catch (Exception)
            {
                SetStatus($@"Error searching Amazon.{_settings.amazonTLD}.");
            }
            finally
            {
                if (searchResults == null)
                {
                    SetStatus($"Failed to find {_metadata.Author} on Amazon.{_settings.amazonTLD}.");
                    if (_settings.amazonTLD != "com")
                    {
                        SetStatus("Trying again with Amazon.com.");
                        searchResults = await _amazonClient.SearchAuthor(_metadata.Author, "com", _cancelTokens.Token, false);
                    }
                }
            }

            if (searchResults == null)
            {
                SetStatus($"Failed to find {_metadata.Author} on Amazon.");
                return;
            }

            SetStatus($"{_metadata.Author} page found  on Amazon.{_settings.amazonTLD}.");
            txtAuthorUrl.Text = searchResults.Url;

            if (_settings.saveBio && File.Exists(_bioFile))
            {
                using var streamReader = new StreamReader(_bioFile, Encoding.UTF8);
                txtBio.Text = await streamReader.ReadToEndAsync();
            }
            else
            {
                txtBio.Text = searchResults.Biography;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            File.WriteAllText(_bioFile, txtBio.Text);
            Result = new DialogData(_dataSource.Name, txtDataProviderUrl.Text, txtAuthorUrl.Text, txtBio.Text);
            Settings.Default.dataSource = cmbSecondaryDataSource.Text;
            Settings.Default.Save();
            Close();
        }

        private void txtAuthorUrl_TextChanged(object sender, EventArgs e)
        {
            _bioFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext", $"{_amazonClient.ParseAsin(txtAuthorUrl.Text)}.bio");
        }

        private void OpenLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred opening this link:{Environment.NewLine}{ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnDataProviderUrlLink_Click(object sender, EventArgs e)
        {
            OpenLink(txtDataProviderUrl.Text);
        }

        private async void btnBookUrlSearch_Click(object sender, EventArgs e)
        {
            await SearchBook();
            CheckStatus();
        }

        private async Task SearchBook()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                SetStatus($@"Searching Amazon.{_settings.amazonTLD}…");

               var amazonSearchResult = await _amazonClient.SearchBook(_metadata.Title, _metadata.Author, _settings.amazonTLD, _cancelTokens.Token);
                if (amazonSearchResult != null)
                {
                    SetStatus($"{_metadata.Title} page found  on Amazon.{_settings.amazonTLD}.");
                    txtBookUrl.Text = amazonSearchResult.AmazonUrl;
                }
            }
            catch (Exception)
            {
                SetStatus($@"Error searching Amazon.{_settings.amazonTLD}.");
            }
        }

        private void AdjustUi()
        {
            lblDataProviderUrl.Location = new Point(txtDataProviderUrl.Location.X - lblDataProviderUrl.Width - 7, lblDataProviderUrl.Location.Y);
        }

        private void cmbSecondaryDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dataProvider = cmbSecondaryDataSource.Text;
            lblDataProviderUrl.Text = $@"{dataProvider} URL:";
            txtDataProviderUrl.Text = secondaryDataSourceUrl.Contains(dataProvider.ToLower()) && !string.IsNullOrEmpty(secondaryDataSourceUrl) ? secondaryDataSourceUrl : string.Empty;
            _tooltip.SetToolTip(btnDataProviderSearch, $"Search for {_metadata.Title} on {dataProvider}.");

            var dataSource = (SecondaryDataSourceFactory.Enum) Enum.Parse(typeof(SecondaryDataSourceFactory.Enum), dataProvider);
            _dataSource = _diContainer.GetInstance<SecondaryDataSourceFactory>().Get(dataSource);

            AdjustUi();
            CheckStatus();
        }
    }
}