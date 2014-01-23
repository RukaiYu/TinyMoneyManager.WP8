namespace TinyMoneyManager
{
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;

    public static class CurrencyExtensions
    {
        public static string GetAmountInfoWithCurrencySymbol(this CurrencyType currencyType, string symbol, object amount)
        {
            return "{0}{1}".FormatWith(new object[] { symbol, amount.ToString() });
        }

        public static decimal GetConversionRateTo(this CurrencyType currencyFrom, CurrencyType currencyTo)
        {
            int possion = ConversionRateHelper.GetPossion(currencyFrom);
            int num2 = ConversionRateHelper.GetPossion(currencyTo);
            return ConversionRateHelper.ConversionRateTable[possion, num2].ConversionRate;
        }

        public static string GetCurrencyStringWithNameFirst(this CurrencyType currencyType)
        {
            return CurrencyHelper.GetCurrencyItemByType(currencyType).CurrencyStringWithNameFirst;
        }

        public static string GetCurrencySymbolWithMoney(this CurrencyType currencyType, decimal? money = new decimal?())
        {
            return string.Format("{0}{1}", currencyType.GetCurrentString(), !money.HasValue ? string.Empty : money.Value.ToMoneyF2());
        }

        public static string GetCurrentString(this CurrencyType currencyType)
        {
            return CurrencyHelper.GetCurrencyItemByType(currencyType).CurrencyString;
        }

        public static string GetGloableCurrencyNameWithSymbol(this CurrencyType currencyType, decimal? money = new decimal?())
        {
            return string.Format("{0}{1}", currencyType.GetCurrencyStringWithNameFirst(), !money.HasValue ? string.Empty : money.Value.ToMoneyF2());
        }

        public static string GetGloableCurrencySymbol(this CurrencyType currencyType, decimal? money = new decimal?())
        {
            if (AppSetting.Instance.GlobleCurrencySymbolStyle == CurrencySymbolStyle.LongWithName)
            {
                return currencyType.GetGloableCurrencyNameWithSymbol(money);
            }
            return currencyType.GetCurrencySymbolWithMoney(money);
        }
    }
}

