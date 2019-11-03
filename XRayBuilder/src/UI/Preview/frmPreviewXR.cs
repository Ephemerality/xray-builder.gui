using System;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilderGUI.XRay;
using XRayBuilderGUI.XRay.Logic.Terms;

namespace XRayBuilderGUI.UI.Preview
{
    public partial class frmPreviewXR : Form, IPreviewForm
    {
        private readonly ITermsService _termsService;

        public frmPreviewXR(ITermsService termsService)
        {
            _termsService = termsService;
            InitializeComponent();
        }

        // TODO: Add notable clips
        public Task Populate(string filePath, CancellationToken cancellationToken = default)
        {
            var ver = XRayUtil.CheckXRayVersion(filePath);
            if (ver == XRayUtil.XRayVersion.Invalid)
                throw new Exception("Invalid X-Ray file.");

            var terms = ver == XRayUtil.XRayVersion.New
                ? _termsService.ExtractTermsNew(new SQLiteConnection($"Data Source={filePath}; Version=3;"), true)
                : _termsService.ExtractTermsOld(filePath);

            flpPeople.Controls.Clear();
            flpTerms.Controls.Clear();

            foreach (var t in terms)
            {
                var p = new XRayPanel(t.Type, t.TermName, Math.Max((int) t.Occurrences.Count, (int) t.Locs.Count).ToString(), t.Desc);
                if (t.Type == "character")
                    flpPeople.Controls.Add(p);
                if (t.Type == "topic")
                    flpTerms.Controls.Add(p);
            }
            tcXray.SelectedIndex = 0;
            return Task.Delay(1, cancellationToken);
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
