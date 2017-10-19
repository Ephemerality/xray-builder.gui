using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
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
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
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
            WebRequest request = WebRequest.Create(BookList[i].bookImageUrl);
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    Bitmap bitmap = new Bitmap(stream);
                    pbCover.Image = bitmap;
                }
            }
            lblTitle.Text = BookList[i].title;
            lblAuthor.Text = String.Format("by {0}", BookList[i].author);
            lblRating.Text = String.Format("{0} average rating ({1} ratings)", BookList[i].amazonRating, BookList[i].numReviews);
            lblEditions.Text = BookList[i].editions == "1" ?
                String.Format("{0} edition", BookList[i].editions) :
                String.Format("{0} editions", BookList[i].editions);
            linkID.Text = BookList[i].goodreadsID;
            toolTip1.SetToolTip(linkID, String.Format(@"http://www.goodreads.com/book/show/{0}", linkID.Text));
        }

        private void linkID_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = String.Format(@"http://www.goodreads.com/book/show/{0}", linkID.Text);
            Process.Start(link);
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
