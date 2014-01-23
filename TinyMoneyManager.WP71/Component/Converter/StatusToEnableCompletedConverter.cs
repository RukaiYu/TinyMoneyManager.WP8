namespace TinyMoneyManager.Component.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TinyMoneyManager.Component;

    public class StatusToEnableCompletedConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                value = RepaymentStatus.OnGoing;
            }
            RepaymentStatus status = (RepaymentStatus) value;
            return (status != RepaymentStatus.Completed);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}

