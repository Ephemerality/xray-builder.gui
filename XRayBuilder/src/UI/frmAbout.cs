using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    sealed partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            var builtAt = new FileInfo(assembly.Location).LastWriteTime.ToString(CultureInfo.InvariantCulture);
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "";
            if (string.IsNullOrEmpty(title))
                title = Path.GetFileNameWithoutExtension(assembly.CodeBase);

            labelProductName.Text = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";
            labelVersion.Text = $"Version {assembly.GetName().Version}";
            labelBuilt.Text = $"Build Date {builtAt}";
            labelCopyright.Text = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
            textBoxDescription.Text = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
            Text = $"About {title}";
        }
    }
}
