﻿using System.IO;
using System.Reflection;
using System.Windows.Forms;
using XRayBuilder.Core.Libraries;

namespace XRayBuilderGUI.UI
{
    sealed partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            var built = new FileInfo(assembly.Location).LastWriteTime;
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "";
            if (string.IsNullOrEmpty(title))
                title = Path.GetFileNameWithoutExtension(assembly.Location);

            lblName.Text = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";
            lblVersion.Text = $"Version {assembly.GetName().Version}";
            lblBuild.Text = $"Built on {built.ToLongDateString()} at {built.ToShortTimeString()}";
            lblCopyright.Text = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
            lblDescription.Text = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
            Text = $"About {title}";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape)
                return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        private void lnklblIcons_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Functions.ShellExecute("https://icons8.com");
            }
            catch
            {
                // ignored
            }
        }
    }
}
