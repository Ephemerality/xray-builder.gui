namespace XRayBuilderGUI
{
    partial class frmPreviewSA
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblBookAuthor = new System.Windows.Forms.Label();
            this.lblBookTitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBookDesc = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblRead = new System.Windows.Forms.Label();
            this.lblAuthorBio = new System.Windows.Forms.Label();
            this.lblPages = new System.Windows.Forms.Label();
            this.lblAboutAuthor = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSeries = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.pbRating = new System.Windows.Forms.PictureBox();
            this.pbBackground = new System.Windows.Forms.PictureBox();
            this.pbAuthorImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(21, 17);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(256, 20);
            this.lblTitle.TabIndex = 86;
            this.lblTitle.Text = "About This Book";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBookAuthor
            // 
            this.lblBookAuthor.AutoEllipsis = true;
            this.lblBookAuthor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBookAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.lblBookAuthor.Location = new System.Drawing.Point(23, 68);
            this.lblBookAuthor.Name = "lblBookAuthor";
            this.lblBookAuthor.Size = new System.Drawing.Size(255, 14);
            this.lblBookAuthor.TabIndex = 89;
            this.lblBookAuthor.Text = "Author Name";
            this.lblBookAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBookTitle
            // 
            this.lblBookTitle.AutoEllipsis = true;
            this.lblBookTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBookTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.lblBookTitle.Location = new System.Drawing.Point(22, 52);
            this.lblBookTitle.Name = "lblBookTitle";
            this.lblBookTitle.Size = new System.Drawing.Size(255, 14);
            this.lblBookTitle.TabIndex = 88;
            this.lblBookTitle.Text = "Title (Series and Index)";
            this.lblBookTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Underline);
            this.label1.Location = new System.Drawing.Point(210, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 14);
            this.label1.TabIndex = 91;
            this.label1.Text = "See in store";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBookDesc
            // 
            this.lblBookDesc.AutoEllipsis = true;
            this.lblBookDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBookDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblBookDesc.Location = new System.Drawing.Point(22, 108);
            this.lblBookDesc.Name = "lblBookDesc";
            this.lblBookDesc.Size = new System.Drawing.Size(246, 40);
            this.lblBookDesc.TabIndex = 92;
            this.lblBookDesc.Text = "This information will be updated as soon as the Start Action file has been create" +
    "d for this book...";
            // 
            // label3
            // 
            this.label3.AutoEllipsis = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(22, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(255, 14);
            this.label3.TabIndex = 94;
            this.label3.Text = "Typical time to read";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRead
            // 
            this.lblRead.AutoEllipsis = true;
            this.lblRead.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblRead.Location = new System.Drawing.Point(22, 186);
            this.lblRead.Name = "lblRead";
            this.lblRead.Size = new System.Drawing.Size(160, 14);
            this.lblRead.TabIndex = 95;
            this.lblRead.Text = "88 hours and 88 minutes";
            this.lblRead.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAuthorBio
            // 
            this.lblAuthorBio.AutoEllipsis = true;
            this.lblAuthorBio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblAuthorBio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblAuthorBio.Location = new System.Drawing.Point(58, 355);
            this.lblAuthorBio.Name = "lblAuthorBio";
            this.lblAuthorBio.Size = new System.Drawing.Size(210, 54);
            this.lblAuthorBio.TabIndex = 96;
            this.lblAuthorBio.Text = "This information will be updated as soon as the Start Action file has been create" +
    "d for this book...";
            // 
            // lblPages
            // 
            this.lblPages.AutoEllipsis = true;
            this.lblPages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblPages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblPages.Location = new System.Drawing.Point(207, 186);
            this.lblPages.Name = "lblPages";
            this.lblPages.Size = new System.Drawing.Size(70, 14);
            this.lblPages.TabIndex = 97;
            this.lblPages.Text = "9999 pages";
            this.lblPages.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAboutAuthor
            // 
            this.lblAboutAuthor.AutoEllipsis = true;
            this.lblAboutAuthor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblAboutAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.lblAboutAuthor.Location = new System.Drawing.Point(58, 337);
            this.lblAboutAuthor.Name = "lblAboutAuthor";
            this.lblAboutAuthor.Size = new System.Drawing.Size(219, 14);
            this.lblAboutAuthor.TabIndex = 98;
            this.lblAboutAuthor.Text = "Author Name";
            this.lblAboutAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoEllipsis = true;
            this.label7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(22, 316);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(255, 14);
            this.label7.TabIndex = 99;
            this.label7.Text = "About the author";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSeries
            // 
            this.lblSeries.AutoEllipsis = true;
            this.lblSeries.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblSeries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblSeries.Location = new System.Drawing.Point(22, 286);
            this.lblSeries.Name = "lblSeries";
            this.lblSeries.Size = new System.Drawing.Size(256, 14);
            this.lblSeries.TabIndex = 101;
            this.lblSeries.Text = "This is book 88 of 88 in A Series Name Novel";
            this.lblSeries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoEllipsis = true;
            this.label9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(22, 266);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(256, 14);
            this.label9.TabIndex = 100;
            this.label9.Text = "About the series";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.AutoEllipsis = true;
            this.label10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Underline);
            this.label10.Location = new System.Drawing.Point(22, 236);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(160, 14);
            this.label10.TabIndex = 102;
            this.label10.Text = "Mark as Currently Reading";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbRating
            // 
            this.pbRating.Image = global::XRayBuilderGUI.Properties.Resources.STAR0;
            this.pbRating.Location = new System.Drawing.Point(25, 88);
            this.pbRating.Name = "pbRating";
            this.pbRating.Size = new System.Drawing.Size(44, 8);
            this.pbRating.TabIndex = 90;
            this.pbRating.TabStop = false;
            // 
            // pbBackground
            // 
            this.pbBackground.Image = global::XRayBuilderGUI.Properties.Resources.SA;
            this.pbBackground.Location = new System.Drawing.Point(13, 13);
            this.pbBackground.Name = "pbBackground";
            this.pbBackground.Size = new System.Drawing.Size(280, 400);
            this.pbBackground.TabIndex = 71;
            this.pbBackground.TabStop = false;
            // 
            // pbAuthorImage
            // 
            this.pbAuthorImage.Location = new System.Drawing.Point(25, 339);
            this.pbAuthorImage.Name = "pbAuthorImage";
            this.pbAuthorImage.Size = new System.Drawing.Size(30, 41);
            this.pbAuthorImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbAuthorImage.TabIndex = 103;
            this.pbAuthorImage.TabStop = false;
            // 
            // frmPreviewSA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 426);
            this.Controls.Add(this.pbAuthorImage);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lblSeries);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblAboutAuthor);
            this.Controls.Add(this.lblPages);
            this.Controls.Add(this.lblAuthorBio);
            this.Controls.Add(this.lblRead);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblBookDesc);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbRating);
            this.Controls.Add(this.lblBookAuthor);
            this.Controls.Add(this.lblBookTitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pbBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewSA";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Start Action Preview";
            ((System.ComponentModel.ISupportInitialize)(this.pbRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox pbBackground;
        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.Label lblBookAuthor;
        public System.Windows.Forms.Label lblBookTitle;
        public System.Windows.Forms.PictureBox pbRating;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label lblBookDesc;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label lblRead;
        public System.Windows.Forms.Label lblAuthorBio;
        public System.Windows.Forms.Label lblPages;
        public System.Windows.Forms.Label lblAboutAuthor;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label lblSeries;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.PictureBox pbAuthorImage;
    }
}