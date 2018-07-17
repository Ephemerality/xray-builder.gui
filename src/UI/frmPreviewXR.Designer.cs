namespace XRayBuilderGUI
{
    partial class frmPreviewXR
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
            this.tcXray = new System.Windows.Forms.TabControl();
            this.tpPeople = new System.Windows.Forms.TabPage();
            this.pPeople = new System.Windows.Forms.Panel();
            this.flpPeople = new System.Windows.Forms.FlowLayoutPanel();
            this.tbTerms = new System.Windows.Forms.TabPage();
            this.pTerms = new System.Windows.Forms.Panel();
            this.flpTerms = new System.Windows.Forms.FlowLayoutPanel();
            this.tcXray.SuspendLayout();
            this.tpPeople.SuspendLayout();
            this.pPeople.SuspendLayout();
            this.tbTerms.SuspendLayout();
            this.pTerms.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcXray
            // 
            this.tcXray.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tcXray.Controls.Add(this.tpPeople);
            this.tcXray.Controls.Add(this.tbTerms);
            this.tcXray.ItemSize = new System.Drawing.Size(141, 32);
            this.tcXray.Location = new System.Drawing.Point(2, 4);
            this.tcXray.Name = "tcXray";
            this.tcXray.SelectedIndex = 0;
            this.tcXray.Size = new System.Drawing.Size(303, 418);
            this.tcXray.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tcXray.TabIndex = 0;
            // 
            // tpPeople
            // 
            this.tpPeople.Controls.Add(this.pPeople);
            this.tpPeople.Location = new System.Drawing.Point(4, 36);
            this.tpPeople.Name = "tpPeople";
            this.tpPeople.Padding = new System.Windows.Forms.Padding(3);
            this.tpPeople.Size = new System.Drawing.Size(295, 378);
            this.tpPeople.TabIndex = 1;
            this.tpPeople.Text = "PEOPLE";
            this.tpPeople.UseVisualStyleBackColor = true;
            // 
            // pPeople
            // 
            this.pPeople.Controls.Add(this.flpPeople);
            this.pPeople.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pPeople.Location = new System.Drawing.Point(3, 3);
            this.pPeople.Name = "pPeople";
            this.pPeople.Size = new System.Drawing.Size(289, 372);
            this.pPeople.TabIndex = 1;
            // 
            // flpPeople
            // 
            this.flpPeople.AutoScroll = true;
            this.flpPeople.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPeople.Location = new System.Drawing.Point(0, 0);
            this.flpPeople.Name = "flpPeople";
            this.flpPeople.Size = new System.Drawing.Size(289, 372);
            this.flpPeople.TabIndex = 1;
            // 
            // tbTerms
            // 
            this.tbTerms.Controls.Add(this.pTerms);
            this.tbTerms.Location = new System.Drawing.Point(4, 36);
            this.tbTerms.Name = "tbTerms";
            this.tbTerms.Padding = new System.Windows.Forms.Padding(3);
            this.tbTerms.Size = new System.Drawing.Size(295, 378);
            this.tbTerms.TabIndex = 2;
            this.tbTerms.Text = "TERMS";
            this.tbTerms.UseVisualStyleBackColor = true;
            // 
            // pTerms
            // 
            this.pTerms.Controls.Add(this.flpTerms);
            this.pTerms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pTerms.Location = new System.Drawing.Point(3, 3);
            this.pTerms.Name = "pTerms";
            this.pTerms.Size = new System.Drawing.Size(289, 372);
            this.pTerms.TabIndex = 0;
            // 
            // flpTerms
            // 
            this.flpTerms.AutoScroll = true;
            this.flpTerms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpTerms.Location = new System.Drawing.Point(0, 0);
            this.flpTerms.Name = "flpTerms";
            this.flpTerms.Size = new System.Drawing.Size(289, 372);
            this.flpTerms.TabIndex = 2;
            // 
            // frmPreviewXR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 426);
            this.Controls.Add(this.tcXray);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPreviewXR";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "X-Ray";
            this.tcXray.ResumeLayout(false);
            this.tpPeople.ResumeLayout(false);
            this.pPeople.ResumeLayout(false);
            this.tbTerms.ResumeLayout(false);
            this.pTerms.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage tpPeople;
        private System.Windows.Forms.TabPage tbTerms;
        private System.Windows.Forms.Panel pPeople;
        public System.Windows.Forms.FlowLayoutPanel flpPeople;
        public System.Windows.Forms.TabControl tcXray;
        private System.Windows.Forms.Panel pTerms;
        public System.Windows.Forms.FlowLayoutPanel flpTerms;
    }
}