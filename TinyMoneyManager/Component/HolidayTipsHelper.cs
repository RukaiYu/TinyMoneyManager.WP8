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
        public static string[] holidays = new string[] { "12-25", "01-01", "2012-01-23", "02-14" };
        public const string NoEnoughMoneyMessageTag = "#NoEnoughMoneyMessage#";

        public static void ShowHolidayWords(PhoneApplicationPage page)
        {
            System.Func<TipsItem, Boolean> predicate = null;
            System.Func<TipsItem, Boolean> func2 = null;
            string key;
            if (!hasShowToday && (System.DateTime.Now.TimeOfDay <= System.TimeSpan.FromHours(18.0)))
            {
                hasShowToday = true;
                string str = System.DateTime.Now.Date.ToString("yyyy-MM-dd");
                string str2 = System.DateTime.Now.Date.ToString("MM-dd");
                key = holidays.Contains<string>(str) ? str : string.Empty;
                key = holidays.Contains<string>(str2) ? str2 : key;
                if (holidays.Contains<string>(str) || holidays.Contains<string>(str2))
                {
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
        }
    }
}

