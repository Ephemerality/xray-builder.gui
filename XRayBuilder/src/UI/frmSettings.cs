using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Language.Localization;
using XRayBuilderGUI.Localization.Main;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmSettings : Form
    {
        private readonly LanguageFactory _languageFactory;

        // TODO: Should be elsewhere maybe
        private readonly Dictionary<string, string> regionTLDs = new Dictionary<string, string>
        {
            {"Australia", "com.au"}, {"Brazil", "com.br"}, {"Canada", "ca"}, {"China", "cn"},
            {"France", "fr"}, {"Germany", "de"}, {"India", "in"}, {"Italy", "it"}, {"Japan", "co.jp"},
            {"Mexico", "com.mx"}, {"Netherlands", "nl"}, {"Spain", "es"}, {"USA", "com"}, {"UK", "co.uk"}
        };

        private readonly Dictionary<string, string> roentgenRegionTLDs = new Dictionary<string, string>
        {
            {"Germany", "de"}, {"USA", "com"}
        };

        public frmSettings(LanguageFactory languageFactory)
        {
            _languageFactory = languageFactory;
            InitializeComponent();
        }

        //http://stackoverflow.com/questions/2612487/how-to-fix-the-flickering-in-user-controls
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void frmSettingsNew_Load(object sender, EventArgs e)
        {
            listSettings.SelectedIndex = 0;

            if (Directory.Exists(Environment.CurrentDirectory + @"\log"))
            {
                var fileCount = Directory.GetFiles($@"{Environment.CurrentDirectory}\log").Length;
                if (fileCount > 0)
                    btnClearLogs.Text = $@"{MainStrings.ClearLogsTitle} ({fileCount})";
                else
                    btnClearLogs.Enabled = false;
            }

            txtOut.Text = Settings.Default.outDir;
            chkRaw.Checked = Settings.Default.saverawml;
            chkSoftHyphen.Checked = Settings.Default.ignoresofthyphen;
            chkUseNew.Checked = Settings.Default.useNewVersion;
            chkAndroid.Checked = Settings.Default.android;
            txtReal.Text = Settings.Default.realName;
            txtPen.Text = Settings.Default.penName;
            chkEnableEdit.Checked = Settings.Default.enableEdit;
            chkSubDirectories.Checked = Settings.Default.useSubDirectories;
            chkSkipNoLikes.Checked = Settings.Default.skipNoLikes;
            txtMinClipLen.Text = Settings.Default.minClipLen.ToString();
            chkAlias.Checked = Settings.Default.overwriteAliases;
            chkChapters.Checked = Settings.Default.overwriteChapters;
            chkSplitAliases.Checked = Settings.Default.splitAliases;
            chkSound.Checked = Settings.Default.playSound;
            chkDownloadAliases.Checked = Settings.Default.downloadAliases;
            chkPageCount.Checked = Settings.Default.pageCount;
            chkSaveBio.Checked = Settings.Default.saveBio;
            chkOverwriteAP.Checked = Settings.Default.overwriteAP;
            chkOverwriteEA.Checked = Settings.Default.overwriteEA;
            chkOverwriteSA.Checked = Settings.Default.overwriteSA;
            chkAutoBuildAP.Checked = Settings.Default.autoBuildAP;
            chkPromptAsin.Checked = Settings.Default.promptASIN;
            chkEditBiography.Checked = Settings.Default.editBiography;
            chkEditDescription.Checked = Settings.Default.editDescription;
            chkUseSidecar.Checked = Settings.Default.outputToSidecar;
            cmbSecondaryDataSource.Text = Settings.Default.dataSource;

            chkRoentgenStartActions.Checked = Settings.Default.downloadSA;
            chkRoentgenEndActions.Checked = Settings.Default.downloadEA;
            chkRoentgenAuthorProfile.Checked = Settings.Default.downloadAP;

            chkIncludeTopics.Checked = Settings.Default.includeTopics;

            var toolTip1 = new ToolTip();
            toolTip1.SetToolTip(chkRaw,
                "Save the rawML (raw markup) of the book\r\n" +
                "in the output directory so you can review it.");
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
                "Changes the naming convention of the X-Ray file\r\n" +
                "for the Android Kindle app. Forces building with\r\n" +
                "the new format. Files will be placed in the output\r\n" +
                "directory within the 'Android' folder.");
            toolTip1.SetToolTip(chkSkipNoLikes, "Skip notable clips with no likes.\r\n" +
                                                "Enable to for filter out garbage\r\n" +
                                                "quotes from Goodreads.");
            toolTip1.SetToolTip(txtMinClipLen, "Minimum length for notable clips.\r\n" +
                                               "Enable to for filter out garbage\r\n" +
                                               "quotes from Goodreads.");
            toolTip1.SetToolTip(txtReal, "Required during the EndActions.data file\r\n" +
                                         "creation. This information allows you to\r\n" +
                                         "rate this book on Amazon.");
            toolTip1.SetToolTip(txtPen, "Required during the EndActions.data file\r\n" +
                                        "creation. This information allows you to\r\n" +
                                        "rate this book on Amazon.");
            toolTip1.SetToolTip(chkEnableEdit,
                "Open Notepad to enable editing of\r\n" +
                "detected Chapters and Aliases\r\n" +
                "before final X-Ray creation.");
            toolTip1.SetToolTip(chkSubDirectories, "Save generated files to an\r\n\"Author\\Filename\" sub directory.");
            toolTip1.SetToolTip(chkUseSidecar,
                "Save generated files to a\r\nsidecar subdirectory based\r\non the filename.");
            toolTip1.SetToolTip(btnLogs, "Open the log files directory.");
            toolTip1.SetToolTip(chkOverwriteAP, "Overwrite existing Author Profile files.");
            toolTip1.SetToolTip(chkOverwriteEA, "Overwrite existing End Actions files.");
            toolTip1.SetToolTip(chkOverwriteSA, "Overwrite existing Start Actions files.");
            toolTip1.SetToolTip(chkAlias, "Overwrite existing alias files.");
            toolTip1.SetToolTip(chkChapters, "Overwrite existing chapter files.");
            toolTip1.SetToolTip(chkSplitAliases, "Automatically split character names\r\n" +
                                                 "into aliases. This can have undesired\r\n" +
                                                 "consequences, so use with caution!");
            toolTip1.SetToolTip(chkSound, "Play a sound after generating the Author\r\n" +
                                          "Profile, End Action and Start Action\r\n" +
                                          "files, or after generating an X-Ray file.");
            toolTip1.SetToolTip(chkDownloadAliases, "Attempt to download pre-made aliases if none exist\r\n" +
                                                    "locally yet. If \"Overwrite aliases\" is enabled, local\r\n" +
                                                    "aliases will be overwritten with the ones downloaded.");
            toolTip1.SetToolTip(btnSupport, "Visit the MobileRead forum for\r\n" +
                                            "support, bug reports, or questions.");
            toolTip1.SetToolTip(chkPageCount, "Try to estimate books page count (based\r\n" +
                                              "on user_none's accurate APNX generation).\r\n" +
                                              "If no page count is found online, an\r\n" +
                                              "estimation will be used.");
            toolTip1.SetToolTip(cmbSecondaryDataSource,
                "Determines from which non-Amazon source\r\n" +
                "terms and metadata will be downloaded.");
            toolTip1.SetToolTip(chkSaveBio, "If enabled, author biographies will be saved to\r\n" +
                                            "the \\ext folder and opened in Notepad before\r\n" +
                                            "continuing to build Author Profiles. Must be\r\n" +
                                            "enabled for a saved biography to be loaded.");
            toolTip1.SetToolTip(cmbRegion, "Default Amazon page to search.\r\n" +
                                           "If no results are found, Amazon.com\r\n" +
                                           "(USA) will be used.");
            toolTip1.SetToolTip(chkPromptAsin, "Allow ASIN entry if the next or previous book\r\n" +
                                               "in a series cannot automatically be found.\r\n" +
                                               "This is useful if you have the metadata available\r\n" +
                                               "in Calibre, and may help file creation.");
            toolTip1.SetToolTip(chkEditBiography,
                "If enabled, allows editing the Author's\r\n" +
                "biography before it's used.");
            toolTip1.SetToolTip(chkEditDescription, "If enabled, allows editing the book's\r\n" +
                                                    "description before it's used.");
            toolTip1.SetToolTip(chkAutoBuildAP,
                "When enabled, the Author Profile will be\r\n" +
                "built using a downloaded End Actions file,\r\n" +
                "if one is available, instead of scraping Amazon.");
            toolTip1.SetToolTip(chkIncludeTopics,
                "When downloading terms, include any terms that\r\n" +
                "are non-characters (topics, locations, etc)");

            var regions = new List<AmazonRegion>(regionTLDs.Count);
            foreach (var (name, tld) in regionTLDs)
                regions.Add(new AmazonRegion(name, tld));
            cmbRegion.DataSource = regions;
            cmbRegion.DisplayMember = "Name";
            cmbRegion.ValueMember = "TLD";
            cmbRegion.SelectedValue = Settings.Default.amazonTLD;

            var roentgenRegions = roentgenRegionTLDs
                .Select(kvp => new AmazonRegion(kvp.Key, kvp.Value))
                .ToArray();
            cmbRoentgenRegion.DataSource = roentgenRegions;
            cmbRoentgenRegion.DisplayMember = "Name";
            cmbRoentgenRegion.ValueMember = "TLD";
            cmbRoentgenRegion.SelectedValue = Settings.Default.roentgenRegion;

            var languages = _languageFactory.GetValues().ToArray();
            cmbLanguage.DataSource = languages;
            cmbLanguage.DisplayMember = "Label";
            cmbLanguage.ValueMember = "Language";
            cmbLanguage.SelectedValue = _languageFactory.Get(Settings.Default.Language)!.Language;
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            txtOut.Text = UIFunctions.GetDir(txtOut.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtReal.Text.Trim().Length == 0 || txtPen.Text.Trim().Length == 0)
            {
                MessageBox.Show(@"Both Real and Pen names are required for" + Environment.NewLine +
                                @"End Action file creation.", @"Missing Name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                listSettings.SelectedIndex = 4;
                return;
            }

            if (!int.TryParse(txtMinClipLen.Text, out var minClipLen))
            {
                MessageBox.Show(@"Notable clip minimum length must be an integer.", @"Length Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                listSettings.SelectedIndex = 2;
                return;
            }

            var language = _languageFactory.Get(cmbLanguage.SelectedValue.ToString());
            if (language != null && language.Language.ToString() != Settings.Default.Language)
            {
                Settings.Default.Language = language.Language.ToString();
                MessageBox.Show(MainStrings.RestartRequired, MainStrings.RestartRequiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Settings.Default.outDir = txtOut.Text;
            Settings.Default.saverawml = chkRaw.Checked;
            Settings.Default.ignoresofthyphen = chkSoftHyphen.Checked;
            Settings.Default.useNewVersion = chkUseNew.Checked;
            Settings.Default.android = chkAndroid.Checked;
            Settings.Default.skipNoLikes = chkSkipNoLikes.Checked;
            Settings.Default.minClipLen = minClipLen;
            Settings.Default.realName = txtReal.Text;
            Settings.Default.penName = txtPen.Text;
            Settings.Default.enableEdit = chkEnableEdit.Checked;
            Settings.Default.useSubDirectories = chkSubDirectories.Checked;
            Settings.Default.overwriteAP = chkOverwriteAP.Checked;
            Settings.Default.overwriteEA = chkOverwriteEA.Checked;
            Settings.Default.overwriteSA = chkOverwriteSA.Checked;
            Settings.Default.overwriteAliases = chkAlias.Checked;
            Settings.Default.overwriteChapters = chkChapters.Checked;
            Settings.Default.autoBuildAP = chkAutoBuildAP.Checked;
            Settings.Default.splitAliases = chkSplitAliases.Checked;
            Settings.Default.playSound = chkSound.Checked;
            Settings.Default.downloadAliases = chkDownloadAliases.Checked;
            Settings.Default.pageCount = chkPageCount.Checked;
            Settings.Default.saveBio = chkSaveBio.Checked;
            Settings.Default.amazonTLD = cmbRegion.SelectedValue.ToString();
            Settings.Default.dataSource = cmbSecondaryDataSource.Text;
            Settings.Default.promptASIN = chkPromptAsin.Checked;
            Settings.Default.editBiography = chkEditBiography.Checked;
            Settings.Default.editDescription = chkEditDescription.Checked;
            Settings.Default.outputToSidecar = chkUseSidecar.Checked;
            Settings.Default.downloadSA = chkRoentgenStartActions.Checked;
            Settings.Default.downloadEA = chkRoentgenEndActions.Checked;
            Settings.Default.downloadAP = chkRoentgenAuthorProfile.Checked;
            Settings.Default.roentgenRegion = cmbRoentgenRegion.SelectedValue.ToString();
            Settings.Default.includeTopics = chkIncludeTopics.Checked;
            Settings.Default.Save();

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
            if (keyData != Keys.Escape) return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\log"))
            {
                MessageBox.Show(MainStrings.LogDirectoryDoesNotExist, MainStrings.LogDirectoryNotFoundTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Process.Start($@"{Environment.CurrentDirectory}\log");
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show(
                $@"{MainStrings.DeleteLogFilesConfirmation}{Environment.NewLine}{MainStrings.ActionCannotBeUndone}",
                MainStrings.AreYouSure, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2))
                try
                {
                    Cursor = Cursors.WaitCursor;
                    Array.ForEach(Directory.GetFiles($@"{Environment.CurrentDirectory}\log"), File.Delete);
                    btnClearLogs.Text = MainStrings.ClearLogsTitle;
                    btnClearLogs.Enabled = false;
                    Cursor = Cursors.Default;
                }
                catch (Exception)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(MainStrings.ErrorDeletingLogFiles, MainStrings.UnableToDeleteLogFilesCaption,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void chkOverwrite_CheckedChanged(object sender, EventArgs e)
        {
            chkAlias.Enabled = chkOverwriteAP.Checked;
            chkChapters.Enabled = chkOverwriteAP.Checked;
            if (chkOverwriteAP.Checked) return;
            chkAlias.Checked = false;
            chkChapters.Checked = false;
        }

        private void btnSupport_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.mobileread.com/forums/showthread.php?t=245754");
        }

        private void listSettings_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            var brush = (e.State & DrawItemState.Selected) != 0 ? Brushes.White : Brushes.Black;
            e.Graphics.DrawString(listSettings.Items[e.Index].ToString(), Font, brush, e.Bounds.X + 5, e.Bounds.Y + 5);
        }

        private void listSettings_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 30;
        }

        private void listSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabSettings.SelectedIndex = listSettings.SelectedIndex;
        }

        private void chkDownloadAliases_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDownloadAliases.Checked)
                chkOverwriteAP.Checked = false;
        }

        private sealed class AmazonRegion
        {
            public AmazonRegion(string name, string tld)
            {
                Name = name;
                TLD = tld;
            }

            public string Name { get; }
            public string TLD { get; }
        }
    }
}