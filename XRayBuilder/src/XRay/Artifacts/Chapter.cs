namespace XRayBuilderGUI.XRay.Artifacts
{
    public class Chapter
    {
        public string Name { get; set; }
        public long Start { get; set; }
        public long End { get; set; }

        // TODO: Replace w/ serialization
        public override string ToString()
        {
            return string.Format(@"{{""name"":{0},""start"":{1},""end"":{2}}}",
                (Name == "" ? "null" : "\"" + Name + "\""), Start, End);
        }
    }
}