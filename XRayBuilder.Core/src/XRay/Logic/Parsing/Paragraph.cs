using JetBrains.Annotations;

namespace XRayBuilder.Core.XRay.Logic.Parsing
{
    public sealed record Paragraph
    {
        /// <summary>
        /// If available, the full HTML of the paragraph
        /// </summary>
        [CanBeNull]
        public string ContentHtml { get; init; }

        /// <summary>
        /// Plaintext version of the paragraph
        /// </summary>
        public string ContentText { get; init; }

        /// <summary>
        /// Location/offset of the paragraph within the book
        /// </summary>
        public int Location { get; init; }

        /// <summary>
        /// Length of the paragraph
        /// </summary>
        public int Length => ContentHtml?.Length ?? ContentText.Length;
    }
}