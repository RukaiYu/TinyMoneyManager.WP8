using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Data;
using TinyMoneyManager.CurrencyConverterByWebService;
using System.Collections.ObjectModel;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Component.Common;
using TinyMoneyManager.ViewModels.AppSettingManager;
using System.Globalization;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.ViewModels
{
    public class SettingPageViewModel : NkjSoftViewModelBase
    {
        public AppSetting AppSetting { get; set; }

        public SettingPageViewModel(AppSetting appSetting)
        {
            this.AppSetting = appSetting;
            this.AppSetting.ServerSyncIPAddress.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ServerSyncIPAddress_PropertyChanged);
            SettingEnteries = new ObservableCollection<object>();

            this.CurrentSected = appSetting.CurrencyInfo;

            this.AppSetting.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(AppSetting_PropertyChanged);
        }

        void AppSetting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayLanguage")
            {

                var lang = AppSetting.Instance.DisplayLanguage.ToLowerInvariant();

                var loadingPagePath = @"\Language\{0}\SplashScreenImage.jpg";
                if (lang == "zh-cn" || lang == "zh-sg")
                {
                    loadingPagePath = loadingPagePath.FormatWith("zh-CN");
                }
                else if (lang == "zh-tw" || lang == "zh-hk")
                {
                    loadingPagePath = loadingPagePath.FormatWith("zh-TW");
                }
                else
                {
                    loadingPagePath = loadingPagePath.FormatWith("en-US");
                }

                // move loading image.  

                var splashFileImage = Application.GetResourceStream(new Uri(loadingPagePath, UriKind.Relative));

                if (splashFileImage != null)
                {
                }
            }
        }

        void ServerSyncIPAddress_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public CurrencyWapper CurrentSected { get; set; }

        public ObservableCollection<object> SettingEnteries { get; set; }

        public void LoadEntries()
        {
            if (IsDataLoaded)
            {
                return;

            }

            IsDataLoaded = true;

            AppSettingListener commonSetting = new AppSettingListener("NormalSettingPageTitle");
            commonSetting.ObjectListenTo = AppSetting;
            commonSetting.NavigateUri = ViewPath.SettingPages.CommonSettingPage;

            commonSetting.RegisterObjectPropertyChanged((o, f) =>
            {
                return AppResources.AppSettingTitle_Common.FormatWith(
                    o.CurrencyInfo.CurrencyNameWithSymbol, o.ShowAssociatedAccountItemSummary.ToLocalizedOnOffValue());
            }, "DisplayLanguage", "DefaultCurrency", "ShowAssociatedAccountItemSummary")
            .NotifyFormat();

            var currencyRateSettingPage = new AppSettingListener("CurrencySetting");

            currencyRateSettingPage.ObjectListenTo = AppSetting;
            currencyRateSettingPage.NavigateUri = ViewPath.SettingPages.CurrencyRateSettingPage;
            currencyRateSettingPage.RegisterObjectPropertyChanged((o, e) =>
            {
                return AppResources.CurrencyRateTable_LastSyncTime
                    .FormatWith(ApplicationHelper.LastSyncAtString);
            }, "DisplayLanguage", "LastSyncAtString")
            .NotifyFormat();

            var dataSettingPage = new AppSettingListener("AppUpdating");
            dataSettingPage.ObjectListenTo = AppSetting;
            dataSettingPage.NavigateUri = ViewPath.SettingPages.DataSettingPage;
            dataSettingPage.RegisterObjectPropertyChanged((o, e) =>
            {
                return AppResources.AppUpdating_SecondTitleFormatter
                    .FormatWith(o.SubscibeNotification.ToLocalizedOnOffValue());
            }, "DisplayLanguage", "SubscibeNotificationTitle")
            .NotifyFormat();

            TwoLineListerner<Data.Model.IPAddress> dataSyncing = new TwoLineListerner<Data.Model.IPAddress>("DataSynchronization");
            dataSyncing.ObjectListenTo = AppSetting.ServerSyncIPAddress;
            dataSyncing.NavigateUri = ViewPath.SettingPages.DataSyncingSettingPage;
            dataSyncing.RegisterObjectPropertyChanged((o, e) =>
            {
                return AppResources.DataSyncing_SecondTitleFormatter
                    .FormatWith(o.HttpAddress.DefaultIfNull(AppResources.NeverSync));
            }, "Address", "DisplayLanguage")
            .NotifyFormat();

            var profileSetting = new AppSettingListener("ProfileSetting");

            profileSetting.ObjectListenTo = AppSetting;
            profileSetting.NavigateUri = ViewPath.SettingPages.ProfileSettingPage;
            profileSetting.RegisterObjectPropertyChanged((o, e) =>
            {
                return AppResources.ProfileSetting_SecondFormatter.FormatWith(
                       new CultureInfo(o.DisplayLanguage).NativeName, o.EnablePoketLock.ToLocalizedOnOffValue());
            }, "DisplayLanguage", "EnablePoketLock")
            .NotifyFormat();

            var preferenceSetting = new AppSettingListener("PreferenceSetting");

            preferenceSetting.ObjectListenTo = AppSetting;
            preferenceSetting.NavigateUri = ViewPath.SettingPages.PreferenceSettingPage;
            preferenceSetting.RegisterObjectPropertyChanged((o, e) =>
            {
                return AppResources.PreferenceSetting_SecondTitleFormatter;
            }, "DisplayLanguage")
                .NotifyFormat();

            var scheduleManagerSetting = new AppSettingListener("ScheduleManagerSetting");
            scheduleManagerSetting.ObjectListenTo = AppSetting;
            scheduleManagerSetting.NavigateUri = ViewPath.SettingPages.ScheduleManagerSettingPage;
            scheduleManagerSetting.RegisterObjectPropertyChanged((o, e) =>
            {
                return AppResources.ScheduleManagerSetting_Tips;
            }, "DisplayLanguage")
                .NotifyFormat();

            this.SettingEnteries.AddRange(
                commonSetting,
                currencyRateSettingPage,
                dataSettingPage,
                preferenceSetting,
                dataSyncing,
                scheduleManagerSetting,
                profileSetting);
        }

        public static void Update()
        {
            AppSettingRepository.Instance.Update(AppSetting.Instance);

            RapidRepository.RapidContext.CurrentContext.SaveChanges();
        }
    }
}
