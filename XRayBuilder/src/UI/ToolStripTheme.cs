using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace XRayBuilderGUI.UI
{
    /// <summary>
    /// Utility for maintaining a standardized toolstrip theme between all forms
    /// </summary>
    public static class ToolStripTheme
    {
        // There is private method ClearAllSelections in ToolStrip class,
        // which removes selections from items. You can invoke it via reflection:
        // https://stackoverflow.com/a/10341622
        private static readonly MethodInfo ClearAllSelectionsMethod = typeof(ToolStrip).GetMethod("ClearAllSelections", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Thread-safe
        /// </summary>
        public static void ClearAllSelections(this ToolStrip toolstrip)
        {
            if (toolstrip.InvokeRequired)
                toolstrip.BeginInvoke(() => ClearAllSelections(toolstrip));
            else
                ClearAllSelectionsMethod.Invoke(toolstrip, null);
        }

        public static ToolStripProfessionalRenderer Renderer() => new CustomToolStripProfessionalRenderer();

        private sealed class CustomToolStripProfessionalRenderer : ToolStripProfessionalRenderer
        {
            public CustomToolStripProfessionalRenderer() : base(new CustomProfessionalColorTable()) { }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Don't draw a border
            }
        }

        private sealed class CustomProfessionalColorTable : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin { get; } = Color.FromArgb(240, 240, 240);
            public override Color ToolStripGradientMiddle => ToolStripGradientBegin;
            public override Color ToolStripGradientEnd => ToolStripGradientBegin;
        }
    }
}