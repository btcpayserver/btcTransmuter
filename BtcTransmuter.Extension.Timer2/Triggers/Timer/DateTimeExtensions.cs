using System;

namespace BtcTransmuter.Extension.Timer.Triggers.Timer
{
    public static class DateTimeExtensions
    {
        public static int MonthDifference(this DateTime lValue, DateTime rValue)
        {
            var yearDifferenceInMonths = (lValue.Year - rValue.Year) * 12;
            var monthDifference = lValue.Month - rValue.Month;

            return yearDifferenceInMonths + monthDifference + 
                   (lValue.Day > rValue.Day
                       ? 1 : 0); // If end day is greater than start day, add 1 to round up the partial month
        }
        
        public static int YearDifference(this DateTime lValue, DateTime rValue)
        {
            return lValue.Year - rValue.Year +
                   (lValue.Month > rValue.Month // Partial month, same year
                       ? 1
                       : lValue.Day > rValue.Day // Partial month, same year and month
                           ? 1 : 0);
        }
    }
}