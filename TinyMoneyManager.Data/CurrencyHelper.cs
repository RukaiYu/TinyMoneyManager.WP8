namespace TinyMoneyManager.Data
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class CurrencyHelper
    {
        private static Dictionary<CurrencyType, CurrencyWapper> _CurrencyWapperCache = new Dictionary<CurrencyType, CurrencyWapper>();

        public static ObservableCollection<CurrencyWapper> CurrencyTable = new ObservableCollection<CurrencyWapper>(
            new System.Collections.Generic.List<CurrencyWapper>(){ 
                new CurrencyWapper(),
                new CurrencyWapper(CurrencyType.HKD,CurrencySymbols.Dollar),
                new CurrencyWapper(CurrencyType.NTD,CurrencySymbols.Dollar),
                new CurrencyWapper(CurrencyType.USD,CurrencySymbols.Dollar),
                new CurrencyWapper(CurrencyType.EUR,CurrencySymbols.Europe),
                new CurrencyWapper(CurrencyType.JPY,CurrencySymbols.Yuan),
                new CurrencyWapper(CurrencyType.AUD, CurrencySymbols.Dollar),
                new CurrencyWapper(CurrencyType.GBP, CurrencySymbols.GreatPound),
                new CurrencyWapper(CurrencyType.MYR, CurrencySymbols.MalDollar),
                new CurrencyWapper(CurrencyType.SGD, CurrencySymbols.SsingaporeDollar),
                new CurrencyWapper(CurrencyType.THP,CurrencySymbols.ThaiBaht),
                new CurrencyWapper(CurrencyType.PKR,CurrencySymbols.PKRS), //11
                new CurrencyWapper(CurrencyType.INR,CurrencySymbols.INR),//12
                new CurrencyWapper(CurrencyType.KRW,CurrencySymbols.KRW),//13
                new CurrencyWapper(CurrencyType.IDR,CurrencySymbols.IDR),//14
                new CurrencyWapper(CurrencyType.BYR,CurrencySymbols.BYR),//15
                new CurrencyWapper(CurrencyType.PHP, CurrencySymbols.PHP),  // 16
                new CurrencyWapper(CurrencyType.RUB, "Rbl."),  // 17
                new CurrencyWapper(CurrencyType.PHP, "HUF"),  // 18

         });

        internal static CurrencyType ConvertCurrencyBy(string currencyName)
        {
            return CurrencyType.CNY;
        }

        public static CurrencyWapper GetCurrencyItemByType(CurrencyType currencyType)
        {
            CurrencyWapper result = null;
            if (_CurrencyWapperCache.ContainsKey(currencyType))
            {
                result = _CurrencyWapperCache[currencyType];
            }
            else
            {
                result = CurrencyTable.FirstOrDefault<CurrencyWapper>(p => (p.Currency == currencyType));

                _CurrencyWapperCache[currencyType] = result;
            }

            // TODO: should do cache here 
            return result;

        }

        public static decimal GetGlobleMoneyFrom(CurrencyType fromCurrency, decimal money)
        {
            CurrencyType defaultCurrency = AppSetting.Instance.DefaultCurrency;
            if (defaultCurrency == fromCurrency)
            {
                return money;
            }
            return (money * fromCurrency.GetConversionRateTo(defaultCurrency));
        }

        public static void RaiseRateChanging(CurrencyType currencyRiseChanger)
        {
            ((System.Collections.Generic.IEnumerable<CurrencyWapper>)CurrencyTable).ForEach<CurrencyWapper>(delegate(CurrencyWapper p)
            {
                p.RaiseRateChanged();
            });
        }
    }
}

