using NUnit.Framework;

namespace XRayBuilder.Test
{
    public class KfxTests
    {
        [TestCase(@"testfiles\Frankenstein - Mary W. Shelley.kfx")]
        //[TestCase(@"testfiles\CR!0S6WXAFV0N6C1BYMGMBMR8048VHJ.kfx")]
        public void GetKfxContainer(string kfxFile)
        {
            //var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            //var kfx = new KfxContainer(fs);
            ////Assert.AreEqual(kfx.ContainerInfo.ContainerId, "CR!4FFEXVUEF2DOQB7ZQ087FJG4J8WE");

            //// TODO: Python handles 146 and 176 when getting content too
            //var content1 = kfx.Entities.Where(fragment => fragment.FragmentType == "$145");
            //var content2 = content1
            //    .Select(frag => frag.Value).OfType<IonStruct>()
            //    .SelectMany(content => content).OfType<IonList>()
            //    .SelectMany(para => para).OfType<IonString>()
            //    .Select(para => para.StringValue)
            //    .ToArray();

            //var yj = (YjContainer) kfx;
            //yj.GetBookNavigation();

            //var offset = 0;
            //foreach (var (content, index) in content2.Select((c, i) => (c, i)))
            //{
            //    var tests = content;
            //    var test2 = Encoding.UTF8.GetBytes(tests);
            //    if (tests.Contains("by the bitterest remorse"))
            //        continue;
            //    if (index == content2.Length - 1)
            //        continue;
            //    if (test2.Length != content.Length)
            //        continue;
            //    offset += test2.Length;
            //}
        }
    }
}
