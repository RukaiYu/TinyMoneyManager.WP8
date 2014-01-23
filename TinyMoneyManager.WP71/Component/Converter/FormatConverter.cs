namespace GlobalizationSample.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class FormatConverter : IValueConverter
    {
        public virtual object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new System.ArgumentNullException("targetType");
            }
            string str = parameter as string;
            if (!string.IsNullOrEmpty(str))
            {
                return string.Format(culture, str, new object[] { value });
            }
            return value;
        }

        public virtual object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

