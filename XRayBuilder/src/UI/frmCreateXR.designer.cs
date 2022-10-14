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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreateXR));
            this.lblName = new System.Windows.Forms.Label();
            this.lblAliases = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtAliases = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.rdoTopic = new System.Windows.Forms.RadioButton();
            this.rdoCharacter = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkRegex = new System.Windows.Forms.CheckBox();
            this.chkMatch = new System.Windows.Forms.CheckBox();
            this.chkCase = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dgvTerms = new System.Windows.Forms.DataGridView();
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
            this.tsmDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnDownloadTerms = new System.Windows.Forms.Button();
            this.btnGenerateAliases = new System.Windows.Forms.Button();
            this.btnClearAliases = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkWrapDescriptions = new System.Windows.Forms.CheckBox();
            this.chkAllowResizeName = new System.Windows.Forms.CheckBox();
            this.lblSep1 = new System.Windows.Forms.Label();
            this.lblSep2 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTerms)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.cmsTerms.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(37, 24);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(42, 15);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "N̲ame:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAliases
            // 
            this.lblAliases.AutoSize = true;
            this.lblAliases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAliases.Location = new System.Drawing.Point(32, 59);
            this.lblAliases.Name = "lblAliases";
            this.lblAliases.Size = new System.Drawing.Size(46, 15);
            this.lblAliases.TabIndex = 4;
            this.lblAliases.Text = "A̲liases:";
            this.lblAliases.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtDescription);
            this.groupBox3.Controls.Add(this.lblDescription);
            this.groupBox3.Controls.Add(this.txtAliases);
            this.groupBox3.Controls.Add(this.txtName);
            this.groupBox3.Controls.Add(this.lblName);
            this.groupBox3.Controls.Add(this.lblAliases);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(12, 10);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(625, 174);
            this.groupBox3.TabIndex = 39;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Details";
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(86, 91);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(526, 69);
            this.txtDescription.TabIndex = 7;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.Location = new System.Drawing.Point(9, 94);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(70, 15);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "D̲escription:";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAliases
            // 
            this.txtAliases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAliases.Location = new System.Drawing.Point(85, 56);
            this.txtAliases.Name = "txtAliases";
            this.txtAliases.Size = new System.Drawing.Size(526, 23);
            this.txtAliases.TabIndex = 5;
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtName.Location = new System.Drawing.Point(85, 21);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(526, 23);
            this.txtName.TabIndex = 1;
            // 
            // rdoTopic
            // 
            this.rdoTopic.AutoSize = true;
            this.rdoTopic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoTopic.Location = new System.Drawing.Point(13, 43);
            this.rdoTopic.Name = "rdoTopic";
            this.rdoTopic.Size = new System.Drawing.Size(62, 19);
            this.rdoTopic.TabIndex = 3;
            this.rdoTopic.Text = "S̲etting";
            this.rdoTopic.UseVisualStyleBackColor = true;
            // 
            // rdoCharacter
            // 
            this.rdoCharacter.AutoSize = true;
            this.rdoCharacter.Checked = true;
            this.rdoCharacter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCharacter.Location = new System.Drawing.Point(13, 18);
            this.rdoCharacter.Name = "rdoCharacter";
            this.rdoCharacter.Size = new System.Drawing.Size(76, 19);
            this.rdoCharacter.TabIndex = 2;
            this.rdoCharacter.TabStop = true;
            this.rdoCharacter.Text = "C̲haracter";
            this.rdoCharacter.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkRegex);
            this.groupBox2.Controls.Add(this.chkMatch);
            this.groupBox2.Controls.Add(this.chkCase);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(649, 86);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(119, 98);
            this.groupBox2.TabIndex = 40;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // chkRegex
            // 
            this.chkRegex.AutoSize = true;
            this.chkRegex.Enabled = false;
            this.chkRegex.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRegex.Location = new System.Drawing.Point(13, 69);
            this.chkRegex.Name = "chkRegex";
            this.chkRegex.Size = new System.Drawing.Size(58, 19);
            this.chkRegex.TabIndex = 2;
            this.chkRegex.Text = "RegEx";
            this.chkRegex.UseVisualStyleBackColor = true;
            // 
            // chkMatch
            // 
            this.chkMatch.AutoSize = true;
            this.chkMatch.Checked = true;
            this.chkMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMatch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkMatch.Location = new System.Drawing.Point(13, 44);
            this.chkMatch.Name = "chkMatch";
            this.chkMatch.Size = new System.Drawing.Size(60, 19);
            this.chkMatch.TabIndex = 1;
            this.chkMatch.Text = "M̲atch";
            this.chkMatch.UseVisualStyleBackColor = true;
            // 
            // chkCase
            // 
            this.chkCase.AutoSize = true;
            this.chkCase.Checked = true;
            this.chkCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCase.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkCase.Location = new System.Drawing.Point(13, 19);
            this.chkCase.Name = "chkCase";
            this.chkCase.Size = new System.Drawing.Size(100, 19);
            this.chkCase.TabIndex = 0;
            this.chkCase.Text = "Case̲ Sensitive";
            this.chkCase.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.dgvTerms);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(12, 187);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(755, 456);
            this.groupBox4.TabIndex = 46;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Terms";
            // 
            // dgvTerms
            // 
            this.dgvTerms.AllowUserToAddRows = false;
            this.dgvTerms.AllowUserToDeleteRows = false;
            this.dgvTerms.AllowUserToResizeRows = false;
            this.dgvTerms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTerms.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvTerms.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvTerms.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvTerms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTerms.Location = new System.Drawing.Point(13, 21);
            this.dgvTerms.MultiSelect = false;
            this.dgvTerms.Name = "dgvTerms";
            this.dgvTerms.RowHeadersVisible = false;
            this.dgvTerms.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgvTerms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTerms.Size = new System.Drawing.Size(729, 421);
            this.dgvTerms.TabIndex = 0;
            this.dgvTerms.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTerms_CellContentClick);
            this.dgvTerms.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvTerms_CellMouseDoubleClick);
            this.dgvTerms.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvTerms_CellMouseDown);
            this.dgvTerms.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.dgvTerms_CellParsing);
            this.dgvTerms.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTerms_CellValueChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.txtAsin);
            this.groupBox5.Controls.Add(this.lblAsin);
            this.groupBox5.Controls.Add(this.txtTitle);
            this.groupBox5.Controls.Add(this.lblTitle);
            this.groupBox5.Controls.Add(this.txtAuthor);
            this.groupBox5.Controls.Add(this.lblAuthor);
            this.groupBox5.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(12, 652);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(709, 58);
            this.groupBox5.TabIndex = 48;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "XML Details";
            // 
            // txtAsin
            // 
            this.txtAsin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAsin.Location = new System.Drawing.Point(594, 21);
            this.txtAsin.Name = "txtAsin";
            this.txtAsin.Size = new System.Drawing.Size(102, 23);
            this.txtAsin.TabIndex = 6;
            // 
            // lblAsin
            // 
            this.lblAsin.AutoSize = true;
            this.lblAsin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAsin.Location = new System.Drawing.Point(551, 25);
            this.lblAsin.Name = "lblAsin";
            this.lblAsin.Size = new System.Drawing.Size(36, 15);
            this.lblAsin.TabIndex = 7;
            this.lblAsin.Text = "ASI̲N:";
            this.lblAsin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTitle
            // 
            this.txtTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitle.Location = new System.Drawing.Point(327, 21);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(215, 23);
            this.txtTitle.TabIndex = 4;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(288, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(32, 15);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "T̲itle:";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAuthor
            // 
            this.txtAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAuthor.Location = new System.Drawing.Point(64, 21);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.Size = new System.Drawing.Size(215, 23);
            this.txtAuthor.TabIndex = 2;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthor.Location = new System.Drawing.Point(10, 25);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(47, 15);
            this.lblAuthor.TabIndex = 3;
            this.lblAuthor.Text = "Au̲thor:";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOpenXml
            // 
            this.btnOpenXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpenXml.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenXml.Image")));
            this.btnOpenXml.Location = new System.Drawing.Point(731, 665);
            this.btnOpenXml.Name = "btnOpenXml";
            this.btnOpenXml.Size = new System.Drawing.Size(37, 37);
            this.btnOpenXml.TabIndex = 47;
            this.btnOpenXml.UseVisualStyleBackColor = true;
            this.btnOpenXml.Click += new System.EventHandler(this.btnOpenXml_Click);
            // 
            // btnEditTerm
            // 
            this.btnEditTerm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnEditTerm.Image")));
            this.btnEditTerm.Location = new System.Drawing.Point(778, 499);
            this.btnEditTerm.Name = "btnEditTerm";
            this.btnEditTerm.Size = new System.Drawing.Size(37, 37);
            this.btnEditTerm.TabIndex = 44;
            this.btnEditTerm.UseVisualStyleBackColor = true;
            this.btnEditTerm.Click += new System.EventHandler(this.btnEditTerm_Click);
            // 
            // btnSaveXML
            // 
            this.btnSaveXML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveXML.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveXML.Image")));
            this.btnSaveXML.Location = new System.Drawing.Point(778, 665);
            this.btnSaveXML.Name = "btnSaveXML";
            this.btnSaveXML.Size = new System.Drawing.Size(37, 37);
            this.btnSaveXML.TabIndex = 43;
            this.btnSaveXML.UseVisualStyleBackColor = true;
            this.btnSaveXML.Click += new System.EventHandler(this.btnSaveXML_Click);
            // 
            // btnRemoveTerm
            // 
            this.btnRemoveTerm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveTerm.Image")));
            this.btnRemoveTerm.Location = new System.Drawing.Point(778, 546);
            this.btnRemoveTerm.Name = "btnRemoveTerm";
            this.btnRemoveTerm.Size = new System.Drawing.Size(37, 37);
            this.btnRemoveTerm.TabIndex = 42;
            this.btnRemoveTerm.UseVisualStyleBackColor = true;
            this.btnRemoveTerm.Click += new System.EventHandler(this.btnRemoveTerm_Click);
            // 
            // btnAddTerm
            // 
            this.btnAddTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnAddTerm.Image")));
            this.btnAddTerm.Location = new System.Drawing.Point(779, 31);
            this.btnAddTerm.Name = "btnAddTerm";
            this.btnAddTerm.Size = new System.Drawing.Size(37, 37);
            this.btnAddTerm.TabIndex = 41;
            this.btnAddTerm.UseVisualStyleBackColor = true;
            this.btnAddTerm.Click += new System.EventHandler(this.btnAddTerm_Click);
            // 
            // cmsTerms
            // 
            this.cmsTerms.AutoSize = false;
            this.cmsTerms.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmsTerms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.tsmDelete });
            this.cmsTerms.Name = "cmsTerms";
            this.cmsTerms.Size = new System.Drawing.Size(80, 48);
            // 
            // tsmDelete
            // 
            this.tsmDelete.AutoSize = false;
            this.tsmDelete.Image = ((System.Drawing.Image)(resources.GetObject("tsmDelete.Image")));
            this.tsmDelete.Name = "tsmDelete";
            this.tsmDelete.Size = new System.Drawing.Size(79, 22);
            this.tsmDelete.Text = "Delete";
            this.tsmDelete.Click += new System.EventHandler(this.tsmDelete_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Image = ((System.Drawing.Image)(resources.GetObject("btnClear.Image")));
            this.btnClear.Location = new System.Drawing.Point(778, 593);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(37, 37);
            this.btnClear.TabIndex = 49;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnDownloadTerms
            // 
            this.btnDownloadTerms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadTerms.Image = ((System.Drawing.Image)(resources.GetObject("btnDownloadTerms.Image")));
            this.btnDownloadTerms.Location = new System.Drawing.Point(778, 207);
            this.btnDownloadTerms.Name = "btnDownloadTerms";
            this.btnDownloadTerms.Size = new System.Drawing.Size(37, 37);
            this.btnDownloadTerms.TabIndex = 50;
            this.btnDownloadTerms.UseVisualStyleBackColor = true;
            this.btnDownloadTerms.Click += new System.EventHandler(this.btnDownloadTerms_Click);
            // 
            // btnGenerateAliases
            // 
            this.btnGenerateAliases.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerateAliases.Image = ((System.Drawing.Image)(resources.GetObject("btnGenerateAliases.Image")));
            this.btnGenerateAliases.Location = new System.Drawing.Point(778, 392);
            this.btnGenerateAliases.Name = "btnGenerateAliases";
            this.btnGenerateAliases.Size = new System.Drawing.Size(37, 37);
            this.btnGenerateAliases.TabIndex = 53;
            this.btnGenerateAliases.UseVisualStyleBackColor = true;
            this.btnGenerateAliases.Click += new System.EventHandler(this.btnGenerateAliases_Click);
            // 
            // btnClearAliases
            // 
            this.btnClearAliases.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearAliases.Image = ((System.Drawing.Image)(resources.GetObject("btnClearAliases.Image")));
            this.btnClearAliases.Location = new System.Drawing.Point(778, 439);
            this.btnClearAliases.Name = "btnClearAliases";
            this.btnClearAliases.Size = new System.Drawing.Size(37, 37);
            this.btnClearAliases.TabIndex = 54;
            this.btnClearAliases.UseVisualStyleBackColor = true;
            this.btnClearAliases.Click += new System.EventHandler(this.btnClearAliases_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoTopic);
            this.groupBox1.Controls.Add(this.rdoCharacter);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(649, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(119, 73);
            this.groupBox1.TabIndex = 55;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Term Type";
            // 
            // chkWrapDescriptions
            // 
            this.chkWrapDescriptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkWrapDescriptions.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkWrapDescriptions.Image = ((System.Drawing.Image)(resources.GetObject("chkWrapDescriptions.Image")));
            this.chkWrapDescriptions.Location = new System.Drawing.Point(778, 316);
            this.chkWrapDescriptions.Name = "chkWrapDescriptions";
            this.chkWrapDescriptions.Size = new System.Drawing.Size(37, 37);
            this.chkWrapDescriptions.TabIndex = 56;
            this.chkWrapDescriptions.UseVisualStyleBackColor = true;
            this.chkWrapDescriptions.CheckedChanged += new System.EventHandler(this.chkWrapDescriptions_CheckedChanged);
            // 
            // chkAllowResizeName
            // 
            this.chkAllowResizeName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowResizeName.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkAllowResizeName.Image = ((System.Drawing.Image)(resources.GetObject("chkAllowResizeName.Image")));
            this.chkAllowResizeName.Location = new System.Drawing.Point(778, 267);
            this.chkAllowResizeName.Name = "chkAllowResizeName";
            this.chkAllowResizeName.Size = new System.Drawing.Size(37, 37);
            this.chkAllowResizeName.TabIndex = 57;
            this.chkAllowResizeName.UseVisualStyleBackColor = true;
            this.chkAllowResizeName.CheckedChanged += new System.EventHandler(this.chkAllowResizeName_CheckedChanged);
            // 
            // lblSep1
            // 
            this.lblSep1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSep1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSep1.Location = new System.Drawing.Point(779, 487);
            this.lblSep1.Name = "lblSep1";
            this.lblSep1.Size = new System.Drawing.Size(36, 2);
            this.lblSep1.TabIndex = 58;
            // 
            // lblSep2
            // 
            this.lblSep2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSep2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSep2.Location = new System.Drawing.Point(779, 255);
            this.lblSep2.Name = "lblSep2";
            this.lblSep2.Size = new System.Drawing.Size(36, 2);
            this.lblSep2.TabIndex = 59;
            // 
            // frmCreateXR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 739);
            this.Controls.Add(this.lblSep2);
            this.Controls.Add(this.lblSep1);
            this.Controls.Add(this.chkAllowResizeName);
            this.Controls.Add(this.chkWrapDescriptions);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClearAliases);
            this.Controls.Add(this.btnGenerateAliases);
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
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(843, 778);
            this.Name = "frmCreateXR";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "X-Ray Terms Creator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCreateXR_FormClosing);
            this.Load += new System.EventHandler(this.frmCreateXR_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTerms)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.cmsTerms.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnAddTerm;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnClearAliases;
        private System.Windows.Forms.Button btnDownloadTerms;
        private System.Windows.Forms.Button btnEditTerm;
        private System.Windows.Forms.Button btnGenerateAliases;
        private System.Windows.Forms.Button btnOpenXml;
        private System.Windows.Forms.Button btnRemoveTerm;
        private System.Windows.Forms.Button btnSaveXML;
        private System.Windows.Forms.CheckBox chkCase;
        private System.Windows.Forms.CheckBox chkMatch;
        private System.Windows.Forms.CheckBox chkRegex;
        private System.Windows.Forms.ContextMenuStrip cmsTerms;
        private System.Windows.Forms.DataGridView dgvTerms;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblAliases;
        private System.Windows.Forms.Label lblAsin;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RadioButton rdoCharacter;
        private System.Windows.Forms.RadioButton rdoTopic;
        private System.Windows.Forms.ToolStripMenuItem tsmDelete;
        private System.Windows.Forms.TextBox txtAliases;
        private System.Windows.Forms.TextBox txtAsin;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtTitle;

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkWrapDescriptions;
        private System.Windows.Forms.CheckBox chkAllowResizeName;
        private System.Windows.Forms.Label lblSep1;
        private System.Windows.Forms.Label lblSep2;
    }
}