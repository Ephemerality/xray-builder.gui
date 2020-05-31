using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.Libraries.Images.Extensions;
using XRayBuilder.Core.Libraries.Images.Util;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI.Preview
{
    public sealed partial class frmPreviewAP : Form, IPreviewForm
    {
        public frmPreviewAP()
        {
            InitializeComponent();
        }

        public Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            dgvOtherBooks.Rows.Clear();

            var authorProfile = JsonUtil.DeserializeFile<AuthorProfile>(inputFile);
            var author = authorProfile.Authors?.FirstOrDefault();
            if (author != null)
            {
                lblAuthorMore.Text = $" Kindle Books By {author.Name}";
                Text = $"About {lblAuthorMore.Text}";
                lblBiography.Text = author.Bio ?? "";
                if (author.Picture != null)
                    pbAuthorImage.Image = ImageUtil.Base64ToImage(author.Picture).ToGrayscale3();
            }

            if (authorProfile.OtherBooks != null)
            {
                foreach (var book in authorProfile.OtherBooks)
                    dgvOtherBooks.Rows.Add(book.Title, Resources.arrow_right);
            }

            return Task.CompletedTask;
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }
    }
}
