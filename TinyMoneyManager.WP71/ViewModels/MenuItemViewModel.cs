namespace TinyMoneyManager
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ScheduledAgentLib;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.ViewModels.CustomizedTallyManager;

    public class MenuItemViewModel : NkjSoftViewModelBase
    {
        private bool _needToShowYearInfo;
        public string amountInfoOfToday = string.Empty;
        public int countOfToday;
        public bool FromAgent;
        private static Grid gridBackground = null;
        private bool hasLoadCompareInfo;
        private bool hasLoadRencentItems;
        private bool hasUpdatingRecents;
        public ImageSource img;
        private bool isForAgent;
        private bool isStartLoadingSummary;
        private bool isSummaryListLoaded;
        private Thickness margin12120000 = new Thickness(12.0, 12.0, 0.0, 0.0);
        private static Uri uriUsedToPinToStartScreen;
        private bool hasLoadedFromFile;


        public override TinyMoneyDataContext AccountBookDataContext
        {
            get;
            set;
        }

        public MenuItemViewModel()
        {
            AccountBookDataContext = new TinyMoneyDataContext();
            this.Items = new ObservableCollection<MainPageItemViewModel>();
            this.SummaryItemList = new ObservableCollection<AnalyziationSummarization>();
            this.InitializeSummaryEntry();
            AppSetting.Instance.PropertyChanged += new PropertyChangedEventHandler(this.Instance_PropertyChanged);
        }

        private void appendLineToBuilder(System.Text.StringBuilder builder, string lineFormatter, AnalyziationSummarization summaryEntry)
        {
            AnalyziationSummaryEntry incomeSummaryEntry = summaryEntry.IncomeSummaryEntry;
            AnalyziationSummaryEntry expenseSummaryEntry = summaryEntry.ExpenseSummaryEntry;
            builder.AppendFormat(lineFormatter, new object[] { summaryEntry.Title, summaryEntry.TotalIncomeAmountInfo, summaryEntry.Spliter, summaryEntry.TotalExpenseAmountInfo });
            if (summaryEntry.ShowCompareInfo)
            {
                builder.AppendFormat("{0}{1}\t\t\t({2}{3}/{4}{5})", new object[] { LocalizedStrings.GetLanguageInfoByKey("CompareTo"), LocalizedStrings.GetLanguageInfoByKey(summaryEntry.ScopeForSummary.ToString()), incomeSummaryEntry.ComparationInfo.BalanceMovingSymbol, incomeSummaryEntry.ComparationInfo.AmountInfo, expenseSummaryEntry.ComparationInfo.BalanceMovingSymbol, expenseSummaryEntry.ComparationInfo.AmountInfo }).AppendLine().AppendLine();
            }
        }

        public void BackupTileDataForAgent()
        {
            if (this.HasPinedToStart)
            {
                StandardTileData data = this.CreateShellTileData();
                TinyMoneyManager.Component.ShellTileHelper.GetAppTile(UriUsedToPinToStartScreen).Update(data);
            }
        }

        private string buildSummaryTextForMail()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}\t\t\t{1}\r\n", new object[] { LocalizedStrings.GetLanguageInfoByKey("Scope"), LocalizedStrings.GetLanguageInfoByKey("{0}/{1}", new string[] { "Income", "Expense" }) }).AppendLine("----------------------------------------------------------").AppendLine();
            this.appendLineToBuilder(builder, "{0}\r\n==================================\r\n\t\t\t{1}{2}{3}\r\n", this.TodaySummary);
            this.appendLineToBuilder(builder, "{0}\r\n==================================\r\n\t\t\t{1}{2}{3}\r\n", this.ThisWeekSummary);
            this.appendLineToBuilder(builder, "{0}\r\n==================================\r\n\t\t\t{1}{2}{3}\r\n", this.ThisMonthSummary);
            if (this._needToShowYearInfo)
            {
                this.appendLineToBuilder(builder, "{0}\r\n==================================\r\n\t\t\t{1}{2}{3}\r\n", this.ThisYearSummary);
            }
            this.appendLineToBuilder(builder, "{0}\r\n==================================\r\n\t\t\t{1}{2}{3}\r\n", this.AccountItemRecordsSummary);
            builder.AppendFormat("{0}\r\n==================================\r\n\t\t\t{1}{2}{3}\r\n", new object[] { this.AccountInfoSummary.Title, this.AccountInfoSummary.MoneyInfo.MoneyInfo, string.Empty, string.Empty });
            return builder.ToString();
        }

        public void CalculateMonthlyExpense(System.Collections.Generic.IEnumerable<AccountItem> data)
        {
            this.ThisMonthSummary.TotalExpenseAmount = this.CountThisMonth(data, ItemType.Expense);
            this.ThisMonthSummary.TotalIncomeAmount = this.CountThisMonth(data, ItemType.Income);
        }

        public decimal CountBetweenDate(System.Collections.Generic.IEnumerable<AccountItem> source, ItemType type, System.DateTime start, System.DateTime end)
        {
            return (from p in source
                    where ((p.CreateTime.Date >= start.Date) && (p.CreateTime.Date <= end.Date)) && (p.Type == type)
                    select p).Sum<AccountItem>(((System.Func<AccountItem, decimal>)(p => p.GetMoney().GetValueOrDefault())));
        }

        private string CountRecords(ItemType itemType)
        {
            return this.AccountBookDataContext.AccountItems.Count<AccountItem>(p => (((int)p.Type) == ((int)itemType))).ToString();
        }

        private decimal CountSum(System.Collections.Generic.IEnumerable<AccountItem> source, System.Func<AccountItem, bool> sumSelector)
        {
            return source.Where(sumSelector).Sum(p => p.GetMoney().GetValueOrDefault());
        }

        public decimal CountThisMonth(System.Collections.Generic.IEnumerable<AccountItem> source, ItemType type)
        {
            return this.CountSum(source, p => p.Type == type);

            //return this.CountSum(source, p => ((p.CreateTime.Date.Year == System.DateTime.Now.Year)
            //  && (p.CreateTime.Date.Month == System.DateTime.Now.Month)) && (p.Type == type));
        }

        public decimal CountThisWeek(System.Collections.Generic.IEnumerable<AccountItem> source, ItemType type, System.DateTime start, System.DateTime end)
        {
            return this.CountSum(source, p => ((p.CreateTime.Date >= start.Date) && (p.CreateTime.Date <= end.Date)) && (p.Type == type));
        }

        public decimal CountToday(System.Collections.Generic.IEnumerable<AccountItem> source, ItemType type)
        {
            return this.CountSum(source, p => (p.CreateTime.Date == System.DateTime.Now.Date) && (p.Type == type));
        }

        public string CreateBackground(Grid grid)
        {
            if (grid != null)
            {
                TextBlock block = grid.Children[0] as TextBlock;
                string[] strArray = LocalizedStrings.GetLanguageInfoByKey("FormatterForExpenseInShellTile").Split(new char[] { ',' });
                block.Text = System.DateTime.Today.ToString(strArray[0]) + strArray[1];
                TextBlock block2 = grid.Children[1] as TextBlock;
                block2.Text = this.TodaySummary.TotalExpenseAmountInfo;
                TextBlock block3 = grid.Children[2] as TextBlock;
                block3.Text = LocalizedStrings.GetCombinedText(AppResources.CurrentWeek, AppResources.Expense, false).ToLowerInvariant();
                (grid.Children[3] as TextBlock).Text = this.ThisWeekSummary.TotalExpenseAmountInfo;
                grid.Arrange(new Rect(0.0, 0.0, 173.0, 173.0));
                TileInfoUpdatingAgent.SavePicture(grid, "Shared/ShellContent/tiles", "Shared/ShellContent/tiles/LiveTile.png");
            }

            return "isostore:/Shared/ShellContent/tiles/LiveTile.png";
        }

        private string CreateEmptyBackground(Grid grid)
        {
            TextBlock block = grid.Children[1] as TextBlock;
            block.Text = "For more?";
            block.FontSize = 23.0;
            (grid.Children[0] as TextBlock).Text = string.Empty;
            (grid.Children[2] as TextBlock).Text = "Turn on Live Tile!";
            (grid.Children[3] as TextBlock).Text = string.Empty;
            grid.Arrange(new Rect(0.0, 0.0, 173.0, 173.0));
            TileInfoUpdatingAgent.SavePicture(grid, "Shared/ShellContent/tiles", "Shared/ShellContent/tiles/LiveTile.png");
            return "isostore:/Shared/ShellContent/tiles/LiveTile.png";
        }

        public Grid CreateGridForAgent()
        {
            Thickness tkness;
            SolidColorBrush fontColor;
            if (gridBackground == null)
            {
                gridBackground = new Grid();
                gridBackground.Width = 173.0;
                gridBackground.Height = 173.0;
                gridBackground.Background = new SolidColorBrush(Colors.Transparent);
                System.Collections.Generic.IEnumerable<int> collection = Enumerable.Range(0, 5);
                tkness = new Thickness(2.0, 0.0, 0.0, 0.0);
                fontColor = new SolidColorBrush(Colors.White);
                collection.ForEach<int>(delegate(int rowIndex)
                {
                    gridBackground.RowDefinitions.Add(new RowDefinition());
                    TextBlock block2 = new TextBlock
                    {
                        FontSize = 20.0,
                        Margin = tkness,
                        Foreground = fontColor,
                        TextWrapping = TextWrapping.NoWrap
                    };
                    TextBlock element = block2;
                    Grid.SetRow(element, rowIndex);
                    gridBackground.Children.Add(element);
                });
            }
            return gridBackground;
        }

        public StandardTileData CreateShellTileData()
        {
            bool showRepaymentInfoOnTile = AppSetting.Instance.ShowRepaymentInfoOnTile;
            StandardTileData data2 = new StandardTileData
            {
                Title = AppSetting.Instance.AppName,
                BackgroundImage = new Uri("Background.png", UriKind.Relative)
            };
            StandardTileData data = data2;
            string gridContentBackground = this.GetGridContentBackground(showRepaymentInfoOnTile);
            if (showRepaymentInfoOnTile)
            {
                int repaymentRecords = this.GetRepaymentRecords(7);
                data.Count = new int?(repaymentRecords);
                data.BackBackgroundImage = new Uri(gridContentBackground);
                data.BackTitle = (repaymentRecords > 0) ? LocalizedStrings.GetLanguageInfoByKey("RecentRepaymentNumbersFormatter").FormatWith(new object[] { repaymentRecords }) : string.Empty;
                return data;
            }
            data.BackContent = string.Empty;
            data.BackTitle = string.Empty;
            data.BackBackgroundImage = null;
            //data.BackBackgroundImage = new Uri(gridContentBackground);
            return data;
        }

        public Grid GetGrid()
        {
            Grid grid = Application.Current.Resources["TileTemplete"] as Grid;
            if (grid == null)
            {
                grid = this.CreateGridForAgent();
            }
            return grid;
        }

        private string GetGridContentBackground(bool showTile)
        {
            Grid grid = this.GetGrid();
            if (showTile)
            {
                return this.CreateBackground(grid);
            }

            TileInfoUpdatingAgent.RemoveTitlePicture("Shared/ShellContent/tiles/LiveTile.png");

            return string.Empty;
            return this.CreateEmptyBackground(grid);
        }

        public int GetRepaymentRecords(int nearDaysCount)
        {
            return ViewModelLocator.RepaymentManagerViewModel.GetOnGoingRepaymentItems(nearDaysCount);
        }

        private void InitializeSummaryEntry()
        {
            AnalyziationSummarization summarization = new AnalyziationSummarization("/")
            {
                Title = AppResources.Today,
                ShowCompareInfo = false,
                TotalExpenseAmount = 0M,
                TotalIncomeAmount = 0M,
                IncomeAmountInfoVisibility = false
            };
            this.TodaySummary = summarization;
            AnalyziationSummarization summarization2 = new AnalyziationSummarization("/")
            {
                Title = AppResources.CurrentWeek,
                IncomeAmountInfoVisibility = false,
                ShowCompareInfo = true,
                TotalExpenseAmount = 0M,
                TotalIncomeAmount = 0M
            };
            this.ThisWeekSummary = summarization2;
            this.ThisWeekSummary.ScopeForSummary = SearchingScope.LastWeek;
            AnalyziationSummarization summarization3 = new AnalyziationSummarization("/")
            {
                Title = AppResources.CurrentMonth,
                ShowCompareInfo = true,
                TotalExpenseAmount = 0M,
                TotalIncomeAmount = 0M
            };
            this.ThisMonthSummary = summarization3;
            this.ThisMonthSummary.ScopeForSummary = SearchingScope.LastMonth;
            AnalyziationSummarization summarization4 = new AnalyziationSummarization("/")
            {
                Title = AppResources.CurrentYear,
                ShowCompareInfo = true,
                TotalExpenseAmount = 0M,
                TotalIncomeAmount = 0M
            };
            this.ThisYearSummary = summarization4;
            this.ThisYearSummary.ScopeForSummary = SearchingScope.LastYear;
            AnalyziationSummarization summarization5 = new AnalyziationSummarization("/")
            {
                Title = AppResources.AllAvaliableAssets
            };
            AccountItemMoney money = new AccountItemMoney
            {
                CurrencySymbolGetter = (cu, mo) => cu.GetGloableCurrencySymbol(new decimal?(mo))
            };
            summarization5.MoneyInfo = money;
            this.AccountInfoSummary = summarization5;
            AnalyziationSummarization summarization6 = new AnalyziationSummarization("/")
            {
                Title = AppResources.RecordsAmount
            };
            this.AccountItemRecordsSummary = summarization6;

            //
            this._needToShowYearInfo = false;// System.DateTime.Now.Date.Month == 12;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                this._needToShowYearInfo = true;
            }
            this.SummaryItemList.Add(this.TodaySummary);
            this.SummaryItemList.Add(this.ThisWeekSummary);
            this.SummaryItemList.Add(this.ThisMonthSummary);
            if (this._needToShowYearInfo)
            {
                this.SummaryItemList.Add(this.ThisYearSummary);
            }
            this.AccountMonthBudget = BudgetManager.Current.CurrentMonthBudgetSummary;
            this.AccountMonthBudget.Title = AppResources.MonthlyBudget;
            this.FavouritesItems = new ObservableCollection<AccountItem>();
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowCashAmountOnAsset")
            {
                this.SetCashAmountOnAsset();
            }
        }

        public void LoadCompareAmountInfo()
        {
            System.DateTime firstDayOfMonth = System.DateTime.Now.GetFirstDayOfMonth();
            System.DateTime end = firstDayOfMonth.AddDays(-1.0);
            firstDayOfMonth = firstDayOfMonth.AddMonths(-1);
            Table<AccountItem> accountItems = ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems;
            this.ThisMonthSummary.UpdatingComparingInfo(this.CountBetweenDate(accountItems, ItemType.Expense, firstDayOfMonth, end), this.CountBetweenDate(accountItems, ItemType.Income, firstDayOfMonth, end));
            if (this._needToShowYearInfo)
            {
                firstDayOfMonth = new System.DateTime(System.DateTime.Now.Year, 1, 1);
                end = firstDayOfMonth.AddDays(-1.0);
                firstDayOfMonth = firstDayOfMonth.AddYears(-1);
                this.ThisYearSummary.UpdatingComparingInfo(this.CountBetweenDate(accountItems, ItemType.Expense, firstDayOfMonth, end), this.CountBetweenDate(accountItems, ItemType.Income, firstDayOfMonth, end));
            }
            firstDayOfMonth = System.DateTime.Now.GetDateTimeOfFisrtDayOfWeek();
            end = firstDayOfMonth;
            firstDayOfMonth = firstDayOfMonth.AddDays(-6.0);
            this.ThisWeekSummary.UpdatingComparingInfo(this.CountBetweenDate(accountItems, ItemType.Expense, firstDayOfMonth, end), this.CountBetweenDate(accountItems, ItemType.Income, firstDayOfMonth, end));
        }

        public override void LoadData()
        {
            this.Items.Clear();
            MainPageItemViewModel item = new MainPageItemViewModel("EandIandCategory")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/EandI_White.png",
                PageUri = "/Pages/AccountItemListPage.xaml"
            };
            this.Items.Add(item);
            MainPageItemViewModel model2 = new MainPageItemViewModel("AccountAndTransfering")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/AccountManager.png",
                PageUri = "/Pages/AccountManager.xaml"
            };
            this.Items.Add(model2);
            MainPageItemViewModel model3 = new MainPageItemViewModel("DataSynchronizationButton")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/Sync_White.png",
                PageUri = ViewPath.DataSyncingPage
            };
            this.Items.Add(model3);
            MainPageItemViewModel model4 = new MainPageItemViewModel("StatisticsAndSearching")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/StatisticIcon_Whitebig.png",
                PageUri = "/Pages/StatisticsPage.xaml?ItemType=2&Index=0"
            };
            this.Items.Add(model4);
            MainPageItemViewModel model5 = new MainPageItemViewModel("NotificationAndSchedule")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/PriodItem_Whitebig.png",
                PageUri = "/Pages/RepaymentManager.xaml"
            };
            this.Items.Add(model5);
            MainPageItemViewModel model6 = new MainPageItemViewModel("BorrowAndLendAndPeople")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/BorrowLeanPeople.png",
                PageUri = "/Pages/BorrowLeanManager.xaml"
            };
            this.Items.Add(model6);
            MainPageItemViewModel model7 = new MainPageItemViewModel("SettingPageTitle")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/Settings.png",
                PageUri = "/Pages/SettingPage.xaml"
            };
            this.Items.Add(model7);
            MainPageItemViewModel model8 = new MainPageItemViewModel("HelpAndAbout")
            {
                TileImagePath = "/TinyMoneyManager;component/Icons/ListItemIcons/Help.png",
                PageUri = "/Pages/AboutPage.xaml"
            };
            this.Items.Add(model8);
            base.IsDataLoaded = true;
        }

        public void LoadSummary()
        {
            this.isStartLoadingSummary = true;

            if (hasLoadedFromFile)
            {
                LoadSummaryFromFile();
                return;
            }
            AccountBookDataContext.Dispose();

            AccountBookDataContext = new TinyMoneyDataContext();
            {
                System.Collections.Generic.IEnumerable<AccountItem> data = null;
                this.countOfToday = this.AccountBookDataContext.AccountItems.Count<AccountItem>(p => p.CreateTime.Date == System.DateTime.Now.Date);

                var hasCustomized = AppSetting.Instance.BudgetStatsicSettings.Calculate();
                if (hasCustomized)
                {
                    var startDate = AppSetting.Instance.BudgetStatsicSettings.StartDate;
                    var endDate = AppSetting.Instance.BudgetStatsicSettings.EndDate;

                    data = ViewModelLocator.AccountItemViewModel.GetOneBudgetMonthData(startDate, endDate);
                }
                else
                {
                    data = ViewModelLocator.AccountItemViewModel.GetOneMonthData(DateTime.Now);
                }

                this.CalculateMonthlyExpense(data);

                if (this._needToShowYearInfo)
                {
                    data = ViewModelLocator.AccountItemViewModel.GetOneYearData(System.DateTime.Now.Year);
                    this.ThisYearSummary.TotalExpenseAmount = this.CountSum(data, p => p.Type == ItemType.Expense);
                    this.ThisYearSummary.TotalIncomeAmount = this.CountSum(data, p => p.Type == ItemType.Income);
                }

                this.TodaySummary.TotalExpenseAmount = this.CountToday(data, ItemType.Expense);
                this.TodaySummary.TotalIncomeAmount = this.CountToday(data, ItemType.Income);
                this.amountInfoOfToday = this.TodaySummary.TotalExpenseAmountInfo;
                System.DateTime date = System.DateTime.Now.Date;
                System.DateTime end = date;
                date = date.GetDateTimeOfFisrtDayOfWeek();
                end = date.AddDays(7.0).Date;
                this.ThisWeekSummary.TotalExpenseAmount = this.CountThisWeek(data, ItemType.Expense, date, end);
                this.ThisWeekSummary.TotalIncomeAmount = this.CountThisWeek(data, ItemType.Income, date, end);
                if (this.isForAgent)
                {
                    this.IsSummaryListLoaded = true;
                    this.isForAgent = false;
                }
                else
                {
                    this.AccountInfoSummary.MoneyInfo.Currency = AppSetting.Instance.DefaultCurrency;
                    this.AccountInfoSummary.MoneyInfo.Money = this.AccountBookDataContext.Accounts.ToList<Account>().Sum<Account>(p => p.GetMoney()).GetValueOrDefault();
                    this.SetCashAmountOnAsset();
                    this.AccountItemRecordsSummary.TotalExpenseAmountInfo = this.CountRecords(ItemType.Expense);
                    this.AccountItemRecordsSummary.TotalIncomeAmountInfo = this.CountRecords(ItemType.Income);
                    BudgetManager.Current.UpdateCurrentMonthBudgetSummary(this.ThisMonthSummary.TotalExpenseAmount);
                    this.IsSummaryListLoaded = true;
                }
                this.isStartLoadingSummary = false;
            }
        }

        private void LoadSummaryFromFile()
        {
            hasLoadedFromFile = true;

            this.ThisMonthSummary.TotalExpenseAmount = 567.00m;
            this.ThisMonthSummary.TotalIncomeAmount = 5100.00m;

            this.TodaySummary.TotalExpenseAmount = 26.00m;
            this.TodaySummary.TotalIncomeAmount = 0.0m;
            this.ThisWeekSummary.TotalExpenseAmount = 234;
            this.ThisWeekSummary.TotalIncomeAmount = 0;

            this.AccountInfoSummary.MoneyInfo.Currency = AppSetting.Instance.DefaultCurrency;
            this.AccountInfoSummary.MoneyInfo.Money = this.AccountBookDataContext.Accounts.ToList<Account>().Sum<Account>(p => p.GetMoney()).GetValueOrDefault();

            this.AccountInfoSummary.TotalExpenseAmount = 5643;
            this.AccountInfoSummary.TotalExpenseAmountInfo = AppSetting.Instance.ShowCashAmountOnAsset ? "({0})".FormatWith(new object[] { 5643m.ToMoneyF2() }) : string.Empty;


            this.AccountItemRecordsSummary.TotalExpenseAmountInfo = 34.ToString();
            this.AccountItemRecordsSummary.TotalIncomeAmountInfo = "";


            BudgetManager.Current.UpdateCurrentMonthBudgetSummaryFromValue(this.ThisMonthSummary.TotalExpenseAmount);

            this.IsSummaryListLoaded = true;
        }

        public void LoadSummaryForAgent()
        {
            this.isForAgent = true;
            this.LoadSummary();
            this.isForAgent = false;
        }

        public void PinToStart(bool fromApp = true)
        {
            if (TinyMoneyManager.Component.ShellTileHelper.IsPinned(UriUsedToPinToStartScreen))
            {
                if (!AppSetting.Instance.ShowRepaymentInfoOnTile)
                {
                    AppSetting.Instance.ShowRepaymentInfoOnTile = true;
                }
                this.UpdateTileData();
                TinyMoneyManager.ScheduledAgentLib.ShellTileHelper.UnPin(UriUsedToPinToStartScreen);
            }
            else
            {
                StandardTileData initialData = this.CreateShellTileData();
                ApplicationHelper.Pin(UriUsedToPinToStartScreen, initialData);
            }
        }

        public void SendSummary()
        {
            string subject = "{0} - {1}".FormatWith(new object[] { LocalizedStrings.GetLanguageInfoByKey("MyBook"), LocalizedStrings.GetLanguageInfoByKey("Particulars") });
            string body = this.buildSummaryTextForMail();
            Helper.SendEmail(subject, body);
        }

        private void SetCashAmountOnAsset()
        {
            decimal val = this.SumCashAmountCount();
            this.AccountInfoSummary.TotalExpenseAmount = val;
            this.AccountInfoSummary.TotalExpenseAmountInfo = AppSetting.Instance.ShowCashAmountOnAsset ? "({0})".FormatWith(new object[] { val.ToMoneyF2() }) : string.Empty;
        }

        private decimal SumCashAmountCount()
        {
            System.Collections.Generic.List<Account> source = (from p in this.AccountBookDataContext.Accounts
                                                               where ((int)p.Category) == 0
                                                               select p).ToList<Account>();
            return source.Select<Account, decimal?>(delegate(Account p)
            {
                decimal conversionRateTo = p.Currency.GetConversionRateTo(AppSetting.Instance.DefaultCurrency);
                decimal? balance = p.Balance;
                if (!balance.HasValue)
                {
                    return null;
                }
                return new decimal?(conversionRateTo * balance.GetValueOrDefault());
            }).Sum().GetValueOrDefault();
        }

        public void UpdateFavoriteLinks(PanoramaItem recentItemContent, PhoneApplicationPage page, Grid sourceProvider)
        {
            if (!this.customizedTallyViewModel.HasLoadItemsInStarupPage)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    System.Action a = null;
                    if (this.isStartLoadingSummary)
                    {
                        while (!this.IsSummaryListLoaded)
                        {
                        }
                    }
                    if (!this.customizedTallyViewModel.HasLoadItemsInStarupPage)
                    {
                        this.customizedTallyViewModel.LoadItemsInStartupPage();
                    }
                    bool isSummaryListLoaded = this.IsSummaryListLoaded;
                    if (!this.hasLoadRencentItems)
                    {
                        if (a == null)
                        {
                            a = delegate
                            {
                                RoutedEventHandler handler = null;
                                DataTemplate template = page.Resources["Original"] as DataTemplate;
                                ItemsPanelTemplate template2 = page.Resources["RecentItemsPanelTemp"] as ItemsPanelTemplate;
                                if (ViewModelLocator.CustomizedTallyViewModel.FavoritesList.Count > 0)
                                {
                                    ListBox element = new ListBox
                                    {
                                        ItemTemplate = template,
                                        ItemsSource = ViewModelLocator.CustomizedTallyViewModel.FavoritesList
                                    };
                                    ScrollViewer.SetVerticalScrollBarVisibility(element, ScrollBarVisibility.Disabled);
                                    element.ItemsPanel = template2;
                                    recentItemContent.Content = element;
                                    this.hasLoadRencentItems = true;
                                }
                                else
                                {
                                    StackPanel panel = new StackPanel
                                    {
                                        Orientation = Orientation.Vertical
                                    };
                                    HyperlinkButton button = new HyperlinkButton
                                    {
                                        Style = Application.Current.Resources["HyperlinkEmptyStyle"] as Style
                                    };
                                    if (handler == null)
                                    {
                                        handler = delegate(object o1, RoutedEventArgs e1)
                                        {
                                            page.NavigationService.Navigate(new Uri("/Pages/AppSettingPage/PreferenceSettingPage.xaml?goto={0}".FormatWith(new object[] { 1 }), UriKind.RelativeOrAbsolute));
                                        };
                                    }
                                    button.Click += handler;
                                    TextBlock block = new TextBlock
                                    {
                                        Text = AppResources.PreferenceSetting_AddRule.ToLowerInvariant(),
                                        Style = Application.Current.Resources["PhoneTextLargeStyle"] as Style,
                                        Foreground = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush
                                    };
                                    button.Content = block;
                                    panel.Children.Add(button);
                                    recentItemContent.Content = panel;
                                }
                            };
                        }
                        page.Dispatcher.BeginInvoke(a);
                    }
                });
            }
        }

        /// <summary>
        /// Updates the tile data.
        /// </summary>
        /// <param name="fromApp">if set to <c>true</c> [from app].</param>
        public void UpdateTileData(bool fromApp = true)
        {
            if (ViewModelLocator.MainPageViewModel.IsDataLoaded)
            {
                ScheduledAgentConfiguration.RestoreTileInfoFromBackupGrid(App.QuickNewRecordName, this.countOfToday, this.amountInfoOfToday, AppSetting.Instance.ShowRepaymentInfoOnTile);

                if (!DataSynchronizationHandler.ToBackupDataBase && this.HasPinedToStart)
                {
                    StandardTileData data = this.CreateShellTileData();
                    TinyMoneyManager.Component.ShellTileHelper.GetAppTile(UriUsedToPinToStartScreen).Update(data);
                }
            }
        }

        public void UpdatingComparationStatus()
        {
            this.ThisMonthSummary.Updating();
            this.ThisWeekSummary.Updating();
            if (this._needToShowYearInfo)
            {
                this.ThisYearSummary.Updating();
            }
        }

        public void UpdatingCompareingAccountInfo(bool forceToLoad = false)
        {
            if (forceToLoad || AppSetting.Instance.ShowAssociatedAccountItemSummary)
            {
                if (this.hasLoadCompareInfo)
                {
                    this.UpdatingComparationStatus();
                }
                else
                {
                    this.LoadCompareAmountInfo();
                    this.hasLoadCompareInfo = true;
                }
            }
        }

        public void UpdatingRecentItems()
        {
            if (!this.hasUpdatingRecents)
            {
                this.hasUpdatingRecents = true;
            }
        }

        public AnalyziationSummarization AccountInfoSummary { get; set; }

        public AnalyziationSummarization AccountItemRecordsSummary { get; set; }

        public AnalyziationSummarization AccountMonthBudget { get; set; }

        public CustomizedTallyViewModel customizedTallyViewModel
        {
            get
            {
                return ViewModelLocator.CustomizedTallyViewModel;
            }
        }

        public ObservableCollection<AccountItem> FavouritesItems { get; set; }

        public bool HasLoadCompareInfo
        {
            get
            {
                return this.hasLoadCompareInfo;
            }
            set
            {
                this.hasLoadCompareInfo = value;
            }
        }

        public bool HasPinedToStart
        {
            get
            {
                return TinyMoneyManager.Component.ShellTileHelper.IsPinned(UriUsedToPinToStartScreen);
            }
        }

        public bool IsSummaryListLoaded
        {
            get
            {
                return this.isSummaryListLoaded;
            }
            set
            {
                if (this.isSummaryListLoaded != value)
                {
                    this.OnNotifyPropertyChanging("IsSummaryListLoaded");
                    this.isSummaryListLoaded = value;
                    this.OnNotifyPropertyChanged("IsSummaryListLoaded");
                }
            }
        }

        public ObservableCollection<MainPageItemViewModel> Items { get; private set; }

        public bool NeedToShowYearInfo
        {
            get
            {
                return this._needToShowYearInfo;
            }
            set
            {
                this._needToShowYearInfo = value;
                this.OnNotifyPropertyChanged("NeedToShowYearInfo");
            }
        }

        public ObservableCollection<AnalyziationSummarization> SummaryItemList { get; set; }

        public AnalyziationSummarization ThisMonthSummary { get; set; }

        public AnalyziationSummarization ThisWeekSummary { get; set; }

        public AnalyziationSummarization ThisYearSummary { get; set; }

        public AnalyziationSummarization TodaySummary { get; set; }

        public static Uri UriUsedToPinToStartScreen
        {
            get
            {
                if (uriUsedToPinToStartScreen == null)
                {
                    uriUsedToPinToStartScreen = new Uri("/", UriKind.RelativeOrAbsolute);
                }
                return uriUsedToPinToStartScreen;
            }
        }
    }
}

