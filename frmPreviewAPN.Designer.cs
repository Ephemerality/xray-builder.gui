namespace XRayBuilderGUI
{
    partial class frmPreviewAPN
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPreviewAPN));
            this.dgvOtherBooks = new System.Windows.Forms.DataGridView();
            this.lblAuthorMore = new System.Windows.Forms.Label();
            this.lblBiography1 = new System.Windows.Forms.Label();
            this.lblBiography2 = new System.Windows.Forms.Label();
            this.ilOtherBooks = new System.Windows.Forms.ImageList(this.components);
            this.pbAuthorImage = new System.Windows.Forms.PictureBox();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewImageColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOtherBooks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvOtherBooks
            // 
            this.dgvOtherBooks.AllowUserToAddRows = false;
            this.dgvOtherBooks.AllowUserToDeleteRows = false;
            this.dgvOtherBooks.AllowUserToResizeColumns = false;
            this.dgvOtherBooks.AllowUserToResizeRows = false;
            this.dgvOtherBooks.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvOtherBooks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvOtherBooks.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvOtherBooks.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dgvOtherBooks.ColumnHeadersVisible = false;
            this.dgvOtherBooks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvOtherBooks.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvOtherBooks.Location = new System.Drawing.Point(333, 32);
            this.dgvOtherBooks.Name = "dgvOtherBooks";
            this.dgvOtherBooks.ReadOnly = true;
            this.dgvOtherBooks.RowHeadersVisible = false;
            this.dgvOtherBooks.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvOtherBooks.RowTemplate.DividerHeight = 1;
            this.dgvOtherBooks.RowTemplate.Height = 32;
            this.dgvOtherBooks.Size = new System.Drawing.Size(259, 385);
            this.dgvOtherBooks.TabIndex = 64;
            // 
            // lblAuthorMore
            // 
            this.lblAuthorMore.AutoEllipsis = true;
            this.lblAuthorMore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.lblAuthorMore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthorMore.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.lblAuthorMore.Location = new System.Drawing.Point(333, 10);
            this.lblAuthorMore.Name = "lblAuthorMore";
            this.lblAuthorMore.Size = new System.Drawing.Size(259, 20);
            this.lblAuthorMore.TabIndex = 63;
            this.lblAuthorMore.Text = " Kindle Books By [author]";
            this.lblAuthorMore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBiography1
            // 
            this.lblBiography1.AutoEllipsis = true;
            this.lblBiography1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBiography1.Location = new System.Drawing.Point(81, 21);
            this.lblBiography1.Name = "lblBiography1";
            this.lblBiography1.Size = new System.Drawing.Size(240, 94);
            this.lblBiography1.TabIndex = 62;
            this.lblBiography1.Text = resources.GetString("lblBiography1.Text");
            // 
            // lblBiography2
            // 
            this.lblBiography2.AutoEllipsis = true;
            this.lblBiography2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBiography2.Location = new System.Drawing.Point(11, 112);
            this.lblBiography2.Name = "lblBiography2";
            this.lblBiography2.Size = new System.Drawing.Size(310, 304);
            this.lblBiography2.TabIndex = 65;
            this.lblBiography2.Text = resources.GetString("lblBiography2.Text");
            // 
            // ilOtherBooks
            // 
            this.ilOtherBooks.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilOtherBooks.ImageSize = new System.Drawing.Size(60, 90);
            this.ilOtherBooks.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pbAuthorImage
            // 
            this.pbAuthorImage.Image = ((System.Drawing.Image)(resources.GetObject("pbAuthorImage.Image")));
            this.pbAuthorImage.Location = new System.Drawing.Point(14, 14);
            this.pbAuthorImage.Name = "pbAuthorImage";
            this.pbAuthorImage.Size = new System.Drawing.Size(60, 90);
            this.pbAuthorImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAuthorImage.TabIndex = 61;
            this.pbAuthorImage.TabStop = false;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle1;
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column1.Width = 241;
            // 
            // Column2
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.NullValue = ((object)(resources.GetObject("dataGridViewCellStyle2.NullValue")));
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Column2.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column2.HeaderText = "Column2";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column2.Width = 18;
            // 
            // frmPreviewAPN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 426);
            this.Controls.Add(this.dgvOtherBooks);
            this.Controls.Add(this.lblAuthorMore);
            this.Controls.Add(this.pbAuthorImage);
            this.Controls.Add(this.lblBiography2);
            this.Controls.Add(this.lblBiography1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewAPN";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Abouth [author]";
            ((System.ComponentModel.ISupportInitialize)(this.dgvOtherBooks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvOtherBooks;
        private System.Windows.Forms.Label lblAuthorMore;
        private System.Windows.Forms.Label lblBiography1;
        private System.Windows.Forms.PictureBox pbAuthorImage;
        private System.Windows.Forms.Label lblBiography2;
        private System.Windows.Forms.ImageList ilOtherBooks;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewImageColumn Column2;
    }
}