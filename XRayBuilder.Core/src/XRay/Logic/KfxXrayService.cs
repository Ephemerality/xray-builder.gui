using System.Linq;
using System.Threading;
using Ephemerality.Unpack.KFX;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.XRay.Logic.Parsing;
using XRayBuilder.Core.XRay.Logic.Terms;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic
{
    /// <summary>
    /// Handles adding locations and highlights to an X-Ray.
    /// Supports UTF8 instead of the 1252 encoding
    /// </summary>
    public sealed class KfxXrayService : IKfxXrayService
    {
        private readonly ILogger _logger;
        private readonly ITermsService _termsService;
        private readonly IParagraphsService _paragraphsService;

        public KfxXrayService(ILogger logger, ITermsService termsService, IParagraphsService paragraphsService)
        {
            _logger = logger;
            _termsService = termsService;
            _paragraphsService = paragraphsService;
        }

        public void AddLocations(XRay xray,
            KfxContainer kfx,
            bool skipNoLikes,
            int minClipLen,
            IProgressBar progress,
            CancellationToken token)
        {
            _logger.Log("Scanning book content...");

            var paragraphs = _paragraphsService.GetParagraphs(kfx).ToArray();

            // Set start and end of content
            // TODO Figure out how to identify the first *actual* bit of content after the TOC
            var last = paragraphs.Last();
            xray.Srl = 1;
            xray.Erl = last.Location + last.Length - 1;

            progress?.Set(0, paragraphs.Length);
            foreach (var paragraph in paragraphs)
            {
                token.ThrowIfCancellationRequested();

                foreach (var character in xray.Terms.Where(term => term.Match))
                {
                    var occurrences = _termsService.FindOccurrences(kfx, character, paragraph);
                    if (!occurrences.Any())
                        continue;

                    character.Occurrences.UnionWith(occurrences);

                    ExcerptHelper.EnhanceOrAddExcerpts(xray.Excerpts, character.Id, new IndexLength(paragraph.Location, paragraph.Length));
                }

                // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                if (xray.NotableClips != null)
                    ExcerptHelper.ProcessNotablesForParagraph(paragraph.ContentText, paragraph.Location, xray.NotableClips, xray.Excerpts, skipNoLikes, minClipLen);

                progress?.Add(1);
            }

            var missingOccurrences = xray.Terms
                .Where(term => term.Match && term.Occurrences.Count == 0)
                .Select(term => term.TermName)
                .ToArray();

            if (!missingOccurrences.Any())
                return;

            var termList = string.Join(", ", missingOccurrences);
            _logger.Log($"\r\nNo locations were found for the following terms. You should add aliases for them using the book as a reference:\r\n{termList}\r\n");
        }
    }
}