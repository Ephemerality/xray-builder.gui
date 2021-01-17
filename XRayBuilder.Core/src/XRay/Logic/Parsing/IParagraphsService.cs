using System.Collections.Generic;
using JetBrains.Annotations;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.XRay.Logic.Parsing
{
    public interface IParagraphsService
    {
        [NotNull]
        IEnumerable<Paragraph> GetParagraphs([NotNull] IMetadata metadata);
    }
}