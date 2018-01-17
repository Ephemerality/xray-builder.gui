using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI
{
    public partial class frmCreateXR : Form
    {
        public frmCreateXR()
        {
            InitializeComponent();
        }

        private ToolTip toolTip1 = new ToolTip();
        private List<XRay.Term> Terms = new List<XRay.Term>(100);
        private Settings settings = Settings.Default;

        private bool aliasesExist;

        private void btnAddTerm_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "") return;
            Image typeImage = rdoCharacter.Checked == true ? Resources.character : Resources.setting;
            dgvTerms.Rows.Add(
                typeImage,
                txtName.Text,
                txtAliases.Text,
                txtDescription.Text,
                txtLink.Text,
                rdoGoodreads.Checked == true ? "Goodreads" : "Wikipedia",
                chkMatch.Checked,
                chkCase.Checked,
                chkDelete.Checked,
                chkRegex.Checked);
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            txtLink.Text = "";
        }

        private void btnEditTerm_Click(object sender, EventArgs e)
        {
            if (txtName.Text != "" && DialogResult.Cancel == MessageBox.Show(
              "You have not added this term to the term list.\r\n" +
              "Click Cancel to add the current term to the term list.\r\n" +
              "Press Ok to replace the current term with this one in the term list." +
              "Do you want to continue?",
              "Unsaved changes",
              MessageBoxButtons.OKCancel,
              MessageBoxIcon.Warning))
                return;
            foreach (DataGridViewRow row in dgvTerms.Rows)
            {
                if (row.Selected == true)
                {
                    rdoCharacter.Checked = CompareImages((Bitmap)row.Cells[0].Value, Resources.character);
                    rdoTopic.Checked = CompareImages((Bitmap)row.Cells[0].Value, Resources.setting);
                    txtName.Text = row.Cells[1].Value.ToString();
                    txtAliases.Text = row.Cells[2].Value.ToString();
                    txtDescription.Text = row.Cells[3].Value.ToString();
                    txtLink.Text = row.Cells[4].Value.ToString();
                    rdoGoodreads.Checked = row.Cells[5].Value.ToString() == "Goodreads" ? true : false;
                    rdoWikipedia.Checked = row.Cells[5].Value.ToString() == "Wikipedia" ? true : false;
                    chkMatch.Checked = (bool)row.Cells[6].Value;
                    chkCase.Checked = (bool)row.Cells[7].Value;
                    //chkDelete.Checked = (bool)row.Cells[8].Value;
                    chkRegex.Checked = (bool)row.Cells[9].Value;
                    dgvTerms.Rows.Remove(row);
                }
            }
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtLink.Text == "") return;
                Process.Start(txtLink.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured opening this link: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void btnOpenXml_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog()
            {
                Title = "Open XML or TXT file",
                Filter = "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt",
                InitialDirectory = Environment.CurrentDirectory + @"\xml\"
            };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string filetype = Path.GetExtension(openFile.FileName);
                string file = openFile.FileName;
                Match match = Regex.Match(file, "(B[A-Z0-9]{9})");
                if (match.Success)
                    txtAsin.Text = match.Value;
                string aliasFile = Environment.CurrentDirectory + @"\ext\" + txtAsin.Text + ".aliases";
                var d = new Dictionary<string, string>();
                dgvTerms.Rows.Clear();
                txtName.Text = "";
                txtAliases.Text = "";
                txtDescription.Text = "";
                txtLink.Text = "";
                try
                {
                    if (filetype == ".xml")
                        Terms = Functions.DeserializeList<XRay.Term>(file);
                    else if (filetype == ".txt")
                        Terms = LoadTermsFromTxt<XRay.Term>(file);
                    else
                        MessageBox.Show("Error: Bad file type \"" + filetype + "\"");
                    foreach (XRay.Term t in Terms)
                    {
                        Image typeImage = t.Type == "character" ? Resources.character : Resources.setting;
                        dgvTerms.Rows.Add(
                            typeImage,
                            t.TermName,
                            "",
                            t.Desc,
                            t.DescUrl,
                            t.DescSrc,
                            t.Match,
                            t.MatchCase,
                            false,
                            t.RegEx);
                    }
                    Terms.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
                try
                {
                    if (!File.Exists(aliasFile)) return;
                    using (var streamReader = new StreamReader(aliasFile, Encoding.UTF8))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            string input = streamReader.ReadLine();
                            string[] temp = input.Split('|');
                            if (temp.Length <= 1 || temp[0] == "" || temp[0].Substring(0, 1) == "#") continue;
                            string temp2 = input.Substring(input.IndexOf('|') + 1);
                            if (!d.ContainsKey(temp[0]))
                                d.Add(temp[0], temp2);
                        }
                    }
                    foreach (DataGridViewRow row in dgvTerms.Rows)
                    {
                        string name = row.Cells[1].Value.ToString();
                        if (d.TryGetValue(name, out var aliases))
                            row.Cells[2].Value = aliases;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        private void btnRemoveTerm_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvTerms.Rows)
            {
                if (row.Selected == true)
                    dgvTerms.Rows.Remove(row);
            }
        }

        private void btnSaveXML_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count == 0)
                return;
            if (txtAuthor.Text == "")
            {
                MessageBox.Show("An author's name is required to save the X-Ray file.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtTitle.Text == "")
            {
                MessageBox.Show("A title is required to save the X-Ray file.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtAsin.Text == "")
            {
                MessageBox.Show("An ASIN is required to save the aliases file.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                CreateTerms();
                if (aliasesExist)
                {
                    CreateAliases();
                    MessageBox.Show("X-Ray entities and Alias files created sucessfully!");
                }
                else
                    MessageBox.Show("X-Ray entities file created sucessfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("An error occurred saving the files: {0}\r\n{1}", ex.Message, ex.StackTrace),
                    "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvTerms_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            btnEditTerm_Click(sender, e);
        }

        private void dgvTerms_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1)
            {
                if (e.Button == MouseButtons.Right)
                {
                    Point relativeMousePosition = dgvTerms.PointToClient(Cursor.Position);
                    this.cmsTerms.Show(dgvTerms, relativeMousePosition);
                }
            }
        }

        private void frmCreateXR_Load(object sender, EventArgs e)
        {
            dgvTerms.Rows.Clear();
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            txtLink.Text = "";
            //txtAuthor.Text = "";
            //txtTitle.Text = "";
            //txtAsin.Text = "";
            dgvTerms.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTerms.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTerms.Columns[7].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTerms.Columns[8].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTerms.Columns[9].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(btnAddTerm, "Add this character or\r\ntopic to the term list.");
            toolTip1.SetToolTip(btnLink, "Open this link in your\r\ndefault browser.");
            toolTip1.SetToolTip(btnEditTerm, "Edit the selected term. It will be\r\nremoved from the list and used to fill\r\nin the information above. Don't\r\nforget to add to the list when done.");
            toolTip1.SetToolTip(btnRemoveTerm, "Remove the selected term from the\r\nterm list. This action is irreversible.");
            toolTip1.SetToolTip(btnClear, "Clear the term list.");
            toolTip1.SetToolTip(btnOpenXml, "Open an existing term XML of TXT file.\r\nIf an alias file with a matching ASIN\r\nis found, aliases wil automatically be\r\npopulated.");
            toolTip1.SetToolTip(btnSaveXML, "Save the term list to an XML file. Any\r\nassociated aliases will be saved to an\r\nASIN.aliases file in the /ext folder.");
        }

        private void tsmDelete_Click(object sender, EventArgs e)
        {
            btnRemoveTerm_Click(sender, e);
        }

        private void tsmEdit_Click(object sender, EventArgs e)
        {
            btnEditTerm_Click(sender, e);
        }

        private void CreateAliases()
        {
            string aliasFile = Environment.CurrentDirectory + @"\ext\" + txtAsin.Text + ".aliases";
            string aliases = "";
            if (!Directory.Exists(Environment.CurrentDirectory + @"\ext\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\ext\");
            using (var streamWriter = new StreamWriter(aliasFile, false, Encoding.UTF8))
            {
                for (int i = 0; i < Terms.Count; i++)
                {
                    if (Terms[i].Aliases.Count > 0)
                    {
                        Terms[i].Aliases.Sort((a, b) => b.Length.CompareTo(a.Length));
                        foreach (string word in Terms[i].Aliases)
                        {
                            aliases += word + ",";
                        }
                        if (aliases.LastIndexOf(",") != -1)
                        {
                            streamWriter.WriteLine(Terms[i].TermName + "|" + aliases.Substring(0, aliases.LastIndexOf(",")));
                            aliases = "";
                        }
                        else
                            streamWriter.WriteLine(Terms[i].TermName + "|");
                    }
                }
            }
        }

        private void CreateTerms()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
            string outfile = Environment.CurrentDirectory + String.Format(@"\xml\{0}.entities.xml", txtAsin.Text);
            Terms.Clear();
            foreach (DataGridViewRow row in dgvTerms.Rows)
            {
                int termId = 1;
                XRay.Term newTerm = new XRay.Term();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    newTerm.Id = termId++;
                    newTerm.Type = CompareImages((Bitmap)row.Cells[0].Value, Resources.character) == true ? "character" : "topic";
                    newTerm.TermName = row.Cells[1].Value.ToString();
                    if (row.Cells[2].Value.ToString() != "")
                    {
                        aliasesExist = true;
                        string[] aliasList = row.Cells[2].Value.ToString().Split(',');
                        foreach (string alias in aliasList)
                        {
                            if (newTerm.Aliases.Contains(alias))
                            {
                                newTerm.Aliases.Remove(alias);
                                newTerm.Aliases.Add(alias);
                            }
                            else
                            {
                                newTerm.Aliases.Add(alias);
                            }
                        }
                    }
                    newTerm.Desc = row.Cells[3].Value.ToString();
                    newTerm.DescUrl = row.Cells[4].Value.ToString();
                    newTerm.DescSrc = row.Cells[5].Value.ToString();
                    newTerm.Match = (bool)row.Cells[6].Value;
                    newTerm.MatchCase = (bool)row.Cells[7].Value;
                    newTerm.RegEx = (bool)row.Cells[9].Value;
                }
                Terms.Add(newTerm);
            }
            Functions.Save<List<XRay.Term>>(Terms, outfile);
        }

        private static bool CompareImages(Bitmap image1, Bitmap image2)
        {
            if (image1.Width == image2.Width && image1.Height == image2.Height)
            {
                for (int i = 0; i < image1.Width; i++)
                {
                    for (int j = 0; j < image1.Height; j++)
                    {
                        if (image1.GetPixel(i, j) != image2.GetPixel(i, j))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<T> LoadTermsFromTxt<T>(string txtfile)
        {
            List<T> itemList = new List<T>();
            using (StreamReader streamReader = new StreamReader(txtfile, Encoding.UTF8))
            {
                int termId = 1;
                int lineCount = 1;
                Terms.Clear();
                while (!streamReader.EndOfStream)
                {
                    try
                    {
                        string temp = streamReader.ReadLine().ToLower();
                        lineCount++;
                        if (temp == "") continue;
                        if (temp != "character" && temp != "topic")
                        {
                            MessageBox.Show("Error: Invalid term type \"" + temp + "\" on line " + lineCount);
                            return null;
                        }
                        XRay.Term newTerm = new XRay.Term()
                        {
                            Type = temp,
                            TermName = streamReader.ReadLine(),
                            Desc = streamReader.ReadLine()
                        };
                        lineCount += 2;
                        newTerm.MatchCase = temp == "character" ? true : false;
                        newTerm.DescSrc = "shelfari";
                        newTerm.Id = termId++;
                        Terms.Add(newTerm);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred reading from txt file: " + ex.Message + "\r\n" + ex.StackTrace);
                        return null;
                    }
                }
            }
            return itemList;
        }

        private void frmCreateXR_FormClosing(object sender, FormClosingEventArgs e)
        {
            txtAuthor.Text = "";
            txtTitle.Text = "";
            txtAsin.Text = "";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count > 0)
            {
                if (DialogResult.Yes == MessageBox.Show("Clearing the term list is irreversible!", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    dgvTerms.Rows.Clear();
                    txtName.Text = "";
                    txtAliases.Text = "";
                    txtDescription.Text = "";
                    txtLink.Text = "";
                    Terms.Clear();
                }
            }
        }
    }
}