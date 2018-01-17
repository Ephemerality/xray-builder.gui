using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace XRayBuilderGUI
{
    public partial class frmPreviewEA : Form
    {

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

        public frmPreviewEA()
        {
            InitializeComponent();
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

        public async Task populateEndActions(string inputFile)
        {
            string input;
            using (StreamReader streamReader = new StreamReader(inputFile, Encoding.UTF8))
                input = streamReader.ReadToEnd();
            ilauthorRecs.Images.Clear();
            lvAuthorRecs.Items.Clear();
            ilcustomersWhoBoughtRecs.Images.Clear();
            lvCustomersWhoBoughtRecs.Items.Clear();

            JObject ea = JObject.Parse(input);
            var tempData = ea["data"]["nextBook"];
            if (tempData != null)
            {
                lblNextTitle.Text = tempData["title"].ToString();
                lblNextAuthor.Text = tempData["authors"][0].ToString();
                string imageUrl = tempData["imageUrl"]?.ToString();
                if (imageUrl != "" && imageUrl != null)
                    pbNextCover.Image = Functions.MakeGrayscale3(await HttpDownloader.GetImage(imageUrl));
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
                    string imageUrl = rec["imageUrl"]?.ToString();
                    if (imageUrl != "" && imageUrl != null)
                        ilauthorRecs.Images.Add(Functions.MakeGrayscale3(await HttpDownloader.GetImage(imageUrl)));
                }
                ListViewItem_SetSpacing(this.lvAuthorRecs, 60 + 7, 90 + 7);
                for (int i = 0; i < ilauthorRecs.Images.Count; i++)
                {
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    lvAuthorRecs.Items.Add(item);
                }
            }

            tempData = ea["data"]["customersWhoBoughtRecs"]["recommendations"];
            if (tempData != null)
            {
                foreach (var rec in tempData)
                {
                    string imageUrl = rec["imageUrl"]?.ToString();
                    if (imageUrl != "" && imageUrl != null)
                        ilcustomersWhoBoughtRecs.Images.Add(Functions.MakeGrayscale3(await HttpDownloader.GetImage(imageUrl)));
                }
                ListViewItem_SetSpacing(this.lvCustomersWhoBoughtRecs, 60 + 7, 90 + 7);
                for (int i = 0; i < ilcustomersWhoBoughtRecs.Images.Count; i++)
                {
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    lvCustomersWhoBoughtRecs.Items.Add(item);
                }
            }
        }
    }
}