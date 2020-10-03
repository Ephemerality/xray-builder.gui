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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cmsTerms = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnOpenXml = new System.Windows.Forms.ToolStripButton();
            this.btnSaveXML = new System.Windows.Forms.ToolStripButton();
            this.btnDownloadTerms = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddTerm = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveTerm = new System.Windows.Forms.ToolStripButton();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.btnEditTerm = new System.Windows.Forms.ToolStripButton();
            this.btnSplitTerm = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnGenerateAliases = new System.Windows.Forms.ToolStripMenuItem();
            this.btnClearAliases = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnLink = new System.Windows.Forms.ToolStripButton();
            this.tsbtnWikipedia = new System.Windows.Forms.ToolStripButton();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.txtAsin = new System.Windows.Forms.TextBox();
            this.lblAsin = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdoCharacter = new System.Windows.Forms.RadioButton();
            this.rdoTopic = new System.Windows.Forms.RadioButton();
            this.dgvTerms = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column11 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkDelete = new System.Windows.Forms.CheckBox();
            this.chkRegex = new System.Windows.Forms.CheckBox();
            this.chkMatch = new System.Windows.Forms.CheckBox();
            this.chkCase = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtWildcards = new System.Windows.Forms.TextBox();
            this.lblWildcards = new System.Windows.Forms.Label();
            this.txtLink = new System.Windows.Forms.TextBox();
            this.lblLink = new System.Windows.Forms.Label();
            this.txtAliases = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblAliases = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdoNone = new System.Windows.Forms.RadioButton();
            this.rdoWikipedia = new System.Windows.Forms.RadioButton();
            this.rdoGoodreads = new System.Windows.Forms.RadioButton();
            this.cmsTerms.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTerms)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsTerms
            // 
            this.cmsTerms.AutoSize = false;
            this.cmsTerms.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cmsTerms.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsTerms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEdit,
            this.tsmDelete});
            this.cmsTerms.Name = "cmsTerms";
            this.cmsTerms.Size = new System.Drawing.Size(80, 48);
            // 
            // tsmEdit
            // 
            this.tsmEdit.AutoSize = false;
            this.tsmEdit.Image = ((System.Drawing.Image)(resources.GetObject("tsmEdit.Image")));
            this.tsmEdit.Name = "tsmEdit";
            this.tsmEdit.ShowShortcutKeys = false;
            this.tsmEdit.Size = new System.Drawing.Size(79, 22);
            this.tsmEdit.Text = "Edit";
            this.tsmEdit.Click += new System.EventHandler(this.tsmEdit_Click);
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
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenXml,
            this.btnSaveXML,
            this.btnDownloadTerms,
            this.toolStripSeparator1,
            this.btnAddTerm,
            this.btnRemoveTerm,
            this.btnClear,
            this.btnEditTerm,
            this.btnSplitTerm,
            this.toolStripSeparator2,
            this.btnLink,
            this.tsbtnWikipedia});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1008, 100);
            this.toolStrip.TabIndex = 55;
            this.toolStrip.Text = "toolStrip1";
            // 
            // btnOpenXml
            // 
            this.btnOpenXml.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenXml.Image")));
            this.btnOpenXml.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOpenXml.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenXml.Margin = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.btnOpenXml.Name = "btnOpenXml";
            this.btnOpenXml.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnOpenXml.Size = new System.Drawing.Size(96, 80);
            this.btnOpenXml.Text = "Open File";
            this.btnOpenXml.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnOpenXml.Click += new System.EventHandler(this.btnOpenXml_Click);
            // 
            // btnSaveXML
            // 
            this.btnSaveXML.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveXML.Image")));
            this.btnSaveXML.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSaveXML.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveXML.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnSaveXML.Name = "btnSaveXML";
            this.btnSaveXML.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnSaveXML.Size = new System.Drawing.Size(64, 80);
            this.btnSaveXML.Text = "Save";
            this.btnSaveXML.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSaveXML.Click += new System.EventHandler(this.btnSaveXML_Click);
            // 
            // btnDownloadTerms
            // 
            this.btnDownloadTerms.Image = ((System.Drawing.Image)(resources.GetObject("btnDownloadTerms.Image")));
            this.btnDownloadTerms.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDownloadTerms.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDownloadTerms.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnDownloadTerms.Name = "btnDownloadTerms";
            this.btnDownloadTerms.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnDownloadTerms.Size = new System.Drawing.Size(97, 80);
            this.btnDownloadTerms.Text = "Roentgen";
            this.btnDownloadTerms.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnDownloadTerms.Click += new System.EventHandler(this.btnDownloadTerms_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 100);
            // 
            // btnAddTerm
            // 
            this.btnAddTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnAddTerm.Image")));
            this.btnAddTerm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnAddTerm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddTerm.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnAddTerm.Name = "btnAddTerm";
            this.btnAddTerm.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnAddTerm.Size = new System.Drawing.Size(64, 80);
            this.btnAddTerm.Text = "Add";
            this.btnAddTerm.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAddTerm.Click += new System.EventHandler(this.btnAddTerm_Click);
            // 
            // btnRemoveTerm
            // 
            this.btnRemoveTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveTerm.Image")));
            this.btnRemoveTerm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRemoveTerm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveTerm.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnRemoveTerm.Name = "btnRemoveTerm";
            this.btnRemoveTerm.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnRemoveTerm.Size = new System.Drawing.Size(87, 80);
            this.btnRemoveTerm.Text = "Remove";
            this.btnRemoveTerm.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRemoveTerm.Click += new System.EventHandler(this.btnRemoveTerm_Click);
            // 
            // btnClear
            // 
            this.btnClear.Image = ((System.Drawing.Image)(resources.GetObject("btnClear.Image")));
            this.btnClear.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnClear.Name = "btnClear";
            this.btnClear.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnClear.Size = new System.Drawing.Size(89, 80);
            this.btnClear.Text = "Clear All";
            this.btnClear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnEditTerm
            // 
            this.btnEditTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnEditTerm.Image")));
            this.btnEditTerm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnEditTerm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditTerm.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnEditTerm.Name = "btnEditTerm";
            this.btnEditTerm.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnEditTerm.Size = new System.Drawing.Size(64, 80);
            this.btnEditTerm.Text = "Edit";
            this.btnEditTerm.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnEditTerm.Click += new System.EventHandler(this.btnEditTerm_Click);
            // 
            // btnSplitTerm
            // 
            this.btnSplitTerm.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnGenerateAliases,
            this.btnClearAliases});
            this.btnSplitTerm.Image = ((System.Drawing.Image)(resources.GetObject("btnSplitTerm.Image")));
            this.btnSplitTerm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSplitTerm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSplitTerm.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnSplitTerm.Name = "btnSplitTerm";
            this.btnSplitTerm.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnSplitTerm.Size = new System.Drawing.Size(89, 80);
            this.btnSplitTerm.Text = "Aliases";
            this.btnSplitTerm.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // btnGenerateAliases
            // 
            this.btnGenerateAliases.Name = "btnGenerateAliases";
            this.btnGenerateAliases.Size = new System.Drawing.Size(152, 26);
            this.btnGenerateAliases.Text = "Generate";
            this.btnGenerateAliases.Click += new System.EventHandler(this.btnGenerateAliases_Click);
            // 
            // btnClearAliases
            // 
            this.btnClearAliases.Name = "btnClearAliases";
            this.btnClearAliases.Size = new System.Drawing.Size(152, 26);
            this.btnClearAliases.Text = "Clear";
            this.btnClearAliases.Click += new System.EventHandler(this.btnClearAliases_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 100);
            // 
            // btnLink
            // 
            this.btnLink.Image = ((System.Drawing.Image)(resources.GetObject("btnLink.Image")));
            this.btnLink.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLink.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.btnLink.Name = "btnLink";
            this.btnLink.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnLink.Size = new System.Drawing.Size(64, 80);
            this.btnLink.Text = "Visit";
            this.btnLink.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnLink.Click += new System.EventHandler(this.btnLink_Click);
            // 
            // tsbtnWikipedia
            // 
            this.tsbtnWikipedia.Enabled = false;
            this.tsbtnWikipedia.Image = ((System.Drawing.Image)(resources.GetObject("tsbtnWikipedia.Image")));
            this.tsbtnWikipedia.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbtnWikipedia.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnWikipedia.Margin = new System.Windows.Forms.Padding(4, 10, 0, 10);
            this.tsbtnWikipedia.Name = "tsbtnWikipedia";
            this.tsbtnWikipedia.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsbtnWikipedia.Size = new System.Drawing.Size(77, 80);
            this.tsbtnWikipedia.Text = "Search";
            this.tsbtnWikipedia.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.txtAsin);
            this.groupBox6.Controls.Add(this.lblAsin);
            this.groupBox6.Controls.Add(this.txtTitle);
            this.groupBox6.Controls.Add(this.lblTitle);
            this.groupBox6.Controls.Add(this.txtAuthor);
            this.groupBox6.Controls.Add(this.lblAuthor);
            this.groupBox6.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(17, 105);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(974, 74);
            this.groupBox6.TabIndex = 56;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Save File As...";
            // 
            // txtAsin
            // 
            this.txtAsin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAsin.Location = new System.Drawing.Point(836, 28);
            this.txtAsin.Name = "txtAsin";
            this.txtAsin.Size = new System.Drawing.Size(120, 27);
            this.txtAsin.TabIndex = 55;
            // 
            // lblAsin
            // 
            this.lblAsin.AutoSize = true;
            this.lblAsin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAsin.Location = new System.Drawing.Point(779, 31);
            this.lblAsin.Name = "lblAsin";
            this.lblAsin.Size = new System.Drawing.Size(45, 20);
            this.lblAsin.TabIndex = 56;
            this.lblAsin.Text = "ASIN:";
            this.lblAsin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTitle
            // 
            this.txtTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitle.Location = new System.Drawing.Point(459, 28);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(307, 27);
            this.txtTitle.TabIndex = 53;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(406, 31);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(41, 20);
            this.lblTitle.TabIndex = 54;
            this.lblTitle.Text = "Title:";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAuthor
            // 
            this.txtAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAuthor.Location = new System.Drawing.Point(83, 28);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.Size = new System.Drawing.Size(310, 27);
            this.txtAuthor.TabIndex = 51;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthor.Location = new System.Drawing.Point(14, 31);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(57, 20);
            this.lblAuthor.TabIndex = 52;
            this.lblAuthor.Text = "Author:";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.txtDescription);
            this.groupBox5.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(556, 185);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(435, 205);
            this.groupBox5.TabIndex = 62;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Description";
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(18, 28);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(399, 159);
            this.txtDescription.TabIndex = 5;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rdoCharacter);
            this.groupBox4.Controls.Add(this.rdoTopic);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(17, 395);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(174, 63);
            this.groupBox4.TabIndex = 61;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Type";
            // 
            // rdoCharacter
            // 
            this.rdoCharacter.AutoSize = true;
            this.rdoCharacter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCharacter.Location = new System.Drawing.Point(18, 24);
            this.rdoCharacter.Name = "rdoCharacter";
            this.rdoCharacter.Size = new System.Drawing.Size(73, 24);
            this.rdoCharacter.TabIndex = 1;
            this.rdoCharacter.Text = "Person";
            this.rdoCharacter.UseVisualStyleBackColor = true;
            // 
            // rdoTopic
            // 
            this.rdoTopic.AutoSize = true;
            this.rdoTopic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoTopic.Location = new System.Drawing.Point(101, 24);
            this.rdoTopic.Name = "rdoTopic";
            this.rdoTopic.Size = new System.Drawing.Size(63, 24);
            this.rdoTopic.TabIndex = 2;
            this.rdoTopic.Text = "Term";
            this.rdoTopic.UseVisualStyleBackColor = true;
            // 
            // dgvTerms
            // 
            this.dgvTerms.AllowUserToAddRows = false;
            this.dgvTerms.AllowUserToDeleteRows = false;
            this.dgvTerms.AllowUserToOrderColumns = true;
            this.dgvTerms.AllowUserToResizeRows = false;
            this.dgvTerms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTerms.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvTerms.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvTerms.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvTerms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTerms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column6,
            this.Column7,
            this.Column8,
            this.Column9,
            this.Column10,
            this.Column11});
            this.dgvTerms.Location = new System.Drawing.Point(17, 476);
            this.dgvTerms.Name = "dgvTerms";
            this.dgvTerms.ReadOnly = true;
            this.dgvTerms.RowHeadersVisible = false;
            this.dgvTerms.RowHeadersWidth = 51;
            this.dgvTerms.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgvTerms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTerms.Size = new System.Drawing.Size(974, 214);
            this.dgvTerms.TabIndex = 57;
            this.dgvTerms.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvTerms_CellMouseDoubleClick);
            this.dgvTerms.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvTerms_CellMouseDown);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Type";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 46;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Name";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 125;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Aliases";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 150;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Description";
            this.Column4.MinimumWidth = 6;
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 236;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "URL";
            this.Column6.MinimumWidth = 6;
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            this.Column6.Width = 150;
            // 
            // Column7
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column7.DefaultCellStyle = dataGridViewCellStyle1;
            this.Column7.HeaderText = "Source";
            this.Column7.MinimumWidth = 6;
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            this.Column7.Width = 70;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "Match";
            this.Column8.MinimumWidth = 6;
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            this.Column8.Width = 70;
            // 
            // Column9
            // 
            this.Column9.HeaderText = "Case Sensitive";
            this.Column9.MinimumWidth = 6;
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            this.Column9.Width = 120;
            // 
            // Column10
            // 
            this.Column10.HeaderText = "Delete";
            this.Column10.MinimumWidth = 6;
            this.Column10.Name = "Column10";
            this.Column10.ReadOnly = true;
            this.Column10.Width = 70;
            // 
            // Column11
            // 
            this.Column11.HeaderText = "RegEx";
            this.Column11.MinimumWidth = 6;
            this.Column11.Name = "Column11";
            this.Column11.ReadOnly = true;
            this.Column11.Width = 70;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkDelete);
            this.groupBox2.Controls.Add(this.chkRegex);
            this.groupBox2.Controls.Add(this.chkMatch);
            this.groupBox2.Controls.Add(this.chkCase);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(556, 397);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(435, 63);
            this.groupBox2.TabIndex = 60;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // chkDelete
            // 
            this.chkDelete.AutoSize = true;
            this.chkDelete.Enabled = false;
            this.chkDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDelete.Location = new System.Drawing.Point(232, 25);
            this.chkDelete.Name = "chkDelete";
            this.chkDelete.Size = new System.Drawing.Size(75, 24);
            this.chkDelete.TabIndex = 2;
            this.chkDelete.Text = "Delete";
            this.chkDelete.UseVisualStyleBackColor = true;
            // 
            // chkRegex
            // 
            this.chkRegex.AutoSize = true;
            this.chkRegex.Enabled = false;
            this.chkRegex.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRegex.Location = new System.Drawing.Point(316, 25);
            this.chkRegex.Name = "chkRegex";
            this.chkRegex.Size = new System.Drawing.Size(72, 24);
            this.chkRegex.TabIndex = 3;
            this.chkRegex.Text = "RegEx";
            this.chkRegex.UseVisualStyleBackColor = true;
            // 
            // chkMatch
            // 
            this.chkMatch.AutoSize = true;
            this.chkMatch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkMatch.Location = new System.Drawing.Point(18, 25);
            this.chkMatch.Name = "chkMatch";
            this.chkMatch.Size = new System.Drawing.Size(72, 24);
            this.chkMatch.TabIndex = 0;
            this.chkMatch.Text = "Match";
            this.chkMatch.UseVisualStyleBackColor = true;
            // 
            // chkCase
            // 
            this.chkCase.AutoSize = true;
            this.chkCase.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkCase.Location = new System.Drawing.Point(99, 25);
            this.chkCase.Name = "chkCase";
            this.chkCase.Size = new System.Drawing.Size(124, 24);
            this.chkCase.TabIndex = 1;
            this.chkCase.Text = "Case Sensitive";
            this.chkCase.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtWildcards);
            this.groupBox3.Controls.Add(this.lblWildcards);
            this.groupBox3.Controls.Add(this.txtLink);
            this.groupBox3.Controls.Add(this.lblLink);
            this.groupBox3.Controls.Add(this.txtAliases);
            this.groupBox3.Controls.Add(this.txtName);
            this.groupBox3.Controls.Add(this.lblName);
            this.groupBox3.Controls.Add(this.lblAliases);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(17, 185);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(522, 206);
            this.groupBox3.TabIndex = 58;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Term Details";
            // 
            // txtWildcards
            // 
            this.txtWildcards.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWildcards.Location = new System.Drawing.Point(104, 116);
            this.txtWildcards.Name = "txtWildcards";
            this.txtWildcards.Size = new System.Drawing.Size(400, 27);
            this.txtWildcards.TabIndex = 4;
            // 
            // lblWildcards
            // 
            this.lblWildcards.AutoSize = true;
            this.lblWildcards.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWildcards.Location = new System.Drawing.Point(14, 117);
            this.lblWildcards.Name = "lblWildcards";
            this.lblWildcards.Size = new System.Drawing.Size(78, 20);
            this.lblWildcards.TabIndex = 50;
            this.lblWildcards.Text = "Wildcards:";
            this.lblWildcards.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLink
            // 
            this.txtLink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLink.Location = new System.Drawing.Point(104, 160);
            this.txtLink.Name = "txtLink";
            this.txtLink.Size = new System.Drawing.Size(400, 27);
            this.txtLink.TabIndex = 6;
            // 
            // lblLink
            // 
            this.lblLink.AutoSize = true;
            this.lblLink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLink.Location = new System.Drawing.Point(20, 161);
            this.lblLink.Name = "lblLink";
            this.lblLink.Size = new System.Drawing.Size(72, 20);
            this.lblLink.TabIndex = 8;
            this.lblLink.Text = "Web Link:";
            this.lblLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAliases
            // 
            this.txtAliases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAliases.Location = new System.Drawing.Point(104, 72);
            this.txtAliases.Name = "txtAliases";
            this.txtAliases.Size = new System.Drawing.Size(400, 27);
            this.txtAliases.TabIndex = 3;
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtName.Location = new System.Drawing.Point(104, 28);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(400, 27);
            this.txtName.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(40, 31);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(52, 20);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAliases
            // 
            this.lblAliases.AutoSize = true;
            this.lblAliases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAliases.Location = new System.Drawing.Point(34, 73);
            this.lblAliases.Name = "lblAliases";
            this.lblAliases.Size = new System.Drawing.Size(58, 20);
            this.lblAliases.TabIndex = 4;
            this.lblAliases.Text = "Aliases:";
            this.lblAliases.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoNone);
            this.groupBox1.Controls.Add(this.rdoWikipedia);
            this.groupBox1.Controls.Add(this.rdoGoodreads);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(208, 397);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(331, 63);
            this.groupBox1.TabIndex = 59;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // rdoNone
            // 
            this.rdoNone.AutoSize = true;
            this.rdoNone.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoNone.Location = new System.Drawing.Point(236, 24);
            this.rdoNone.Name = "rdoNone";
            this.rdoNone.Size = new System.Drawing.Size(66, 24);
            this.rdoNone.TabIndex = 2;
            this.rdoNone.Text = "None";
            this.rdoNone.UseVisualStyleBackColor = true;
            // 
            // rdoWikipedia
            // 
            this.rdoWikipedia.AutoSize = true;
            this.rdoWikipedia.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoWikipedia.Location = new System.Drawing.Point(130, 24);
            this.rdoWikipedia.Name = "rdoWikipedia";
            this.rdoWikipedia.Size = new System.Drawing.Size(97, 24);
            this.rdoWikipedia.TabIndex = 1;
            this.rdoWikipedia.Text = "Wikipedia";
            this.rdoWikipedia.UseVisualStyleBackColor = true;
            // 
            // rdoGoodreads
            // 
            this.rdoGoodreads.AutoSize = true;
            this.rdoGoodreads.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoGoodreads.Location = new System.Drawing.Point(17, 24);
            this.rdoGoodreads.Name = "rdoGoodreads";
            this.rdoGoodreads.Size = new System.Drawing.Size(103, 24);
            this.rdoGoodreads.TabIndex = 0;
            this.rdoGoodreads.Text = "Goodreads";
            this.rdoGoodreads.UseVisualStyleBackColor = true;
            // 
            // frmCreateXR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 707);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.dgvTerms);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.toolStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1026, 754);
            this.Name = "frmCreateXR";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "X-Ray Terms Creator";
            this.Load += new System.EventHandler(this.frmCreateXR_Load);
            this.cmsTerms.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTerms)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.ContextMenuStrip cmsTerms;
        private System.Windows.Forms.ToolStripMenuItem tsmDelete;
        private System.Windows.Forms.ToolStripMenuItem tsmEdit;

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnOpenXml;
        private System.Windows.Forms.ToolStripButton btnDownloadTerms;
        private System.Windows.Forms.ToolStripButton btnSaveXML;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton btnSplitTerm;
        private System.Windows.Forms.ToolStripMenuItem btnGenerateAliases;
        private System.Windows.Forms.ToolStripMenuItem btnClearAliases;
        private System.Windows.Forms.ToolStripButton btnEditTerm;
        private System.Windows.Forms.ToolStripButton btnAddTerm;
        private System.Windows.Forms.ToolStripButton btnRemoveTerm;
        private System.Windows.Forms.ToolStripButton btnClear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnLink;
        private System.Windows.Forms.ToolStripButton tsbtnWikipedia;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox txtAsin;
        private System.Windows.Forms.Label lblAsin;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rdoCharacter;
        private System.Windows.Forms.RadioButton rdoTopic;
        private System.Windows.Forms.DataGridView dgvTerms;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkDelete;
        private System.Windows.Forms.CheckBox chkRegex;
        private System.Windows.Forms.CheckBox chkMatch;
        private System.Windows.Forms.CheckBox chkCase;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtWildcards;
        private System.Windows.Forms.Label lblWildcards;
        private System.Windows.Forms.TextBox txtLink;
        private System.Windows.Forms.Label lblLink;
        private System.Windows.Forms.TextBox txtAliases;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblAliases;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdoNone;
        private System.Windows.Forms.RadioButton rdoWikipedia;
        private System.Windows.Forms.RadioButton rdoGoodreads;
        private System.Windows.Forms.DataGridViewImageColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column8;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column9;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column10;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column11;
    }
}