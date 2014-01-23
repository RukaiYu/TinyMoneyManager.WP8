namespace TinyMoneyManager.Pages
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Microsoft.Unsupported;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using PixelLab.Common;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class MonthSelectorPage : PhoneApplicationPage
    {
        public ItemType ItemTypeFor;

        private MessageBoxService mbs;

        private DateConfigViewModel viewModel;

        public MonthSelectorPage()
        {
            this.InitializeComponent();
            this.mbs = new MessageBoxService(this);
            this.viewModel = new DateConfigViewModel();
            base.Loaded += new RoutedEventHandler(this.MonthSelectorPage_Loaded);
            Microsoft.Unsupported.TiltEffect.SetIsTiltEnabled(this, true);
            System.Action<ApplicationBarIconButton>[] setters = new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = AppResources.Search;
            } };
            base.ApplicationBar.GetIconButtonFrom(0).SetPropertyValue(setters);
        }

        private void AdventureCondition_Click(object sender, System.EventArgs e)
        {
            this.GetViewModel().ViewModeInfo.Searching();
            this.SafeGoBack();
        }

        private void DeleteBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            System.Action isOkToDo = null;
            int item = (sender as MenuItem).Tag.ToString().ToInt32();
            ViewModeConfig viewModeInfo = this.GetViewModel().ViewModeInfo;
            bool hasValue = this.SelectedYear.HasValue;
            bool revertAmountToAccount = false;
            bool deleteFromFullYear = false;
            System.EventHandler onClosed = null;
            onClosed = delegate(object o1, System.EventArgs e1)
            {
                this.BusyForWork(AppResources.Delete + "...");
                this.mbs.Closed -= onClosed;
                if (this.mbs.Result == MessageBoxResult.Yes)
                {
                    revertAmountToAccount = true;
                }
                System.Collections.Generic.IEnumerable<AccountItem> oneMonthData = null;
                if (deleteFromFullYear)
                {
                    oneMonthData = ViewModelLocator.AccountItemViewModel.GetOneYearData(item);
                }
                else
                {
                    oneMonthData = ViewModelLocator.AccountItemViewModel.GetOneMonthData(this.SelectedYear.GetValueOrDefault(), item);
                }
                System.Action<AccountItem> action = new System.Action<AccountItem>(ViewModelLocator.AccountViewModel.HandleAccountItemDeleting);
                if (!revertAmountToAccount)
                {
                    action = new System.Action<AccountItem>(ViewModelLocator.AccountItemViewModel.Delete);
                }
                foreach (AccountItem item1 in oneMonthData)
                {
                    action(item1);
                }
                if (!revertAmountToAccount)
                {
                    ViewModelLocator.AccountItemViewModel.AccountBookDataContext.SubmitChanges();
                }
                ViewModelLocator.MainPageViewModel.IsSummaryListLoaded = false;
                this.WorkDone();
                this.AlertNotification(AppResources.OperationSuccessfullyMessage, null);
            };
            this.mbs.Closed += onClosed;
            if (item > 12)
            {
                deleteFromFullYear = true;
                if (this.AlertConfirm(AppResources.DeletingDataInOneYear.FormatWith(new object[] { item }), delegate
                {
                }, null) == MessageBoxResult.OK)
                {
                    this.mbs.Show(AppResources.ConfirmRevertAmountToAccount, AppResources.ConfirmDeletingAccountItems, MessageBoxServiceButton.YesNo, null);
                }
            }
            else
            {
                if (isOkToDo == null)
                {
                    isOkToDo = delegate
                    {
                        this.mbs.Show(AppResources.ConfirmRevertAmountToAccount, AppResources.ConfirmDeletingAccountItems, MessageBoxServiceButton.YesNo, null);
                    };
                }
                this.AlertConfirm(AppResources.DeletingDataInOneYear.FormatWith(new object[] { this.SelectedYear, item }), isOkToDo, null);
            }
        }

        private void FirstCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((this.FirstCategoryItems != null) && (this.FirstCategoryItems.SelectedItem != null))
            {
                object selectedItem = this.FirstCategoryItems.SelectedItem;
                this.TurnToCurrentDate(selectedItem.ToString().ToInt32());
                this.FirstCategoryItems.SelectedItem = null;
            }
        }

        public GroupViewModel GetViewModel()
        {
            if (this.ItemTypeFor == ItemType.Income)
            {
                return ViewModelLocator.IncomeViewModel;
            }
            return ViewModelLocator.ExpensesViewModel;
        }

        private void InitializeSearchingScope()
        {
            string[] strArray = new string[] { AppResources.CurrentWeek, AppResources.CurrentMonth, AppResources.CurrentYear, AppResources.Customize };
            this.SearchDuringDate.ItemsSource = strArray;
            this.SearchDuringDate.SelectionChanged += new SelectionChangedEventHandler(this.SearchDuringDate_SelectionChanged);
            this.SearchDuringDate.SelectedIndex = this.viewModel.SearchingConfig.CustomizedSearchingIndex;
        }

        private void MonthSelectorPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.SelectorPagesPivot.SelectionChanged += new SelectionChangedEventHandler(this.SelectorPagesPivot_SelectionChanged);
                switch (base.NavigationContext.QueryString["Type"].ToInt32())
                {
                    case 0:
                        this.ItemTypeFor = ItemType.Expense;
                        this.viewModel.SearchingConfig = ViewModelLocator.ExpensesViewModel.ViewModeInfo;
                        break;

                    case 1:
                        this.ItemTypeFor = ItemType.Income;
                        this.viewModel.SearchingConfig = ViewModelLocator.IncomeViewModel.ViewModeInfo;
                        break;
                }
                this.CurrentDate = this.viewModel.SearchingConfig.ViewDateTime;
                this.InitializeSearchingScope();
                this.FirstCategoryItems.ItemsSource = this.viewModel.LoadYears(this.ItemTypeFor);
                if (this.viewModel.SearchingConfig.IsCustomized)
                {
                    this.SelectedYear = new int?(this.CurrentDate.Year);
                    this.SelectorPagesPivot.SelectedIndex = 2;
                }
                else
                {
                    this.TurnToCurrentDate(this.CurrentDate.Year);
                }
                base.DataContext = this;
            }
        }

        private void SearchDuringDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.EndDateRow.Visibility = Visibility.Collapsed;
            this.StartDateRow.Visibility = Visibility.Collapsed;
            int selectedIndex = this.SearchDuringDate.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    this.SearchingCondition.SearchingScope = SearchingScope.CurrentWeek;
                    break;

                case 1:
                    this.SearchingCondition.SearchingScope = SearchingScope.CurrentMonth;
                    break;

                case 2:
                    this.SearchingCondition.SearchingScope = SearchingScope.CurrentYear;
                    break;

                case 3:
                    this.SearchingCondition.SearchingScope = SearchingScope.Customize;
                    this.EndDateRow.Visibility = Visibility.Visible;
                    this.StartDateRow.Visibility = Visibility.Visible;
                    this.StartDateSelector.Value = new System.DateTime?(System.DateTime.Now.AddMonths(-1));
                    this.EndDateSelector.Value = new System.DateTime?(System.DateTime.Now);
                    break;

                case 4:
                    this.SearchingCondition.SearchingScope = SearchingScope.All;
                    break;
            }
            this.viewModel.SearchingConfig.CustomizedSearchingIndex = selectedIndex;
        }

        private void SecondCategoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((this.SecondCategoryItems != null) && (this.SecondCategoryItems.SelectedItem != null))
            {
                this.SelectedMonth = new int?(this.SecondCategoryItems.SelectedItem.ToString().ToInt32());
                ViewModeConfig viewModeInfo = this.GetViewModel().ViewModeInfo;
                viewModeInfo.CustomizedSearchingIndex = -1;
                if (this.SelectedYear.HasValue && this.SelectedMonth.HasValue)
                {
                    viewModeInfo.RaiseChange(new int?(this.SelectedYear.Value), new int?(this.SelectedMonth.Value));
                    base.NavigationService.GoBack();
                }
                this.SecondCategoryItems.SelectedItem = null;
            }
        }

        private void SelectorPagesPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            base.ApplicationBar.IsVisible = this.SelectorPagesPivot.SelectedIndex == 2;
        }

        private void TurnToCurrentDate(int year)
        {
            this.SecondPivot.Visibility = Visibility.Visible;
            this.SelectorPagesPivot.SelectedIndex = 1;
            this.FirstPivot.Visibility = Visibility.Collapsed;
            this.SelectedYear = new int?(year);
            this.SecondPivot.Header = year;
            this.SecondCategoryItems.ItemsSource = this.viewModel.LoadMonthsByYear(year);
        }

        public System.DateTime CurrentDate { get; set; }

        public DetailsCondition SearchingCondition
        {
            get
            {
                return this.viewModel.SearchingConfig.SearchingCondition;
            }
        }

        public int? SelectedMonth { get; set; }

        public int? SelectedYear { get; set; }
    }
}

