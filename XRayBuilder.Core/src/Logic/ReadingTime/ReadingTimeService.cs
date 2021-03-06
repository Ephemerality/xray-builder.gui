﻿using System;
using XRayBuilder.Core.Libraries.Language.Pluralization;

namespace XRayBuilder.Core.Logic.ReadingTime
{
    public sealed class ReadingTimeService : IReadingTimeService
    {
        public TimeSpan GetReadingTime(int pageCount)
        {
            return pageCount == 0 ? new TimeSpan() : TimeSpan.FromMinutes(pageCount * 1.098507462686567);
        }

        public string GetFormattedReadingTime(int pageCount)
        {
            if (pageCount == 0)
                return null;

            var readingTime = GetReadingTime(pageCount);

            var days = PluralUtil.Pluralize($"{readingTime.Days:day}");
            var hours = PluralUtil.Pluralize($"{readingTime.Hours:hour}");
            var minutes = PluralUtil.Pluralize($"{readingTime.Minutes:minute}");

            return $"Typical time to read: {(readingTime.Days > 1 ? $"{days}, " : string.Empty)}{(readingTime.Hours > 1 ? $"{hours}" : string.Empty)}{(readingTime.Hours > 1 ? " and " : ", ")}{(readingTime.Minutes > 1 ? $"{minutes}" : string.Empty)} ({pageCount} pages)";
        }
    }
}
