namespace XRayBuilderGUI
{
    partial class frmSeries
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSeries));
            this.btnSave = new System.Windows.Forms.Button();
            this.cbSeriesList = new System.Windows.Forms.ComboBox();
            this.gbResults = new System.Windows.Forms.GroupBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.gbResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 100);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(270, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "OK";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cbSeriesList
            // 
            this.cbSeriesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSeriesList.FormattingEnabled = true;
            this.cbSeriesList.Location = new System.Drawing.Point(13, 19);
            this.cbSeriesList.Name = "cbSeriesList";
            this.cbSeriesList.Size = new System.Drawing.Size(244, 21);
            this.cbSeriesList.TabIndex = 2;
            // 
            // gbResults
            // 
            this.gbResults.Controls.Add(this.lblMessage);
            this.gbResults.Controls.Add(this.cbSeriesList);
            this.gbResults.Location = new System.Drawing.Point(12, 12);
            this.gbResults.Name = "gbResults";
            this.gbResults.Size = new System.Drawing.Size(270, 82);
            this.gbResults.TabIndex = 3;
            this.gbResults.TabStop = false;
            this.gbResults.Text = "Results";
            // 
            // lblMessage
            // 
            this.lblMessage.Location = new System.Drawing.Point(13, 43);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(244, 36);
            this.lblMessage.TabIndex = 4;
            this.lblMessage.Text = "This book was found to belong to multiple series.\r\nWhich series would you like to" +
    " use?";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmSeries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 142);
            this.Controls.Add(this.gbResults);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSeries";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Series & Lists";
            this.Load += new System.EventHandler(this.frmSeries_Load);
            this.gbResults.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox gbResults;
        private System.Windows.Forms.Label lblMessage;
        public System.Windows.Forms.ComboBox cbSeriesList;
    }
}