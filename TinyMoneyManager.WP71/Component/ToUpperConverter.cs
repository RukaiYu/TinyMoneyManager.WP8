namespace TinyMoneyManager.Component
{
    using GlobalizationSample.Converter;
    using System;
    using System.Globalization;

    public class ToUpperConverter : FormatConverter
    {
        public override object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return base.Convert(value, targetType, parameter, culture).ToString().ToUpper();
        }
    }
}

