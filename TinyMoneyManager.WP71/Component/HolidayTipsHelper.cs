namespace TinyMoneyManager.Component
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.ViewModels;

    public class HolidayTipsHelper
    {
        public static bool hasShowToday = false;
        public static string[] holidays = new string[] { "12-25", "01-01", "2013-02-09", "02-14" };
        public const string NoEnoughMoneyMessageTag = "#NoEnoughMoneyMessage#";

        /// <summary>
        /// Shows the holiday words.
        /// </summary>
        /// <param name="page">The page.</param>
        public static void ShowHolidayWords(PhoneApplicationPage page)
        {
            string key;
            string str2 = System.DateTime.Now.Date.ToString("MM-dd");
            var str = System.DateTime.Now.Date.ToString("yyyy-MM-dd");

            if (holidays.Contains<string>(str) || holidays.Contains<string>(str2))
            {
                IsolatedAppSetingsHelper.DoActionOnceBy("HolidayKey", (p) => p != str2, str2, () =>
                {
                    if (!hasShowToday && (System.DateTime.Now.TimeOfDay <= System.TimeSpan.FromHours(18.0)))
                    {
                        hasShowToday = true;
                        key = holidays.Contains<string>(str) ? str : string.Empty;
                        key = holidays.Contains<string>(str2) ? str2 : key;
                        if (holidays.Contains<string>(str) || holidays.Contains<string>(str2))
                        {
                            System.Func<TipsItem, Boolean> predicate = null;
                            System.Func<TipsItem, Boolean> func2 = null;

                            System.Collections.Generic.IEnumerable<TipsItem> tips = AboutPageViewModel.GetTips(4);
                            if (predicate == null)
                            {
                                predicate = p => p.Text.Contains(key);
                            }
                            if (tips.Count<TipsItem>(predicate) != 0)
                            {
                                if (func2 == null)
                                {
                                    func2 = p => p.Text.Contains(key);
                                }
                                key = tips.FirstOrDefault<TipsItem>(func2).Text.Replace("#NTL#", "\r\n").Replace("[" + key + "]", string.Empty).FormatWith(new object[] { ViewModelLocator.MainPageViewModel.AccountInfoSummary.MoneyInfo.MoneyInfo });
                                decimal money = ViewModelLocator.MainPageViewModel.AccountInfoSummary.MoneyInfo.Money;
                                if (key.Contains("#NoEnoughMoneyMessage#"))
                                {
                                    string newValue = string.Empty;
                                    if (((money > 0.0M) && (money <= 1000M)) || (money < 0M))
                                    {
                                        newValue = LocalizedStrings.GetLanguageInfoByKey("NoEnoughMoneyMessage");
                                    }
                                    key = key.Replace("#NoEnoughMoneyMessage#", newValue);
                                }
                                page.Alert(key, null);
                            }
                        }

                    }
                });
            }
        }
    }
}

