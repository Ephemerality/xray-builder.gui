using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI
{
    public partial class frmPreviewSA : Form
    {
        public frmPreviewSA()
        {
            InitializeComponent();
        }

        public string titlePopup = "";
        public string descriptionPopup = "";
        public string biographyPopup = "";

        private List<string[]> otherBookList = new List<string[]>();

        public async Task populateStartActions(string inputFile)
        {
            string input;
            using (StreamReader streamReader = new StreamReader(inputFile, Encoding.UTF8))
                input = streamReader.ReadToEnd();
            ilOtherBooks.Images.Clear();
            dgvOtherBooks.Rows.Clear();

            JObject sa = JObject.Parse(input);
            var tempData = sa["data"]["seriesPosition"];
            if (tempData != null)
            {
                string position = tempData["positionInSeries"].ToString();
                string total = tempData["totalInSeries"].ToString();
                string name = tempData["seriesName"].ToString();
                lblSeries.Text = String.Format("This is book {0} of {1} in {2}", position, total, name);
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
                Match popularHighlightsText = Regex.Match(tempData.ToString(),
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
                Match rating = Regex.Match(tempData["amazonRating"].ToString(), @"(\d+)");
                if (rating != null && rating.Groups.Count > 1)
                    pbRating.Image = (Image)Resources.ResourceManager.GetObject(String.Format("STAR{0}", rating.Groups[1].Value));
                lblVotes.Text = String.Format("({0} votes)", tempData["numberOfReviews"].ToString());
            }

            tempData = sa["data"]["authorBios"]?["authors"]?[0];
            if (tempData != null)
            {
                string imageUrl = tempData["imageUrl"]?.ToString();
                if (imageUrl != "" && imageUrl != null)
                    pbAuthorImage.Image = Functions.MakeGrayscale3(await HttpDownloader.GetImage(imageUrl));
                lblBiography.Text = tempData["bio"].ToString();
                biographyPopup = lblBiography.Text;
            }

            tempData = sa["data"]["authorRecs"]["recommendations"];
            if (tempData != null)
            {
                var otherBooks = new List<Tuple<string, string, string, string>>();
                foreach (var rec in tempData)
                {
                    string imageUrl = rec["imageUrl"]?.ToString() ?? "";
                    string author = rec["authors"][0].ToString();
                    string title = rec["title"].ToString();
                    otherBooks.Add(new Tuple<string, string, string, string>(
                        rec["asin"].ToString(), title, author, imageUrl));
                    if (imageUrl != "")
                        ilOtherBooks.Images.Add(Functions.MakeGrayscale3(await HttpDownloader.GetImage(imageUrl)));
                    otherBookList.Add(new [] { title, author });
                    dgvOtherBooks.Rows.Add(ilOtherBooks.Images[ilOtherBooks.Images.Count - 1],
                        String.Format("{0}\n{1}", title, author));
                }
            }

            tempData = sa["data"]["readingTime"];
            if (tempData != null)
            {
                lblReadingTime.Text = String.Format("{0} hours and {1} minutes to read",
                    tempData["hours"].ToString(), tempData["minutes"].ToString());
                tempData = sa["data"]["readingPages"];
                if (tempData != null)
                    lblReadingTime.Text = lblReadingTime.Text + String.Format(@" ({0} pages)", tempData["pagesInBook"].ToString());
            }

            tempData = sa["data"]["previousBookInTheSeries"];
            if (tempData != null)
            {
                lblPreviousTitle.Text = tempData["title"].ToString();
                string imageUrl = tempData["imageUrl"]?.ToString();
                if (imageUrl != "" && imageUrl != null)
                    pbPreviousCover.Image = Functions.MakeGrayscale3(await HttpDownloader.GetImage(imageUrl));
            }
        }
    }
}
