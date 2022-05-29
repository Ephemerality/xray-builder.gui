using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Language.Pluralization;

namespace XRayBuilderGUI.UI
{
    public partial class frmAuthorList : Form
    {
        private readonly List<AuthorSearchResults> _authorList;
        private readonly IHttpClient _httpClient;
        private readonly ToolTip _tooltip = new();

        public frmAuthorList(List<AuthorSearchResults> authorList, IHttpClient httpClient)
        {
            InitializeComponent();
            _authorList = authorList;
            _httpClient = httpClient;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape)
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
            dgvOtherBooks.Rows.Clear();
            var i = cbResults.SelectedIndex == -1 ? 0 : cbResults.SelectedIndex;
            lblBiography.Text = _authorList[i].Biography;
            if (_authorList[i].Books != null)
                foreach (var book in _authorList[i].Books)
                    dgvOtherBooks.Rows.Add($" {book.Title}");
        }

        private void frmAuthorList_Load(object sender, EventArgs e)
        {
            lblMessage1.Text = $@"{PluralUtil.Pluralize($"{_authorList.Count:author}")} were found on Amazon with this name.";
            cbResults.Items.Clear();
            foreach (var author in _authorList)
                cbResults.Items.Add(author.Asin);
            cbResults.SelectedIndex = 0;
            _tooltip.SetToolTip(linkStore, "Visit this author's page on Amazon.");
        }

        private void linkStore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(_authorList[cbResults.SelectedIndex].Url) { UseShellExecute = true });
        }

        private void dgvOtherBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _authorList[cbResults.SelectedIndex].Books != null)
                Process.Start(new ProcessStartInfo(_authorList[cbResults.SelectedIndex].Books[e.RowIndex].AmazonUrl) { UseShellExecute = true });
        }
    }
}