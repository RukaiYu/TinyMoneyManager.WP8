namespace TinyMoneyManager.Pages.Summary
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System.Linq;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.Data.Model;
    using System.Collections.Generic;
    using TinyMoneyManager.ViewModels.Common;

    public partial class ParticularsDetails : PhoneApplicationPage
    {
        private MenuItemViewModel mainPageSummaryViewModel;

        private ParticularsViewModel particularsViewModel;
        private bool _hasDataLoaded;
        private bool hasLoadMainSummary;
        private bool hasLoadCategoriesSummary;

        private bool hasCategoriesSummaryLoaded = false;

        public List<CategorySummaryGroup> CategorySummary
        {
            get { return (List<CategorySummaryGroup>)GetValue(categorySummaryProperty); }
            set { SetValue(categorySummaryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for categorySummary.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty categorySummaryProperty =
            DependencyProperty.Register("CategorySummary", typeof(List<CategorySummaryGroup>), typeof(ParticularsDetails), null);

        /// <summary>
        /// Gets or sets the net assets summary items.
        /// </summary>
        /// <value>
        /// The net assets summary items.
        /// </value>
        public List<SummaryDetails> NetAssetsSummaryItems
        {
            get { return (List<SummaryDetails>)GetValue(NetAssetsSummaryItemsProperty); }
            set { SetValue(NetAssetsSummaryItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NetAssetsSummaryItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NetAssetsSummaryItemsProperty =
            DependencyProperty.Register("NetAssetsSummaryItems", typeof(List<SummaryDetails>), typeof(ParticularsDetails), null);

        private bool hasLoadNetAssetSummary;

        public string TotalNetAssetMoneyInfo
        {
            get { return (string)GetValue(TotalNetAssetMoneyInfoProperty); }
            set { SetValue(TotalNetAssetMoneyInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalNetAssetMoneyInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalNetAssetMoneyInfoProperty =
            DependencyProperty.Register("TotalNetAssetMoneyInfo", typeof(string), typeof(ParticularsDetails), new PropertyMetadata("计算中..."));


        public ParticularsDetails()
        {
            this.InitializeComponent();
            this.particularsViewModel = ViewModelLocator.instanceLoader.LoadSingelton<ParticularsViewModel>("ParticularsViewModel");
            this.DataContext = this.particularsViewModel;
            this.InitailizeApplicationBar();
            this.mainPageSummaryViewModel = ViewModelLocator.MainPageViewModel;
            this.mainPageSummaryViewModel.ThisMonthSummary.IncomeSummaryEntry.ComparationInfo.Amount = -1M;
            this.mainPageSummaryViewModel.ThisMonthSummary.ExpenseSummaryEntry.ComparationInfo.Amount = -1M;

            this.NetAssetsSummary.DataContext = this;

            TiltEffect.SetIsTiltEnabled(this, true);
            this.Loaded += ParticularsDetails_Loaded;

            this.CategorySummaryPivot.DataContext = this;
        }

        void ParticularsDetails_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasDataLoaded)
            {
                this._hasDataLoaded = true;

                this.LoadSummary();
                this.MainPivot.SelectionChanged += MainPivot_SelectionChanged;
            }
        }

        private void InitailizeApplicationBar()
        {
        }

        private void LoadSummary()
        {
            if (!this.hasLoadMainSummary)
            {
                hasLoadMainSummary = true;
                this.MainPivot.IsLocked = true;
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { this.MainSummary.Header }));
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    this.particularsViewModel.LoadData();
                    this.mainPageSummaryViewModel.UpdatingCompareingAccountInfo(true);
                    base.Dispatcher.BeginInvoke(delegate
                    {
                        this.particularsViewModel.MonthlyIncomExpenseChangesAmountInfo = "{0}/{1}".FormatWith(new object[] { this.mainPageSummaryViewModel.ThisMonthSummary.IncomeSummaryEntry.ComparationInfo.AmountInfoWithArrow, this.mainPageSummaryViewModel.ThisMonthSummary.ExpenseSummaryEntry.ComparationInfo.AmountInfoWithArrow });
                        this.MainPivot.IsLocked = false;

                        this.TotalInDebtButton.IsEnabled = true;
                        this.TotalLoanOutButton.IsEnabled = true;
                        this.TotalTransactionButton.IsEnabled = true;

                        this.WorkDone();
                    });
                });
            }
        }

        private void LoadCategoriesSummary(bool showAllCategories = false)
        {
            if (!this.hasLoadCategoriesSummary)
            {
                this.hasLoadCategoriesSummary = true;
                this.hasCategoriesSummaryLoaded = false;
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(this.CategorySummaryPivot.Header.ToString()));
                this.CategoriesList.IsEnabled = false;
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    var hasRows = false;
                    var categorySummaryItems = ViewModelLocator.CategoryViewModel.LoadCategoriesSummary(ItemType.All, ref  hasRows, showAllCategories);

                    this.Dispatcher.BeginInvoke(delegate
                    {
                        CategorySummary = categorySummaryItems;
                        this.MainPivot.IsLocked = false;

                        if (showAllCategories)
                        {
                            HeaderControlPanelInCategorySummaryPivot.Visibility = System.Windows.Visibility.Collapsed;
                        }

                        this.CategoriesList.IsEnabled = true;
                        this.CategoriesPivtorInfoHeader.Text = hasRows ? AppResources.CategoriesFollowedTitle_SubTitleWithRows
                            : AppResources.CategoriesFollowedTitle_SubTitleWithoutRows;
                        hasCategoriesSummaryLoaded = true;
                        this.WorkDone();
                    });
                });
            }
        }

        private void LoadNetAssetSummary()
        {
            if (!this.hasLoadNetAssetSummary)
            {
                var canToLoadData = true;

                if (hasLoadCategoriesSummary)
                {
                    canToLoadData = hasCategoriesSummaryLoaded;
                }

                if (canToLoadData)
                {
                    this.hasLoadNetAssetSummary = true;

                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        var items = new List<ObjectGroupingViewModel<string, NetSummaryDetails>>();

                        var group = new ObjectGroupingViewModel<string, NetSummaryDetails>("1");

                        group.Add(new NetSummaryDetails() { Name = "总账户余额", AccountItemType = ItemType.Income, TotalAmout = 34.00m });
                        group.Add(new NetSummaryDetails() { Name = AppResources.TotalLoanOut.ToLower(), AccountItemType = ItemType.Income, TotalAmout = this.particularsViewModel.TotalLoanOut.TotalExpenseAmount });
                        group.Add(new NetSummaryDetails() { Name = AppResources.TotalInDebt.ToLower(), AccountItemType = ItemType.Expense, TotalAmout = this.particularsViewModel.TotalInDebt.TotalExpenseAmount });
                        group.Add(new NetSummaryDetails() { Name = "信用卡还款", AccountItemType = ItemType.Expense, TotalAmout = 4545.00m });

                        items.Add(group);
                        Dispatcher.BeginInvoke(() =>
                        {
                            //TotalNetAssetMoneyInfo = group.TotalAmount;
                            TotalNetAssetTextBock.Text = group.TotalAmount;
                            this.NetAssetsSummary.ItemsSource = items;
                            this.hasLoadNetAssetSummary = true;
                            this.WorkDone();
                        });
                    });
                }
            }
        }

        void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var indexOfItem = MainPivot.SelectedIndex;

            if (indexOfItem != -1)
            {
                if (indexOfItem == 0)
                {
                    LoadSummary();
                }
                else if (indexOfItem == 1)
                {
                    LoadCategoriesSummary();
                }
                else if (indexOfItem == 2)
                {
                    TotalNetAssetTextBock.Text = AppResources.TestingMessage;
                    // LoadNetAssetSummary();
                }
            }
        }

        private void CategoriesList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var category = CategoriesList.SelectedItem as Category;

            if (category != null)
            {
                CategoriesList.SelectedItem = null;
                CategoryManager.CategoryInfoViewer.Go(category.Id, this);
            }
        }

        private void ShowAllCategories_Click_1(object sender, RoutedEventArgs e)
        {
            hasLoadCategoriesSummary = false;
            LoadCategoriesSummary(true);
        }
    }
}

