namespace TinyMoneyManager.Data
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using TinyMoneyManager.Component;

    public class ConversionRateHelper
    {
        private static System.Collections.Generic.Dictionary<CurrencyType, Int32> currencyPossion;
        public static bool HasCurrencyTableStructureChanged = false;
        public const int SupportCurrencyCount = 18;

        public static int GetPossion(CurrencyType currency)
        {
            if (!CurrencyPossion.ContainsKey(currency))
            {
                int num = (int)currency;
                CurrencyPossion[currency] = num;
                return num;
            }
            return CurrencyPossion[currency];
        }

        public static ConversionCell[,] LoadDefaultConversionTable(string data)
        {
            return LoadTable(data, () => data);
        }

        public static ConversionCell[,] LoadTable(string dataFromString, System.Func<String> loadDefaultCurrencyTableContent)
        {
            string[] strArray = dataFromString.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length == 1)
            {
                strArray = dataFromString.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            if (strArray.Length < SupportCurrencyCount)
            {
                HasCurrencyTableStructureChanged = true;
                dataFromString = loadDefaultCurrencyTableContent();
                strArray = dataFromString.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (strArray.Length == 1)
                {
                    strArray = dataFromString.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                }
            }
            ConversionCell[,] cellArray = new ConversionCell[SupportCurrencyCount, SupportCurrencyCount];
            char[] separator = new char[] { ',' };
            string[] strArray2 = (from p in Enumerable.Repeat<int>(1, SupportCurrencyCount) select p.ToString()).ToArray<string>();
            int length = strArray.Length;
            for (int i = 0; i < SupportCurrencyCount; i++)
            {
                string[] strArray3 = (length < SupportCurrencyCount) ? strArray2 : strArray[i].Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < SupportCurrencyCount; j++)
                {
                    cellArray[i, j] = new ConversionCell((CurrencyType)j, strArray3[j].ToDecimal());
                }
            }
            return cellArray;
        }

        public static string Save()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < SupportCurrencyCount; i++)
            {
                for (int j = 0; j < SupportCurrencyCount; j++)
                {
                    builder.AppendFormat("{0},", new object[] { ConversionRateTable[i, j].ConversionRate });
                }
                builder.Remove(builder.Length - 1, 1);
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public static void UpdateRate(CurrencyType fromCurrency, CurrencyType toCurrencyType, decimal rate)
        {
            int possion = GetPossion(fromCurrency);
            int num2 = GetPossion(toCurrencyType);
            ConversionRateTable[possion, num2].ConversionRate = rate;
        }

        public static ConversionCell[,] ConversionRateTable
        {
            get;
            set;
        }

        public static System.Collections.Generic.Dictionary<CurrencyType, Int32> CurrencyPossion
        {
            get
            {
                if (currencyPossion == null)
                {
                    currencyPossion = new System.Collections.Generic.Dictionary<CurrencyType, Int32>();
                }
                return currencyPossion;
            }
        }
    }
}

