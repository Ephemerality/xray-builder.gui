using System;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Core.XRay.Util;

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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape) return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
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
                var p = new XRayPanel(t.Type, t.TermName, t.Occurrences.Count.ToString(), t.Desc);
                var controls = t.Type switch
                {
                    "character" => flpPeople.Controls,
                    "topic" => flpTerms.Controls,
                    _ => null
                };
                controls?.Add(p);
            }
            tcXray.SelectedIndex = 0;
            return Task.CompletedTask;
        }

        public new void ShowDialog() => base.ShowDialog();

        private void flpPeople_Scroll(object sender, ScrollEventArgs e)
        {
            flpPeople.VerticalScroll.Value = e.NewValue;
        }
    }
}
