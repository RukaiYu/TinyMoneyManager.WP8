namespace TinyMoneyManager.Component
{
    using System;
    using System.Globalization;

    public static class LocalizedObjectHelper
    {
        public static System.Globalization.CultureInfo CultureInfoCurrentUsed;
        public static System.Func<String, String, String> GetCombinedText;
        public static System.Func<String, String> GetLocalizedStringFrom;
    }
}

