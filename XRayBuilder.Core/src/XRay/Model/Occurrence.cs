namespace XRayBuilder.Core.XRay.Model
{
    public sealed record Occurrence
    {
        public IndexLength Excerpt { get; init; }
        public IndexLength Highlight { get; init; }
    }

    public sealed record IndexLength(int Index, int Length);
}