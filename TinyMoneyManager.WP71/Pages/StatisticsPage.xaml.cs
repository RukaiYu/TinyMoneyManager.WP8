using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using TinyMoneyManager.Controls;
using NkjSoft.WPhone.Extensions;
using System.Text;
using System.Collections;
namespace TinyMoneyManager.Pages
{
    using NkjSoft.Extensions;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.Data;
    using NkjSoft.WPhone;
    using System.Threading;
    using JasonPopupDemo;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.Language;

    public partial class StatisticsPage : PhoneApplicationPage
    {
        //string showBlance = "";
        //string hideBlance = "";
        private ColumnSummary columnSummary;
        private CategorySummary categorySummary;
        private string income = "";
        private string expense = "";
        bool hasNavigate = false;
        int currentSummary = -1;
        bool hasChartFirstTimeLoaded = false;
        int beforePivotIndex = 1;
        //ApplicationBarIconButton showAndHideBlanceButton;
        ApplicationBarIconButton sendButton;
        ApplicationBarIconButton _searchButton;
        private string currentTitle = string.Empty;
        public DetailsCondition SearchingCondition { get; set; }

        private string searchingText = "";
        private string refineText = "";

        private DateTime movingMonthStartDateOffset = DateTime.Now;
        private DateTime movingMonthEndDateOffset = DateTime.Now;

        public static DetailsCondition cached = null;


        public string balanceName { get; set; }
        bool canSendText = false;

        bool needRefreshDataDueToTheSearchingConditionChanged;
        bool NeedRefreshDataDueToTheSearchingConditionChanged
        {
            get
            {
                var temp = needRefreshDataDueToTheSearchingConditionChanged;
                needRefreshDataDueToTheSearchingConditionChanged = false;

                return temp;
            }
            set
            {
                needRefreshDataDueToTheSearchingConditionChanged = value;
            }
        }

        public StatisticsPage()
        {
            SearchingCondition = ensureSearchingCondition();

            InitializeComponent();
            SearchDuringDate.ItemCountThreshold = 6;
            TiltEffect.SetIsTiltEnabled(this, true);
            SetText();
            this.SearchingPivot.DataContext = SearchingCondition;
            this.income = AppResources.Income;
            this.searchingText = AppResources.Search;
            this.refineText = AppResources.Refine;
            this.expense = AppResources.Expenses;
            balanceName = AppResources.BlanceInfo;

            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(StatisticsPage_OrientationChanged);
            this.Loaded += new RoutedEventHandler(StatisticsPage_Loaded);
            this.Pivots.SelectionChanged += new SelectionChangedEventHandler(Pivots_SelectionChanged);

            canSendText = AppSetting.Instance.HasEmail;

            option = new ExportDataOption();
            //option.Subject = LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport");
            summaryTitleFormatter = AppResources.SummaryTitleFormatter;

            option.EnableChangeSearchingScope = false;
            option.EnableChangeExportDataMode = false;
            option.EnableChangeExportDataType = true;

            movingMonthStartDateOffset = AppSetting.Instance.BudgetStatsicSettings.StartDate;
            movingMonthEndDateOffset = AppSetting.Instance.BudgetStatsicSettings.EndDate;
        }

        private DetailsCondition ensureSearchingCondition()
        {
            if (cached != null)
                return cached;
            var fromCacheString = System.IO.IsolatedStorage.IsolatedStorageSettings
                .ApplicationSettings.GetIsolatedStorageAppSettingValue(UserSearchingHabitKey, string.Empty);

            if (string.IsNullOrEmpty(fromCacheString))
            {
                cached = new DetailsCondition();
            }
            else
            {
                try
                {
                    cached = XmlHelper.DeserializeFromXmlString<DetailsCondition>(fromCacheString);
                }
                catch (Exception)
                {
                    cached = new DetailsCondition();
                }
            }

            return cached;
        }

        void StatisticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (hasNavigate == false)
            {

                //this.InvokeInThread(() =>
                //{
                PrepareSearchingCondition();
                //}); 
                hasNavigate = true;
            }
        }

        private void PrepareSearchingCondition()
        {
            SearchDuringDate.SelectedIndex = (int)SearchingCondition.SearchingScope;

            CategoryType.SelectedIndex = (int)SearchingCondition.IncomeOrExpenses;

            CategoryGroupModePicker.SelectedIndex = (int)SearchingCondition.GroupCategoryMode;

            DuringChartMode.SelectedIndex = (int)SearchingCondition.DuringMode;

            AccountName.SelectedItems = SearchingCondition.GetAccountsyEntryiesRelated();

            CategoryName.SelectedItems = SearchingCondition.GetCategoryEntryiesRelated();

            ShowOnlyIsClaim.IsChecked = SearchingCondition.ShowOnlyIsClaim.GetValueOrDefault();

            InitializeChart();

            StartDateSelector.Value = SearchingCondition.StartDate;
            EndDateSelector.Value = SearchingCondition.EndDate;

            hasNavigate = true;

            ShowDetails(SearchingCondition);

            hockControlEvent();

            SearchingCondition.PropertyChanged += new PropertyChangedEventHandler(SearchingCondition_PropertyChanged);
        }

        void SearchingCondition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NeedRefreshDataDueToTheSearchingConditionChanged = true;
        }

        private void hockControlEvent()
        {
            SearchDuringDate.SelectionChanged += new SelectionChangedEventHandler(SearchDuringDate_SelectionChanged);
            CategoryType.SelectionChanged += new SelectionChangedEventHandler(CategoryType_SelectionChanged);

            CategoryGroupModePicker.SelectionChanged += new SelectionChangedEventHandler(CategoryGroupModePicker_SelectionChanged);

            DuringChartMode.SelectionChanged += new SelectionChangedEventHandler(DuringChartMode_SelectionChanged);
        }

        private ApplicationBarMenuItem dayOfCurveButton;
        private ApplicationBarMenuItem countOfPercentButton;
        private ApplicationBarMenuItem amountOfPercentButton;

        private ApplicationBarIconButton previousMonthButton;
        private ApplicationBarIconButton nextMonthButton;

        private void SetText()
        {
            sendButton = ApplicationBar.GetIconButtonFrom(1);
            _searchButton = ApplicationBar.GetIconButtonFrom(0);

            sendButton.Text = this.GetLanguageInfoByKey("ExportReport");

            ShowTipsButton = ApplicationBar.GetMenuItemButtonFrom(3);
            ShowTipsButton.Text = this.GetLanguageInfoByKey("Tips");

            dayOfCurveButton = (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem);
            countOfPercentButton = (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem);
            amountOfPercentButton = ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;

            dayOfCurveButton.Text = this.GetLanguageInfoByKey("ButtonTextForDayOfDuring");
            countOfPercentButton.Text = this.GetLanguageInfoByKey("ButtonTextCategoryForCount");
            amountOfPercentButton.Text = this.GetLanguageInfoByKey("ButtonTextCategoryForTotalAmount");

            var searchScopeTexts = new string[] { 
                LocalizedStrings.GetLanguageInfoByKey("Today"),
                LocalizedStrings.GetLanguageInfoByKey("CurrentWeek"),
                LocalizedStrings.GetLanguageInfoByKey("CurrentMonth"),
                LocalizedStrings.GetLanguageInfoByKey("CurrentYear"),
                LocalizedStrings.GetLanguageInfoByKey("Customize"),
                LocalizedStrings.GetLanguageInfoByKey("All"),
            };

            this.loadParentCategoryForSearching = () => GetCategories(p => p.IsParent);
            this.loadChildCategoryForSearching = () => GetCategories(p => !p.IsParent);

            SearchDuringDate.ItemsSource = searchScopeTexts;
            AccountName.ItemsSource = ViewModelLocator.AccountViewModel.Accounts;
            AccountName.SummaryForSelectedItemsDelegate = ShowAccountsAfterSeleted;
            //LoadCategoryDataForSearching();
            CategoryName.SummaryForSelectedItemsDelegate = ShowCategoriesAfterSeleted;


            InitializeMonthNavigateButtons();
        }

        private void InitializeMonthNavigateButtons()
        {
            previousMonthButton = new ApplicationBarIconButton(IconUirs.PreviousIcon);
            previousMonthButton.Text = this.GetLanguateInfoByKeys("{0} {1}", "Previous", "Month");

            nextMonthButton = new ApplicationBarIconButton(IconUirs.NextIcon);
            nextMonthButton.Text = this.GetLanguateInfoByKeys("{0} {1}", "Next", "Month");

            ApplicationBar.Buttons.Add(previousMonthButton);
            ApplicationBar.Buttons.Add(nextMonthButton);

            previousMonthButton.Click += new EventHandler((o, e) =>
            {
                MovingDataByMonth(SearchingScope.LastMonth);
            });

            nextMonthButton.Click += new EventHandler((o, e) =>
            {
                MovingDataByMonth(SearchingScope.NextMonth);
            });
        }

        /// <summary>
        /// Movings the data by month.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="dateOffset">The date offset.</param>
        private void MovingDataByMonth(SearchingScope scope)
        {
            this.BusyForWork(AppResources.Loading);
            this.SearchDuringDate.SelectionChanged -= SearchDuringDate_SelectionChanged;
            this.SearchDuringDate.SelectedIndex = 4;
            this.SearchDuringDate.SelectionChanged += SearchDuringDate_SelectionChanged;

            DetailsCondition.ReCalculate(this.SearchingCondition, scope, movingMonthStartDateOffset, movingMonthEndDateOffset);

            this.StartDateSelector.Visibility = EndDateSelector.Visibility = Visibility.Visible;

            this.StartDateSelector.Value = this.SearchingCondition.StartDate;
            this.EndDateSelector.Value = this.SearchingCondition.EndDate;

            movingMonthStartDateOffset = this.SearchingCondition.StartDate.Value;
            movingMonthEndDateOffset = this.SearchingCondition.EndDate.Value;

            NeedRefreshDataDueToTheSearchingConditionChanged = true;
            CurrentChart.NeedRefreshData = true;

            ShowDetails(SearchingCondition, SearchingScope.Customize);
        }

        private IEnumerable<Category> GetCategories(Func<Category, bool> fliter)
        {
            var vm = new ViewModels.CategoryViewModel()
            {
                AccountBookDataContext = new TinyMoneyDataContext()
            };
            {
                var searchingType = SearchingCondition.IncomeOrExpenses;
                if (searchingType == ItemType.All)
                {
                    return vm.GetDataFromDatabase(fliter);
                }
                else
                {
                    return vm.GetDataFromDatabase(p => fliter(p) && p.CategoryType == searchingType);
                }
            }

        }

        /// <summary>
        /// Gets the currency chart.
        /// </summary>
        public ISummaryChart CurrentChart
        {
            get { return (ImagePivot.Content as ISummaryChart); }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Pivots control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        void Pivots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = Pivots.SelectedIndex;

            beforePivotIndex = index;

            if (index == 2 && !hasChartFirstTimeLoaded)
            {
                this.DoingWork();

                ToggleShowLinerChartDataBind();

                hasChartFirstTimeLoaded = true;
            }
            //this.InvokeInThread(() =>
            //{
            if (index == 0)
            {
                _searchButton.IconUri = IconUirs.SearchingButtonIcon;
                _searchButton.Text = searchingText;
            }
            else
            {
                _searchButton.IconUri = IconUirs.SearchRefineIconButton;
                _searchButton.Text = refineText;
            }

            //});
            sendButton.IsEnabled = index != 0;
        }

        void showOrHideBlance(bool isShow)
        {
            //showAndHideBlanceButton.Text = isShow ? hideBlance : showBlance;
        }

        void StatisticsPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (CurrentChart != null)
                CurrentChart.DetectImageSize(this.Orientation);
        }

        private string c(ItemType t)
        {
            return t == ItemType.Expense ? expense : income;
        }

        private string ShowAccountsAfterSeleted(IList listSource)
        {
            if (listSource == null || listSource.Count == 0)
            {
                SearchingCondition.AccountIds.Clear();
                return string.Empty;
            }
            SearchingCondition.AccountIds.Clear();
            var names = new StringBuilder();
            listSource.OfType<Account>()
           .ForEach(p =>
           {
               names.AppendFormat("{0},", p.Name);
               SearchingCondition.AccountIds.Add(p.Id);
           });
            var temp = names.ToString(0, names.Length - 1);
            AccountName.Tag = temp;
            return temp;
        }

        private string ShowCategoriesAfterSeleted(IList listSource)
        {
            if (listSource == null || listSource.Count == 0)
            {
                SearchingCondition.CategoryIds.Clear();
                return string.Empty;
            }

            SearchingCondition.CategoryIds.Clear();
            var names = new StringBuilder();
            listSource.OfType<Category>()
           .ForEach(p =>
           {
               names.AppendFormat("{0},", p.Name);
               SearchingCondition.CategoryIds.Add(p.Id);
           });
            var temp = names.ToString(0, names.Length - 1);
            CategoryName.Tag = temp;
            return temp;
        }

        IEnumerable<SummaryDetails> cacheForSummary;
        private void ShowDetails(DetailsCondition e, SearchingScope? scoprToFireChanging = null)
        {
            //this.InvokeInThread(() =>
            //{
            List<AccountItemSummaryGroup> _temp = null;
            ThreadPool.QueueUserWorkItem((o) =>
            {
                IEnumerable<AccountItem> data;
                _temp = RetrivteSummaryData(e, out data, true);

                Dispatcher.BeginInvoke(() =>
                {
                    if (_temp != null)
                    {
                        summaryDetails.ItemsSource = _temp;

                        var _tempGroup = _temp.FirstOrDefault(p => p.Key == income);
                        var _totalIncome = _tempGroup == null ? 0.0M : _tempGroup.GroupTotalAmout.GetValueOrDefault();

                        _tempGroup = _temp.FirstOrDefault(p => p.Key == expense);
                        var _totalExpenses = _tempGroup == null ? 0.0M : _tempGroup.GroupTotalAmout.GetValueOrDefault();

                        TotalIncomeText.Text = AccountItemMoney.GetMoneyInfoWithCurrency(_totalIncome);
                        TotalExpensesText.Text = AccountItemMoney.GetMoneyInfoWithCurrency(_totalExpenses);
                        BlanceAmountText.Text = AccountItemMoney.GetMoneyInfoWithCurrency((_totalIncome - _totalExpenses));

                        BalanceTitle.Text = SearchingCondition.BuildeOnlySearchingScope(scoprToFireChanging);
                        currentTitle = SearchingCondition.SummaryTitle;
                        CurrentChart.SummaryTitle = currentTitle;
                    }

                    portDataToISummaryChart(e, cacheForSummary, data);
                    this.WorkDone();
                });

            });

            //BuilderEmailBody(_temp);

            //});

        }

        private List<AccountItemSummaryGroup> RetrivteSummaryData(DetailsCondition e, out IEnumerable<AccountItem> data, bool needBack)
        {
            data = StatsticSummaryItemsViewer.QueryDataBySearchingCondition(e);

            if (data != null)
            {
                var dataForBinding = (from d in data
                                      where d.Category != null
                                      group d by new
                                      {
                                          CategoryName = e.ChartGroupMode == ChartGroupMode.ByCategoryName ? (
                                                        e.GroupCategoryMode == CategorySortType.ByChildCategory || d.Category.IsParent ? d.Category.Name : d.Category.ParentCategory.Name)
                                                        : d.AccountName,
                                          d.Type
                                      } into g
                                      select new SummaryDetails()
                                      {
                                          Name = g.Key.CategoryName,
                                          AccountItemType = g.Key.Type,
                                          Count = g.Count(),
                                          TotalAmout = g.Sum(p => p.GetMoney()),
                                          Tag = g.FirstOrDefault(),
                                      });

                cacheForSummary = dataForBinding;

                var _temp = (from d in dataForBinding
                             group d by c(d.AccountItemType) into g
                             select new AccountItemSummaryGroup(g)
                             {
                                 GroupCount = g.Sum(x => x.Count),
                                 GroupTotalAmout = g.Sum(p => p.TotalAmout),
                             }).ToList();
                if (needBack == false)
                    data = null;
                return _temp;
            }

            return null;
        }

        public void CreateControl()
        {
            ColumnSummary cs = new ColumnSummary();
            this.ImagePivot.Content = cs;
        }

        bool hasRegisteFullScreenForChart;
        /// <summary>
        /// Sets the type of the chart.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="imageSummary">The image summary.</param>
        private void SetChartType<T>(T imageSummary) where T : UserControl, ISummaryChart, new()
        {
            this.ImagePivot.Content = imageSummary;

            if (imageSummary.ToShowChartInFullScreen == null)
            {
                imageSummary.ToShowChartInFullScreen = (c) =>
                {
                    //StatisticCharShower.ContentToShowGetter = () => imageSummary;
                    this.NavigateTo("/Pages/DialogBox/StatisticCharShower.xaml");
                };
            }
        }

        /// <summary>
        /// Ports the data to I summary chart.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="dataForBinding">The data for binding.</param>
        /// <param name="originalData">The original data.</param>
        void portDataToISummaryChart(DetailsCondition e, IEnumerable<SummaryDetails> dataForBinding, IEnumerable<AccountItem> originalData)
        {
            var chart = CurrentChart;

            if (chart != null)
            {
                chart.LoadData(e, dataForBinding);
            }
        }

        private void EachDayOfDuring_Click(object sender, EventArgs e)
        {
            ToggleShowLinerChartDataBind();
        }

        private void EachCategoryCountOfDuring_Click(object sender, EventArgs e)
        {
            ToggleCategoryDataBind(CategorySummary.DependentValuePathForCount);
        }

        private void EachCategoryTotalAmountOfDuring_Click(object sender, EventArgs e)
        {
            ToggleCategoryDataBind(CategorySummary.DependentValuePathForAmount);
        }

        private void ToggleShowLinerChartDataBind()
        {
            this.DoingWork();
            if (currentSummary != 0)
            {
                if (columnSummary == null)
                    columnSummary = new ColumnSummary();
                SetChartType(columnSummary);
                currentSummary = 0;
            }

            if (NeedRefreshDataDueToTheSearchingConditionChanged)
            {
                columnSummary.NeedRefreshData = true;
                currentTitle = SearchingCondition.SummaryTitle;
                Thread th = new Thread(new ThreadStart(() =>
                {
                    this.InvokeInThread(() => columnSummary.LoadData(SearchingCondition, null));
                }));
                th.Start();

            }
            Pivots.SelectedIndex = 2;
            this.WorkDone();
        }

        private void ToggleCategoryDataBind(string dependentValuePath)
        {
            DoingWork();
            if (currentSummary != 1)
            {
                InitializeChart();
                currentSummary = 1;
            }

            if (categorySummary != null)
            {
                if (NeedRefreshDataDueToTheSearchingConditionChanged)
                {
                    categorySummary.NeedRefreshData = true;
                }

                if (categorySummary.NeedRefreshData)
                {
                    currentTitle = SearchingCondition.SummaryTitle;
                }

                categorySummary.DependentValuePath = dependentValuePath;
                categorySummary.LoadData(SearchingCondition, cacheForSummary);

                categorySummary.SummaryTitle = currentTitle;
            }
            beforePivotIndex = 2;
            Pivots.SelectedIndex = 2;
            this.WorkDone();
        }

        void categorySummary_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DependentValuePath")
            {
                categorySummary.LoadData(SearchingCondition, cacheForSummary);
            }
        }

        private void ShowOrHideBlanceButton_Click(object sender, EventArgs e)
        {
            var isShow = BlancePanel.Visibility == System.Windows.Visibility.Visible;
            this.BlancePanel.Visibility = isShow ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            showOrHideBlance(!isShow);
        }

        private void SearchDuringDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchDuringDate == null) return;

            var index = SearchDuringDate.SelectedIndex;
            SearchingCondition.SearchingScope = (SearchingScope)index;

            StartDateSelector.Visibility = EndDateSelector.Visibility = index == 4 ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            StartDateSelector.Value = SearchingCondition.StartDate;
            EndDateSelector.Value = SearchingCondition.EndDate;
        }

        /// <summary>
        /// Initializes the chart.
        /// </summary>
        private void InitializeChart()
        {
            if (categorySummary == null)
            {
                categorySummary = new CategorySummary();
                categorySummary.PropertyChanged += new PropertyChangedEventHandler(categorySummary_PropertyChanged);
            }

            SetChartType(categorySummary);
        }

        const string dateTimeFormatter = "yyyy-M-dd";
        ExportDataOption option;
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                option = new ExportDataOption();
                option.Subject = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";//.Replace("/", ".");
                option.ExportDataMode = SummarySendingMode.BySkyDrive;
                cmb = new CustomMessageBox();

                ExportDataOptionPanel popupContent = new ExportDataOptionPanel(option)
                {
                    ExportDataOption = option
                };
                popupContent.ExportDataOption.ExportDataType = SummaryDataType.ExpenseOrIncomeRecords;
                popupContent.ExportDataOption.ExportDataMode = SummarySendingMode.BySkyDrive;
                cmb.LeftButtonContent = AppResources.ExportReport.ToUpper();
                cmb.IsLeftButtonEnabled = true;
                cmb.Dismissed += cmb_Dismissed;
                cmb.Content = popupContent;

                cmb.Show();

            }
            catch (Exception ex)
            {
                this.Alert(ex.Message);
            }
        }

        void cmb_Dismissed(object sender, DismissedEventArgs e)
        {
            if (e.Result == CustomMessageBoxResult.LeftButton)
            {
                try
                {
                    SendingProcess();
                }
                catch (System.Exception exception)
                {
                    this.Alert(exception.Message, null);
                }
            }
        }

        void optionPanel_Confirmed(object sender, EventArgs e)
        {
            // show option panel.
            SendingProcess();
        }
        private string summaryTitleFormatter = "{0}";

        public string SummaryTitleFormatter
        {
            get
            {
                if (!option.Subject.IsNullOrEmpty())
                {
                    return option.Subject;
                }

                return summaryTitleFormatter;
            }
            set
            {
                summaryTitleFormatter = value;
            }
        }

        /// <summary>
        /// Sendings the process.
        /// </summary>
        private void SendingProcess()
        {
            var index = Pivots.SelectedIndex;

            if (option.ExportDataType == SummaryDataType.ExpenseOrIncomeRecords)
            {
                IEnumerable<AccountItem> dataSource = BuildDataForSendingAccountItemEntryReport(SearchingCondition);
                SendToSkyDrive(() =>
                ViewModelLocator.AccountItemViewModel.BuildAccountItemReport(dataSource));
                return;
            }

            switch (index)
            {
                case 0:
                default:
                    break;
                case 1:
                    SendingForSummary();
                    break;
                case 2:
                    SendingForChart();
                    break;
            }
        }

        public IEnumerable<AccountItem> BuildDataForSendingAccountItemEntryReport(DetailsCondition e)
        {
            IEnumerable<AccountItem> data = ViewModelLocator.AccountItemViewModel.AccountBookDataContext.AccountItems;
            if (e.SearchingScope != SearchingScope.All)
            {
                data = data.Where(p => p.CreateTime.Date >= e.StartDate.GetValueOrDefault().Date
                              && p.CreateTime.Date <= e.EndDate.GetValueOrDefault().Date);
            }

            if (e.IncomeOrExpenses != ItemType.All)
                data = data.Where(p => p.Type == e.IncomeOrExpenses);

            if (e.AccountIds.Count != 0)
            {
                data = data.Where(p => e.AccountIds.Contains(p.AccountId));
            }

            if (e.CategoryIds.Count != 0)
            {
                if (e.GroupCategoryMode == CategorySortType.ByChildCategory)
                    data = data.Where(p => e.CategoryIds.Contains(p.CategoryId));
                else
                {
                    data = data.Where(p => e.CategoryIds.Contains(p.Category.ParentCategoryId));
                }
            }

            var dataForBinding = (from d in data
                                  group d by new
                                  {
                                      CategoryName = e.ChartGroupMode == ChartGroupMode.ByCategoryName ? (
                                                    e.GroupCategoryMode == CategorySortType.ByChildCategory ? d.Category.Name : d.Category.ParentCategory.Name)
                                                    : d.AccountName,
                                      d.Type
                                  } into g
                                  select g);

            return data;
        }

        /// <summary>
        /// Sendings for chart.
        /// </summary>
        private void SendingForChart()
        {
            if (CurrentChart is ColumnSummary)
            {
                SendingForColumnSummary();
            }
            else if (CurrentChart is CategorySummary)
            {
                SendingForSummary();
            }
        }

        /// <summary>
        /// Sendings for column summary.
        /// </summary>
        private void SendingForColumnSummary()
        {
            SendToSkyDrive(() =>
            {
                var part = new StringBuilder(BuildDuringSummaryRowsContent(columnSummary.GetDataForSummary(ItemType.Expense), ItemType.Expense));
                part = part.Append(BuildDuringSummaryRowsContent(columnSummary.GetDataForSummary(ItemType.Income), ItemType.Income));

                return MergeSummary(part);
            });
        }

        /// <summary>
        /// Merges the summary.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        private string MergeSummary(StringBuilder part)
        {
            var columnHeader = ExcelSummaryBuilder.TableColumnRowFormatter.FormatWith(CategoryType.Header.ToString(),
                LocalizedStrings.GetLanguageInfoByKey("CreateDate"),
                "{0}({1})".FormatWith(LocalizedStrings.GetLanguageInfoByKey("Amount"), AppSetting.Instance.CurrencyInfo.CurrencyString),
                LocalizedStrings.GetLanguageInfoByKey("Records").Replace(":", string.Empty)
                , string.Empty, string.Empty, string.Empty).ToUnicodeInt64Value();

            var title = LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport")
             .ToUnicodeInt64Value();

            part.Append(BuildSumLine(string.Empty, string.Empty));
            part.Append(BuildSumLine(string.Empty, string.Empty));
            part.Append(BuildSumLine(this.TotalIncomeLabel.Text, TotalIncomeText.Text));
            part.Append(BuildSumLine(this.TotalExpenseLabel.Text, TotalExpensesText.Text));
            part.Append(BuildSumLine(this.TotalBalanceLabel.Text, BlanceAmountText.Text));
            part.Append(BuildSumLine(string.Empty, string.Empty));
            part.Append(BuildSumLine(string.Empty, string.Empty));
            part.Append(BuildSumLine(string.Empty, string.Empty));
            part.Append(BuildSumLine(string.Empty, string.Empty));
            part.Append(BuildSearchingInfoRow());
            part.Append(BuildSearchingInfoRows());

            var templeteFile = ExcelSummaryBuilder.AccountItemSummaryExcelTempleteFilePath;

            var t = ExcelSummaryBuilder.Build(templeteFile, title, columnHeader, part.ToString());

            return t;
        }

        /// <summary>
        /// Builds the content of the during summary rows.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <returns></returns>
        public string BuildDuringSummaryRowsContent(IEnumerable<SummaryDetails> dataSource, ItemType itemType)
        {
            var firstRowFormatter = string.Empty;
            var rowsFormatter = string.Empty;

            if (itemType == ItemType.Expense)
            {
                firstRowFormatter = ExcelSummaryBuilder.AccountItemRowBackgroundColorMoreGrayFirstRowFormatter;
                rowsFormatter = ExcelSummaryBuilder.AccountItemRowBackgroundColorMoreGrayFormatter;
            }

            if (itemType == ItemType.Income)
            {
                firstRowFormatter = ExcelSummaryBuilder.AccountItemRowBackgroundColorLightGrayFirstRowFormatter;
                rowsFormatter = ExcelSummaryBuilder.AccountItemRowBackgroundColorLightGrayFormatter;
            }


            var itemTypeName = LocalizedStrings.GetLanguageInfoByKey("CategoryType");
            StringBuilder sb = new StringBuilder();

            var items = dataSource.ToList();

            int count = items.Count;
            var item = items[0];

            sb.Append(string.Format(firstRowFormatter, itemType == ItemType.Expense ? expense : income, item.Name, item.AmountInfo, item.Count,
                   string.Empty, string.Empty, string.Empty, count)
                   .ToUnicodeInt64Value());

            for (int i = 1; i < count; i++)
            {
                item = items[i];
                sb.Append(string.Format(rowsFormatter, item.Name, item.AmountInfo, item.Count,
                    string.Empty, string.Empty, string.Empty)
                    .ToUnicodeInt64Value());
            }


            return sb.ToString();
        }

        /// <summary>
        /// Sendings for summary.
        /// </summary>
        private void SendingForSummary()
        {
            var regetdatedData = summaryDetails.ItemsSource as IEnumerable<AccountItemSummaryGroup>;
            if (regetdatedData == null || regetdatedData.Count() == 0)
            {
                this.Alert(this.GetLanguageInfoByKey("NoSummaryData"));
                return;
            }

            //var test = BuildSummaryContentBody();
            SendToSkyDrive(() => BuildSummaryContentBody());
        }

        /// <summary>
        /// Builds the summary content body.
        /// </summary>
        /// <returns></returns>
        private string BuildSummaryContentBody()
        {
            var income = LocalizedStrings.GetLanguageInfoByKey("Income");
            var expense = LocalizedStrings.GetLanguageInfoByKey("Expense");
            var countPencentageName = LocalizedStrings.GetLanguageInfoByKey("CountPencentage");
            var amountPencentageName = LocalizedStrings.GetLanguageInfoByKey("AmountPencentage");

            var title = LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport")
                 .ToUnicodeInt64Value();

            var recordsTitle = this.GetLanguageInfoByKey("Records").Replace(":", string.Empty);
            var amountTitle = "{0}({1})".FormatWith(this.GetLanguageInfoByKey("TotalMoney"), AppSetting.Instance.CurrencyInfo.CurrencyString);
            var type = this.CategoryType.Header.ToString();

            var columnHeader = ExcelSummaryBuilder.TableColumnRowFormatter
                .FormatWith(LocalizedStrings.GetLanguageInfoByKey("CategoryType"),
                 LocalizedStrings.GetLanguageInfoByKey("CategoryName"),
                recordsTitle,
               amountTitle,
               countPencentageName,
             amountPencentageName,
              string.Empty)
                .ToUnicodeInt64Value();

            StringBuilder sb = new StringBuilder();
            string[] keyType = new string[] { string.Empty, string.Empty };
            string[] count = new string[] { string.Empty, string.Empty };
            int counter = 0;

            var regetdatedData = summaryDetails.ItemsSource as IEnumerable<AccountItemSummaryGroup>;

            foreach (var item in regetdatedData)
            {
                var items = item.GetEnumerator();
                keyType[counter] = item.Key;
                count[counter] = item.GroupCount.ToString();
                counter++;
                //, item.GroupCount, amountTitle, item.GroupTotalAmoutInfo);
                while (items.MoveNext())
                {
                    var summary = items.Current;
                    var countPencentage = ((summary.Count * 1.0 / item.GroupCount * 1.0) * 100.0d).ToString("F2") + "%";
                    var amountPencentage = ((summary.TotalAmout / item.GroupTotalAmout) * 100.0m).Value.ToString("F2") + "%";

                    var formatter = summary.AccountItemType == ItemType.Expense
                        ? ExcelSummaryBuilder.AccountItemRowBackgroundColorMoreGrayFullFormatter : ExcelSummaryBuilder.AccountItemRowBackgroundColorLightGrayFullFormatter;

                    sb.Append(string.Format(formatter,
                        item.Key,//summary.AccountItemType == ItemType.Expense ? expense : income,
                        summary.Name, summary.Count, summary.AmountInfo, countPencentage, amountPencentage, string.Empty)
                        .ToUnicodeInt64Value());
                }
            }
            sb.Append(BuildSumLine(string.Empty, string.Empty));
            sb.Append(BuildSumLine(string.Empty, string.Empty));
            if (keyType[0].Length != 0)
            {
                sb.Append(BuildSumLine(keyType[0] + recordsTitle, count[0]));
            }
            if (keyType[1].Length != 0)
            {
                sb.Append(BuildSumLine(keyType[1] + recordsTitle, count[1]));
            }
            sb.Append(BuildSumLine(this.TotalIncomeLabel.Text, TotalIncomeText.Text));
            sb.Append(BuildSumLine(this.TotalExpenseLabel.Text, TotalExpensesText.Text));
            sb.Append(BuildSumLine(this.TotalBalanceLabel.Text, BlanceAmountText.Text));
            sb.Append(BuildSumLine(string.Empty, string.Empty));
            sb.Append(BuildSumLine(string.Empty, string.Empty));
            sb.Append(BuildSumLine(string.Empty, string.Empty));
            sb.Append(BuildSumLine(string.Empty, string.Empty));
            sb.Append(BuildSearchingInfoRow());
            sb.Append(BuildSearchingInfoRows());

            var templeteFile = ExcelSummaryBuilder.AccountItemSummaryExcelTempleteFilePath;

            var t = ExcelSummaryBuilder.Build(templeteFile, title, columnHeader, sb.ToString());

            return t;
        }

        /// <summary>
        /// Builds the searching info rows.
        /// </summary>
        /// <returns></returns>
        private string BuildSearchingInfoRows()
        {
            StringBuilder sb = new StringBuilder();
            var all = LocalizedStrings.GetLanguageInfoByKey("All");
            //
            sb.Append(BuildSumLine12(SearchDuringDate.Header.ToString() + ":" + SearchingCondition.BuildeOnlySearchingScope(), string.Empty));
            sb.Append(BuildSumLine12(CategoryType.Header.ToString() + ":" + LocalizedStrings.GetLanguageInfoByKey(((ItemType)CategoryType.SelectedIndex).ToString()), string.Empty));
            sb.Append(BuildSumLine12("[{0}] {1}".FormatWith(CategoryName.Header.ToString(), CategoryName.Tag ?? all), string.Empty));
            sb.Append(BuildSumLine12("[{0}] {1}".FormatWith(AccountName.Header.ToString(), AccountName.Tag ?? all), string.Empty));

            return sb.ToString();
        }

        /// <summary>
        /// Builds the searching info row.
        /// </summary>
        /// <returns></returns>
        public string BuildSearchingInfoRow()
        {
            var appInfoLine = LocalizedStrings.GetLanguageInfoByKey("ExpenseIncomeHistoryReport") + "-" + DateTime.Now.ToLocalizedDateTimeString();

            return string.Format(ExcelSummaryBuilder.AccountItemRowBackgroundColorMoreGrayAndMergeAllRowFormatter,
              appInfoLine, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
              .ToUnicodeInt64Value();
        }

        /// <summary>
        /// Buides the sum line12.This will take (1 ,2 ) columns
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public string BuildSumLine12(string left, string right)
        {
            return string.Format(
                 ExcelSummaryBuilder.AccountItemRowFormatter,
                 left, right, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty).ToUnicodeInt64Value();
        }

        /// <summary>
        /// Buildes the sum line. This will take (4 ,5 ) columns
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        private string BuildSumLine(string left, string right)
        {
            return string.Format(
                ExcelSummaryBuilder.AccountItemRowFormatter,
                string.Empty, string.Empty, string.Empty, string.Empty, left, right, string.Empty).ToUnicodeInt64Value();
        }

        private void SendToSkyDrive(Func<string> dataGetter)
        {
            SkyDriveDataSyncingPageViewModel.BackupAndRestoreDataSyncingMode = false;
            SkyDriveDataSyncingPage.DefaultFileExtension = ".xls";
            SkyDriveDataSyncingPage.Filename = option.Subject.Trim() + SkyDriveDataSyncingPage.DefaultFileExtension;

            SkyDriveDataSyncingPageViewModel.NormalDataSetter = dataGetter;

            this.NavigateTo(ViewPath.SkyDriveDataSynchronizationPage);
        }

        private void SendByEmail(IEnumerable<AccountItemSummaryGroup> data)
        {
            var email = AppSetting.Instance.Email;
            Microsoft.Phone.Tasks.EmailComposeTask emailSender = new Microsoft.Phone.Tasks.EmailComposeTask();

            emailSender.To = email;
            emailSender.Subject = summaryTitleFormatter.FormatWith(currentTitle);
            emailSender.Body = BuilderEmailBody(data);
            emailSender.Show();
        }

        private string BuilderEmailBody(IEnumerable<AccountItemSummaryGroup> regetdatedData)
        {
            return string.Empty;
        }

        private void CategoryGroupModePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryGroupModePicker == null) return;

            var changed = (CategorySortType)CategoryGroupModePicker.SelectedIndex;

            if (SearchingCondition.GroupCategoryMode != changed)
            {
                SearchingCondition.GroupCategoryMode = changed;

                CategoryName.SelectedItems = null;
                LoadCategoryDataForSearching();
            }

        }

        private Func<IEnumerable<Category>> loadChildCategoryForSearching;

        private Func<IEnumerable<Category>> loadParentCategoryForSearching;

        private void LoadCategoryDataForSearching()
        {
            if (SearchingCondition.GroupCategoryMode == CategorySortType.ByChildCategory)
            {
                var data = loadChildCategoryForSearching().ToList();
                Dispatcher.BeginInvoke(() =>
                {
                    this.CategoryName.ItemsSource = data;
                });
            }
            else if (SearchingCondition.GroupCategoryMode == CategorySortType.ByParentCategory)
            {
                var data = loadParentCategoryForSearching();

                Dispatcher.BeginInvoke(() =>
                {
                    this.CategoryName.ItemsSource = data;
                });
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            var pivotIndex = Pivots.SelectedIndex;

            if (pivotIndex != 0)
            {
                Pivots.SelectedIndex = 0;
            }
            else if (pivotIndex == 0)
            {
                DoingWork();
                DoSearch();
            }
        }

        private void DoingWork()
        {
            this.BusyForWork("thinking...");
        }

        private void DoSearch()
        {
            var startDate = StartDateSelector.Value;
            var endDate = EndDateSelector.Value;

            if (startDate > endDate)
            {
                this.Alert(this.GetLanguageInfoByKey("StartDateMustBeLargerThanEndDate"));
                return;
            }

            SearchingCondition.StartDate = startDate;
            SearchingCondition.EndDate = endDate;

            SearchingCondition.ShowOnlyIsClaim = ShowOnlyIsClaim.IsChecked;

            SearchingCondition.ChartGroupMode = (ChartGroupMode)ChartGroupModePicker.SelectedIndex;

            NeedRefreshDataDueToTheSearchingConditionChanged = true;

            ShowDetails(SearchingCondition);

            Pivots.SelectedIndex = beforePivotIndex == 0 ? 1 : beforePivotIndex;
        }

        private void AccountName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CategoryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryType == null) return;
            this.SearchingCondition.IncomeOrExpenses = (ItemType)CategoryType.SelectedIndex;
            LoadCategoryDataForSearching();
        }

        private void DuringChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DuringChartMode == null)
                return;
            var changed = (DuringMode)DuringChartMode.SelectedIndex;
            SearchingCondition.DuringMode = changed;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {

                System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[UserSearchingHabitKey]
                    = XmlHelper.SerializeToXmlString(SearchingCondition);
            });

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                DoingWork();
            }

            base.OnNavigatedTo(e);
        }

        private const string UserSearchingHabitKey = "UserSearchingHabitKey";
        private CustomMessageBox cmb;

        private void ShowTipsButton_Click(object sender, EventArgs e)
        {
            this.Alert(LocalizedStrings.GetLanguageInfoByKey("TipsWhenUploadingStasticsReport"));
        }

        private void summaryDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = summaryDetails.SelectedItem as SummaryDetails;

            if (item == null)
                return;

            Pages.DialogBox.StatsticSummaryItemsViewer.Show(item, SearchingCondition, this); //DataSourceGetter = () => queryForSeeMore;

            summaryDetails.SelectedItem = null;
        }

    }

}
