using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            txtOut.Text = XRayBuilderGUI.Properties.Settings.Default.outDir;
            txtUnpack.Text = XRayBuilderGUI.Properties.Settings.Default.mobi_unpack;
            chkSpoilers.Checked = XRayBuilderGUI.Properties.Settings.Default.spoilers;
            chkRaw.Checked = XRayBuilderGUI.Properties.Settings.Default.saverawml;
            if (txtUnpack.Text == "") txtUnpack.Text = "dist/kindleunpack.exe";

            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(chkRaw, "Save the .rawml (raw markup) of the book in the output directory so you can review it.");
            ToolTip1.SetToolTip(chkSpoilers, "Use Shelfari descriptions that contain spoilers when they exist.");
        }

        private void btnBrowseUnpack_Click(object sender, EventArgs e)
        {
            txtUnpack.Text = Functions.getFile(txtUnpack.Text);
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            txtOut.Text = Functions.getDir(txtOut.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            XRayBuilderGUI.Properties.Settings.Default.outDir = txtOut.Text;
            XRayBuilderGUI.Properties.Settings.Default.mobi_unpack = txtUnpack.Text;
            XRayBuilderGUI.Properties.Settings.Default.spoilers = chkSpoilers.Checked;
            XRayBuilderGUI.Properties.Settings.Default.saverawml = chkRaw.Checked;
            XRayBuilderGUI.Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
