using System.Collections.Generic;
using JetBrains.Annotations;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.XRay.Logic.Aliases
{
    public interface IAliasesRepository
    {
        /// <summary>
        /// Load any available aliases for <paramref name="xray"/>, adding and removing terms as specified in the alias file
        /// </summary>
        void LoadAliasesForXRay(XRay xray);

        /// <summary>
        /// Loads a set of aliases for terms from <paramref name="aliasFile"/>.
        /// Returns null if the file does not exist.
        /// </summary>
        [CanBeNull]
        Dictionary<string, string[]> LoadAliasesFromFile(string aliasFile);

        /// <summary>
        /// Write any characters and their aliases from <paramref name="terms"/> to an alias file for <paramref name="asin"/>
        /// </summary>
        [CanBeNull]
        string SaveCharactersToFile(IEnumerable<Term> terms, string asin, bool splitAliases);
    }
}