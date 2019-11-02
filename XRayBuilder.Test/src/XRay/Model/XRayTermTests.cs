using System.Collections.Generic;
using NUnit.Framework;
using XRayBuilderGUI.XRay.Artifacts;

namespace XRayBuilder.Test.XRay.Model
{
    [TestFixture]
    public class XRayTermTests
    {
        [Test]
        public void ToStringLocsTest()
        {
            var term = new Term
            {
                Aliases = new List<string> {"Billy", "Bill"},
                Desc = "Definitely Billy",
                Id = 1,
                Locs = new List<long[]> {new long[] {123,123,123,1}},
                Match = true,
                Occurrences = new List<int[]> {new [] {123, 456}},
                Type = "character",
                DescSrc = "Shelfari",
                DescUrl = "https://www.shelfari.com",
                MatchCase = false,
                TermName = "Billificent"
            };

            var termString = term.ToString();

            Assert.AreEqual(@"{""type"":""character"",""term"":""Billificent"",""desc"":""Definitely Billy"",""descSrc"":""Shelfari"",""descUrl"":""https://www.shelfari.com"",""locs"":[[123,123,123,1]]}", termString);
        }
        [Test]
        public void ToStringNoLocsTest()
        {
            var term = new Term
            {
                Aliases = new List<string> {"Billy", "Bill"},
                Desc = "Definitely Billy",
                Id = 1,
                Match = true,
                Occurrences = new List<int[]> {new [] {123, 456}},
                Type = "character",
                DescSrc = "Shelfari",
                DescUrl = "https://www.shelfari.com",
                MatchCase = false,
                TermName = "Billificent"
            };

            var termString = term.ToString();

            Assert.AreEqual(@"{""type"":""character"",""term"":""Billificent"",""desc"":""Definitely Billy"",""descSrc"":""Shelfari"",""descUrl"":""https://www.shelfari.com"",""locs"":[[100,100,100,6]]}", termString);
        }
    }
}