namespace XRayBuilderGUI
{
    partial class frmGR
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGR));
            this.cbResults = new System.Windows.Forms.ComboBox();
            this.lblMessage1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblMessage2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbResults
            // 
            this.cbResults.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbResults.FormattingEnabled = true;
            this.cbResults.IntegralHeight = false;
            this.cbResults.Location = new System.Drawing.Point(12, 12);
            this.cbResults.Name = "cbResults";
            this.cbResults.Size = new System.Drawing.Size(310, 21);
            this.cbResults.TabIndex = 3;
            // 
            // lblMessage1
            // 
            this.lblMessage1.AutoEllipsis = true;
            this.lblMessage1.AutoSize = true;
            this.lblMessage1.Location = new System.Drawing.Point(9, 38);
            this.lblMessage1.Name = "lblMessage1";
            this.lblMessage1.Size = new System.Drawing.Size(315, 13);
            this.lblMessage1.TabIndex = 4;
            this.lblMessage1.Text = "Multiple title matches were returned from Goodreads for this book.";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(248, 58);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblMessage2
            // 
            this.lblMessage2.AutoEllipsis = true;
            this.lblMessage2.AutoSize = true;
            this.lblMessage2.Location = new System.Drawing.Point(9, 55);
            this.lblMessage2.Name = "lblMessage2";
            this.lblMessage2.Size = new System.Drawing.Size(173, 13);
            this.lblMessage2.TabIndex = 6;
            this.lblMessage2.Text = "Which book would you like to use?";
            // 
            // frmGR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 92);
            this.Controls.Add(this.lblMessage2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMessage1);
            this.Controls.Add(this.cbResults);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmGR";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Goodread Results";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Label lblMessage1;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.ComboBox cbResults;
        public System.Windows.Forms.Label lblMessage2;
    }
}