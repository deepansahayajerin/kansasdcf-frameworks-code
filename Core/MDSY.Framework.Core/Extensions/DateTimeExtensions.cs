using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSY.Framework.Core
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns integer representing Day of week based on DB2 Day OF Week ISO setting. Monday start with 1
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static int DayOfWeekISOAsInt(this DateTime instance)
        {
            int dayOfWeek = 0;
            dayOfWeek = (int)instance.DayOfWeek;
            if (dayOfWeek == 0)
                dayOfWeek = 7;
            return dayOfWeek;
        }

        /// <summary>
        /// Retunrs an integer representing DB2 WEEK_ISO function
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static int WeekOfYearISO(this DateTime instance)
        {
            int weekOfYear = 0;
            weekOfYear = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(instance, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            return weekOfYear;
        }

        /// <summary>
        /// returns DateTime of last day of the month from specified DateTime
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(this DateTime instance)
        {
            DateTime lastDay;
            lastDay = instance.AddDays(1 - (instance.Day)).AddMonths(1).AddDays(-1);

            return lastDay;
        }
    }
}
