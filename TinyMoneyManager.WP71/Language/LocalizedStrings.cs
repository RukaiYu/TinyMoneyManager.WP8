namespace TinyMoneyManager
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Language;

    public class LocalizedStrings
    {
        public static System.Globalization.CultureInfo CultureName;

        private static AppResources localizedresources = new AppResources();
        private static System.Collections.Generic.Dictionary<string, string> mulitipleDict = new System.Collections.Generic.Dictionary<string, string>();
        private static System.Resources.ResourceManager rm = null;

        public static event System.EventHandler LanguageChanged;
        public static string GetCombinedText(string key1, string key2, bool fromKey = false)
        {
            if (fromKey)
            {
                return AppResources.BlankWithFormatter.FormatWith(new object[] { GetLanguageInfoByKey(key1), GetLanguageInfoByKey(key2) });
            }
            return AppResources.BlankWithFormatter.FormatWith(new object[] { key1, key2 });
        }

        public static string GetLanguageInfoByKey(string keyName)
        {
            string str = rm.GetString(keyName);
            if (string.IsNullOrEmpty(str))
            {
                return keyName;
            }
            return str;
        }

        public static string GetLanguageInfoByKey(string formatter, params string[] keys)
        {
            string key = keys.ToStringLine<string>(",");
            if (!mulitipleDict.ContainsKey(key))
            {
                mulitipleDict[key] = string.Format(formatter, (object[])getMultipleValueByKeys(keys).ToArray<string>());
            }
            return mulitipleDict[key];
        }

        public static Uri GetLocailizedResourceUriFrom(string resourcePath, params object[] args)
        {
            return GetLocailizedResourceUriFrom(UriKind.RelativeOrAbsolute, resourcePath, args);
        }

        public static Uri GetLocailizedResourceUriFrom(UriKind uriKind, string resourcePath, params object[] args)
        {
            return new Uri(string.Format(CultureName, resourcePath, new object[] { CultureName.Name, args }), uriKind);
        }

        public static System.Collections.Generic.IEnumerable<String> getMultipleValueByKeys(params string[] keys)
        {
            foreach (string iteratorVariable0 in keys)
            {
                yield return rm.GetString(iteratorVariable0);
            }
        }

        public static void InitializeLanguage()
        {
            rm = AppResources.ResourceManager;
            CultureName = System.Threading.Thread.CurrentThread.CurrentCulture;
            CommonExtension.CultureForConverter = System.Threading.Thread.CurrentThread.CurrentCulture;
            LocalizedObjectHelper.CultureInfoCurrentUsed = CultureName;
            LocalizedObjectHelper.GetLocalizedStringFrom = new System.Func<string, string>(LocalizedStrings.GetLanguageInfoByKey);
            LocalizedObjectHelper.GetCombinedText = (key1, key2) => GetCombinedText(key1, key2, true);
        }

        protected static void OnLanguageChanged()
        {
            System.EventHandler languageChanged = LanguageChanged;
            if (languageChanged != null)
            {
                languageChanged(rm, System.EventArgs.Empty);
            }
        }

        public static void SetLanguage(string language)
        {
        }

        public static AppResources Localizedresources
        {
            get
            {
                return localizedresources;
            }
        }
    }
}

