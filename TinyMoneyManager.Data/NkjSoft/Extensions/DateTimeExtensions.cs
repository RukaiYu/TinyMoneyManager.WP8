namespace NkjSoft.Extensions
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public static class DateTimeExtensions
    {
        private static readonly System.TimeSpan _OneDay = new System.TimeSpan(1, 0, 0, 0);
        private static readonly System.TimeSpan _OneHour = new System.TimeSpan(1, 0, 0);
        private static readonly System.TimeSpan _OneMinute = new System.TimeSpan(0, 1, 0);
        private static readonly System.TimeSpan _OneMonth = new System.TimeSpan(0x1f, 0, 0, 0);
        private static readonly System.TimeSpan _OneWeek = new System.TimeSpan(7, 0, 0, 0);
        private static readonly System.TimeSpan _OneYear = new System.TimeSpan(0x16d, 0, 0, 0);
        private static readonly System.TimeSpan _TwoDays = new System.TimeSpan(2, 0, 0, 0);
        private static readonly System.TimeSpan _TwoHours = new System.TimeSpan(2, 0, 0);
        private static readonly System.TimeSpan _TwoMinutes = new System.TimeSpan(0, 2, 0);
        private static readonly System.TimeSpan _TwoMonths = new System.TimeSpan(0x3e, 0, 0, 0);
        private static readonly System.TimeSpan _TwoWeeks = new System.TimeSpan(14, 0, 0, 0);
        private static readonly System.TimeSpan _TwoYears = new System.TimeSpan(730, 0, 0, 0);
        private static readonly string formatter = "yyyy/M/d";
        private static readonly string formatter_Chinese = "yyyy年M月d日";
        private static readonly string formatter_Chinese_NoneSecond = "yyyy年M月d日 H点mm分";
        private static readonly string formatter_Chinese_WithSecond = "yyyy年M月d日 H点mm分ss秒";
        private static readonly string formatter_Chinese2 = "M月d日";
        private static readonly string formatter2 = "M/d";

        public static bool AtSameYearMonth(this System.DateTime date, System.DateTime compareTo)
        {
            return ((date.Date.Year == compareTo.Year) && (date.Month == compareTo.Month));
        }

        public static int CalculateAge(this System.DateTime dateOfBirth)
        {
            return dateOfBirth.CalculateAge(System.DateTime.Today);
        }

        public static int CalculateAge(this System.DateTime dateOfBirth, System.DateTime referenceDate)
        {
            int num = referenceDate.Year - dateOfBirth.Year;
            if ((referenceDate.Month < dateOfBirth.Month) || ((referenceDate.Month == dateOfBirth.Month) && (referenceDate.Day < dateOfBirth.Day)))
            {
                num--;
            }
            return num;
        }

        public static int GetCountDaysOfMonth(this System.DateTime date)
        {
            System.DateTime time = date.AddMonths(1);
            System.DateTime time2 = new System.DateTime(time.Year, time.Month, 1);
            return time2.AddDays(-1.0).Day;
        }

        public static System.DateTime GetDateTimeOfFisrtDayOfWeek(this System.DateTime dateStartToCount)
        {
            System.DayOfWeek firstDayOfWeek = System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
            int num = (dateStartToCount.DayOfWeek == System.DayOfWeek.Sunday) ? ((int) (System.DayOfWeek.Saturday | System.DayOfWeek.Monday)) : ((int) dateStartToCount.DayOfWeek);
            System.DateTime date = dateStartToCount.Date;
            int num2 = -1;
            if (num < (int)firstDayOfWeek)
            {
                num2 = 1;
            }
            while (date.DayOfWeek != firstDayOfWeek)
            {
                date = date.AddDays((double) num2);
            }
            return date;
        }

        public static System.DateTime GetFirstDayOfMonth(this System.DateTime date)
        {
            return new System.DateTime(date.Year, date.Month, 1);
        }

        public static System.DateTime GetFirstDayOfMonth(this System.DateTime date, System.DayOfWeek dayOfWeek)
        {
            System.DateTime firstDayOfMonth = date.GetFirstDayOfMonth();
            while (firstDayOfMonth.DayOfWeek != dayOfWeek)
            {
                firstDayOfMonth = firstDayOfMonth.AddDays(1.0);
            }
            return firstDayOfMonth;
        }

        public static System.DateTime GetLastDayOfMonth(this System.DateTime date)
        {
            return new System.DateTime(date.Year, date.Month, date.GetCountDaysOfMonth());
        }

        public static System.DateTime GetLastDayOfMonth(this System.DateTime date, System.DayOfWeek dayOfWeek)
        {
            System.DateTime lastDayOfMonth = date.GetLastDayOfMonth();
            while (lastDayOfMonth.DayOfWeek != dayOfWeek)
            {
                lastDayOfMonth = lastDayOfMonth.AddDays(-1.0);
            }
            return lastDayOfMonth;
        }

        public static System.TimeSpan GetTimeSpan(this System.DateTime startTime, System.DateTime endTime)
        {
            return endTime.Subtract(startTime);
        }

        public static string ToAgo(this System.DateTime date)
        {
            System.TimeSpan timeSpan = date.GetTimeSpan(System.DateTime.Now);
            if (timeSpan < System.TimeSpan.Zero)
            {
                return "未来";
            }
            if (timeSpan < _OneMinute)
            {
                return "现在";
            }
            if (timeSpan < _TwoMinutes)
            {
                return "1 分钟前";
            }
            if (timeSpan < _OneHour)
            {
                return string.Format("{0} 分钟前", timeSpan.Minutes);
            }
            if (timeSpan < _TwoHours)
            {
                return "1 小时前";
            }
            if (timeSpan < _OneDay)
            {
                return string.Format("{0} 小时前", timeSpan.Hours);
            }
            if (timeSpan < _TwoDays)
            {
                return "昨天";
            }
            if (timeSpan < _OneWeek)
            {
                return string.Format("{0} 天前", timeSpan.Days);
            }
            if (timeSpan < _TwoWeeks)
            {
                return "1 周前";
            }
            if (timeSpan < _OneMonth)
            {
                return string.Format("{0} 周前", timeSpan.Days / 7);
            }
            if (timeSpan < _TwoMonths)
            {
                return "1 月前";
            }
            if (timeSpan < _OneYear)
            {
                return string.Format("{0} 月前", timeSpan.Days / 0x1f);
            }
            if (timeSpan < _TwoYears)
            {
                return "1 年前";
            }
            return string.Format("{0} 年前", timeSpan.Days / 0x16d);
        }

        public static string ToAgo(this System.DateTime date, string outOfMonthStringFormatter)
        {
            System.TimeSpan timeSpan = date.GetTimeSpan(System.DateTime.Now);
            if (timeSpan < System.TimeSpan.Zero)
            {
                return "未来";
            }
            if (timeSpan < _OneMinute)
            {
                return "现在";
            }
            if (timeSpan < _TwoMinutes)
            {
                return "1 分钟前";
            }
            if (timeSpan < _OneHour)
            {
                return string.Format("{0} 分钟前", timeSpan.Minutes);
            }
            if (timeSpan < _TwoHours)
            {
                return "1 小时前";
            }
            if (timeSpan < _OneDay)
            {
                return string.Format("{0} 小时前", timeSpan.Hours);
            }
            if (timeSpan < _TwoDays)
            {
                return "昨天";
            }
            if (timeSpan < _OneWeek)
            {
                return string.Format("{0} 天前", timeSpan.Days);
            }
            if (timeSpan < _TwoWeeks)
            {
                return "1 周前";
            }
            if (timeSpan < _OneMonth)
            {
                return string.Format("{0} 周前", timeSpan.Days / 7);
            }
            if (timeSpan < _TwoMonths)
            {
                return "1 月前";
            }
            return date.ToString(outOfMonthStringFormatter);
        }

        public static string ToChineseDayOfWeek(this System.DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case System.DayOfWeek.Sunday:
                    return "星期日";

                case System.DayOfWeek.Monday:
                    return "星期一";

                case System.DayOfWeek.Tuesday:
                    return "星期二";

                case System.DayOfWeek.Wednesday:
                    return "星期三";

                case System.DayOfWeek.Thursday:
                    return "星期四";

                case System.DayOfWeek.Friday:
                    return "星期五";

                case System.DayOfWeek.Saturday:
                    return "星期六";
            }
            return "";
        }

        public static string ToChineseFullLocal(this System.DateTime date)
        {
            return date.ToString(formatter_Chinese_NoneSecond);
        }

        public static string ToChineseFullLocal(this System.DateTime date, bool includeSecond)
        {
            if (includeSecond)
            {
                return date.ToString(formatter_Chinese_WithSecond);
            }
            return date.ToString(formatter_Chinese_NoneSecond);
        }

        public static string ToShortChineseTimeAmPm(this System.DateTime date)
        {
            int hour = date.Hour;
            string str = string.Empty;
            if ((hour >= 0) && (hour < 1))
            {
                str = "午夜";
            }
            else if ((hour >= 1) && (hour <= 6))
            {
                str = "凌晨";
            }
            else if ((hour > 6) && (hour <= 12))
            {
                str = "早上";
            }
            else if ((hour > 12) && (hour < 13))
            {
                str = "中午";
            }
            else if ((hour > 13) && (hour <= 0x12))
            {
                str = "下午";
            }
            else if ((hour > 0x12) && (hour <= 0x17))
            {
                str = "晚上";
            }
            System.Console.WriteLine(hour);
            int num2 = (hour > 12) ? System.Math.Abs((int) (12 - hour)) : hour;
            return string.Format("{0}{1}点{2}", str, num2, date.ToString("mm分"));
        }

        public static string ToStringInThisYear(this System.DateTime date)
        {
            return date.ToStringInThisYear(false);
        }

        public static string ToStringInThisYear(this System.DateTime date, bool useChinese)
        {
            if (date.Year >= System.DateTime.Now.Year)
            {
                return date.ToString(useChinese ? formatter_Chinese2 : formatter2);
            }
            return date.ToString(formatter);
        }
    }
}

