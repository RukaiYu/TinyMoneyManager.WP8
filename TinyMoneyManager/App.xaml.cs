namespace TinyMoneyManager
{
    using ImageTools.IO;
    using mangoProgressIndicator;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO.IsolatedStorage;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.DataSyncing;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages;
    using ImageTools.IO.Png;

    public partial class App : Application
    {
        public static string AlertBoxTitle = AppName;
        private static string APPCRASH = "App Crash";
        private static string APPKEY = "2e5971f921a3244a408d181-8185e420-d018-11e1-4655-00ef75f32667";
        public static string AppName;
        public static LocalyticsSession appSession;
        public const string ForumLink = "http://www.wpxap.com/";
        public static System.Action GoToRestoreBackupSdfPage;
        public static string IsMainPageUsingBackgroundImageKey = "IsMainPageUsingBackgroundImage";
        public const string LASTACCESSTIMEKEYString = "LastAccessTime";
        public const string MainVer = "1.9.7";
        public const string PCClinetAddress_SkyDrive = "https://skydrive.live.com/redir.aspx?cid=d9cb9d904309ae62&resid=D9CB9D904309AE62!551&parid=root";
        private bool phoneApplicationInitialized;
        public static string QuickNewRecordName = null;
        public static string SeniorVersion = "1529.1021";
        public static string SingleVersionInfo = ("1.9.7(" + SeniorVersion + ")");
        public const string SupportWebSiteLink = "http://yurukai.wordpress.com";
        public static SolidColorBrush SystemAccentBrush = null;
        public static SolidColorBrush SystemBackgroundColor = null;
        public static SolidColorBrush SystemForegroundColor = null;
        public const string Version = "1.9.7";

        public App()
        {
            base.UnhandledException += new System.EventHandler<ApplicationUnhandledExceptionEventArgs>(this.Application_UnhandledException);
            this.InitializeComponent();
            AppSettingRepository.Instance = ViewModelLocator.instanceLoader.LoadSingelton<AppSettingRepository>("AppSetting");
            AppSetting.LoadAppSetting(new System.Action(ApplicationHelper.LoadCurrencyConversionData));
            this.InitializePhoneApplication();
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Application.Current.Host.Settings.EnableFrameRateCounter = true;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            Encoders.AddEncoder<PngEncoder>();
            Decoders.AddDecoder<PngDecoder>();
            DataSyncingObjectManager.Version = Version;
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            LoadUIDisplayLanguage(AppSetting.Instance.DisplayLanguage);
            SetupAppSessionControl();
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            ViewModelLocator.MainPageViewModel.UpdateTileData();
            IsolatedAppSetingsHelper.UpdatLastMainPageIndex();
            appSession.close();
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            ViewModelLocator.MainPageViewModel.UpdateTileData();
            appSession.close();
        }

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            LoadUIDisplayLanguage(AppSetting.Instance.DisplayLanguage);
            LocalizedStrings.InitializeLanguage();
            this.InitializeCacheLocalizedStrings();
            AppUpdater.HandleDatabaseUpdatingAndDataPreload();
            SystemForegroundColor = Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
            SystemAccentBrush = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            SystemBackgroundColor = Application.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            TileInfoUpdatingAgent.StartTileInfoUpdatingAgent();
            SetupAppSessionControl();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
            System.Collections.Generic.Dictionary<string, string> attributes = new System.Collections.Generic.Dictionary<string, string>();
            attributes.Add("exception", e.ExceptionObject.Message);
            attributes.Add("exception call stack", e.ExceptionObject.Message);
            appSession.tagEvent(APPCRASH, attributes);
            if (e.ExceptionObject is System.InvalidOperationException)
            {
                CommonExtensions.AlertNotification(null, AppResources.FailedLoadingRequestRefresMsg, null);
                e.Handled = true;
            }
            else
            {
                MessageBox.Show(AppUpdater.EnsureWhetherNeedToRebuildDataBase(e.ExceptionObject) + "\r\n" + AppResources.DeathFailedExceptionMessage.FormatWith(new object[] { "http://yurukai.wordpress.com", e.ExceptionObject.Message }), AlertBoxTitle, MessageBoxButton.OK);
                if (MessageBox.Show(AppResources.NeedToGoToSdfBackupPage, AlertBoxTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    e.Handled = true;
                    DataSynchronizationHandler.ToBackupDataBase = true;
                    DataSynchronizationPage.NeedToConfirmRebuildDatabase = true;
                    if (GoToRestoreBackupSdfPage != null)
                    {
                        GoToRestoreBackupSdfPage();
                    }
                }
                else
                {
                    e.Handled = false;
                }
            }
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            if (base.RootVisual != this.RootFrame)
            {
                base.RootVisual = this.RootFrame;
            }
            this.RootFrame.Navigated -= new NavigatedEventHandler(this.CompleteInitializePhoneApplication);
        }

        public static string GetAppInfo()
        {
            return string.Format("\r\nApp Version\\t\\t: {0}\r\nData base Version:\\t\\t{1}", SingleVersionInfo, 8);
        }

        public void InitializeCacheLocalizedStrings()
        {
            AlertBoxTitle = AppResources.AlertBoxTitle;
            AppName = AppSetting.Instance.AppName.IsNullOrEmpty() ? AppResources.AppName : AppSetting.Instance.AppName;
            Repayment.AlarmName = AppResources.AlarmNotication;
            Repayment.ReminderName = AppResources.ReminderNotication;
            Repayment.LocalizedCompletedAt = AppResources.CompletedAt;
            QuickNewRecordName = AppResources.AddRecordName;
            base.Resources.Add("GlobalAppName", AppName);
        }

        private void InitializePhoneApplication()
        {
            if (!this.phoneApplicationInitialized)
            {
                TransitionFrame frame = new TransitionFrame
                {
                    Language = XmlLanguage.GetLanguage(AppSetting.Instance.DisplayLanguage)
                };
                this.RootFrame = frame;
                this.RootFrame.Navigated += new NavigatedEventHandler(this.CompleteInitializePhoneApplication);
                this.RootFrame.NavigationFailed += new NavigationFailedEventHandler(this.RootFrame_NavigationFailed);
                this.phoneApplicationInitialized = true;
                GlobalIndicator.Instance.Initialize(this.RootFrame);
            }
        }

        public static void LoadUIDisplayLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
                }
                catch
                {
                }
            }
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
                MessageBox.Show(e.Exception.ToString());
            }
        }

        public static void SetupAppSessionControl()
        {
            appSession = new LocalyticsSession(APPKEY);
            appSession.open();
            appSession.upload();
        }

        public static bool HasLogined
        {
            get;
            set;
        }

        public static bool IsMainPageUsingBackgroundImage
        {
            get
            {
                return (!IsolatedStorageSettings.ApplicationSettings.Contains(IsMainPageUsingBackgroundImageKey) || ((bool)IsolatedStorageSettings.ApplicationSettings[IsMainPageUsingBackgroundImageKey]));
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[IsMainPageUsingBackgroundImageKey] = value;
            }
        }

        public static System.DateTime? LastAccessTime
        {
            get
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains("LastAccessTime"))
                {
                    return null;
                }
                return (IsolatedStorageSettings.ApplicationSettings["LastAccessTime"] as System.DateTime?);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LastAccessTime"] = value;
            }
        }

        public PhoneApplicationFrame RootFrame { get; private set; }
    }
}

