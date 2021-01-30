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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.lblAuthorMore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(115)))), ((int)(((byte)(115)))));
            this.lblAuthorMore.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthorMore.ForeColor = System.Drawing.Color.White;
            this.lblAuthorMore.Location = new System.Drawing.Point(17, 124);
            this.lblAuthorMore.Name = "lblAuthorMore";
            this.lblAuthorMore.Size = new System.Drawing.Size(623, 25);
            this.lblAuthorMore.TabIndex = 67;
            this.lblAuthorMore.Text = " Kindle Books By [author]";
            this.lblAuthorMore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbAuthorImage
            // 
            this.pbAuthorImage.Image = ((System.Drawing.Image)(resources.GetObject("pbAuthorImage.Image")));
            this.pbAuthorImage.Location = new System.Drawing.Point(17, 17);
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
            this.lblBiography.Location = new System.Drawing.Point(119, 12);
            this.lblBiography.Name = "lblBiography";
            this.lblBiography.Size = new System.Drawing.Size(521, 103);
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
            this.dgvOtherBooks.Location = new System.Drawing.Point(17, 155);
            this.dgvOtherBooks.Name = "dgvOtherBooks";
            this.dgvOtherBooks.ReadOnly = true;
            this.dgvOtherBooks.RowHeadersVisible = false;
            this.dgvOtherBooks.RowHeadersWidth = 617;
            this.dgvOtherBooks.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvOtherBooks.RowTemplate.DividerHeight = 1;
            this.dgvOtherBooks.RowTemplate.Height = 32;
            this.dgvOtherBooks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvOtherBooks.Size = new System.Drawing.Size(623, 384);
            this.dgvOtherBooks.TabIndex = 68;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle1;
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
            this.ClientSize = new System.Drawing.Size(657, 556);
            this.Controls.Add(this.dgvOtherBooks);
            this.Controls.Add(this.lblAuthorMore);
            this.Controls.Add(this.pbAuthorImage);
            this.Controls.Add(this.lblBiography);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewAP";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Abouth [author]";
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