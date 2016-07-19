using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public partial class frmPreviewEAN : Form
    {

        #region SET LISTWIEW ICON SPACING

        // http://qdevblog.blogspot.ch/2011/11/c-listview-item-spacing.html
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public int MakeLong(short lowPart, short highPart)
        {
            return (int) (((ushort) lowPart) | (uint) (highPart << 16));
        }

        public void ListViewItem_SetSpacing(ListView listview, short leftPadding, short topPadding)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr) MakeLong(leftPadding, topPadding));
        }

        #endregion

        public frmPreviewEAN()
        {
            InitializeComponent();
        }

        private string currentLine;

        public string GetCurrentLine
        {
            get { return this.currentLine; }
            set { this.currentLine = value; }
        }

        #region PREVENT LISTWIEW ICON SELECTION

        private void lvcustomersWhoBoughtRecs_ItemSelectionChanged(object sender,
            ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        private void lvauthorRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        #endregion

        public bool populateEndActions(string inputFile)
        {
            Cursor.Current = Cursors.WaitCursor;
            List<string> split = new List<string>();
            StreamReader streamReader = new StreamReader(inputFile, Encoding.UTF8);
            string input = streamReader.ReadToEnd();
            ilauthorRecs.Images.Clear();
            lvAuthorRecs.Items.Clear();
            ilcustomersWhoBoughtRecs.Images.Clear();
            lvCustomersWhoBoughtRecs.Items.Clear();

            Match nextinSeries = Regex.Match(input,
                @"""nextBook"":{(.*)},""publicSharedRating""");
            if (nextinSeries.Success)
            {
                currentLine = nextinSeries.Value;
                split.AddRange(Regex.Split(nextinSeries.Value, (@",""")));
                if (split.Count == 7)
                {
                    Match title = Regex.Match(split[2], @""":""(.*)""");
                    if (title.Success)
                        lblNextTitle.Text = Regex.Replace(title.Groups[1].Value, @" \(.*\)", string.Empty);
                    Match author = Regex.Match(split[3], @""":\[""(.*)""\]");
                    if (author.Success)
                        lblNextAuthor.Text = author.Groups[1].Value;
                    Match image = Regex.Match(split[4], @""":""(.*)""");
                    if (image.Success)
                    {
                        WebRequest request = WebRequest.Create(image.Groups[1].Value);
                        using (WebResponse response = request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        {
                            if (stream != null)
                            {
                                Bitmap bitmap = new Bitmap(stream);
                                pbNextCover.Image = Functions.MakeGrayscale3(bitmap);
                            }
                        }
                    }
                }
                else
                {
                    pbNextCover.Visible = false;
                    lblNextTitle.Visible = false;
                    lblNextAuthor.Visible = false;
                    lblNotInSeries.Visible = true;
                }
            }
            
            Match authorRecs = Regex.Match(input, @"""authorRecs"":{(.*)},""customersWhoBoughtRecs""");
            currentLine = authorRecs.Value;
            split.Clear();
            split.AddRange(Regex.Split(authorRecs.Value, (@"},{")));
            if (split.Count != 0)
            {
                foreach (string line in split)
                {
                    Match bookInfo = Regex.Match(line, @"""imageUrl"":""(.*)"",""hasSample""");
                    if (bookInfo.Success)
                    {
                        currentLine = bookInfo.Value;
                        WebRequest request = WebRequest.Create(bookInfo.Groups[1].Value);
                        using (WebResponse response = request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        {
                            if (stream != null)
                            {
                                Bitmap bitmap = new Bitmap(stream);
                                Image img = Functions.MakeGrayscale3(bitmap);
                                ilauthorRecs.Images.Add(img);
                            }
                        }
                    }
                }
                ListViewItem_SetSpacing(this.lvAuthorRecs, 60 + 7, 90 + 7);
                for (int i = 0; i < ilauthorRecs.Images.Count; i++)
                {
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    lvAuthorRecs.Items.Add(item);
                }
            }
            else
                MessageBox.Show("This EndAction does not contain any of this author's other books.");

            Match customersWhoBoughtRecs = Regex.Match(input, @"""customersWhoBoughtRecs"":{(.*)},""goodReadsReview""");
            currentLine = customersWhoBoughtRecs.Value;
            split.Clear();
            split.AddRange(Regex.Split(customersWhoBoughtRecs.Value, (@"},{")));
            if (split.Count != 0)
            {
                foreach (string line in split)
                {
                    Match bookInfo = Regex.Match(line, @"""imageUrl"":""(.*)"",""hasSample""");
                    if (bookInfo.Success)
                    {
                        currentLine = bookInfo.Value;
                        WebRequest request = WebRequest.Create(bookInfo.Groups[1].Value);
                        using (WebResponse response = request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        {
                            if (stream != null)
                            {
                                Bitmap bitmap = new Bitmap(stream);
                                Image img = Functions.MakeGrayscale3(bitmap);
                                ilcustomersWhoBoughtRecs.Images.Add(img);
                            }
                        }
                    }
                }
                ListViewItem_SetSpacing(this.lvCustomersWhoBoughtRecs, 60 + 7, 90 + 7);
                for (int i = 0; i < ilcustomersWhoBoughtRecs.Images.Count; i++)
                {
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    lvCustomersWhoBoughtRecs.Items.Add(item);
                }
            }
            else
                MessageBox.Show("This EndAction does not contain any of this other customer books.");
            return true;
        }
    }
}