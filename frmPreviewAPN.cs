using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI
{
    public partial class frmPreviewAPN : Form
    {
        public frmPreviewAPN()
        {
            InitializeComponent();
        }

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

        public bool populateAuthorProfile(string inputFile)
        {
            Cursor.Current = Cursors.WaitCursor;
            dgvOtherBooks.Rows.Clear();

            StreamReader streamReader = new StreamReader(inputFile, Encoding.UTF8);
            string input = streamReader.ReadToEnd();
            
            Match authorBios = Regex.Match(input, @"""n"":""(.*)"",""a"":""B[A-Z0-9]{9}"",""b"":""(.*)"",""i"":""(.*)""}\],""a""");
            if (authorBios.Success)
            {
                currentLine = authorBios.Value;
                string[] split = Regex.Split(authorBios.Value, (@","""));
                if (split.Length == 5)
                {
                    Match author = Regex.Match(split[0], @""":""(.*)""");
                    if (author.Success)
                    {
                        lblAuthorMore.Text = String.Format(" Kindle Books By {0}", author.Groups[1].Value);
                        this.Text = String.Format("About {0}", author.Groups[1].Value);
                    }

                    Match find = Regex.Match(split[2], @""":""(.*)""");
                    string bio = find.Groups[1].Value;
                    lblBiography.Text = bio;

                    Match image = Regex.Match(split[3], @""":""(.*)""");
                    if (image.Success)
                    {
                        Bitmap temp = Functions.Base64ToImage(image.Groups[1].Value);
                        pbAuthorImage.Image = Functions.MakeGrayscale3(temp);
                    }
                }
            }
            else
                MessageBox.Show("This Author Profile does not contain any author biography information.");

            Match authorRecs = Regex.Match(input, @"""o"":\[{(.*)}\]");
            if (authorRecs.Success)
            {
                currentLine = authorRecs.Value;
                string[] split = Regex.Split(authorRecs.Value, (@"},{"));
                if (split.Length != 0)
                {
                    int i = 0;
                    foreach (string line in split)
                    {
                        Match bookInfo = Regex.Match(line, @"""t"":""(.*)""");
                        if (bookInfo.Success)
                        {
                            currentLine = bookInfo.Value;
                            string titleSpaced = " " + bookInfo.Groups[1].Value;
                            dgvOtherBooks.Rows.Add(titleSpaced, Resources.arrow_right);
                            i++;
                        }
                    }
                }
            }
            else
                MessageBox.Show("This Author Profile does not contain any of this author's other book information.");
            
            Cursor.Current = Cursors.Default;
            return true;
        }
    }
}
