using System;
using System.IO;
using NUnit.Framework;
using XRayBuilderGUI;

namespace XRayBuilder.Test
{
    [SetUpFixture]
    public class XRayBuilderTests
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            HttpClient.Instance = new HttpClient(null);
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            if (Directory.Exists("ext")) Directory.Delete("ext", true);
            if (Directory.Exists("out")) Directory.Delete("out", true);
            Directory.CreateDirectory("ext");
            Directory.CreateDirectory("out");
        }
    }
}
