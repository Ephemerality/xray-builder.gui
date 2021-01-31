namespace XRayBuilderGUI.UI.Preview
{
    sealed partial class frmPreviewAP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPreviewAP));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblAuthorMore = new System.Windows.Forms.Label();
            this.pbAuthorImage = new System.Windows.Forms.PictureBox();
            this.lblBiography = new System.Windows.Forms.Label();
            this.dgvOtherBooks = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOtherBooks)).BeginInit();
            this.SuspendLayout();
            // 
            // lblAuthorMore
            // 
            this.lblAuthorMore.AutoEllipsis = true;
            this.lblAuthorMore.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lblAuthorMore.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthorMore.ForeColor = System.Drawing.Color.White;
            this.lblAuthorMore.Location = new System.Drawing.Point(12, 114);
            this.lblAuthorMore.Name = "lblAuthorMore";
            this.lblAuthorMore.Size = new System.Drawing.Size(560, 25);
            this.lblAuthorMore.TabIndex = 67;
            this.lblAuthorMore.Text = " Kindle Books By Author";
            this.lblAuthorMore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbAuthorImage
            // 
            this.pbAuthorImage.Image = ((System.Drawing.Image)(resources.GetObject("pbAuthorImage.Image")));
            this.pbAuthorImage.Location = new System.Drawing.Point(12, 12);
            this.pbAuthorImage.Name = "pbAuthorImage";
            this.pbAuthorImage.Size = new System.Drawing.Size(90, 90);
            this.pbAuthorImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAuthorImage.TabIndex = 65;
            this.pbAuthorImage.TabStop = false;
            // 
            // lblBiography
            // 
            this.lblBiography.AutoEllipsis = true;
            this.lblBiography.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBiography.Location = new System.Drawing.Point(110, 15);
            this.lblBiography.Name = "lblBiography";
            this.lblBiography.Size = new System.Drawing.Size(462, 99);
            this.lblBiography.TabIndex = 66;
            this.lblBiography.Text = "Biography";
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
            this.dgvOtherBooks.ColumnHeadersHeight = 29;
            this.dgvOtherBooks.ColumnHeadersVisible = false;
            this.dgvOtherBooks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.dgvOtherBooks.GridColor = System.Drawing.SystemColors.Control;
            this.dgvOtherBooks.Location = new System.Drawing.Point(12, 139);
            this.dgvOtherBooks.Name = "dgvOtherBooks";
            this.dgvOtherBooks.ReadOnly = true;
            this.dgvOtherBooks.RowHeadersVisible = false;
            this.dgvOtherBooks.RowHeadersWidth = 617;
            this.dgvOtherBooks.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvOtherBooks.RowTemplate.DividerHeight = 1;
            this.dgvOtherBooks.RowTemplate.Height = 30;
            this.dgvOtherBooks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvOtherBooks.Size = new System.Drawing.Size(560, 300);
            this.dgvOtherBooks.TabIndex = 68;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column1.HeaderText = "Column1";
            this.Column1.MaxInputLength = 32;
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // frmPreviewAP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 451);
            this.Controls.Add(this.lblAuthorMore);
            this.Controls.Add(this.pbAuthorImage);
            this.Controls.Add(this.lblBiography);
            this.Controls.Add(this.dgvOtherBooks);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewAP";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Author";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPreviewAP_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbAuthorImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOtherBooks)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblAuthorMore;
        private System.Windows.Forms.PictureBox pbAuthorImage;
        private System.Windows.Forms.Label lblBiography;
        private System.Windows.Forms.DataGridView dgvOtherBooks;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}