using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ephemerality.Unpack;
using Ephemerality.Unpack.KFX;
using Ephemerality.Unpack.Mobi;
using HtmlAgilityPack;

namespace XRayBuilder.Core.XRay.Logic.Parsing
{
    public sealed class ParagraphsService : IParagraphsService
    {
        private readonly Encoding _encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);

        public IEnumerable<Paragraph> GetParagraphs(IMetadata metadata)
            => metadata switch
            {
                MobiMetadata _ => GetRegular(metadata),
                KfxContainer kfx => GetKfx(kfx),
                _ => GetRegular(metadata)
            };

        private IEnumerable<Paragraph> GetRegular(IMetadata metadata)
        {
            var locOffset = metadata.IsAzw3 ? -16 : 0;

            var rawml = metadata.GetRawMlStream();
            var doc = new HtmlDocument();
            doc.Load(rawml, _encoding);

            var nodes = doc.DocumentNode.SelectNodes("//p")
                        ?? doc.DocumentNode.SelectNodes("//div[@class='paragraph']")
                        ?? doc.DocumentNode.SelectNodes("//div[@class='p-indent']")
                        ?? doc.DocumentNode.SelectNodes("//div");

            if (nodes == null)
                return Enumerable.Empty<Paragraph>();

            return nodes
                .Where(node => node.FirstChild != null && !string.IsNullOrEmpty(node.InnerText) && node.FirstChild.StreamPosition >= 0)
                .Select(node => new Paragraph
                {
                    ContentHtml = node.InnerHtml,
                    ContentText = node.InnerText,
                    Location = node.FirstChild.StreamPosition + locOffset
                });
        }

        private IEnumerable<Paragraph> GetKfx(YjContainer kfx)
            => kfx.GetContentChunks()
                .Where(contentChunk => contentChunk.ContentText != null)
                .Select(contentChunk => new Paragraph
                {
                    ContentText = contentChunk.ContentText,
                    Location = contentChunk.Pid
                });
    }
}