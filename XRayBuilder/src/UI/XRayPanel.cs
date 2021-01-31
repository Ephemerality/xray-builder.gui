using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using XRayBuilder.Core.Libraries.Language.Pluralization;
using XRayBuilderGUI.Properties;

namespace XRayBuilderGUI.UI
{
    [DesignerCategory("Code")]
    public sealed class XRayPanel : Panel
    {
        public XRayPanel(string type, string name, string mentions, string description)
        {
            BorderStyle = BorderStyle.None;
            Name = "XRayPanel";
            Size = new Size(460, 72);
            BackColor = Color.FromArgb(240, 240, 240);

            var pbType = new PictureBox
            {
                Image = type == "character" ? Resources.character : Resources.setting,
                Location = new Point(5, 5),
                Size = new Size(16, 16),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var lblName = new Label
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(22, 5),
                Size = new Size(335, 15),
                Text = name
            };

            var lblMentions = new Label
            {
                Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(304, 5),
                Size = new Size(150, 15),
                Text = PluralUtil.Pluralize($"{int.Parse(mentions):mention}")
            };

            var lblDescription = new Label
            {
                Font = new Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0),
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(22, 24),
                Size = new Size(435, 40),
                Text = description
            };

            var pbSeperator = new PictureBox
            {
                Location = new Point(0, 71),
                Size = new Size(460, 1),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(214, 214, 214)
            };

            Controls.Add(lblName);
            Controls.Add(lblMentions);
            Controls.Add(lblDescription);
            Controls.Add(pbType);
            Controls.Add(pbSeperator);
        }
    }
}