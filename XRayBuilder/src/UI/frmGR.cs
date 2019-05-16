using System;
using System.Diagnostics;
using System.Windows.Forms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmGR : Form
    {
        private readonly ILogger _logger;

        public BookInfo[] BookList { get; set; }

        public frmGR(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
        }

        private readonly ToolTip _toolTip1 = new ToolTip();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            var i = cbResults.SelectedIndex == -1 ? 0 : cbResults.SelectedIndex;
            try
            {
                pbCover.Image = BookList[i].CoverImage() ?? Resources.missing_image;
            }
            catch (Exception ex)
            {
                _logger.Log("Failed to download cover image: " + ex.Message);
            }
            lblTitle.Text = BookList[i].Title;
            lblAuthor.Text = "by " + BookList[i].Author;
            lblRating.Text = $"{BookList[i].AmazonRating:#.#} average rating " + Functions.Pluralize($"({BookList[i].Reviews:rating})");
            lblEditions.Text = Functions.Pluralize($"{BookList[i].Editions:edition}");
            linkID.Text = BookList[i].GoodreadsId;
            _toolTip1.SetToolTip(linkID, $"http://www.goodreads.com/book/show/{linkID.Text}");
        }

        private void linkID_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://www.goodreads.com/book/show/" + linkID.Text);
        }

        private void frmGR_Load(object sender, EventArgs e)
        {
            lblMessage1.Text = $"{BookList.Length} matches for this book were found on Goodreads.";
            cbResults.Items.Clear();
            foreach (var book in BookList)
                cbResults.Items.Add(book.Title);
            cbResults.SelectedIndex = 0;
        }
    }
}
