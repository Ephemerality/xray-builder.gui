using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Http;
using XRayBuilder.Core.Libraries.Language.Pluralization;

namespace XRayBuilderGUI.UI
{
    public partial class frmAuthorList : Form
    {
        private readonly List<AuthorSearchResults> _authorList;
        private readonly ToolTip _tooltip = new();

        public frmAuthorList(List<AuthorSearchResults> authorList, IHttpClient httpClient)
        {
            InitializeComponent();
            _authorList = authorList;
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

        private void frmAuthorList_Load(object sender, EventArgs e)
        {
            lblMessage1.Text = $@"{PluralUtil.Pluralize($"{_authorList.Count:author}")} were found on Amazon.";
            cbResults.Items.Clear();
            foreach (var author in _authorList)
                cbResults.Items.Add(author.Name);
            cbResults.SelectedIndex = 0;
            _tooltip.SetToolTip(linkStore, "Visit this author's page on Amazon.");
        }

        private void linkStore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Functions.ShellExecute(_authorList[cbResults.SelectedIndex].Url);
        }
    }
}