using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources
{
    public abstract class DataSource
    {
        public abstract string Name { get; }
        public virtual HtmlDocument sourceHtmlDoc { get; set; }
        public abstract Task<List<BookInfo>> SearchBook(string author, string title);
        public abstract Task<BookInfo> GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, string TLD);
        public virtual Task<bool> GetPageCount(BookInfo curBook) { return Task.FromResult(false); }
        public virtual Task GetExtras(BookInfo curBook, CancellationToken token, IProgressBar progress = null) { return Task.FromResult(false); }
        public virtual Task<List<XRay.Term>> GetTerms(string dataUrl, IProgressBar progress, CancellationToken token) { return Task.FromResult(new List<XRay.Term>()); }
        public virtual Task<List<NotableClip>> GetNotableClips(string url, CancellationToken token, HtmlDocument srcDoc = null, IProgressBar progress = null) { return Task.FromResult(new List<NotableClip>()); }

        public class FormatChangedException : Exception
        {
            public FormatChangedException(string source, string message, Exception previous = null)
                : base($"Format changed on {source} ({message}).", previous)
            {
            }
        }
    }

    public class NotableClip
    {
        public string Text { get; set; }
        public int Likes { get; set; }
    }
}
