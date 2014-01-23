namespace TinyMoneyManager
{
    using Coding4Fun.Phone.Controls;
    using mangoProgressIndicator;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages;
    using TinyMoneyManager.Pages.CustomizedTally;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.Controls;

    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public static System.Action<Stream> backgroundImageSetter;
        private string currentPanoramaItemName;
        public int daysWithoutTally;
        private const string detailPanoramaItemName = "SummaryPanoramaItem";
        private bool hasCheckFromQuickNewRecordToMainPage;
        private bool hasNavigated;
        private bool hasRunLoaded;
        private string image;
        public static bool IsFromMain = false;

        private MenuItemViewModel mainPageViewModel;

        private Thickness margin12120000;

        private bool needShowNotificatioToDoTally;

        private string passwordErrorMessage;
        private ApplicationSafetyService safetyService;

        public const int TimeoutToShowNotification = 7;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            System.Action action = null;
            this.currentPanoramaItemName = string.Empty;
            this.passwordErrorMessage = string.Empty;
            this.image = string.Empty;
            this.margin12120000 = new Thickness(12.0, 12.0, 0.0, 0.0);
            base.Resources.Add("GlobalAppNameLower", AppSetting.Instance.AppName.ToLowerInvariant());
            this.InitializeComponent();
            if (action == null)
            {
                action = delegate
                {
                    this.NavigateTo("/Pages/DataSynchronizationPage.xaml");
                };
            }
            App.GoToRestoreBackupSdfPage = action;
            TiltEffect.SetIsTiltEnabled(this, true);
            backgroundImageSetter = new System.Action<Stream>(this.setBackgroundImage);
            IsolatedAppSetingsHelper.LoadLastMainPageIndex();
            this.mainPageViewModel = ViewModelLocator.MainPageViewModel;
            base.DataContext = this.mainPageViewModel;
            this.InitializeMenu();
            this.StartApp();
            base.Loaded += new RoutedEventHandler(this.MainPage_Loaded);
            base.BackKeyPress += new System.EventHandler<CancelEventArgs>(this.MainPage_BackKeyPress);
            System.DateTime? lastAccessTime = App.LastAccessTime;
            if (lastAccessTime.HasValue)
            {
                System.TimeSpan span = (System.TimeSpan)(System.DateTime.Now.Date - lastAccessTime.Value.Date);
                this.daysWithoutTally = span.Days;
                this.needShowNotificatioToDoTally = this.daysWithoutTally >= 7;
            }
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.needShowNotificatioToDoTally = true;
            }
            IsolatedAppSetingsHelper.LastMainPageIndexCache = (IsolatedAppSetingsHelper.LastMainPageIndexCache < 0) ? 0 : IsolatedAppSetingsHelper.LastMainPageIndexCache;

            if (UpdatingController.HasSomeThingToDoBeforeGoToMainPage(this))
            {
                IsolatedAppSetingsHelper.LastMainPageIndexCache = 0;
            }

            this.MainPanorama.DefaultItem = this.MainPanorama.Items[IsolatedAppSetingsHelper.LastMainPageIndexCache];
            TextBlock block = new TextBlock
            {
                Text = AppResources.Today.ToLowerInvariant(),
                FontSize = 48.0,
                Margin = new Thickness(0.0, 40.0, 0.0, 0.0)
            };
            this.RecentItemList.Header = block;
            Binding binding = new Binding("FavoritesPageVisibiable")
            {
                Source = AppSetting.Instance
            };
            this.RecentItemList.SetBinding(UIElement.VisibilityProperty, binding);
        }

        private void AddAccountItemButton_Click(object sender, RoutedEventArgs e)
        {
            IsFromMain = true;

            NewOrEditAccountItemPage.IsSelectionMode = false;
            this.NavigateToPage("/Pages/NewOrEditAccountItemPage.xaml?action=add&FromMainPlus=true");
        }

        private void AfterPasswordPassed()
        {
            this.AddAccountItemButton.Visibility = Visibility.Visible;
            this.SummaryPanoramaItem.Visibility = Visibility.Visible;
            App.HasLogined = true;
            ViewModelLocator.ScheduleManagerViewModel.RecoveryDatas(false, new System.Action(this.UpdateSummary));
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
        }

        private void CheckLastTimeTally()
        {
            if (this.needShowNotificatioToDoTally)
            {
                this.AlertConfirm(LocalizedStrings.GetLanguageInfoByKey("NoTallyOverTimemoutDaysNotificationMessage").FormatWith(new object[] { this.daysWithoutTally }), delegate
                {
                    this.AddAccountItemButton_Click(this.AddAccountItemButton, null);
                }, null);
                this.needShowNotificatioToDoTally = false;
            }
        }

        private bool EnsureFireEvent(string panoramaItemName)
        {
            return (this.currentPanoramaItemName == panoramaItemName);
        }

        private void FavoriteItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.EnsureFireEvent("RecentItemList"))
            {
                TallySchedule tag = (sender as Grid).Tag as TallySchedule;
                if (tag != null)
                {
                    CustomizedTallyPage.Go(this, tag);
                }
            }
        }

        private void GoForMoreHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.EnsureFireEvent("SummaryPanoramaItem"))
            {
                this.NavigateTo("/Pages/Summary/ParticularsDetails.xaml");
            }
        }

        private void GoToAnalysisPageIconButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void GoToBudgetItemFromCategory_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo("/Pages/BudgetManagement/BudgetProjectListPage.xaml?itemType=Expenses");
        }

        private void InitializeMenu()
        {
        }

        private void InitializeSafeService()
        {
            this.safetyService = new ApplicationSafetyService(this);
            this.safetyService.AfterPasswordPassed = new System.Action(this.AfterPasswordPassed);
            this.safetyService.BeforePopupPasswordInputDialog = delegate
            {
                this.AddAccountItemButton.Visibility = Visibility.Collapsed;
                this.SummaryPanoramaItem.Visibility = Visibility.Collapsed;
            };
            this.safetyService.AfterPasswordWrong = delegate(string msg)
            {
                if (base.NavigationService.CanGoBack)
                {
                    base.NavigationService.GoBack();
                }
                else
                {
                    this.mainPageViewModel.Items.Clear();
                    this.SetMyAccountBookPageContent(LocalizedStrings.GetLanguageInfoByKey("PasswordWrongText"));
                    this.MainPanorama.SelectionChanged -= new System.EventHandler<SelectionChangedEventArgs>(this.MainPanorama_SelectionChanged);
                }
            };
        }

        private void MainPage_BackKeyPress(object sender, CancelEventArgs e)
        {
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            bool flag = true;
            if (!this.hasRunLoaded)
            {
                WelcomePage.ShowWelcome(this);
                if (DataSynchronizationHandler.ToBackupDataBase)
                {
                    this.SetMyAccountBookPageContent(LocalizedStrings.GetLanguageInfoByKey("SafeMode"));
                    this.AddAccountItemButton.Visibility = Visibility.Collapsed;
                    this.SummaryPanoramaItem.Visibility = Visibility.Collapsed;
                    App.GoToRestoreBackupSdfPage();
                    return;
                }
                if (AppSetting.Instance.EnablePoketLock && !App.HasLogined)
                {
                    this.InitializeSafeService();
                    this.ShowPasswordPopup();
                    flag = false;
                }
            }
            if (flag)
            {
                if (!this.hasRunLoaded)
                {
                    ViewModelLocator.ScheduleManagerViewModel.RecoveryDatas(false, new System.Action(this.UpdateSummary));
                    this.hasRunLoaded = true;
                    this.WorkingAfterLoad();
                }
                else
                {
                    this.UpdateSummary();
                }
            }
        }

        private void MainPanorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateSummary();
        }

        private void ManagerTemplates_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateTo("/Pages/AppSettingPage/PreferenceSettingPage.xaml?goto={0}", new object[] { 1 });
        }

        private void MyAccountItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.EnsureFireEvent("MyAccountBookPanoramaItem"))
            {
                string source = (sender as HyperlinkButton).Tag.ToString();
                if (!source.IsNullOrEmpty())
                {
                    this.NavigateTo(source);
                }
            }
        }

        private void NavigateToPage(string uri)
        {
            this.NavigateTo(uri);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsolatedAppSetingsHelper.LastMainPageIndexCache = this.MainPanorama.SelectedIndex;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ShowOpactityZeroTrayWhiteColor();
            if (!this.hasCheckFromQuickNewRecordToMainPage && this.GetNavigatingParameter("fromStartScreen", null).ToBoolean(false))
            {
                base.NavigationService.RemoveBackEntry();
                this.mainPageViewModel.IsSummaryListLoaded = false;
                NewOrEditAccountItemPage.hasCheckFromQuickNewRecordToMainPage = true;
                this.mainPageViewModel.HasLoadCompareInfo = false;
                this.hasCheckFromQuickNewRecordToMainPage = true;
            }
        }

        protected virtual void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PinToStart_Click(object sender, System.EventArgs e)
        {
            this.mainPageViewModel.PinToStart();
        }

        private void RemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule tag = (sender as MenuItem).Tag as TallySchedule;
            if (tag != null)
            {
                ViewModelLocator.CustomizedTallyViewModel.RemoveFromHomeScreen(tag);
            }
        }

        private void SendSummary_Click(object sender, RoutedEventArgs e)
        {
            this.mainPageViewModel.SendSummary();
        }

        private void setBackgroundImage(System.IO.Stream stream = null)
        {
            if (AppSetting.Instance.UseBackgroundImageForMainPage)
            {
                this.setImg();
            }
            else
            {
                this.MainPanorama.Background = new SolidColorBrush(App.SystemBackgroundColor.Color);
            }
        }

        public void setBackgroundImageAtStart()
        {
            if (AppSetting.Instance.UseBackgroundImageForMainPage)
            {
                this.setImg();
            }
        }

        private void setImg()
        {
            ImageBrush brush = new ImageBrush
            {
                ImageSource = ApplicationHelper.GetPictureFromApplication(AppSetting.MainPageBackgroundPictureFileName)
            };
            this.MainPanorama.Background = brush;
        }

        private void SetMyAccountBookPageContent(string content)
        {
            this.MyAccountBookPanoramaItemGrid.Children.Clear();
            TextBlock block = new TextBlock
            {
                Text = content,
                FontSize = 48.0,
                TextWrapping = TextWrapping.Wrap
            };
            this.MyAccountBookPanoramaItemGrid.Children.Add(block);
        }

        private void Share_Click(object sender, System.EventArgs e)
        {
            this.mainPageViewModel.SendSummary();
        }

        public static void ShowOpactityZeroTray(Color color)
        {
            SystemTray.IsVisible = true;
            SystemTray.Opacity = 0.0;
            SystemTray.ForegroundColor = color;
        }

        public static void ShowOpactityZeroTrayBlack()
        {
            ShowOpactityZeroTray(App.SystemForegroundColor.Color);
        }

        public static void ShowOpactityZeroTrayWhiteColor()
        {
            ShowOpactityZeroTray(App.SystemForegroundColor.Color);
        }

        private void ShowPasswordPopup()
        {
            this.safetyService.ShowPasswordInputPopup();
        }

        private void StartApp()
        {
            System.Func<String, String> func = null;
            if (!ViewModelLocator.MainPageViewModel.IsDataLoaded)
            {
                if (func == null)
                {
                    func = key => this.GetLanguageInfoByKey(key);
                }
                MainPageItemViewModel.LocalizationTitleGetter = func;
                this.mainPageViewModel.LoadData();
            }
            this.setBackgroundImageAtStart();
        }

        private void UpdateSummary()
        {
            this.currentPanoramaItemName = (this.MainPanorama.SelectedItem as PanoramaItem).Name;
            if (this.currentPanoramaItemName == "SummaryPanoramaItem")
            {
                if (!ViewModelLocator.MainPageViewModel.IsSummaryListLoaded)
                {
                    this.BusyForWork(string.Empty);

                    System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                    {
                        ViewModelLocator.MainPageViewModel.LoadSummary();
                        this.InvokeInThread(() =>
                        {
                            GlobalIndicator.Instance.ProgressIndicator.IsIndeterminate = false;
                            GlobalIndicator.Instance.ProgressIndicator.IsVisible = false;
                        });
                    });
                }
            }
            else if ((this.currentPanoramaItemName == "RecentItemList") && (AppSetting.Instance.FavoritesPageVisibiable == Visibility.Visible))
            {
                this.mainPageViewModel.UpdateFavoriteLinks(this.RecentItemList, this, this.LayoutRoot);
            }
        }

        private void WorkingAfterLoad()
        {
            System.Threading.WaitCallback callBack = null;
            if (!this.hasNavigated)
            {
                this.hasNavigated = true;
                if (callBack == null)
                {
                    callBack = delegate(object o)
                    {
                        this.InvokeInThread(delegate
                        {
                            HolidayTipsHelper.ShowHolidayWords(this);
                            this.CheckLastTimeTally();
                        });
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
        }
    }
}

