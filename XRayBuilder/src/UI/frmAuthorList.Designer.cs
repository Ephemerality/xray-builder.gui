namespace XRayBuilderGUI.UI
{
    partial class frmAuthorList
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAuthorList));
            this.lblAuthorMore = new System.Windows.Forms.Label();
            this.lblBiography = new System.Windows.Forms.Label();
            this.lblMessage2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblMessage1 = new System.Windows.Forms.Label();
            this.cbResults = new System.Windows.Forms.ComboBox();
            this.linkStore = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBio = new System.Windows.Forms.Label();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvOtherBooks = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOtherBooks)).BeginInit();
            this.SuspendLayout();
            // 
            // lblAuthorMore
            // 
            this.lblAuthorMore.AutoSize = true;
            this.lblAuthorMore.BackColor = System.Drawing.SystemColors.Control;
            this.lblAuthorMore.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthorMore.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblAuthorMore.Location = new System.Drawing.Point(6, 203);
            this.lblAuthorMore.Name = "lblAuthorMore";
            this.lblAuthorMore.Size = new System.Drawing.Size(156, 15);
            this.lblAuthorMore.TabIndex = 71;
            this.lblAuthorMore.Text = " Other Books By This Author";
            this.lblAuthorMore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBiography
            // 
            this.lblBiography.AutoEllipsis = true;
            this.lblBiography.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBiography.Location = new System.Drawing.Point(8, 122);
            this.lblBiography.Name = "lblBiography";
            this.lblBiography.Size = new System.Drawing.Size(371, 80);
            this.lblBiography.TabIndex = 70;
            this.lblBiography.Text = "Biography";
            // 
            // lblMessage2
            // 
            this.lblMessage2.AutoEllipsis = true;
            this.lblMessage2.AutoSize = true;
            this.lblMessage2.Location = new System.Drawing.Point(9, 65);
            this.lblMessage2.Name = "lblMessage2";
            this.lblMessage2.Size = new System.Drawing.Size(221, 15);
            this.lblMessage2.TabIndex = 76;
            this.lblMessage2.Text = "Which is the correct author of this book?";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(291, 11);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 25);
            this.btnOK.TabIndex = 75;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblMessage1
            // 
            this.lblMessage1.AutoEllipsis = true;
            this.lblMessage1.AutoSize = true;
            this.lblMessage1.Location = new System.Drawing.Point(9, 44);
            this.lblMessage1.Name = "lblMessage1";
            this.lblMessage1.Size = new System.Drawing.Size(271, 15);
            this.lblMessage1.TabIndex = 74;
            this.lblMessage1.Text = "?? authors were found on Amazon with this name.";
            // 
            // cbResults
            // 
            this.cbResults.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbResults.FormattingEnabled = true;
            this.cbResults.IntegralHeight = false;
            this.cbResults.Location = new System.Drawing.Point(12, 12);
            this.cbResults.Name = "cbResults";
            this.cbResults.Size = new System.Drawing.Size(268, 23);
            this.cbResults.TabIndex = 73;
            this.cbResults.SelectedIndexChanged += new System.EventHandler(this.cbResults_SelectedIndexChanged);
            // 
            // linkStore
            // 
            this.linkStore.ActiveLinkColor = System.Drawing.Color.RoyalBlue;
            this.linkStore.AutoSize = true;
            this.linkStore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkStore.Location = new System.Drawing.Point(302, 63);
            this.linkStore.Name = "linkStore";
            this.linkStore.Size = new System.Drawing.Size(67, 15);
            this.linkStore.TabIndex = 86;
            this.linkStore.TabStop = true;
            this.linkStore.Text = "See in store";
            this.linkStore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkStore.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkStore_LinkClicked);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(0, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(391, 2);
            this.label1.TabIndex = 87;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblBio
            // 
            this.lblBio.AutoSize = true;
            this.lblBio.BackColor = System.Drawing.SystemColors.Control;
            this.lblBio.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBio.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblBio.Location = new System.Drawing.Point(8, 102);
            this.lblBio.Name = "lblBio";
            this.lblBio.Size = new System.Drawing.Size(61, 15);
            this.lblBio.TabIndex = 88;
            this.lblBio.Text = "Biography";
            this.lblBio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.dgvOtherBooks.Location = new System.Drawing.Point(6, 219);
            this.dgvOtherBooks.Name = "dgvOtherBooks";
            this.dgvOtherBooks.ReadOnly = true;
            this.dgvOtherBooks.RowHeadersVisible = false;
            this.dgvOtherBooks.RowHeadersWidth = 617;
            this.dgvOtherBooks.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvOtherBooks.RowTemplate.DividerHeight = 1;
            this.dgvOtherBooks.RowTemplate.Height = 26;
            this.dgvOtherBooks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvOtherBooks.Size = new System.Drawing.Size(373, 130);
            this.dgvOtherBooks.TabIndex = 72;
            this.dgvOtherBooks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOtherBooks_CellContentClick);
            // 
            // frmAuthorList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 361);
            this.Controls.Add(this.lblBio);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvOtherBooks);
            this.Controls.Add(this.linkStore);
            this.Controls.Add(this.lblBiography);
            this.Controls.Add(this.lblAuthorMore);
            this.Controls.Add(this.lblMessage2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMessage1);
            this.Controls.Add(this.cbResults);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAuthorList";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Author Search Results";
            this.Load += new System.EventHandler(this.frmAuthorList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOtherBooks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAuthorMore;
        private System.Windows.Forms.Label lblBiography;
        public System.Windows.Forms.Label lblMessage2;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.Label lblMessage1;
        public System.Windows.Forms.ComboBox cbResults;
        private System.Windows.Forms.LinkLabel linkStore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblBio;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridView dgvOtherBooks;
    }
}