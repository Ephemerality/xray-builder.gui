using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XRayBuilderGUI.Libraries.Progress;
using XRayBuilderGUI.Libraries.Serialization.Json.Util;
using XRayBuilderGUI.XRay.Artifacts;

namespace XRayBuilderGUI.XRay.Logic.Export
{
    public class ExporterJson : IExporter
    {
        public void Export(XRay xray, string path, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var start = (long?) xray.Srl;
            var end = (long?) xray.Erl;
            var chapters = xray.Chapters.ToArray();

            if (xray.Chapters.Count <= 0)
            {
                start = null;
                end = null;
                chapters = new[]
                {
                    new Chapter
                    {
                        Name = null,
                        Start = 1,
                        End = 9999999
                    }
                };
            }

            var xrayArtifact = new Artifacts.XRay
            {
                Asin = xray.Asin,
                Guid = $"{xray.databaseName}:{xray.Guid}",
                Version = "1",
                XRayVersion = $"{appVersion.Major}.{appVersion.Minor}{appVersion.Build}",
                Created = (xray.CreatedAt ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                Terms = xray.Terms.Select(term => new Term
                {
                    Desc = term.Desc ?? "",
                    DescSrc = term.DescSrc ?? "",
                    DescUrl = term.DescUrl ?? "",
                    Type = term.Type,
                    TermName = term.TermName,
                    Locs = term.Locs.Count > 0 ? term.Locs : new List<long[]> {new long[] {100, 100, 100, 6}}
                }).ToArray(),
                Chapters = chapters,
                Start = start,
                End = end
            };

            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            streamWriter.Write(JsonUtil.Serialize(xrayArtifact));
        }
    }
}