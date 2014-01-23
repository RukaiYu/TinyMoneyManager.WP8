namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;

    public class AccountItemMoney : MultipleThreadSupportedNotionObject, IMoney
    {
        public System.Func<CurrencyType, Decimal, String> CurrencySymbolGetter;
        private decimal money;
        public static readonly string MoneyProperty = "Money";

        public AccountItemMoney()
        {
            this.CurrencySymbolGetter = new System.Func<CurrencyType, Decimal, String>(this.getCurrencySymbolProxy);
        }

        private string getCurrencySymbolProxy(CurrencyType currency, decimal money)
        {
            return GetMoneyInfoWithCurrency(currency, new decimal?(money), "{0}{1}");
        }

        public decimal? GetMoney()
        {
            return new decimal?(this.Money);
        }

        public static string GetMoneyInfoWithCurrency(decimal? money, string formatter = "{0}{1}")
        {
            return GetMoneyInfoWithCurrency(AppSetting.Instance.DefaultCurrency, money, formatter);
        }

        public static string GetMoneyInfoWithCurrency(string symbol, decimal? money, string formatter = "{0}{1}")
        {
            return string.Format(formatter, symbol, money.GetValueOrDefault().ToMoneyF2(System.Threading.Thread.CurrentThread.CurrentCulture));
        }

        public static string GetMoneyInfoWithCurrency(CurrencyType currency, decimal? money, string formatter = "{0}{1}")
        {
            return string.Format(formatter, currency.GetCurrentString(), money.GetValueOrDefault().ToMoneyF2(LocalizedObjectHelper.CultureInfoCurrentUsed));
        }

        public CurrencyType Currency { get; set; }

        public decimal Money
        {
            get
            {
                return this.money;
            }
            set
            {
                this.OnNotifyPropertyChanging("MoneyInfo");
                this.money = value;
                this.OnNotifyPropertyChanged(MoneyProperty);
                this.OnNotifyPropertyChanged("MoneyInfo");
            }
        }

        public string MoneyInfo
        {
            get
            {
                return this.CurrencySymbolGetter(this.Currency, this.Money);
            }
        }
    }
}

