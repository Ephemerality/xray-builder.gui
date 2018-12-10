using System.IO;
using System.Linq;
using System.Text;
using IonDotnet.Tree;
using NUnit.Framework;
using XRayBuilderGUI;
using XRayBuilderGUI.Unpack;
using XRayBuilderGUI.Unpack.KFX;

namespace XRayBuilderTests
{
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

        [TestCase(@"testfiles\A Storm of Swords - George R. R. Martin.mobi")]
        public void GetMetaDataInternal(string mobiFile)
        {
            var md = MetadataLoader.Load(mobiFile);
            Assert.AreEqual(md.Asin, "B000FBFN1U");
            Assert.AreEqual(md.UniqueId, "171927873");
            Assert.AreEqual(md.DbName, "A_Storm_of_Swords");
            Assert.AreEqual(md.Author, "George R. R. Martin");
            Assert.AreEqual(md.Title, "A Storm of Swords");
        }
    }
}
