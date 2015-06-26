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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
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
            this.lblVersion = new System.Windows.Forms.Label();
            this.gbDirectories.SuspendLayout();
            this.gbXray.SuspendLayout();
            this.gbDetails.SuspendLayout();
            this.gbSite.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(367, 322);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseOut.Image")));
            this.btnBrowseOut.Location = new System.Drawing.Point(383, 49);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseOut.TabIndex = 13;
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // txtOut
            // 
            this.txtOut.Location = new System.Drawing.Point(99, 51);
            this.txtOut.Name = "txtOut";
            this.txtOut.Size = new System.Drawing.Size(278, 20);
            this.txtOut.TabIndex = 12;
            // 
            // lblOut
            // 
            this.lblOut.AutoSize = true;
            this.lblOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOut.Location = new System.Drawing.Point(6, 54);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(87, 13);
            this.lblOut.TabIndex = 11;
            this.lblOut.Text = "Output Directory:";
            // 
            // btnBrowseUnpack
            // 
            this.btnBrowseUnpack.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseUnpack.Image")));
            this.btnBrowseUnpack.Location = new System.Drawing.Point(383, 20);
            this.btnBrowseUnpack.Name = "btnBrowseUnpack";
            this.btnBrowseUnpack.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseUnpack.TabIndex = 16;
            this.btnBrowseUnpack.UseVisualStyleBackColor = true;
            this.btnBrowseUnpack.Click += new System.EventHandler(this.btnBrowseUnpack_Click);
            // 
            // txtUnpack
            // 
            this.txtUnpack.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtUnpack.Location = new System.Drawing.Point(99, 22);
            this.txtUnpack.Name = "txtUnpack";
            this.txtUnpack.Size = new System.Drawing.Size(278, 20);
            this.txtUnpack.TabIndex = 15;
            // 
            // lblUnpack
            // 
            this.lblUnpack.AutoSize = true;
            this.lblUnpack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnpack.Location = new System.Drawing.Point(18, 25);
            this.lblUnpack.Name = "lblUnpack";
            this.lblUnpack.Size = new System.Drawing.Size(75, 13);
            this.lblUnpack.TabIndex = 14;
            this.lblUnpack.Text = "Kindleunpack:";
            // 
            // chkRaw
            // 
            this.chkRaw.AutoSize = true;
            this.chkRaw.Location = new System.Drawing.Point(9, 26);
            this.chkRaw.Name = "chkRaw";
            this.chkRaw.Size = new System.Drawing.Size(95, 17);
            this.chkRaw.TabIndex = 17;
            this.chkRaw.Text = "Save RAWML";
            this.chkRaw.UseVisualStyleBackColor = true;
            // 
            // chkSpoilers
            // 
            this.chkSpoilers.AutoSize = true;
            this.chkSpoilers.Location = new System.Drawing.Point(9, 49);
            this.chkSpoilers.Name = "chkSpoilers";
            this.chkSpoilers.Size = new System.Drawing.Size(63, 17);
            this.chkSpoilers.TabIndex = 18;
            this.chkSpoilers.Text = "Spoilers";
            this.chkSpoilers.UseVisualStyleBackColor = true;
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(288, 70);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(47, 20);
            this.txtOffset.TabIndex = 20;
            // 
            // lblOffset
            // 
            this.lblOffset.AutoSize = true;
            this.lblOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOffset.Location = new System.Drawing.Point(244, 73);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(38, 13);
            this.lblOffset.TabIndex = 19;
            this.lblOffset.Text = "Offset:";
            // 
            // chkSoftHyphen
            // 
            this.chkSoftHyphen.AutoSize = true;
            this.chkSoftHyphen.Location = new System.Drawing.Point(118, 26);
            this.chkSoftHyphen.Name = "chkSoftHyphen";
            this.chkSoftHyphen.Size = new System.Drawing.Size(123, 17);
            this.chkSoftHyphen.TabIndex = 21;
            this.chkSoftHyphen.Text = "Ignore Soft Hyphens";
            this.chkSoftHyphen.UseVisualStyleBackColor = true;
            // 
            // chkUseNew
            // 
            this.chkUseNew.AutoSize = true;
            this.chkUseNew.Location = new System.Drawing.Point(247, 26);
            this.chkUseNew.Name = "chkUseNew";
            this.chkUseNew.Size = new System.Drawing.Size(137, 17);
            this.chkUseNew.TabIndex = 22;
            this.chkUseNew.Text = "Use New X-Ray Format";
            this.chkUseNew.UseVisualStyleBackColor = true;
            // 
            // chkAndroid
            // 
            this.chkAndroid.AutoSize = true;
            this.chkAndroid.Enabled = false;
            this.chkAndroid.Location = new System.Drawing.Point(118, 49);
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
            this.gbDirectories.Size = new System.Drawing.Size(430, 86);
            this.gbDirectories.TabIndex = 24;
            this.gbDirectories.TabStop = false;
            this.gbDirectories.Text = "Directories";
            // 
            // chkSubDirectories
            // 
            this.chkSubDirectories.AutoSize = true;
            this.chkSubDirectories.Location = new System.Drawing.Point(9, 95);
            this.chkSubDirectories.Name = "chkSubDirectories";
            this.chkSubDirectories.Size = new System.Drawing.Size(113, 17);
            this.chkSubDirectories.TabIndex = 25;
            this.chkSubDirectories.Text = "Use subdirectories";
            this.chkSubDirectories.UseVisualStyleBackColor = true;
            // 
            // gbXray
            // 
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
            this.gbXray.Location = new System.Drawing.Point(12, 106);
            this.gbXray.Name = "gbXray";
            this.gbXray.Size = new System.Drawing.Size(430, 118);
            this.gbXray.TabIndex = 25;
            this.gbXray.TabStop = false;
            this.gbXray.Text = "X-ray Configuration";
            // 
            // chkUTF8
            // 
            this.chkUTF8.AutoSize = true;
            this.chkUTF8.Location = new System.Drawing.Point(247, 49);
            this.chkUTF8.Name = "chkUTF8";
            this.chkUTF8.Size = new System.Drawing.Size(99, 17);
            this.chkUTF8.TabIndex = 25;
            this.chkUTF8.Text = "Output in UTF8";
            this.chkUTF8.UseVisualStyleBackColor = true;
            // 
            // chkEnableEdit
            // 
            this.chkEnableEdit.AutoSize = true;
            this.chkEnableEdit.Location = new System.Drawing.Point(9, 72);
            this.chkEnableEdit.Name = "chkEnableEdit";
            this.chkEnableEdit.Size = new System.Drawing.Size(127, 17);
            this.chkEnableEdit.TabIndex = 24;
            this.chkEnableEdit.Text = "Edit Chapters/Aliases";
            this.chkEnableEdit.UseVisualStyleBackColor = true;
            // 
            // gbDetails
            // 
            this.gbDetails.Controls.Add(this.lblReal);
            this.gbDetails.Controls.Add(this.txtReal);
            this.gbDetails.Controls.Add(this.txtPen);
            this.gbDetails.Controls.Add(this.lblPen);
            this.gbDetails.Location = new System.Drawing.Point(12, 230);
            this.gbDetails.Name = "gbDetails";
            this.gbDetails.Size = new System.Drawing.Size(314, 86);
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
            this.txtReal.Size = new System.Drawing.Size(226, 20);
            this.txtReal.TabIndex = 18;
            // 
            // txtPen
            // 
            this.txtPen.Location = new System.Drawing.Point(75, 51);
            this.txtPen.Name = "txtPen";
            this.txtPen.Size = new System.Drawing.Size(226, 20);
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
            this.gbSite.Location = new System.Drawing.Point(332, 230);
            this.gbSite.Name = "gbSite";
            this.gbSite.Size = new System.Drawing.Size(110, 86);
            this.gbSite.TabIndex = 27;
            this.gbSite.TabStop = false;
            this.gbSite.Text = "Amazon Site";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblVersion.Location = new System.Drawing.Point(9, 327);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(115, 13);
            this.lblVersion.TabIndex = 29;
            this.lblVersion.Text = "X-ray Builder GUI vx.xx";
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 356);
            this.Controls.Add(this.lblVersion);
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
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.gbDirectories.ResumeLayout(false);
            this.gbDirectories.PerformLayout();
            this.gbXray.ResumeLayout(false);
            this.gbXray.PerformLayout();
            this.gbDetails.ResumeLayout(false);
            this.gbDetails.PerformLayout();
            this.gbSite.ResumeLayout(false);
            this.gbSite.PerformLayout();
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
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.CheckBox chkEnableEdit;
        private System.Windows.Forms.CheckBox chkSubDirectories;
        private System.Windows.Forms.CheckBox chkUTF8;
    }
}