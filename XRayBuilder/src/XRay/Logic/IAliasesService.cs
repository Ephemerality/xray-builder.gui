using System.Collections.Generic;
using JetBrains.Annotations;

namespace XRayBuilderGUI.XRay.Logic
{
    public interface IAliasesService
    {
        /// <summary>
        /// Loads a set of aliases for terms from <paramref name="aliasFile"/>.
        /// Returns null if the file does not exist.
        /// </summary>
        [CanBeNull]
        Dictionary<string, string[]> LoadAliases(string aliasFile);
    }
}