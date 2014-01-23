namespace TinyMoneyManager.Component
{
    using System;

    public class ConstString
    {
        private static string formatFullWithoutYear;
        public const string formatFullWithoutYear_Chinese = "M月d日 ddd";
        private static string formatFullWithYear;
        public const string formatFullWithYear_Chinese = "yyyy年M月d日 ddd";
        public const string formatWithoutYear = "M/d ddd";
        public const string formatWithoutYearNoDDD = "M/d";
        private static string formatWithShortDateAndWeekWithYear;
        public const string formatWithYear = "yyyy/M/d ddd";
        public const string formatWithYearNoDDD = "yyyy/M/d";

        public static string FormatFullWithoutYear
        {
            get
            {
                if (formatFullWithoutYear == null)
                {
                    formatFullWithoutYear = LocalizedObjectHelper.GetLocalizedStringFrom("FormatFullWithoutYear");
                }
                return formatFullWithoutYear;
            }
        }

        public static string FormatFullWithYear
        {
            get
            {
                if (formatFullWithYear == null)
                {
                    formatFullWithYear = LocalizedObjectHelper.GetLocalizedStringFrom("FormatFullWithYear");
                }
                return formatFullWithYear;
            }
        }

        public static string FormatWithShortDateAndWeekWithoutYear
        {
            get
            {
                return "M/d ddd";
            }
        }

        public static string FormatWithShortDateAndWeekWithYear
        {
            get
            {
                if (string.IsNullOrEmpty(formatWithShortDateAndWeekWithYear))
                {
                    formatWithShortDateAndWeekWithYear = LocalizedObjectHelper.GetLocalizedStringFrom("FormatFullWithYear");
                }
                return formatWithShortDateAndWeekWithYear;
            }
        }
    }
}

