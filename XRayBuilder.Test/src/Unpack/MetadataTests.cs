using System.IO;
using Ephemerality.Unpack;
using NUnit.Framework;
using XRayBuilder.Test.XRay;

namespace XRayBuilder.Test.Unpack
{
    [TestFixture]
    public sealed class MetadataTests
    {
        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public void GetMetaDataTest(Book book)
        {
            var md = MetadataLoader.Load(book.Bookpath);
            Assert.AreEqual(book.Asin, md.Asin);
            Assert.AreEqual(book.Guid, md.UniqueId);
            Assert.AreEqual(book.Db, md.DbName);
            Assert.AreEqual(book.Author, md.Author);
            Assert.AreEqual(book.Title, md.Title);
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