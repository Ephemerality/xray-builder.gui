using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public partial class frmPreviewXR : Form
    {
        public frmPreviewXR()
        {
            InitializeComponent();
        }

        // TODO: Add notable clips
        public void PopulateXRay(List<XRay.Term> terms)
        {
            flpPeople.Controls.Clear();
            flpTerms.Controls.Clear();

            foreach (XRay.Term t in terms)
            {
                XRayPanel p = new XRayPanel(t.Type, t.TermName, Math.Max(t.Occurrences.Count, t.Locs.Count).ToString(), t.Desc);
                if (t.Type == "character")
                    flpPeople.Controls.Add(p);
                if (t.Type == "topic")
                    flpTerms.Controls.Add(p);
            }
            tcXray.SelectedIndex = 0;
        }
    }
}
