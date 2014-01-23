namespace TinyMoneyManager.Component
{
    using mangoProgressIndicator;
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public static class PageBaseExtensions
    {
        public static void AlertErrorMessage(this System.Exception exceptionToShow, string message)
        {
            MessageBox.Show("{0}\r\n{1}".FormatWith(new object[] { message, AppResources.ErrorDetails.FormatWith(new object[] { exceptionToShow.Message }) }), App.AlertBoxTitle, MessageBoxButton.OK);
        }

        public static void BusyForWork(this UserControl page, string workText)
        {
            GlobalIndicator.Instance.BusyForWork(workText, new object[0]);
        }

        public static void DelayWorkFor(this UserControl page, int timeOut, string text)
        {
            GlobalIndicator.Instance.DelayWorkFor(timeOut, text);
        }

        public static string GetCurrencyString(this AccountItemMoney budgetMoney)
        {
            if (budgetMoney == null)
            {
                return AppSetting.Instance.DefaultCurrency.GetCurrentString();
            }
            return budgetMoney.Currency.GetCurrentString();
        }

        public static string GetLanguageInfoByKey(this PhoneApplicationPage page, string keyName)
        {
            return LocalizedStrings.GetLanguageInfoByKey(keyName);
        }

        public static string GetLanguateInfoByKeys(this PhoneApplicationPage page, string formatter, params string[] keys)
        {
            return LocalizedStrings.GetLanguageInfoByKey(formatter, keys);
        }

        public static void ShowErrorMessageAfterConfirm(this PhoneApplicationPage page, string msg, bool isKeyFromLanguageTable = false)
        {
            if (MessageBox.Show(LocalizedStrings.GetLanguageInfoByKey("BeforeShowExceptionInfoMessage"), App.AlertBoxTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                msg = isKeyFromLanguageTable ? LocalizedStrings.GetLanguageInfoByKey(msg) : msg;
                MessageBox.Show(msg, App.AlertBoxTitle, MessageBoxButton.OK);
            }
        }

        public static void WorkDone(this UserControl page)
        {
            GlobalIndicator.Instance.WorkDone();
        }

        public static void WorkDoneStillShowTray(this UserControl page)
        {
            GlobalIndicator.Instance.WorkDoneStillShowTray();
        }
    }
}

