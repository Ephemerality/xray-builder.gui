using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
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
            chkSoftHyphen.Checked = XRayBuilderGUI.Properties.Settings.Default.ignoresofthyphen;
            chkUseNew.Checked = XRayBuilderGUI.Properties.Settings.Default.useNewVersion;
            txtOffset.Text = XRayBuilderGUI.Properties.Settings.Default.offset.ToString();
            chkAndroid.Checked = XRayBuilderGUI.Properties.Settings.Default.android;
            chkUTF8.Checked = XRayBuilderGUI.Properties.Settings.Default.utf8;
            chkEnableEdit.Checked = XRayBuilderGUI.Properties.Settings.Default.enableEdit;
            chkSubDirectories.Checked = XRayBuilderGUI.Properties.Settings.Default.useSubDirs;
            if (txtUnpack.Text == "") txtUnpack.Text = "dist/kindleunpack.exe";

            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(chkRaw, "Save the .rawml (raw markup) of the book in the output directory so you can review it.");
            ToolTip1.SetToolTip(chkSpoilers, "Use Shelfari descriptions that contain spoilers when they exist.");
            ToolTip1.SetToolTip(txtOffset, "This offset will be applied to every book location (usually a negative number). Must be an integer.");
            ToolTip1.SetToolTip(chkSoftHyphen, "Ignore soft hyphens (Unicode U+00AD) while searching for terms. This may slow down the parsing process slightly.");
            ToolTip1.SetToolTip(chkUseNew, "Write the X-Ray file in the new format for Paperwhite 2 / Voyage firmware 5.6+.\r\nMassively slower than the normal build process and is still in alpha testing.");
            ToolTip1.SetToolTip(chkAndroid, "Changes the naming convention of the X-Ray file for the Android Kindle app. Forces building with the new format.");
            ToolTip1.SetToolTip(chkUTF8, "Write the X-Ray file in UTF8 instead of ANSI.");
            ToolTip1.SetToolTip(txtReal, "Required for the EndActions.data file creation.");
            ToolTip1.SetToolTip(txtPen, "Required for the EndActions.data file creation.");
            ToolTip1.SetToolTip(chkAmazonUK, "Search Amazon.co.uk first, use Amazon.com as fallback.\r\n(Amazon.com is used if Amazon.co.uk is not selected.)");
            ToolTip1.SetToolTip(chkEnableEdit, "Open Notepad to enable editing of detected Chapters\r\nand Aliases before final X-Ray creation.");
            ToolTip1.SetToolTip(chkSubDirectories, "Save generated files to an \"Author - Book\" subdirectory within the defined output directory.");
            ToolTip1.SetToolTip(btnLogs, "Open the log files directory.");
            this.TopMost = true;
        }

        private void btnBrowseUnpack_Click(object sender, EventArgs e)
        {
            txtUnpack.Text = Functions.getFile(txtUnpack.Text);
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            txtOut.Text = Functions.getDir(txtOut.Text);
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\log"))
            {
                MessageBox.Show("Log directory does not exist.", "Logs Directory Not found");
                return;
            }
            else
                this.TopMost = false;
            Process.Start(Environment.CurrentDirectory + @"\log");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            int offset = 0;
            if (!int.TryParse(txtOffset.Text, out offset))
            {
                MessageBox.Show("The offset must be an integer.", "Offset Error");
                e.Cancel = true;
                return;
            }
            XRayBuilderGUI.Properties.Settings.Default.outDir = txtOut.Text;
            XRayBuilderGUI.Properties.Settings.Default.mobi_unpack = txtUnpack.Text;
            XRayBuilderGUI.Properties.Settings.Default.spoilers = chkSpoilers.Checked;
            XRayBuilderGUI.Properties.Settings.Default.saverawml = chkRaw.Checked;
            XRayBuilderGUI.Properties.Settings.Default.ignoresofthyphen = chkSoftHyphen.Checked;
            XRayBuilderGUI.Properties.Settings.Default.useNewVersion = chkUseNew.Checked;
            XRayBuilderGUI.Properties.Settings.Default.android = chkAndroid.Checked;
            XRayBuilderGUI.Properties.Settings.Default.utf8 = chkUTF8.Checked;
            XRayBuilderGUI.Properties.Settings.Default.offset = offset;
            XRayBuilderGUI.Properties.Settings.Default.Save();
        }

        private void chkAndroid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAndroid.Checked == true)
            {
                chkUseNew.Checked = true;
                chkUseNew.Enabled = false;
            }
            else
                chkUseNew.Enabled = true;
        }
    }
}
