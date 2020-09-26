namespace XRayBuilder.Core.XRay.Model
{
    public sealed class Occurrence
    {
        public Occurrence(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        public int Offset { get; }
        public int Length { get; }
    }
}