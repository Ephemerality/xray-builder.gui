using System.Drawing;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class XRayPanel : Panel
    {
        public XRayPanel(string type, string name, string mentions, string description)
        {
            BorderStyle = BorderStyle.None;
            Name = "XRayPanel";
            Size = new Size(460, 66);

            var pbType = new PictureBox
            {
                Image = type == "character" ? Properties.Resources.character : Properties.Resources.setting,
                Location = new Point(0, 0),
                Size = new Size(16, 16),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var lblName = new Label
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(22, -2),
                Size = new Size(335, 20),
                Text = name
            };
            
            var lblMentions = new Label
            {
                Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(370, -2),
                Size = new Size(84, 20),
                Text = mentions == "0" ? "" : $"{mentions} mentions"
            };

            var lblDescription = new Label
            {
                Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(22, 20),
                Size = new Size(430, 40),
                Text = description
            };

            var pbSeperator = new PictureBox
            {
                Image = Properties.Resources.seperator,
                Location = new Point(0, 62),
                Size = new Size(450, 2),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(lblName);
            Controls.Add(lblMentions);
            Controls.Add(lblDescription);
            Controls.Add(pbType);
            Controls.Add(pbSeperator);
        }
    }
}
