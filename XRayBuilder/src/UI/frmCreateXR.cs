using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Properties;
using XRayBuilderGUI.XRay.Model;

namespace XRayBuilderGUI.UI
{
    public partial class frmCreateXR : Form
    {
        public frmCreateXR()
        {
            InitializeComponent();
        }

        private ToolTip toolTip1 = new ToolTip();
        private List<Term> Terms = new List<Term>(100);

        private void btnAddTerm_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "") return;
            Image typeImage = rdoCharacter.Checked ? Resources.character : Resources.setting;
            dgvTerms.Rows.Add(
                typeImage,
                txtName.Text,
                txtAliases.Text,
                txtDescription.Text,
                txtLink.Text,
                rdoGoodreads.Checked ? "Goodreads" : "Wikipedia",
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
                if (row.Selected)
                {
                    rdoCharacter.Checked = CompareImages((Bitmap)row.Cells[0].Value, Resources.character);
                    rdoTopic.Checked = CompareImages((Bitmap)row.Cells[0].Value, Resources.setting);
                    txtName.Text = row.Cells[1].Value.ToString();
                    txtAliases.Text = row.Cells[2].Value.ToString();
                    txtDescription.Text = row.Cells[3].Value.ToString();
                    txtLink.Text = row.Cells[4].Value.ToString();
                    rdoGoodreads.Checked = row.Cells[5].Value.ToString() == "Goodreads";
                    rdoWikipedia.Checked = row.Cells[5].Value.ToString() == "Wikipedia";
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
            var openFile = new OpenFileDialog
            {
                Title = "Open XML or TXT file",
                Filter = "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt",
                InitialDirectory = Environment.CurrentDirectory + @"\xml\"
            };
            if (openFile.ShowDialog() != DialogResult.OK) return;
            var filetype = Path.GetExtension(openFile.FileName);
            var file = openFile.FileName;
            var match = Regex.Match(file, "(B[A-Z0-9]{9})", RegexOptions.Compiled);
            if (match.Success)
                txtAsin.Text = match.Value;
            var aliasFile = Environment.CurrentDirectory + @"\ext\" + txtAsin.Text + ".aliases";
            var d = new Dictionary<string, string>();
            dgvTerms.Rows.Clear();
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            txtLink.Text = "";
            try
            {
                if (filetype == ".xml")
                    Terms = Functions.DeserializeList<Term>(file);
                else if (filetype == ".txt")
                    Terms = LoadTermsFromTxt<Term>(file);
                else
                    MessageBox.Show("Error: Bad file type \"" + filetype + "\"");
                foreach (var t in Terms)
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
                        t.RegexAliases);
                }
                Terms.Clear();

                if (!File.Exists(aliasFile)) return;
                using (var streamReader = new StreamReader(aliasFile, Encoding.UTF8))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var input = streamReader.ReadLine();
                        var temp = input?.Split('|') ?? throw new IOException("Empty or invalid file.");
                        if (temp.Length <= 1 || temp[0] == "" || temp[0].Substring(0, 1) == "#") continue;
                        var temp2 = input.Substring(input.IndexOf('|') + 1);
                        if (!d.ContainsKey(temp[0]))
                            d.Add(temp[0], temp2);
                    }
                }
                foreach (DataGridViewRow row in dgvTerms.Rows)
                {
                    var name = row.Cells[1].Value.ToString();
                    if (d.TryGetValue(name, out var aliases))
                        row.Cells[2].Value = aliases;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void btnRemoveTerm_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvTerms.Rows)
            {
                if (row.Selected)
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
                CreateAliases();
                MessageBox.Show("X-Ray entities and Alias files created sucessfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An error occurred saving the files: {0}\r\n{1}", ex.Message, ex.StackTrace),
                    "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvTerms_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            btnEditTerm_Click(sender, e);
        }

        private void dgvTerms_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1 && e.Button == MouseButtons.Right)
            {
                var relativeMousePosition = dgvTerms.PointToClient(Cursor.Position);
                cmsTerms.Show(dgvTerms, relativeMousePosition);
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
            for (var i = 5; i <= 9; i++)
                dgvTerms.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
            var aliasFile = Environment.CurrentDirectory + @"\ext\" + txtAsin.Text + ".aliases";
            Directory.CreateDirectory(Environment.CurrentDirectory + @"\ext\");
            using var streamWriter = new StreamWriter(aliasFile, false, Encoding.UTF8);
            foreach (var term in Terms)
            {
                if (term.Aliases.Count > 0)
                {
                    term.Aliases.Sort((a, b) => b.Length.CompareTo(a.Length));
                    streamWriter.WriteLine($"{term.TermName}|{string.Join((string) ",", (IEnumerable<string>) term.Aliases)}");
                }
                else
                    streamWriter.WriteLine(term.TermName + "|");
            }
        }

        private void CreateTerms()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\xml\"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\xml\");
            var outfile = Environment.CurrentDirectory + $@"\xml\{txtAsin.Text}.entities.xml";
            Terms.Clear();
            var termId = 1;
            foreach (DataGridViewRow row in dgvTerms.Rows)
            {
                var newTerm = new Term
                {
                    Id = termId++,
                    Type = CompareImages((Bitmap) row.Cells[0].Value, Resources.character) ? "character" : "topic",
                    TermName = row.Cells[1].Value.ToString(),
                    Aliases = row.Cells[2].Value.ToString() != ""
                        ? row.Cells[2].Value.ToString().Split(',').Distinct().ToList()
                        : new List<string>(),
                    Desc = row.Cells[3].Value.ToString(),
                    DescUrl = row.Cells[4].Value.ToString(),
                    DescSrc = row.Cells[5].Value.ToString(),
                    Match = (bool) row.Cells[6].Value,
                    MatchCase = (bool) row.Cells[7].Value,
                    RegexAliases = (bool) row.Cells[9].Value
                };
                Terms.Add(newTerm);
            }
            Functions.Save(Terms, outfile);
        }

        private static bool CompareImages(Bitmap image1, Bitmap image2)
        {
            if (image1.Width != image2.Width || image1.Height != image2.Height) return false;
            for (var i = 0; i < image1.Width; i++)
                for (var j = 0; j < image1.Height; j++)
                    if (image1.GetPixel(i, j) != image2.GetPixel(i, j))
                        return false;
            return true;

        }

        private List<T> LoadTermsFromTxt<T>(string txtfile)
        {
            var itemList = new List<T>();
            using (var streamReader = new StreamReader(txtfile, Encoding.UTF8))
            {
                var termId = 1;
                var lineCount = 1;
                Terms.Clear();
                while (!streamReader.EndOfStream)
                {
                    try
                    {
                        var temp = streamReader.ReadLine()?.ToLower();
                        if (string.IsNullOrEmpty(temp)) continue;
                        lineCount++;
                        if (temp != "character" && temp != "topic")
                        {
                            MessageBox.Show("Error: Invalid term type \"" + temp + "\" on line " + lineCount);
                            return null;
                        }
                        var newTerm = new Term
                        {
                            Type = temp,
                            TermName = streamReader.ReadLine(),
                            Desc = streamReader.ReadLine()
                        };
                        lineCount += 2;
                        newTerm.MatchCase = temp == "character";
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