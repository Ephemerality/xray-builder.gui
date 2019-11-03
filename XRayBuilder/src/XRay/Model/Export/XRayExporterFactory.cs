using System.Collections.Generic;
using JetBrains.Annotations;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.XRay.Logic.Export;

namespace XRayBuilderGUI.XRay.Model.Export
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

        protected override Dictionary<Enum, IXRayExporter> Dictionary { get; }
    }
}