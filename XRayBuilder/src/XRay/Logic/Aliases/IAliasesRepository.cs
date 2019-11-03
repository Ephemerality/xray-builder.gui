using System.Collections.Generic;
using JetBrains.Annotations;

namespace XRayBuilderGUI.XRay.Logic.Aliases
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
    }
}