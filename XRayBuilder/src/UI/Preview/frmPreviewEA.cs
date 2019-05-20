using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace XRayBuilderGUI.UI.Preview
{
    public partial class frmPreviewEA : Form, IPreviewForm
    {
        private readonly IHttpClient _httpClient;

        #region SET LISTVIEW ICON SPACING

        // http://qdevblog.blogspot.ch/2011/11/c-listview-item-spacing.html
        private int MakeLong(short lowPart, short highPart)
        {
            return (int) (((ushort) lowPart) | (uint) (highPart << 16));
        }

        private void ListViewItem_SetSpacing(ListView listview, short leftPadding, short topPadding)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            NativeMethods.SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr) MakeLong(leftPadding, topPadding));
        }

        #endregion

        public frmPreviewEA(IHttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }

        #region PREVENT LISTVIEW ICON SELECTION

        private void lvcustomersWhoBoughtRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        private void lvauthorRecs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected) e.Item.Selected = false;
        }

        #endregion

        // TODO: Deserialize properly
        public async Task Populate(string inputFile, CancellationToken cancellationToken = default)
        {
            string input;
            using (var streamReader = new StreamReader(inputFile, Encoding.UTF8))
                input = streamReader.ReadToEnd();
            ilauthorRecs.Images.Clear();
            lvAuthorRecs.Items.Clear();
            ilcustomersWhoBoughtRecs.Images.Clear();
            lvCustomersWhoBoughtRecs.Items.Clear();

            var ea = JObject.Parse(input);
            var data = ea["data"]
                       ?? throw new Exception("Invalid EndActions file!");
            var tempData = data["nextBook"];
            if (tempData != null)
            {
                lblNextTitle.Text = tempData["title"].ToString();
                lblNextAuthor.Text = tempData["authors"][0].ToString();
                var imageUrl = tempData["imageUrl"]?.ToString();
                if (!string.IsNullOrEmpty(imageUrl))
                    pbNextCover.Image = Functions.MakeGrayscale3(await _httpClient.GetImageAsync(imageUrl, cancellationToken));
            }
            else
            {
                pbNextCover.Visible = false;
                lblNextTitle.Visible = false;
                lblNextAuthor.Visible = false;
                lblNotInSeries.Visible = true;
            }

            tempData = ea["data"]["authorRecs"]["recommendations"];
            if (tempData != null)
            {
                foreach (var rec in tempData)
                {
                    var imageUrl = rec["imageUrl"]?.ToString();
                    if (!string.IsNullOrEmpty(imageUrl))
                        ilauthorRecs.Images.Add(Functions.MakeGrayscale3(await _httpClient.GetImageAsync(imageUrl, cancellationToken)));
                }
                ListViewItem_SetSpacing(lvAuthorRecs, 60 + 7, 90 + 7);
                for (var i = 0; i < ilauthorRecs.Images.Count; i++)
                {
                    var item = new ListViewItem { ImageIndex = i };
                    lvAuthorRecs.Items.Add(item);
                }
            }

            tempData = ea["data"]["customersWhoBoughtRecs"]["recommendations"];
            if (tempData != null)
            {
                foreach (var rec in tempData)
                {
                    var imageUrl = rec["imageUrl"]?.ToString();
                    if (!string.IsNullOrEmpty(imageUrl))
                        ilcustomersWhoBoughtRecs.Images.Add(Functions.MakeGrayscale3(await _httpClient.GetImageAsync(imageUrl, cancellationToken)));
                }
                ListViewItem_SetSpacing(lvCustomersWhoBoughtRecs, 60 + 7, 90 + 7);
                for (var i = 0; i < ilcustomersWhoBoughtRecs.Images.Count; i++)
                {
                    var item = new ListViewItem { ImageIndex = i };
                    lvCustomersWhoBoughtRecs.Items.Add(item);
                }
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
            Dispose(true);
        }
    }
}