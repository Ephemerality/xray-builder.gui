namespace XRayBuilderGUI
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.btnBuild = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.prgBar = new System.Windows.Forms.ProgressBar();
            this.btnShelfari = new System.Windows.Forms.Button();
            this.lblShelfari = new System.Windows.Forms.Label();
            this.btnKindleExtras = new System.Windows.Forms.Button();
            this.lblSeperator1 = new System.Windows.Forms.Label();
            this.lblSeperator2 = new System.Windows.Forms.Label();
            this.txtMobi = new System.Windows.Forms.TextBox();
            this.btnBrowseMobi = new System.Windows.Forms.Button();
            this.btnBrowseXML = new System.Windows.Forms.Button();
            this.txtXMLFile = new System.Windows.Forms.TextBox();
            this.rdoShelfari = new System.Windows.Forms.RadioButton();
            this.rdoFile = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnLink = new System.Windows.Forms.Button();
            this.lblXMLFile = new System.Windows.Forms.Label();
            this.txtShelfari = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.cbEndAction = new System.Windows.Forms.CheckBox();
            this.cbAuthorProfile = new System.Windows.Forms.CheckBox();
            this.cbXray = new System.Windows.Forms.CheckBox();
            this.btnSearchShelfari = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBuild
            // 
            this.btnBuild.Location = new System.Drawing.Point(492, 12);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(110, 30);
            this.btnBuild.TabIndex = 14;
            this.btnBuild.Text = "Build X-Ray";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(252, 12);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(110, 30);
            this.btnSettings.TabIndex = 16;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // prgBar
            // 
            this.prgBar.Location = new System.Drawing.Point(12, 392);
            this.prgBar.Name = "prgBar";
            this.prgBar.Size = new System.Drawing.Size(356, 13);
            this.prgBar.Step = 1;
            this.prgBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgBar.TabIndex = 18;
            // 
            // btnShelfari
            // 
            this.btnShelfari.Location = new System.Drawing.Point(128, 12);
            this.btnShelfari.Name = "btnShelfari";
            this.btnShelfari.Size = new System.Drawing.Size(110, 30);
            this.btnShelfari.TabIndex = 19;
            this.btnShelfari.Text = "Save Shelfari Info";
            this.btnShelfari.UseVisualStyleBackColor = true;
            this.btnShelfari.Click += new System.EventHandler(this.btnShelfari_Click);
            // 
            // lblShelfari
            // 
            this.lblShelfari.AutoSize = true;
            this.lblShelfari.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShelfari.Location = new System.Drawing.Point(164, 20);
            this.lblShelfari.Name = "lblShelfari";
            this.lblShelfari.Size = new System.Drawing.Size(70, 13);
            this.lblShelfari.TabIndex = 8;
            this.lblShelfari.Text = "Shelfari URL:";
            this.lblShelfari.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnKindleExtras
            // 
            this.btnKindleExtras.Location = new System.Drawing.Point(376, 12);
            this.btnKindleExtras.Name = "btnKindleExtras";
            this.btnKindleExtras.Size = new System.Drawing.Size(110, 30);
            this.btnKindleExtras.TabIndex = 27;
            this.btnKindleExtras.Text = "Build Kindle Extras";
            this.btnKindleExtras.UseVisualStyleBackColor = true;
            // 
            // lblSeperator1
            // 
            this.lblSeperator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeperator1.Location = new System.Drawing.Point(244, 12);
            this.lblSeperator1.Name = "lblSeperator1";
            this.lblSeperator1.Size = new System.Drawing.Size(2, 30);
            this.lblSeperator1.TabIndex = 32;
            // 
            // lblSeperator2
            // 
            this.lblSeperator2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeperator2.Location = new System.Drawing.Point(368, 12);
            this.lblSeperator2.Name = "lblSeperator2";
            this.lblSeperator2.Size = new System.Drawing.Size(2, 30);
            this.lblSeperator2.TabIndex = 33;
            // 
            // txtMobi
            // 
            this.txtMobi.Location = new System.Drawing.Point(10, 17);
            this.txtMobi.Name = "txtMobi";
            this.txtMobi.Size = new System.Drawing.Size(493, 20);
            this.txtMobi.TabIndex = 1;
            // 
            // btnBrowseMobi
            // 
            this.btnBrowseMobi.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseMobi.Image")));
            this.btnBrowseMobi.Location = new System.Drawing.Point(509, 15);
            this.btnBrowseMobi.Name = "btnBrowseMobi";
            this.btnBrowseMobi.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseMobi.TabIndex = 10;
            this.btnBrowseMobi.UseVisualStyleBackColor = true;
            this.btnBrowseMobi.Click += new System.EventHandler(this.btnBrowseMobi_Click);
            // 
            // btnBrowseXML
            // 
            this.btnBrowseXML.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseXML.Image")));
            this.btnBrowseXML.Location = new System.Drawing.Point(548, 15);
            this.btnBrowseXML.Name = "btnBrowseXML";
            this.btnBrowseXML.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseXML.TabIndex = 23;
            this.btnBrowseXML.UseVisualStyleBackColor = true;
            this.btnBrowseXML.Visible = false;
            this.btnBrowseXML.Click += new System.EventHandler(this.btnBrowseXML_Click);
            // 
            // txtXMLFile
            // 
            this.txtXMLFile.Location = new System.Drawing.Point(240, 17);
            this.txtXMLFile.Name = "txtXMLFile";
            this.txtXMLFile.Size = new System.Drawing.Size(302, 20);
            this.txtXMLFile.TabIndex = 22;
            this.txtXMLFile.Visible = false;
            // 
            // rdoShelfari
            // 
            this.rdoShelfari.AutoSize = true;
            this.rdoShelfari.Checked = true;
            this.rdoShelfari.Location = new System.Drawing.Point(12, 18);
            this.rdoShelfari.Name = "rdoShelfari";
            this.rdoShelfari.Size = new System.Drawing.Size(60, 17);
            this.rdoShelfari.TabIndex = 0;
            this.rdoShelfari.TabStop = true;
            this.rdoShelfari.Text = "Shelfari";
            this.rdoShelfari.UseVisualStyleBackColor = true;
            this.rdoShelfari.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // rdoFile
            // 
            this.rdoFile.AutoSize = true;
            this.rdoFile.Location = new System.Drawing.Point(92, 18);
            this.rdoFile.Name = "rdoFile";
            this.rdoFile.Size = new System.Drawing.Size(41, 17);
            this.rdoFile.TabIndex = 1;
            this.rdoFile.Text = "File";
            this.rdoFile.UseVisualStyleBackColor = true;
            this.rdoFile.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnLink);
            this.groupBox1.Controls.Add(this.lblXMLFile);
            this.groupBox1.Controls.Add(this.rdoFile);
            this.groupBox1.Controls.Add(this.txtXMLFile);
            this.groupBox1.Controls.Add(this.rdoShelfari);
            this.groupBox1.Controls.Add(this.btnBrowseXML);
            this.groupBox1.Controls.Add(this.lblShelfari);
            this.groupBox1.Controls.Add(this.txtShelfari);
            this.groupBox1.Location = new System.Drawing.Point(12, 48);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(590, 48);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // btnLink
            // 
            this.btnLink.Image = ((System.Drawing.Image)(resources.GetObject("btnLink.Image")));
            this.btnLink.Location = new System.Drawing.Point(548, 15);
            this.btnLink.Name = "btnLink";
            this.btnLink.Size = new System.Drawing.Size(33, 23);
            this.btnLink.TabIndex = 28;
            this.btnLink.UseVisualStyleBackColor = true;
            this.btnLink.Click += new System.EventHandler(this.btnLink_Click);
            // 
            // lblXMLFile
            // 
            this.lblXMLFile.AutoSize = true;
            this.lblXMLFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblXMLFile.Location = new System.Drawing.Point(208, 20);
            this.lblXMLFile.Name = "lblXMLFile";
            this.lblXMLFile.Size = new System.Drawing.Size(26, 13);
            this.lblXMLFile.TabIndex = 26;
            this.lblXMLFile.Text = "File:";
            this.lblXMLFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblXMLFile.Visible = false;
            // 
            // txtShelfari
            // 
            this.txtShelfari.Location = new System.Drawing.Point(240, 17);
            this.txtShelfari.Name = "txtShelfari";
            this.txtShelfari.Size = new System.Drawing.Size(302, 20);
            this.txtShelfari.TabIndex = 27;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnBrowseOutput);
            this.groupBox3.Controls.Add(this.btnBrowseMobi);
            this.groupBox3.Controls.Add(this.txtMobi);
            this.groupBox3.Location = new System.Drawing.Point(12, 102);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(590, 48);
            this.groupBox3.TabIndex = 38;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Book";
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseOutput.Image")));
            this.btnBrowseOutput.Location = new System.Drawing.Point(548, 15);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseOutput.TabIndex = 11;
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.txtOutput.Location = new System.Drawing.Point(12, 156);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(590, 229);
            this.txtOutput.TabIndex = 17;
            // 
            // cbEndAction
            // 
            this.cbEndAction.AutoSize = true;
            this.cbEndAction.Enabled = false;
            this.cbEndAction.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cbEndAction.Location = new System.Drawing.Point(473, 391);
            this.cbEndAction.Name = "cbEndAction";
            this.cbEndAction.Size = new System.Drawing.Size(78, 17);
            this.cbEndAction.TabIndex = 11;
            this.cbEndAction.Text = "End Action";
            this.cbEndAction.UseVisualStyleBackColor = true;
            // 
            // cbAuthorProfile
            // 
            this.cbAuthorProfile.AutoSize = true;
            this.cbAuthorProfile.Enabled = false;
            this.cbAuthorProfile.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cbAuthorProfile.Location = new System.Drawing.Point(378, 391);
            this.cbAuthorProfile.Name = "cbAuthorProfile";
            this.cbAuthorProfile.Size = new System.Drawing.Size(89, 17);
            this.cbAuthorProfile.TabIndex = 39;
            this.cbAuthorProfile.Text = "Author Profile";
            this.cbAuthorProfile.UseVisualStyleBackColor = true;
            // 
            // cbXray
            // 
            this.cbXray.AutoSize = true;
            this.cbXray.Enabled = false;
            this.cbXray.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cbXray.Location = new System.Drawing.Point(557, 391);
            this.cbXray.Name = "cbXray";
            this.cbXray.Size = new System.Drawing.Size(50, 17);
            this.cbXray.TabIndex = 40;
            this.cbXray.Text = "X-ray";
            this.cbXray.UseVisualStyleBackColor = true;
            // 
            // btnSearchShelfari
            // 
            this.btnSearchShelfari.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearchShelfari.Location = new System.Drawing.Point(12, 12);
            this.btnSearchShelfari.Name = "btnSearchShelfari";
            this.btnSearchShelfari.Size = new System.Drawing.Size(110, 30);
            this.btnSearchShelfari.TabIndex = 26;
            this.btnSearchShelfari.Text = "Search Shelfari";
            this.btnSearchShelfari.UseVisualStyleBackColor = true;
            this.btnSearchShelfari.Click += new System.EventHandler(this.btnSearchShelfari_Click);
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 412);
            this.Controls.Add(this.cbXray);
            this.Controls.Add(this.cbAuthorProfile);
            this.Controls.Add(this.cbEndAction);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.lblSeperator2);
            this.Controls.Add(this.lblSeperator1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnKindleExtras);
            this.Controls.Add(this.btnSearchShelfari);
            this.Controls.Add(this.btnShelfari);
            this.Controls.Add(this.prgBar);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnBuild);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "X-Ray Builder GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Button btnSettings;
        public System.Windows.Forms.ProgressBar prgBar;
        private System.Windows.Forms.Button btnShelfari;
        private System.Windows.Forms.Button btnSearchShelfari;
        private System.Windows.Forms.Label lblShelfari;
        private System.Windows.Forms.Button btnKindleExtras;
        private System.Windows.Forms.Label lblSeperator1;
        private System.Windows.Forms.Label lblSeperator2;
        private System.Windows.Forms.TextBox txtMobi;
        private System.Windows.Forms.Button btnBrowseMobi;
        private System.Windows.Forms.Button btnBrowseXML;
        private System.Windows.Forms.TextBox txtXMLFile;
        private System.Windows.Forms.RadioButton rdoShelfari;
        private System.Windows.Forms.RadioButton rdoFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblXMLFile;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.TextBox txtShelfari;
        private System.Windows.Forms.Button btnLink;
        public System.Windows.Forms.CheckBox cbEndAction;
        public System.Windows.Forms.CheckBox cbAuthorProfile;
        public System.Windows.Forms.CheckBox cbXray;
        private System.Windows.Forms.Button btnBrowseOutput;
    }
}

