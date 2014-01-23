namespace TinyMoneyManager.Data
{
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;

    public class CurrencyWapper : NotionObject
    {
        public static CurrencyType CompareToCurrency;
        public static System.Action<CurrencyType, CurrencyType, Decimal> ConversionRateHelper_UpdateRateRexy;

        public CurrencyWapper()
            : this(CurrencyType.CNY, "\x00a5")
        {
        }

        public CurrencyWapper(CurrencyType currency, string currencyString)
        {
            this.Currency = currency;
            this.CurrencyString = currencyString;
            this.CurrencyStringWithNameFirst = currency.ToString().Substring(0, 2) + currencyString;
        }

        public CurrencyWapper(int currencyOfInt, string currencyString)
        {
            this.Currency = currencyOfInt.ToEnum<CurrencyType>();
            this.CurrencyString = currencyString;
            this.CurrencyStringWithNameFirst = this.Currency.ToString().Substring(0, 2) + currencyString;
        }

        public void RaiseRateChanged()
        {
            this.OnNotifyPropertyChanged("RateToCompareToCurrency");
        }

        public void SetCurrency(CurrencyType fromCurrency, decimal rate)
        {
            ConversionRateHelper_UpdateRateRexy(fromCurrency, this.Currency, rate);
            this.OnNotifyPropertyChanged("RateToCompareToCurrency");
        }


        public CurrencyType Currency { get; set; }

        public string CurrencyNameWithSymbol
        {
            get
            {
                return "{0}({1})".FormatWith(new object[] { this.Currency, this.CurrencyString });
            }
        }

        [XmlIgnore]
        public string CurrencyString { get; set; }

        [XmlIgnore]
        public string CurrencyStringWithNameFirst { get; set; }

        public string RateToCompareToCurrency
        {
            get
            {
                return CompareToCurrency.GetConversionRateTo(this.Currency).ToString("F4");
            }
        }
    }
}

