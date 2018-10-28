using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    public partial class frmPreviewXR : Form, IPreviewForm
    {
        public frmPreviewXR()
        {
            InitializeComponent();
        }

        // TODO: Add notable clips
        public Task Populate(string filePath)
        {
            var ver = XRayUtil.CheckXRayVersion(filePath);
            if (ver == XRayUtil.XRayVersion.Invalid)
                throw new Exception("Invalid X-Ray file.");

            var terms = ver == XRayUtil.XRayVersion.New
                ? XRayUtil.ExtractTermsNew(new SQLiteConnection($"Data Source={filePath}; Version=3;"), true)
                : XRayUtil.ExtractTermsOld(filePath);

            flpPeople.Controls.Clear();
            flpTerms.Controls.Clear();

            foreach (XRay.Term t in terms)
            {
                XRayPanel p = new XRayPanel(t.Type, t.TermName, Math.Max((int) t.Occurrences.Count, (int) t.Locs.Count).ToString(), t.Desc);
                if (t.Type == "character")
                    flpPeople.Controls.Add(p);
                if (t.Type == "topic")
                    flpTerms.Controls.Add(p);
            }
            tcXray.SelectedIndex = 0;
            return Task.Delay(1);
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }

        private void flpPeople_Scroll(object sender, ScrollEventArgs e)
        {
            flpPeople.VerticalScroll.Value = e.NewValue;
        }
    }
}
