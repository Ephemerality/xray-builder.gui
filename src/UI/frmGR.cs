using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmGR : Form
    {
        public List<BookInfo> BookList = new List<BookInfo>();

        public frmGR()
        {
            InitializeComponent();
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
            int i = cbResults.SelectedIndex == -1 ? 0 : cbResults.SelectedIndex;
            pbCover.Image = BookList[i].CoverImage() ?? Resources.missing_image;
            lblTitle.Text = BookList[i].title;
            lblAuthor.Text = "by " + BookList[i].author;
            lblRating.Text = $"{BookList[i].amazonRating:#.#} average rating " + Functions.Pluralize($"({BookList[i].numReviews:rating})");
            lblEditions.Text = Functions.Pluralize($"{BookList[i].editions:edition}");
            linkID.Text = BookList[i].goodreadsID;
            _toolTip1.SetToolTip(linkID, $"http://www.goodreads.com/book/show/{linkID.Text}");
        }

        private void linkID_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://www.goodreads.com/book/show/" + linkID.Text);
        }

        private void frmGR_Load(object sender, EventArgs e)
        {
            lblMessage1.Text = $"{BookList.Count} matches for this book were found on Goodreads.";
            cbResults.Items.Clear();
            foreach (BookInfo book in BookList)
                cbResults.Items.Add(book.title);
            cbResults.SelectedIndex = 0;
        }
    }
}
