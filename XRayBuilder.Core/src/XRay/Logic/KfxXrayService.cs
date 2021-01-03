using System.Linq;
using System.Threading;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Unpack.KFX;
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

        public KfxXrayService(ILogger logger, ITermsService termsService)
        {
            _logger = logger;
            _termsService = termsService;
        }

        public void AddLocations(XRay xray,
            KfxContainer kfx,
            bool skipNoLikes,
            int minClipLen,
            IProgressBar progress,
            CancellationToken token)
        {
            _logger.Log("Scanning book content...");
            var contentChunks = kfx.GetContentChunks();

            // Set start and end of content
            // TODO Figure out how to identify the first *actual* bit of content after the TOC
            var last = contentChunks.Last();
            xray.Srl = 1;
            xray.Erl = last.Pid + last.Length - 1;

            var offset = 0;
            progress?.Set(0, contentChunks.Count);
            foreach (var contentChunk in contentChunks)
            {
                token.ThrowIfCancellationRequested();

                if (contentChunk.ContentText != null)
                {
                    foreach (var character in xray.Terms.Where(term => term.Match))
                    {
                        var paragraphInfo = new IndexLength(offset, contentChunk.Length);
                        var occurrences = _termsService.FindOccurrences(kfx, character, contentChunk.ContentText, paragraphInfo);
                        if (!occurrences.Any())
                            continue;

                        character.Occurrences.UnionWith(occurrences);

                        ExcerptHelper.EnhanceOrAddExcerpts(xray.Excerpts, character.Id, paragraphInfo);
                    }

                    // Attempt to match downloaded notable clips, not worried if no matches occur as some will be added later anyway
                    if (xray.NotableClips != null)
                        ExcerptHelper.ProcessNotablesForParagraph(contentChunk.ContentText, offset, xray.NotableClips, xray.Excerpts, skipNoLikes, minClipLen);

                    progress?.Add(1);
                }

                offset += contentChunk.Length;
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