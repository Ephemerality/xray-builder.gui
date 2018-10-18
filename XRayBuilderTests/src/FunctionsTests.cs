using System;
using System.IO;
using NUnit.Framework;
using XRayBuilderGUI;

namespace XRayBuilderTests
{
    [SetUpFixture]
    public class MySetUpClass
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            if (Directory.Exists("ext")) Directory.Delete("ext", true);
            if (Directory.Exists("out")) Directory.Delete("out", true);
            Directory.CreateDirectory("ext");
            Directory.CreateDirectory("out");
        }
    }

    [TestFixture]
    public class FunctionsTests
    {
        [TestCase("@#$%_\"", ExpectedResult="'")]
        [TestCase("\u201C", ExpectedResult = "'")]
        [TestCase("\u201D", ExpectedResult = "'")]
        [TestCase("<br>", ExpectedResult = "")]
        [TestCase("&#133;", ExpectedResult = "…")]
        [TestCase("&amp;#133;", ExpectedResult = "…")]
        [TestCase("&#169;", ExpectedResult = "")]
        [TestCase(" . . .", ExpectedResult = "…")]
        [TestCase("&amp;#169;", ExpectedResult = "")]
        [TestCase("&#174;", ExpectedResult = "")]
        [TestCase(" - ", ExpectedResult = "—")]
        [TestCase("--", ExpectedResult = "—")]
        [TestCase("&mdash;", ExpectedResult = "")]
        [TestCase("<b></b>", ExpectedResult = "")]
        [TestCase("\t\r\n•", ExpectedResult = "")]
        [TestCase("       ", ExpectedResult = "")]
        [TestCase(" s", ExpectedResult = "s")]
        [TestCase("s ", ExpectedResult = "s")]
        [TestCase("sentence. …", ExpectedResult = "sentence.")]
        [TestCase("@#$%_&#169;&amp;#169;&#174;&amp;#174;&mdash;<b></b>“”\t\n\r•    ", ExpectedResult = "''")]
        public string CleanString(string s)
        {
            return s.Clean();
        }

        [TestCase("é", ExpectedResult = "e")]
        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = "")]
        public string RemoveDiacritics(string s)
        {
            return s.RemoveDiacritics();
        }

        [TestCase(@"testfiles\A Storm of Swords - George R. R. Martin.mobi", "out", false, "")]
        public void GetMetaDataInternal(string mobiFile, string outDir, bool saveRawML, string randomFile)
        {
            var md = Functions.GetMetaDataInternal(mobiFile, outDir, saveRawML, randomFile);
            Assert.AreEqual(md.ASIN, "B000FBFN1U");
            Assert.AreEqual(md.UniqueID, "171927873");
            Assert.AreEqual(md.DBName, "A_Storm_of_Swords");
            Assert.AreEqual(md.Author, "George R. R. Martin");
            Assert.AreEqual(md.Title, "A Storm of Swords");
        }
    }
}
