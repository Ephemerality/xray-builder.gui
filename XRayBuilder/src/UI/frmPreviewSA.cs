using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmPreviewSA : Form, IPreviewForm
    {
        public frmPreviewSA()
        {
            InitializeComponent();
        }

        public string titlePopup = "";
        public string descriptionPopup = "";
        public string biographyPopup = "";

        public async Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            string input;
            using (var streamReader = new StreamReader(inputFile, Encoding.UTF8))
                input = streamReader.ReadToEnd();
            ilOtherBooks.Images.Clear();
            dgvOtherBooks.Rows.Clear();

            var sa = JObject.Parse(input);
            var tempData = sa["data"]["seriesPosition"];
            if (tempData != null)
            {
                var position = tempData["positionInSeries"].ToString();
                var total = tempData["totalInSeries"].ToString();
                var name = tempData["seriesName"].ToString();
                lblSeries.Text = $"This is book {position} of {total} in {name}";
                if (position == "1")
                {
                    pbPreviousCover.Visible = false;
                    lblPreviousHeading.Visible = false;
                    lblPreviousTitle.Visible = false;
                    lblSeries.Left = 12;
                    lblSeries.Width = 312;
                }
                else
                {
                    lblSeries.Left = 80;
                    lblSeries.Width = 244;
                    pbPreviousCover.Visible = true;
                    lblPreviousHeading.Visible = true;
                    lblPreviousTitle.Visible = true;
                }
            }
            else
            {
                lblSeries.Text = "This book is not part of a series...";
                pbPreviousCover.Image = Resources.missing_image;
                lblPreviousHeading.Visible = false;
                lblPreviousTitle.Visible = false;
            }

            tempData = sa["data"]["popularHighlightsText"]?["localizedText"]?["en-US"];
            if (tempData != null)
            {
                var popularHighlightsText = Regex.Match(tempData.ToString(),
                    @"((\d+) passages have been highlighted (\d+) times)");
                if (popularHighlightsText.Success)
                    lblHighlights.Text = popularHighlightsText.Groups[1].Value;
            }

            tempData = sa["data"]["bookDescription"];
            if (tempData != null)
            {
                lblTitle.Text = tempData["title"].ToString();
                lblAuthor.Text = tempData["authors"][0].ToString();
                titlePopup = lblAuthor.Text;
                lblDescription.Text = tempData["description"].ToString();
                descriptionPopup = lblDescription.Text;
                var rating = Regex.Match(tempData["amazonRating"].ToString(), @"(\d+)");
                if (rating.Success)
                    pbRating.Image = (Image)Resources.ResourceManager.GetObject($"STAR{rating.Groups[1].Value}");
                lblVotes.Text = $"({tempData["numberOfReviews"]} votes)";
            }

            tempData = sa["data"]["authorBios"]?["authors"]?[0];
            if (tempData != null)
            {
                var imageUrl = tempData["imageUrl"]?.ToString() ?? "";
                if (imageUrl != "")
                    pbAuthorImage.Image = Functions.MakeGrayscale3(await HttpClient.GetImageAsync(imageUrl, cancellationToken));
                lblBiography.Text = tempData["bio"]?.ToString();
                biographyPopup = lblBiography.Text;
            }

            tempData = sa["data"]["authorRecs"]?["recommendations"];
            if (tempData != null)
            {
                // TODO: Figure out why otherBooks is here but not used
                //var otherBooks = new List<Tuple<string, string, string, string>>();
                foreach (var rec in tempData)
                {
                    var imageUrl = rec["imageUrl"]?.ToString() ?? "";
                    var author = rec["authors"][0].ToString();
                    var title = rec["title"].ToString();
                    //otherBooks.Add(new Tuple<string, string, string, string>(rec["asin"].ToString(), title, author, imageUrl));
                    if (imageUrl != "")
                        ilOtherBooks.Images.Add(Functions.MakeGrayscale3(await HttpClient.GetImageAsync(imageUrl, cancellationToken)));
                    dgvOtherBooks.Rows.Add(ilOtherBooks.Images[ilOtherBooks.Images.Count - 1], $"{title}\n{author}");
                }
            }

            tempData = sa["data"]["readingTime"];
            if (tempData != null)
            {
                lblReadingTime.Text = $"{tempData["hours"]} hours and {tempData["minutes"]} minutes to read";
                tempData = sa["data"]["readingPages"];
                if (tempData != null)
                    lblReadingTime.Text = $"{lblReadingTime.Text} ({tempData["pagesInBook"]} pages)";
            }

            tempData = sa["data"]["previousBookInTheSeries"];
            if (tempData != null)
            {
                lblPreviousTitle.Text = tempData["title"].ToString();
                var imageUrl = tempData["imageUrl"]?.ToString() ?? "";
                if (imageUrl != "")
                    pbPreviousCover.Image = Functions.MakeGrayscale3(await HttpClient.GetImageAsync(imageUrl, cancellationToken));
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }
    }
}
