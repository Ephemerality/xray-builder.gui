using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries.Images.Extensions;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilderGUI.Extensions;

namespace XRayBuilderGUI.UI.Preview
{
    public sealed partial class frmPreviewAP : Form, IPreviewForm
    {
        public frmPreviewAP()
        {
            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape)
                return base.ProcessCmdKey(ref msg, keyData);
            Close();
            return true;
        }

        public Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            dgvOtherBooks.Rows.Clear();

            var authorProfile = JsonUtil.DeserializeFile<AuthorProfile>(inputFile);
            var author = authorProfile.Authors?.FirstOrDefault();
            if (author != null)
            {
                lblAuthorMore.Text = $" Kindle Books By {author.Name}";
                Text = $"About {author.Name}";
                lblBiography.Text = author.Bio ?? "";
                if (author.Picture != null)
                    pbAuthorImage.Image = author.Picture.Base64ToImage().ToBitmap();
            }

            if (authorProfile.OtherBooks != null)
            {
                foreach (var book in authorProfile.OtherBooks)
                    dgvOtherBooks.Rows.Add($" {book.Title}");
            }

            return Task.CompletedTask;
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }
    }
}
