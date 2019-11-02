namespace XRayBuilderGUI.XRay.Logic.Export
{
    public interface IPreviewDataExporter
    {
        /// <summary>
        /// Export a previewData file for <paramref name="xray"/> to <paramref name="path"/>
        /// </summary>
        // TODO support images & excerpts
        void Export(XRay xray, string path);
    }
}