using System;

namespace FlowKunevDev.Common
{
    public static class TimeHelper
    {
        private static readonly string[] SofiaTimeZoneIds = { "FLE Standard Time", "Europe/Sofia" };

        public static DateTime UtcNow => DateTime.UtcNow;

        public static DateTime LocalNow => ConvertUtcToLocal(DateTime.UtcNow);

        public static DateTime ConvertUtcToLocal(DateTime utcDate)
        {
            foreach (var id in SofiaTimeZoneIds)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDate, DateTimeKind.Utc), tz);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return utcDate;
        }
    }
}