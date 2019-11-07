using System.Collections.Generic;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.XRay.Logic.Chapters;

namespace XRayBuilder.Test.XRay
{
    public static class TestData
    {
        public static XRayBuilderGUI.XRay.XRay CreateXRayFromXML(string path, string db, string guid, string asin, SecondarySourceGoodreads goodreads, ILogger logger, ChaptersService chaptersService)
            => new XRayBuilderGUI.XRay.XRay(path, db, guid, asin, goodreads, true, 0, "") { Unattended = true };

        public static List<Book> Books = new List<Book>
        {
            new Book
            {
                Rawml = @"testfiles\A Storm of Swords - George R. R. Martin.rawml",
                Xml = @"testfiles\A Storm of Swords - George R. R. Martin.xml",
                Db = "A_Storm_of_Swords",
                Guid = "171927873",
                Asin = "B000FBFN1U",
                Bookpath = @"testfiles\A Storm of Swords - George R. R. Martin.mobi"
            },
            new Book
            {
                Rawml = @"testfiles\Tick Tock - James Patterson.rawml",
                Xml = @"testfiles\Tick Tock - James Patterson.xml",
                Db = "Tick_Tock",
                Guid = "2219522925",
                Asin = "B0047Y16MG",
                Bookpath = @"testfiles\Tick Tock - James Patterson.mobi"
            }
        };
    }
}