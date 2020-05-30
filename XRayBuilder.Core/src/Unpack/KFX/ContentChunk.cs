namespace XRayBuilder.Core.Unpack.KFX
{
    public sealed class ContentChunk
    {
        public string Name { get; set; }
        public bool MatchZeroLen { get; set; }
        public string ContentName { get; set; }
        public string ContentText { get; set; }
        /// <summary>
        /// Position ID, analogous to the true offset (eg for xray locations)
        /// </summary>
        public int Pid { get; set; }
        public int Eid { get; set; }
        public int EidOffset { get; set; }
        public int Length { get; set; }
    }
}