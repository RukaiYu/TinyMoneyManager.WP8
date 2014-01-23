namespace TinyMoneyManager.Component.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TinyMoneyManager.Component;

    public class RepaymentStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                value = RepaymentStatus.OnGoing;
            }
            RepaymentStatus status = (RepaymentStatus) value;
            return GetColorFromStatus(status);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDecimal(value);
        }

        public static string GetColorFromStatus(RepaymentStatus status)
        {
            string str = "red";
            switch (status)
            {
                case RepaymentStatus.Completed:
                    return "green";

                case RepaymentStatus.Cancel:
                    return "gray";

                case RepaymentStatus.Deleted:
                    return "gray";

                case RepaymentStatus.OnGoing:
                    return str;
            }
            return str;
        }
    }
}

