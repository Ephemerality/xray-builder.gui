using NUnit.Framework;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Test.Libraries.Primitives.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase("é", ExpectedResult = "e")]
        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = "")]
        public string RemoveDiacritics(string s)
        {
            return s.RemoveDiacritics();
        }
    }
}