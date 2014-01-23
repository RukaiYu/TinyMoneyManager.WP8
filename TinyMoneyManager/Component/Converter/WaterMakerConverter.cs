namespace TinyMoneyManager.Component.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TinyMoneyManager;

    public class WaterMakerConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value != null) && !string.IsNullOrEmpty((string) value))
            {
                return value;
            }
            return LocalizedStrings.GetLanguageInfoByKey(parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

