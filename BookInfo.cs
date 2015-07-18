using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XRayBuilderGUI
{
    public class BookInfo
    {
        public string title;
        public string author;
        public string asin;
        public string guid;
        public string databasename;
        public string path;
        public string sidecarName;

        public BookInfo(string title, string author, string asin, string guid, string databasename, string path, string sidecarName)
        {
            this.title = title;
            this.author = author;
            this.asin = asin;
            this.guid = guid;
            this.databasename = databasename;
            this.path = path;
            this.sidecarName = sidecarName;
        }
    }
}
