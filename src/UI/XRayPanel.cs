using System.Drawing;
using System.Windows.Forms;

namespace XRayBuilderGUI
{
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class XRayPanel : Panel
    {
        public XRayPanel(string type, string name, string mentions, string description)
        {
            BorderStyle = BorderStyle.None;
            Name = "XRayPanel";
            Size = new Size(258, 66);

            var pbType = new PictureBox
            {
                Image = type == "character" ? Properties.Resources.people : Properties.Resources.terms,
                Location = new Point(0, 0),
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var lblName = new Label
            {
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(26, 2),
                Size = new Size(100, 13),
                Text = name
            };

            var lblMentions = new Label
            {
                Font = new Font("Microsoft Sans Serif", 6.75F, FontStyle.Regular, GraphicsUnit.Point, 0),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(180, 3),
                Size = new Size(80, 13),
                Text = mentions == "0" ? "" : $"{mentions} mentions"
            };

            var lblDescription = new Label
            {
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(26, 19),
                Size = new Size(230, 45),
                Text = description
            };

            var pbSeperator = new PictureBox
            {
                Image = Properties.Resources.seperator,
                Location = new Point(0, 65),
                Size = new Size(350, 2)
            };

            Controls.Add(lblName);
            Controls.Add(lblMentions);
            Controls.Add(lblDescription);
            Controls.Add(pbType);
            Controls.Add(pbSeperator);
        }
    }
}
