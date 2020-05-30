using System.Collections.Generic;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.XRay.Logic.Aliases
{
    public interface IAliasesService
    {
        /// <summary>
        /// Takes a set of characters and attempts to split their names into plausible aliases.
        /// Ignores common titles like "Sir", "Mr.", etc.
        /// Strips and uses keywords like "aka" to determine aliases.
        /// Duplicate aliases will be ignored, only the first will remain.
        /// Characters with no aliases will still be present.
        /// </summary>
        IEnumerable<KeyValuePair<string, string[]>> GenerateAliases(IEnumerable<Term> characters);
    }
}