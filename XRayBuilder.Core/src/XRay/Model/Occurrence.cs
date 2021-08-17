namespace XRayBuilder.Core.XRay.Model
{
    public readonly struct Occurrence
    {
        public IndexLength Excerpt { get; init; }
        public IndexLength Highlight { get; init; }
    }

    public readonly struct IndexLength
    {
        public IndexLength(int index, int length)
        {
            Index = index;
            Length = length;
        }

        public int Index { get; }
        public int Length { get; }
    }
}