using System.Collections.Generic;

namespace XRayBuilderGUI.XRay.Logic
{
    public interface IAliasesService
    {
        /// <summary>
        /// Loads a set of aliases for terms from <paramref name="aliasFile"/>.
        /// Returns null if the file does not exist.
        /// </summary>
        Dictionary<string, string[]> LoadAliases(string aliasFile);
    }
}