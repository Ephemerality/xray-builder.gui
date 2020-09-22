using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Images.Util;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    public partial class frmCreateXR : Form
    {
        private readonly IAliasesRepository _aliasesRepository;
        private readonly IAliasesService _aliasesService;
        private readonly IAmazonClient _amazonClient;

        // There is private method ClearAllSelections in ToolStrip class,
        // which removes selections from items. You can invoke it via reflection:
        // https://stackoverflow.com/a/10341622
        private readonly MethodInfo _method =
            typeof(ToolStrip).GetMethod("ClearAllSelections", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly IRoentgenClient _roentgenClient;
        private readonly ITermsService _termsService;

        private List<Term> _terms = new List<Term>(100);

        public frmCreateXR(
            ITermsService termsService,
            IAliasesRepository aliasesRepository,
            IAmazonClient amazonClient,
            IRoentgenClient roentgenClient,
            IAliasesService aliasesService)
        {
            _termsService = termsService;
            _aliasesRepository = aliasesRepository;
            _amazonClient = amazonClient;
            _roentgenClient = roentgenClient;
            _aliasesService = aliasesService;
            InitializeComponent();

            toolStrip.Renderer = new CustomToolStripProfessionalRenderer();

            var dgvType = dgvTerms.GetType();
            var pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi?.SetValue(dgvTerms, true, null);
        }

        public void SetMetadata(string asin, string author, string title)
        {
            txtAuthor.Text = author;
            txtTitle.Text = title;
            txtAsin.Text = asin;
        }

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
            if (!string.IsNullOrWhiteSpace(txtName.Text) && DialogResult.Cancel == MessageBox.Show(
                $@"The current term ({txtName.Text}) has not been added to the list!" +
                Environment.NewLine +
                @"Click Cancel if you want a chance to add the term first." +
                Environment.NewLine +
                @"Press OK to overwrite the current term with selected one from the term list.",
                $@"Unsaved changes to {txtName.Text}",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                return;

            var row = dgvTerms.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            if (row == null)
                return;

            rdoCharacter.Checked = ImageUtil.AreEqual((Bitmap) row.Cells[0].Value, Resources.character);
            rdoTopic.Checked = ImageUtil.AreEqual((Bitmap) row.Cells[0].Value, Resources.setting);
            txtName.Text = row.Cells[1].Value?.ToString() ?? "";
            txtAliases.Text = row.Cells[2].Value?.ToString() ?? "";
            txtDescription.Text = row.Cells[3].Value?.ToString() ?? "";
            txtLink.Text = row.Cells[4].Value?.ToString() ?? "";
            rdoGoodreads.Checked = row.Cells[5].Value?.ToString() == "Goodreads";
            rdoWikipedia.Checked = row.Cells[5].Value?.ToString() == "Wikipedia";
            chkMatch.Checked = (bool?) row.Cells[6].Value ?? false;
            chkCase.Checked = (bool?) row.Cells[7].Value ?? false;
            //chkDelete.Checked = (bool)row.Cells[8].Value;
            chkRegex.Checked = (bool?) row.Cells[9].Value ?? false;
            dgvTerms.Rows.Remove(row);
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtLink.Text == "")
                    return;
                Process.Start(txtLink.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"An error occured opening this link:" +
                                Environment.NewLine + $@"{ex.Message}" +
                                Environment.NewLine + $@"{ex.StackTrace}",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenXml_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Title = @"Open XML or TXT file",
                Filter = @"XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt",
                InitialDirectory = $@"{Environment.CurrentDirectory}\xml\"
            };
            if (openFile.ShowDialog() != DialogResult.OK)
                return;
            var filetype = Path.GetExtension(openFile.FileName);
            txtAsin.Text = _amazonClient.ParseAsin(openFile.FileName) ?? "";
            try
            {
                switch (filetype)
                {
                    case ".xml":
                        _terms = XmlUtil.DeserializeFile<List<Term>>(openFile.FileName);
                        break;
                    case ".txt":
                        _terms = _termsService.ReadTermsFromTxt(openFile.FileName).ToList();
                        break;
                    default:
                        MessageBox.Show($@"Unsupported file type : {filetype}",
                            @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                ReloadTerms();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"An error occured:" +
                                Environment.NewLine + $@"{ex.Message}" +
                                Environment.NewLine + $@"{ex.StackTrace}",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadTerms()
        {
            // todo another path to centralize
            var aliasFile = $@"{AppDomain.CurrentDomain.BaseDirectory}ext\{txtAsin.Text}.aliases";
            var d = new Dictionary<string, string>();
            dgvTerms.Rows.Clear();
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            txtLink.Text = "";
            foreach (var t in _terms)
            {
                var typeImage = t.Type == "character" ? Resources.character : Resources.setting;
                dgvTerms.Rows.Add(
                    typeImage,
                    t.TermName,
                    t.Aliases?.Count > 0 ? string.Join(",", t.Aliases) : "",
                    t.Desc,
                    t.DescUrl,
                    t.DescSrc,
                    t.Match,
                    t.MatchCase,
                    false,
                    t.RegexAliases);
            }

            _terms.Clear();

            if (!File.Exists(aliasFile))
                return;

            if (_terms.Any(term => term.Aliases?.Count > 0))
            {
                MessageBox.Show(@"The XML file already contained aliases," +
                                Environment.NewLine +
                                @"so the .aliases file will be ignored.",
                    @"Pre-Existing Aliases", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var streamReader = new StreamReader(aliasFile, Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    var input = streamReader.ReadLine();
                    var temp = input?.Split('|')
                               ?? throw new IOException("Empty or invalid file.");
                    if (temp.Length <= 1 || temp[0] == "" || temp[0].Substring(0, 1) == "#")
                        continue;
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

        private void btnRemoveTerm_Click(object sender, EventArgs e)
        {
            foreach (var row in dgvTerms.Rows.Cast<DataGridViewRow>().Where(row => row.Selected))
                dgvTerms.Rows.Remove(row);
        }

        private void btnSaveXML_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count == 0)
                return;
            if (txtAuthor.Text == "")
            {
                MessageBox.Show(@"An author's name is required to save the X-Ray file.",
                    @"Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtTitle.Text == "")
            {
                MessageBox.Show(@"A title is required to save the X-Ray file.", @"Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtAsin.Text == "")
            {
                MessageBox.Show(@"An ASIN is required to save the aliases file.", @"Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                CreateTerms();
                _aliasesRepository.SaveCharactersToFile(_terms, txtAsin.Text, Settings.Default.splitAliases);
                MessageBox.Show(@"X-Ray entities and Alias files created successfully!",
                    @"Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"An error occurred saving the XML files:" +
                                Environment.NewLine + $@"{ex.Message}" +
                                Environment.NewLine + $@"{ex.StackTrace}",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvTerms_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            btnEditTerm_Click(sender, e);
        }

        private void dgvTerms_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1 || e.Button != MouseButtons.Right) return;
            var relativeMousePosition = dgvTerms.PointToClient(Cursor.Position);
            cmsTerms.Show(dgvTerms, relativeMousePosition);
        }

        private void frmCreateXR_Load(object sender, EventArgs e)
        {
            dgvTerms.Rows.Clear();
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            txtLink.Text = "";
            for (var i = 5; i <= 9; i++)
                dgvTerms.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            btnAddTerm.ToolTipText = @"Add this character or" +
                                     Environment.NewLine +
                                     @"topic to the term list.";
            btnLink.ToolTipText = @"Open this link in your" +
                                  Environment.NewLine +
                                  @"default browser.";
            btnEditTerm.ToolTipText = @"Edit the selected term. It will be" +
                                      Environment.NewLine +
                                      @"removed from the list and used to fill" +
                                      Environment.NewLine +
                                      @"in the information above. Don't" +
                                      Environment.NewLine +
                                      @"forget to add to the list when done.";
            btnRemoveTerm.ToolTipText = @"Remove the selected term from the" +
                                        Environment.NewLine +
                                        @"term list. This action is irreversible.";
            btnClear.ToolTipText = @"Clear the term list.";
            btnOpenXml.ToolTipText = @"Open an existing term XML of TXT file." +
                                     Environment.NewLine +
                                     @"If an alias file with a matching ASIN" +
                                     Environment.NewLine +
                                     @"is found, aliases wil automatically be" +
                                     Environment.NewLine +
                                     @"populated.";
            btnSaveXML.ToolTipText = @"Save the term list to an XML file. Any" +
                                     Environment.NewLine +
                                     @"associated aliases will be saved to an" +
                                     Environment.NewLine +
                                     @"ASIN.aliases file in the \ext folder.";
            btnDownloadTerms.ToolTipText = @"Download terms from Roentgen if any are available." +
                                           Environment.NewLine +
                                           @"Existing terms will be cleared!";
        }

        private void tsmDelete_Click(object sender, EventArgs e)
        {
            btnRemoveTerm_Click(sender, e);
        }

        private void tsmEdit_Click(object sender, EventArgs e)
        {
            btnEditTerm_Click(sender, e);
        }

        private IEnumerable<Term> GetTermsFromGrid()
        {
            var termId = 1;
            foreach (DataGridViewRow row in dgvTerms.Rows)
                yield return new Term
                {
                    Id = termId++,
                    Type = ImageUtil.AreEqual((Bitmap) row.Cells[0].Value, Resources.character)
                        ? "character"
                        : "topic",
                    TermName = row.Cells[1].Value?.ToString() ?? "",
                    Aliases = !string.IsNullOrEmpty(row.Cells[2].Value?.ToString())
                        ? row.Cells[2].Value.ToString().Split(',').Distinct().ToList()
                        : new List<string>(),
                    Desc = row.Cells[3].Value?.ToString() ?? "",
                    DescUrl = row.Cells[4].Value?.ToString() ?? "",
                    DescSrc = row.Cells[5].Value?.ToString() ?? "",
                    Match = (bool?) row.Cells[6].Value ?? false,
                    MatchCase = (bool?) row.Cells[7].Value ?? false,
                    RegexAliases = (bool?) row.Cells[9].Value ?? false
                };
        }

        private void CreateTerms()
        {
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\xml\"))
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\xml\");
            var outfile = Environment.CurrentDirectory + $@"\xml\{txtAsin.Text}.entities.xml";
            _terms.Clear();
            _terms = GetTermsFromGrid().ToList();
            XmlUtil.SerializeToFile(_terms, outfile);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count <= 0)
                return;

            if (DialogResult.OK != MessageBox.Show(@"Clearing the term list is irreversible!",
                @"Are you sure?",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2))
                return;

            dgvTerms.Rows.Clear();
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            txtLink.Text = "";
            _terms.Clear();
        }

        private void ToggleInterface(bool enabled)
        {
            _method.Invoke(toolStrip, null);
            toolStrip.Enabled = enabled;
        }

        private async void btnDownloadTerms_Click(object sender, EventArgs e)
        {
            if (!AmazonClient.IsAsin(txtAsin.Text))
            {
                MessageBox.Show($@"{txtAsin.Text} is not a valid ASIN." +
                                Environment.NewLine +
                                @"Roentgen requires one!",
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            ToggleInterface(false);
            await DownloadTermsAsync(txtAsin.Text);
            ToggleInterface(true);
        }

        private async Task DownloadTermsAsync(string asin)
        {
            try
            {
                var terms = await _roentgenClient.DownloadTermsAsync(asin, Settings.Default.roentgenRegion,
                    CancellationToken.None);
                if (terms == null)
                {
                    MessageBox.Show(@"No terms were available for this book.",
                        @"No Data Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _terms = terms.Where(term => term.Type == "character" || Settings.Default.includeTopics).ToList();
                var trueCount = _terms.Count;
                ReloadTerms();
                MessageBox.Show($@"Successfully downloaded {trueCount} terms from Roentgen!",
                    @"Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Failed to download terms:" + Environment.NewLine + $@"{e.Message}",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerateAliases_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count < 0)
                return;

            var aliasesByTerm = _aliasesService.GenerateAliases(GetTermsFromGrid()).ToDictionary();
            foreach (DataGridViewRow row in dgvTerms.Rows)
            {
                var name = row.Cells[1].Value.ToString();
                if (aliasesByTerm.TryGetValue(name, out var aliases) && aliases.Any())
                    row.Cells[2].Value = string.IsNullOrEmpty((string) row.Cells[2].Value)
                        ? string.Join(",", aliases)
                        : $"{row.Cells[2].Value}{string.Join(",", aliases)}";
            }
        }

        private void btnClearAliases_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count < 0)
                return;

            if (DialogResult.No == MessageBox.Show(@"Are you sure you want to clear all aliases?" +
                                                   Environment.NewLine +
                                                   @"This action can not be undone.",
                @"Are you sure?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2))
                return;

            foreach (DataGridViewRow row in dgvTerms.Rows)
                row.Cells[2].Value = "";
        }

        private class CustomToolStripProfessionalRenderer : ToolStripProfessionalRenderer
        {
            public CustomToolStripProfessionalRenderer()
                : base(new CustomProfessionalColorTable())
            {
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Don't draw a border
            }
        }

        private class CustomProfessionalColorTable : ProfessionalColorTable
        {
            private readonly Color _toolStripColor = Color.FromArgb(240, 240, 240);

            public override Color ToolStripGradientBegin => _toolStripColor;

            public override Color ToolStripGradientMiddle => _toolStripColor;

            public override Color ToolStripGradientEnd => _toolStripColor;
        }
    }
}