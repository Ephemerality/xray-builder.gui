using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI
{
    public partial class frmPreviewAP : Form
    {
        public frmPreviewAP()
        {
            InitializeComponent();
        }

        public void populateAuthorProfile(string inputFile)
        {
            string input;
            using (StreamReader streamReader = new StreamReader(inputFile, Encoding.UTF8))
                input = streamReader.ReadToEnd();

            dgvOtherBooks.Rows.Clear();
            
            JObject ap = JObject.Parse(input);
            var tempData = ap["u"]?[0];
            if (tempData != null)
            {
                lblAuthorMore.Text = String.Format(" Kindle Books By {0}", tempData["n"].ToString());
                Text = String.Format("About {0}", lblAuthorMore.Text);
                lblBiography.Text = tempData["b"]?.ToString() ?? "";
                string image64 = tempData["i"]?.ToString() ?? "";
                if (image64 != "")
                    pbAuthorImage.Image = Functions.MakeGrayscale3(Functions.Base64ToImage(image64));
            }

            tempData = ap["o"];
            if (tempData != null)
            {
                foreach (var rec in tempData)
                    dgvOtherBooks.Rows.Add(" " + rec["t"].ToString(), Resources.arrow_right);
            }
        }
    }
}
