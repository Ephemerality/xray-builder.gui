namespace XRayBuilderGUI.UI
{
    partial class frmBookInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBookInfo));
            this.txtAuthorUrl = new System.Windows.Forms.TextBox();
            this.lblAuthorUrl = new System.Windows.Forms.Label();
            this.txtGoodreadsUrl = new System.Windows.Forms.TextBox();
            this.lblGoodreadsUrl = new System.Windows.Forms.Label();
            this.txtBookUrl = new System.Windows.Forms.TextBox();
            this.lblBookUrl = new System.Windows.Forms.Label();
            this.btnGoodreadsUrlLink = new System.Windows.Forms.Button();
            this.btnGoodreadsSearch = new System.Windows.Forms.Button();
            this.btnBookUrlSearch = new System.Windows.Forms.Button();
            this.btnBookUrlLink = new System.Windows.Forms.Button();
            this.btnAuthorUrlSearch = new System.Windows.Forms.Button();
            this.btnAuthorUrlLink = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblBio = new System.Windows.Forms.Label();
            this.txtBio = new System.Windows.Forms.TextBox();
            this.pbCover = new System.Windows.Forms.PictureBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtXMLFile = new System.Windows.Forms.TextBox();
            this.cbXraySource = new System.Windows.Forms.ComboBox();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtAuthorUrl
            // 
            this.txtAuthorUrl.Location = new System.Drawing.Point(439, 17);
            this.txtAuthorUrl.Name = "txtAuthorUrl";
            this.txtAuthorUrl.Size = new System.Drawing.Size(370, 27);
            this.txtAuthorUrl.TabIndex = 0;
            this.txtAuthorUrl.TextChanged += new System.EventHandler(this.txtAuthorUrl_TextChanged);
            // 
            // lblAuthorUrl
            // 
            this.lblAuthorUrl.AutoSize = true;
            this.lblAuthorUrl.Location = new System.Drawing.Point(341, 20);
            this.lblAuthorUrl.Name = "lblAuthorUrl";
            this.lblAuthorUrl.Size = new System.Drawing.Size(87, 20);
            this.lblAuthorUrl.TabIndex = 0;
            this.lblAuthorUrl.Text = "Author URL:";
            this.lblAuthorUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtGoodreadsUrl
            // 
            this.txtGoodreadsUrl.Location = new System.Drawing.Point(439, 287);
            this.txtGoodreadsUrl.Name = "txtGoodreadsUrl";
            this.txtGoodreadsUrl.Size = new System.Drawing.Size(370, 27);
            this.txtGoodreadsUrl.TabIndex = 6;
            // 
            // lblGoodreadsUrl
            // 
            this.lblGoodreadsUrl.AutoSize = true;
            this.lblGoodreadsUrl.Location = new System.Drawing.Point(313, 290);
            this.lblGoodreadsUrl.Name = "lblGoodreadsUrl";
            this.lblGoodreadsUrl.Size = new System.Drawing.Size(115, 20);
            this.lblGoodreadsUrl.TabIndex = 0;
            this.lblGoodreadsUrl.Text = "Goodreads URL:";
            this.lblGoodreadsUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBookUrl
            // 
            this.txtBookUrl.Location = new System.Drawing.Point(439, 243);
            this.txtBookUrl.Name = "txtBookUrl";
            this.txtBookUrl.Size = new System.Drawing.Size(370, 27);
            this.txtBookUrl.TabIndex = 3;
            // 
            // lblBookUrl
            // 
            this.lblBookUrl.AutoSize = true;
            this.lblBookUrl.Location = new System.Drawing.Point(352, 246);
            this.lblBookUrl.Name = "lblBookUrl";
            this.lblBookUrl.Size = new System.Drawing.Size(76, 20);
            this.lblBookUrl.TabIndex = 0;
            this.lblBookUrl.Text = "Book URL:";
            this.lblBookUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnGoodreadsUrlLink
            // 
            this.btnGoodreadsUrlLink.Image = ((System.Drawing.Image)(resources.GetObject("btnGoodreadsUrlLink.Image")));
            this.btnGoodreadsUrlLink.Location = new System.Drawing.Point(825, 286);
            this.btnGoodreadsUrlLink.Name = "btnGoodreadsUrlLink";
            this.btnGoodreadsUrlLink.Size = new System.Drawing.Size(29, 29);
            this.btnGoodreadsUrlLink.TabIndex = 7;
            this.btnGoodreadsUrlLink.UseVisualStyleBackColor = true;
            this.btnGoodreadsUrlLink.Click += new System.EventHandler(this.btnGoodreadsUrlLink_Click);
            // 
            // btnGoodreadsSearch
            // 
            this.btnGoodreadsSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnGoodreadsSearch.Image")));
            this.btnGoodreadsSearch.Location = new System.Drawing.Point(869, 286);
            this.btnGoodreadsSearch.Name = "btnGoodreadsSearch";
            this.btnGoodreadsSearch.Size = new System.Drawing.Size(29, 29);
            this.btnGoodreadsSearch.TabIndex = 8;
            this.btnGoodreadsSearch.UseVisualStyleBackColor = true;
            this.btnGoodreadsSearch.Click += new System.EventHandler(this.btnGoodreadsSearch_Click);
            // 
            // btnBookUrlSearch
            // 
            this.btnBookUrlSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnBookUrlSearch.Image")));
            this.btnBookUrlSearch.Location = new System.Drawing.Point(869, 242);
            this.btnBookUrlSearch.Name = "btnBookUrlSearch";
            this.btnBookUrlSearch.Size = new System.Drawing.Size(29, 29);
            this.btnBookUrlSearch.TabIndex = 5;
            this.btnBookUrlSearch.UseVisualStyleBackColor = true;
            // 
            // btnBookUrlLink
            // 
            this.btnBookUrlLink.Image = ((System.Drawing.Image)(resources.GetObject("btnBookUrlLink.Image")));
            this.btnBookUrlLink.Location = new System.Drawing.Point(825, 242);
            this.btnBookUrlLink.Name = "btnBookUrlLink";
            this.btnBookUrlLink.Size = new System.Drawing.Size(29, 29);
            this.btnBookUrlLink.TabIndex = 4;
            this.btnBookUrlLink.UseVisualStyleBackColor = true;
            this.btnBookUrlLink.Click += new System.EventHandler(this.btnBookUrlLink_Click);
            // 
            // btnAuthorUrlSearch
            // 
            this.btnAuthorUrlSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnAuthorUrlSearch.Image")));
            this.btnAuthorUrlSearch.Location = new System.Drawing.Point(869, 16);
            this.btnAuthorUrlSearch.Name = "btnAuthorUrlSearch";
            this.btnAuthorUrlSearch.Size = new System.Drawing.Size(29, 29);
            this.btnAuthorUrlSearch.TabIndex = 2;
            this.btnAuthorUrlSearch.UseVisualStyleBackColor = true;
            this.btnAuthorUrlSearch.Click += new System.EventHandler(this.btnAuthorUrlSearch_Click);
            // 
            // btnAuthorUrlLink
            // 
            this.btnAuthorUrlLink.Image = ((System.Drawing.Image)(resources.GetObject("btnAuthorUrlLink.Image")));
            this.btnAuthorUrlLink.Location = new System.Drawing.Point(825, 16);
            this.btnAuthorUrlLink.Name = "btnAuthorUrlLink";
            this.btnAuthorUrlLink.Size = new System.Drawing.Size(29, 29);
            this.btnAuthorUrlLink.TabIndex = 1;
            this.btnAuthorUrlLink.UseVisualStyleBackColor = true;
            this.btnAuthorUrlLink.Click += new System.EventHandler(this.btnAuthorUrlLink_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(797, 411);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 32);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(682, 411);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 459);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(914, 26);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(426, 20);
            this.StatusLabel.Text = "Use the search or browse buttons to update book information...";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBio
            // 
            this.lblBio.AutoSize = true;
            this.lblBio.Location = new System.Drawing.Point(348, 65);
            this.lblBio.Name = "lblBio";
            this.lblBio.Size = new System.Drawing.Size(80, 20);
            this.lblBio.TabIndex = 11;
            this.lblBio.Text = "Biography:";
            this.lblBio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBio
            // 
            this.txtBio.Location = new System.Drawing.Point(439, 62);
            this.txtBio.Multiline = true;
            this.txtBio.Name = "txtBio";
            this.txtBio.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBio.Size = new System.Drawing.Size(370, 164);
            this.txtBio.TabIndex = 12;
            // 
            // pbCover
            // 
            this.pbCover.Image = global::XRayBuilderGUI.Properties.Resources.missing_cover;
            this.pbCover.Location = new System.Drawing.Point(17, 17);
            this.pbCover.Name = "pbCover";
            this.pbCover.Size = new System.Drawing.Size(283, 425);
            this.pbCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCover.TabIndex = 0;
            this.pbCover.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.cbXraySource);
            this.groupBox4.Controls.Add(this.txtXMLFile);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(317, 323);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(580, 75);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "X-Ray Terms Source";
            // 
            // txtXMLFile
            // 
            this.txtXMLFile.Enabled = false;
            this.txtXMLFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtXMLFile.Location = new System.Drawing.Point(155, 29);
            this.txtXMLFile.Name = "txtXMLFile";
            this.txtXMLFile.Size = new System.Drawing.Size(407, 27);
            this.txtXMLFile.TabIndex = 0;
            // 
            // cbXraySource
            // 
            this.cbXraySource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbXraySource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbXraySource.FormattingEnabled = true;
            this.cbXraySource.Items.AddRange(new object[] {
            "Goodreads",
            "Roentgen",
            "XML"});
            this.cbXraySource.Location = new System.Drawing.Point(18, 28);
            this.cbXraySource.Name = "cbXraySource";
            this.cbXraySource.Size = new System.Drawing.Size(120, 28);
            this.cbXraySource.TabIndex = 1;
            this.cbXraySource.SelectedIndexChanged += new System.EventHandler(this.cbXraySource_SelectedIndexChanged);
            // 
            // frmBookInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(914, 485);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.txtBio);
            this.Controls.Add(this.lblBio);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnAuthorUrlSearch);
            this.Controls.Add(this.btnAuthorUrlLink);
            this.Controls.Add(this.btnBookUrlSearch);
            this.Controls.Add(this.btnBookUrlLink);
            this.Controls.Add(this.btnGoodreadsSearch);
            this.Controls.Add(this.btnGoodreadsUrlLink);
            this.Controls.Add(this.txtBookUrl);
            this.Controls.Add(this.lblBookUrl);
            this.Controls.Add(this.txtGoodreadsUrl);
            this.Controls.Add(this.lblGoodreadsUrl);
            this.Controls.Add(this.txtAuthorUrl);
            this.Controls.Add(this.lblAuthorUrl);
            this.Controls.Add(this.pbCover);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmBookInfo";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Title by Author";
            this.Load += new System.EventHandler(this.frmBookInfo_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtAuthorUrl;
        private System.Windows.Forms.Label lblAuthorUrl;
        private System.Windows.Forms.TextBox txtGoodreadsUrl;
        private System.Windows.Forms.Label lblGoodreadsUrl;
        private System.Windows.Forms.TextBox txtBookUrl;
        private System.Windows.Forms.Label lblBookUrl;
        private System.Windows.Forms.Button btnGoodreadsUrlLink;
        private System.Windows.Forms.Button btnGoodreadsSearch;
        private System.Windows.Forms.Button btnBookUrlSearch;
        private System.Windows.Forms.Button btnBookUrlLink;
        private System.Windows.Forms.Button btnAuthorUrlSearch;
        private System.Windows.Forms.Button btnAuthorUrlLink;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Label lblBio;
        private System.Windows.Forms.TextBox txtBio;
        private System.Windows.Forms.PictureBox pbCover;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtXMLFile;
        private System.Windows.Forms.ComboBox cbXraySource;
    }
}