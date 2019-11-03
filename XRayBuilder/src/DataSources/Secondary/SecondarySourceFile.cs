using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using XRayBuilderGUI.DataSources.Secondary.Model;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Libraries.Progress;
using XRayBuilderGUI.Model;
using XRayBuilderGUI.XRay.Artifacts;

namespace XRayBuilderGUI.DataSources.Secondary
{
    public class SecondarySourceFile : ISecondarySource
    {
        private readonly ILogger _logger;

        public SecondarySourceFile(ILogger logger)
        {
            _logger = logger;
        }

        public string Name { get; } = "File";
        public bool SearchEnabled { get; } = false;
        public int UrlLabelPosition { get; } = 0;
        public bool SupportsNotableClips { get; } = false;

        public Task<IEnumerable<Term>> GetTermsAsync(string xmlFile, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            _logger.Log("Loading terms from file...");
            var filetype = Path.GetExtension(xmlFile);
            switch (filetype)
            {
                case ".xml":
                    return Task.FromResult((IEnumerable<Term>) Functions.XmlDeserialize<Term[]>(xmlFile));
                case ".txt":
                {
                    return Task.FromResult(LoadTermsFromTxt(xmlFile));
                }
                default:
                    _logger.Log("Error: Bad file type \"" + filetype + "\"");
                    break;
            }

            return Task.FromResult(Enumerable.Empty<Term>());
        }

        private IEnumerable<Term> LoadTermsFromTxt(string txtfile)
        {
            using var streamReader = new StreamReader(txtfile, Encoding.UTF8);
            var termId = 1;
            var lineCount = 1;
            var terms = new List<Term>();
            while (!streamReader.EndOfStream)
            {
                var type = streamReader.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(type))
                    continue;
                lineCount++;
                if (type != "character" && type != "topic")
                    throw new Exception($"Error: Invalid term type \"{type}\" on line {lineCount}");

                terms.Add(new Term
                {
                    Type = type,
                    TermName = streamReader.ReadLine(),
                    Desc = streamReader.ReadLine(),
                    MatchCase = type == "character",
                    DescSrc = "shelfari",
                    Id = termId++
                });

                lineCount += 2;
            }

            return terms;
        }

        #region Unsupported
        public Task<IEnumerable<BookInfo>> SearchBookAsync(string author, string title, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeriesInfo> GetSeriesInfoAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> GetPageCountAsync(BookInfo curBook, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task GetExtrasAsync(BookInfo curBook, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IEnumerable<NotableClip>> GetNotableClipsAsync(string url, HtmlDocument srcDoc = null, IProgressBar progress = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IEnumerable<BookInfo>> SearchBookByAsinAsync(string asin, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}