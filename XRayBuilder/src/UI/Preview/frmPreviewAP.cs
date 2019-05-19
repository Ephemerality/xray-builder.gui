using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI.Preview
{
    public partial class frmPreviewAP : Form, IPreviewForm
    {
        public frmPreviewAP()
        {
            InitializeComponent();
        }

        public Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            string input;
            using (var streamReader = new StreamReader(inputFile, Encoding.UTF8))
                input = streamReader.ReadToEnd();

            dgvOtherBooks.Rows.Clear();

            var ap = JObject.Parse(input);
            var tempData = ap["u"]?[0];
            if (tempData != null)
            {
                lblAuthorMore.Text = $" Kindle Books By {tempData["n"]}";
                Text = $"About {lblAuthorMore.Text}";
                lblBiography.Text = tempData["b"]?.ToString() ?? "";
                var image64 = tempData["i"]?.ToString() ?? "";
                if (image64 != "")
                    pbAuthorImage.Image = Functions.MakeGrayscale3(Functions.Base64ToImage(image64));
            }

            tempData = ap["o"];
            if (tempData != null)
            {
                foreach (var rec in tempData)
                    dgvOtherBooks.Rows.Add(" " + rec["t"], Resources.arrow_right);
            }

            return Task.Delay(1, cancellationToken);
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }
    }
}
