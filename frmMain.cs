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

namespace XRayBuilderGUI
{
    public partial class frmMain : Form
    {
        private Properties.Settings settings = XRayBuilderGUI.Properties.Settings.Default;

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
            if(txtMobi.Text == "") txtMobi.Text = XRayBuilderGUI.Properties.Settings.Default.mobiFile;
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
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            XRayBuilderGUI.Properties.Settings.Default.mobiFile = txtMobi.Text;
            XRayBuilderGUI.Properties.Settings.Default.shelfari = txtShelfari.Text;
            XRayBuilderGUI.Properties.Settings.Default.Save();
            Application.Exit();
        }

        private void btnBrowseMobi_Click(object sender, EventArgs e)
        {
            txtMobi.Text = Functions.getFile(txtMobi.Text);
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
            Log(String.Format("Got metadata!\r\nDatabase Name: {0}\r\nASIN: {1}\r\nUniqueID: {2}\r\nAttempting to build X-Ray...", results[2], results[0], results[1]));
            Log(String.Format("Spoilers: {0}", settings.spoilers ? "Enabled" : "Disabled"));
            //Console.WriteLine("Location Offset: {0}", offset);
            //Create X-Ray and attempt to create the base file (essentially the same as the site)
            XRay ss = new XRay(txtShelfari.Text, results[2], results[1], results[0], this, settings.spoilers, 0, "", false);
            if (ss.createXRAY() > 0)
            {
                Log("Error while processing.");
                return;
            }
            Log("Initial X-Ray built, adding locs and chapters...");
            //Expand the X-Ray file from the unpacked mobi
            if (ss.expandFromRawML(results[3]) > 0)
            {
                Log("Error while processing locations and chapters.");
                return;
            }
            Log("Saving X-Ray to file...");
            using (StreamWriter streamWriter = new StreamWriter(settings.outDir + "\\" + ss.getXRayName(), false, Encoding.Default))
            {
                streamWriter.Write(ss.ToString());
            }
            Log("XRay file created successfully!\r\nSaved to " + settings.outDir + "\\" + ss.getXRayName());
            Directory.Delete(randomFile, true);
            this.TopMost = false;

            //frmXRay frm = new frmXRay(ss, results[3]);
            //frm.Show(this);
        }
        public void Log(string message)
        {
            txtOutput.AppendText(message + "\r\n");
        }
    }
}
