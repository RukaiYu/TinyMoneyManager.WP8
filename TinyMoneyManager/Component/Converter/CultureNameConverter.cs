namespace TinyMoneyManager.Component.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class CultureNameConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new System.ArgumentNullException("targetType");
            }
            if (!(((string) value) == string.Empty))
            {
                return new System.Globalization.CultureInfo((string) value).DisplayName;
            }
            if (parameter == null)
            {
                throw new System.NotSupportedException();
            }
            return parameter;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

