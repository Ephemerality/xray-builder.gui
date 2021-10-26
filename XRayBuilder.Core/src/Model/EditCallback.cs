using JetBrains.Annotations;

namespace XRayBuilder.Core.Model
{
    /// <summary>
    /// Given the <paramref name="path"/> to a file, allows the file to be opened/edited/closed if desired.
    /// Returns true if the file was opened/edited, false if it wasn't.
    /// </summary>
    public delegate bool EditCallback([NotNull] string path);
}