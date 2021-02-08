using System;
using System.Linq;
using XRayBuilder.Core.Unpack;
using XRayBuilder.Core.XRay.Logic.Parsing;

namespace XRayBuilder.Core.Logic.PageCount
{
    public sealed class PageCountService : IPageCountService
    {
        private readonly IParagraphsService _paragraphsService;

        public PageCountService(IParagraphsService paragraphsService)
        {
            _paragraphsService = paragraphsService;
        }

        public int EstimatePageCount(IMetadata metadata)
        {
            var paragraphs = _paragraphsService.GetParagraphs(metadata).ToArray();
            if (!paragraphs.Any())
                return 0;

            var lineCount = 0;
            foreach (var paragraph in paragraphs)
            {
                var lineLength = paragraph.ContentText.Length + 1;

                lineCount += lineCount < 70
                    ? 1
                    : (int) Math.Ceiling(lineLength / 70.0);
            }

            return (int) Math.Ceiling(lineCount / 31.0);
        }
    }
}