using System.IO;
using System.Linq;
using Amazon.IonDotnet.Tree.Impl;
using NUnit.Framework;
using XRayBuilder.Core.Unpack.KFX;

namespace XRayBuilder.Test
{
    public class KfxTests
    {
        // TODO Parameterize test values
        [TestCase(@"testfiles\Frankenstein - Mary W. Shelley.kfx")]
        public void GetKfxContainerMetadataTest(string kfxFile)
        {
            var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            var kfx = new KfxContainer(fs);

            Assert.AreEqual("B018LJYLS8", kfx.Asin);
            Assert.AreEqual("Shelley, Mary W.", kfx.Author);
            Assert.AreEqual("EBOK", kfx.CdeContentType);
            Assert.AreEqual(4096, kfx.ContainerInfo.ChunkSize);
            Assert.AreEqual(0, kfx.ContainerInfo.CompressionType);
            Assert.AreEqual(YjContainer.ContainerFormat.KfxMain, kfx.ContainerInfo.ContainerFormat);
            Assert.AreEqual("CR!ZLWPJZFVMQ5HGT49NILVDTAKVNRN", kfx.ContainerInfo.ContainerId);
            Assert.AreEqual(0, kfx.ContainerInfo.DrmScheme);
            Assert.AreEqual("CONT", kfx.ContainerInfo.Header.Signature);
            Assert.AreEqual(2, kfx.ContainerInfo.Header.Version);
            Assert.AreEqual("KPR-3.28.1", kfx.ContainerInfo.KfxGenApplicationVersion);
            Assert.AreEqual("kfxlib-20181220", kfx.ContainerInfo.KfxGenPackageVersion);
            Assert.AreEqual("Frankenstein", kfx.Title);
        }

        [TestCase(@"testfiles\Frankenstein - Mary W. Shelley.kfx", 29)]
        public void DefaultTocTest(string kfxFile, int tocLength)
        {
            var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            var kfx = new KfxContainer(fs);
            var toc = kfx.GetDefaultToc();
            Assert.NotNull(toc);
            Assert.AreEqual(tocLength, toc.Count);
        }

        //// TODO: Python handles 146 and 176 when getting content too
        // var content1 = kfx.Entities.Where(fragment => fragment.FragmentType == KfxSymbols.Content);
        // var content2 = content1
        //     .Select(frag => frag.Value).OfType<IonStruct>()
        //     .SelectMany(content => content).OfType<IonList>()
        //     .SelectMany(para => para).OfType<IonString>()
        //     .Select(para => para.StringValue)
        //     .ToArray();

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
