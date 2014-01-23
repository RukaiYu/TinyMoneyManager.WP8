namespace TinyMoneyManager.Component
{
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.IO.IsolatedStorage;

    public class IsolatedAppSetingsHelper
    {
        public static string[] AllTipsVariables = new string[] { "_PCClientHasUpdatedNotified", "HasShowHowToAddCategoryToFavourite" };
        public const string HasFixedMonthlyBudgetReportBugKey = "HasFixedMonthlyBudgetReportBug";
        public const string HasShowHowToAddCategoryToFavourite = "HasShowHowToAddCategoryToFavourite";
        public const string HasShowUpdatedKey = "_PCClientHasUpdatedNotified";
        public static string HowToCreateExpenseOrIncomeSchedule = "HowToCreateExpenseOrIncomeSchedule";
        public static int LastMainPageIndexCache = 0;
        public static System.Func<Int32> LastMainPageIndexCacheGetter;
        public const string lastMainPageIndexKey = "lastMainPageIndexKey";
        private static string lastVersion = string.Empty;
        private const string lastVersionKey = "lastVersionForApp";
        public const string MonthHasSaveBudgetReportKey = "MonthHasSaveBudgetReport";

        public static void DoActionOnce(string key, System.Action action)
        {
            if ("1.9.7" != LastVersion)
            {
                LastVersion = "1.9.7";
                ResetAllTipsVariables();
            }
        }

        public static void LoadLastMainPageIndex()
        {
            LastMainPageIndexCache = LastMainPageIndex;
        }

        public static bool MonthHasSaveBudgetReport(System.DateTime dateTime)
        {
            return (IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<string>("MonthHasSaveBudgetReport", "2012-01") == dateTime.ToString("yyyy-MM"));
        }

        private static void ResetAllTipsVariables()
        {
            foreach (string str in AllTipsVariables)
            {
                IsolatedStorageSettings.ApplicationSettings[str] = false;
            }
        }

        public static void ShowTipsByVerion(string key, System.Action action)
        {
            if ("1.9.7" != LastVersion)
            {
                LastVersion = "1.9.7";
                ResetAllTipsVariables();
            }
            if (!IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<bool>(key, true))
            {
                IsolatedStorageSettings.ApplicationSettings[key] = true;
                action();
            }
        }

        public static void UpdatLastMainPageIndex()
        {
            LastMainPageIndex = LastMainPageIndexCache;
        }

        public static bool HasFixedMonthlyBudgetReportBug
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<bool>("HasFixedMonthlyBudgetReportBug", false);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["HasFixedMonthlyBudgetReportBug"] = value;
            }
        }

        public static int LastMainPageIndex
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<int>("lastMainPageIndexKey", 0);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["lastMainPageIndexKey"] = value;
            }
        }

        public static string LastVersion
        {
            get
            {
                if (lastVersion.Length == 0)
                {
                    lastVersion = IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<string>("lastVersionForApp", "1.7.0");
                }
                return lastVersion;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["lastVersionForApp"] = value;
                lastVersion = value;
            }
        }
    }
}

