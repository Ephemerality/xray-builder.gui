namespace XRayBuilderGUI.UI
{
    partial class frmSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
            this.btnSupport = new System.Windows.Forms.Button();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.btnLogs = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblSeperator = new System.Windows.Forms.Label();
            this.tabSettings = new System.Windows.Forms.TabControl();
            this.tabPgGeneral = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkSearchAsin = new System.Windows.Forms.CheckBox();
            this.gbProcess = new System.Windows.Forms.GroupBox();
            this.chkPageCount = new System.Windows.Forms.CheckBox();
            this.gbUnpack = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblNote = new System.Windows.Forms.Label();
            this.chkSaveHtml = new System.Windows.Forms.CheckBox();
            this.chkRaw = new System.Windows.Forms.CheckBox();
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.chkSound = new System.Windows.Forms.CheckBox();
            this.tabPgDirectories = new System.Windows.Forms.TabPage();
            this.gbDirectories = new System.Windows.Forms.GroupBox();
            this.chkUseSidecar = new System.Windows.Forms.CheckBox();
            this.chkSubDirectories = new System.Windows.Forms.CheckBox();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.tabPgXray = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rdoShelfari = new System.Windows.Forms.RadioButton();
            this.rdoGoodreads = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chkSoftHyphen = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMinClipLen = new System.Windows.Forms.TextBox();
            this.chkSkipNoLikes = new System.Windows.Forms.CheckBox();
            this.gbXray = new System.Windows.Forms.GroupBox();
            this.chkAndroid = new System.Windows.Forms.CheckBox();
            this.chkUseNew = new System.Windows.Forms.CheckBox();
            this.tabPgAliasChapter = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.chkChapters = new System.Windows.Forms.CheckBox();
            this.chkAlias = new System.Windows.Forms.CheckBox();
            this.chkOverwrite = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkDownloadAliases = new System.Windows.Forms.CheckBox();
            this.chkSplitAliases = new System.Windows.Forms.CheckBox();
            this.chkEnableEdit = new System.Windows.Forms.CheckBox();
            this.tabPgAmazon = new System.Windows.Forms.TabPage();
            this.gbAmazonPrefs = new System.Windows.Forms.GroupBox();
            this.chkEditBiography = new System.Windows.Forms.CheckBox();
            this.chkDownloadSA = new System.Windows.Forms.CheckBox();
            this.chkPromptAsin = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbRegion = new System.Windows.Forms.ComboBox();
            this.chkSaveBio = new System.Windows.Forms.CheckBox();
            this.gbDetails = new System.Windows.Forms.GroupBox();
            this.lblReal = new System.Windows.Forms.Label();
            this.txtReal = new System.Windows.Forms.TextBox();
            this.txtPen = new System.Windows.Forms.TextBox();
            this.lblPen = new System.Windows.Forms.Label();
            this.listSettings = new System.Windows.Forms.ListBox();
            this.tabSettings.SuspendLayout();
            this.tabPgGeneral.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gbProcess.SuspendLayout();
            this.gbUnpack.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.gbGeneral.SuspendLayout();
            this.tabPgDirectories.SuspendLayout();
            this.gbDirectories.SuspendLayout();
            this.tabPgXray.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbXray.SuspendLayout();
            this.tabPgAliasChapter.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabPgAmazon.SuspendLayout();
            this.gbAmazonPrefs.SuspendLayout();
            this.gbDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSupport
            // 
            this.btnSupport.Location = new System.Drawing.Point(328, 290);
            this.btnSupport.Name = "btnSupport";
            this.btnSupport.Size = new System.Drawing.Size(99, 30);
            this.btnSupport.TabIndex = 41;
            this.btnSupport.Text = "Forum";
            this.btnSupport.UseVisualStyleBackColor = true;
            this.btnSupport.Click += new System.EventHandler(this.btnSupport_Click);
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Location = new System.Drawing.Point(117, 290);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(99, 30);
            this.btnClearLogs.TabIndex = 39;
            this.btnClearLogs.Text = "Clear Logs";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // btnLogs
            // 
            this.btnLogs.Location = new System.Drawing.Point(12, 290);
            this.btnLogs.Name = "btnLogs";
            this.btnLogs.Size = new System.Drawing.Size(99, 30);
            this.btnLogs.TabIndex = 38;
            this.btnLogs.Text = "Logs";
            this.btnLogs.UseVisualStyleBackColor = true;
            this.btnLogs.Click += new System.EventHandler(this.btnLogs_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(433, 290);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(99, 30);
            this.btnSave.TabIndex = 37;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblSeperator
            // 
            this.lblSeperator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeperator.Location = new System.Drawing.Point(0, 277);
            this.lblSeperator.Name = "lblSeperator";
            this.lblSeperator.Size = new System.Drawing.Size(546, 2);
            this.lblSeperator.TabIndex = 42;
            this.lblSeperator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabSettings
            // 
            this.tabSettings.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabSettings.Controls.Add(this.tabPgGeneral);
            this.tabSettings.Controls.Add(this.tabPgDirectories);
            this.tabSettings.Controls.Add(this.tabPgXray);
            this.tabSettings.Controls.Add(this.tabPgAliasChapter);
            this.tabSettings.Controls.Add(this.tabPgAmazon);
            this.tabSettings.ItemSize = new System.Drawing.Size(0, 1);
            this.tabSettings.Location = new System.Drawing.Point(144, 1);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(398, 268);
            this.tabSettings.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabSettings.TabIndex = 43;
            // 
            // tabPgGeneral
            // 
            this.tabPgGeneral.Controls.Add(this.groupBox1);
            this.tabPgGeneral.Controls.Add(this.gbProcess);
            this.tabPgGeneral.Controls.Add(this.gbUnpack);
            this.tabPgGeneral.Controls.Add(this.gbGeneral);
            this.tabPgGeneral.Location = new System.Drawing.Point(4, 5);
            this.tabPgGeneral.Name = "tabPgGeneral";
            this.tabPgGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgGeneral.Size = new System.Drawing.Size(390, 259);
            this.tabPgGeneral.TabIndex = 0;
            this.tabPgGeneral.Text = "tabPage1";
            this.tabPgGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkSearchAsin);
            this.groupBox1.Location = new System.Drawing.Point(6, 208);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 48);
            this.groupBox1.TabIndex = 39;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Provider";
            // 
            // chkSearchAsin
            // 
            this.chkSearchAsin.AutoSize = true;
            this.chkSearchAsin.Location = new System.Drawing.Point(14, 19);
            this.chkSearchAsin.Name = "chkSearchAsin";
            this.chkSearchAsin.Size = new System.Drawing.Size(222, 17);
            this.chkSearchAsin.TabIndex = 34;
            this.chkSearchAsin.Text = "Allow search by ASIN before author + title";
            this.chkSearchAsin.UseVisualStyleBackColor = true;
            // 
            // gbProcess
            // 
            this.gbProcess.Controls.Add(this.chkPageCount);
            this.gbProcess.Location = new System.Drawing.Point(6, 159);
            this.gbProcess.Name = "gbProcess";
            this.gbProcess.Size = new System.Drawing.Size(378, 48);
            this.gbProcess.TabIndex = 38;
            this.gbProcess.TabStop = false;
            this.gbProcess.Text = "eBook Processing";
            // 
            // chkPageCount
            // 
            this.chkPageCount.AutoSize = true;
            this.chkPageCount.Location = new System.Drawing.Point(14, 19);
            this.chkPageCount.Name = "chkPageCount";
            this.chkPageCount.Size = new System.Drawing.Size(125, 17);
            this.chkPageCount.TabIndex = 34;
            this.chkPageCount.Text = "Estimate Page Count";
            this.chkPageCount.UseVisualStyleBackColor = true;
            // 
            // gbUnpack
            // 
            this.gbUnpack.Controls.Add(this.pictureBox1);
            this.gbUnpack.Controls.Add(this.lblNote);
            this.gbUnpack.Controls.Add(this.chkSaveHtml);
            this.gbUnpack.Controls.Add(this.chkRaw);
            this.gbUnpack.Location = new System.Drawing.Point(6, 66);
            this.gbUnpack.Name = "gbUnpack";
            this.gbUnpack.Size = new System.Drawing.Size(378, 89);
            this.gbUnpack.TabIndex = 37;
            this.gbUnpack.TabStop = false;
            this.gbUnpack.Text = "eBook Unpacking";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(14, 45);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(14, 14);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 35;
            this.pictureBox1.TabStop = false;
            // 
            // lblNote
            // 
            this.lblNote.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblNote.Location = new System.Drawing.Point(31, 43);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(325, 43);
            this.lblNote.TabIndex = 34;
            this.lblNote.Text = "Metadata is now gathered internally rather than with KindleUnpack. If you run int" +
    "o any metadata extraction errors, please report them on the MobileRead thread.";
            // 
            // chkSaveHtml
            // 
            this.chkSaveHtml.AutoSize = true;
            this.chkSaveHtml.Location = new System.Drawing.Point(124, 19);
            this.chkSaveHtml.Name = "chkSaveHtml";
            this.chkSaveHtml.Size = new System.Drawing.Size(84, 17);
            this.chkSaveHtml.TabIndex = 33;
            this.chkSaveHtml.Text = "Save HTML";
            this.chkSaveHtml.UseVisualStyleBackColor = true;
            // 
            // chkRaw
            // 
            this.chkRaw.AutoSize = true;
            this.chkRaw.Location = new System.Drawing.Point(14, 19);
            this.chkRaw.Name = "chkRaw";
            this.chkRaw.Size = new System.Drawing.Size(81, 17);
            this.chkRaw.TabIndex = 32;
            this.chkRaw.Text = "Save rawml";
            this.chkRaw.UseVisualStyleBackColor = true;
            // 
            // gbGeneral
            // 
            this.gbGeneral.Controls.Add(this.chkSound);
            this.gbGeneral.Location = new System.Drawing.Point(6, 12);
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.Size = new System.Drawing.Size(378, 48);
            this.gbGeneral.TabIndex = 36;
            this.gbGeneral.TabStop = false;
            this.gbGeneral.Text = "Sounds";
            // 
            // chkSound
            // 
            this.chkSound.AutoSize = true;
            this.chkSound.Location = new System.Drawing.Point(14, 19);
            this.chkSound.Name = "chkSound";
            this.chkSound.Size = new System.Drawing.Size(216, 17);
            this.chkSound.TabIndex = 30;
            this.chkSound.Text = "Play a sound when a process completes";
            this.chkSound.UseVisualStyleBackColor = true;
            // 
            // tabPgDirectories
            // 
            this.tabPgDirectories.Controls.Add(this.gbDirectories);
            this.tabPgDirectories.Location = new System.Drawing.Point(4, 5);
            this.tabPgDirectories.Name = "tabPgDirectories";
            this.tabPgDirectories.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgDirectories.Size = new System.Drawing.Size(390, 259);
            this.tabPgDirectories.TabIndex = 1;
            this.tabPgDirectories.Text = "tabPage2";
            this.tabPgDirectories.UseVisualStyleBackColor = true;
            // 
            // gbDirectories
            // 
            this.gbDirectories.Controls.Add(this.chkUseSidecar);
            this.gbDirectories.Controls.Add(this.chkSubDirectories);
            this.gbDirectories.Controls.Add(this.txtOut);
            this.gbDirectories.Controls.Add(this.btnBrowseOut);
            this.gbDirectories.Location = new System.Drawing.Point(6, 12);
            this.gbDirectories.Name = "gbDirectories";
            this.gbDirectories.Size = new System.Drawing.Size(378, 81);
            this.gbDirectories.TabIndex = 39;
            this.gbDirectories.TabStop = false;
            this.gbDirectories.Text = "Output Directory";
            // 
            // chkUseSidecar
            // 
            this.chkUseSidecar.AutoSize = true;
            this.chkUseSidecar.Location = new System.Drawing.Point(133, 52);
            this.chkUseSidecar.Name = "chkUseSidecar";
            this.chkUseSidecar.Size = new System.Drawing.Size(125, 17);
            this.chkUseSidecar.TabIndex = 27;
            this.chkUseSidecar.Text = "Use sidecar directory";
            this.chkUseSidecar.UseVisualStyleBackColor = true;
            // 
            // chkSubDirectories
            // 
            this.chkSubDirectories.AutoSize = true;
            this.chkSubDirectories.Location = new System.Drawing.Point(14, 52);
            this.chkSubDirectories.Name = "chkSubDirectories";
            this.chkSubDirectories.Size = new System.Drawing.Size(113, 17);
            this.chkSubDirectories.TabIndex = 26;
            this.chkSubDirectories.Text = "Use subdirectories";
            this.chkSubDirectories.UseVisualStyleBackColor = true;
            // 
            // txtOut
            // 
            this.txtOut.Location = new System.Drawing.Point(14, 20);
            this.txtOut.Name = "txtOut";
            this.txtOut.Size = new System.Drawing.Size(308, 20);
            this.txtOut.TabIndex = 12;
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Location = new System.Drawing.Point(331, 19);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(34, 22);
            this.btnBrowseOut.TabIndex = 13;
            this.btnBrowseOut.Text = "...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // tabPgXray
            // 
            this.tabPgXray.Controls.Add(this.groupBox2);
            this.tabPgXray.Controls.Add(this.groupBox4);
            this.tabPgXray.Controls.Add(this.groupBox3);
            this.tabPgXray.Controls.Add(this.gbXray);
            this.tabPgXray.Location = new System.Drawing.Point(4, 5);
            this.tabPgXray.Name = "tabPgXray";
            this.tabPgXray.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgXray.Size = new System.Drawing.Size(390, 259);
            this.tabPgXray.TabIndex = 2;
            this.tabPgXray.Text = "tabPage3";
            this.tabPgXray.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rdoShelfari);
            this.groupBox2.Controls.Add(this.rdoGoodreads);
            this.groupBox2.Location = new System.Drawing.Point(202, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(182, 74);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data Provider";
            // 
            // rdoShelfari
            // 
            this.rdoShelfari.AutoSize = true;
            this.rdoShelfari.Location = new System.Drawing.Point(17, 45);
            this.rdoShelfari.Name = "rdoShelfari";
            this.rdoShelfari.Size = new System.Drawing.Size(139, 17);
            this.rdoShelfari.TabIndex = 1;
            this.rdoShelfari.TabStop = true;
            this.rdoShelfari.Text = "Shelfari (via archive.org)";
            this.rdoShelfari.UseVisualStyleBackColor = true;
            // 
            // rdoGoodreads
            // 
            this.rdoGoodreads.AutoSize = true;
            this.rdoGoodreads.Checked = true;
            this.rdoGoodreads.Location = new System.Drawing.Point(17, 18);
            this.rdoGoodreads.Name = "rdoGoodreads";
            this.rdoGoodreads.Size = new System.Drawing.Size(77, 17);
            this.rdoGoodreads.TabIndex = 0;
            this.rdoGoodreads.TabStop = true;
            this.rdoGoodreads.Text = "Goodreads";
            this.rdoGoodreads.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chkSoftHyphen);
            this.groupBox4.Location = new System.Drawing.Point(6, 146);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(378, 107);
            this.groupBox4.TabIndex = 28;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Preferences";
            // 
            // chkSoftHyphen
            // 
            this.chkSoftHyphen.AutoSize = true;
            this.chkSoftHyphen.Location = new System.Drawing.Point(14, 19);
            this.chkSoftHyphen.Name = "chkSoftHyphen";
            this.chkSoftHyphen.Size = new System.Drawing.Size(123, 17);
            this.chkSoftHyphen.TabIndex = 21;
            this.chkSoftHyphen.Text = "Ignore Soft Hyphens";
            this.chkSoftHyphen.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.txtMinClipLen);
            this.groupBox3.Controls.Add(this.chkSkipNoLikes);
            this.groupBox3.Location = new System.Drawing.Point(6, 92);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(378, 48);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Notable Clips";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(168, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Minimum length:";
            // 
            // txtMinClipLen
            // 
            this.txtMinClipLen.Location = new System.Drawing.Point(257, 17);
            this.txtMinClipLen.Name = "txtMinClipLen";
            this.txtMinClipLen.Size = new System.Drawing.Size(47, 20);
            this.txtMinClipLen.TabIndex = 27;
            // 
            // chkSkipNoLikes
            // 
            this.chkSkipNoLikes.AutoSize = true;
            this.chkSkipNoLikes.Location = new System.Drawing.Point(14, 19);
            this.chkSkipNoLikes.Name = "chkSkipNoLikes";
            this.chkSkipNoLikes.Size = new System.Drawing.Size(129, 17);
            this.chkSkipNoLikes.TabIndex = 25;
            this.chkSkipNoLikes.Text = "Ignore clips w/ 0 likes";
            this.chkSkipNoLikes.UseVisualStyleBackColor = true;
            // 
            // gbXray
            // 
            this.gbXray.Controls.Add(this.chkAndroid);
            this.gbXray.Controls.Add(this.chkUseNew);
            this.gbXray.Location = new System.Drawing.Point(6, 12);
            this.gbXray.Name = "gbXray";
            this.gbXray.Size = new System.Drawing.Size(183, 74);
            this.gbXray.TabIndex = 26;
            this.gbXray.TabStop = false;
            this.gbXray.Text = "Format";
            // 
            // chkAndroid
            // 
            this.chkAndroid.AutoSize = true;
            this.chkAndroid.Location = new System.Drawing.Point(14, 45);
            this.chkAndroid.Name = "chkAndroid";
            this.chkAndroid.Size = new System.Drawing.Size(103, 17);
            this.chkAndroid.TabIndex = 23;
            this.chkAndroid.Text = "Build for Android";
            this.chkAndroid.UseVisualStyleBackColor = true;
            this.chkAndroid.CheckedChanged += new System.EventHandler(this.chkAndroid_CheckedChanged);
            // 
            // chkUseNew
            // 
            this.chkUseNew.AutoSize = true;
            this.chkUseNew.Location = new System.Drawing.Point(14, 19);
            this.chkUseNew.Name = "chkUseNew";
            this.chkUseNew.Size = new System.Drawing.Size(137, 17);
            this.chkUseNew.TabIndex = 22;
            this.chkUseNew.Text = "Use New X-Ray Format";
            this.chkUseNew.UseVisualStyleBackColor = true;
            // 
            // tabPgAliasChapter
            // 
            this.tabPgAliasChapter.Controls.Add(this.groupBox6);
            this.tabPgAliasChapter.Controls.Add(this.groupBox5);
            this.tabPgAliasChapter.Location = new System.Drawing.Point(4, 5);
            this.tabPgAliasChapter.Name = "tabPgAliasChapter";
            this.tabPgAliasChapter.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgAliasChapter.Size = new System.Drawing.Size(390, 259);
            this.tabPgAliasChapter.TabIndex = 3;
            this.tabPgAliasChapter.Text = "tabPage4";
            this.tabPgAliasChapter.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.chkChapters);
            this.groupBox6.Controls.Add(this.chkAlias);
            this.groupBox6.Controls.Add(this.chkOverwrite);
            this.groupBox6.Location = new System.Drawing.Point(6, 118);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(378, 100);
            this.groupBox6.TabIndex = 31;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Output";
            // 
            // chkChapters
            // 
            this.chkChapters.AutoSize = true;
            this.chkChapters.Location = new System.Drawing.Point(14, 71);
            this.chkChapters.Name = "chkChapters";
            this.chkChapters.Size = new System.Drawing.Size(68, 17);
            this.chkChapters.TabIndex = 23;
            this.chkChapters.Text = "Chapters";
            this.chkChapters.UseVisualStyleBackColor = true;
            // 
            // chkAlias
            // 
            this.chkAlias.AutoSize = true;
            this.chkAlias.Location = new System.Drawing.Point(14, 45);
            this.chkAlias.Name = "chkAlias";
            this.chkAlias.Size = new System.Drawing.Size(59, 17);
            this.chkAlias.TabIndex = 30;
            this.chkAlias.Text = "Aliases";
            this.chkAlias.UseVisualStyleBackColor = true;
            // 
            // chkOverwrite
            // 
            this.chkOverwrite.AutoSize = true;
            this.chkOverwrite.Location = new System.Drawing.Point(14, 19);
            this.chkOverwrite.Name = "chkOverwrite";
            this.chkOverwrite.Size = new System.Drawing.Size(130, 17);
            this.chkOverwrite.TabIndex = 26;
            this.chkOverwrite.Text = "Overwrite existing files";
            this.chkOverwrite.UseVisualStyleBackColor = true;
            this.chkOverwrite.CheckedChanged += new System.EventHandler(this.chkOverwrite_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.chkDownloadAliases);
            this.groupBox5.Controls.Add(this.chkSplitAliases);
            this.groupBox5.Controls.Add(this.chkEnableEdit);
            this.groupBox5.Location = new System.Drawing.Point(6, 12);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(378, 100);
            this.groupBox5.TabIndex = 26;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Configuration";
            // 
            // chkDownloadAliases
            // 
            this.chkDownloadAliases.AutoSize = true;
            this.chkDownloadAliases.Location = new System.Drawing.Point(14, 71);
            this.chkDownloadAliases.Name = "chkDownloadAliases";
            this.chkDownloadAliases.Size = new System.Drawing.Size(109, 17);
            this.chkDownloadAliases.TabIndex = 33;
            this.chkDownloadAliases.Text = "Download aliases";
            this.chkDownloadAliases.UseVisualStyleBackColor = true;
            this.chkDownloadAliases.CheckedChanged += new System.EventHandler(this.chkDownloadAliases_CheckedChanged);
            // 
            // chkSplitAliases
            // 
            this.chkSplitAliases.AutoSize = true;
            this.chkSplitAliases.Location = new System.Drawing.Point(14, 45);
            this.chkSplitAliases.Name = "chkSplitAliases";
            this.chkSplitAliases.Size = new System.Drawing.Size(145, 17);
            this.chkSplitAliases.TabIndex = 29;
            this.chkSplitAliases.Text = "Automatically split Aliases";
            this.chkSplitAliases.UseVisualStyleBackColor = true;
            // 
            // chkEnableEdit
            // 
            this.chkEnableEdit.AutoSize = true;
            this.chkEnableEdit.Location = new System.Drawing.Point(14, 19);
            this.chkEnableEdit.Name = "chkEnableEdit";
            this.chkEnableEdit.Size = new System.Drawing.Size(207, 17);
            this.chkEnableEdit.TabIndex = 24;
            this.chkEnableEdit.Text = "Enable editing of Chapters and Aliases";
            this.chkEnableEdit.UseVisualStyleBackColor = true;
            // 
            // tabPgAmazon
            // 
            this.tabPgAmazon.Controls.Add(this.gbAmazonPrefs);
            this.tabPgAmazon.Controls.Add(this.gbDetails);
            this.tabPgAmazon.Location = new System.Drawing.Point(4, 5);
            this.tabPgAmazon.Name = "tabPgAmazon";
            this.tabPgAmazon.Padding = new System.Windows.Forms.Padding(3);
            this.tabPgAmazon.Size = new System.Drawing.Size(390, 259);
            this.tabPgAmazon.TabIndex = 4;
            this.tabPgAmazon.Text = "tabPage5";
            this.tabPgAmazon.UseVisualStyleBackColor = true;
            // 
            // gbAmazonPrefs
            // 
            this.gbAmazonPrefs.Controls.Add(this.chkEditBiography);
            this.gbAmazonPrefs.Controls.Add(this.chkDownloadSA);
            this.gbAmazonPrefs.Controls.Add(this.chkPromptAsin);
            this.gbAmazonPrefs.Controls.Add(this.label1);
            this.gbAmazonPrefs.Controls.Add(this.cmbRegion);
            this.gbAmazonPrefs.Controls.Add(this.chkSaveBio);
            this.gbAmazonPrefs.Location = new System.Drawing.Point(6, 106);
            this.gbAmazonPrefs.Name = "gbAmazonPrefs";
            this.gbAmazonPrefs.Size = new System.Drawing.Size(378, 147);
            this.gbAmazonPrefs.TabIndex = 29;
            this.gbAmazonPrefs.TabStop = false;
            this.gbAmazonPrefs.Text = "Preferences";
            // 
            // chkEditBiography
            // 
            this.chkEditBiography.AutoSize = true;
            this.chkEditBiography.Location = new System.Drawing.Point(153, 45);
            this.chkEditBiography.Name = "chkEditBiography";
            this.chkEditBiography.Size = new System.Drawing.Size(126, 17);
            this.chkEditBiography.TabIndex = 29;
            this.chkEditBiography.Text = "Edit author biography";
            this.chkEditBiography.UseVisualStyleBackColor = true;
            // 
            // chkDownloadSA
            // 
            this.chkDownloadSA.AutoSize = true;
            this.chkDownloadSA.Location = new System.Drawing.Point(14, 68);
            this.chkDownloadSA.Name = "chkDownloadSA";
            this.chkDownloadSA.Size = new System.Drawing.Size(137, 17);
            this.chkDownloadSA.TabIndex = 28;
            this.chkDownloadSA.Text = "Download Start Actions";
            this.chkDownloadSA.UseVisualStyleBackColor = true;
            // 
            // chkPromptAsin
            // 
            this.chkPromptAsin.AutoSize = true;
            this.chkPromptAsin.Location = new System.Drawing.Point(14, 19);
            this.chkPromptAsin.Name = "chkPromptAsin";
            this.chkPromptAsin.Size = new System.Drawing.Size(116, 17);
            this.chkPromptAsin.TabIndex = 27;
            this.chkPromptAsin.Text = "Show ASIN prompt";
            this.chkPromptAsin.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Region:";
            // 
            // cmbRegion
            // 
            this.cmbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegion.FormattingEnabled = true;
            this.cmbRegion.Location = new System.Drawing.Point(62, 91);
            this.cmbRegion.Name = "cmbRegion";
            this.cmbRegion.Size = new System.Drawing.Size(78, 21);
            this.cmbRegion.TabIndex = 25;
            // 
            // chkSaveBio
            // 
            this.chkSaveBio.AutoSize = true;
            this.chkSaveBio.Location = new System.Drawing.Point(14, 45);
            this.chkSaveBio.Name = "chkSaveBio";
            this.chkSaveBio.Size = new System.Drawing.Size(133, 17);
            this.chkSaveBio.TabIndex = 24;
            this.chkSaveBio.Text = "Save author biography";
            this.chkSaveBio.UseVisualStyleBackColor = true;
            // 
            // gbDetails
            // 
            this.gbDetails.Controls.Add(this.lblReal);
            this.gbDetails.Controls.Add(this.txtReal);
            this.gbDetails.Controls.Add(this.txtPen);
            this.gbDetails.Controls.Add(this.lblPen);
            this.gbDetails.Location = new System.Drawing.Point(6, 12);
            this.gbDetails.Name = "gbDetails";
            this.gbDetails.Size = new System.Drawing.Size(378, 88);
            this.gbDetails.TabIndex = 27;
            this.gbDetails.TabStop = false;
            this.gbDetails.Text = "Amazon Customer Details";
            // 
            // lblReal
            // 
            this.lblReal.AutoSize = true;
            this.lblReal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReal.Location = new System.Drawing.Point(12, 23);
            this.lblReal.Name = "lblReal";
            this.lblReal.Size = new System.Drawing.Size(63, 13);
            this.lblReal.TabIndex = 17;
            this.lblReal.Text = "Real Name:";
            // 
            // txtReal
            // 
            this.txtReal.Location = new System.Drawing.Point(81, 20);
            this.txtReal.Name = "txtReal";
            this.txtReal.Size = new System.Drawing.Size(283, 20);
            this.txtReal.TabIndex = 18;
            // 
            // txtPen
            // 
            this.txtPen.Location = new System.Drawing.Point(81, 53);
            this.txtPen.Name = "txtPen";
            this.txtPen.Size = new System.Drawing.Size(283, 20);
            this.txtPen.TabIndex = 20;
            // 
            // lblPen
            // 
            this.lblPen.AutoSize = true;
            this.lblPen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPen.Location = new System.Drawing.Point(15, 56);
            this.lblPen.Name = "lblPen";
            this.lblPen.Size = new System.Drawing.Size(60, 13);
            this.lblPen.TabIndex = 19;
            this.lblPen.Text = "Pen Name:";
            // 
            // listSettings
            // 
            this.listSettings.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listSettings.FormattingEnabled = true;
            this.listSettings.IntegralHeight = false;
            this.listSettings.ItemHeight = 20;
            this.listSettings.Items.AddRange(new object[] {
            "General",
            "Directories",
            "X-Ray",
            "Aliases and Chapters",
            "Amazon"});
            this.listSettings.Location = new System.Drawing.Point(12, 12);
            this.listSettings.Name = "listSettings";
            this.listSettings.Size = new System.Drawing.Size(130, 253);
            this.listSettings.TabIndex = 38;
            this.listSettings.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listSettings_DrawItem);
            this.listSettings.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.listSettings_MeasureItem);
            this.listSettings.SelectedIndexChanged += new System.EventHandler(this.listSettings_SelectedIndexChanged);
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 331);
            this.Controls.Add(this.listSettings);
            this.Controls.Add(this.tabSettings);
            this.Controls.Add(this.lblSeperator);
            this.Controls.Add(this.btnSupport);
            this.Controls.Add(this.btnClearLogs);
            this.Controls.Add(this.btnLogs);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettings";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSettingsNew_Load);
            this.tabSettings.ResumeLayout(false);
            this.tabPgGeneral.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbProcess.ResumeLayout(false);
            this.gbProcess.PerformLayout();
            this.gbUnpack.ResumeLayout(false);
            this.gbUnpack.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.tabPgDirectories.ResumeLayout(false);
            this.gbDirectories.ResumeLayout(false);
            this.gbDirectories.PerformLayout();
            this.tabPgXray.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.gbXray.ResumeLayout(false);
            this.gbXray.PerformLayout();
            this.tabPgAliasChapter.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabPgAmazon.ResumeLayout(false);
            this.gbAmazonPrefs.ResumeLayout(false);
            this.gbAmazonPrefs.PerformLayout();
            this.gbDetails.ResumeLayout(false);
            this.gbDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSupport;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.Button btnLogs;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblSeperator;
        private System.Windows.Forms.TabControl tabSettings;
        private System.Windows.Forms.TabPage tabPgGeneral;
        private System.Windows.Forms.TabPage tabPgDirectories;
        private System.Windows.Forms.TabPage tabPgAliasChapter;
        private System.Windows.Forms.TabPage tabPgAmazon;
        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.CheckBox chkSound;
        private System.Windows.Forms.GroupBox gbDirectories;
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.GroupBox gbUnpack;
        private System.Windows.Forms.CheckBox chkSaveHtml;
        private System.Windows.Forms.CheckBox chkRaw;
        private System.Windows.Forms.CheckBox chkSubDirectories;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkAlias;
        private System.Windows.Forms.CheckBox chkSplitAliases;
        private System.Windows.Forms.CheckBox chkEnableEdit;
        private System.Windows.Forms.CheckBox chkDownloadAliases;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox chkChapters;
        private System.Windows.Forms.CheckBox chkOverwrite;
        private System.Windows.Forms.GroupBox gbDetails;
        private System.Windows.Forms.Label lblReal;
        private System.Windows.Forms.TextBox txtReal;
        private System.Windows.Forms.TextBox txtPen;
        private System.Windows.Forms.Label lblPen;
        private System.Windows.Forms.ListBox listSettings;
        private System.Windows.Forms.CheckBox chkPageCount;
        private System.Windows.Forms.TabPage tabPgXray;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkSoftHyphen;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkSkipNoLikes;
        private System.Windows.Forms.GroupBox gbXray;
        private System.Windows.Forms.CheckBox chkAndroid;
        private System.Windows.Forms.CheckBox chkUseNew;
        private System.Windows.Forms.GroupBox gbProcess;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rdoShelfari;
        private System.Windows.Forms.RadioButton rdoGoodreads;
        private System.Windows.Forms.GroupBox gbAmazonPrefs;
        private System.Windows.Forms.CheckBox chkSaveBio;
        private System.Windows.Forms.ComboBox cmbRegion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkPromptAsin;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMinClipLen;
        private System.Windows.Forms.CheckBox chkDownloadSA;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkSearchAsin;
        private System.Windows.Forms.CheckBox chkEditBiography;
        private System.Windows.Forms.CheckBox chkUseSidecar;
    }
}