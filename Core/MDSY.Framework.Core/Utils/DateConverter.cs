using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Core
{
    public class DateConverter
    {
        /// <summary>
        /// Returns a date in the required format
        /// </summary>
        private static DateTime _date = DateTime.Now;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DateConverter class.
        /// </summary>
        public DateConverter()
        {
        }
        static DateConverter()
        {
        }

        /// <summary>
        /// Returns and sets date
        /// </summary>
        public static DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
        #endregion

        #region Static Properties
        /// <summary>
        /// Returns date in mmddyyyy format
        /// </summary>
        public static string LegacyCalDate
        {
            get
            {
                return String.Format("{0}{1}{2}", _date.Month.ToString().PadLeft(2, '0'), _date.Day.ToString().PadLeft(2, '0'), _date.Year);
            }
            set
            {
                _date = new DateTime(Convert.ToInt32(value.Substring(4)), Convert.ToInt32(value.Substring(0, 2)), Convert.ToInt32(value.Substring(2, 2)));
            }
        }

        /// <summary>
        /// Returns date in yyyymmdd format
        /// </summary>
        public static string LegacyGrgDate
        {
            get
            {
                return String.Format("{0}{1}{2}", _date.Year, _date.Month.ToString().PadLeft(2, '0'), _date.Day.ToString().PadLeft(2, '0'));
            }
            set
            {
                _date = new DateTime(Convert.ToInt32(value.Substring(0, 4)), Convert.ToInt32(value.Substring(4, 2)), Convert.ToInt32(value.Substring(6, 2)));
            }
        }

        /// <summary>
        /// Returns date in yyyyMMddHHmmssffffff format
        /// </summary>
        public static string FunctionCurrentDate
        {
            get
            {
                string systemDate = ConfigSettings.GetAppSettingsString("ApplicationDate"); //CCYY-MM-DD                                                    
                string returnDate = _date.ToString("yyyyMMddHHmmssffffff");
                if (!String.IsNullOrEmpty(systemDate))
                {
                    systemDate = systemDate.Replace("-", "");
                    string currTime = DateTime.Now.ToString("HHmmss");
                    returnDate = string.Concat(systemDate, currTime, returnDate.Substring(14));
                }
                return returnDate;
            }

        }

        /// <summary>
        /// Return date in ddmmyyyy format
        /// </summary>
        static string LegacyEurDate
        {
            get
            {
                return String.Format("{0}{1}{2}", _date.Day.ToString().PadLeft(2, '0'), _date.Month.ToString().PadLeft(2, '0'), _date.Year);
            }
            set
            {
                _date = new DateTime(Convert.ToInt32(value.Substring(4)), Convert.ToInt32(value.Substring(2, 2)), Convert.ToInt32(value.Substring(0, 2)));
            }
        }

        /// <summary>
        /// Returns date in yyyyddd format
        /// </summary>
        static string LegacyJulDate
        {
            get
            {
                return String.Format("{0}{1}", _date.Year, _date.DayOfYear.ToString().PadLeft(3, '0'));
            }
            // Add Julian date conversion
            set
            {
                int jDay = 0;
                int temp = 0;
                int jTotal = 0;
                int jMonth = 0;
                int jYear = Convert.ToInt32(value.Substring(1, 4));
                int jNoDays = Convert.ToInt32(value.Substring(5, 3));
                bool stopCheck = false;
                for (int i = 1; i <= 12; i++)
                {
                    if (!(stopCheck))
                    {
                        int noDays = DateTime.DaysInMonth(jYear, i);
                        jTotal += noDays;
                        if (jTotal <= jNoDays)
                        {
                            jMonth = i;
                            jDay = noDays;
                        }
                        else
                        {
                            jTotal = jTotal - noDays;
                            temp = jNoDays - jTotal;

                            if (!(temp == 0))
                            {
                                jMonth = i;
                                jDay = temp;
                            }
                            stopCheck = true;
                        }
                    }
                }
                _date = new DateTime(jYear, jMonth, jDay);
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Converts Legacy Date strings to .Net DateTime type
        /// </summary>
        /// <param name="inputDate">The date to be converted</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <returns></returns>
        public static DateTime ConvertToDate(string inputDate, LegacyDate dateType)
        {
            if (dateType == LegacyDate.Julian)
                LegacyJulDate = inputDate;
            else if (dateType == LegacyDate.Gregorian)
                LegacyGrgDate = inputDate;
            else if (dateType == LegacyDate.European)
                LegacyEurDate = inputDate;
            else
                LegacyCalDate = inputDate;
            return Date;
        }

        /// <summary>
        /// Coverts DateTime Type to Legacy Date String
        /// </summary>
        /// <param name="inputDate">The date to be converted</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <returns></returns>
        public static string ConvertToLegacyDate(DateTime inputDate, LegacyDate dateType)
        {
            _date = inputDate;
            if (dateType == LegacyDate.Julian)
                return LegacyJulDate;
            else if (dateType == LegacyDate.Gregorian)
                return LegacyGrgDate;
            else if (dateType == LegacyDate.European)
                return LegacyEurDate;
            else
                return LegacyCalDate;
        }
        /// <summary>
        /// Coverts DateTime Type to Legacy Date String after checking for system override date
        /// </summary>
        /// <param name="inputDate">The date to be converted</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <param name="checkOverride">if true use the environment date Override</param>
        /// <returns>Legacy date string</returns>
        public static string ConvertToLegacyDate(DateTime inputDate, LegacyDate dateType, bool checkOverride)
        {
            _date = inputDate;

            try
            {
                if (checkOverride)
                {
                    string newDate = Environment.GetEnvironmentVariable("DateOverride");
                    if (!string.IsNullOrEmpty(newDate))
                    {
                        _date = Convert.ToDateTime(newDate);
                    }
                }
            }
            catch
            {
                Console.WriteLine("** Invalid Date Override ");
                _date = inputDate;
            }

            if (dateType == LegacyDate.Julian)
                return LegacyJulDate;
            else if (dateType == LegacyDate.Gregorian)
                return LegacyGrgDate;
            else if (dateType == LegacyDate.European)
                return LegacyEurDate;
            else
                return LegacyCalDate;
        }
        /// <summary>
        /// Checks for Valid Legacy Date
        /// </summary>
        /// <param name="inputDate">The date to be validated</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <returns>true if it's a valid date, otherwise it returns false</returns>
        public static bool IsValidLegacyDate(string inputDate, LegacyDate dateType)
        {
            DateTime dummyResult;
            bool rtn = false;
            if (inputDate.Length > 8)
            {
                inputDate = inputDate.Substring(inputDate.Length - 8);
            }

            if (dateType == LegacyDate.Gregorian)
            {
                rtn = DateTime.TryParseExact(inputDate, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dummyResult);
            }
            else if (dateType == LegacyDate.Calendar)
            {
                rtn = DateTime.TryParseExact(inputDate, "MMddyyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dummyResult);
            }
            else if (dateType == LegacyDate.European)
            {
                rtn = DateTime.TryParseExact(inputDate, "ddMMyyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dummyResult);
            }
            else if (dateType == LegacyDate.Julian)
            {
                int tempInt;
                if (int.TryParse(inputDate.Substring(0, 4), out tempInt))
                {
                    if (tempInt > 0 && tempInt < 367)
                    {
                        if (DateTime.TryParseExact(inputDate.Substring(0, 4) + "0101", "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dummyResult))
                        {
                            rtn = true;
                        }
                    }
                }
            }

            return (rtn);
        }
        /// <summary>
        /// Add days to Legacy date
        /// </summary>
        /// <param name="inputDate">Date to which days are added</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <param name="days">The number of days to be added</param>
        /// <returns>date with added days</returns>
        public static string AddDaysToLegacyDate(string inputDate, LegacyDate dateType, int days)
        {
            Date = ConvertToDate(inputDate, dateType);
            return ConvertToLegacyDate(Date.AddDays(days), dateType);
        }

        public static string AddDaysToLegacyDate(IBufferValue inputDate, LegacyDate dateType, int days)
        {
            Date = ConvertToDate(inputDate.DisplayValue, dateType);
            return ConvertToLegacyDate(Date.AddDays(days), dateType);
        }
        /// <summary>
        /// Add days to Legacy date
        /// </summary>
        /// <param name="inputDate">Date to which days are to added</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <param name="days">The number of days to be added</param>
        /// <returns>Date with added days</returns>
        public static string AddDaysToLegacyDate(string inputDate, LegacyDate dateType, string days)
        {
            Date = ConvertToDate(inputDate, dateType);
            return ConvertToLegacyDate(Date.AddDays(Convert.ToInt32(days)), dateType);
        }
        /// <summary>
        /// Returns the number of days difference between two Legacy Dates 
        /// </summary>
        /// <param name="firstDate">Start date</param>
        /// <param name="secondDate">End date</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <returns>number of days difference between start and end dates</returns>
        public static int SubtractLegacyDates(string firstDate, string secondDate, LegacyDate dateType)
        {
            TimeSpan compareDays = ConvertToDate(firstDate, dateType).Subtract(ConvertToDate(secondDate, dateType));
            return compareDays.Days;
        }

        /// <summary>
        /// Returns the day of the week
        /// </summary>
        /// <param name="inputDate">Date to be used</param>
        /// <param name="dateType">The type of date such as Julian, Gregorian, etc.</param>
        /// <returns>The day of the week</returns>
        public static string GetLegacyDateDayOfWeek(string inputDate, LegacyDate dateType)
        {
            Date = ConvertToDate(inputDate, dateType);
            return Date.DayOfWeek.ToString();
        }

        /// <summary>
        /// Returns absolute time in different formats
        /// </summary>
        /// <param name="inputDate">Date to be converted</param>
        /// <param name="format">Specified date format</param>
        /// <param name="outputDate">The date in the format specified</param>
        public static void ConvertABSTime(IField inputDate, DateTimeFormat format, IField outputDate)
        {
            long nbrTicks = long.Parse(inputDate.DisplayValue) * 10000;

            DateTime centuryBegin = new DateTime(1900, 1, 1);
            DateTime currentDate = centuryBegin.AddTicks(nbrTicks);

            if (format == DateTimeFormat.MMDDYY)
            {
                outputDate.Assign(currentDate.ToString("MMddyy"));
            }
            else if (format == DateTimeFormat.YYMMDD)
            {
                outputDate.Assign(currentDate.ToString("yyMMdd"));
            }
            else if (format == DateTimeFormat.YYYYMMDD)
            {
                outputDate.Assign(currentDate.ToString("yyyyMMdd"));
            }
            else if (format == DateTimeFormat.MMDDYYYY)
            {
                outputDate.Assign(currentDate.ToString("MMddyyyy"));
            }

        }

        /// <summary>
        /// Returns absolute DateTime in different formats
        /// </summary>
        /// <param name="inputDate">Date to be converted</param>
        /// <param name="format">Specified date format</param>
        /// <param name="outputDate">The date in the required format</param>
        /// <param name="dateSep">Specified date separator</param>
        /// <param name="outputTime">The time in the required format</param>
        /// <param name="timeSep">Specified Tme Separator</param>
        public static void ConvertABSTime(IBufferValue inputDate, DateTimeFormat format, IBufferValue outputDate, string dateSep, IBufferValue outputTime, string timeSep)
        {
            //ConvertABSTime(inputDate, format, outputDate);
            long nbrTicks = long.Parse(inputDate.DisplayValue) * 10000;

            DateTime centuryBegin = new DateTime(1900, 1, 1);
            DateTime currentDate = centuryBegin.AddTicks(nbrTicks);

            if (format == DateTimeFormat.MMDDYY)
            {
                outputDate.Assign(currentDate.ToString("MM" + dateSep + "dd" + dateSep + "yy"));
            }
            else if (format == DateTimeFormat.YYMMDD)
            {
                outputDate.Assign(currentDate.ToString("yy" + dateSep + "MM" + dateSep + "dd"));
            }
            else if (format == DateTimeFormat.YYYYMMDD)
            {
                outputDate.Assign(currentDate.ToString("yyyy" + dateSep + "MM" + dateSep + "dd"));
            }
            else if (format == DateTimeFormat.MMDDYYYY)
            {
                outputDate.Assign(currentDate.ToString("MM" + dateSep + "dd" + dateSep + "yyyy"));
            }

            if (outputTime != null)
            {
                if (outputTime is IField && ((IField)outputTime).FieldType == Buffer.Common.FieldType.NumericEdited)
                {
                    outputTime.Assign(currentDate.ToString("HH" + "mm" + "ss"));
                }
                else
                {
                    outputTime.Assign(currentDate.ToString("HH" + timeSep + "mm" + timeSep + "ss"));
                }
            }
        }

        /// <summary>
        /// Returns absolute DateTime in different formats
        /// </summary>
        /// <param name="outputTime">The time in the required format</param>
        /// <param name="timeSep">Specified Tme Separator</param>
        public static void ConvertABSTime(IBufferValue inputDate, IBufferValue outputTime, string timeSep)
        {
            long nbrTicks = long.Parse(inputDate.DisplayValue) * 10000;

            DateTime centuryBegin = new DateTime(1900, 1, 1);
            DateTime currentDate = centuryBegin.AddTicks(nbrTicks);

            outputTime.Assign(currentDate.ToString("HH" + timeSep + "mm" + timeSep + "ss"));
        }

        /// <summary>
        /// This function converts a date in the Gregorian calendar form standard date form (YYYYMMDD) to integer date form 
        /// </summary>
        /// <param name="inputDate">Date in integer form</param>
        /// <returns>An integer that is the number of days the date represented succeeds December 31, 1600 in the Gregorian Calendar</returns>
        public static int IntegerOfDate(int inputDate)
        {
            string strDate = string.Concat(inputDate.ToString().Substring(0, 4), "-", inputDate.ToString().Substring(4, 2), "-", inputDate.ToString().Substring(6, 2));

            DateTime gregDate = DateTime.Parse("1600-12-31");
            DateTime newDate = DateTime.Parse(strDate);

            TimeSpan difference = newDate.Subtract(gregDate);
            return (int)difference.TotalDays;
        }

        /// <summary>
        /// Create a Julian format date from an IntegerOfDate number
        /// </summary>
        /// <param name="dateInteger"></param>
        /// <returns></returns>
        public static int DayOfInteger(int dateInteger)
        {
            DateTime gregDate = DateTime.Parse("16001231");
            DateTime newDate = gregDate.AddDays(dateInteger);

            return Convert.ToInt32(ConvertToLegacyDate(newDate, LegacyDate.Julian));
        }
        /// <summary>
        /// Returns the number of week days between 2 dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static int GetWeekDays(DateTime startDate, DateTime endDate)
        {
            ////checks if date is correct
            //if (startDate > endDate)
            //    throw new ArgumentException("Incorrect last day" + endDate);


            //TimeSpan span = endDate - startDate;
            //int businessDays = span.Days + 1;
            //int fullWeek = businessDays / 7;

            ////checking for weekends
            //if (businessDays > fullWeek * 7)
            //{
            //    int firstWeekDay = startDate.DayOfWeek;
            //    int lastWeekDay = endDate.DayOfWeek;

            //    if (lastWeekDay < firstWeekDay)
            //        lastWeekDay += 7;
            //    if (firstWeekDay <= 6)
            //    {
            //        if (lastWeekDay >= 7)
            //            businessDays -= 2;
            //        else if (lastWeekDay >= 6)
            //            businessDays -= 1;
            //    }
            //    else if (firstWeekDay <= 7 && lastWeekDay >= 7)
            //        businessDays -= 1;
            //}

            ////subtract weekends
            //businessDays -= fullWeek + fullWeek;

            //return businessDays;

            return 0;

        }

        #endregion
    }

    /// <summary>
    /// Date formats
    /// </summary>
    public enum DateTimeFormat
    {
        YYMMDD,
        YYYYMMDD,
        MMDDYY,
        MMDDYYYY
    }

}
