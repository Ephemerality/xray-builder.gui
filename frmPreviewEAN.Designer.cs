namespace XRayBuilderGUI
{
    partial class frmPreviewEAN
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPreviewEAN));
            this.lvCustomersWhoBoughtRecs = new System.Windows.Forms.ListView();
            this.ilcustomersWhoBoughtRecs = new System.Windows.Forms.ImageList(this.components);
            this.lblCustomersWhoBoughtRecs = new System.Windows.Forms.Label();
            this.lblAuthorRecs = new System.Windows.Forms.Label();
            this.lblNextAuthor = new System.Windows.Forms.Label();
            this.lblNextTitle = new System.Windows.Forms.Label();
            this.lblNextInSeries = new System.Windows.Forms.Label();
            this.pbSeperator = new System.Windows.Forms.PictureBox();
            this.pbNextCover = new System.Windows.Forms.PictureBox();
            this.ilauthorRecs = new System.Windows.Forms.ImageList(this.components);
            this.lvAuthorRecs = new System.Windows.Forms.ListView();
            this.lblNotInSeries = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbSeperator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNextCover)).BeginInit();
            this.SuspendLayout();
            // 
            // lvCustomersWhoBoughtRecs
            // 
            this.lvCustomersWhoBoughtRecs.BackColor = System.Drawing.SystemColors.Control;
            this.lvCustomersWhoBoughtRecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvCustomersWhoBoughtRecs.LargeImageList = this.ilcustomersWhoBoughtRecs;
            this.lvCustomersWhoBoughtRecs.Location = new System.Drawing.Point(8, 161);
            this.lvCustomersWhoBoughtRecs.Name = "lvCustomersWhoBoughtRecs";
            this.lvCustomersWhoBoughtRecs.Size = new System.Drawing.Size(291, 258);
            this.lvCustomersWhoBoughtRecs.TabIndex = 41;
            this.lvCustomersWhoBoughtRecs.UseCompatibleStateImageBehavior = false;
            this.lvCustomersWhoBoughtRecs.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvcustomersWhoBoughtRecs_ItemSelectionChanged);
            // 
            // ilcustomersWhoBoughtRecs
            // 
            this.ilcustomersWhoBoughtRecs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilcustomersWhoBoughtRecs.ImageSize = new System.Drawing.Size(60, 90);
            this.ilcustomersWhoBoughtRecs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lblCustomersWhoBoughtRecs
            // 
            this.lblCustomersWhoBoughtRecs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCustomersWhoBoughtRecs.Location = new System.Drawing.Point(11, 140);
            this.lblCustomersWhoBoughtRecs.Name = "lblCustomersWhoBoughtRecs";
            this.lblCustomersWhoBoughtRecs.Size = new System.Drawing.Size(286, 13);
            this.lblCustomersWhoBoughtRecs.TabIndex = 40;
            this.lblCustomersWhoBoughtRecs.Text = "Customers who bought this book also bought";
            this.lblCustomersWhoBoughtRecs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAuthorRecs
            // 
            this.lblAuthorRecs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthorRecs.Location = new System.Drawing.Point(306, 10);
            this.lblAuthorRecs.Name = "lblAuthorRecs";
            this.lblAuthorRecs.Size = new System.Drawing.Size(289, 13);
            this.lblAuthorRecs.TabIndex = 38;
            this.lblAuthorRecs.Text = "More by the author";
            this.lblAuthorRecs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNextAuthor
            // 
            this.lblNextAuthor.AutoEllipsis = true;
            this.lblNextAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextAuthor.Location = new System.Drawing.Point(80, 66);
            this.lblNextAuthor.Name = "lblNextAuthor";
            this.lblNextAuthor.Size = new System.Drawing.Size(219, 13);
            this.lblNextAuthor.TabIndex = 36;
            this.lblNextAuthor.Text = "NextAuthor";
            this.lblNextAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNextTitle
            // 
            this.lblNextTitle.AutoEllipsis = true;
            this.lblNextTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextTitle.Location = new System.Drawing.Point(80, 49);
            this.lblNextTitle.Name = "lblNextTitle";
            this.lblNextTitle.Size = new System.Drawing.Size(219, 13);
            this.lblNextTitle.TabIndex = 34;
            this.lblNextTitle.Text = "Next Title";
            this.lblNextTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNextInSeries
            // 
            this.lblNextInSeries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextInSeries.Location = new System.Drawing.Point(10, 10);
            this.lblNextInSeries.Name = "lblNextInSeries";
            this.lblNextInSeries.Size = new System.Drawing.Size(289, 13);
            this.lblNextInSeries.TabIndex = 33;
            this.lblNextInSeries.Text = "Next in series";
            this.lblNextInSeries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbSeperator
            // 
            this.pbSeperator.Image = global::XRayBuilderGUI.Properties.Resources.seperator;
            this.pbSeperator.Location = new System.Drawing.Point(14, 131);
            this.pbSeperator.Name = "pbSeperator";
            this.pbSeperator.Size = new System.Drawing.Size(285, 2);
            this.pbSeperator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbSeperator.TabIndex = 37;
            this.pbSeperator.TabStop = false;
            // 
            // pbNextCover
            // 
            this.pbNextCover.Image = ((System.Drawing.Image)(resources.GetObject("pbNextCover.Image")));
            this.pbNextCover.Location = new System.Drawing.Point(14, 31);
            this.pbNextCover.Name = "pbNextCover";
            this.pbNextCover.Size = new System.Drawing.Size(60, 90);
            this.pbNextCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbNextCover.TabIndex = 35;
            this.pbNextCover.TabStop = false;
            // 
            // ilauthorRecs
            // 
            this.ilauthorRecs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilauthorRecs.ImageSize = new System.Drawing.Size(60, 90);
            this.ilauthorRecs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lvAuthorRecs
            // 
            this.lvAuthorRecs.BackColor = System.Drawing.SystemColors.Control;
            this.lvAuthorRecs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvAuthorRecs.LargeImageList = this.ilauthorRecs;
            this.lvAuthorRecs.Location = new System.Drawing.Point(304, 31);
            this.lvAuthorRecs.Name = "lvAuthorRecs";
            this.lvAuthorRecs.Size = new System.Drawing.Size(291, 388);
            this.lvAuthorRecs.TabIndex = 42;
            this.lvAuthorRecs.UseCompatibleStateImageBehavior = false;
            this.lvAuthorRecs.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvauthorRecs_ItemSelectionChanged);
            // 
            // lblNotInSeries
            // 
            this.lblNotInSeries.AutoEllipsis = true;
            this.lblNotInSeries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNotInSeries.Location = new System.Drawing.Point(11, 29);
            this.lblNotInSeries.Name = "lblNotInSeries";
            this.lblNotInSeries.Size = new System.Drawing.Size(289, 30);
            this.lblNotInSeries.TabIndex = 43;
            this.lblNotInSeries.Text = "This book is not part of a series or\r\nit is the last book in a series...";
            this.lblNotInSeries.Visible = false;
            // 
            // frmPreviewEAN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 426);
            this.Controls.Add(this.lvAuthorRecs);
            this.Controls.Add(this.lvCustomersWhoBoughtRecs);
            this.Controls.Add(this.lblCustomersWhoBoughtRecs);
            this.Controls.Add(this.lblAuthorRecs);
            this.Controls.Add(this.pbSeperator);
            this.Controls.Add(this.lblNextAuthor);
            this.Controls.Add(this.pbNextCover);
            this.Controls.Add(this.lblNextTitle);
            this.Controls.Add(this.lblNextInSeries);
            this.Controls.Add(this.lblNotInSeries);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewEAN";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Before You Go...";
            ((System.ComponentModel.ISupportInitialize)(this.pbSeperator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNextCover)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvCustomersWhoBoughtRecs;
        private System.Windows.Forms.Label lblCustomersWhoBoughtRecs;
        private System.Windows.Forms.Label lblAuthorRecs;
        private System.Windows.Forms.PictureBox pbSeperator;
        private System.Windows.Forms.Label lblNextAuthor;
        private System.Windows.Forms.PictureBox pbNextCover;
        private System.Windows.Forms.Label lblNextTitle;
        private System.Windows.Forms.Label lblNextInSeries;
        private System.Windows.Forms.ImageList ilauthorRecs;
        private System.Windows.Forms.ImageList ilcustomersWhoBoughtRecs;
        private System.Windows.Forms.ListView lvAuthorRecs;
        private System.Windows.Forms.Label lblNotInSeries;
    }
}