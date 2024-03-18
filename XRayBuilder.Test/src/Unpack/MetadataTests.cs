using System.IO;
using Ephemerality.Unpack;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using XRayBuilder.Test.XRay;

namespace XRayBuilder.Test.Unpack
{
    [TestFixture]
    public sealed class MetadataTests
    {
        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public void GetMetaDataTest(Book book)
        {
            var md = MetadataReader.Load(book.Bookpath);
            ClassicAssert.AreEqual(book.Asin, md.Asin);
            ClassicAssert.AreEqual(book.Guid, md.UniqueId);
            ClassicAssert.AreEqual(book.Db, md.DbName);
            ClassicAssert.AreEqual(book.Author, md.Author);
            ClassicAssert.AreEqual(book.Title, md.Title);
        }

        [Test, TestCaseSource(typeof(TestData), nameof(TestData.Books))]
        public void RawlTest(Book book)
        {
            var metadata = MetadataReader.Load(book.Bookpath);
            var expected = File.ReadAllBytes(book.Rawml);
            ClassicAssert.AreEqual(expected, metadata.GetRawMl());
        }
    }
}