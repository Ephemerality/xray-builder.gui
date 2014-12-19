using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ExceptionReporting;
using System.Threading;
using System.Data.SQLite;

namespace XRayBuilderGUI
{
    public partial class frmMain : Form
    {
        private Properties.Settings settings = XRayBuilderGUI.Properties.Settings.Default;
        public bool exiting = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                foreach (string fileLoc in filePaths)
                {
                    if (File.Exists(fileLoc))
                    {
                        txtMobi.Text = fileLoc;
                        return;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DragEnter += frmMain_DragEnter;
            this.DragDrop += frmMain_DragDrop;
            Version dd = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string xrayversion = dd.Major.ToString() + "." + dd.Minor.ToString() + dd.Build.ToString();
            this.Text = "X-Ray Builder GUI v" + xrayversion;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    
                    if (File.Exists(args[i]))
                    {
                        txtMobi.Text = Path.GetFullPath(args[i]);
                    }
                }
            }
            if (txtMobi.Text == "") txtMobi.Text = XRayBuilderGUI.Properties.Settings.Default.mobiFile;
            if (txtXMLFile.Text == "") txtXMLFile.Text = XRayBuilderGUI.Properties.Settings.Default.xmlFile;
            if (XRayBuilderGUI.Properties.Settings.Default.outDir == "")
            {
                XRayBuilderGUI.Properties.Settings.Default.outDir = Environment.CurrentDirectory + "/out";
                if (!Directory.Exists(XRayBuilderGUI.Properties.Settings.Default.outDir)) Directory.CreateDirectory(XRayBuilderGUI.Properties.Settings.Default.outDir);
            }
            if (XRayBuilderGUI.Properties.Settings.Default.mobi_unpack == "")
            {
                XRayBuilderGUI.Properties.Settings.Default.mobi_unpack = Environment.CurrentDirectory + "/dist/kindleunpack.exe";
            }
            txtShelfari.Text = XRayBuilderGUI.Properties.Settings.Default.shelfari;
            if (XRayBuilderGUI.Properties.Settings.Default.buildSource == "Shelfari")
                rdoShelfari.Checked = true;
            else
                rdoFile.Checked = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            XRayBuilderGUI.Properties.Settings.Default.mobiFile = txtMobi.Text;
            XRayBuilderGUI.Properties.Settings.Default.xmlFile = txtXMLFile.Text;
            XRayBuilderGUI.Properties.Settings.Default.shelfari = txtShelfari.Text;
            if (rdoShelfari.Checked)
                XRayBuilderGUI.Properties.Settings.Default.buildSource = "Shelfari";
            else
                XRayBuilderGUI.Properties.Settings.Default.buildSource = "XML";
            XRayBuilderGUI.Properties.Settings.Default.Save();
            exiting = true;
            Application.Exit();
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = Functions.getFile(txtMobi.Text);
        }
        private void btnBrowseXML_Click(object sender, EventArgs e)
        {
            txtXMLFile.Text = Functions.getFile(txtXMLFile.Text);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            Form frm = new frmSettings();
            frm.ShowDialog();
            this.TopMost = true;
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            //Check current settings
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (rdoShelfari.Checked && txtShelfari.Text == "")
            {
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
                return;
            }
            if (!File.Exists(settings.mobi_unpack))
            {
                MessageBox.Show("Kindleunpack was not found. Please review the settings page.", "Kindleunpack Not Found");
                return;
            }
            if (!Directory.Exists(settings.outDir))
            {
                MessageBox.Show("Specified output directory does not exist. Please review the settings page.", "Output Directory Not found");
                return;
            }
            //Create temp dir and ensure it exists
            string randomFile = Functions.GetTempDirectory();
            if (!Directory.Exists(randomFile))
            {
                MessageBox.Show("Temporary path not accessible for some reason.", "Temporary Directory Error");
                return;
            }
            Log("Running kindleunpack to get metadata...\r\n");
            //0 = asin, 1 = uniqid, 2 = databasename, 3 = rawML
            this.TopMost = true;
            List<string> results = Functions.GetMetaData(txtMobi.Text, settings.outDir, randomFile, settings.mobi_unpack);
            if (results.Count != 4)
            {
                Log(results[0]);
                return;
            }
            if (settings.saverawml)
            {
                Log("Saving rawML to output directory.");
                File.Copy(results[3], Path.Combine(settings.outDir, Path.GetFileName(results[3])), true);
            }
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nUniqueID: {2}\r\nAttempting to build X-Ray...", results[2], results[0], results[1]));
            Log(String.Format("Spoilers: {0}", settings.spoilers ? "Enabled" : "Disabled"));
            Log("Offset: " + settings.offset.ToString());
            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay ss;
            try
            {
                if (rdoShelfari.Checked)
                    ss = new XRay(txtShelfari.Text, results[2], results[1], results[0], this, settings.spoilers, settings.offset, "", false);
                else
                    ss = new XRay(txtXMLFile.Text, results[2], results[1], results[0], this, settings.spoilers, settings.offset, "");
                if (ss.createXRAY() > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Initial X-Ray built, adding locs and chapters...");
                //Expand the X-Ray file from the unpacked mobi
                if (ss.expandFromRawML(results[3], settings.ignoresofthyphen, !settings.useNewVersion) > 0)
                {
                    Log("Error while processing locations and chapters.");
                    return;
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler(exception);
                return;
            }
            Log("Saving X-Ray to file...");
            string newPath = settings.outDir + "\\" + ss.getXRayName();
            if (settings.useNewVersion)
            {
                try
                {
                    SQLiteConnection.CreateFile(newPath);
                } catch (Exception ex) {
                    Log("An error occurred while creating the new X-Ray database. Is it opened in another program?\n" + ex.Message);
                    return;
                }
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection(("Data Source=" + newPath + ";Version=3;"));
                m_dbConnection.Open();
                string sql;
                try
                {
                    using (StreamReader streamReader = new StreamReader("BaseDB.sql", Encoding.UTF8))
                    {
                        sql = streamReader.ReadToEnd();
                    }
                } catch (Exception ex) {
                    Log("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\n" + ex.Message);
                    m_dbConnection.Close();
                    return;
                }
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                Log("\nBuilding new X-ray database. May take a few minutes...");
                command.ExecuteNonQuery();
                command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8;", m_dbConnection);
                command.ExecuteNonQuery();
                Console.WriteLine("Done building initial database. Populating with info from source X-Ray...");
                try
                {
                    ss.PopulateDB(m_dbConnection);
                } catch (Exception ex) {
                    ExceptionHandler(ex);
                    m_dbConnection.Close();
                    return;
                }
                Console.WriteLine("Updating indices...");
                sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                    + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                    + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC);";
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                m_dbConnection.Close();
            }
            else
            {
                using (StreamWriter streamWriter = new StreamWriter(newPath, false, Encoding.Default))
                {
                    streamWriter.Write(ss.ToString());
                }
            }
            Log("XRay file created successfully!\r\nSaved to " + newPath);
            Directory.Delete(randomFile, true);
            this.TopMost = false;

            //frmXRay frm = new frmXRay(ss, results[3]);
            //frm.Show(this);
        }
        public void Log(string message)
        {
            if (exiting) return;
            txtOutput.AppendText(message + "\r\n");
        }

        private void btnShelfari_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtMobi.Text))
            {
                MessageBox.Show("Specified book was not found.", "Book Not Found");
                return;
            }
            if (txtShelfari.Text == "")
            {
                MessageBox.Show("No Shelfari link was specified.", "Missing Shelfari Link");
                return;
            }
            string path = Environment.CurrentDirectory + "/ext/" + Path.GetFileNameWithoutExtension(txtMobi.Text) + ".xml";
            try
            {
                txtXMLFile.Text = path;
                XRay xray = new XRay(txtShelfari.Text, this, settings.spoilers);
                if (xray.saveXML(path) > 0)
                {
                    Log("Error while processing.");
                    return;
                }
                Log("Shelfari info has been saved to: " + path);
            }
            catch (Exception exception)
            {
                Log("Error while saving Shelfari data to XML. Path was: " + path);
                ExceptionHandler(exception);
                return;
            }
        }

        private void rdoSource_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Text == "Shelfari")
            {
                lblShelfari.Visible = !lblShelfari.Visible;
                txtShelfari.Visible = !txtShelfari.Visible;
                lblXMLFile.Visible = !lblXMLFile.Visible;
                txtXMLFile.Visible = !txtXMLFile.Visible;
                btnBrowseXML.Visible = !btnBrowseXML.Visible;
                btnSaveShelfari.Enabled = !btnSaveShelfari.Enabled;
            }
        }

        public void ExceptionHandler(Exception exception)
        {
            ExceptionReporter reporter = new ExceptionReporter();
            reporter.ReadConfig();
            reporter.Config.ShowSysInfoTab = false;
            reporter.Config.EmailReportAddress = "revensoftware@gmail.com";
            reporter.Config.ShowAssembliesTab = false;
            reporter.Config.ShowConfigTab = false;

            reporter.Config.UserExplanationLabel = "No description required, but you can enter one if you like:";
            this.TopMost = false;
            reporter.Show(exception);
            this.TopMost = true;
            Log("Unhandled error occurred while processing this book, please report it.");
        }
    }
}
