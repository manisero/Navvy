using System;

namespace Manisero.Navvy.Reporting.Utils
{
    internal static class DateTimeUtils
    {
        public static bool IsBetween(
            this DateTime value,
            DateTime from,
            DateTime to)
            => value >= from && value <= to;
    }
}
