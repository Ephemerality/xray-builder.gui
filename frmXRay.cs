using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace XRayBuilderGUI
{
    public partial class frmXRay : Form
    {
        XRay xray;
        public frmXRay(XRay xray, string rawML)
        {
            InitializeComponent();
            this.xray = xray;

            HtmlAgilityPack.HtmlDocument web = new HtmlAgilityPack.HtmlDocument();

            string readContents;
            using (StreamReader streamReader = new StreamReader(rawML, Encoding.Default))
            {
                readContents = streamReader.ReadToEnd();
            }
            web.LoadHtml(readContents);

        }

        private void frmXRay_Load(object sender, EventArgs e)
        {
            foreach (XRay.Term t in xray.terms)
            {
                lstTerms.Items.Add(t.termName);
            }
            lstTerms.SelectedIndex = 0;
        }

        private void lstTerms_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtDesc.Text = xray.terms[lstTerms.SelectedIndex].desc;

        }
    }
}
