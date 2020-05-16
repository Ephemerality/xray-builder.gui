namespace XRayBuilderGUI.UI
{
    partial class frmCreateXR
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreateXR));
            this.lblName = new System.Windows.Forms.Label();
            this.lblAliases = new System.Windows.Forms.Label();
            this.rdoWikipedia = new System.Windows.Forms.RadioButton();
            this.rdoGoodreads = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnLink = new System.Windows.Forms.Button();
            this.rdoTopic = new System.Windows.Forms.RadioButton();
            this.rdoCharacter = new System.Windows.Forms.RadioButton();
            this.txtLink = new System.Windows.Forms.TextBox();
            this.lblLink = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtAliases = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkDelete = new System.Windows.Forms.CheckBox();
            this.chkRegex = new System.Windows.Forms.CheckBox();
            this.chkMatch = new System.Windows.Forms.CheckBox();
            this.chkCase = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvTerms = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtAsin = new System.Windows.Forms.TextBox();
            this.lblAsin = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.btnOpenXml = new System.Windows.Forms.Button();
            this.btnEditTerm = new System.Windows.Forms.Button();
            this.btnSaveXML = new System.Windows.Forms.Button();
            this.btnRemoveTerm = new System.Windows.Forms.Button();
            this.btnAddTerm = new System.Windows.Forms.Button();
            this.cmsTerms = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnDownloadTerms = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.dgvTerms)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.cmsTerms.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(10, 23);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAliases
            // 
            this.lblAliases.AutoSize = true;
            this.lblAliases.Location = new System.Drawing.Point(11, 55);
            this.lblAliases.Name = "lblAliases";
            this.lblAliases.Size = new System.Drawing.Size(43, 13);
            this.lblAliases.TabIndex = 4;
            this.lblAliases.Text = "Aliases:";
            this.lblAliases.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rdoWikipedia
            // 
            this.rdoWikipedia.AutoSize = true;
            this.rdoWikipedia.Location = new System.Drawing.Point(95, 17);
            this.rdoWikipedia.Name = "rdoWikipedia";
            this.rdoWikipedia.Size = new System.Drawing.Size(72, 17);
            this.rdoWikipedia.TabIndex = 1;
            this.rdoWikipedia.Text = "Wikipedia";
            this.rdoWikipedia.UseVisualStyleBackColor = true;
            // 
            // rdoGoodreads
            // 
            this.rdoGoodreads.AutoSize = true;
            this.rdoGoodreads.Checked = true;
            this.rdoGoodreads.Location = new System.Drawing.Point(14, 17);
            this.rdoGoodreads.Name = "rdoGoodreads";
            this.rdoGoodreads.Size = new System.Drawing.Size(77, 17);
            this.rdoGoodreads.TabIndex = 0;
            this.rdoGoodreads.TabStop = true;
            this.rdoGoodreads.Text = "Goodreads";
            this.rdoGoodreads.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnLink);
            this.groupBox3.Controls.Add(this.rdoTopic);
            this.groupBox3.Controls.Add(this.rdoCharacter);
            this.groupBox3.Controls.Add(this.txtLink);
            this.groupBox3.Controls.Add(this.lblLink);
            this.groupBox3.Controls.Add(this.txtDescription);
            this.groupBox3.Controls.Add(this.lblDescription);
            this.groupBox3.Controls.Add(this.txtAliases);
            this.groupBox3.Controls.Add(this.txtName);
            this.groupBox3.Controls.Add(this.lblName);
            this.groupBox3.Controls.Add(this.lblAliases);
            this.groupBox3.Location = new System.Drawing.Point(12, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(536, 190);
            this.groupBox3.TabIndex = 39;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Details";
            // 
            // btnLink
            // 
            this.btnLink.Location = new System.Drawing.Point(491, 150);
            this.btnLink.Name = "btnLink";
            this.btnLink.Size = new System.Drawing.Size(31, 26);
            this.btnLink.TabIndex = 49;
            this.btnLink.Text = "...";
            this.btnLink.UseVisualStyleBackColor = true;
            this.btnLink.Click += new System.EventHandler(this.btnLink_Click);
            // 
            // rdoTopic
            // 
            this.rdoTopic.AutoSize = true;
            this.rdoTopic.Location = new System.Drawing.Point(471, 22);
            this.rdoTopic.Name = "rdoTopic";
            this.rdoTopic.Size = new System.Drawing.Size(58, 17);
            this.rdoTopic.TabIndex = 3;
            this.rdoTopic.Text = "Setting";
            this.rdoTopic.UseVisualStyleBackColor = true;
            // 
            // rdoCharacter
            // 
            this.rdoCharacter.AutoSize = true;
            this.rdoCharacter.Checked = true;
            this.rdoCharacter.Location = new System.Drawing.Point(394, 22);
            this.rdoCharacter.Name = "rdoCharacter";
            this.rdoCharacter.Size = new System.Drawing.Size(71, 17);
            this.rdoCharacter.TabIndex = 2;
            this.rdoCharacter.TabStop = true;
            this.rdoCharacter.Text = "Character";
            this.rdoCharacter.UseVisualStyleBackColor = true;
            // 
            // txtLink
            // 
            this.txtLink.Location = new System.Drawing.Point(78, 156);
            this.txtLink.Name = "txtLink";
            this.txtLink.Size = new System.Drawing.Size(403, 20);
            this.txtLink.TabIndex = 9;
            // 
            // lblLink
            // 
            this.lblLink.AutoSize = true;
            this.lblLink.Location = new System.Drawing.Point(11, 159);
            this.lblLink.Name = "lblLink";
            this.lblLink.Size = new System.Drawing.Size(56, 13);
            this.lblLink.TabIndex = 8;
            this.lblLink.Text = "Web Link:";
            this.lblLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(79, 84);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(443, 60);
            this.txtDescription.TabIndex = 7;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(10, 87);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "Description:";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAliases
            // 
            this.txtAliases.Location = new System.Drawing.Point(78, 52);
            this.txtAliases.Name = "txtAliases";
            this.txtAliases.Size = new System.Drawing.Size(444, 20);
            this.txtAliases.TabIndex = 5;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(79, 20);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(303, 20);
            this.txtName.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkDelete);
            this.groupBox2.Controls.Add(this.chkRegex);
            this.groupBox2.Controls.Add(this.chkMatch);
            this.groupBox2.Controls.Add(this.chkCase);
            this.groupBox2.Location = new System.Drawing.Point(198, 202);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(350, 47);
            this.groupBox2.TabIndex = 40;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // chkDelete
            // 
            this.chkDelete.AutoSize = true;
            this.chkDelete.Enabled = false;
            this.chkDelete.Location = new System.Drawing.Point(184, 19);
            this.chkDelete.Name = "chkDelete";
            this.chkDelete.Size = new System.Drawing.Size(57, 17);
            this.chkDelete.TabIndex = 3;
            this.chkDelete.Text = "Delete";
            this.chkDelete.UseVisualStyleBackColor = true;
            // 
            // chkRegex
            // 
            this.chkRegex.AutoSize = true;
            this.chkRegex.Enabled = false;
            this.chkRegex.Location = new System.Drawing.Point(244, 19);
            this.chkRegex.Name = "chkRegex";
            this.chkRegex.Size = new System.Drawing.Size(58, 17);
            this.chkRegex.TabIndex = 2;
            this.chkRegex.Text = "RegEx";
            this.chkRegex.UseVisualStyleBackColor = true;
            // 
            // chkMatch
            // 
            this.chkMatch.AutoSize = true;
            this.chkMatch.Checked = true;
            this.chkMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMatch.Location = new System.Drawing.Point(14, 19);
            this.chkMatch.Name = "chkMatch";
            this.chkMatch.Size = new System.Drawing.Size(56, 17);
            this.chkMatch.TabIndex = 1;
            this.chkMatch.Text = "Match";
            this.chkMatch.UseVisualStyleBackColor = true;
            // 
            // chkCase
            // 
            this.chkCase.AutoSize = true;
            this.chkCase.Checked = true;
            this.chkCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCase.Location = new System.Drawing.Point(84, 19);
            this.chkCase.Name = "chkCase";
            this.chkCase.Size = new System.Drawing.Size(96, 17);
            this.chkCase.TabIndex = 0;
            this.chkCase.Text = "Case Sensitive";
            this.chkCase.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.dgvTerms);
            this.groupBox4.Location = new System.Drawing.Point(12, 255);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(536, 187);
            this.groupBox4.TabIndex = 46;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Terms";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(14, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(507, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Double-click an item to remove it and edit it above. Don\'t forget to add it back " + "in!";
            // 
            // dgvTerms
            // 
            this.dgvTerms.AllowUserToAddRows = false;
            this.dgvTerms.AllowUserToDeleteRows = false;
            this.dgvTerms.AllowUserToResizeRows = false;
            this.dgvTerms.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTerms.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvTerms.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvTerms.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvTerms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTerms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.Column1, this.Column2, this.Column3, this.Column4, this.Column6, this.Column10, this.Column5, this.Column7, this.Column8, this.Column9});
            this.dgvTerms.Location = new System.Drawing.Point(14, 32);
            this.dgvTerms.MultiSelect = false;
            this.dgvTerms.Name = "dgvTerms";
            this.dgvTerms.ReadOnly = true;
            this.dgvTerms.RowHeadersVisible = false;
            this.dgvTerms.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgvTerms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTerms.Size = new System.Drawing.Size(508, 140);
            this.dgvTerms.TabIndex = 0;
            this.dgvTerms.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvTerms_CellMouseDoubleClick);
            this.dgvTerms.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvTerms_CellMouseDown);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Type";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 36;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Name";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Aliases";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 150;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Description";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 150;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "URL";
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            // 
            // Column10
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column10.DefaultCellStyle = dataGridViewCellStyle1;
            this.Column10.HeaderText = "Source";
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            this.Column10.Width = 70;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Match";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column5.Width = 90;
            // 
            // Column7
            // 
            this.Column7.HeaderText = "Case Sensitive";
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            this.Column7.Width = 90;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "Delete";
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            this.Column8.Width = 90;
            // 
            // Column9
            // 
            this.Column9.HeaderText = "RegEx";
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            this.Column9.Width = 90;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoWikipedia);
            this.groupBox1.Controls.Add(this.rdoGoodreads);
            this.groupBox1.Location = new System.Drawing.Point(12, 202);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 47);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.txtAsin);
            this.groupBox5.Controls.Add(this.lblAsin);
            this.groupBox5.Controls.Add(this.txtTitle);
            this.groupBox5.Controls.Add(this.lblTitle);
            this.groupBox5.Controls.Add(this.txtAuthor);
            this.groupBox5.Controls.Add(this.lblAuthor);
            this.groupBox5.Location = new System.Drawing.Point(12, 448);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(536, 51);
            this.groupBox5.TabIndex = 48;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Save File As";
            // 
            // txtAsin
            // 
            this.txtAsin.Location = new System.Drawing.Point(442, 20);
            this.txtAsin.Name = "txtAsin";
            this.txtAsin.Size = new System.Drawing.Size(80, 20);
            this.txtAsin.TabIndex = 6;
            // 
            // lblAsin
            // 
            this.lblAsin.AutoSize = true;
            this.lblAsin.Location = new System.Drawing.Point(400, 23);
            this.lblAsin.Name = "lblAsin";
            this.lblAsin.Size = new System.Drawing.Size(35, 13);
            this.lblAsin.TabIndex = 7;
            this.lblAsin.Text = "ASIN:";
            this.lblAsin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(246, 20);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(145, 20);
            this.txtTitle.TabIndex = 4;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(211, 23);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(28, 13);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "Tite:";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAuthor
            // 
            this.txtAuthor.Location = new System.Drawing.Point(57, 20);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.Size = new System.Drawing.Size(145, 20);
            this.txtAuthor.TabIndex = 2;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(9, 23);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(41, 13);
            this.lblAuthor.TabIndex = 3;
            this.lblAuthor.Text = "Author:";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOpenXml
            // 
            this.btnOpenXml.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenXml.Image = ((System.Drawing.Image) (resources.GetObject("btnOpenXml.Image")));
            this.btnOpenXml.Location = new System.Drawing.Point(559, 398);
            this.btnOpenXml.Name = "btnOpenXml";
            this.btnOpenXml.Size = new System.Drawing.Size(32, 32);
            this.btnOpenXml.TabIndex = 47;
            this.btnOpenXml.UseVisualStyleBackColor = true;
            this.btnOpenXml.Click += new System.EventHandler(this.btnOpenXml_Click);
            // 
            // btnEditTerm
            // 
            this.btnEditTerm.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditTerm.Image = ((System.Drawing.Image) (resources.GetObject("btnEditTerm.Image")));
            this.btnEditTerm.Location = new System.Drawing.Point(559, 260);
            this.btnEditTerm.Name = "btnEditTerm";
            this.btnEditTerm.Size = new System.Drawing.Size(32, 32);
            this.btnEditTerm.TabIndex = 44;
            this.btnEditTerm.UseVisualStyleBackColor = true;
            this.btnEditTerm.Click += new System.EventHandler(this.btnEditTerm_Click);
            // 
            // btnSaveXML
            // 
            this.btnSaveXML.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveXML.Image = ((System.Drawing.Image) (resources.GetObject("btnSaveXML.Image")));
            this.btnSaveXML.Location = new System.Drawing.Point(559, 440);
            this.btnSaveXML.Name = "btnSaveXML";
            this.btnSaveXML.Size = new System.Drawing.Size(32, 32);
            this.btnSaveXML.TabIndex = 43;
            this.btnSaveXML.UseVisualStyleBackColor = true;
            this.btnSaveXML.Click += new System.EventHandler(this.btnSaveXML_Click);
            // 
            // btnRemoveTerm
            // 
            this.btnRemoveTerm.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveTerm.Image = ((System.Drawing.Image) (resources.GetObject("btnRemoveTerm.Image")));
            this.btnRemoveTerm.Location = new System.Drawing.Point(559, 302);
            this.btnRemoveTerm.Name = "btnRemoveTerm";
            this.btnRemoveTerm.Size = new System.Drawing.Size(32, 32);
            this.btnRemoveTerm.TabIndex = 42;
            this.btnRemoveTerm.UseVisualStyleBackColor = true;
            this.btnRemoveTerm.Click += new System.EventHandler(this.btnRemoveTerm_Click);
            // 
            // btnAddTerm
            // 
            this.btnAddTerm.Image = ((System.Drawing.Image) (resources.GetObject("btnAddTerm.Image")));
            this.btnAddTerm.Location = new System.Drawing.Point(559, 11);
            this.btnAddTerm.Name = "btnAddTerm";
            this.btnAddTerm.Size = new System.Drawing.Size(32, 32);
            this.btnAddTerm.TabIndex = 41;
            this.btnAddTerm.UseVisualStyleBackColor = true;
            this.btnAddTerm.Click += new System.EventHandler(this.btnAddTerm_Click);
            // 
            // cmsTerms
            // 
            this.cmsTerms.AutoSize = false;
            this.cmsTerms.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmsTerms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {this.tsmEdit, this.tsmDelete});
            this.cmsTerms.Name = "cmsTerms";
            this.cmsTerms.Size = new System.Drawing.Size(80, 48);
            // 
            // tsmEdit
            // 
            this.tsmEdit.AutoSize = false;
            this.tsmEdit.Image = ((System.Drawing.Image) (resources.GetObject("tsmEdit.Image")));
            this.tsmEdit.Name = "tsmEdit";
            this.tsmEdit.ShowShortcutKeys = false;
            this.tsmEdit.Size = new System.Drawing.Size(79, 22);
            this.tsmEdit.Text = "Edit";
            this.tsmEdit.Click += new System.EventHandler(this.tsmEdit_Click);
            // 
            // tsmDelete
            // 
            this.tsmDelete.AutoSize = false;
            this.tsmDelete.Image = ((System.Drawing.Image) (resources.GetObject("tsmDelete.Image")));
            this.tsmDelete.Name = "tsmDelete";
            this.tsmDelete.Size = new System.Drawing.Size(79, 22);
            this.tsmDelete.Text = "Delete";
            this.tsmDelete.Click += new System.EventHandler(this.tsmDelete_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Image = ((System.Drawing.Image) (resources.GetObject("btnClear.Image")));
            this.btnClear.Location = new System.Drawing.Point(559, 344);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(32, 32);
            this.btnClear.TabIndex = 49;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnDownloadTerms
            // 
            this.btnDownloadTerms.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadTerms.Image = global::XRayBuilderGUI.Properties.Resources.setting;
            this.btnDownloadTerms.Location = new System.Drawing.Point(559, 209);
            this.btnDownloadTerms.Name = "btnDownloadTerms";
            this.btnDownloadTerms.Size = new System.Drawing.Size(32, 32);
            this.btnDownloadTerms.TabIndex = 50;
            this.btnDownloadTerms.UseVisualStyleBackColor = true;
            this.btnDownloadTerms.Click += new System.EventHandler(this.btnDownloadTerms_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(559, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 16);
            this.label2.TabIndex = 52;
            this.label2.Text = "New!";
            // 
            // frmCreateXR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 511);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDownloadTerms);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.btnOpenXml);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.btnEditTerm);
            this.Controls.Add(this.btnSaveXML);
            this.Controls.Add(this.btnRemoveTerm);
            this.Controls.Add(this.btnAddTerm);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(618, 550);
            this.Name = "frmCreateXR";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "X-Ray Terms Creator";
            this.Load += new System.EventHandler(this.frmCreateXR_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.dgvTerms)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.cmsTerms.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnAddTerm;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnDownloadTerms;
        private System.Windows.Forms.Button btnEditTerm;
        private System.Windows.Forms.Button btnLink;
        private System.Windows.Forms.Button btnOpenXml;
        private System.Windows.Forms.Button btnRemoveTerm;
        private System.Windows.Forms.Button btnSaveXML;
        private System.Windows.Forms.CheckBox chkCase;
        private System.Windows.Forms.CheckBox chkDelete;
        private System.Windows.Forms.CheckBox chkMatch;
        private System.Windows.Forms.CheckBox chkRegex;
        private System.Windows.Forms.ContextMenuStrip cmsTerms;
        private System.Windows.Forms.DataGridViewImageColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column7;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column8;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column9;
        private System.Windows.Forms.DataGridView dgvTerms;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblAliases;
        private System.Windows.Forms.Label lblAsin;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblLink;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RadioButton rdoCharacter;
        private System.Windows.Forms.RadioButton rdoGoodreads;
        private System.Windows.Forms.RadioButton rdoTopic;
        private System.Windows.Forms.RadioButton rdoWikipedia;
        private System.Windows.Forms.ToolStripMenuItem tsmDelete;
        private System.Windows.Forms.ToolStripMenuItem tsmEdit;
        private System.Windows.Forms.TextBox txtAliases;
        private System.Windows.Forms.TextBox txtAsin;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtLink;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtTitle;

        #endregion
    }
}