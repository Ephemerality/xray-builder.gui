namespace XRayBuilderGUI.UI
{
    partial class frmMain
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
                _cancelTokens.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lblGoodreads = new System.Windows.Forms.Label();
            this.txtMobi = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtGoodreads = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.prgBar = new System.Windows.Forms.ProgressBar();
            this.txtOutput = new System.Windows.Forms.RichTextBox();
            this.cmsLog = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAndSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblFiles = new System.Windows.Forms.Label();
            this.txtAsin = new System.Windows.Forms.LinkLabel();
            this.txtAuthor = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.Label();
            this.lblAsin = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdoRoentgen = new System.Windows.Forms.RadioButton();
            this.rdoFile = new System.Windows.Forms.RadioButton();
            this.rdoGoodreads = new System.Windows.Forms.RadioButton();
            this.txtXMLFile = new System.Windows.Forms.TextBox();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnBrowseMobi = new System.Windows.Forms.ToolStripButton();
            this.btnSearchGoodreads = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnKindleExtras = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnBuild = new System.Windows.Forms.ToolStripButton();
            this.btnXraySource = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnXraySourceFile = new System.Windows.Forms.ToolStripMenuItem();
            this.btnXraySourceGoodreads = new System.Windows.Forms.ToolStripMenuItem();
            this.btnXraySourceRoentgen = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCreate = new System.Windows.Forms.ToolStripButton();
            this.btnDownloadTerms = new System.Windows.Forms.ToolStripButton();
            this.btnBrowseXML = new System.Windows.Forms.ToolStripButton();
            this.btnExtractTerms = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnBrowseFolders = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnBrowseOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnBrowseDump = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBrowseGeneratedData = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBrowseLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBrowseTemp = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBrowseXmlFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSettings = new System.Windows.Forms.ToolStripButton();
            this.btnOneClick = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnHelp = new System.Windows.Forms.ToolStripButton();
            this.btnAbout = new System.Windows.Forms.ToolStripButton();
            this.pbFile4 = new System.Windows.Forms.PictureBox();
            this.pbFile3 = new System.Windows.Forms.PictureBox();
            this.pbFile2 = new System.Windows.Forms.PictureBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pbFile1 = new System.Windows.Forms.PictureBox();
            this.pbCover = new System.Windows.Forms.PictureBox();
            this.txtDatasource = new System.Windows.Forms.LinkLabel();
            this.lblDatasource = new System.Windows.Forms.Label();
            this.btnUnpack = new System.Windows.Forms.ToolStripButton();
            this.tmiAuthorProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.tmiEndAction = new System.Windows.Forms.ToolStripMenuItem();
            this.tmiStartAction = new System.Windows.Forms.ToolStripMenuItem();
            this.tmiXray = new System.Windows.Forms.ToolStripMenuItem();
            this.btnPreview = new System.Windows.Forms.ToolStripDropDownButton();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.cmsLog.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).BeginInit();
            this.SuspendLayout();
            // 
            // lblGoodreads
            // 
            this.lblGoodreads.AutoSize = true;
            this.lblGoodreads.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGoodreads.Location = new System.Drawing.Point(14, 31);
            this.lblGoodreads.Name = "lblGoodreads";
            this.lblGoodreads.Size = new System.Drawing.Size(115, 20);
            this.lblGoodreads.TabIndex = 0;
            this.lblGoodreads.Text = "Goodreads URL:";
            this.lblGoodreads.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMobi
            // 
            this.txtMobi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMobi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMobi.Location = new System.Drawing.Point(18, 28);
            this.txtMobi.Name = "txtMobi";
            this.txtMobi.Size = new System.Drawing.Size(1197, 27);
            this.txtMobi.TabIndex = 0;
            this.txtMobi.TextChanged += new System.EventHandler(this.txtMobi_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblGoodreads);
            this.groupBox1.Controls.Add(this.txtGoodreads);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 183);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1233, 74);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Metadata Provider";
            // 
            // txtGoodreads
            // 
            this.txtGoodreads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGoodreads.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGoodreads.Location = new System.Drawing.Point(140, 28);
            this.txtGoodreads.Name = "txtGoodreads";
            this.txtGoodreads.Size = new System.Drawing.Size(1075, 27);
            this.txtGoodreads.TabIndex = 0;
            this.txtGoodreads.TextChanged += new System.EventHandler(this.txtGoodreads_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.txtMobi);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(12, 103);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1233, 74);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Book";
            // 
            // prgBar
            // 
            this.prgBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgBar.Location = new System.Drawing.Point(11, 913);
            this.prgBar.Name = "prgBar";
            this.prgBar.Size = new System.Drawing.Size(1196, 21);
            this.prgBar.Step = 1;
            this.prgBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgBar.TabIndex = 0;
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.BackColor = System.Drawing.SystemColors.Window;
            this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtOutput.ContextMenuStrip = this.cmsLog;
            this.txtOutput.HideSelection = false;
            this.txtOutput.Location = new System.Drawing.Point(12, 353);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(933, 543);
            this.txtOutput.TabIndex = 0;
            this.txtOutput.Text = "";
            this.txtOutput.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.txtOutput_LinkClicked);
            // 
            // cmsLog
            // 
            this.cmsLog.AutoSize = false;
            this.cmsLog.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripSeparator,
            this.clearToolStripMenuItem,
            this.clearAndSaveToolStripMenuItem});
            this.cmsLog.Name = "cmsLog";
            this.cmsLog.ShowImageMargin = false;
            this.cmsLog.ShowItemToolTips = false;
            this.cmsLog.Size = new System.Drawing.Size(122, 82);
            this.cmsLog.Text = "Poop";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.AutoSize = false;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(121, 24);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(118, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.AutoSize = false;
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(121, 24);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // clearAndSaveToolStripMenuItem
            // 
            this.clearAndSaveToolStripMenuItem.AutoSize = false;
            this.clearAndSaveToolStripMenuItem.Name = "clearAndSaveToolStripMenuItem";
            this.clearAndSaveToolStripMenuItem.Size = new System.Drawing.Size(121, 24);
            this.clearAndSaveToolStripMenuItem.Text = "Save and Clear";
            this.clearAndSaveToolStripMenuItem.Click += new System.EventHandler(this.clearAndSaveToolStripMenuItem_Click);
            // 
            // lblFiles
            // 
            this.lblFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiles.AutoSize = true;
            this.lblFiles.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFiles.Location = new System.Drawing.Point(958, 878);
            this.lblFiles.Name = "lblFiles";
            this.lblFiles.Size = new System.Drawing.Size(40, 19);
            this.lblFiles.TabIndex = 0;
            this.lblFiles.Text = "Files:";
            this.lblFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAsin
            // 
            this.txtAsin.ActiveLinkColor = System.Drawing.Color.MediumBlue;
            this.txtAsin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAsin.AutoSize = true;
            this.txtAsin.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAsin.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.txtAsin.LinkColor = System.Drawing.Color.MediumBlue;
            this.txtAsin.Location = new System.Drawing.Point(1052, 834);
            this.txtAsin.Name = "txtAsin";
            this.txtAsin.Size = new System.Drawing.Size(39, 19);
            this.txtAsin.TabIndex = 0;
            this.txtAsin.TabStop = true;
            this.txtAsin.Text = "ASIN";
            this.txtAsin.Visible = false;
            this.txtAsin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.txtAsin_LinkClicked);
            // 
            // txtAuthor
            // 
            this.txtAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAuthor.AutoEllipsis = true;
            this.txtAuthor.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAuthor.Location = new System.Drawing.Point(1052, 812);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.Size = new System.Drawing.Size(190, 19);
            this.txtAuthor.TabIndex = 0;
            this.txtAuthor.Text = "Author";
            this.txtAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtAuthor.Visible = false;
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.AutoEllipsis = true;
            this.txtTitle.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitle.Location = new System.Drawing.Point(1052, 790);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(190, 19);
            this.txtTitle.TabIndex = 0;
            this.txtTitle.Text = "Title";
            this.txtTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtTitle.Visible = false;
            // 
            // lblAsin
            // 
            this.lblAsin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAsin.AutoSize = true;
            this.lblAsin.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAsin.Location = new System.Drawing.Point(958, 834);
            this.lblAsin.Name = "lblAsin";
            this.lblAsin.Size = new System.Drawing.Size(44, 19);
            this.lblAsin.TabIndex = 0;
            this.lblAsin.Text = "ASIN:";
            this.lblAsin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAuthor
            // 
            this.lblAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthor.Location = new System.Drawing.Point(958, 812);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(56, 19);
            this.lblAuthor.TabIndex = 0;
            this.lblAuthor.Text = "Author:";
            this.lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(958, 790);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(40, 19);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Title:";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.rdoRoentgen);
            this.groupBox4.Controls.Add(this.rdoFile);
            this.groupBox4.Controls.Add(this.rdoGoodreads);
            this.groupBox4.Controls.Add(this.txtXMLFile);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(12, 263);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1233, 74);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "X-Ray Terms Source";
            // 
            // rdoRoentgen
            // 
            this.rdoRoentgen.AutoSize = true;
            this.rdoRoentgen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoRoentgen.Location = new System.Drawing.Point(131, 29);
            this.rdoRoentgen.Name = "rdoRoentgen";
            this.rdoRoentgen.Size = new System.Drawing.Size(94, 24);
            this.rdoRoentgen.TabIndex = 0;
            this.rdoRoentgen.Text = "Roentgen";
            this.rdoRoentgen.UseVisualStyleBackColor = true;
            this.rdoRoentgen.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // rdoFile
            // 
            this.rdoFile.AutoSize = true;
            this.rdoFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoFile.Location = new System.Drawing.Point(234, 29);
            this.rdoFile.Name = "rdoFile";
            this.rdoFile.Size = new System.Drawing.Size(53, 24);
            this.rdoFile.TabIndex = 0;
            this.rdoFile.Text = "File";
            this.rdoFile.UseVisualStyleBackColor = true;
            this.rdoFile.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // rdoGoodreads
            // 
            this.rdoGoodreads.AutoSize = true;
            this.rdoGoodreads.Checked = true;
            this.rdoGoodreads.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoGoodreads.Location = new System.Drawing.Point(18, 29);
            this.rdoGoodreads.Name = "rdoGoodreads";
            this.rdoGoodreads.Size = new System.Drawing.Size(103, 24);
            this.rdoGoodreads.TabIndex = 0;
            this.rdoGoodreads.TabStop = true;
            this.rdoGoodreads.Text = "Goodreads";
            this.rdoGoodreads.UseVisualStyleBackColor = true;
            this.rdoGoodreads.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // txtXMLFile
            // 
            this.txtXMLFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtXMLFile.Enabled = false;
            this.txtXMLFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtXMLFile.Location = new System.Drawing.Point(297, 28);
            this.txtXMLFile.Name = "txtXMLFile";
            this.txtXMLFile.Size = new System.Drawing.Size(918, 27);
            this.txtXMLFile.TabIndex = 0;
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnBrowseMobi,
            this.btnSearchGoodreads,
            this.toolStripSeparator2,
            this.btnKindleExtras,
            this.btnBuild,
            this.btnOneClick,
            this.toolStripSeparator5,
            this.btnXraySource,
            this.btnCreate,
            this.btnDownloadTerms,
            this.btnBrowseXML,
            this.btnExtractTerms,
            this.toolStripSeparator1,
            this.btnPreview,
            this.btnUnpack,
            this.btnBrowseFolders,
            this.btnSettings,
            this.toolStripSeparator3,
            this.btnHelp,
            this.btnAbout});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1262, 100);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // btnBrowseMobi
            // 
            this.btnBrowseMobi.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseMobi.Image")));
            this.btnBrowseMobi.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBrowseMobi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBrowseMobi.Margin = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.btnBrowseMobi.Name = "btnBrowseMobi";
            this.btnBrowseMobi.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnBrowseMobi.Size = new System.Drawing.Size(91, 80);
            this.btnBrowseMobi.Text = "Open Book";
            this.btnBrowseMobi.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnBrowseMobi.Click += new System.EventHandler(this.btnBrowseMobi_Click);
            // 
            // btnSearchGoodreads
            // 
            this.btnSearchGoodreads.Image = ((System.Drawing.Image)(resources.GetObject("btnSearchGoodreads.Image")));
            this.btnSearchGoodreads.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSearchGoodreads.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSearchGoodreads.Margin = new System.Windows.Forms.Padding(5, 10, 7, 10);
            this.btnSearchGoodreads.Name = "btnSearchGoodreads";
            this.btnSearchGoodreads.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnSearchGoodreads.Size = new System.Drawing.Size(99, 80);
            this.btnSearchGoodreads.Text = "Search Book";
            this.btnSearchGoodreads.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSearchGoodreads.Click += new System.EventHandler(this.btnSearchGoodreads_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 100);
            // 
            // btnKindleExtras
            // 
            this.btnKindleExtras.Image = ((System.Drawing.Image)(resources.GetObject("btnKindleExtras.Image")));
            this.btnKindleExtras.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnKindleExtras.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnKindleExtras.Margin = new System.Windows.Forms.Padding(8, 10, 5, 10);
            this.btnKindleExtras.Name = "btnKindleExtras";
            this.btnKindleExtras.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnKindleExtras.Size = new System.Drawing.Size(102, 80);
            this.btnKindleExtras.Text = "Kindle Extras";
            this.btnKindleExtras.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnKindleExtras.Click += new System.EventHandler(this.btnKindleExtras_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 100);
            // 
            // btnBuild
            // 
            this.btnBuild.Image = ((System.Drawing.Image)(resources.GetObject("btnBuild.Image")));
            this.btnBuild.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBuild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBuild.Margin = new System.Windows.Forms.Padding(0, 10, 5, 10);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnBuild.Size = new System.Drawing.Size(56, 80);
            this.btnBuild.Text = "X-Ray";
            this.btnBuild.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // btnXraySource
            // 
            this.btnXraySource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnXraySourceFile,
            this.btnXraySourceGoodreads,
            this.btnXraySourceRoentgen});
            this.btnXraySource.Image = ((System.Drawing.Image)(resources.GetObject("btnXraySource.Image")));
            this.btnXraySource.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnXraySource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnXraySource.Margin = new System.Windows.Forms.Padding(8, 10, 5, 10);
            this.btnXraySource.Name = "btnXraySource";
            this.btnXraySource.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnXraySource.Size = new System.Drawing.Size(72, 80);
            this.btnXraySource.Text = "Source";
            this.btnXraySource.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // btnXraySourceFile
            // 
            this.btnXraySourceFile.Name = "btnXraySourceFile";
            this.btnXraySourceFile.Size = new System.Drawing.Size(224, 26);
            this.btnXraySourceFile.Text = "File";
            // 
            // btnXraySourceGoodreads
            // 
            this.btnXraySourceGoodreads.Name = "btnXraySourceGoodreads";
            this.btnXraySourceGoodreads.Size = new System.Drawing.Size(224, 26);
            this.btnXraySourceGoodreads.Text = "Goodreads";
            // 
            // btnXraySourceRoentgen
            // 
            this.btnXraySourceRoentgen.Name = "btnXraySourceRoentgen";
            this.btnXraySourceRoentgen.Size = new System.Drawing.Size(224, 26);
            this.btnXraySourceRoentgen.Text = "Roentgen";
            // 
            // btnCreate
            // 
            this.btnCreate.Image = ((System.Drawing.Image)(resources.GetObject("btnCreate.Image")));
            this.btnCreate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnCreate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCreate.Margin = new System.Windows.Forms.Padding(0, 10, 5, 10);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnCreate.Size = new System.Drawing.Size(60, 80);
            this.btnCreate.Text = "Create";
            this.btnCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnDownloadTerms
            // 
            this.btnDownloadTerms.Image = ((System.Drawing.Image)(resources.GetObject("btnDownloadTerms.Image")));
            this.btnDownloadTerms.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDownloadTerms.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDownloadTerms.Margin = new System.Windows.Forms.Padding(0, 10, 5, 10);
            this.btnDownloadTerms.Name = "btnDownloadTerms";
            this.btnDownloadTerms.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnDownloadTerms.Size = new System.Drawing.Size(86, 80);
            this.btnDownloadTerms.Text = "Download";
            this.btnDownloadTerms.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnDownloadTerms.Click += new System.EventHandler(this.btnDownloadTerms_Click);
            // 
            // btnBrowseXML
            // 
            this.btnBrowseXML.AutoSize = false;
            this.btnBrowseXML.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseXML.Image")));
            this.btnBrowseXML.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBrowseXML.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBrowseXML.Margin = new System.Windows.Forms.Padding(0, 10, 5, 10);
            this.btnBrowseXML.Name = "btnBrowseXML";
            this.btnBrowseXML.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnBrowseXML.Size = new System.Drawing.Size(86, 80);
            this.btnBrowseXML.Text = "Open File";
            this.btnBrowseXML.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnBrowseXML.Click += new System.EventHandler(this.btnBrowseXML_Click);
            // 
            // btnExtractTerms
            // 
            this.btnExtractTerms.Image = ((System.Drawing.Image)(resources.GetObject("btnExtractTerms.Image")));
            this.btnExtractTerms.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnExtractTerms.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExtractTerms.Margin = new System.Windows.Forms.Padding(0, 10, 7, 10);
            this.btnExtractTerms.Name = "btnExtractTerms";
            this.btnExtractTerms.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnExtractTerms.Size = new System.Drawing.Size(62, 80);
            this.btnExtractTerms.Text = "Extract";
            this.btnExtractTerms.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnExtractTerms.Click += new System.EventHandler(this.btnExtractTerms_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 100);
            // 
            // btnBrowseFolders
            // 
            this.btnBrowseFolders.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnBrowseOutput,
            this.toolStripSeparator4,
            this.btnBrowseDump,
            this.btnBrowseGeneratedData,
            this.btnBrowseLogs,
            this.btnBrowseTemp,
            this.btnBrowseXmlFolder});
            this.btnBrowseFolders.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseFolders.Image")));
            this.btnBrowseFolders.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBrowseFolders.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBrowseFolders.Margin = new System.Windows.Forms.Padding(0, 10, 5, 10);
            this.btnBrowseFolders.Name = "btnBrowseFolders";
            this.btnBrowseFolders.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnBrowseFolders.Size = new System.Drawing.Size(75, 80);
            this.btnBrowseFolders.Text = "Folders";
            this.btnBrowseFolders.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(224, 26);
            this.btnBrowseOutput.Text = "Output";
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(221, 6);
            // 
            // btnBrowseDump
            // 
            this.btnBrowseDump.Name = "btnBrowseDump";
            this.btnBrowseDump.Size = new System.Drawing.Size(224, 26);
            this.btnBrowseDump.Text = "Dump";
            this.btnBrowseDump.Click += new System.EventHandler(this.btnBrowseDump_Click);
            // 
            // btnBrowseGeneratedData
            // 
            this.btnBrowseGeneratedData.Name = "btnBrowseGeneratedData";
            this.btnBrowseGeneratedData.Size = new System.Drawing.Size(224, 26);
            this.btnBrowseGeneratedData.Text = "Generated Data";
            this.btnBrowseGeneratedData.Click += new System.EventHandler(this.btnBrowseGeneratedData_Click);
            // 
            // btnBrowseLogs
            // 
            this.btnBrowseLogs.Name = "btnBrowseLogs";
            this.btnBrowseLogs.Size = new System.Drawing.Size(224, 26);
            this.btnBrowseLogs.Text = "Logs";
            this.btnBrowseLogs.Click += new System.EventHandler(this.btnBrowseLogs_Click);
            // 
            // btnBrowseTemp
            // 
            this.btnBrowseTemp.Name = "btnBrowseTemp";
            this.btnBrowseTemp.Size = new System.Drawing.Size(224, 26);
            this.btnBrowseTemp.Text = "Temp";
            this.btnBrowseTemp.Click += new System.EventHandler(this.btnBrowseTemp_Click);
            // 
            // btnBrowseXmlFolder
            // 
            this.btnBrowseXmlFolder.Name = "btnBrowseXmlFolder";
            this.btnBrowseXmlFolder.Size = new System.Drawing.Size(224, 26);
            this.btnBrowseXmlFolder.Text = "XML";
            this.btnBrowseXmlFolder.Click += new System.EventHandler(this.btnBrowseXmlFolder_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnSettings.Image")));
            this.btnSettings.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettings.Margin = new System.Windows.Forms.Padding(0, 10, 7, 10);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnSettings.Size = new System.Drawing.Size(70, 80);
            this.btnSettings.Text = "Settings";
            this.btnSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnOneClick
            // 
            this.btnOneClick.Image = ((System.Drawing.Image)(resources.GetObject("btnOneClick.Image")));
            this.btnOneClick.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOneClick.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOneClick.Margin = new System.Windows.Forms.Padding(0, 10, 7, 10);
            this.btnOneClick.Name = "btnOneClick";
            this.btnOneClick.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnOneClick.Size = new System.Drawing.Size(79, 80);
            this.btnOneClick.Text = "One Click";
            this.btnOneClick.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnOneClick.Click += new System.EventHandler(this.btnOneClick_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 100);
            // 
            // btnHelp
            // 
            this.btnHelp.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp.Image")));
            this.btnHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHelp.Margin = new System.Windows.Forms.Padding(8, 10, 5, 10);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnHelp.Size = new System.Drawing.Size(57, 64);
            this.btnHelp.Text = "Help";
            this.btnHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Image = ((System.Drawing.Image)(resources.GetObject("btnAbout.Image")));
            this.btnAbout.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAbout.Margin = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnAbout.Size = new System.Drawing.Size(58, 64);
            this.btnAbout.Text = "About";
            this.btnAbout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // pbFile4
            // 
            this.pbFile4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbFile4.Image = ((System.Drawing.Image)(resources.GetObject("pbFile4.Image")));
            this.pbFile4.Location = new System.Drawing.Point(1134, 880);
            this.pbFile4.Name = "pbFile4";
            this.pbFile4.Size = new System.Drawing.Size(16, 16);
            this.pbFile4.TabIndex = 0;
            this.pbFile4.TabStop = false;
            // 
            // pbFile3
            // 
            this.pbFile3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbFile3.Image = ((System.Drawing.Image)(resources.GetObject("pbFile3.Image")));
            this.pbFile3.Location = new System.Drawing.Point(1108, 880);
            this.pbFile3.Name = "pbFile3";
            this.pbFile3.Size = new System.Drawing.Size(16, 16);
            this.pbFile3.TabIndex = 0;
            this.pbFile3.TabStop = false;
            // 
            // pbFile2
            // 
            this.pbFile2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbFile2.Image = ((System.Drawing.Image)(resources.GetObject("pbFile2.Image")));
            this.pbFile2.Location = new System.Drawing.Point(1082, 880);
            this.pbFile2.Name = "pbFile2";
            this.pbFile2.Size = new System.Drawing.Size(16, 16);
            this.pbFile2.TabIndex = 0;
            this.pbFile2.TabStop = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Enabled = false;
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.Location = new System.Drawing.Point(1223, 912);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(23, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pbFile1
            // 
            this.pbFile1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbFile1.Image = ((System.Drawing.Image)(resources.GetObject("pbFile1.Image")));
            this.pbFile1.Location = new System.Drawing.Point(1056, 880);
            this.pbFile1.Name = "pbFile1";
            this.pbFile1.Size = new System.Drawing.Size(16, 16);
            this.pbFile1.TabIndex = 0;
            this.pbFile1.TabStop = false;
            // 
            // pbCover
            // 
            this.pbCover.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCover.Image = ((System.Drawing.Image)(resources.GetObject("pbCover.Image")));
            this.pbCover.Location = new System.Drawing.Point(962, 353);
            this.pbCover.Name = "pbCover";
            this.pbCover.Size = new System.Drawing.Size(283, 425);
            this.pbCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCover.TabIndex = 0;
            this.pbCover.TabStop = false;
            this.pbCover.Click += new System.EventHandler(this.pbCover_Click);
            // 
            // txtDatasource
            // 
            this.txtDatasource.ActiveLinkColor = System.Drawing.Color.MediumBlue;
            this.txtDatasource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDatasource.AutoSize = true;
            this.txtDatasource.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDatasource.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.txtDatasource.LinkColor = System.Drawing.Color.MediumBlue;
            this.txtDatasource.Location = new System.Drawing.Point(1052, 856);
            this.txtDatasource.Name = "txtDatasource";
            this.txtDatasource.Size = new System.Drawing.Size(130, 19);
            this.txtDatasource.TabIndex = 0;
            this.txtDatasource.TabStop = true;
            this.txtDatasource.Text = "Search datasource…";
            this.txtDatasource.Visible = false;
            this.txtDatasource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.txtDatasource_LinkClicked);
            // 
            // lblDatasource
            // 
            this.lblDatasource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDatasource.AutoSize = true;
            this.lblDatasource.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDatasource.Location = new System.Drawing.Point(958, 856);
            this.lblDatasource.Name = "lblDatasource";
            this.lblDatasource.Size = new System.Drawing.Size(82, 19);
            this.lblDatasource.TabIndex = 0;
            this.lblDatasource.Text = "Datasource:";
            this.lblDatasource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnUnpack
            // 
            this.btnUnpack.Image = ((System.Drawing.Image)(resources.GetObject("btnUnpack.Image")));
            this.btnUnpack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnUnpack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUnpack.Margin = new System.Windows.Forms.Padding(0, 10, 5, 10);
            this.btnUnpack.Name = "btnUnpack";
            this.btnUnpack.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnUnpack.Size = new System.Drawing.Size(66, 80);
            this.btnUnpack.Text = "Unpack";
            this.btnUnpack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnUnpack.Click += new System.EventHandler(this.btnUnpack_Click);
            // 
            // tmiAuthorProfile
            // 
            this.tmiAuthorProfile.Name = "tmiAuthorProfile";
            this.tmiAuthorProfile.Size = new System.Drawing.Size(224, 26);
            this.tmiAuthorProfile.Text = "Author Profile";
            this.tmiAuthorProfile.Click += new System.EventHandler(this.tmiAuthorProfile_Click);
            // 
            // tmiEndAction
            // 
            this.tmiEndAction.Name = "tmiEndAction";
            this.tmiEndAction.Size = new System.Drawing.Size(224, 26);
            this.tmiEndAction.Text = "End Actions";
            this.tmiEndAction.Click += new System.EventHandler(this.tmiEndAction_Click);
            // 
            // tmiStartAction
            // 
            this.tmiStartAction.Name = "tmiStartAction";
            this.tmiStartAction.Size = new System.Drawing.Size(224, 26);
            this.tmiStartAction.Text = "Start Actions";
            this.tmiStartAction.Click += new System.EventHandler(this.tmiStartAction_Click);
            // 
            // tmiXray
            // 
            this.tmiXray.Name = "tmiXray";
            this.tmiXray.Size = new System.Drawing.Size(224, 26);
            this.tmiXray.Text = "X-Ray";
            this.tmiXray.Click += new System.EventHandler(this.tmiXray_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tmiAuthorProfile,
            this.tmiEndAction,
            this.tmiStartAction,
            this.tmiXray});
            this.btnPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnPreview.Image")));
            this.btnPreview.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreview.Margin = new System.Windows.Forms.Padding(8, 10, 5, 10);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnPreview.Size = new System.Drawing.Size(78, 80);
            this.btnPreview.Text = "Preview";
            this.btnPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 949);
            this.Controls.Add(this.txtDatasource);
            this.Controls.Add(this.lblDatasource);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.pbFile4);
            this.Controls.Add(this.pbFile3);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.pbFile2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pbFile1);
            this.Controls.Add(this.lblFiles);
            this.Controls.Add(this.txtAsin);
            this.Controls.Add(this.txtAuthor);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.lblAsin);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.pbCover);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.prgBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(1262, 900);
            this.Name = "frmMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "X-Ray Builder GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.cmsLog.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblAsin;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblFiles;
        private System.Windows.Forms.Label lblGoodreads;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox pbCover;
        private System.Windows.Forms.PictureBox pbFile1;
        private System.Windows.Forms.PictureBox pbFile2;
        private System.Windows.Forms.PictureBox pbFile3;
        private System.Windows.Forms.PictureBox pbFile4;
        private System.Windows.Forms.ProgressBar prgBar;
        private System.Windows.Forms.RadioButton rdoFile;
        private System.Windows.Forms.RadioButton rdoGoodreads;
        private System.Windows.Forms.RadioButton rdoRoentgen;
        private System.Windows.Forms.LinkLabel txtAsin;
        private System.Windows.Forms.Label txtAuthor;
        private System.Windows.Forms.TextBox txtGoodreads;
        private System.Windows.Forms.TextBox txtMobi;
        private System.Windows.Forms.RichTextBox txtOutput;
        private System.Windows.Forms.Label txtTitle;
        private System.Windows.Forms.TextBox txtXMLFile;

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnBrowseMobi;
        private System.Windows.Forms.ToolStripButton btnKindleExtras;
        private System.Windows.Forms.ToolStripButton btnBuild;
        private System.Windows.Forms.ToolStripButton btnOneClick;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnCreate;
        private System.Windows.Forms.ToolStripButton btnDownloadTerms;
        private System.Windows.Forms.ToolStripButton btnBrowseXML;
        private System.Windows.Forms.ToolStripButton btnExtractTerms;
        private System.Windows.Forms.ToolStripDropDownButton btnXraySource;
        private System.Windows.Forms.ToolStripMenuItem btnXraySourceFile;
        private System.Windows.Forms.ToolStripMenuItem btnXraySourceGoodreads;
        private System.Windows.Forms.ToolStripMenuItem btnXraySourceRoentgen;
        private System.Windows.Forms.ToolStripDropDownButton btnBrowseFolders;
        private System.Windows.Forms.ToolStripMenuItem btnBrowseOutput;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem btnBrowseDump;
        private System.Windows.Forms.ToolStripMenuItem btnBrowseGeneratedData;
        private System.Windows.Forms.ToolStripMenuItem btnBrowseLogs;
        private System.Windows.Forms.ToolStripMenuItem btnBrowseTemp;
        private System.Windows.Forms.ToolStripMenuItem btnBrowseXmlFolder;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnHelp;
        private System.Windows.Forms.ToolStripButton btnAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnSearchGoodreads;
        private System.Windows.Forms.ContextMenuStrip cmsLog;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAndSaveToolStripMenuItem;
        private System.Windows.Forms.LinkLabel txtDatasource;
        private System.Windows.Forms.Label lblDatasource;
        private System.Windows.Forms.ToolStripDropDownButton btnPreview;
        private System.Windows.Forms.ToolStripMenuItem tmiAuthorProfile;
        private System.Windows.Forms.ToolStripMenuItem tmiEndAction;
        private System.Windows.Forms.ToolStripMenuItem tmiStartAction;
        private System.Windows.Forms.ToolStripMenuItem tmiXray;
        private System.Windows.Forms.ToolStripButton btnUnpack;
    }
}

