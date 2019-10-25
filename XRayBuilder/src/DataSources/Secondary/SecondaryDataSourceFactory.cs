using System;
using System.Collections.Generic;
using XRayBuilderGUI.Libraries;

namespace XRayBuilderGUI.DataSources.Secondary
{
    public class SecondaryDataSourceFactory : Factory<Enum, ISecondarySource>
    {
        public SecondaryDataSourceFactory(Shelfari shelfari, Goodreads goodreads)
        {
            _dictionary = new Dictionary<System.Enum, ISecondarySource>
            {
                {Enum.Shelfari, shelfari},
                {Enum.Goodreads, goodreads}
            };
        }

        public enum Enum
        {
            Shelfari,
            Goodreads
        }

        protected override Dictionary<System.Enum, ISecondarySource> _dictionary { get; }
    }
}
