namespace XRayBuilderGUI
{
    partial class frmXRay
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
            this.lstTerms = new System.Windows.Forms.ListBox();
            this.txtExcerpt = new System.Windows.Forms.RichTextBox();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lstTerms
            // 
            this.lstTerms.FormattingEnabled = true;
            this.lstTerms.Location = new System.Drawing.Point(0, 0);
            this.lstTerms.Name = "lstTerms";
            this.lstTerms.Size = new System.Drawing.Size(101, 277);
            this.lstTerms.TabIndex = 0;
            this.lstTerms.SelectedIndexChanged += new System.EventHandler(this.lstTerms_SelectedIndexChanged);
            // 
            // txtExcerpt
            // 
            this.txtExcerpt.Location = new System.Drawing.Point(102, 114);
            this.txtExcerpt.Name = "txtExcerpt";
            this.txtExcerpt.ReadOnly = true;
            this.txtExcerpt.Size = new System.Drawing.Size(265, 163);
            this.txtExcerpt.TabIndex = 1;
            this.txtExcerpt.Text = "";
            // 
            // txtDesc
            // 
            this.txtDesc.Location = new System.Drawing.Point(102, 1);
            this.txtDesc.Multiline = true;
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.ReadOnly = true;
            this.txtDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDesc.Size = new System.Drawing.Size(265, 107);
            this.txtDesc.TabIndex = 2;
            // 
            // frmXRay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 278);
            this.Controls.Add(this.txtDesc);
            this.Controls.Add(this.txtExcerpt);
            this.Controls.Add(this.lstTerms);
            this.Name = "frmXRay";
            this.Text = "frmXRay";
            this.Load += new System.EventHandler(this.frmXRay_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstTerms;
        private System.Windows.Forms.RichTextBox txtExcerpt;
        private System.Windows.Forms.TextBox txtDesc;
    }
}