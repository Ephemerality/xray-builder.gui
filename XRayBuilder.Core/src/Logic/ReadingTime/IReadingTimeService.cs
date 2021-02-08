using System;

namespace XRayBuilder.Core.Logic.ReadingTime
{
    public interface IReadingTimeService
    {
        /// <summary>
        /// Converts an integer into reading time.
        /// </summary>
        /// <returns>Estimated typical time to read as a <see cref="TimeSpan"/>.</returns>
        /// <param name="pageCount">Page count as an integer.</param>
        TimeSpan GetReadingTime(int pageCount);

        /// <summary>
        /// Converts an integer into a formatted string.
        /// </summary>
        /// <returns>Typical time to read including the number of pages or null if no pages are specified.</returns>
        /// <param name="pageCount">Page count as an integer.</param>
        string GetFormattedReadingTime(int pageCount);
    }
}