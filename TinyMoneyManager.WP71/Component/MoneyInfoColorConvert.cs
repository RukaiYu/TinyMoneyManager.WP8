namespace TinyMoneyManager.Component
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows;

    public class MoneyInfoColorConvert : IValueConverter
    {
        public static Color expensesColor = Color.FromArgb(255, 249, 87, 87);
        public static Color incomeColor = Color.FromArgb(255, 81, 124, 45);

        public static SolidColorBrush expenseColorBrush = new SolidColorBrush(expensesColor);
        public static SolidColorBrush incomeColorBrush = new SolidColorBrush(incomeColor);
        public static SolidColorBrush phoneForegroundBrush = (Application.Current.Resources["PhoneForegroundBrush"]) as SolidColorBrush;

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var money = value == null ? 0.0M : Convert.ToDecimal(value);
            if (money <= 0.0M)
            {
                return expenseColorBrush;
            }
            return phoneForegroundBrush;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

