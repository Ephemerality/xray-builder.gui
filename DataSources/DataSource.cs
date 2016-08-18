using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources
{
    public abstract class DataSource
    {
        public abstract string Name { get; }
        public virtual HtmlDocument sourceHtmlDoc { get; set; }
        public abstract string SearchBook(string author, string title, Action<string> Log);
        public abstract BookInfo GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, string TLD, Action<string> Log);
        public virtual bool GetPageCount(BookInfo curBook, Action<string> Log) { return false; }
        public virtual List<string[]> GetNotableQuotes(string dataUrl) { return new List<string[]>(); }
        public virtual List<XRay.Term> GetTerms(bool useSpoilers, string dataUrl, Action<string> Log) { return new List<XRay.Term>(); }
    }
}
