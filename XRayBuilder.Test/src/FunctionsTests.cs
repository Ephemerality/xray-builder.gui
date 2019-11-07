using NUnit.Framework;
using XRayBuilderGUI.Libraries;

namespace XRayBuilder.Test
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
    }
}
