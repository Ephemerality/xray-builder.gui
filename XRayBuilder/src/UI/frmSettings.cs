using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using XRayBuilderGUI.Libraries.Enumerables.Extensions;

namespace XRayBuilderGUI.UI
{
    public partial class frmSettings : Form
    {
        // TODO: Should be elsewhere maybe
        private readonly Dictionary<string, string> regionTLDs = new Dictionary<string, string> {
            { "Australia", "com.au" }, { "Brazil", "com.br" }, { "Canada", "ca" }, { "China", "cn" },
            { "France", "fr" }, { "Germany", "de" }, { "India", "in" }, { "Italy", "it" }, { "Japan", "co.jp" },
            { "Mexico", "com.mx" }, { "Netherlands", "nl" }, { "Spain", "es" }, { "USA", "com" }, { "UK", "co.uk" }
        };

        public frmSettings()
        {
            InitializeComponent();
        }

        class AmazonRegion
        {
            public string Name { get; }
            public string TLD { get; }

            public AmazonRegion(string name, string tld)
            {
                Name = name;
                TLD = tld;
            }
        }

        //http://stackoverflow.com/questions/2612487/how-to-fix-the-flickering-in-user-controls
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void frmSettingsNew_Load(object sender, EventArgs e)
        {
            listSettings.SelectedIndex = 0;

            if (Directory.Exists(Environment.CurrentDirectory + @"\log"))
            {
                var fileCount = Directory.GetFiles(Environment.CurrentDirectory + @"\log").Length;
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
            chkRaw.Checked = Properties.Settings.Default.saverawml;
            chkSoftHyphen.Checked = Properties.Settings.Default.ignoresofthyphen;
            chkUseNew.Checked = Properties.Settings.Default.useNewVersion;
            txtOffset.Text = Properties.Settings.Default.offset.ToString();
            chkAndroid.Checked = Properties.Settings.Default.android;
            txtReal.Text = Properties.Settings.Default.realName;
            txtPen.Text = Properties.Settings.Default.penName;
            chkEnableEdit.Checked = Properties.Settings.Default.enableEdit;
            chkSubDirectories.Checked = Properties.Settings.Default.useSubDirectories;
            chkSkipNoLikes.Checked = Properties.Settings.Default.skipNoLikes;
            txtMinClipLen.Text = Properties.Settings.Default.minClipLen.ToString();
            chkOverwrite.Checked = Properties.Settings.Default.overwrite;
            chkAlias.Checked = Properties.Settings.Default.overwriteAliases;
            chkChapters.Checked = Properties.Settings.Default.overwriteChapters;
            chkSaveHtml.Checked = Properties.Settings.Default.saveHtml;
            chkSplitAliases.Checked = Properties.Settings.Default.splitAliases;
            chkSound.Checked = Properties.Settings.Default.playSound;
            chkDownloadAliases.Checked = Properties.Settings.Default.downloadAliases;
            chkOverrideOffset.Checked = Properties.Settings.Default.overrideOffset;
            txtAZWOffset.Text = Properties.Settings.Default.offsetAZW3.ToString();
            chkPageCount.Checked = Properties.Settings.Default.pageCount;
            chkSaveBio.Checked = Properties.Settings.Default.saveBio;
            if (Properties.Settings.Default.dataSource == "Goodreads")
                rdoGoodreads.Checked = true;
            else
                rdoShelfari.Checked = true;
            chkPromptAsin.Checked = Properties.Settings.Default.promptASIN;
            chkDownloadSA.Checked = Properties.Settings.Default.downloadSA;
            chkSearchAsin.Checked = Properties.Settings.Default.searchByAsin;
            chkEditBiography.Checked = Properties.Settings.Default.editBiography;
            chkUseSidecar.Checked = Properties.Settings.Default.outputToSidecar;

            // Added \r\n to show smaller tooltips
            var toolTip1 = new ToolTip();
            toolTip1.SetToolTip(chkRaw,
                "Save the rawML (raw markup) of the book\r\nin the output directory so you can review it.");
            toolTip1.SetToolTip(txtOffset,
                "This offset will be applied to every\r\nbook location (usually a negative\r\nnumber). Must be an integer.");
            toolTip1.SetToolTip(chkSoftHyphen,
                "Ignore soft hyphens (Unicode U+00AD)\r\n" +
                "while searching for terms. This may\r\n" +
                "slow down the parsing process slightly.");
            toolTip1.SetToolTip(chkUseNew,
                "Write the X-Ray file in the new format for\r\n" +
                "Paperwhite 2 or Voyage firmware 5.6+. If\r\n" +
                "you have one of these devices but this\r\n" +
                "does not work, try the old format.");
            toolTip1.SetToolTip(chkAndroid,
                "Changes the naming convention of the X-Ray file\r\n"+
                "for the Android Kindle app. Forces building with\r\n" +
                "the new format. Files will be placed in the output\r\n" +
                "directory within the 'Android' folder.");
            toolTip1.SetToolTip(chkSkipNoLikes, "Skip notable clips with no likes.\r\n" +
                                                "Good for filtering out garbage quotes from Goodreads.");
            toolTip1.SetToolTip(txtMinClipLen, "Minimum length for notable clips.\r\n" +
                                               "Good for filtering out garbage quotes from Goodreads.");
            toolTip1.SetToolTip(txtReal, "Required during the EndActions.data file\r\n" +
                                         "creation. This information allows you to\r\n" +
                                         "rate this book on Amazon.");
            toolTip1.SetToolTip(txtPen, "Required during the EndActions.data file\r\n" +
                                        "creation. This information allows you to\r\n" +
                                        "rate this book on Amazon.");
            toolTip1.SetToolTip(chkEnableEdit,
                "Open Notepad to enable editing of detected Chapters\r\nand Aliases before final X-Ray creation.");
            toolTip1.SetToolTip(chkSubDirectories, "Save generated files to an\r\n\"Author\\Filename\" subdirectory.");
            toolTip1.SetToolTip(chkUseSidecar, "Save generated files to a sidecar subdirectory based on the filename.");
            toolTip1.SetToolTip(btnLogs, "Open the log files directory.");
            toolTip1.SetToolTip(chkOverwrite, "Overwrite existing Author Profile,\r\nStart and End Actions files.");
            toolTip1.SetToolTip(chkAlias, "Overwrite existing alias files.");
            toolTip1.SetToolTip(chkChapters, "Overwrite existing chapter files.");
            toolTip1.SetToolTip(chkSaveHtml, "Save parsed HTML files. This is generally used\r\n" +
                                         "for debugging and can be left unchecked.");
            toolTip1.SetToolTip(chkSplitAliases, "Automatically split character names\r\n" +
                "into aliases. This can have undesired\r\n" +
                "consequences, so use with caution!!!");
            toolTip1.SetToolTip(chkSound, "Play a sound after generating the Author\r\n" +
                                        "Profile, End Action and Start Action\r\n" +
                                        "files, or after generating an X-Ray file.");
            toolTip1.SetToolTip(chkDownloadAliases, "Attempt to download pre-made aliases if none exist\r\n" +
                                                    "locally yet. If \"Overwrite aliases\" is enabled, local\r\n" +
                                                    "aliases will be overwritten with the ones downloaded.");
            toolTip1.SetToolTip(btnSupport, "Visit the MobileRead forum for\r\n" +
                                        "support, bug reports, or questions.");
            toolTip1.SetToolTip(chkOverrideOffset, "This offset will be applied to every\r\nAWZ3 book location (usually -16).\r\nMust be an integer.");
            toolTip1.SetToolTip(chkPageCount, "Try to estimate books page count (based\r\n" +
                                              "on user_none accurate APNX generation).\r\n" +
                                              "If no page count is found online, an\r\n" +
                                              "estimation will be used.");
            toolTip1.SetToolTip(rdoGoodreads, "Search for books and download\r\n" +
                                              "all data from Goodreads.");
            toolTip1.SetToolTip(rdoShelfari, "Search for books and download\r\n" +
                                              "all data from Shelfari.");
            toolTip1.SetToolTip(chkSaveBio, "If checked, author biographies will be saved to the \\ext folder\r\n" +
                                            "and opened in Notepad before continuing to build Author Profiles.\r\n" +
                                            "Must be enabled for a saved biography to be loaded.");
            toolTip1.SetToolTip(cmbRegion, "Default Amazon page to search.\r\n" +
                                            "If no results are found, Amazom.com (USA) will be used.");
            toolTip1.SetToolTip(chkPromptAsin, "Allow ASIN entry if the next or previous book\r\n" +
                                               "in a series cannot automatically be found.\r\n" +
                                               "This is useful if you have the metadata available\r\n" +
                                               "in Calibre, and may help file creation.");
            toolTip1.SetToolTip(chkSearchAsin, "If enabled, search results will be filtered so that non-Kindle Edition books are removed");
            toolTip1.SetToolTip(chkEditBiography, "If enabled, allows editing the Author's biography before it's used.");

            var regions = new List<AmazonRegion>(regionTLDs.Count);
            foreach (var (name, tld) in regionTLDs)
                regions.Add(new AmazonRegion(name, tld));
            cmbRegion.DataSource = regions;
            cmbRegion.DisplayMember = "Name";
            cmbRegion.ValueMember = "TLD";
            cmbRegion.SelectedValue = Properties.Settings.Default.amazonTLD;
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            txtOut.Text = UIFunctions.GetDir(txtOut.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtOffset.Text, out var offset))
            {
                MessageBox.Show("The offset must be an integer.", "Offset Error");
                return;
            }
            if (!int.TryParse(txtAZWOffset.Text, out var offsetAZW))
            {
                MessageBox.Show("The offset must be an integer.", "Offset Error");
                return;
            }
            if (txtReal.Text.Trim().Length == 0 || txtPen.Text.Trim().Length == 0)
            {
                MessageBox.Show("Both Real and Pen names are required for\r\nEnd Action file creation.");
            }
            if (!int.TryParse(txtMinClipLen.Text, out var minClipLen))
            {
                MessageBox.Show("Length must be an integer.", "Length Error");
                return;
            }

            Properties.Settings.Default.outDir = txtOut.Text;
            Properties.Settings.Default.saverawml = chkRaw.Checked;
            Properties.Settings.Default.ignoresofthyphen = chkSoftHyphen.Checked;
            Properties.Settings.Default.useNewVersion = chkUseNew.Checked;
            Properties.Settings.Default.android = chkAndroid.Checked;
            Properties.Settings.Default.skipNoLikes = chkSkipNoLikes.Checked;
            Properties.Settings.Default.minClipLen = minClipLen;
            Properties.Settings.Default.offset = offset;
            Properties.Settings.Default.realName = txtReal.Text;
            Properties.Settings.Default.penName = txtPen.Text;
            Properties.Settings.Default.enableEdit = chkEnableEdit.Checked;
            Properties.Settings.Default.useSubDirectories = chkSubDirectories.Checked;
            Properties.Settings.Default.overwrite = chkOverwrite.Checked;
            Properties.Settings.Default.overwriteAliases = chkAlias.Checked;
            Properties.Settings.Default.overwriteChapters = chkChapters.Checked;
            Properties.Settings.Default.saveHtml = chkSaveHtml.Checked;
            Properties.Settings.Default.splitAliases = chkSplitAliases.Checked;
            Properties.Settings.Default.playSound = chkSound.Checked;
            Properties.Settings.Default.downloadAliases = chkDownloadAliases.Checked;
            Properties.Settings.Default.overrideOffset = chkOverrideOffset.Checked;
            Properties.Settings.Default.offsetAZW3 = offsetAZW;
            Properties.Settings.Default.pageCount = chkPageCount.Checked;
            Properties.Settings.Default.saveBio = chkSaveBio.Checked;
            Properties.Settings.Default.amazonTLD = cmbRegion.SelectedValue.ToString();
            Properties.Settings.Default.dataSource = rdoGoodreads.Checked ? "Goodreads" : "Shelfari";
            Properties.Settings.Default.promptASIN = chkPromptAsin.Checked;
            Properties.Settings.Default.downloadSA = chkDownloadSA.Checked;
            Properties.Settings.Default.searchByAsin = chkSearchAsin.Checked;
            Properties.Settings.Default.editBiography = chkEditBiography.Checked;
            Properties.Settings.Default.outputToSidecar = chkUseSidecar.Checked;
            Properties.Settings.Default.Save();

            Close();
        }

        private void chkAndroid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAndroid.Checked)
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
                Close();
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
                TopMost = false;
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

        private void chkOverwrite_CheckedChanged(object sender, EventArgs e)
        {
            chkAlias.Enabled = chkOverwrite.Checked;
            chkChapters.Enabled = chkOverwrite.Checked;
            if (!chkOverwrite.Checked)
            {
                chkAlias.Checked = false;
                chkChapters.Checked = false;
            }
        }

        private void btnSupport_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.mobileread.com/forums/showthread.php?t=245754");
        }

        private void listSettings_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.DrawString(listSettings.Items[e.Index].ToString(),
                    Font, Brushes.White, e.Bounds.X + 3, e.Bounds.Y + 3);
            }
            else
            {
                e.Graphics.DrawString(listSettings.Items[e.Index].ToString(),
                    Font, Brushes.Black, e.Bounds.X + 3, e.Bounds.Y + 3);
            }
        }

        private void listSettings_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 20;
        }

        private void listSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabSettings.SelectedIndex = listSettings.SelectedIndex;
        }

        private void chkDownloadAliases_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDownloadAliases.Checked)
                chkOverwrite.Checked = false;
        }

        private void chkOverrideOffset_CheckedChanged(object sender, EventArgs e)
        {
            txtAZWOffset.Enabled = chkOverrideOffset.Checked;
        }
    }
}
