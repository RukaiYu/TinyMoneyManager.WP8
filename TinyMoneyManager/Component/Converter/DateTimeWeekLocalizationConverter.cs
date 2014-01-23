namespace TinyMoneyManager.Component.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TinyMoneyManager;

    public class DateTimeWeekLocalizationConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.DateTime time = (System.DateTime) value;
            return time.ToString("ddd", LocalizedStrings.CultureName);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

