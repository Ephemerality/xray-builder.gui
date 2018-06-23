using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI
{
    public partial class frmGR : Form
    {
        public List<BookInfo> BookList = new List<BookInfo>();

        public frmGR()
        {
            InitializeComponent();
        }

        ToolTip toolTip1 = new ToolTip();

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
            pbCover.Image = Resources.missing_image;
            lblTitle.Text = "";
            lblAuthor.Text = "";
            lblRating.Text = "";
            lblEditions.Text = "";
            linkID.Text = "";

            int i = cbResults.SelectedIndex == -1 ? 0 : cbResults.SelectedIndex;
            pbCover.Image = BookList[i].CoverImage();
            lblTitle.Text = BookList[i].title;
            lblAuthor.Text = "by " + BookList[i].author;
            lblRating.Text = String.Format("{0} average rating ({1} ratings)", BookList[i].amazonRating, BookList[i].numReviews);
            lblEditions.Text = String.Format(BookList[i].editions == "1" ? "{0} edition" : "{0} editions", BookList[i].editions);
            linkID.Text = BookList[i].goodreadsID;
            toolTip1.SetToolTip(linkID, @"http://www.goodreads.com/book/show/" + linkID.Text);
        }

        private void linkID_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://www.goodreads.com/book/show/" + linkID.Text);
        }

        private void frmGR_Load(object sender, EventArgs e)
        {
            lblMessage1.Text = String.Format("{0} matches for this book were found of Goodreads.", BookList.Count);
            cbResults.Items.Clear();
            foreach (BookInfo book in BookList)
                cbResults.Items.Add(book.title);
            cbResults.SelectedIndex = 0;
        }
    }
}
