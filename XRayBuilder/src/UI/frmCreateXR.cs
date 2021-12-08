using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ephemerality.Unpack;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.Libraries.Enumerables;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.Images.Util;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.Logic;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Logic.Aliases;
using XRayBuilder.Core.XRay.Logic.Parsing;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Core.XRay.Model;
using XRayBuilderGUI.Properties;
using XRayBuilderGUI.UI.Model;

namespace XRayBuilderGUI.UI
{
    public partial class frmCreateXR : Form
    {
        private readonly ITermsService _termsService;
        private readonly IAliasesRepository _aliasesRepository;
        private readonly IAmazonClient _amazonClient;
        private readonly IRoentgenClient _roentgenClient;
        private readonly IAliasesService _aliasesService;
        private readonly IDirectoryService _directoryService;
        private readonly IParagraphsService _paragraphsService;
        private readonly ILogger _logger;

        public frmCreateXR(
            ITermsService termsService,
            IAliasesRepository aliasesRepository,
            IAmazonClient amazonClient,
            IRoentgenClient roentgenClient,
            IAliasesService aliasesService,
            IDirectoryService directoryService,
            IParagraphsService paragraphsService,
            ILogger logger)
        {
            _termsService = termsService;
            _aliasesRepository = aliasesRepository;
            _amazonClient = amazonClient;
            _roentgenClient = roentgenClient;
            _aliasesService = aliasesService;
            _directoryService = directoryService;
            _paragraphsService = paragraphsService;
            _logger = logger;
            InitializeComponent();

            var pi = dgvTerms.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi?.SetValue(dgvTerms, true, null);
        }

        public void SetMetadata(IMetadata metadata)
        {
            txtAuthor.Text = metadata.Author;
            txtTitle.Text = metadata.Title;
            txtAsin.Text = metadata.Asin;
            _activeMetadata = metadata;

            // Load the paragraph content in the background before starting the occurrence worker
            _ = Task.Run(() =>
            {
                _paragraphs = _paragraphsService.GetParagraphs(metadata).ToArray();
                _ = Task.Run(() => TermRefreshWorker(_cts.Token).ConfigureAwait(false), _cts.Token).ConfigureAwait(false);
            }, _cts.Token).ConfigureAwait(false);
        }

        private readonly ToolTip _toolTip1 = new();
        private SortableBindingList<Term> _terms = new(new List<Term>());
        private IMetadata _activeMetadata;
        private Paragraph[] _paragraphs;

        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentQueue<Term> _queue = new();
        private readonly string[] _monitoredColumns =
        {
            nameof(Term.Aliases),
            nameof(Term.Match),
            nameof(Term.MatchCase),
            nameof(Term.RegexAliases),
            nameof(Term.TermName)
        };

        private DataGridViewColumn[] ColumnDefinitions
            => new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "#",
                    Name = nameof(Term.Occurrences),
                    DataPropertyName = nameof(Term.Occurrences),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                    ReadOnly = true,
                    Visible = false,
                    SortMode = DataGridViewColumnSortMode.Automatic,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                },
                new DataGridViewImageColumn
                {
                    HeaderText = "Type",
                    Name = nameof(Term.Type),
                    DataPropertyName = nameof(Term.Type),
                    ReadOnly = true,
                    Width = 39,
                    Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Name",
                    DataPropertyName = nameof(Term.TermName),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    Resizable = DataGridViewTriState.True,
                    SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Aliases",
                    Name = nameof(Term.Aliases),
                    DataPropertyName = nameof(Term.Aliases),
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        WrapMode = DataGridViewTriState.True
                    },
                    SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Description",
                    DataPropertyName = nameof(Term.Desc),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        WrapMode = DataGridViewTriState.False
                    },
                    MinimumWidth = 50,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = "M",
                    ToolTipText = "Match",
                    DataPropertyName = nameof(Term.Match),
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Resizable = DataGridViewTriState.False,
                    Width = 27
                },
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = "CS",
                    ToolTipText = "Case-sensitive",
                    DataPropertyName = nameof(Term.MatchCase),
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Resizable = DataGridViewTriState.False,
                    Width = 29
                },
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = "R",
                    ToolTipText = "Regular expression mode.",
                    DataPropertyName = nameof(Term.RegexAliases),
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Resizable = DataGridViewTriState.False,
                    Width = 23
                }
            };

        private void EnqueueTermOccurrencesRefresh(Term term)
        {
            // Don't enqueue the same term again if it is already pending or if we don't care about occurrences
            if (_activeMetadata == null || _queue.Any(t => ReferenceEquals(t, term)))
                return;
            term.Occurrences = null;
            _queue.Enqueue(term);
        }

        private async Task TermRefreshWorker(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    // TODO Add some concurrency to refresh X at a time to improve speed for larger books
                    while (_queue.TryPeek(out var term))
                    {
                        RefreshTermOccurrences(term, cancellationToken);
                        _queue.TryDequeue(out _);
                    }
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                // Ideally we'd ignore all errors for the sake of keeping the loop going but for now we'll log and exit for debugging
                catch (Exception ex)
                {
                    _logger.Log($"Unexpected error during {nameof(TermRefreshWorker)} loop - {ex.Message}");
                    return;
                }
            }
        }

        private void RefreshTermOccurrences(Term term, CancellationToken cancellationToken)
        {
            if (_activeMetadata == null)
                return;

            term.Occurrences?.Clear();

            if (term.Match)
            {
                IEnumerable<HashSet<Occurrence>> GetOccurenceSets()
                {
                    foreach (var paragraph in _paragraphs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return _termsService.FindOccurrences(_activeMetadata, term, paragraph);
                    }
                }

                SetOccurrencesThreadsafe(term, new HashSet<Occurrence>(GetOccurenceSets().SelectMany(o => o)));
            }
            else
                SetOccurrencesThreadsafe(term, new HashSet<Occurrence>());
        }

        private void SetOccurrencesThreadsafe(Term term, HashSet<Occurrence> occurrences)
        {
            if (dgvTerms.InvokeRequired)
                dgvTerms.BeginInvoke(new Action(() => SetOccurrencesThreadsafe(term, occurrences)));
            else
                term.Occurrences = occurrences;
        }

        private void btnAddTerm_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
                return;

            var term = new Term
            {
                Type = rdoCharacter.Checked ? "character" : "topic",
                TermName = txtName.Text,
                Aliases = string.IsNullOrWhiteSpace(txtAliases.Text)
                    ? new List<string>()
                    : txtAliases.Text.Split(',').ToList(),
                Occurrences = null,
                Desc = txtDescription.Text,
                Match = chkMatch.Checked,
                MatchCase = chkCase.Checked,
                RegexAliases = chkRegex.Checked
            };
            _terms.Add(term);
            EnqueueTermOccurrencesRefresh(term);

            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
        }

        // todo remove
        private void btnEditTerm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text) && DialogResult.Cancel == MessageBox.Show(
                $"The current term ({txtName.Text}) has not been added to the list!\r\n" +
                "Click Cancel if you want a chance to add the term first.\r\n" +
                "Press Ok to overwrite the current term with selected one from the term list.",
                $"Unsaved changes to {txtName.Text}",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
            {
                return;
            }

            var row = dgvTerms.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            if (row == null)
                return;

            rdoCharacter.Checked = ImageUtil.AreEqual((Bitmap)GetCellByColumnName(row, nameof(Term.Type)).FormattedValue, Resources.character);
            rdoTopic.Checked = ImageUtil.AreEqual((Bitmap)GetCellByColumnName(row, nameof(Term.Type)).FormattedValue, Resources.setting);
            txtName.Text = GetCellByColumnName(row, nameof(Term.TermName)).Value?.ToString() ?? "";
            txtAliases.Text = GetCellByColumnName(row, nameof(Term.Aliases)).FormattedValue?.ToString() ?? "";
            txtDescription.Text = GetCellByColumnName(row, nameof(Term.Desc)).Value?.ToString() ?? "";
            chkMatch.Checked = (bool?)GetCellByColumnName(row, nameof(Term.Match)).Value ?? false;
            chkCase.Checked = (bool?)GetCellByColumnName(row, nameof(Term.MatchCase)).Value ?? false;
            chkRegex.Checked = (bool?)GetCellByColumnName(row, nameof(Term.RegexAliases)).Value ?? false;
            dgvTerms.Rows.Remove(row);
        }

        [NotNull]
        private DataGridViewCell GetCellByColumnName(DataGridViewRow row, string columnName)
            => row.Cells
                   .OfType<DataGridViewCell>()
                   .FirstOrDefault(cell => cell.OwningColumn.DataPropertyName == columnName)
               ?? throw new InvalidOperationException($"Could not find cell for column {columnName}");

        private void btnOpenXml_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Title = "Open an entity file",
                Filter = "XML files (*.xml)|*.xml|TXT files (*.txt)|*.txt",
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
                        _terms = new SortableBindingList<Term>(XmlUtil.DeserializeFile<List<Term>>(openFile.FileName));
                        break;
                    case ".txt":
                        _terms = new SortableBindingList<Term>(_termsService.ReadTermsFromTxt(openFile.FileName).ToList());
                        break;
                    default:
                        MessageBox.Show($"Error: Bad file type \"{filetype}\"");
                        return;
                }
                // Fix terms that may contain any invalid aliases from a previous bug
                foreach (var term in _terms)
                {
                    if (term.Aliases == null)
                        continue;
                    term.Aliases = term.Aliases.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                }
                dgvTerms.DataSource = _terms;
                ReloadTerms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\r\n{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        private void ReloadTerms()
        {
            var aliasFile = _directoryService.GetAliasPath(txtAsin.Text);
            var d = new Dictionary<string, string>();
            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";

            try
            {
                if (!File.Exists(aliasFile))
                    return;

                if (_terms.Any(term => term.Aliases?.Count > 0))
                {
                    MessageBox.Show("The XML file already contained aliases, so the .aliases file will be ignored.");
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

                foreach (var term in _terms)
                {
                    if (d.TryGetValue(term.TermName, out var aliases))
                    {
                        term.Aliases = aliases
                            .Split(',')
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .OrderByDescending(a => a.Length)
                            .ToList();
                    }
                }
            }
            finally
            {
                foreach (var term in _terms)
                    EnqueueTermOccurrencesRefresh(term);
            }
        }

        private void btnRemoveTerm_Click(object sender, EventArgs e)
        {
            if (dgvTerms.CurrentRow != null)
                dgvTerms.Rows.Remove(dgvTerms.CurrentRow);
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
                _aliasesRepository.SaveCharactersToFile(_terms, txtAsin.Text, Settings.Default.splitAliases);
                MessageBox.Show("X-Ray entities and Alias files created sucessfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred saving the files: {ex.Message}\r\n{ex.StackTrace}",
                    "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvTerms_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!_monitoredColumns.Contains(dgvTerms[e.ColumnIndex, e.RowIndex].OwningColumn.DataPropertyName))
                return;

            if (!(dgvTerms[e.ColumnIndex, e.RowIndex].OwningRow.DataBoundItem is Term term))
                return;
            // dgvTerms.Refresh();
            EnqueueTermOccurrencesRefresh(term);
        }

        /// <summary>
        /// Used to force checkboxes to commit their state change immediately rather than waiting for the cell to lose focus
        /// </summary>
        private void dgvTerms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == -1 || e.RowIndex == -1)
                return;

            var cell = dgvTerms[e.ColumnIndex, e.RowIndex];
            if (cell is not DataGridViewCheckBoxCell)
                return;
            dgvTerms.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvTerms_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1 || e.Button != MouseButtons.Left)
                return;

            if (dgvTerms[e.ColumnIndex, e.RowIndex] is not DataGridViewTextBoxCell)
                return;

            dgvTerms.CurrentCell = dgvTerms[e.ColumnIndex, e.RowIndex];
            dgvTerms.BeginEdit(true);
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
            dgvTerms.AutoGenerateColumns = false;
            dgvTerms.AutoSize = true;
            dgvTerms.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            dgvTerms.DataSource = _terms;
            dgvTerms.CellFormatting += dgvTerms_CellFormatting;
            dgvTerms.Columns.Clear();
            dgvTerms.Columns.AddRange(ColumnDefinitions);
            if (_activeMetadata != null)
                dgvTerms.Columns[nameof(Term.Occurrences)]!.Visible = true;

            var settings = Settings.Default.TermsCreatorSettings;
            if (settings != null)
            {
                Width = settings.Width;
                Height = settings.Height;
                SetWrapDescriptionsState(settings.WrapDescription);
            }

            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";

            _toolTip1.SetToolTip(btnAddTerm, "Add this character or\r\ntopic to the term list.");
            _toolTip1.SetToolTip(btnEditTerm, "Edit the selected term. It will be removed from\r\nthe list and used to fill in the information\r\nabove. Don't forget to add to the list when done!");
            _toolTip1.SetToolTip(btnRemoveTerm, "Remove the selected term from the\r\nterm list. This action is irreversible!");
            _toolTip1.SetToolTip(btnClear, "Clear the term list.");
            _toolTip1.SetToolTip(btnOpenXml, "Open an existing term XML of TXT file. If\r\nan alias file with a matching ASIN is found,\r\naliases wil automatically be populated.");
            _toolTip1.SetToolTip(btnSaveXML, "Save the term list to an XML file. Any\r\nassociated aliases will be saved to an\r\nASIN.aliases file in the /ext folder.");
            _toolTip1.SetToolTip(btnDownloadTerms, "Download terms from Roentgen if any are\r\navailable. Existing terms will be cleared!");
            _toolTip1.SetToolTip(btnGenerateAliases, "Automatically split character\r\nnames into aliases.");
            _toolTip1.SetToolTip(btnClearAliases, "Clear all aliases.");

            _toolTip1.SetToolTip(chkMatch, "Usually enabled. Disabling this option is useful\r\nwhen you want a character to be displayed but\r\ntheir name doesn't work well for matching.");
            _toolTip1.SetToolTip(chkCase, "Whether or not to match this term in\r\ncase-sensitive mode. In general, characters\r\nwill use this and others will not.");
            _toolTip1.SetToolTip(chkRegex, "When enabled, the aliases will be treated\r\nas a set of regular expressions.");

            _toolTip1.SetToolTip(rdoCharacter, "A character is an individual, fictional or\r\nreal, in your book. Examples of characters include\r\n\"Don Quixote\", \"Warren Buffett\", and \"Darth Vader\".");
            _toolTip1.SetToolTip(rdoTopic, "Terms are places, organizations, or phrases, and can\r\nalso be fictional or real. Examples of terms include\r\n\"Westeros\", \"IBM\", and \"deadlock\".");

            _toolTip1.SetToolTip(chkAllowResizeName,"Allow resizing of the term name column\r\nso that the entire name is visible.");
            _toolTip1.SetToolTip(chkWrapDescriptions, "Resize term rows so that the\r\nentire description is visible.");
        }

        private void dgvTerms_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            switch (dgvTerms.Columns[e.ColumnIndex].Name)
            {
                case nameof(Term.Occurrences):
                    e.Value = ((HashSet<Occurrence>) e.Value)?.Count.ToString("N0") ?? "…";
                    break;
                case nameof(Term.Type):
                {
                    var cell = dgvTerms[e.ColumnIndex, e.RowIndex];
                    var value = (string) e.Value;
                    cell.ToolTipText = value == "character" ? "Character" : "Setting";
                    e.Value = value == "character" ? Resources.character : Resources.setting;
                    break;
                }
                case nameof(Term.Aliases):
                    e.Value = string.Join(",", ((List<string>) e.Value).OrderByDescending(s => s.Length));
                    break;
            }
        }

        private void dgvTerms_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            switch (dgvTerms.Columns[e.ColumnIndex].Name)
            {
                case nameof(Term.Aliases):
                    var aliases = e.Value.ToString()
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .OrderByDescending(s => s.Length)
                        .ToList();
                    e.Value = aliases;
                    e.ParsingApplied = true;
                    break;
            }
        }

        private void tsmDelete_Click(object sender, EventArgs e)
        {
            btnRemoveTerm_Click(sender, e);
        }

        private void CreateTerms()
        {
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\xml\"))
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\xml\");
            var outfile = Environment.CurrentDirectory + $@"\xml\{txtAsin.Text}.entities.xml";
            XmlUtil.SerializeToFile(_terms, outfile);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (!_terms.Any())
                return;

            if (DialogResult.OK != MessageBox.Show("Clearing the term list is irreversible!", "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                return;

            txtName.Text = "";
            txtAliases.Text = "";
            txtDescription.Text = "";
            _terms.Clear();
        }

        private void ToggleInterface(bool enabled)
        {
            foreach (var c in Controls.OfType<Button>())
                c.Enabled = enabled;
            chkAllowResizeName.Enabled = enabled;
            chkWrapDescriptions.Enabled = enabled;
        }

        private async void btnDownloadTerms_Click(object sender, EventArgs e)
        {
            if (!AmazonClient.IsAsin(txtAsin.Text))
            {
                MessageBox.Show(
                    string.IsNullOrEmpty(txtAsin.Text)
                        ? "ASIN is missing.\r\nRoentgen requires one!"
                        : $"'{txtAsin.Text}' is not a valid ASIN.\r\nRoentgen requires one!",
                    "Invalid ASIN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
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
                var terms = await _roentgenClient.DownloadTermsAsync(asin, Settings.Default.roentgenRegion, CancellationToken.None);
                if (terms == null)
                {
                    MessageBox.Show("No terms were available for this book :(", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _terms.Clear();
                foreach (var term in terms.Where(term => term.Type == "character" || Settings.Default.includeTopics))
                    _terms.Add(term);
                var trueCount = _terms.Count;
                ReloadTerms();
                MessageBox.Show($"Successfully downloaded {trueCount} terms from Roentgen!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to download terms: {e.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnGenerateAliases_Click(object sender, EventArgs e)
        {
            var aliasesByTerm = _aliasesService.GenerateAliases(_terms);
            foreach (var (term, aliases) in aliasesByTerm)
            {
                term.Aliases = aliases.ToList();
                EnqueueTermOccurrencesRefresh(term);
            }
        }

        private void btnClearAliases_Click(object sender, EventArgs e)
        {
            if (dgvTerms.Rows.Count < 0)
                return;

            if (DialogResult.No == MessageBox.Show("Are you sure you want to clear all aliases?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                return;

            foreach (var term in _terms)
            {
                term.Aliases.Clear();
                EnqueueTermOccurrencesRefresh(term);
            }

            dgvTerms.Refresh();
        }

        private void frmCreateXR_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel();

            Settings.Default.TermsCreatorSettings ??= new TermsCreatorSettings();
            Settings.Default.TermsCreatorSettings.Width = Width;
            Settings.Default.TermsCreatorSettings.Height = Height;
            Settings.Default.TermsCreatorSettings.WrapDescription = chkWrapDescriptions.Checked;
            Settings.Default.TermsCreatorSettings.AllowResizeNameColumn = chkAllowResizeName.Checked;
        }

        private void chkWrapDescriptions_CheckedChanged(object sender, EventArgs e)
        {
            SetWrapDescriptionsState(chkWrapDescriptions.Checked);
        }

        private void chkAllowResizeName_CheckedChanged(object sender, EventArgs e)
        {
            SetAllowResizeNameState(chkAllowResizeName.Checked);
        }

        private void SetWrapDescriptionsState(bool wrapDescriptions)
        {
            var descriptionColumn = dgvTerms.Columns
                .OfType<DataGridViewTextBoxColumn>()
                .First(def => def.DataPropertyName == nameof(Term.Desc));
            descriptionColumn.DefaultCellStyle.WrapMode = wrapDescriptions
                ? DataGridViewTriState.True
                : DataGridViewTriState.False;
        }

        private void SetAllowResizeNameState(bool allowResize)
        {
            var nameColumn= dgvTerms.Columns
                .OfType<DataGridViewTextBoxColumn>()
                .First(def => def.DataPropertyName == nameof(Term.TermName));
            nameColumn.AutoSizeMode = allowResize
                ? DataGridViewAutoSizeColumnMode.None
                : DataGridViewAutoSizeColumnMode.AllCells;
        }
    }
}