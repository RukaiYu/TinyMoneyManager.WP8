namespace TinyMoneyManager.Component.Converter
{
    using NkjSoft.Extensions;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class MoneyStringConvertToF2 : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDecimal(value).ToMoneyF2();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDecimal(value);
        }
    }
}

