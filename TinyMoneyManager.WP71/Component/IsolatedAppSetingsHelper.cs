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
            if (App.Version != LastVersion)
            {
                LastVersion = App.Version;
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
            if (App.Version != LastVersion)
            {
                LastVersion = App.Version;
                ResetAllTipsVariables();
            }

            if (!IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<bool>(key, true))
            {
                IsolatedStorageSettings.ApplicationSettings[key] = true;
                action();
            }
        }

        /// <summary>
        /// Does the action once by.
        /// </summary>
        /// <param name="keyToCompre">The key to compre.</param>
        /// <param name="predicator">The predicator.</param>
        public static void DoActionOnceBy(string keyToCompre, Predicate<string> predicator, string newValue, Action actionToDo)
        {
            var originalValue = IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue(keyToCompre, "");

            if (predicator(originalValue))
            {
                if (actionToDo != null)
                {
                    actionToDo();
                }

                IsolatedStorageSettings.ApplicationSettings[keyToCompre] = newValue;
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

        public static DateTime LastTimeBudgetReportSaved
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.GetIsolatedStorageAppSettingValue<DateTime>("LastTimeBudgetReportSaved", DateTime.Now);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LastTimeBudgetReportSaved"] = value;
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

