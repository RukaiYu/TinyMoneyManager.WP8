namespace TinyMoneyManager.Component
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class TrunToFalseWhen : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            bool.TryParse(value.ToString(), out result);
            if (!result)
            {
                return false;
            }
            bool flag2 = false;
            bool.TryParse(parameter.ToString(), out flag2);
            if (result == flag2)
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

