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
        public abstract Task<string> SearchBook(string author, string title);
        public abstract Task<BookInfo> GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, string TLD);
        public virtual async Task<bool> GetPageCount(BookInfo curBook) { return false; }
        public virtual async Task GetExtras(BookInfo curBook, CancellationToken token, IProgress<Tuple<int, int>> progress = null) { }
        public virtual async Task<List<XRay.Term>> GetTerms(string dataUrl, IProgress<Tuple<int, int>> progress, CancellationToken token) { return new List<XRay.Term>(); }
        public virtual async Task<List<Tuple<string, int>>> GetNotableClips(string url, CancellationToken token, HtmlDocument srcDoc = null, IProgress<Tuple<int, int>> progress = null) { return new List<Tuple<string, int>>(); }
    }
}
