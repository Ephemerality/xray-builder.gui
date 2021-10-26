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
        private readonly Dictionary<string, string> _regionTlDs = new()
        {
            { "Australia", "com.au" }, { "Brazil", "com.br" }, { "Canada", "ca" }, { "China", "cn" },
            { "France", "fr" }, { "Germany", "de" }, { "India", "in" }, { "Italy", "it" }, { "Japan", "co.jp" },
            { "Mexico", "com.mx" }, { "Netherlands", "nl" }, { "Spain", "es" }, { "USA", "com" }, { "UK", "co.uk" }
        };

        private readonly Dictionary<string, string> roentgenRegionTLDs = new()
        {
            { "Germany", "de" }, { "USA", "com" }
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

            // todo log directory responsibility should be elsewhere
            if (Directory.Exists($@"{Environment.CurrentDirectory}\log"))
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
            chkRoentgenNextInSeries.Checked = Settings.Default.downloadNIS;

            chkIncludeTopics.Checked = Settings.Default.includeTopics;

            var toolTip1 = new ToolTip();
            toolTip1.SetToolTip(chkRaw, MainStrings.SaveRawMlTooltip);
            toolTip1.SetToolTip(chkSoftHyphen, MainStrings.IgnoreSoftHyphensTooltip);
            toolTip1.SetToolTip(chkUseNew, MainStrings.WriteXRayNewFormatTooltip);
            toolTip1.SetToolTip(chkAndroid, $@"{MainStrings.BuildXRayForAndroidTooltip}{Environment.NewLine}{MainStrings.RequiresNewFormat}{Environment.NewLine}{MainStrings.FilesInAndroidFolder}");
            toolTip1.SetToolTip(chkSkipNoLikes, $"{MainStrings.SkipNoLikes}{Environment.NewLine}({MainStrings.GoodForFilteringQuotes})");
            toolTip1.SetToolTip(txtMinClipLen, $"{MainStrings.MinimumQuoteLength}{Environment.NewLine}({MainStrings.GoodForFilteringQuotes})");
            toolTip1.SetToolTip(txtReal, $"{MainStrings.RequiredDuringEndActionsCreation}{Environment.NewLine}{MainStrings.InformationAllowsRatingOnAmazon}");
            toolTip1.SetToolTip(txtPen, $"{MainStrings.RequiredDuringEndActionsCreation}{Environment.NewLine}{MainStrings.InformationAllowsRatingOnAmazon}");
            toolTip1.SetToolTip(chkEnableEdit, MainStrings.EnableEditTooltip);
            toolTip1.SetToolTip(chkSubDirectories, MainStrings.SaveToAuthorSubdirTooltip);
            toolTip1.SetToolTip(chkUseSidecar, MainStrings.SaveToSidecarSubdirTooltip);
            toolTip1.SetToolTip(chkOverwriteAP, MainStrings.OverwriteAuthorProfileTooltip);
            toolTip1.SetToolTip(chkOverwriteEA, MainStrings.OverwriteEndActionsTooltip);
            toolTip1.SetToolTip(chkOverwriteSA, MainStrings.OverwriteStartActionsTooltip);
            toolTip1.SetToolTip(chkAlias, MainStrings.OverwriteAliasesTooltip);
            toolTip1.SetToolTip(chkChapters, MainStrings.OverwriteChaptersTooltip);
            toolTip1.SetToolTip(chkSplitAliases, MainStrings.AutomaticallySplitAliasesTooltip);
            toolTip1.SetToolTip(chkSound, MainStrings.PlaySoundTooltip);
            toolTip1.SetToolTip(chkDownloadAliases, MainStrings.DownloadAliasesTooltip);
            toolTip1.SetToolTip(chkPageCount, MainStrings.EstimatePageCountTooltip);
            toolTip1.SetToolTip(cmbSecondaryDataSource, MainStrings.SecondarySourceTooltip);
            toolTip1.SetToolTip(chkSaveBio, MainStrings.SaveBioTooltip);
            toolTip1.SetToolTip(cmbRegion, MainStrings.AmazonRegionTooltip);
            toolTip1.SetToolTip(chkPromptAsin, MainStrings.PromptAsinTooltip);
            toolTip1.SetToolTip(chkEditBiography, MainStrings.EditBioTooltip);
            toolTip1.SetToolTip(chkAutoBuildAP, MainStrings.AutoBuildAuthorProfileTooltip);
            toolTip1.SetToolTip(chkIncludeTopics, MainStrings.IncludeTopicsTooltip);
            toolTip1.SetToolTip(chkEditDescription, $"If enabled, allows editing the book's{Environment.NewLine}description before it's used.");

            var regions = new List<AmazonRegion>(_regionTlDs.Count);
            foreach (var (name, tld) in _regionTlDs)
                regions.Add(new AmazonRegion(name, tld));
            cmbRegion.DataSource = regions;
            cmbRegion.DisplayMember = "Name";
            cmbRegion.ValueMember = "TLD";
            cmbRegion.SelectedValue = Settings.Default.amazonTLD;

            var roentgenRegions = roentgenRegionTLDs
                .Select(kvp => new AmazonRegion(kvp.Key, kvp.Value))
                .ToArray();
            cmbRoentgenRegion.DataSource = roentgenRegions;
            cmbRoentgenRegion.DisplayMember = nameof(AmazonRegion.Name);
            cmbRoentgenRegion.ValueMember = nameof(AmazonRegion.Tld);
            cmbRoentgenRegion.SelectedValue = Settings.Default.roentgenRegion;

            var languages = _languageFactory.GetValues().ToArray();
            cmbLanguage.DataSource = languages;
            cmbLanguage.DisplayMember = nameof(ILanguage.Label);
            cmbLanguage.ValueMember = nameof(ILanguage.Language);
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
                MessageBox.Show($"Both Real and Pen names are required for{Environment.NewLine}End Action file creation.", "Missing Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                listSettings.SelectedIndex = 4;
                return;
            }

            if (!int.TryParse(txtMinClipLen.Text, out var minClipLen))
            {
                MessageBox.Show("Notable clip minimum length must be an integer.", "Length Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                listSettings.SelectedIndex = 2;
                return;
            }

            var language = _languageFactory.Get(cmbLanguage.SelectedValue.ToString());
            if (language != null && language.Language.ToString() != Settings.Default.Language)
            {
                Settings.Default.Language = language.Language.ToString();
                MessageBox.Show(MainStrings.RestartRequired, MainStrings.RestartRequiredTitle);
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
            Settings.Default.downloadNIS = chkRoentgenNextInSeries.Checked;
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

        /// <summary>
        /// Handle escape key since there's no cancel button
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape)
                return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes != MessageBox.Show($@"{MainStrings.DeleteLogFilesConfirmation}{Environment.NewLine}{MainStrings.ActionCannotBeUndone}", MainStrings.AreYouSure, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                Array.ForEach(Directory.GetFiles($@"{Environment.CurrentDirectory}\log"), File.Delete);
                btnClearLogs.Text = MainStrings.ClearLogsTitle;
                btnClearLogs.Enabled = false;
            }
            catch (Exception)
            {
                MessageBox.Show(MainStrings.ErrorDeletingLogFiles, MainStrings.UnableToDeleteLogFilesCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void chkOverwrite_CheckedChanged(object sender, EventArgs e)
        {
            chkAlias.Enabled = chkOverwriteAP.Checked;
            chkChapters.Enabled = chkOverwriteAP.Checked;
            if (!chkOverwriteAP.Checked)
            {
                chkAlias.Checked = false;
                chkChapters.Checked = false;
            }
        }

        private void listSettings_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            var brush = (e.State & DrawItemState.Selected) != 0 ? Brushes.White : Brushes.Black;
            e.Graphics.DrawString(listSettings.Items[e.Index].ToString(), Font, brush, e.Bounds.X + 5, e.Bounds.Y + 3);
        }

        private void listSettings_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 23;
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

        private sealed record AmazonRegion(string Name, string Tld);
    }
}