namespace TinyMoneyManager.Component
{
    using Coding4Fun.Phone.Controls;
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;

    public static class CommonExtensions
    {
        private static FullViewDateTimeConverter hdt;
        public const string HowToKey = "HowTo";

        public static void Alert(this Control page, string text, string title = null)
        {
            title = string.IsNullOrEmpty(title) ? App.AlertBoxTitle : title;
            MessageBox.Show(text, title, MessageBoxButton.OK);
        }

        public static MessageBoxResult AlertConfirm(this PhoneApplicationPage page, string text, System.Action isOkToDo = null, string title = null)
        {
            title = string.IsNullOrEmpty(title) ? App.AlertBoxTitle : title;
            MessageBoxResult result = MessageBox.Show(text, title, MessageBoxButton.OKCancel);
            if ((result == MessageBoxResult.OK) && (isOkToDo != null))
            {
                isOkToDo();
            }
            return result;
        }

        public static void AlertNotification(this Control page, string text, string title = null)
        {
            new ToastPrompt { MillisecondsUntilHidden = 0x7d0, Title = title ?? string.Empty, Message = text }.Show();
        }

        public static FullViewDateTimeConverter GetHourlyDateTimeConverter()
        {
            if (hdt == null)
            {
                hdt = new FullViewDateTimeConverter();
            }
            return hdt;
        }

        public static void Open(this PhoneApplicationPage page, string startUri = "/Pages")
        {
            string name = page.Name;
            startUri = "{0}/{1}.xaml".FormatWith(new object[] { startUri, name.Replace(".", "/") });
            page.NavigationService.Navigate(new Uri(startUri, UriKind.RelativeOrAbsolute));
        }

        public static void ShowTipsOnce(this UserControl pageOrUserControl, string isoKey, string messageTitleKey, string localizedStringKey = null)
        {
            IsolatedAppSetingsHelper.ShowTipsByVerion(isoKey, delegate
            {
                localizedStringKey = localizedStringKey ?? ("HowTo" + messageTitleKey);
                string title = LocalizedStrings.GetLanguageInfoByKey("HowTo").FormatWith(new object[] { LocalizedStrings.GetLanguageInfoByKey(messageTitleKey) });
                pageOrUserControl.Alert(LocalizedStrings.GetLanguageInfoByKey(localizedStringKey), title);
            });
        }

        public static void ShowTipsOnceDirectly(this UserControl pageOrUserControl, string isoKey, string messageTitle, string message)
        {
            IsolatedAppSetingsHelper.ShowTipsByVerion(isoKey, delegate
            {
                pageOrUserControl.Alert("{0}\r\n\r\n    {1}".FormatWith(new object[] { messageTitle, message }), null);
            });
        }

        public static string ToLocalizedDateString(this System.DateTime dateTime)
        {
            return dateTime.ToString(LocalizedStrings.CultureName.DateTimeFormat.ShortDatePattern, LocalizedStrings.CultureName);
        }

        public static string ToLocalizedDateTimeString(this System.DateTime dateTime)
        {
            return dateTime.ToString(LocalizedStrings.CultureName);
        }

        public static string ToLocalizedOnOffValue(this bool trueOrFalse, string formatter = null)
        {
            string languageInfoByKey = LocalizedStrings.GetLanguageInfoByKey(trueOrFalse ? "On" : "Off");
            if (formatter == null)
            {
                return languageInfoByKey;
            }
            return formatter.FormatWith(new object[] { languageInfoByKey });
        }
    }
}

