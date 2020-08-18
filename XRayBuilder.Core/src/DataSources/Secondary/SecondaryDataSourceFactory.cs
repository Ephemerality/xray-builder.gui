using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries;

namespace XRayBuilder.Core.DataSources.Secondary
{
    [UsedImplicitly]
    public sealed class SecondaryDataSourceFactory : Factory<SecondaryDataSourceFactory.Enum, ISecondarySource>
    {
        public SecondaryDataSourceFactory(
            SecondarySourceShelfari shelfari,
            SecondarySourceGoodreads goodreads,
            SecondarySourceFile file,
            SecondarySourceLibraryThing libraryThing)
        {
            Dictionary = new Dictionary<Enum, ISecondarySource>
            {
                {Enum.Shelfari, shelfari},
                {Enum.Goodreads, goodreads},
                {Enum.File, file},
                {Enum.LibraryThing, libraryThing}
            };
        }

        public enum Enum
        {
            Shelfari,
            Goodreads,
            File,
            LibraryThing
        }

        protected override Dictionary<Enum, ISecondarySource> Dictionary { get; }

        [CanBeNull]
        public ISecondarySource GetInferredSource(string urlOrPath)
        {
            var matchingSources = Dictionary.Values.Where(source => source.IsMatchingUrl(urlOrPath)).ToArray();
            return matchingSources.Length == 1 ? matchingSources.First() : null;
        }
    }
}
