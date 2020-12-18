using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Serialization.Json.Util;
using XRayBuilder.Core.XRay.Artifacts;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic.Export
{
    public sealed class XRayExporterJson : IXRayExporter
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
                Guid = $"{xray.DatabaseName}:{xray.Guid}",
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
                    Occurrences = term.Occurrences.Count > 0
                        ? term.Occurrences
                        : new List<Occurrence>
                        {
                            new Occurrence
                            {
                                Excerpt = new IndexLength(100, 100),
                                Highlight = new IndexLength(100, 6)
                            }
                        }
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