using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources
{
    public abstract class DataSource
    {
        public abstract string Name { get; }
        public virtual HtmlDocument sourceHtmlDoc { get; set; }
        public abstract string SearchBook(string author, string title);
        public abstract BookInfo GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, string TLD);
        public virtual bool GetPageCount(BookInfo curBook) { return false; }
        public virtual void GetExtras(BookInfo curBook, CancellationToken token, IProgress<Tuple<int, int>> progress = null) { }
        public virtual List<XRay.Term> GetTerms(string dataUrl, IProgress<Tuple<int, int>> progress, CancellationToken token) { return new List<XRay.Term>(); }
        public virtual List<Tuple<string, int>> GetNotableClips(string url, CancellationToken token, HtmlDocument srcDoc = null, IProgress<Tuple<int, int>> progress = null) { return new List<Tuple<string, int>>(); }
    }
}
