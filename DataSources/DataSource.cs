using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XRayBuilderGUI.DataSources
{
    abstract class DataSource
    {
        public abstract string Name { get; }
        public abstract string SearchBook(string author, string title);
    }
}
