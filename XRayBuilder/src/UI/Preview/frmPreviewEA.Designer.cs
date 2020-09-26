namespace XRayBuilderGUI.UI.Preview
{
    partial class frmPreviewEA
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPreviewEA));
            this.ilcustomersWhoBoughtRecs = new System.Windows.Forms.ImageList(this.components);
            this.ilauthorRecs = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnFollow = new System.Windows.Forms.Button();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.pbAuthor = new System.Windows.Forms.PictureBox();
            this.lblFollowAuthor = new System.Windows.Forms.Label();
            this.pbSeperator1 = new System.Windows.Forms.PictureBox();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.pbRating = new System.Windows.Forms.PictureBox();
            this.lblYourRating = new System.Windows.Forms.Label();
            this.lvAuthorRecs = new System.Windows.Forms.ListView();
            this.lvCustomersWhoBoughtRecs = new System.Windows.Forms.ListView();
            this.lblCustomersWhoBoughtRecs = new System.Windows.Forms.Label();
            this.lblAuthorRecs = new System.Windows.Forms.Label();
            this.pbNextCover = new System.Windows.Forms.PictureBox();
            this.lblNextInSeries = new System.Windows.Forms.Label();
            this.txtNextInSeries = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSeperator1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNextCover)).BeginInit();
            this.SuspendLayout();
            // 
            // ilcustomersWhoBoughtRecs
            // 
            this.ilcustomersWhoBoughtRecs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilcustomersWhoBoughtRecs.ImageSize = new System.Drawing.Size(60, 90);
            this.ilcustomersWhoBoughtRecs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ilauthorRecs
            // 
            this.ilauthorRecs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilauthorRecs.ImageSize = new System.Drawing.Size(60, 90);
            this.ilauthorRecs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.pictureBox2.InitialImage = null;
            this.pictureBox2.Location = new System.Drawing.Point(413, 215);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(379, 1);
            this.pictureBox2.TabIndex = 84;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(17, 215);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(379, 1);
            this.pictureBox1.TabIndex = 83;
            this.pictureBox1.TabStop = false;
            // 
            // btnFollow
            // 
            this.btnFollow.Image = ((System.Drawing.Image)(resources.GetObject("btnFollow.Image")));
            this.btnFollow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFollow.Location = new System.Drawing.Point(313, 139);
            this.btnFollow.Name = "btnFollow";
            this.btnFollow.Size = new System.Drawing.Size(83, 30);
            this.btnFollow.TabIndex = 82;
            this.btnFollow.Text = "Follow ";
            this.btnFollow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnFollow.UseVisualStyleBackColor = true;
            this.btnFollow.Click += new System.EventHandler(this.btnFollow_Click);
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoEllipsis = true;
            this.lblAuthor.Location = new System.Drawing.Point(90, 144);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(201, 20);
            this.lblAuthor.TabIndex = 81;
            this.lblAuthor.Text = "Author";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbAuthor
            // 
            this.pbAuthor.Image = ((System.Drawing.Image)(resources.GetObject("pbAuthor.Image")));
            this.pbAuthor.Location = new System.Drawing.Point(17, 111);
            this.pbAuthor.Name = "pbAuthor";
            this.pbAuthor.Size = new System.Drawing.Size(60, 90);
            this.pbAuthor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAuthor.TabIndex = 80;
            this.pbAuthor.TabStop = false;
            // 
            // lblFollowAuthor
            // 
            this.lblFollowAuthor.AutoSize = true;
            this.lblFollowAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFollowAuthor.Location = new System.Drawing.Point(13, 78);
            this.lblFollowAuthor.Name = "lblFollowAuthor";
            this.lblFollowAuthor.Size = new System.Drawing.Size(136, 20);
            this.lblFollowAuthor.TabIndex = 79;
            this.lblFollowAuthor.Text = "Follow the Author";
            this.lblFollowAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbSeperator1
            // 
            this.pbSeperator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.pbSeperator1.InitialImage = null;
            this.pbSeperator1.Location = new System.Drawing.Point(17, 65);
            this.pbSeperator1.Name = "pbSeperator1";
            this.pbSeperator1.Size = new System.Drawing.Size(379, 1);
            this.pbSeperator1.TabIndex = 78;
            this.pbSeperator1.TabStop = false;
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoEllipsis = true;
            this.lblUpdate.Location = new System.Drawing.Point(13, 35);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(384, 20);
            this.lblUpdate.TabIndex = 77;
            this.lblUpdate.Text = "Update on Amazon (as Pen Name)";
            this.lblUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbRating
            // 
            this.pbRating.Image = global::XRayBuilderGUI.Properties.Resources.STAR0;
            this.pbRating.Location = new System.Drawing.Point(192, 15);
            this.pbRating.Name = "pbRating";
            this.pbRating.Size = new System.Drawing.Size(80, 16);
            this.pbRating.TabIndex = 76;
            this.pbRating.TabStop = false;
            // 
            // lblYourRating
            // 
            this.lblYourRating.AutoSize = true;
            this.lblYourRating.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblYourRating.Location = new System.Drawing.Point(13, 12);
            this.lblYourRating.Name = "lblYourRating";
            this.lblYourRating.Size = new System.Drawing.Size(175, 20);
            this.lblYourRating.TabIndex = 75;
            this.lblYourRating.Text = "Your Rating on Amazon";
            this.lblYourRating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvAuthorRecs
            // 
            this.lvAuthorRecs.BackColor = System.Drawing.SystemColors.Control;
            this.lvAuthorRecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvAuthorRecs.HideSelection = false;
            this.lvAuthorRecs.LargeImageList = this.ilauthorRecs;
            this.lvAuthorRecs.Location = new System.Drawing.Point(405, 259);
            this.lvAuthorRecs.Name = "lvAuthorRecs";
            this.lvAuthorRecs.Size = new System.Drawing.Size(388, 206);
            this.lvAuthorRecs.TabIndex = 73;
            this.lvAuthorRecs.UseCompatibleStateImageBehavior = false;
            this.lvAuthorRecs.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvAuthorRecs_ItemSelectionChanged);
            // 
            // lvCustomersWhoBoughtRecs
            // 
            this.lvCustomersWhoBoughtRecs.BackColor = System.Drawing.SystemColors.Control;
            this.lvCustomersWhoBoughtRecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvCustomersWhoBoughtRecs.HideSelection = false;
            this.lvCustomersWhoBoughtRecs.LargeImageList = this.ilcustomersWhoBoughtRecs;
            this.lvCustomersWhoBoughtRecs.Location = new System.Drawing.Point(9, 259);
            this.lvCustomersWhoBoughtRecs.Name = "lvCustomersWhoBoughtRecs";
            this.lvCustomersWhoBoughtRecs.Size = new System.Drawing.Size(388, 206);
            this.lvCustomersWhoBoughtRecs.TabIndex = 72;
            this.lvCustomersWhoBoughtRecs.UseCompatibleStateImageBehavior = false;
            this.lvCustomersWhoBoughtRecs.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvCustomersWhoBoughtRecs_ItemSelectionChanged);
            // 
            // lblCustomersWhoBoughtRecs
            // 
            this.lblCustomersWhoBoughtRecs.AutoSize = true;
            this.lblCustomersWhoBoughtRecs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCustomersWhoBoughtRecs.Location = new System.Drawing.Point(13, 228);
            this.lblCustomersWhoBoughtRecs.Name = "lblCustomersWhoBoughtRecs";
            this.lblCustomersWhoBoughtRecs.Size = new System.Drawing.Size(329, 20);
            this.lblCustomersWhoBoughtRecs.TabIndex = 71;
            this.lblCustomersWhoBoughtRecs.Text = "Customers who bought this book also bought";
            this.lblCustomersWhoBoughtRecs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAuthorRecs
            // 
            this.lblAuthorRecs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthorRecs.Location = new System.Drawing.Point(409, 228);
            this.lblAuthorRecs.Name = "lblAuthorRecs";
            this.lblAuthorRecs.Size = new System.Drawing.Size(383, 20);
            this.lblAuthorRecs.TabIndex = 70;
            this.lblAuthorRecs.Text = "More by Author";
            this.lblAuthorRecs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbNextCover
            // 
            this.pbNextCover.Image = ((System.Drawing.Image)(resources.GetObject("pbNextCover.Image")));
            this.pbNextCover.Location = new System.Drawing.Point(413, 45);
            this.pbNextCover.Name = "pbNextCover";
            this.pbNextCover.Size = new System.Drawing.Size(60, 90);
            this.pbNextCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbNextCover.TabIndex = 69;
            this.pbNextCover.TabStop = false;
            // 
            // lblNextInSeries
            // 
            this.lblNextInSeries.AutoSize = true;
            this.lblNextInSeries.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextInSeries.Location = new System.Drawing.Point(408, 12);
            this.lblNextInSeries.Name = "lblNextInSeries";
            this.lblNextInSeries.Size = new System.Drawing.Size(104, 20);
            this.lblNextInSeries.TabIndex = 68;
            this.lblNextInSeries.Text = "Next in series";
            this.lblNextInSeries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtNextInSeries
            // 
            this.txtNextInSeries.AutoEllipsis = true;
            this.txtNextInSeries.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNextInSeries.Location = new System.Drawing.Point(486, 70);
            this.txtNextInSeries.Name = "txtNextInSeries";
            this.txtNextInSeries.Size = new System.Drawing.Size(307, 40);
            this.txtNextInSeries.TabIndex = 74;
            this.txtNextInSeries.Text = "This book is not part of a series or\r\nit is the last book in a series...";
            // 
            // frmPreviewEA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 482);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnFollow);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.pbAuthor);
            this.Controls.Add(this.lblFollowAuthor);
            this.Controls.Add(this.pbSeperator1);
            this.Controls.Add(this.lblUpdate);
            this.Controls.Add(this.pbRating);
            this.Controls.Add(this.lblYourRating);
            this.Controls.Add(this.lvAuthorRecs);
            this.Controls.Add(this.lvCustomersWhoBoughtRecs);
            this.Controls.Add(this.lblCustomersWhoBoughtRecs);
            this.Controls.Add(this.lblAuthorRecs);
            this.Controls.Add(this.pbNextCover);
            this.Controls.Add(this.lblNextInSeries);
            this.Controls.Add(this.txtNextInSeries);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewEA";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Before You Go...";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSeperator1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNextCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList ilcustomersWhoBoughtRecs;
        private System.Windows.Forms.ImageList ilauthorRecs;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnFollow;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.PictureBox pbAuthor;
        private System.Windows.Forms.Label lblFollowAuthor;
        private System.Windows.Forms.PictureBox pbSeperator1;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.PictureBox pbRating;
        private System.Windows.Forms.Label lblYourRating;
        private System.Windows.Forms.ListView lvAuthorRecs;
        private System.Windows.Forms.ListView lvCustomersWhoBoughtRecs;
        private System.Windows.Forms.Label lblCustomersWhoBoughtRecs;
        private System.Windows.Forms.Label lblAuthorRecs;
        private System.Windows.Forms.PictureBox pbNextCover;
        private System.Windows.Forms.Label lblNextInSeries;
        private System.Windows.Forms.Label txtNextInSeries;
    }
}