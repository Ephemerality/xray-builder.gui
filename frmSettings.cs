using System;
using System.Diagnostics;
using System.IO;
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
            lblVersion.Text = Functions.AppVersion();

            if (Directory.Exists(Environment.CurrentDirectory + @"\log"))
            {
                int fileCount = Directory.GetFiles(Environment.CurrentDirectory + @"\log").Length;
                if (fileCount > 0)
                {
                    btnClearLogs.Text = string.Format("Clear Logs ({0})", fileCount);
                }
                else
                {
                    btnClearLogs.Enabled = false;
                }
            }
            txtOut.Text = Properties.Settings.Default.outDir;
            txtUnpack.Text = Properties.Settings.Default.mobi_unpack;
            chkSpoilers.Checked = Properties.Settings.Default.spoilers;
            chkRaw.Checked = Properties.Settings.Default.saverawml;
            chkSoftHyphen.Checked = Properties.Settings.Default.ignoresofthyphen;
            chkUseNew.Checked = Properties.Settings.Default.useNewVersion;
            txtOffset.Text = Properties.Settings.Default.offset.ToString();
            chkAndroid.Checked = Properties.Settings.Default.android;
            txtReal.Text = Properties.Settings.Default.realName;
            txtPen.Text = Properties.Settings.Default.penName;
            chkEnableEdit.Checked = Properties.Settings.Default.enableEdit;
            chkSubDirectories.Checked = Properties.Settings.Default.useSubDirectories;
            chkUTF8.Checked = Properties.Settings.Default.utf8;
            if (txtUnpack.Text == "") txtUnpack.Text = "dist/kindleunpack.exe";
            chkAmazonUK.Checked = Properties.Settings.Default.amazonUk;
            chkOverwrite.Checked = Properties.Settings.Default.overwrite;
            chkSaveHtml.Checked = Properties.Settings.Default.saveHtml;
            chkSplitAliases.Checked = Properties.Settings.Default.splitAliases;

            // Added \r\n to show smaller tooltips
            ToolTip toolTip1 = new ToolTip();
            toolTip1.SetToolTip(chkRaw,
                "Save the .rawml (raw markup) of the book\r\nin the output directory so you can review it.");
            toolTip1.SetToolTip(chkSpoilers, "Use Shelfari descriptions that\r\ncontain spoilers when they exist.");
            toolTip1.SetToolTip(txtOffset,
                "This offset will be applied to every book location\r\n(usually a negative number). Must be an integer.");
            toolTip1.SetToolTip(chkSoftHyphen,
                "Ignore soft hyphens (Unicode U+00AD) while searching\r\nfor terms. This may slow down the parsing process slightly.");
            toolTip1.SetToolTip(chkUseNew,
                "Write the X-Ray file in the new format for\r\n" +
                "Paperwhite 2 or Voyage firmware 5.6+. If\r\n" +
                "you have one of these devices but this\r\n" +
                "does notwork, try the old format.");
            toolTip1.SetToolTip(chkAndroid,
                "Changes the naming convention of the X-Ray file\r\n"+
                "for the Android Kindle app. Forces building with\r\n" +
                "the new format. Files will be places in the output\r\n" +
                "directory within the 'Android' folder.");
            toolTip1.SetToolTip(chkUTF8, "Write the X-Ray file in UTF8 instead of ANSI.\r\n" +
                "Use this option if there are accented characters\r\n" +
                "in your book, the title, or author's name.");
            toolTip1.SetToolTip(txtReal, "Required during the EndActions.data file\r\n" +
                                         "creation. This information allows you to\r\n" +
                                         "rate this book on Amazon.");
            toolTip1.SetToolTip(txtPen, "Required during the EndActions.data file\r\n" +
                                        "creation. This information allows you to\r\n" +
                                        "rate this book on Amazon.");
            toolTip1.SetToolTip(chkAmazonUK,
                "Search Amazon.co.uk first, use Amazon.com as fallback.\r\n(Amazon.com is used if Amazon.co.uk is not selected.)");
            toolTip1.SetToolTip(chkEnableEdit,
                "Open Notepad to enable editing of detected Chapters\r\nand Aliases before final X-Ray creation.");
            toolTip1.SetToolTip(chkSubDirectories, "Save generated files to an\r\n" +
                                         "\"Author\\Filename\"\r\nsubdirectory.");
            toolTip1.SetToolTip(btnLogs, "Open the log files directory.");
            toolTip1.SetToolTip(chkOverwrite,
                "Overwrite exiting Author Profile and\r\nEnd Actions files, if they exist.\r\n(Includes alias and chapter files)");
            toolTip1.SetToolTip(chkSaveHtml, "Save parsed HTML files. This is generally used\r\n" +
                                         "for debugging and can be left unchecked.");
            toolTip1.SetToolTip(chkSplitAliases, "Automatically split character names\r\n" +
                "into aliases. This can have undesired\r\n" +
                "consequences, so use with caution!!!");
            toolTip1.SetToolTip(btnHelp, "View the included help documentation.");
        }

        private void btnBrowseUnpack_Click(object sender, EventArgs e)
        {
            txtUnpack.Text = Functions.GetExe(txtUnpack.Text);
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            txtOut.Text = Functions.GetDir(txtOut.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int offset = 0;
            if (!int.TryParse(txtOffset.Text, out offset))
            {
                MessageBox.Show("The offset must be an integer.", "Offset Error");
                return;
            }
            if (txtReal.Text.Trim().Length == 0 | txtPen.Text.Trim().Length == 0)
            {
                MessageBox.Show("Both Real and Pen names are required for\r\nEnd Action file creation.");
            }

            Properties.Settings.Default.outDir = txtOut.Text;
            Properties.Settings.Default.mobi_unpack = txtUnpack.Text;
            Properties.Settings.Default.spoilers = chkSpoilers.Checked;
            Properties.Settings.Default.saverawml = chkRaw.Checked;
            Properties.Settings.Default.ignoresofthyphen = chkSoftHyphen.Checked;
            Properties.Settings.Default.useNewVersion = chkUseNew.Checked;
            Properties.Settings.Default.android = chkAndroid.Checked;
            Properties.Settings.Default.utf8 = chkUTF8.Checked;
            Properties.Settings.Default.offset = offset;
            Properties.Settings.Default.realName = txtReal.Text;
            Properties.Settings.Default.penName = txtPen.Text;
            Properties.Settings.Default.amazonUk = chkAmazonUK.Checked;
            Properties.Settings.Default.enableEdit = chkEnableEdit.Checked;
            Properties.Settings.Default.useSubDirectories = chkSubDirectories.Checked;
            Properties.Settings.Default.overwrite = chkOverwrite.Checked;
            Properties.Settings.Default.saveHtml = chkSaveHtml.Checked;
            Properties.Settings.Default.splitAliases = chkSplitAliases.Checked;
            Properties.Settings.Default.Save();

            this.Close();
        }

        private void chkAndroid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAndroid.Checked == true)
            {
                chkUseNew.Checked = true;
                chkUseNew.Enabled = false;
                chkSubDirectories.Enabled = false;
            }
            else
            {
                chkUseNew.Enabled = true;
                chkSubDirectories.Enabled = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes ==
                MessageBox.Show("Are you sure you want to delete all log files?\r\nThis action can not be undone.",
                    "Are you sure...",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2))
            {
                try
                {
                    Array.ForEach(Directory.GetFiles(Environment.CurrentDirectory + @"\log"), File.Delete);
                    btnClearLogs.Text = "Clear Logs";
                    btnClearLogs.Enabled = false;
                }
                catch (Exception)
                {
                    MessageBox.Show("An error occurred while trying to delete log files.", "Unable to delete Log files");
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\doc\readme.txt"))
            {
                MessageBox.Show("Unable to find the included help file.",
                    "Help file not found...");
            }
            else
                Process.Start(Environment.CurrentDirectory + @"\doc\readme.txt");
        }
    }
}