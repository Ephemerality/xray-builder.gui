namespace XRayBuilderGUI
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
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.lblOut = new System.Windows.Forms.Label();
            this.btnBrowseUnpack = new System.Windows.Forms.Button();
            this.txtUnpack = new System.Windows.Forms.TextBox();
            this.lblUnpack = new System.Windows.Forms.Label();
            this.chkRaw = new System.Windows.Forms.CheckBox();
            this.chkSpoilers = new System.Windows.Forms.CheckBox();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.lblOffset = new System.Windows.Forms.Label();
            this.chkSoftHyphen = new System.Windows.Forms.CheckBox();
            this.chkUseNew = new System.Windows.Forms.CheckBox();
            this.chkAndroid = new System.Windows.Forms.CheckBox();
            this.gbDirectories = new System.Windows.Forms.GroupBox();
            this.chkSubDirectories = new System.Windows.Forms.CheckBox();
            this.gbXray = new System.Windows.Forms.GroupBox();
            this.chkAliasChapters = new System.Windows.Forms.CheckBox();
            this.chkSplitAliases = new System.Windows.Forms.CheckBox();
            this.chkSaveHtml = new System.Windows.Forms.CheckBox();
            this.chkOverwrite = new System.Windows.Forms.CheckBox();
            this.chkUTF8 = new System.Windows.Forms.CheckBox();
            this.chkEnableEdit = new System.Windows.Forms.CheckBox();
            this.gbDetails = new System.Windows.Forms.GroupBox();
            this.lblReal = new System.Windows.Forms.Label();
            this.txtReal = new System.Windows.Forms.TextBox();
            this.txtPen = new System.Windows.Forms.TextBox();
            this.lblPen = new System.Windows.Forms.Label();
            this.chkAmazonUK = new System.Windows.Forms.CheckBox();
            this.chkAmazonUSA = new System.Windows.Forms.CheckBox();
            this.gbSite = new System.Windows.Forms.GroupBox();
            this.btnLogs = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.chkKindleUnpack = new System.Windows.Forms.CheckBox();
            this.chkSound = new System.Windows.Forms.CheckBox();
            this.chkDownloadAliases = new System.Windows.Forms.CheckBox();
            this.gbDirectories.SuspendLayout();
            this.gbXray.SuspendLayout();
            this.gbDetails.SuspendLayout();
            this.gbSite.SuspendLayout();
            this.gbGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(422, 384);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Location = new System.Drawing.Point(471, 49);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(34, 23);
            this.btnBrowseOut.TabIndex = 13;
            this.btnBrowseOut.Text = "...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // txtOut
            // 
            this.txtOut.Location = new System.Drawing.Point(153, 51);
            this.txtOut.Name = "txtOut";
            this.txtOut.Size = new System.Drawing.Size(312, 20);
            this.txtOut.TabIndex = 12;
            // 
            // lblOut
            // 
            this.lblOut.AutoSize = true;
            this.lblOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOut.Location = new System.Drawing.Point(60, 54);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(87, 13);
            this.lblOut.TabIndex = 11;
            this.lblOut.Text = "Output Directory:";
            // 
            // btnBrowseUnpack
            // 
            this.btnBrowseUnpack.Location = new System.Drawing.Point(471, 20);
            this.btnBrowseUnpack.Name = "btnBrowseUnpack";
            this.btnBrowseUnpack.Size = new System.Drawing.Size(34, 23);
            this.btnBrowseUnpack.TabIndex = 16;
            this.btnBrowseUnpack.Text = "...";
            this.btnBrowseUnpack.UseVisualStyleBackColor = true;
            this.btnBrowseUnpack.Click += new System.EventHandler(this.btnBrowseUnpack_Click);
            // 
            // txtUnpack
            // 
            this.txtUnpack.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtUnpack.Enabled = false;
            this.txtUnpack.Location = new System.Drawing.Point(153, 22);
            this.txtUnpack.Name = "txtUnpack";
            this.txtUnpack.Size = new System.Drawing.Size(312, 20);
            this.txtUnpack.TabIndex = 15;
            // 
            // lblUnpack
            // 
            this.lblUnpack.AutoSize = true;
            this.lblUnpack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnpack.Location = new System.Drawing.Point(28, 25);
            this.lblUnpack.Name = "lblUnpack";
            this.lblUnpack.Size = new System.Drawing.Size(121, 13);
            this.lblUnpack.TabIndex = 14;
            this.lblUnpack.Text = "KindleUnpack Location:";
            // 
            // chkRaw
            // 
            this.chkRaw.AutoSize = true;
            this.chkRaw.Location = new System.Drawing.Point(145, 26);
            this.chkRaw.Name = "chkRaw";
            this.chkRaw.Size = new System.Drawing.Size(81, 17);
            this.chkRaw.TabIndex = 17;
            this.chkRaw.Text = "Save rawml";
            this.chkRaw.UseVisualStyleBackColor = true;
            // 
            // chkSpoilers
            // 
            this.chkSpoilers.AutoSize = true;
            this.chkSpoilers.Location = new System.Drawing.Point(408, 26);
            this.chkSpoilers.Name = "chkSpoilers";
            this.chkSpoilers.Size = new System.Drawing.Size(63, 17);
            this.chkSpoilers.TabIndex = 18;
            this.chkSpoilers.Text = "Spoilers";
            this.chkSpoilers.UseVisualStyleBackColor = true;
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(337, 93);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(47, 20);
            this.txtOffset.TabIndex = 20;
            // 
            // lblOffset
            // 
            this.lblOffset.AutoSize = true;
            this.lblOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOffset.Location = new System.Drawing.Point(252, 96);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(77, 13);
            this.lblOffset.TabIndex = 19;
            this.lblOffset.Text = "Excerpt Offset:";
            // 
            // chkSoftHyphen
            // 
            this.chkSoftHyphen.AutoSize = true;
            this.chkSoftHyphen.Location = new System.Drawing.Point(9, 26);
            this.chkSoftHyphen.Name = "chkSoftHyphen";
            this.chkSoftHyphen.Size = new System.Drawing.Size(123, 17);
            this.chkSoftHyphen.TabIndex = 21;
            this.chkSoftHyphen.Text = "Ignore Soft Hyphens";
            this.chkSoftHyphen.UseVisualStyleBackColor = true;
            // 
            // chkUseNew
            // 
            this.chkUseNew.AutoSize = true;
            this.chkUseNew.Location = new System.Drawing.Point(255, 26);
            this.chkUseNew.Name = "chkUseNew";
            this.chkUseNew.Size = new System.Drawing.Size(137, 17);
            this.chkUseNew.TabIndex = 22;
            this.chkUseNew.Text = "Use New X-Ray Format";
            this.chkUseNew.UseVisualStyleBackColor = true;
            // 
            // chkAndroid
            // 
            this.chkAndroid.AutoSize = true;
            this.chkAndroid.Location = new System.Drawing.Point(408, 72);
            this.chkAndroid.Name = "chkAndroid";
            this.chkAndroid.Size = new System.Drawing.Size(103, 17);
            this.chkAndroid.TabIndex = 23;
            this.chkAndroid.Text = "Build for Android";
            this.chkAndroid.UseVisualStyleBackColor = true;
            this.chkAndroid.CheckedChanged += new System.EventHandler(this.chkAndroid_CheckedChanged);
            // 
            // gbDirectories
            // 
            this.gbDirectories.Controls.Add(this.lblOut);
            this.gbDirectories.Controls.Add(this.txtOut);
            this.gbDirectories.Controls.Add(this.btnBrowseOut);
            this.gbDirectories.Controls.Add(this.lblUnpack);
            this.gbDirectories.Controls.Add(this.txtUnpack);
            this.gbDirectories.Controls.Add(this.btnBrowseUnpack);
            this.gbDirectories.Location = new System.Drawing.Point(12, 12);
            this.gbDirectories.Name = "gbDirectories";
            this.gbDirectories.Size = new System.Drawing.Size(520, 87);
            this.gbDirectories.TabIndex = 24;
            this.gbDirectories.TabStop = false;
            this.gbDirectories.Text = "Directories";
            // 
            // chkSubDirectories
            // 
            this.chkSubDirectories.AutoSize = true;
            this.chkSubDirectories.Location = new System.Drawing.Point(255, 49);
            this.chkSubDirectories.Name = "chkSubDirectories";
            this.chkSubDirectories.Size = new System.Drawing.Size(113, 17);
            this.chkSubDirectories.TabIndex = 25;
            this.chkSubDirectories.Text = "Use subdirectories";
            this.chkSubDirectories.UseVisualStyleBackColor = true;
            // 
            // gbXray
            // 
            this.gbXray.Controls.Add(this.chkAliasChapters);
            this.gbXray.Controls.Add(this.chkSplitAliases);
            this.gbXray.Controls.Add(this.chkSaveHtml);
            this.gbXray.Controls.Add(this.chkOverwrite);
            this.gbXray.Controls.Add(this.chkSubDirectories);
            this.gbXray.Controls.Add(this.chkUTF8);
            this.gbXray.Controls.Add(this.chkEnableEdit);
            this.gbXray.Controls.Add(this.chkRaw);
            this.gbXray.Controls.Add(this.chkSpoilers);
            this.gbXray.Controls.Add(this.chkSoftHyphen);
            this.gbXray.Controls.Add(this.txtOffset);
            this.gbXray.Controls.Add(this.chkAndroid);
            this.gbXray.Controls.Add(this.lblOffset);
            this.gbXray.Controls.Add(this.chkUseNew);
            this.gbXray.Location = new System.Drawing.Point(12, 164);
            this.gbXray.Name = "gbXray";
            this.gbXray.Size = new System.Drawing.Size(520, 122);
            this.gbXray.TabIndex = 25;
            this.gbXray.TabStop = false;
            this.gbXray.Text = "X-Ray Configuration";
            // 
            // chkAliasChapters
            // 
            this.chkAliasChapters.AutoSize = true;
            this.chkAliasChapters.Location = new System.Drawing.Point(145, 49);
            this.chkAliasChapters.Name = "chkAliasChapters";
            this.chkAliasChapters.Size = new System.Drawing.Size(106, 17);
            this.chkAliasChapters.TabIndex = 30;
            this.chkAliasChapters.Text = "Aliases/Chapters";
            this.chkAliasChapters.UseVisualStyleBackColor = true;
            // 
            // chkSplitAliases
            // 
            this.chkSplitAliases.AutoSize = true;
            this.chkSplitAliases.Location = new System.Drawing.Point(255, 72);
            this.chkSplitAliases.Name = "chkSplitAliases";
            this.chkSplitAliases.Size = new System.Drawing.Size(145, 17);
            this.chkSplitAliases.TabIndex = 29;
            this.chkSplitAliases.Text = "Automatically split Aliases";
            this.chkSplitAliases.UseVisualStyleBackColor = true;
            // 
            // chkSaveHtml
            // 
            this.chkSaveHtml.AutoSize = true;
            this.chkSaveHtml.Location = new System.Drawing.Point(408, 49);
            this.chkSaveHtml.Name = "chkSaveHtml";
            this.chkSaveHtml.Size = new System.Drawing.Size(84, 17);
            this.chkSaveHtml.TabIndex = 28;
            this.chkSaveHtml.Text = "Save HTML";
            this.chkSaveHtml.UseVisualStyleBackColor = true;
            // 
            // chkOverwrite
            // 
            this.chkOverwrite.AutoSize = true;
            this.chkOverwrite.Location = new System.Drawing.Point(9, 49);
            this.chkOverwrite.Name = "chkOverwrite";
            this.chkOverwrite.Size = new System.Drawing.Size(130, 17);
            this.chkOverwrite.TabIndex = 26;
            this.chkOverwrite.Text = "Overwrite existing files";
            this.chkOverwrite.UseVisualStyleBackColor = true;
            this.chkOverwrite.CheckedChanged += new System.EventHandler(this.chkOverwrite_CheckedChanged);
            // 
            // chkUTF8
            // 
            this.chkUTF8.AutoSize = true;
            this.chkUTF8.Location = new System.Drawing.Point(9, 72);
            this.chkUTF8.Name = "chkUTF8";
            this.chkUTF8.Size = new System.Drawing.Size(99, 17);
            this.chkUTF8.TabIndex = 25;
            this.chkUTF8.Text = "Output in UTF8";
            this.chkUTF8.UseVisualStyleBackColor = true;
            // 
            // chkEnableEdit
            // 
            this.chkEnableEdit.AutoSize = true;
            this.chkEnableEdit.Location = new System.Drawing.Point(9, 95);
            this.chkEnableEdit.Name = "chkEnableEdit";
            this.chkEnableEdit.Size = new System.Drawing.Size(207, 17);
            this.chkEnableEdit.TabIndex = 24;
            this.chkEnableEdit.Text = "Enable editing of Chapters and Aliases";
            this.chkEnableEdit.UseVisualStyleBackColor = true;
            // 
            // gbDetails
            // 
            this.gbDetails.Controls.Add(this.lblReal);
            this.gbDetails.Controls.Add(this.txtReal);
            this.gbDetails.Controls.Add(this.txtPen);
            this.gbDetails.Controls.Add(this.lblPen);
            this.gbDetails.Location = new System.Drawing.Point(12, 292);
            this.gbDetails.Name = "gbDetails";
            this.gbDetails.Size = new System.Drawing.Size(389, 86);
            this.gbDetails.TabIndex = 26;
            this.gbDetails.TabStop = false;
            this.gbDetails.Text = "Amazon Customer Details";
            // 
            // lblReal
            // 
            this.lblReal.AutoSize = true;
            this.lblReal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReal.Location = new System.Drawing.Point(6, 25);
            this.lblReal.Name = "lblReal";
            this.lblReal.Size = new System.Drawing.Size(63, 13);
            this.lblReal.TabIndex = 17;
            this.lblReal.Text = "Real Name:";
            // 
            // txtReal
            // 
            this.txtReal.Location = new System.Drawing.Point(75, 22);
            this.txtReal.Name = "txtReal";
            this.txtReal.Size = new System.Drawing.Size(300, 20);
            this.txtReal.TabIndex = 18;
            // 
            // txtPen
            // 
            this.txtPen.Location = new System.Drawing.Point(75, 51);
            this.txtPen.Name = "txtPen";
            this.txtPen.Size = new System.Drawing.Size(300, 20);
            this.txtPen.TabIndex = 20;
            // 
            // lblPen
            // 
            this.lblPen.AutoSize = true;
            this.lblPen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPen.Location = new System.Drawing.Point(9, 54);
            this.lblPen.Name = "lblPen";
            this.lblPen.Size = new System.Drawing.Size(60, 13);
            this.lblPen.TabIndex = 19;
            this.lblPen.Text = "Pen Name:";
            // 
            // chkAmazonUK
            // 
            this.chkAmazonUK.AutoSize = true;
            this.chkAmazonUK.Location = new System.Drawing.Point(9, 24);
            this.chkAmazonUK.Name = "chkAmazonUK";
            this.chkAmazonUK.Size = new System.Drawing.Size(94, 17);
            this.chkAmazonUK.TabIndex = 24;
            this.chkAmazonUK.Text = "Amazon.co.uk";
            this.chkAmazonUK.UseVisualStyleBackColor = true;
            // 
            // chkAmazonUSA
            // 
            this.chkAmazonUSA.AutoSize = true;
            this.chkAmazonUSA.Checked = true;
            this.chkAmazonUSA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAmazonUSA.Enabled = false;
            this.chkAmazonUSA.Location = new System.Drawing.Point(9, 53);
            this.chkAmazonUSA.Name = "chkAmazonUSA";
            this.chkAmazonUSA.Size = new System.Drawing.Size(87, 17);
            this.chkAmazonUSA.TabIndex = 25;
            this.chkAmazonUSA.Text = "Amazon.com";
            this.chkAmazonUSA.UseVisualStyleBackColor = true;
            // 
            // gbSite
            // 
            this.gbSite.Controls.Add(this.chkAmazonUSA);
            this.gbSite.Controls.Add(this.chkAmazonUK);
            this.gbSite.Enabled = false;
            this.gbSite.Location = new System.Drawing.Point(411, 292);
            this.gbSite.Name = "gbSite";
            this.gbSite.Size = new System.Drawing.Size(121, 86);
            this.gbSite.TabIndex = 27;
            this.gbSite.TabStop = false;
            this.gbSite.Text = "Amazon Site";
            // 
            // btnLogs
            // 
            this.btnLogs.Location = new System.Drawing.Point(12, 384);
            this.btnLogs.Name = "btnLogs";
            this.btnLogs.Size = new System.Drawing.Size(110, 30);
            this.btnLogs.TabIndex = 30;
            this.btnLogs.Text = "Logs";
            this.btnLogs.UseVisualStyleBackColor = true;
            this.btnLogs.Click += new System.EventHandler(this.btnLogs_Click);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblVersion.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblVersion.Location = new System.Drawing.Point(281, 393);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(135, 13);
            this.lblVersion.TabIndex = 32;
            this.lblVersion.Text = "X-Ray Builder GUI v0.0.0.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Location = new System.Drawing.Point(128, 384);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(110, 30);
            this.btnClearLogs.TabIndex = 33;
            this.btnClearLogs.Text = "Clear Logs";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(244, 384);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(30, 30);
            this.btnHelp.TabIndex = 34;
            this.btnHelp.Text = "?";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // gbGeneral
            // 
            this.gbGeneral.Controls.Add(this.chkDownloadAliases);
            this.gbGeneral.Controls.Add(this.chkKindleUnpack);
            this.gbGeneral.Controls.Add(this.chkSound);
            this.gbGeneral.Location = new System.Drawing.Point(12, 105);
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.Size = new System.Drawing.Size(520, 53);
            this.gbGeneral.TabIndex = 35;
            this.gbGeneral.TabStop = false;
            this.gbGeneral.Text = "General";
            // 
            // chkKindleUnpack
            // 
            this.chkKindleUnpack.AutoSize = true;
            this.chkKindleUnpack.Location = new System.Drawing.Point(232, 26);
            this.chkKindleUnpack.Name = "chkKindleUnpack";
            this.chkKindleUnpack.Size = new System.Drawing.Size(115, 17);
            this.chkKindleUnpack.TabIndex = 31;
            this.chkKindleUnpack.Text = "Use KindleUnpack";
            this.chkKindleUnpack.UseVisualStyleBackColor = true;
            this.chkKindleUnpack.CheckedChanged += new System.EventHandler(this.chkKindleUnpack_CheckedChanged);
            // 
            // chkSound
            // 
            this.chkSound.AutoSize = true;
            this.chkSound.Location = new System.Drawing.Point(9, 26);
            this.chkSound.Name = "chkSound";
            this.chkSound.Size = new System.Drawing.Size(216, 17);
            this.chkSound.TabIndex = 30;
            this.chkSound.Text = "Play a sound when a process completes";
            this.chkSound.UseVisualStyleBackColor = true;
            // 
            // chkDownloadAliases
            // 
            this.chkDownloadAliases.AutoSize = true;
            this.chkDownloadAliases.Location = new System.Drawing.Point(353, 26);
            this.chkDownloadAliases.Name = "chkDownloadAliases";
            this.chkDownloadAliases.Size = new System.Drawing.Size(109, 17);
            this.chkDownloadAliases.TabIndex = 32;
            this.chkDownloadAliases.Text = "Download aliases";
            this.chkDownloadAliases.UseVisualStyleBackColor = true;
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 426);
            this.Controls.Add(this.gbGeneral);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnClearLogs);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.btnLogs);
            this.Controls.Add(this.gbSite);
            this.Controls.Add(this.gbDetails);
            this.Controls.Add(this.gbXray);
            this.Controls.Add(this.gbDirectories);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.gbDirectories.ResumeLayout(false);
            this.gbDirectories.PerformLayout();
            this.gbXray.ResumeLayout(false);
            this.gbXray.PerformLayout();
            this.gbDetails.ResumeLayout(false);
            this.gbDetails.PerformLayout();
            this.gbSite.ResumeLayout(false);
            this.gbSite.PerformLayout();
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.Button btnBrowseUnpack;
        private System.Windows.Forms.TextBox txtUnpack;
        private System.Windows.Forms.Label lblUnpack;
        private System.Windows.Forms.CheckBox chkRaw;
        private System.Windows.Forms.CheckBox chkSpoilers;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Label lblOffset;
        private System.Windows.Forms.CheckBox chkSoftHyphen;
        private System.Windows.Forms.CheckBox chkUseNew;
        private System.Windows.Forms.CheckBox chkAndroid;
        private System.Windows.Forms.GroupBox gbDirectories;
        private System.Windows.Forms.GroupBox gbXray;
        private System.Windows.Forms.GroupBox gbDetails;
        private System.Windows.Forms.Label lblReal;
        private System.Windows.Forms.TextBox txtReal;
        private System.Windows.Forms.TextBox txtPen;
        private System.Windows.Forms.Label lblPen;
        private System.Windows.Forms.CheckBox chkAmazonUSA;
        private System.Windows.Forms.CheckBox chkAmazonUK;
        private System.Windows.Forms.GroupBox gbSite;
        private System.Windows.Forms.CheckBox chkEnableEdit;
        private System.Windows.Forms.CheckBox chkSubDirectories;
        private System.Windows.Forms.Button btnLogs;
        private System.Windows.Forms.CheckBox chkUTF8;
        private System.Windows.Forms.CheckBox chkOverwrite;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.CheckBox chkSaveHtml;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.CheckBox chkSplitAliases;
        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.CheckBox chkSound;
        private System.Windows.Forms.CheckBox chkAliasChapters;
        private System.Windows.Forms.CheckBox chkKindleUnpack;
        private System.Windows.Forms.CheckBox chkDownloadAliases;
    }
}