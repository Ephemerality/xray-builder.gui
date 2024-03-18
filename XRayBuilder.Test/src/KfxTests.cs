using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ephemerality.Unpack.KFX;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

            ClassicAssert.AreEqual("B018LJYLS8", kfx.Asin);
            ClassicAssert.AreEqual("Shelley, Mary W.", kfx.Author);
            ClassicAssert.AreEqual("EBOK", kfx.CdeContentType);
            ClassicAssert.AreEqual(4096, kfx.ContainerInfo.ChunkSize);
            ClassicAssert.AreEqual(0, kfx.ContainerInfo.CompressionType);
            ClassicAssert.AreEqual(YjContainer.ContainerFormat.KfxMain, kfx.ContainerInfo.ContainerFormat);
            ClassicAssert.AreEqual("CR!ZLWPJZFVMQ5HGT49NILVDTAKVNRN", kfx.ContainerInfo.ContainerId);
            ClassicAssert.AreEqual(0, kfx.ContainerInfo.DrmScheme);
            ClassicAssert.AreEqual("CONT", kfx.ContainerInfo.Header.Signature);
            ClassicAssert.AreEqual(2, kfx.ContainerInfo.Header.Version);
            ClassicAssert.AreEqual("KPR-3.28.1", kfx.ContainerInfo.KfxGenApplicationVersion);
            ClassicAssert.AreEqual("kfxlib-20181220", kfx.ContainerInfo.KfxGenPackageVersion);
            ClassicAssert.AreEqual("Frankenstein", kfx.Title);
        }

        [TestCase(@"testfiles\Fire & Blood (A Song of Ice and - George R. R. Martin.kfx", 33)]
        [TestCase(@"testfiles\Frankenstein - Mary W. Shelley.kfx", 29)]
        public void DefaultTocTest(string kfxFile, int tocLength)
        {
            var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            var kfx = new KfxContainer(fs);
            var toc = kfx.GetDefaultToc();
            ClassicAssert.NotNull(toc);
            ClassicAssert.AreEqual(tocLength, toc.Count);
        }

        [TestCase(@"testfiles\Fire & Blood (A Song of Ice and - George R. R. Martin.kfx", 738)]
        public void GetPageCountTest(string kfxFile, int pages)
        {
            var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            var kfx = new KfxContainer(fs);
            var pageCount = kfx.GetPageCount();
            ClassicAssert.NotNull(pageCount);
            ClassicAssert.AreEqual(pages, pageCount);
        }

        [TestCase(@"testfiles\Fire & Blood (A Song of Ice and - George R. R. Martin.kfx", 1920, 1263)]
        [TestCase(@"testfiles\Frankenstein - Mary W. Shelley.kfx", 800, 600)]
        public void CoverImageTest(string kfxFile, int height, int width)
        {
            var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            var kfx = new KfxContainer(fs);
            var coverImage = kfx.CoverImage;
            ClassicAssert.NotNull(coverImage);
            ClassicAssert.AreEqual(height, coverImage.Height);
            ClassicAssert.AreEqual(width, coverImage.Width);
        }

        [TestCase(@"testfiles\Fire & Blood (A Song of Ice and - George R. R. Martin.kfx", "Baelon", 6213, 799606, 3274, 34956246)]
        [TestCase(@"testfiles\Frankenstein - Mary W. Shelley.kfx", "Frankenstein", 3, 415895, 757, 6457329)]
        public void ContentTest(string kfxFile, string search, int firstOffset, int lastOffset, int chunkCount, long sum)
        {
            var fs = new FileStream(kfxFile, FileMode.Open, FileAccess.Read);
            var kfx = new KfxContainer(fs);
            var contentChunks = kfx.GetContentChunks();
            var testSearch = FindInChunks(contentChunks, search).ToArray();

            ClassicAssert.AreEqual(chunkCount, contentChunks.Count);
            ClassicAssert.AreEqual(firstOffset, testSearch.First());
            ClassicAssert.AreEqual(lastOffset, testSearch.Last());
            ClassicAssert.AreEqual(sum, testSearch.Sum());
        }

        private IEnumerable<long> FindInChunks(IEnumerable<ContentChunk> chunks, string search)
        {
            var offset = 0L;
            foreach (var chunk in chunks)
            {
                if (chunk.ContentText != null && chunk.ContentText.Contains(search))
                    yield return offset + chunk.ContentText.IndexOf(search, StringComparison.Ordinal);

                offset += chunk.Length;
            }
        }

        // var content1 = kfx.Entities.Where(fragment => fragment.FragmentType == KfxSymbols.Content);
        // var content2 = content1
        //     .Select(frag => frag.Value).OfType<IonStruct>()
        //     .SelectMany(content => content).OfType<IonList>()
        //     .SelectMany(para => para).OfType<IonString>()
        //     .Select(para => para.StringValue)
        //     .ToArray();

        // offset = 0;
        // foreach (var (content, index) in content2.Select((c, i) => (c, i)))
        // {
        //     var tests = content;
        //     var test2 = Encoding.UTF8.GetBytes(tests);
        //     if (tests.Contains("Baelon"))
        //     {
        //         var result = offset + tests.IndexOf("Baelon", StringComparison.Ordinal);
        //         continue;
        //     }
        //     if (index == content2.Length - 1)
        //         continue;
        //     // if (test2.Length != content.Length)
        //     //     continue;
        //     offset += tests.Length;
        // }
    }
}
