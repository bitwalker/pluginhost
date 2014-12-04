using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;

namespace PluginHost.Extensions.Time
{
    public static class DateTimeExtensions
    {
        private static readonly DateTimeFormatInfo _formatInfo = DateTimeFormatInfo.InvariantInfo;
        private static readonly DateTimeStyles _formatStyle    = DateTimeStyles.None;

        public struct Formats
        {
            public const string ISO_8601 = "yyyy-MM-ddTHH:mm:ssK";
        }

        /// <summary>
        /// The first moment of the Unix epoch. (1/1/1970)
        /// </summary>
        public static DateTime Epoch
        {
            get { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); }
        }

        /// <summary>
        /// Returns the humanized relative difference between
        /// the provided date and right now.
        /// 
        /// Examples: 10 minutes ago, 45 seconds from now, 2 years ago
        /// </summary>
        public static string Humanize(this DateTime date)
        {
            var now      = DateTime.UtcNow;
            var shifted  = date.Kind == DateTimeKind.Utc || date.Kind == DateTimeKind.Unspecified
                            ? date
                            : date.ToUniversalTime();
            var timeSpan = now.Subtract(shifted);

            var interval   = "0";
            var magnitude  = "seconds";
            var modifer    = now.Ticks > shifted.Ticks ? "ago" : "from now";
            var format     = "{0} {1} {2}";

            if (timeSpan.TotalSeconds < 60)
            {
                magnitude = "seconds";
                interval  = timeSpan.TotalSeconds.ToString("#");
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                magnitude = "minutes";
                interval  = timeSpan.TotalMinutes.ToString("#");
            }
            else if (timeSpan.TotalHours < 24)
            {
                magnitude = "hours";
                interval  = timeSpan.TotalHours.ToString("#");
            }
            else if (timeSpan.TotalDays < 365)
            {
                magnitude = "days";
                interval  = timeSpan.TotalDays.ToString("#");
            }
            else
            {
                magnitude = "years";
                interval  = (now.Year - shifted.Year).ToString("#");
            }

            return string.Format(format, interval, magnitude, modifer);
        }

        /// <summary>
        /// Converts a date time to an ISO 8601 formatted string, using the DateTimeKind
        /// to format the timezone appropriately. Kind defaults to Local.
        /// 
        /// Example: 2007-08-13T16:48:01+0300
        /// </summary>
        public static string ToISO8601(this DateTime date, DateTimeKind kind = DateTimeKind.Local)
        {
            var kinded = new DateTime(date.Ticks, kind);
            return kinded.ToString(Formats.ISO_8601);
        }

        /// <summary>
        /// Converts a date time to an ISO 8601 formatted string, shifting the date
        /// to UTC/Zulu.
        /// 
        /// Example: 2007-08-13T16:48:01Z
        /// </summary>
        public static string ToISO8601z(this DateTime date)
        {
            return ToISO8601(date.ToUniversalTime(), DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns the date suffix (e.g., 'st', 'nd', 'rd', 'th') as a string
        /// </summary>
        public static string GetDaySuffix(this DateTime d)
        {
            switch (d.Day)
            {
                case 31: case 21: case 1: return "st";
                case 22: case 2: return "nd";
                case 23: case 3: return "rd";
                default: return "th";
            }
        }

        /// <summary>
        /// Attempt to parse a DateTime from a string, and return either the successfully
        /// parsed value, or use the provided default or DateTime.MinValue if no default was
        /// provided.
        /// </summary>
        /// <param name="date">The date string to parse</param>
        /// <param name="default">The default date to use. If this isn't provided, DateTime.MinValue is used.</param>
        public static DateTime ParseOrDefault(string date, DateTime? @default = null)
        {
            var result  = DateTime.MinValue;
            bool parsed = DateTime.TryParse(date, out result);
            if (parsed)
                return result;
            else
                return @default.HasValue ? @default.Value : DateTime.MinValue;
        }

        /// <summary>
        /// Parses a date string as an ISO8601-formatted DateTime. If the format is incorrect,
        /// or the input string is invalid, DateTime.MinValue will be returned.
        /// </summary>
        /// <param name="date">The date string to parse.</param>
        public static DateTime ParseISO8601(string date, DateTime? @default = null)
        {
            if (string.IsNullOrWhiteSpace(date))
                return @default.HasValue ? @default.Value : DateTime.MinValue;

            DateTime parsed;
            var valid = DateTime.TryParseExact(date, Formats.ISO_8601, _formatInfo, _formatStyle, out parsed);

            if (valid)
                return parsed;

            return @default.HasValue ? @default.Value : DateTime.MinValue;
        }

        public static bool IsFuture(this DateTime @this, bool utc = true)
        {
            return @this.Ticks > (utc ? DateTime.UtcNow : DateTime.Now).Ticks;
        }

        public static bool IsPast(this DateTime @this, bool utc = true)
        {
            // We consider the current tick to be in the past, since the comparison will take at least one tick
            return @this.Ticks <= (utc ? DateTime.UtcNow : DateTime.Now).Ticks;
        }

        /// <summary>
        /// Allows us to use natural language to describe future points in time
        /// Example:
        ///     3.Minutes().From().Now(); // Where .Minutes() returns a TimeSpan
        ///         or
        ///     var oneMinutes = TimeSpan.FromMinutes(1);
        ///     var nextRun    = oneMinute.From().Now();
        /// </summary>
        public static From From(this TimeSpan @this)
        {
            return new From(@this);
        }

        /// <summary>
        /// Determine whether a date falls within the range defined by two dates. You can specify the
        /// granularity of the DateTime comparison using the DateTimeGranularity enum.
        /// 
        /// Examples:
        ///     augustEightAtTwo.WithinRange(augustEightAtFive, augustNine, DateTimeGranularity.Day)  #=> true
        ///     augustEightAtTwo.WithinRange(augustEightAtFive, augustNine, DateTimeGranularity.Hour) #=> false
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <param name="start">The date marking the beginning of the range</param>
        /// <param name="date">The date marking the end of the range</param>
        /// <param name="date">The granularity in time units to use when doing comparisons.</param>
        public static bool WithinRange(this DateTime date, DateTime start, DateTime end, DateTimeGranularity granularity = DateTimeGranularity.Second)
        {
            DateTime rounded = date;
            switch (granularity)
            {
                case DateTimeGranularity.Minute:
                    rounded = date.RoundNearest(1.Minutes());
                    return rounded >= start.RoundNearest(1.Minutes()) && rounded <= end.RoundNearest(1.Minutes());
                case DateTimeGranularity.Hour:
                    rounded = date.RoundNearest(1.Hours());
                    return rounded >= start.RoundNearest(1.Hours()) && rounded <= end.RoundNearest(1.Hours());
                case DateTimeGranularity.Day:
                    rounded = date.RoundNearest(1.Days());
                    return rounded >= start.RoundNearest(1.Days()) && rounded <= end.RoundNearest(1.Days());
                case DateTimeGranularity.Second:
                default:
                    rounded = date.RoundNearest(1.Seconds());
                    return rounded >= start.RoundNearest(1.Seconds()) && rounded <= end.RoundNearest(1.Seconds());
            }
        }

        /// <summary>
        /// Round a date up to the nearest interval specified by the TimeSpan.
        /// Returns a new DateTime object.
        /// 
        /// Usage:
        ///     > var date = new DateTime(2014, 8, 14, 12, 26, 59, 59);
        ///     > date.RoundUp(5.Minutes())
        ///     DateTime(2014, 8, 14, 12, 30, 0, 0)
        ///     > date.RoundUp(1.Minutes())
        ///     DateTime(2014, 8, 14, 12, 27, 0, 0)
        /// </summary>
        /// <param name="date">The date to round.</param>
        /// <param name="time">The timespan defining the interval to round by.</param>
        /// <returns>DateTime</returns>
        public static DateTime RoundUp(this DateTime date, TimeSpan time)
        {
            return new DateTime(((date.Ticks + time.Ticks - 1)/time.Ticks)*time.Ticks);
        }

        /// <summary>
        /// Round a date to the nearest interval specified by the TimeSpan.
        /// Returns a new DateTime object.
        /// 
        /// Usage:
        ///     > var date = new DateTime(2014, 8, 14, 12, 26, 59, 59);
        ///     > date.RoundNearest(5.Minutes())
        ///     DateTime(2014, 8, 14, 12, 25, 0, 0)
        ///     > date.RoundNearest(1.Hours())
        ///     DateTime(2014, 8, 14, 12, 0, 0, 0)
        /// </summary>
        /// <param name="date">The date to round.</param>
        /// <param name="time">The timespan defining the interval to round by.</param>
        /// <returns>DateTime</returns>
        public static DateTime RoundNearest(this DateTime date, TimeSpan time)
        {
            int f = 0;
            double m = (double) (date.Ticks%time.Ticks)/time.Ticks;
            if (m >= 0.5)
                f = 1;
            return new DateTime(((date.Ticks / time.Ticks) + f) * time.Ticks);
        }

        /// <summary>
        /// Get the smallest DateTime value allowed by SQL Server.
        /// 
        /// Useful in comparisons when validating whether a DateTime
        /// is valid for storing in SQL.
        /// </summary>
        /// <returns>DateTime</returns>
        public static DateTime MinSqlValue()
        {
            return DateTime.MinValue.AddYears(1752);
        }

        /// <summary>
        /// Returns a TimeSpan representing `num` milliseconds.
        /// </summary>
        /// <param name="num">The number of milliseconds</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan Milliseconds(this int num)
        {
            return new TimeSpan(0, 0, 0, 0, num);
        }

        /// <summary>
        /// Returns a TimeSpan representing `num` seconds.
        /// </summary>
        /// <param name="num">The number of seconds</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan Seconds(this int num)
        {
            return new TimeSpan(0, 0, num);
        }

        /// <summary>
        /// Returns a TimeSpan representing `num` minutes.
        /// </summary>
        /// <param name="num">The number of minutes</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan Minutes(this int num)
        {
            return new TimeSpan(0, num, 0);
        }

        /// <summary>
        /// Returns a TimeSpan representing `num` hours.
        /// </summary>
        /// <param name="num">The number of hours</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan Hours(this int num)
        {
            return new TimeSpan(num, 0, 0);
        }

        /// <summary>
        /// Returns a TimeSpan representing `num` days.
        /// </summary>
        /// <param name="num">The number of days</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan Days(this int num)
        {
            return new TimeSpan(num, 0, 0, 0);
        }
    }

    /// <summary>
    /// Defines valid units of time which can be used
    /// when fuzzily comparing dates. For example,
    /// when you want to consider two dates equivalent
    /// if they occur on the same day, or within the same
    /// hour.
    /// </summary>
    public enum DateTimeGranularity
    {
        Second,
        Minute,
        Hour,
        Day
    }

    public class From
    {
        private readonly TimeSpan _time;

        public DateTime Now()
        {
            return DateTime.Now.Add(_time);
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow.Add(_time);
        }

        public From(TimeSpan time)
        {
            _time = time;
        }
    }
}
