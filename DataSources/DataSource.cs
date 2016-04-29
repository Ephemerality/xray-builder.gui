using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace XRayBuilderGUI.DataSources
{
    abstract class DataSource
    {
        public abstract string Name { get; }
        public virtual HtmlDocument searchHtmlDoc { get; set; }
        public abstract string SearchBook(string author, string title);
        public abstract BookInfo GetNextInSeries(BookInfo curBook, AuthorProfile authorProfile, Action<string> Log);
        public virtual bool GetPageCount(BookInfo curBook, Action<string> Log) { return false; }
    }
}
