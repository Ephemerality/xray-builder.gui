using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    public partial class frmASIN : Form
    {
        public string thisAsin = "";

        public frmASIN()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (CheckAsin())
                Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (CheckAsin())
                    Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool CheckAsin()
        {
            Match validASIN = Regex.Match(tbAsin.Text, "(B[A-Z0-9]{9})");
            if (validASIN.Success)
            {
                thisAsin = validASIN.Value;
                return true;
            }
            MessageBox.Show("This does not appear to be a valid ASIN." +
                                "\r\nAre you sure it is correct?",
                                "Invalid ASIN", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            return false;
        }
    }
}
