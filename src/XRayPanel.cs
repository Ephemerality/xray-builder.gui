using System;
using System.Drawing;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    public partial class XRayPanel : Panel
    {
        private PictureBox pbType = null;
        private Label lblName = null;
        private Label lblMentions = null;
        private Label lblDescription = null;
        private PictureBox pbSeperator = null;

        public XRayPanel()
            : base()
        {
            InitializeComponent();
        }

        public XRayPanel(String type, String name, String mentions, String description)
            : base()
        {
            this.BorderStyle = BorderStyle.None;
            this.Name = "XRayPanel";
            this.Size = new Size(258, 66);

            this.pbType = new PictureBox();
            this.pbType.Image = type == "character" ? Properties.Resources.people : Properties.Resources.terms;
            this.pbType.Location = new Point(0, 0);
            this.pbType.Size = new Size(20, 20);
            this.pbType.SizeMode = PictureBoxSizeMode.StretchImage;

            this.lblName = new Label();
            //this.lblName.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.lblName.AutoSize = false;
            this.lblName.AutoEllipsis = true;
            this.lblName.TextAlign = ContentAlignment.MiddleLeft;
            this.lblName.Location = new Point(26, 2);
            this.lblName.Size = new Size(100, 13);
            this.lblName.Text = name;

            this.lblMentions = new Label();
            this.lblMentions.Font = new Font("Microsoft Sans Serif", 6.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.lblMentions.AutoSize = false;
            this.lblMentions.AutoEllipsis = true;
            this.lblMentions.TextAlign = ContentAlignment.MiddleRight;
            this.lblMentions.Location = new Point(180, 3);
            this.lblMentions.Size = new Size(80, 13);
            this.lblMentions.Text = mentions == "0" ? "" : String.Format("{0} mentions", mentions);

            this.lblDescription = new Label();
            this.lblDescription.AutoSize = false;
            this.lblDescription.AutoEllipsis = true;
            this.lblDescription.TextAlign = ContentAlignment.TopLeft;
            this.lblDescription.Location = new Point(26, 19);
            this.lblDescription.Size = new Size(230, 45);
            this.lblDescription.Text = description;

            this.pbSeperator = new PictureBox();
            this.pbSeperator.Image = Properties.Resources.seperator;
            this.pbSeperator.Location = new Point(0, 65);
            this.pbSeperator.Size = new Size(350, 2);

            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblMentions);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.pbType);
            this.Controls.Add(this.pbSeperator);

            InitializeComponent();
        }
    }
}
