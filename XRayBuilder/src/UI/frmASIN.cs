using System;
using System.Windows.Forms;
using XRayBuilder.Core.DataSources.Amazon;

namespace XRayBuilderGUI.UI
{
    public partial class frmASIN : Form
    {
        public frmASIN(IAmazonClient amazonClient)
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
            if (AmazonClient.IsAsin(tbAsin.Text))
                return true;

            MessageBox.Show("This does not appear to be a valid ASIN." +
                                "\r\nAre you sure it is correct?",
                                "Invalid ASIN", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            return false;
        }
    }
}
