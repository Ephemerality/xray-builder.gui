using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI
{
    public partial class frmPreviewSAN : Form
    {
        public frmPreviewSAN()
        {
            InitializeComponent();
        }

        //private frmPreviewPopup frmMessage;
        public string titlePopup = "";
        public string descriptionPopup = "";
        public string biographyPopup = "";

        public ImageList ilOtherBooks = new ImageList();
        private List<string[]> otherBookList = new List<string[]>();

        private string currentLine;
        public string GetCurrentLine
        {
            get
            {
                return this.currentLine;
            }
            set
            {
                this.currentLine = value;
            }
        }

        public bool populateStartActions(string inputFile)
        {
            Cursor.Current = Cursors.WaitCursor;

            StreamReader streamReader = new StreamReader(inputFile, Encoding.UTF8);
            string input = streamReader.ReadToEnd();
            Match seriesPosition = Regex.Match(input, @"""seriesPosition"":{(.*)},""welcomeText""");
            if (seriesPosition.Success)
            {
                currentLine = seriesPosition.Value;
                string[] split = Regex.Split(seriesPosition.Value, (@","""));
                if (split.Length == 5)
                {
                    Match position = Regex.Match(split[1], @"(\d+)");
                    Match total = Regex.Match(split[2], @"(\d+)");
                    Match name = Regex.Match(split[3], @""":""(.*)""");
                    if (position.Value == "1")
                        lblSeries.Left = 12;
                    lblSeries.Text = String.Format("This is book {0} of {1} in {2}",
                        position.Value, total.Value, name.Groups[1].Value);
                    if (position.Value != "1")
                    {
                        lblSeries.Left = 80;
                        pbPreviousCover.Visible = true;
                        lblPreviousHeading.Visible = true;
                        lblPreviousTitle.Visible = true;
                    }
                    else
                    {
                        pbPreviousCover.Visible = false;
                        lblPreviousHeading.Visible = false;
                        lblPreviousTitle.Visible = false;
                    }
                }
            }
            else
            {
                lblSeries.Text = "This book is not part of a series...";
                pbPreviousCover.Image = Resources.missing_image;
                lblPreviousHeading.Visible = false;
                lblPreviousTitle.Visible = false;
            }

            Match popularHighlightsText = Regex.Match(input,
                @"""en-US"":""((\d+) passages have been highlighted (\d+) times)""");
            if (popularHighlightsText.Success)
            {
                currentLine = popularHighlightsText.Value;
                lblHighlights.Text = popularHighlightsText.Groups[1].Value;
            }
            else
                MessageBox.Show("This Start Action does not contain any popular highlight information.");

            Match bookDescription = Regex.Match(input, @"""bookDescription"":{(.*)},""authorBios""");
            if (bookDescription.Success)
            {
                currentLine = bookDescription.Value;
                string[] split = Regex.Split(bookDescription.Value, (@","""));
                if (split.Length == 10)
                {
                    Match find = Regex.Match(split[2], @""":""(.*)""");
                    lblTitle.Text = find.Groups[1].Value;
                    find = Regex.Match(split[4], @""":\[""(.*)""\]");
                    lblAuthor.Text = find.Groups[1].Value;
                    titlePopup = find.Groups[1].Value;
                    find = Regex.Match(split[3], @""":""(.*)""");
                    lblDescription.Text = find.Groups[1].Value;
                    descriptionPopup = find.Groups[1].Value;
                    find = Regex.Match(split[7], @""":(\d+)");
                    object O =
                        Resources.ResourceManager.GetObject(String.Format("STAR{0}", find.Groups[1].Value));
                    pbRating.Image = (Image)O;
                    find = Regex.Match(split[8], @""":(\d+)");
                    lblVotes.Text = String.Format("({0} votes)", find.Groups[1].Value);
                }
            }
            else
                MessageBox.Show("This Start Action does not contain any book description information.");

            Match authorBios = Regex.Match(input, @"""authorBios"":{(.*)},""authorRecs""");
            if (authorBios.Success)
            {
                currentLine = authorBios.Value;
                string[] split = Regex.Split(authorBios.Value, (@","""));
                if (split.Length == 7)
                {
                    Match image = Regex.Match(split[5], @""":""(.*)""");
                    WebRequest request = WebRequest.Create(image.Groups[1].Value);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            Bitmap bitmap = new Bitmap(stream);
                            pbAuthorImage.Image = MakeGrayscale3(bitmap);
                        }
                    }
                    Match find = Regex.Match(split[4], @""":""(.*)""");
                    lblBiography.Text = find.Groups[1].Value;
                    biographyPopup = find.Groups[1].Value;
                }
            }
            else
                MessageBox.Show("This Start Action does not contain any author biography information.");

            Match authorRecs = Regex.Match(input, @"""authorRecs"":{(.*)},""currentBook""");
            if (authorRecs.Success)
            {
                currentLine = authorRecs.Value;
                var otherBooks = new List<Tuple<string, string, string, string>>();

                string[] split = Regex.Split(authorRecs.Value, (@"},{"));
                if (split.Length != 0)
                {
                    int i = 0;
                    foreach (string line in split)
                    {
                        Match bookInfo = Regex.Match(line,
                            @"""class"":""recommendation"",""asin"":""(B[A-Z0-9]{9})"",""title"":""(.*)"",""authors"":\[""(.*)""\],""imageUrl"":""(.*)"",""hasSample"":false");
                        if (bookInfo.Success)
                        {
                            currentLine = bookInfo.Value;
                            otherBooks.Add(new Tuple<string, string, string, string>(
                                bookInfo.Groups[1].Value, bookInfo.Groups[2].Value,
                                bookInfo.Groups[3].Value, bookInfo.Groups[4].Value));
                            WebRequest request = WebRequest.Create(bookInfo.Groups[4].Value);
                            using (WebResponse response = request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                {
                                    Bitmap bitmap = new Bitmap(stream);
                                    Image img = MakeGrayscale3(bitmap);
                                    ilOtherBooks.Images.Add(img);
                                }
                            }
                            otherBookList.Add(new string[] { bookInfo.Groups[2].Value, bookInfo.Groups[3].Value });
                            dgvOtherBooks.Rows.Add(ilOtherBooks.Images[i],
                                String.Format("{0}\n{1}", bookInfo.Groups[2].Value,
                                    bookInfo.Groups[3].Value));
                            i++;
                        }
                    }
                }
            }
            else
                MessageBox.Show("This Start Action does not contain any of this author's other book information.");

            Match readingTime = Regex.Match(input,
                @"""readingTime"":{""class"":""time"",""hours"":(\d+),""minutes"":(\d+)");
            if (readingTime.Success)
            {
                currentLine = readingTime.Value;
                lblReadingTime.Text = String.Format("{0} hours and {1} minutes to read",
                    readingTime.Groups[1].Value, readingTime.Groups[2].Value);
            }
            else
                MessageBox.Show("This Start Action does not contain any typical reading time information.");

            Match previousBookInTheSeries = Regex.Match(input, @"""previousBookInTheSeries"":{(.*)}}");
            if (previousBookInTheSeries.Success)
            {
                currentLine = previousBookInTheSeries.Value;
                string[] split = Regex.Split(previousBookInTheSeries.Value, (@","""));
                if (split.Length == 11)
                {
                    Match image = Regex.Match(split[5], @""":""(.*)""");
                    WebRequest request = WebRequest.Create(image.Groups[1].Value);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            Bitmap bitmap = new Bitmap(stream);
                            pbPreviousCover.Image = MakeGrayscale3(bitmap);
                        }
                    }
                    Match find = Regex.Match(split[2], @""":""(.*)""");
                    lblPreviousTitle.Text = find.Groups[1].Value;
                }
            }

            Match readingPages = Regex.Match(input,
                @"""readingPages"":{""class"":""pages"",""pagesInBook"":(\d+)}}}");
            if (readingPages.Success)
            {
                currentLine = readingPages.Value;
                lblReadingTime.Text = lblReadingTime.Text +
                                      String.Format(@" ({0} pages)", readingPages.Groups[1].Value);
            }
            else
                MessageBox.Show("This Start Action does not contain page count information.");

            Cursor.Current = Cursors.Default;
            return true;
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        //private void pbMoreDesc_Click(object sender, EventArgs e)
        //{
        //    if (descriptionPopup != "")
        //    {
        //        frmMessage = new frmPreviewPopup();
        //        frmMessage.Location = new Point(this.Left, this.Top);
        //        frmMessage.lblPopup.Text = descriptionPopup;
        //        frmMessage.Text = "Book Description";
        //        frmMessage.ShowDialog(this);
        //    }
        //}

        //private void lblDescription_Click(object sender, EventArgs e)
        //{
        //    if (descriptionPopup != "")
        //    {
        //        frmMessage = new frmPreviewPopup();
        //        frmMessage.Location = new Point(this.Left, this.Top);
        //        frmMessage.Text = "Book Description";
        //        frmMessage.lblPopup.Text = descriptionPopup;
        //        frmMessage.ShowDialog(this);
        //    }
        //}

        //private void pbMoreAuthor_Click(object sender, EventArgs e)
        //{
        //    if (biographyPopup != "")
        //    {
        //        frmMessage = new frmPreviewPopup();
        //        frmMessage.Location = new Point(this.Left, this.Top);
        //        frmMessage.lblPopup.Text = biographyPopup;
        //        frmMessage.Text = titlePopup;
        //        frmMessage.ShowDialog(this);
        //    }
        //}

        //private void lblBiography_Click(object sender, EventArgs e)
        //{
        //    if (biographyPopup != "")
        //    {
        //        frmMessage = new frmPreviewPopup();
        //        frmMessage.Location = new Point(this.Left, this.Top);
        //        frmMessage.lblPopup.Text = biographyPopup;
        //        frmMessage.Text = titlePopup;
        //        frmMessage.ShowDialog(this);
        //    }
        //}
    }
}
