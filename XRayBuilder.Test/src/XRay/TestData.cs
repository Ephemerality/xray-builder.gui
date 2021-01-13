using System.Collections.Generic;

namespace XRayBuilder.Test.XRay
{
    public static class TestData
    {
        public static List<Book> Books = new List<Book>
        {
            new Book
            {
                Rawml = @"testfiles\A Storm of Swords - George R. R. Martin.rawml",
                Xml = @"testfiles\A Storm of Swords - George R. R. Martin.xml",
                Db = "A_Storm_of_Swords",
                Guid = "171927873",
                Asin = "B000FBFN1U",
                Bookpath = @"testfiles\A Storm of Swords - George R. R. Martin.mobi",
                Author = "George R. R. Martin",
                Title = "A Storm of Swords"
            },
            new Book
            {
                Rawml = @"testfiles\Tick Tock - James Patterson.rawml",
                Xml = @"testfiles\Tick Tock - James Patterson.xml",
                Db = "Tick_Tock",
                Guid = "2219522925",
                Asin = "B0047Y16MG",
                Bookpath = @"testfiles\Tick Tock - James Patterson.mobi",
                Author = "James Patterson",
                Title = "Tick Tock"
            },
            new Book
            {
                Rawml = @"testfiles\Firewing (Silverwing)_nodrm.rawml",
                Xml = @"testfiles\Firewing (Silverwing)_nodrm.xml",
                Db = "CR!EVRXGDB8416WN5C6CC4HWZS1QH6M",
                Guid = "1127971787",
                Asin = "B00655KOZK",
                Bookpath = @"testfiles\Firewing (Silverwing)_nodrm-huffcdic.azw3",
                Author = "Oppel, Kenneth",
                Title = "Firewing"
            }
        };
    }
}