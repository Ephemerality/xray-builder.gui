namespace XRayBuilderGUI
{
    public class SeriesInfo
    {
        public string Name { get; set; }

        /// <summary>
        /// Position in the series
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Total books in the series
        /// </summary>
        public int Total { get; set; }

        public BookInfo Next { get; set; }

        public BookInfo Previous { get; set; }

        public string Url { get; set; }
    }
}
