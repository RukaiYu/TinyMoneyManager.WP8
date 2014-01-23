namespace TinyMoneyManager.Pages
{
    using JasonPopupDemo;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.AccountItemViews;
    using TinyMoneyManager.Pages.CategoryManager;
    using TinyMoneyManager.ViewModels;
    using Microsoft.Unsupported;

    public partial class AccountItemListPage : PhoneApplicationPage
    {
        private AccountItemListViewModel accountItemLisViewModel;
        private ApplicationBarIconButton chooseMonthButton;
        private string currentDialogItemTypeTitle = string.Empty;

        private ExportDataOption exportDataOption = new ExportDataOption();
        public static bool fromAddOrEdit;
        private ApplicationBar functionBar;
        private bool hasLoaded;
        public bool hasRemovedOnece;
        private ApplicationBarMenuItem openCategoryPageMenuButton;
        protected bool preventListSelectionChanged;
        private string selectAllText = string.Empty;
        private string SelectInvertText = string.Empty;
        private ApplicationBarIconButton sendingSummaryIconButton;
        private string unSelectAllText = string.Empty;
        private ViewModeConfig vmc;

        public AccountItemListPage()
        {
            this.InitializeComponent();
            Microsoft.Unsupported.TiltEffect.SetIsTiltEnabled(this, true);
            base.Loaded += new RoutedEventHandler(this.MainPage_Loaded);
            AccountItemViewer.DeleteItemHandler = new System.Action<AccountItem>(AccountItemListPage.DeleteAccountItem);
            this.accountItemLisViewModel = ViewModelLocator.AccountItemViewModel;
            this.SetTitleTrigger();
            this.SetButtonText();
            this.exportDataOption.EnableChangeSearchingScope = true;
            this.exportDataOption.EnableChangeExportDataMode = true;
            this.exportDataOption.EnableChangeExportDataType = false;
            this.exportDataOption.Subject = LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport");
            this.exportDataOption.ExportDataType = SummaryDataType.ExpenseOrIncomeRecords;
        }

        private void AddNewItemButton_Click(object sender, System.EventArgs e)
        {
            int selectedIndex = this.MainPivotTitle.SelectedIndex;
            NewOrEditAccountItemPage.IsSelectionMode = false;
            this.NavigateTo("/Pages/NewOrEditAccountItemPage.xaml?isFromList=true&action=Add&categoryType=" + selectedIndex.ToString());
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            base.ApplicationBar.Opacity = e.IsMenuVisible ? 0.76 : 0.5;
        }

        private void ChooseElementForViewModeButton_Click(object sender, System.EventArgs e)
        {
            this.ShowTipsOnceDirectly("HowToSwitchScopeForHistoryList", AppResources.AboutSwitchScopeForHistoryList, AppResources.HowToSwitchScopeForHistoryList);
            this.vmc.DoCustomizedSearching = delegate(ViewModeConfig e_vmc)
            {
            };
            this.NavigateTo("/Pages/MonthSelectorPage.xaml?Type={0}", new object[] { this.GetCurrentViewModel() });
        }

        private SummarySendingMode ChooseSendingMode()
        {
            return this.exportDataOption.ExportDataMode;
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            if (sender != null)
            {
                if (menu.Tag.ToString() == "IncomeListBox")
                {
                    this.IncomeListBox.IsEnabled = false;
                }
                else
                {
                    this.ExpensesListBox.IsEnabled = false;
                }
            }
        }

        private void ContextMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            if (sender != null)
            {
                if (menu.Tag.ToString() == "IncomeListBox")
                {
                    this.IncomeListBox.IsEnabled = true;
                }
                else
                {
                    this.ExpensesListBox.IsEnabled = true;
                }
            }
        }

        private void DataSynchronizationButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/DataSynchronizationPage.xaml");
        }

        public static void DeleteAccountItem(AccountItem item)
        {
            ViewModelLocator.AccountViewModel.HandleAccountItemDeleting(item);
        }

        private void DeleteBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            if (!this.preventListSelectionChanged)
            {
                MenuItem item = sender as MenuItem;
                if ((item == null) || (item.Tag == null))
                {
                    this.Alert(AppResources.NoneSelectedMessage, null);
                }
                else
                {
                    AccountItem tag = item.Tag as AccountItem;
                    this.DeleteItemWithMessagBox(tag);
                }
            }
        }

        private void DeleteItemWithMessagBox(AccountItem item)
        {
            if (this.AlertConfirm(AppResources.DeleteAccountItemMessage, null, null) == MessageBoxResult.OK)
            {
                DeleteAccountItem(item);
            }
        }

        private void DeleteSelectedItemButton_Click(object sender, System.EventArgs e)
        {
        }

        private void DoExportData()
        {
            SummarySendingMode mode = this.ChooseSendingMode();
            this.exportDataOption.Subject = this.exportDataOption.Subject.IsNullOrEmpty() ? LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport") : this.exportDataOption.Subject;
            switch (mode)
            {
                case SummarySendingMode.BySkyDrive:
                    this.SendingReportToSkyDriveInExcel();
                    return;

                case SummarySendingMode.ByEmail:
                    {
                        string body = this.accountItemLisViewModel.BuildReports(this.exportDataOption);
                        Helper.SendEmail(this.exportDataOption.Subject, body);
                        break;
                    }
            }
        }

        private void EditBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            if (!this.preventListSelectionChanged)
            {
                MenuItem item = sender as MenuItem;
                if ((item == null) || (item.Tag == null))
                {
                    this.AlertNotification(AppResources.NoneSelectedMessage, null);
                }
                else
                {
                    this.GoToEditBudgetItem(item.Tag as AccountItem);
                }
            }
        }


        private void ExpensesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.preventListSelectionChanged && (this.ExpensesListBox.SelectedItem != null))
            {
                this.OnListBoxSelectedItemChanged(() => this.ExpensesListBox.SelectedItem as AccountItem);
                this.ExpensesListBox.SelectedItem = null;
            }
        }

        public int GetCurrentViewModel()
        {
            int selectedIndex = this.MainPivotTitle.SelectedIndex;
            if (selectedIndex == 0)
            {
                return 0;
            }
            if (selectedIndex != 1)
            {
                return 2;
            }
            return 1;
        }

        private void GoToEditBudgetItem(AccountItem itemForEdit)
        {
            NewOrEditAccountItemPage.currentEditObject = itemForEdit;
            this.NavigateTo("/Pages/NewOrEditAccountItemPage.xaml?action={0}&id={1}&isFromList=true", new object[] { PageActionType.Edit, itemForEdit.Id });
        }

        private void IncomeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.preventListSelectionChanged && (this.IncomeListBox.SelectedItem != null))
            {
                this.OnListBoxSelectedItemChanged(() => this.IncomeListBox.SelectedItem as AccountItem);
                this.IncomeListBox.SelectedItem = null;
            }
        }

        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasLoaded)
            {
                this.hasLoaded = true;
                this.IncomeListBox.DataContext = ViewModelLocator.IncomeViewModel;
                this.ExpensesListBox.DataContext = ViewModelLocator.ExpensesViewModel;
            }
        }

        private void MainPivotTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.MainPivotTitle.SelectedIndex;
            GroupViewModel expensesViewModel = ViewModelLocator.ExpensesViewModel;
            switch (selectedIndex)
            {
                case 0:
                    expensesViewModel = ViewModelLocator.ExpensesViewModel;
                    this.vmc = ViewModelLocator.ExpensesViewModel.ViewModeInfo;
                    this.openCategoryPageMenuButton.Text = LocalizedStrings.GetCombinedText(this.ExpensesPivot.Header.ToString(), this.currentDialogItemTypeTitle, false);
                    break;

                case 1:
                    expensesViewModel = ViewModelLocator.IncomeViewModel;
                    this.vmc = ViewModelLocator.IncomeViewModel.ViewModeInfo;
                    this.openCategoryPageMenuButton.Text = LocalizedStrings.GetCombinedText(this.IncomePivot.Header.ToString(), this.currentDialogItemTypeTitle, false);
                    break;
            }
            if (!expensesViewModel.IsDataLoaded)
            {
                new System.Threading.Thread(delegate(object o)
                {
                    (o as GroupViewModel).Load();
                }).Start(expensesViewModel);
            }
            this.chooseMonthButton.Text = this.vmc.GetTitle();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
        }

        private void OnListBoxSelectedItemChanged(System.Func<AccountItem> acctionGetter)
        {
            if (acctionGetter != null)
            {
                AccountItem item = acctionGetter();
                if (item != null)
                {
                    this.NavigateTo("/Pages/AccountItemViews/AccountItemViewer.xaml?id={0}", new object[] { item.Id });
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);
                if (e.NavigationMode != NavigationMode.Back)
                {
                    if (fromAddOrEdit)
                    {
                        if (NewOrEditAccountItemPage.currentEditObject != null)
                        {
                            this.MainPivotTitle.SelectedIndex = (int)NewOrEditAccountItemPage.currentEditObject.Type;
                        }
                        fromAddOrEdit = false;
                    }
                    if ((!this.hasRemovedOnece && this.GetNavigatingParameter("FromMainPlus", null).ToBoolean(false)) || this.GetNavigatingParameter("fromStartScreen", null).ToBoolean(false))
                    {
                        if (this.GetNavigatingParameter("removeTwo", null).ToBoolean(false))
                        {
                            base.NavigationService.RemoveBackEntry();
                        }
                        base.NavigationService.RemoveBackEntry();
                        this.hasRemovedOnece = true;
                        ViewModelLocator.ScheduleManagerViewModel.RecoveryDatas(false, null);
                    }
                }
            }
            catch (System.Exception)
            {
                this.NavigateTo("/MainPage.xaml");
            }
        }

        private void openCategoryPageMenuButton_Click(object sender, System.EventArgs e)
        {
            ItemType currentViewModel = (ItemType)this.GetCurrentViewModel();
            CategoryManagment.Go(this, null, currentViewModel, false);
        }



        private void Rectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if ((rectangle != null) && (rectangle.Tag != null))
            {
                object tag = rectangle.Tag;
            }
        }

        private void SendingReportToSkyDriveInExcel()
        {
            SkyDriveDataSyncingPageViewModel.BackupAndRestoreDataSyncingMode = false;
            SkyDriveDataSyncingPage.DefaultFileExtension = ".xls";
            SkyDriveDataSyncingPage.Filename = this.exportDataOption.Subject.Trim() + SkyDriveDataSyncingPage.DefaultFileExtension;
            SkyDriveDataSyncingPageViewModel.NormalDataSetter = () => this.accountItemLisViewModel.BuildReportsForExcel(this.exportDataOption);
            this.NavigateTo("/Pages/SkyDriveDataSyncingPage.xaml");
        }

        CustomMessageBox cmb;
        private void sendingSummaryIconButton_Click(object sender, System.EventArgs e)
        {
            cmb = new CustomMessageBox();

            ExportDataOptionPanel popupContent = new ExportDataOptionPanel(this.exportDataOption, () => this.vmc.YearMonthDate)
            {
                ExportDataOption =
                {
                    CustomizedDateSetter = delegate(ExportDataOption op)
                    {
                        op.StartDate = new System.DateTime(this.vmc.Year, this.vmc.Month, 1);
                        op.EndDate = new System.DateTime(this.vmc.Year, this.vmc.Month, this.vmc.ViewDateTime.GetLastDayOfMonth().Day);
                    }
                },
            };
            popupContent.ExportDataOption.ExportDataType = SummaryDataType.ExpenseOrIncomeRecords;

            cmb.LeftButtonContent = AppResources.ExportReport.ToUpper();
            cmb.IsLeftButtonEnabled = true;
            cmb.Dismissed += cmb_Dismissed;
            cmb.Content = popupContent;

            cmb.Show();
        }

        void cmb_Dismissed(object sender, DismissedEventArgs e)
        {
            if (e.Result == CustomMessageBoxResult.LeftButton)
            {
                try
                {
                    this.DoExportData();
                }
                catch (System.Exception exception)
                {
                    this.Alert(exception.Message, null);
                }
            }
        }

        private void SetButtonText()
        {
            ApplicationBarIconButton button3 = new ApplicationBarIconButton
            {
                Text = this.GetLanguageInfoByKey("Statistics"),
                IconUri = IconUirs.StaticsIconButton
            };
            ApplicationBarIconButton button = button3;
            button.Click += new System.EventHandler(this.StatisticsSelectedItemButton_Click);
            ApplicationBarIconButton button4 = new ApplicationBarIconButton
            {
                Text = AppResources.AddButtonText,
                IconUri = IconUirs.AddPlusIconButton
            };
            ApplicationBarIconButton button2 = button4;
            button2.Click += new System.EventHandler(this.AddNewItemButton_Click);
            ApplicationBarIconButton button5 = new ApplicationBarIconButton
            {
                Text = AppResources.ChooseElementForViewModeButtonText,
                IconUri = IconUirs.ChooseMonthIconButton
            };
            this.chooseMonthButton = button5;
            this.chooseMonthButton.Click += new System.EventHandler(this.ChooseElementForViewModeButton_Click);
            ApplicationBarIconButton button6 = new ApplicationBarIconButton
            {
                Text = AppResources.ExportReport,
                IconUri = IconUirs.ColoudIconButton
            };
            this.sendingSummaryIconButton = button6;
            this.sendingSummaryIconButton.Click += new System.EventHandler(this.sendingSummaryIconButton_Click);
            this.functionBar = new ApplicationBar();

            new ApplicationBarIconButton[] { 
                button, button2, this.chooseMonthButton, 
                this.sendingSummaryIconButton 
            }
                .ForEach(p =>
            {
                this.functionBar.Buttons.Add(p);
            });

            this.openCategoryPageMenuButton = new ApplicationBarMenuItem();
            this.currentDialogItemTypeTitle = AppResources.CategoryManagementPageTitle;
            this.openCategoryPageMenuButton.Text = this.currentDialogItemTypeTitle;
            this.openCategoryPageMenuButton.Click += new System.EventHandler(this.openCategoryPageMenuButton_Click);
            this.functionBar.MenuItems.Add(this.openCategoryPageMenuButton);

            var openMutipleManagingMenuItem = new ApplicationBarMenuItem(AppResources.MutipleManageing);
            openMutipleManagingMenuItem.Click += openMutipleManagingMenuItem_Click;
            openMutipleManagingMenuItem.IsEnabled = false;
            this.functionBar.MenuItems.Add(openMutipleManagingMenuItem);

            base.ApplicationBar = this.functionBar;

        }

        private void SettingButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/SettingPage.xaml");
        }

        private void SetTitleTrigger()
        {
            System.Action<ViewModeConfig> action = delegate(ViewModeConfig vmi)
            {
                if (!vmi.HasBindPropertyChangedEvent)
                {
                    vmi.PropertyChanged += new PropertyChangedEventHandler(this.vmi_PropertyChanged);
                    vmi.HasBindPropertyChangedEvent = true;
                }
            };
            action(ViewModelLocator.IncomeViewModel.ViewModeInfo);
            action(ViewModelLocator.ExpensesViewModel.ViewModeInfo);
        }

        private void StatisticsSelectedItemButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/StatisticsPage.xaml?ItemType={0}&Index={1}", new object[] { this.GetCurrentViewModel(), this.MainPivotTitle.SelectedIndex });
        }


        private void vmi_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ViewModeConfig.ViewDateTimeProperty)
            {
                this.chooseMonthButton.Text = (sender as ViewModeConfig).GetTitle();
            }
        }

        void openMutipleManagingMenuItem_Click(object sender, EventArgs e)
        {
            var currentType = (ItemType)GetCurrentViewModel();

            GroupViewModel viewModel = null;

            if (currentType == ItemType.Expense)
            {
                viewModel = accountItemLisViewModel.ExpenseItemListViewModel;
            }
            else
            {
                viewModel = accountItemLisViewModel.IncomeItemListViewModel;
            }

            var items = viewModel.GroupItems;

            this.Alert(items.Count.ToString());
        }
    }
}

