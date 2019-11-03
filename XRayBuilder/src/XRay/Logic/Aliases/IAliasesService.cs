using System.Collections.Generic;
using XRayBuilderGUI.XRay.Artifacts;

namespace XRayBuilderGUI.XRay.Logic.Aliases
{
    public interface IAliasesService
    {
        /// <summary>
        /// Write any characters and their aliases from <paramref name="terms"/> to <paramref name="aliasFile"/>
        /// </summary>
        void SaveCharacters(IEnumerable<Term> terms, string aliasFile);

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