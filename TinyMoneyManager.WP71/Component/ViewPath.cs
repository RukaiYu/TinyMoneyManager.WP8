namespace TinyMoneyManager
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Resources;

    public class ViewPath
    {
        public const string AboutPage = "/Pages/AboutPage.xaml";
        public const string AccountEditorPage = "/Pages/AccountEditorPage.xaml";
        public const string AccountItemListPage = "/Pages/AccountItemListPage.xaml";
        public const string AccountItemViewerPage = "/Pages/AccountItemViews/AccountItemViewer.xaml?id={0}";
        public const string AccountManagerPage = "/Pages/AccountManager.xaml";
        public const string AccountTransferingPage = "/Pages/AccountTransferingPage.xaml";
        public const string BorrowLeanAndPeopleManager = "/Pages/BorrowLeanManager.xaml";
        public static string BorrowLeanInfoViewerPage = "/Pages/BorrowAndLean/BorrowLoanInfoViewerPage.xaml?id={0}";
        public const string CategoryEditPage = "/Pages/CategoryEditPage.xaml";
        public const string CategoryPickerDialogBox = "/Pages/CategoryPickerDialogBox.xaml";
        public const string DataSynchronizationPage = "/Pages/DataSynchronizationPage.xaml";
        public const string DataSynchronizationPageWithoutExitRequest = "/Pages/DataSynchronizationPage.xaml?from=normal";
        public static string DataSyncingPage = "/Pages/DataSyncing/SkyDriveDataSyncingPage.xaml";
        public static string EditValueInTextBoxEditor = "/Pages/DialogBox/EditValueInTextBoxEditor.xaml?keyName={0}&value={1}";
        public const string MainPage = "/MainPage.xaml";
        public const string MonthSelectorPage = "/Pages/MonthSelectorPage.xaml";
        public const string NewOrEditAccountItemPage = "/Pages/NewOrEditAccountItemPage.xaml";
        public const string PasswordSetting = "/Pages/PasswordSetting.xaml";
        public const string PcClientPage = "/Pages/DataSynchronizationPage.xaml";
        public const string PCClienURL = "https://skydrive.live.com/?cid=d9cb9d904309ae62#!/?cid=d9cb9d904309ae62&sc=documents&uc=1&id=D9CB9D904309AE62%21551";
        public const string RepaymentItemEditorPagePath = "/Pages/RepaymentManagerViews/RepaymentItemEditor.xaml?action={0}&itemId={1}";
        public const string RepaymentManagerPath = "/Pages/RepaymentManager.xaml";
        public static string RepaymentOrReceiptViewerPage = "/Pages/BorrowAndLean/BorrowOrLoanRepayReceiveInfoViewerPage.xaml?id={0}";
        public static string ScheduleEditorPage = "/Pages/DialogBox/EditExpenseOrIncomeSchedule.xaml?action={0}&id={1}";
        public const string SettingPage = "/Pages/SettingPage.xaml";
        public const string SkyDriveDataSynchronizationPage = "/Pages/SkyDriveDataSyncingPage.xaml";
        public const string StatisticsPage = "/Pages/StatisticsPage.xaml";

        public static string LoadContentFromFile(string file)
        {
            StreamResourceInfo resourceStream = Application.GetResourceStream(new Uri("/TinyMoneyManager;component/" + file, UriKind.Relative));
            System.IO.StreamReader reader = new System.IO.StreamReader(resourceStream.Stream);
            string str = reader.ReadToEnd();
            reader.Close();
            resourceStream.Stream.Close();
            return str;
        }

        public class BudgetManagement
        {
            public const string BudgetProjectEditPage = "/Pages/BudgetManagement/BudgetProjectEditPage.xaml?action={0}";
        }

        public class SettingPages
        {
            public static readonly string CategorySettingPage = "/Pages/CategoryManager/CategoryManagment.xaml?type={0}&selectionMode={1}";
            public static readonly string CommonSettingPage = "/Pages/AppSettingPage/CommonSettingPage.xaml";
            public static readonly string CurrencyRateSettingPage = "/Pages/AppSettingPage/CurrencyRateSettingPage.xaml";
            public static readonly string DataSettingPage = "/Pages/AppSettingPage/DataSettingPage.xaml";
            public static readonly string DataSyncingSettingPage = "/Pages/AppSettingPage/DataSyncingSettingPage.xaml";
            public static readonly string PreferenceSettingPage = "/Pages/AppSettingPage/PreferenceSettingPage.xaml?goto={0}";
            public static readonly string ProfileSettingPage = "/Pages/AppSettingPage/ProfileSettingPage.xaml";
            public static readonly string ScheduleManagerSettingPage = "/Pages/AppSettingPage/ScheduleManager.xaml";

            public static string BudgetAndStasticsSettingsPage = "/Pages/AppSettingPage/BudgetAndStasticsSettings.xaml";

            public static readonly string TileSettingPage = "/Pages/AppSettingPage/TileSettings.xaml";

            public static readonly string VoiceCommandSettingsPaga = "/Pages/AppSettingPage/VoiceCommandSettings.xaml";
        }
    }
}

