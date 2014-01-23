namespace GlobalizationSample.Converter
{
    using System;
    using System.Globalization;

    public class ToLowerConverter : FormatConverter
    {
        public override object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return base.Convert(value, targetType, parameter, culture).ToString().ToLower();
        }
    }
}

