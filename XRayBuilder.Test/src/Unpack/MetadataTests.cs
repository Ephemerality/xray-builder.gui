using System.IO;
using NUnit.Framework;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Test.XRay;

namespace XRayBuilder.Test.Unpack
{
    [TestFixture]
    public sealed class MetadataTests
    {
        [TestCase(@"testfiles\A Storm of Swords - George R. R. Martin.mobi")]
        public void GetMetaDataTest(string mobiFile)
        {
            var md = MetadataLoader.Load(mobiFile);
            Assert.AreEqual(md.Asin, "B000FBFN1U");
            Assert.AreEqual(md.UniqueId, "171927873");
            Assert.AreEqual(md.DbName, "A_Storm_of_Swords");
            Assert.AreEqual(md.Author, "George R. R. Martin");
            Assert.AreEqual(md.Title, "A Storm of Swords");
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public void RawlTest(Book book)
        {
            var metadata = MetadataLoader.Load(book.Bookpath);
            var expected = File.ReadAllBytes(book.Rawml);
            Assert.AreEqual(expected, metadata.GetRawMl());
        }
    }
}