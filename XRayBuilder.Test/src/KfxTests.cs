using System.IO;
using System.Linq;
using Amazon.IonDotnet.Tree.Impl;
using NUnit.Framework;
using XRayBuilder.Core.Unpack.KFX;

namespace XRayBuilder.Test
{
    public class KfxTests
    {
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
    }
}
