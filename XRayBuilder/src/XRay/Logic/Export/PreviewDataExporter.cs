using System.IO;
using System.Linq;
using System.Text;
using XRayBuilderGUI.Libraries.Serialization.Json.Util;
using XRayBuilderGUI.XRay.Artifacts;

namespace XRayBuilderGUI.XRay.Logic.Export
{
    public class PreviewDataExporter : IPreviewDataExporter
    {
        public void Export(XRay xray, string path)
        {
            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            streamWriter.Write(JsonUtil.Serialize(new PreviewData
            {
                NumImages = 0,
                NumTerms = xray.Terms.Count(t => t.Type == "topic"),
                PreviewImages = "[]",
                ExcerptIds = new string[0],
                NumPeople = xray.Terms.Count(t => t.Type == "character")
            }));
        }
    }
}