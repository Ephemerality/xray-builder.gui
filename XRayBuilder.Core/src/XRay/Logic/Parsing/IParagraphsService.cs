using System.Collections.Generic;
using Ephemerality.Unpack;
using JetBrains.Annotations;

namespace XRayBuilder.Core.XRay.Logic.Parsing
{
    public interface IParagraphsService
    {
        [NotNull]
        IEnumerable<Paragraph> GetParagraphs([NotNull] IMetadata metadata);
    }
}