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
            this.label5 = new System.Windows.Forms.Label();
            this.btnBuild = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMobi = new System.Windows.Forms.TextBox();
            this.btnBrowseMobi = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.prgBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // txtShelfari
            // 
            this.txtShelfari.Location = new System.Drawing.Point(128, 35);
            this.txtShelfari.Name = "txtShelfari";
            this.txtShelfari.Size = new System.Drawing.Size(260, 20);
            this.txtShelfari.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(36, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "Shelfari URL:";
            // 
            // btnBuild
            // 
            this.btnBuild.Location = new System.Drawing.Point(166, 61);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(75, 23);
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
            this.btnSettings.Location = new System.Drawing.Point(247, 61);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 16;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(12, 90);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(461, 278);
            this.txtOutput.TabIndex = 17;
            // 
            // prgBar
            // 
            this.prgBar.Location = new System.Drawing.Point(22, 374);
            this.prgBar.Name = "prgBar";
            this.prgBar.Size = new System.Drawing.Size(447, 10);
            this.prgBar.Step = 1;
            this.prgBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgBar.TabIndex = 18;
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 393);
            this.Controls.Add(this.prgBar);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.btnBrowseMobi);
            this.Controls.Add(this.txtShelfari);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtMobi);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmMain";
            this.Text = "X-Ray Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtShelfari;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMobi;
        private System.Windows.Forms.Button btnBrowseMobi;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.TextBox txtOutput;
        public System.Windows.Forms.ProgressBar prgBar;
    }
}

