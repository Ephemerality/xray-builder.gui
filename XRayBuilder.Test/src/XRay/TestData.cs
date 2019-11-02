using System.Collections.Generic;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.XRay.Logic.Chapters;

namespace XRayBuilder.Test.XRay
{
    public static class TestData
    {
        public static XRayBuilderGUI.XRay.XRay CreateXRayFromXML(string path, string db, string guid, string asin, Goodreads goodreads, ILogger logger, ChaptersService chaptersService)
            => new XRayBuilderGUI.XRay.XRay(path, db, guid, asin, goodreads, logger, chaptersService, true, 0, "") { Unattended = true };

        public static List<Book> Books = new List<Book>
        {
            new Book(@"testfiles\A Storm of Swords - George R. R. Martin.rawml", @"testfiles\A Storm of Swords - George R. R. Martin.xml", "A_Storm_of_Swords", "171927873", "B000FBFN1U"),
            new Book(@"testfiles\Tick Tock - James Patterson.rawml", @"testfiles\Tick Tock - James Patterson.xml", "Tick_Tock", "2219522925", "B0047Y16MG")
        };
    }
}