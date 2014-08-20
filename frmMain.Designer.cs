namespace XRayBuilderGUI
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
            this.txtShelfari = new System.Windows.Forms.TextBox();
            this.lblShelfari = new System.Windows.Forms.Label();
            this.btnBuild = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMobi = new System.Windows.Forms.TextBox();
            this.btnBrowseMobi = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.prgBar = new System.Windows.Forms.ProgressBar();
            this.btnSaveShelfari = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdoFile = new System.Windows.Forms.RadioButton();
            this.rdoShelfari = new System.Windows.Forms.RadioButton();
            this.txtXMLFile = new System.Windows.Forms.TextBox();
            this.lblXMLFile = new System.Windows.Forms.Label();
            this.btnBrowseXML = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtShelfari
            // 
            this.txtShelfari.Location = new System.Drawing.Point(128, 35);
            this.txtShelfari.Name = "txtShelfari";
            this.txtShelfari.Size = new System.Drawing.Size(260, 20);
            this.txtShelfari.TabIndex = 9;
            // 
            // lblShelfari
            // 
            this.lblShelfari.AutoSize = true;
            this.lblShelfari.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShelfari.Location = new System.Drawing.Point(36, 36);
            this.lblShelfari.Name = "lblShelfari";
            this.lblShelfari.Size = new System.Drawing.Size(86, 16);
            this.lblShelfari.TabIndex = 8;
            this.lblShelfari.Text = "Shelfari URL:";
            // 
            // btnBuild
            // 
            this.btnBuild.Location = new System.Drawing.Point(95, 69);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(105, 23);
            this.btnBuild.TabIndex = 14;
            this.btnBuild.Text = "Build X-Ray";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(79, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Book:";
            // 
            // txtMobi
            // 
            this.txtMobi.Location = new System.Drawing.Point(128, 9);
            this.txtMobi.Name = "txtMobi";
            this.txtMobi.Size = new System.Drawing.Size(260, 20);
            this.txtMobi.TabIndex = 1;
            // 
            // btnBrowseMobi
            // 
            this.btnBrowseMobi.Location = new System.Drawing.Point(394, 9);
            this.btnBrowseMobi.Name = "btnBrowseMobi";
            this.btnBrowseMobi.Size = new System.Drawing.Size(75, 20);
            this.btnBrowseMobi.TabIndex = 10;
            this.btnBrowseMobi.Text = "Browse...";
            this.btnBrowseMobi.UseVisualStyleBackColor = true;
            this.btnBrowseMobi.Click += new System.EventHandler(this.btnBrowseMobi_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(317, 69);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 16;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(12, 110);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(461, 280);
            this.txtOutput.TabIndex = 17;
            // 
            // prgBar
            // 
            this.prgBar.Location = new System.Drawing.Point(22, 396);
            this.prgBar.Name = "prgBar";
            this.prgBar.Size = new System.Drawing.Size(447, 10);
            this.prgBar.Step = 1;
            this.prgBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgBar.TabIndex = 18;
            // 
            // btnSaveShelfari
            // 
            this.btnSaveShelfari.Location = new System.Drawing.Point(206, 69);
            this.btnSaveShelfari.Name = "btnSaveShelfari";
            this.btnSaveShelfari.Size = new System.Drawing.Size(105, 23);
            this.btnSaveShelfari.TabIndex = 19;
            this.btnSaveShelfari.Text = "Save Shelfari Info";
            this.btnSaveShelfari.UseVisualStyleBackColor = true;
            this.btnSaveShelfari.Click += new System.EventHandler(this.btnShelfari_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoFile);
            this.groupBox1.Controls.Add(this.rdoShelfari);
            this.groupBox1.Location = new System.Drawing.Point(12, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(77, 49);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // rdoFile
            // 
            this.rdoFile.AutoSize = true;
            this.rdoFile.Location = new System.Drawing.Point(9, 27);
            this.rdoFile.Name = "rdoFile";
            this.rdoFile.Size = new System.Drawing.Size(66, 17);
            this.rdoFile.TabIndex = 1;
            this.rdoFile.TabStop = true;
            this.rdoFile.Text = "XML File";
            this.rdoFile.UseVisualStyleBackColor = true;
            this.rdoFile.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // rdoShelfari
            // 
            this.rdoShelfari.AutoSize = true;
            this.rdoShelfari.Checked = true;
            this.rdoShelfari.Location = new System.Drawing.Point(9, 11);
            this.rdoShelfari.Name = "rdoShelfari";
            this.rdoShelfari.Size = new System.Drawing.Size(60, 17);
            this.rdoShelfari.TabIndex = 0;
            this.rdoShelfari.TabStop = true;
            this.rdoShelfari.Text = "Shelfari";
            this.rdoShelfari.UseVisualStyleBackColor = true;
            this.rdoShelfari.CheckedChanged += new System.EventHandler(this.rdoSource_CheckedChanged);
            // 
            // txtXMLFile
            // 
            this.txtXMLFile.Location = new System.Drawing.Point(128, 35);
            this.txtXMLFile.Name = "txtXMLFile";
            this.txtXMLFile.Size = new System.Drawing.Size(260, 20);
            this.txtXMLFile.TabIndex = 22;
            this.txtXMLFile.Visible = false;
            // 
            // lblXMLFile
            // 
            this.lblXMLFile.AutoSize = true;
            this.lblXMLFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblXMLFile.Location = new System.Drawing.Point(60, 36);
            this.lblXMLFile.Name = "lblXMLFile";
            this.lblXMLFile.Size = new System.Drawing.Size(62, 16);
            this.lblXMLFile.TabIndex = 21;
            this.lblXMLFile.Text = "XML File:";
            this.lblXMLFile.Visible = false;
            // 
            // btnBrowseXML
            // 
            this.btnBrowseXML.Location = new System.Drawing.Point(394, 35);
            this.btnBrowseXML.Name = "btnBrowseXML";
            this.btnBrowseXML.Size = new System.Drawing.Size(75, 20);
            this.btnBrowseXML.TabIndex = 23;
            this.btnBrowseXML.Text = "Browse...";
            this.btnBrowseXML.UseVisualStyleBackColor = true;
            this.btnBrowseXML.Visible = false;
            this.btnBrowseXML.Click += new System.EventHandler(this.btnBrowseXML_Click);
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 413);
            this.Controls.Add(this.btnBrowseXML);
            this.Controls.Add(this.txtXMLFile);
            this.Controls.Add(this.lblXMLFile);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSaveShelfari);
            this.Controls.Add(this.prgBar);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.btnBrowseMobi);
            this.Controls.Add(this.txtShelfari);
            this.Controls.Add(this.lblShelfari);
            this.Controls.Add(this.txtMobi);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmMain";
            this.Text = "X-Ray Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtShelfari;
        private System.Windows.Forms.Label lblShelfari;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMobi;
        private System.Windows.Forms.Button btnBrowseMobi;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.TextBox txtOutput;
        public System.Windows.Forms.ProgressBar prgBar;
        private System.Windows.Forms.Button btnSaveShelfari;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdoShelfari;
        private System.Windows.Forms.RadioButton rdoFile;
        private System.Windows.Forms.TextBox txtXMLFile;
        private System.Windows.Forms.Label lblXMLFile;
        private System.Windows.Forms.Button btnBrowseXML;
    }
}

