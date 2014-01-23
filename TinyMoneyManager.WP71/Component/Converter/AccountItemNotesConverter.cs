using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using NkjSoft.Extensions;
namespace TinyMoneyManager.Component.Converter
{
    public class AccountItemNotesConverter : IValueConverter
    {
        public static char[] _spliter = new char[] { ';' };
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "N/A";

            var items = value.ToString();

            if (items.IsNullOrEmpty()) { return "N/A"; }
            else
            {
                var itemsToShow = items.Split(_spliter, StringSplitOptions.RemoveEmptyEntries);

                return itemsToShow.ToStringLine();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
