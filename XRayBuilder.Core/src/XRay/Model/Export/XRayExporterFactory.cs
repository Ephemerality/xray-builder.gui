using System.Collections.Generic;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.XRay.Logic.Export;

namespace XRayBuilder.Core.XRay.Model.Export
{
    [UsedImplicitly]
    public sealed class XRayExporterFactory : Factory<XRayExporterFactory.Enum, IXRayExporter>
    {
        public XRayExporterFactory(XRayExporterJson xrayExporterJson, XRayExporterSqlite xrayExporterSqlite)
        {
            Dictionary = new Dictionary<Enum, IXRayExporter>
            {
                {Enum.Json, xrayExporterJson},
                {Enum.Sqlite, xrayExporterSqlite}
            };
        }

        public enum Enum
        {
            Json = 1,
            Sqlite
        }

        protected override IReadOnlyDictionary<Enum, IXRayExporter> Dictionary { get; }
    }
}