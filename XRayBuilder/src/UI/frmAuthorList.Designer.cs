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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAuthorList));
            this.lblMessage2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblMessage1 = new System.Windows.Forms.Label();
            this.cbResults = new System.Windows.Forms.ComboBox();
            this.linkStore = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
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
            // 
            // linkStore
            // 
            this.linkStore.ActiveLinkColor = System.Drawing.Color.RoyalBlue;
            this.linkStore.AutoSize = true;
            this.linkStore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.linkStore.Location = new System.Drawing.Point(302, 63);
            this.linkStore.Name = "linkStore";
            this.linkStore.Size = new System.Drawing.Size(67, 15);
            this.linkStore.TabIndex = 86;
            this.linkStore.TabStop = true;
            this.linkStore.Text = "See in store";
            this.linkStore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkStore.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkStore_LinkClicked);
            // 
            // frmAuthorList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 89);
            this.Controls.Add(this.linkStore);
            this.Controls.Add(this.lblMessage2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMessage1);
            this.Controls.Add(this.cbResults);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAuthorList";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Author Search Results";
            this.Load += new System.EventHandler(this.frmAuthorList_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Label lblMessage2;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.Label lblMessage1;
        public System.Windows.Forms.ComboBox cbResults;
        private System.Windows.Forms.LinkLabel linkStore;
    }
}