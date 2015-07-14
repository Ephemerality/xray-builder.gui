using System.Drawing;

namespace XRayBuilderGUI
{
    partial class FrmPreviewAp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPreviewAp));
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblBio2 = new System.Windows.Forms.Label();
            this.lblBio1 = new System.Windows.Forms.Label();
            this.lblBook4 = new System.Windows.Forms.Label();
            this.lblBook3 = new System.Windows.Forms.Label();
            this.lblBook2 = new System.Windows.Forms.Label();
            this.lblBook1 = new System.Windows.Forms.Label();
            this.lblKindleBooks = new System.Windows.Forms.Label();
            this.pbBackground = new System.Windows.Forms.PictureBox();
            this.pbAuthorImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoEllipsis = true;
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(23, 17);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(260, 20);
            this.lblTitle.TabIndex = 68;
            this.lblTitle.Text = "About the Author ...Waiting...";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBio2
            // 
            this.lblBio2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBio2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblBio2.ForeColor = System.Drawing.Color.Black;
            this.lblBio2.Location = new System.Drawing.Point(17, 179);
            this.lblBio2.Name = "lblBio2";
            this.lblBio2.Size = new System.Drawing.Size(266, 70);
            this.lblBio2.TabIndex = 77;
            // 
            // lblBio1
            // 
            this.lblBio1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBio1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblBio1.ForeColor = System.Drawing.Color.Black;
            this.lblBio1.Location = new System.Drawing.Point(116, 49);
            this.lblBio1.Name = "lblBio1";
            this.lblBio1.Size = new System.Drawing.Size(168, 130);
            this.lblBio1.TabIndex = 76;
            this.lblBio1.Text = resources.GetString("lblBio1.Text");
            // 
            // lblBook4
            // 
            this.lblBook4.AutoEllipsis = true;
            this.lblBook4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBook4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBook4.Location = new System.Drawing.Point(22, 379);
            this.lblBook4.Name = "lblBook4";
            this.lblBook4.Size = new System.Drawing.Size(245, 15);
            this.lblBook4.TabIndex = 74;
            this.lblBook4.Text = "Book 4 ...Waiting...";
            this.lblBook4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBook4.UseMnemonic = false;
            // 
            // lblBook3
            // 
            this.lblBook3.AutoEllipsis = true;
            this.lblBook3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBook3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBook3.Location = new System.Drawing.Point(22, 346);
            this.lblBook3.Name = "lblBook3";
            this.lblBook3.Size = new System.Drawing.Size(245, 15);
            this.lblBook3.TabIndex = 73;
            this.lblBook3.Text = "Book 3 ...Waiting...";
            this.lblBook3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBook3.UseMnemonic = false;
            // 
            // lblBook2
            // 
            this.lblBook2.AutoEllipsis = true;
            this.lblBook2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBook2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBook2.Location = new System.Drawing.Point(22, 313);
            this.lblBook2.Name = "lblBook2";
            this.lblBook2.Size = new System.Drawing.Size(245, 15);
            this.lblBook2.TabIndex = 72;
            this.lblBook2.Text = "Book 2 ...Waiting...";
            this.lblBook2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBook2.UseMnemonic = false;
            // 
            // lblBook1
            // 
            this.lblBook1.AutoEllipsis = true;
            this.lblBook1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.lblBook1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBook1.Location = new System.Drawing.Point(22, 280);
            this.lblBook1.Name = "lblBook1";
            this.lblBook1.Size = new System.Drawing.Size(245, 15);
            this.lblBook1.TabIndex = 71;
            this.lblBook1.Text = "Book 1 ...Waiting...";
            this.lblBook1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBook1.UseMnemonic = false;
            // 
            // lblKindleBooks
            // 
            this.lblKindleBooks.AutoEllipsis = true;
            this.lblKindleBooks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.lblKindleBooks.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKindleBooks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.lblKindleBooks.Location = new System.Drawing.Point(23, 253);
            this.lblKindleBooks.Name = "lblKindleBooks";
            this.lblKindleBooks.Size = new System.Drawing.Size(250, 20);
            this.lblKindleBooks.TabIndex = 69;
            this.lblKindleBooks.Text = "Kindle books ...Waiting...";
            this.lblKindleBooks.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbBackground
            // 
            this.pbBackground.Image = global::XRayBuilderGUI.Properties.Resources.AP;
            this.pbBackground.Location = new System.Drawing.Point(13, 13);
            this.pbBackground.Name = "pbBackground";
            this.pbBackground.Size = new System.Drawing.Size(280, 400);
            this.pbBackground.TabIndex = 70;
            this.pbBackground.TabStop = false;
            // 
            // pbAuthorImage
            // 
            this.pbAuthorImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.pbAuthorImage.Image = global::XRayBuilderGUI.Properties.Resources.AI;
            this.pbAuthorImage.Location = new System.Drawing.Point(17, 48);
            this.pbAuthorImage.Name = "pbAuthorImage";
            this.pbAuthorImage.Size = new System.Drawing.Size(95, 130);
            this.pbAuthorImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbAuthorImage.TabIndex = 75;
            this.pbAuthorImage.TabStop = false;
            // 
            // FrmPreviewAp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.ClientSize = new System.Drawing.Size(306, 426);
            this.Controls.Add(this.pbAuthorImage);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblBio2);
            this.Controls.Add(this.lblBio1);
            this.Controls.Add(this.lblBook4);
            this.Controls.Add(this.lblBook3);
            this.Controls.Add(this.lblBook2);
            this.Controls.Add(this.lblBook1);
            this.Controls.Add(this.lblKindleBooks);
            this.Controls.Add(this.pbBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPreviewAp";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Author Profile Preview";
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.Label lblBio2;
        public System.Windows.Forms.Label lblBio1;
        public System.Windows.Forms.Label lblBook4;
        public System.Windows.Forms.Label lblBook3;
        public System.Windows.Forms.Label lblBook2;
        public System.Windows.Forms.Label lblBook1;
        public System.Windows.Forms.Label lblKindleBooks;
        public System.Windows.Forms.PictureBox pbBackground;
        public System.Windows.Forms.PictureBox pbAuthorImage;
    }
}