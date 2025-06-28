using System;

namespace FlowKunevDev.Common
{
    public static class TimeHelper
    {
        private static readonly string[] SofiaTimeZoneIds = { "FLE Standard Time", "Europe/Sofia" };

        public static DateTime UtcNow => RemoveSeconds(DateTime.UtcNow);

        public static DateTime LocalNow => RemoveSeconds(ConvertUtcToLocal(DateTime.UtcNow));

        public static DateTime ConvertUtcToLocal(DateTime utcDate)
        {
            foreach (var id in SofiaTimeZoneIds)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                    return RemoveSeconds(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDate, DateTimeKind.Utc), tz));
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return RemoveSeconds(utcDate);
        }

        private static DateTime RemoveSeconds(DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                0,
                dateTime.Kind);
        }
    }
}