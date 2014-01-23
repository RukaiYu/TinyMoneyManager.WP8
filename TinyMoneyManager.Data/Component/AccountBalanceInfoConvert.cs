namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TinyMoneyManager.Data.Model;

    public class AccountBalanceInfoConvert : IValueConverter
    {
        private static string currentBalance = string.Empty;
        private static string foratter = "{0} : {1}";

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Account account = value as Account;
            if (account == null)
            {
                return "N/A";
            }
            if (currentBalance.Length == 0)
            {
                currentBalance = LocalizedObjectHelper.GetLocalizedStringFrom("CurrentBalance");
            }
            return GetBalanceInfo(account);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        public static string GetBalanceInfo(Account account)
        {
            if (currentBalance.Length == 0)
            {
                currentBalance = LocalizedObjectHelper.GetLocalizedStringFrom("CurrentBalance");
            }
            string str = "{0}{1}".FormatWith(new object[] { account.CurrencyTypeSymbol, account.Balance.GetValueOrDefault().ToMoneyF2() });
            return foratter.FormatWith(new object[] { currentBalance, str });
        }
    }
}

