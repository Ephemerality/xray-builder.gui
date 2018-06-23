using System.Drawing;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class XRayPanel : Panel
    {
        private PictureBox pbType;
        private Label lblName;
        private Label lblMentions;
        private Label lblDescription;
        private PictureBox pbSeperator;

        public XRayPanel(string type, string name, string mentions, string description) : base()
        {
            BorderStyle = BorderStyle.None;
            Name = "XRayPanel";
            Size = new Size(258, 66);

            pbType = new PictureBox();
            pbType.Image = type == "character" ? Properties.Resources.people : Properties.Resources.terms;
            pbType.Location = new Point(0, 0);
            pbType.Size = new Size(20, 20);
            pbType.SizeMode = PictureBoxSizeMode.StretchImage;

            lblName = new Label();
            lblName.AutoSize = false;
            lblName.AutoEllipsis = true;
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            lblName.Location = new Point(26, 2);
            lblName.Size = new Size(100, 13);
            lblName.Text = name;

            lblMentions = new Label();
            lblMentions.Font = new Font("Microsoft Sans Serif", 6.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblMentions.AutoSize = false;
            lblMentions.AutoEllipsis = true;
            lblMentions.TextAlign = ContentAlignment.MiddleRight;
            lblMentions.Location = new Point(180, 3);
            lblMentions.Size = new Size(80, 13);
            lblMentions.Text = mentions == "0" ? "" : $"{mentions} mentions";

            lblDescription = new Label();
            lblDescription.AutoSize = false;
            lblDescription.AutoEllipsis = true;
            lblDescription.TextAlign = ContentAlignment.TopLeft;
            lblDescription.Location = new Point(26, 19);
            lblDescription.Size = new Size(230, 45);
            lblDescription.Text = description;

            pbSeperator = new PictureBox();
            pbSeperator.Image = Properties.Resources.seperator;
            pbSeperator.Location = new Point(0, 65);
            pbSeperator.Size = new Size(350, 2);

            Controls.Add(lblName);
            Controls.Add(lblMentions);
            Controls.Add(lblDescription);
            Controls.Add(pbType);
            Controls.Add(pbSeperator);
        }
    }
}
