using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Language.Pluralization;
using XRayBuilder.Core.Model;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmGR : Form
    {
        private readonly BookInfo[] _bookList;

        public frmGR(BookInfo[] bookList, ISecondarySource source)
        {
            InitializeComponent();
            _bookList = bookList;
            lblID.Text = $"{source.Name} ID:";
            linkID.Location = new Point(lblID.Location.X + lblID.Width - 4, linkID.Location.Y);
        }

        private readonly ToolTip _toolTip1 = new ToolTip();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Enter)
                return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            var i = cbResults.SelectedIndex == -1 ? 0 : cbResults.SelectedIndex;
            pbCover.Image = _bookList[i].CoverImage ?? Resources.missing_image;
            lblTitle.Text = _bookList[i].Title;
            lblAuthor.Text = $"by {_bookList[i].Author}";
            lblRating.Text = $"{_bookList[i].AmazonRating:#.#} average rating " + PluralUtil.Pluralize($"({_bookList[i].Reviews:rating})");
            lblEditions.Text = PluralUtil.Pluralize($"{_bookList[i].Editions:edition}");
            linkID.Text = _bookList[i].GoodreadsId;
            _toolTip1.SetToolTip(linkID, _bookList[i].DataUrl);
        }

        private void linkID_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(_toolTip1.GetToolTip(linkID));
        }

        private void frmGR_Load(object sender, EventArgs e)
        {
            lblMessage1.Text = $"{_bookList.Length} matches for this book were found on Goodreads.";
            cbResults.Items.Clear();
            foreach (var book in _bookList)
                cbResults.Items.Add(book.Title);
            cbResults.SelectedIndex = 0;
        }
    }
}
