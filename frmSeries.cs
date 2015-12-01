using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public partial class frmSeries : Form
    {
        public frmSeries()
        {
            InitializeComponent();
        }

        private void frmSeries_Load(object sender, EventArgs e)
        {
            cbSeriesList.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
