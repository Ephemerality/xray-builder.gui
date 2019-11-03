using System;
using System.Reflection;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = $"Version {AssemblyVersion}";
            labelBuilt.Text = $"Build Date {AssemblyBuilt}";
            labelCopyright.Text = AssemblyCopyright;
            textBoxDescription.Text = AssemblyDescription;
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            Text = $"About {AssemblyTitle}";
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AssemblyBuilt
        {
            get
            {
                var buildDate = new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
                return buildDate.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                return attributes.Length == 0
                    ? ""
                    : ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0
                    ? ""
                    : ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0
                    ? ""
                    : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }
        #endregion
    }
}
