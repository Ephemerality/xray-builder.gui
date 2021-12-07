using System.Diagnostics;

namespace Ephemerality.Unpack.KFX
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
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

        private string DebuggerDisplay => $"pid={Pid}  eid={Eid}{(EidOffset > 0 ? $"+{EidOffset}" : "")}  len={Length}  sect={Name}  content-name={ContentName}  content-text={ContentText}";
    }
}