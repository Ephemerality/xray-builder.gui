namespace XRayBuilderGUI
{
    partial class frmSettings
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
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBrowseUnpack = new System.Windows.Forms.Button();
            this.txtUnpack = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkRaw = new System.Windows.Forms.CheckBox();
            this.chkSpoilers = new System.Windows.Forms.CheckBox();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.lblOffset = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(181, 125);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Location = new System.Drawing.Point(386, 12);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(75, 20);
            this.btnBrowseOut.TabIndex = 13;
            this.btnBrowseOut.Text = "Browse...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // txtOut
            // 
            this.txtOut.Location = new System.Drawing.Point(120, 12);
            this.txtOut.Name = "txtOut";
            this.txtOut.Size = new System.Drawing.Size(260, 20);
            this.txtOut.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "Output Directory:";
            // 
            // btnBrowseUnpack
            // 
            this.btnBrowseUnpack.Location = new System.Drawing.Point(386, 38);
            this.btnBrowseUnpack.Name = "btnBrowseUnpack";
            this.btnBrowseUnpack.Size = new System.Drawing.Size(75, 20);
            this.btnBrowseUnpack.TabIndex = 16;
            this.btnBrowseUnpack.Text = "Browse...";
            this.btnBrowseUnpack.UseVisualStyleBackColor = true;
            this.btnBrowseUnpack.Click += new System.EventHandler(this.btnBrowseUnpack_Click);
            // 
            // txtUnpack
            // 
            this.txtUnpack.Location = new System.Drawing.Point(120, 38);
            this.txtUnpack.Name = "txtUnpack";
            this.txtUnpack.Size = new System.Drawing.Size(260, 20);
            this.txtUnpack.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(20, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 16);
            this.label2.TabIndex = 14;
            this.label2.Text = "Kindle Unpack:";
            // 
            // chkRaw
            // 
            this.chkRaw.AutoSize = true;
            this.chkRaw.Location = new System.Drawing.Point(15, 98);
            this.chkRaw.Name = "chkRaw";
            this.chkRaw.Size = new System.Drawing.Size(95, 17);
            this.chkRaw.TabIndex = 17;
            this.chkRaw.Text = "Save RAWML";
            this.chkRaw.UseVisualStyleBackColor = true;
            // 
            // chkSpoilers
            // 
            this.chkSpoilers.AutoSize = true;
            this.chkSpoilers.Location = new System.Drawing.Point(116, 98);
            this.chkSpoilers.Name = "chkSpoilers";
            this.chkSpoilers.Size = new System.Drawing.Size(63, 17);
            this.chkSpoilers.TabIndex = 18;
            this.chkSpoilers.Text = "Spoilers";
            this.chkSpoilers.UseVisualStyleBackColor = true;
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(120, 64);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(42, 20);
            this.txtOffset.TabIndex = 20;
            // 
            // lblOffset
            // 
            this.lblOffset.AutoSize = true;
            this.lblOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOffset.Location = new System.Drawing.Point(73, 64);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(45, 16);
            this.lblOffset.TabIndex = 19;
            this.lblOffset.Text = "Offset:";
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 160);
            this.Controls.Add(this.txtOffset);
            this.Controls.Add(this.lblOffset);
            this.Controls.Add(this.chkSpoilers);
            this.Controls.Add(this.chkRaw);
            this.Controls.Add(this.btnBrowseUnpack);
            this.Controls.Add(this.txtUnpack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseOut);
            this.Controls.Add(this.txtOut);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Name = "frmSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSettings_FormClosing);
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowseUnpack;
        private System.Windows.Forms.TextBox txtUnpack;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkRaw;
        private System.Windows.Forms.CheckBox chkSpoilers;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Label lblOffset;
    }
}