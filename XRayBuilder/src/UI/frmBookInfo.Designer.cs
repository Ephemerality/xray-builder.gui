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
            this.txtDataProviderUrl = new System.Windows.Forms.TextBox();
            this.lblDataProviderUrl = new System.Windows.Forms.Label();
            this.txtBookUrl = new System.Windows.Forms.TextBox();
            this.lblBookUrl = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblBio = new System.Windows.Forms.Label();
            this.txtBio = new System.Windows.Forms.TextBox();
            this.btnAuthorUrlSearch = new System.Windows.Forms.Button();
            this.btnAuthorUrlLink = new System.Windows.Forms.Button();
            this.btnBookUrlSearch = new System.Windows.Forms.Button();
            this.btnBookUrlLink = new System.Windows.Forms.Button();
            this.btnDataProviderSearch = new System.Windows.Forms.Button();
            this.btnDataProviderUrlLink = new System.Windows.Forms.Button();
            this.pbCover = new System.Windows.Forms.PictureBox();
            this.cmbSecondaryDataSource = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).BeginInit();
            this.SuspendLayout();
            // 
            // txtAuthorUrl
            // 
            this.txtAuthorUrl.Location = new System.Drawing.Point(317, 12);
            this.txtAuthorUrl.Name = "txtAuthorUrl";
            this.txtAuthorUrl.Size = new System.Drawing.Size(335, 23);
            this.txtAuthorUrl.TabIndex = 0;
            this.txtAuthorUrl.TextChanged += new System.EventHandler(this.txtAuthorUrl_TextChanged);
            // 
            // lblAuthorUrl
            // 
            this.lblAuthorUrl.AutoSize = true;
            this.lblAuthorUrl.Location = new System.Drawing.Point(239, 15);
            this.lblAuthorUrl.Name = "lblAuthorUrl";
            this.lblAuthorUrl.Size = new System.Drawing.Size(71, 15);
            this.lblAuthorUrl.TabIndex = 0;
            this.lblAuthorUrl.Text = "Author URL:";
            this.lblAuthorUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDataProviderUrl
            // 
            this.txtDataProviderUrl.Location = new System.Drawing.Point(317, 274);
            this.txtDataProviderUrl.Name = "txtDataProviderUrl";
            this.txtDataProviderUrl.Size = new System.Drawing.Size(335, 23);
            this.txtDataProviderUrl.TabIndex = 6;
            // 
            // lblDataProviderUrl
            // 
            this.lblDataProviderUrl.AutoSize = true;
            this.lblDataProviderUrl.Location = new System.Drawing.Point(205, 277);
            this.lblDataProviderUrl.Name = "lblDataProviderUrl";
            this.lblDataProviderUrl.Size = new System.Drawing.Size(105, 15);
            this.lblDataProviderUrl.TabIndex = 0;
            this.lblDataProviderUrl.Text = "Data Provider URL:";
            this.lblDataProviderUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBookUrl
            // 
            this.txtBookUrl.Location = new System.Drawing.Point(317, 239);
            this.txtBookUrl.Name = "txtBookUrl";
            this.txtBookUrl.Size = new System.Drawing.Size(335, 23);
            this.txtBookUrl.TabIndex = 3;
            // 
            // lblBookUrl
            // 
            this.lblBookUrl.AutoSize = true;
            this.lblBookUrl.Location = new System.Drawing.Point(249, 242);
            this.lblBookUrl.Name = "lblBookUrl";
            this.lblBookUrl.Size = new System.Drawing.Size(61, 15);
            this.lblBookUrl.TabIndex = 0;
            this.lblBookUrl.Text = "Book URL:";
            this.lblBookUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(578, 308);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(491, 308);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 342);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(734, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StatusLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(286, 17);
            this.StatusLabel.Text = "Use the search buttons to update book information...";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBio
            // 
            this.lblBio.AutoSize = true;
            this.lblBio.Location = new System.Drawing.Point(246, 50);
            this.lblBio.Name = "lblBio";
            this.lblBio.Size = new System.Drawing.Size(64, 15);
            this.lblBio.TabIndex = 11;
            this.lblBio.Text = "Biography:";
            this.lblBio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBio
            // 
            this.txtBio.Location = new System.Drawing.Point(317, 47);
            this.txtBio.Multiline = true;
            this.txtBio.Name = "txtBio";
            this.txtBio.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBio.Size = new System.Drawing.Size(335, 180);
            this.txtBio.TabIndex = 12;
            // 
            // btnAuthorUrlSearch
            // 
            this.btnAuthorUrlSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnAuthorUrlSearch.Image")));
            this.btnAuthorUrlSearch.Location = new System.Drawing.Point(698, 11);
            this.btnAuthorUrlSearch.Name = "btnAuthorUrlSearch";
            this.btnAuthorUrlSearch.Size = new System.Drawing.Size(25, 25);
            this.btnAuthorUrlSearch.TabIndex = 2;
            this.btnAuthorUrlSearch.UseVisualStyleBackColor = true;
            this.btnAuthorUrlSearch.Click += new System.EventHandler(this.btnAuthorUrlSearch_Click);
            // 
            // btnAuthorUrlLink
            // 
            this.btnAuthorUrlLink.Image = ((System.Drawing.Image)(resources.GetObject("btnAuthorUrlLink.Image")));
            this.btnAuthorUrlLink.Location = new System.Drawing.Point(663, 11);
            this.btnAuthorUrlLink.Name = "btnAuthorUrlLink";
            this.btnAuthorUrlLink.Size = new System.Drawing.Size(25, 25);
            this.btnAuthorUrlLink.TabIndex = 1;
            this.btnAuthorUrlLink.UseVisualStyleBackColor = true;
            this.btnAuthorUrlLink.Click += new System.EventHandler(this.btnAuthorUrlLink_Click);
            // 
            // btnBookUrlSearch
            // 
            this.btnBookUrlSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnBookUrlSearch.Image")));
            this.btnBookUrlSearch.Location = new System.Drawing.Point(698, 238);
            this.btnBookUrlSearch.Name = "btnBookUrlSearch";
            this.btnBookUrlSearch.Size = new System.Drawing.Size(25, 25);
            this.btnBookUrlSearch.TabIndex = 5;
            this.btnBookUrlSearch.UseVisualStyleBackColor = true;
            this.btnBookUrlSearch.Click += new System.EventHandler(this.btnBookUrlSearch_Click);
            // 
            // btnBookUrlLink
            // 
            this.btnBookUrlLink.Image = ((System.Drawing.Image)(resources.GetObject("btnBookUrlLink.Image")));
            this.btnBookUrlLink.Location = new System.Drawing.Point(663, 238);
            this.btnBookUrlLink.Name = "btnBookUrlLink";
            this.btnBookUrlLink.Size = new System.Drawing.Size(25, 25);
            this.btnBookUrlLink.TabIndex = 4;
            this.btnBookUrlLink.UseVisualStyleBackColor = true;
            this.btnBookUrlLink.Click += new System.EventHandler(this.btnBookUrlLink_Click);
            // 
            // btnDataProviderSearch
            // 
            this.btnDataProviderSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnDataProviderSearch.Image")));
            this.btnDataProviderSearch.Location = new System.Drawing.Point(698, 273);
            this.btnDataProviderSearch.Name = "btnDataProviderSearch";
            this.btnDataProviderSearch.Size = new System.Drawing.Size(25, 25);
            this.btnDataProviderSearch.TabIndex = 8;
            this.btnDataProviderSearch.UseVisualStyleBackColor = true;
            this.btnDataProviderSearch.Click += new System.EventHandler(this.btnDataProviderSearch_Click);
            // 
            // btnDataProviderUrlLink
            // 
            this.btnDataProviderUrlLink.Image = ((System.Drawing.Image)(resources.GetObject("btnDataProviderUrlLink.Image")));
            this.btnDataProviderUrlLink.Location = new System.Drawing.Point(663, 273);
            this.btnDataProviderUrlLink.Name = "btnDataProviderUrlLink";
            this.btnDataProviderUrlLink.Size = new System.Drawing.Size(25, 25);
            this.btnDataProviderUrlLink.TabIndex = 7;
            this.btnDataProviderUrlLink.UseVisualStyleBackColor = true;
            this.btnDataProviderUrlLink.Click += new System.EventHandler(this.btnDataProviderUrlLink_Click);
            // 
            // pbCover
            // 
            this.pbCover.Image = global::XRayBuilderGUI.Properties.Resources.missing_cover_medium;
            this.pbCover.Location = new System.Drawing.Point(12, 12);
            this.pbCover.Name = "pbCover";
            this.pbCover.Size = new System.Drawing.Size(190, 285);
            this.pbCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCover.TabIndex = 0;
            this.pbCover.TabStop = false;
            // 
            // cmbSecondaryDataSource
            // 
            this.cmbSecondaryDataSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSecondaryDataSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbSecondaryDataSource.FormattingEnabled = true;
            this.cmbSecondaryDataSource.Items.AddRange(new object[] {
            "Goodreads",
            "Shelfari",
            "LibraryThing"});
            this.cmbSecondaryDataSource.Location = new System.Drawing.Point(317, 307);
            this.cmbSecondaryDataSource.Name = "cmbSecondaryDataSource";
            this.cmbSecondaryDataSource.Size = new System.Drawing.Size(138, 23);
            this.cmbSecondaryDataSource.TabIndex = 13;
            this.cmbSecondaryDataSource.SelectedIndexChanged += new System.EventHandler(this.cmbSecondaryDataSource_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(229, 312);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 15);
            this.label1.TabIndex = 14;
            this.label1.Text = "Data Provider:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmBookInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(734, 364);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbSecondaryDataSource);
            this.Controls.Add(this.txtBio);
            this.Controls.Add(this.lblBio);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnAuthorUrlSearch);
            this.Controls.Add(this.btnAuthorUrlLink);
            this.Controls.Add(this.btnBookUrlSearch);
            this.Controls.Add(this.btnBookUrlLink);
            this.Controls.Add(this.btnDataProviderSearch);
            this.Controls.Add(this.btnDataProviderUrlLink);
            this.Controls.Add(this.txtBookUrl);
            this.Controls.Add(this.lblBookUrl);
            this.Controls.Add(this.txtDataProviderUrl);
            this.Controls.Add(this.lblDataProviderUrl);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtAuthorUrl;
        private System.Windows.Forms.Label lblAuthorUrl;
        private System.Windows.Forms.TextBox txtDataProviderUrl;
        private System.Windows.Forms.Label lblDataProviderUrl;
        private System.Windows.Forms.TextBox txtBookUrl;
        private System.Windows.Forms.Label lblBookUrl;
        private System.Windows.Forms.Button btnDataProviderUrlLink;
        private System.Windows.Forms.Button btnDataProviderSearch;
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
        private System.Windows.Forms.ComboBox cmbSecondaryDataSource;
        private System.Windows.Forms.Label label1;
    }
}