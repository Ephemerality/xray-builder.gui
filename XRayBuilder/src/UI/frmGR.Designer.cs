namespace XRayBuilderGUI.UI
{
    partial class frmGR
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGR));
            this.cbResults = new System.Windows.Forms.ComboBox();
            this.lblMessage1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblMessage2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkID = new System.Windows.Forms.LinkLabel();
            this.lblEditions = new System.Windows.Forms.Label();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.pbCover = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.pbCover)).BeginInit();
            this.SuspendLayout();
            // 
            // cbResults
            // 
            this.cbResults.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbResults.FormattingEnabled = true;
            this.cbResults.IntegralHeight = false;
            this.cbResults.Location = new System.Drawing.Point(12, 12);
            this.cbResults.Name = "cbResults";
            this.cbResults.Size = new System.Drawing.Size(360, 21);
            this.cbResults.TabIndex = 3;
            this.cbResults.SelectedIndexChanged += new System.EventHandler(this.cbResults_SelectedIndexChanged);
            // 
            // lblMessage1
            // 
            this.lblMessage1.AutoEllipsis = true;
            this.lblMessage1.AutoSize = true;
            this.lblMessage1.Location = new System.Drawing.Point(9, 38);
            this.lblMessage1.Name = "lblMessage1";
            this.lblMessage1.Size = new System.Drawing.Size(252, 13);
            this.lblMessage1.TabIndex = 4;
            this.lblMessage1.Text = "?? matches for this book were found on Goodreads.";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(298, 58);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblMessage2
            // 
            this.lblMessage2.AutoEllipsis = true;
            this.lblMessage2.AutoSize = true;
            this.lblMessage2.Location = new System.Drawing.Point(9, 55);
            this.lblMessage2.Name = "lblMessage2";
            this.lblMessage2.Size = new System.Drawing.Size(173, 13);
            this.lblMessage2.TabIndex = 6;
            this.lblMessage2.Text = "Which book would you like to use?";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkID);
            this.groupBox1.Controls.Add(this.lblEditions);
            this.groupBox1.Controls.Add(this.lblRating);
            this.groupBox1.Controls.Add(this.lblTitle);
            this.groupBox1.Controls.Add(this.lblAuthor);
            this.groupBox1.Controls.Add(this.lblID);
            this.groupBox1.Controls.Add(this.pbCover);
            this.groupBox1.Location = new System.Drawing.Point(12, 86);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 104);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Book Details";
            // 
            // linkID
            // 
            this.linkID.ActiveLinkColor = System.Drawing.Color.MediumBlue;
            this.linkID.AutoSize = true;
            this.linkID.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkID.LinkColor = System.Drawing.Color.MediumBlue;
            this.linkID.Location = new System.Drawing.Point(142, 76);
            this.linkID.Name = "linkID";
            this.linkID.Size = new System.Drawing.Size(18, 13);
            this.linkID.TabIndex = 6;
            this.linkID.TabStop = true;
            this.linkID.Text = "ID";
            this.linkID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkID.VisitedLinkColor = System.Drawing.Color.MediumBlue;
            this.linkID.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkID_LinkClicked);
            // 
            // lblEditions
            // 
            this.lblEditions.AutoEllipsis = true;
            this.lblEditions.Location = new System.Drawing.Point(70, 62);
            this.lblEditions.Name = "lblEditions";
            this.lblEditions.Size = new System.Drawing.Size(284, 13);
            this.lblEditions.TabIndex = 5;
            this.lblEditions.Text = "Editions:";
            this.lblEditions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRating
            // 
            this.lblRating.AutoEllipsis = true;
            this.lblRating.Location = new System.Drawing.Point(70, 47);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(284, 13);
            this.lblRating.TabIndex = 4;
            this.lblRating.Text = "Rating:";
            this.lblRating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoEllipsis = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.lblTitle.Location = new System.Drawing.Point(70, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(284, 13);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "Title";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.UseMnemonic = false;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoEllipsis = true;
            this.lblAuthor.Location = new System.Drawing.Point(70, 34);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(284, 13);
            this.lblAuthor.TabIndex = 2;
            this.lblAuthor.Text = "Author";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblID
            // 
            this.lblID.AutoEllipsis = true;
            this.lblID.Location = new System.Drawing.Point(70, 76);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(76, 13);
            this.lblID.TabIndex = 1;
            this.lblID.Text = "Goodreads ID:";
            this.lblID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbCover
            // 
            this.pbCover.Image = global::XRayBuilderGUI.Properties.Resources.missing_image;
            this.pbCover.Location = new System.Drawing.Point(14, 20);
            this.pbCover.Name = "pbCover";
            this.pbCover.Size = new System.Drawing.Size(50, 70);
            this.pbCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCover.TabIndex = 0;
            this.pbCover.TabStop = false;
            // 
            // frmGR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 202);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblMessage2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMessage1);
            this.Controls.Add(this.cbResults);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmGR";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Goodread Matches";
            this.Load += new System.EventHandler(this.frmGR_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.pbCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.ComboBox cbResults;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label lblAuthor;
        public System.Windows.Forms.Label lblEditions;
        public System.Windows.Forms.Label lblID;
        public System.Windows.Forms.Label lblMessage1;
        public System.Windows.Forms.Label lblMessage2;
        public System.Windows.Forms.Label lblRating;
        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.LinkLabel linkID;
        public System.Windows.Forms.PictureBox pbCover;

        #endregion
    }
}