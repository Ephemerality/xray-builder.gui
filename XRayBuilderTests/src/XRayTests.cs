using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using XRayBuilderGUI;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Model;
using AuthorProfile = XRayBuilderGUI.AuthorProfile;
using EndActions = XRayBuilderGUI.EndActions;

namespace XRayBuilderTests
{
    [TestFixture()]
    public class XRayTests
    {
        private static CancellationTokenSource tokens = new CancellationTokenSource();

        private static List<Book> books = new List<Book>
        {
            new Book(@"testfiles\A Storm of Swords - George R. R. Martin.rawml", @"testfiles\A Storm of Swords - George R. R. Martin.xml", "A_Storm_of_Swords", "171927873", "B000FBFN1U")
        };

        private XRay CreateXRayFromXML(string path, string db, string guid, string asin)
        {
            return new XRay(path, db, guid, asin, new Goodreads(new Logger()), new Logger(), 0, "") { unattended = true };
        }

        [Test(), TestCaseSource(nameof(books))]
        public async Task XRayXMLTest(Book book)
        {
            XRay xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            Assert.NotNull(xray);
            Assert.AreEqual(await xray.CreateXray(null, tokens.Token), 0);
        }

        [Test(), TestCaseSource(nameof(books))]
        public async Task XRayXMLAliasTest(Book book)
        {
            XRay xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            xray.ExportAndDisplayTerms();
            FileAssert.AreEqual($"ext\\{book.asin}.aliases", $"testfiles\\{book.asin}.aliases");
        }

        [Test(), TestCaseSource(nameof(books))]
        public async Task XRayXMLExpandRawMLTest(Book book)
        {
            XRay xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            Assert.AreEqual(xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, tokens.Token, false, false), 0);
            FileAssert.AreEqual($"ext\\{book.asin}.chapters", $"testfiles\\{book.asin}.chapters");
        }

        [Test(), TestCaseSource(nameof(books))]
        public async Task XRayXMLSaveNewTest(Book book)
        {
            XRay xray = CreateXRayFromXML(book.xml, book.db, book.guid, book.asin);
            await xray.CreateXray(null, tokens.Token);
            xray.ExportAndDisplayTerms();
            xray.LoadAliases();
            xray.ExpandFromRawMl(new FileStream(book.rawml, FileMode.Open), null, null, tokens.Token, false, false);
            string filename = xray.XRayName();
            string outpath = Path.Combine(Environment.CurrentDirectory, "out", filename);
            xray.SaveToFileNew(outpath, null, tokens.Token);
        }
    }

    [TestFixture()]
    public class AuthorProfileTests
    {
        private static List<BookInfo> books = new List<BookInfo>
        {
            new BookInfo("A Storm of Swords", "George R. R. Martin",
                "B000FBFN1U", "171927873", "A_Storm_of_Swords", Path.Combine(Environment.CurrentDirectory, "out"),
                "A Storm of Swords", "https://www.goodreads.com/book/show/62291",
                @"testfiles\A Storm of Swords - George R. R. Martin.rawml")
        };

        // TODO: Compare the actual contents (objects) rather than the string itself due to the order of books changing
        //[Test(), TestCaseSource(nameof(books))]
        //public async Task AuthorProfileBuildTest(BookInfo book)
        //{
        //    AuthorProfile ap = new AuthorProfile(book, new AuthorProfile.Settings
        //    {
        //        AmazonTld = "com",
        //        Android = false,
        //        OutDir = Path.Combine(Environment.CurrentDirectory, "out"),
        //        SaveBio = false,
        //        UseNewVersion = true,
        //        UseSubDirectories = false
        //    });
        //    if (!await ap.Generate()) return;

        //    using (StreamReader streamReader = new StreamReader(@"out\AuthorProfile.profile.B000FBFN1U.asc", Encoding.UTF8))
        //    using (StreamReader streamReader2 = new StreamReader(@"testfiles\AuthorProfile.profile.B000FBFN1U.asc", Encoding.UTF8))
        //        Assert.AreEqual(streamReader.ReadToEnd(), streamReader2.ReadToEnd());
        //}

        [Test()]
        public void AuthorProfileDeserializeTest()
        {
            using (StreamReader streamReader = new StreamReader(@"testfiles\AuthorProfile.profile.B000FBFN1U.asc", Encoding.UTF8))
            {
                var ap = JsonConvert.DeserializeObject<XRayBuilderGUI.Model.AuthorProfile>(streamReader.ReadToEnd());
                var outtxt = JsonConvert.SerializeObject(ap);
                File.WriteAllText(@"sampleap.txt", outtxt);
            }
        }

        [Test()]
        public void StartActionsDeserializeTest()
        {
            using (StreamReader streamReader = new StreamReader(@"testfiles\StartActions.data.B000FBFN1U.asc", Encoding.UTF8))
            {
                var sa = JsonConvert.DeserializeObject<StartActions>(streamReader.ReadToEnd());
                var outtxt = Functions.ExpandUnicode(JsonConvert.SerializeObject(sa));
                File.WriteAllText(@"samplesa.txt", outtxt, Encoding.UTF8);
            }
        }

        [Test()]
        public void EndActionsDeserializeTest()
        {
            using (StreamReader streamReader = new StreamReader(@"testfiles\EndActions.data.B000FBFN1U.asc", Encoding.UTF8))
            {
                var ea = JsonConvert.DeserializeObject<EndActions>(streamReader.ReadToEnd());
                var outtxt = Functions.ExpandUnicode(JsonConvert.SerializeObject(ea));
                File.WriteAllText(@"sampleea.txt", outtxt, Encoding.UTF8);
            }
        }
    }

    public class Book
    {
        public string rawml;
        public string xml;
        public string db;
        public string guid;
        public string asin;

        public Book(string rawml, string xml, string db, string guid, string asin)
        {
            this.rawml = rawml;
            this.xml = xml;
            this.db = db;
            this.guid = guid;
            this.asin = asin;
        }
    }
}