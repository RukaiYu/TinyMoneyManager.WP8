namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;

    public class DetailsCondition : NotionObject
    {
        private TinyMoneyManager.Component.ChartGroupMode chartGroupMode;
        private TinyMoneyManager.Component.DuringMode duringMode;
        private System.DateTime? endDate;
        private CategorySortType groupCategoryMode;
        private ItemType incomeOrExpenses;
        private string notesKey;
        private TinyMoneyManager.Component.SearchingScope searchingScope;
        private System.DateTime? startDate;

        public DetailsCondition()
        {
            this.StartDate = new System.DateTime?(System.DateTime.Now.Date);
            this.EndDate = new System.DateTime?(System.DateTime.Now.Date);
            this.IncomeOrExpenses = ItemType.All;
            this.AccountIds = new System.Collections.ObjectModel.Collection<Guid>();
            this.CategoryIds = new System.Collections.ObjectModel.Collection<Guid>();
            this.notesKey = string.Empty;
        }

        public string BuildeOnlySearchingScope(SearchingScope? scopreToFireChanging = null)
        {
            string format = "{0} ~ {1}";

            scopreToFireChanging = scopreToFireChanging ?? this.SearchingScope;

            if (scopreToFireChanging == TinyMoneyManager.Component.SearchingScope.All)
            {
                return LocalizedStrings.GetLanguageInfoByKey("All");
            }
            format = string.Format(format, this.StartDate.GetValueOrDefault().ToShortDateString(), this.EndDate.GetValueOrDefault().ToShortDateString());
            if (scopreToFireChanging != TinyMoneyManager.Component.SearchingScope.Today)
            {
                return string.Format("{0}({1})", LocalizedStrings.GetLanguageInfoByKey(scopreToFireChanging.Value.ToString()), format);
            }
            if (scopreToFireChanging != TinyMoneyManager.Component.SearchingScope.Customize)
            {
                format = LocalizedStrings.GetLanguageInfoByKey(scopreToFireChanging.Value.ToString()) + "(" + System.DateTime.Now.Date.ToLongDateString() + ")";
            }
            return format;
        }

        public string BuildSearchingScope()
        {
            string format = "{0} ~ {1}, ";
            if (this.SearchingScope == TinyMoneyManager.Component.SearchingScope.All)
            {
                if (this.IncomeOrExpenses != ItemType.All)
                {
                    return (LocalizedStrings.GetLanguageInfoByKey("All") + " ");
                }
                return string.Empty;
            }
            if (this.SearchingScope == TinyMoneyManager.Component.SearchingScope.Customize)
            {
                return string.Format(format, this.StartDate.GetValueOrDefault().ToShortDateString(), this.EndDate.GetValueOrDefault().ToShortDateString());
            }

            format = LocalizedStrings.GetLanguageInfoByKey(this.SearchingScope.ToString());
            string source = string.Empty;
            switch (this.SearchingScope)
            {
                case TinyMoneyManager.Component.SearchingScope.Today:
                    source = source.FormatWith(new object[] { System.DateTime.Now.ToLongDateString() });
                    break;

                case TinyMoneyManager.Component.SearchingScope.CurrentWeek:
                case TinyMoneyManager.Component.SearchingScope.CurrentMonth:
                case TinyMoneyManager.Component.SearchingScope.CurrentYear:
                    source = "{0}-{1}".FormatWith(new object[] { this.StartDate.Value.ToShortDateString(), this.EndDate.Value.ToShortDateString() });
                    break;
            }

            return "{0}({1})".FormatWith(new object[] { format, source });
        }

        private string buildTitle()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            string str = this.BuildSearchingScope();
            builder.AppendFormat("{0}{1}{2}", new object[] { str, LocalizedStrings.GetLanguageInfoByKey(this.IncomeOrExpenses.ToString()), LocalizedStrings.GetLanguageInfoByKey("Item") });
            return builder.ToString();
        }

        public System.Collections.IList GetAccountsyEntryiesRelated()
        {
            if (this.AccountIds.Count == 0)
            {
                return null;
            }
            return (from p in ViewModelLocator.AccountViewModel.AccountBookDataContext.Accounts
                    where this.AccountIds.Contains(p.Id)
                    select p).ToList<Account>();
        }

        public System.Collections.IList GetCategoryEntryiesRelated()
        {
            if (this.CategoryIds.Count == 0)
            {
                return null;
            }
            if (this.GroupCategoryMode == CategorySortType.ByChildCategory)
            {
                return (from p in ViewModelLocator.CategoryViewModel.AccountBookDataContext.Categories
                        where this.CategoryIds.Contains(p.Id)
                        select p).ToList<Category>();
            }
            return (from p in ViewModelLocator.CategoryViewModel.AccountBookDataContext.Categories
                    where this.CategoryIds.Contains(p.Id)
                    select p).ToList<Category>();
        }


        public System.Collections.ObjectModel.Collection<Guid> AccountIds { get; private set; }

        public System.Collections.ObjectModel.Collection<Guid> CategoryIds { get; private set; }

        public TinyMoneyManager.Component.ChartGroupMode ChartGroupMode
        {
            get
            {
                return this.chartGroupMode;
            }
            set
            {
                if (value != this.chartGroupMode)
                {
                    this.OnNotifyPropertyChanging("ChartGroupModeProperty");
                    this.chartGroupMode = value;
                    this.OnNotifyPropertyChanged("ChartGroupModeProperty");
                }
            }
        }

        public TinyMoneyManager.Component.DuringMode DuringMode
        {
            get
            {
                return this.duringMode;
            }
            set
            {
                if (value != this.duringMode)
                {
                    this.OnNotifyPropertyChanging("DuringMode");
                    this.duringMode = value;
                    this.OnNotifyPropertyChanged("DuringMode");
                }
            }
        }

        public System.DateTime? EndDate
        {
            get
            {
                return this.endDate;
            }
            set
            {
                if (this.endDate != value)
                {
                    this.OnNotifyPropertyChanging("EndDate");
                    this.endDate = value;
                    this.OnNotifyPropertyChanged("EndDate");
                }
            }
        }

        public CategorySortType GroupCategoryMode
        {
            get
            {
                return this.groupCategoryMode;
            }
            set
            {
                if (value != this.groupCategoryMode)
                {
                    this.OnNotifyPropertyChanging("GroupCategoryModeProperty");
                    this.groupCategoryMode = value;
                    this.OnNotifyPropertyChanged("GroupCategoryModeProperty");
                }
            }
        }

        public ItemType IncomeOrExpenses
        {
            get
            {
                return this.incomeOrExpenses;
            }
            set
            {
                if (this.incomeOrExpenses != value)
                {
                    this.OnNotifyPropertyChanging("IncomeOrExpenses");
                    this.incomeOrExpenses = value;
                    this.OnNotifyPropertyChanged("IncomeOrExpenses");
                }
            }
        }

        public bool? ShowOnlyIsClaim { get; set; }

        public string NotesKey
        {
            get
            {
                return this.notesKey;
            }
            set
            {
                if (this.notesKey != value)
                {
                    this.OnNotifyPropertyChanging("NotesKey");
                    this.notesKey = value;
                    this.OnNotifyPropertyChanged("NotesKey");
                }
            }
        }

        public TinyMoneyManager.Component.SearchingScope SearchingScope
        {
            get
            {
                return this.searchingScope;
            }
            set
            {
                if (this.searchingScope != value)
                {
                    this.OnNotifyPropertyChanging("SearchingScope");
                    this.searchingScope = value;
                    initializeStartDateAndEndDateBySearchScope(this);
                    this.OnNotifyPropertyChanged("SearchingScope");
                    this.OnNotifyPropertyChanged("SearchingScopeIndex");
                }
            }
        }

        public int SearchingScopeIndex
        {
            get
            {
                return (int)this.searchingScope;
            }
        }

        public System.DateTime? StartDate
        {
            get
            {
                return this.startDate;
            }
            set
            {
                if (this.startDate != value)
                {
                    this.OnNotifyPropertyChanging("StartDate");
                    this.startDate = value;
                    this.OnNotifyPropertyChanged("StartDate");
                }
            }
        }

        public string SummaryTitle
        {
            get
            {
                string str = this.buildTitle();
                this.OnNotifyPropertyChanged("SummaryTitle");
                return str;
            }
        }

        /// <summary>
        /// Res the calculate.
        /// </summary>
        /// <param name="searchingContext">The searching context.</param>
        /// <param name="movingMonthDateOffset">The moving month date offset.</param>
        /// <returns></returns>
        public static DetailsCondition ReCalculate(DetailsCondition searchingContext, SearchingScope scope, DateTime movingMonthStartDateOffset, DateTime movingMonthEndDateOffset)
        {
            initializeStartDateAndEndDateBySearchScope(searchingContext, scope, movingMonthStartDateOffset, movingMonthEndDateOffset);

            return searchingContext;
        }

        /// <summary>
        /// Res the calculate.
        /// </summary>
        /// <param name="searchingScope">The searching scope.</param>
        /// <param name="movingMonthDateOffset">The moving month date offset.</param>
        /// <returns></returns>
        public static DetailsCondition ReCalculate(SearchingScope searchingScope, DateTime movingMonthDateOffset)
        {
            var searchingContext = new DetailsCondition();
            searchingContext.searchingScope = searchingScope;

            initializeStartDateAndEndDateBySearchScope(searchingContext, searchingScope, movingMonthDateOffset);

            return searchingContext;
        }

        /// <summary>
        /// Initializes the start date and end date by search scope.
        /// </summary>
        /// <param name="calculatingContext">The calculating context.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="startDateOffset">The start date offset.</param>
        /// <param name="endDateOffset">The end date offset.</param>
        private static void initializeStartDateAndEndDateBySearchScope(DetailsCondition calculatingContext, SearchingScope? scope = null, DateTime? startDateOffset = null, DateTime? endDateOffset = null)
        {
            if (startDateOffset == null)
            {
                startDateOffset = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
            }
            if (endDateOffset == null)
            {
                endDateOffset = startDateOffset.Value.GetLastDayOfMonth();
            }

            var offsetStartDate = startDateOffset.Value;
            var offsetEndDate = endDateOffset.Value;

            int startDay, startMonth, startYear;
            int endDay, endMonth, endYear;

            var isCustomized = AppSetting.Instance.BudgetStatsicSettings.BudgetStatsicMode == BudgetStatsicMode.BudgetStaticModeOfCustomized;

            var needToCalculateAgagin = true;

            if (startDateOffset == null && endDateOffset == null &&
                (AppSetting.Instance.BudgetStatsicSettings.BudgetStatsicMode == BudgetStatsicMode.BudgetStaticModeOfCustomized))
            {
                needToCalculateAgagin = false;
                offsetStartDate = AppSetting.Instance.BudgetStatsicSettings.StartDate;
                offsetEndDate = AppSetting.Instance.BudgetStatsicSettings.EndDate;
            }

            startDay = offsetStartDate.Day;
            startMonth = offsetStartDate.Month;
            startYear = offsetStartDate.Year;

            endDay = offsetEndDate.Day;
            endMonth = offsetEndDate.Month;
            endYear = offsetEndDate.Year;

            if (AppSetting.Instance.BudgetStatsicSettings.BudgetStatsicMode == BudgetStatsicMode.BudgetStaticModeOfCustomized)
            {
                needToCalculateAgagin = false;
                startDay = AppSetting.Instance.BudgetStatsicSettings.StartDate.Day;
                endDay = AppSetting.Instance.BudgetStatsicSettings.EndDate.Day;
            }


            if (scope == null && calculatingContext != null)
            {
                scope = calculatingContext.searchingScope;
            }
            else if (scope == null)
            {
                scope = SearchingScope.CurrentMonth;
            }

            var l_searchingScopr = scope;


            if (l_searchingScopr == TinyMoneyManager.Component.SearchingScope.LastMonth)
            {
                // ---> 2012.11.14.00.00 ---2012.12.14, 
                // ---> 2012.12.13.59.59 ---2013.1.15.  

                //  2012.11.15-2012.12.14
                //  2012.12.15-2013.1.14
                //  2013.1.15 - 2013.2.14.

                // --> 
                if (startMonth == 1)
                {
                    startMonth = 12;
                    startYear = startYear - 1;
                }
                else
                {
                    startMonth--;
                }

                if (endMonth == 1)
                {
                    endMonth = 12;
                    endYear = endYear - 1;
                }
                else
                {
                    endMonth--;
                }

                var maxDayOfStartDate = new DateTime(startYear, startMonth, 1).GetLastDayOfMonth().Day;
                var maxDayOfEndDate = new DateTime(endYear, endMonth, 1).GetLastDayOfMonth().Day;

                if (maxDayOfEndDate < endDay || !isCustomized)
                {
                    endDay = maxDayOfEndDate;
                }

                if (maxDayOfStartDate < startDay)
                {
                    startDay = maxDayOfStartDate;
                }

                offsetStartDate = new System.DateTime(startYear, startMonth, startDay, 0, 0, 0);
                offsetEndDate = new System.DateTime(endYear, endMonth, endDay, 23, 59, 59);
            }
            else if (l_searchingScopr == Component.SearchingScope.NextMonth)
            {
                if (startMonth == 12)
                {
                    startMonth = 1;
                    startYear = startYear + 1;
                }
                else
                {
                    startMonth++;
                }

                if (endMonth == 12)
                {
                    endMonth = 1;
                    endYear = endYear + 1;
                }
                else
                {
                    endMonth++;
                }

                var maxDayOfStartDate = new DateTime(startYear, startMonth, 1).GetLastDayOfMonth().Day;
                var maxDayOfEndDate = new DateTime(endYear, endMonth, 1).GetLastDayOfMonth().Day;

                if (maxDayOfEndDate < endDay || !isCustomized)
                {
                    endDay = maxDayOfEndDate;
                }
                if (maxDayOfStartDate < startDay)
                {
                    startDay = maxDayOfStartDate;
                }

                offsetStartDate = new System.DateTime(startYear, startMonth, startDay, 0, 0, 0);
                offsetEndDate = new System.DateTime(endYear, endMonth, endDay, 23, 59, 59);
            }
            else if (l_searchingScopr == TinyMoneyManager.Component.SearchingScope.CurrentWeek)
            {
                offsetStartDate = DateTime.Now;

                offsetStartDate = offsetStartDate.GetDateTimeOfFisrtDayOfWeek();
                offsetEndDate = offsetStartDate.AddDays(7.0).Date;
            }
            else if (l_searchingScopr == TinyMoneyManager.Component.SearchingScope.CurrentMonth)
            {
                if (needToCalculateAgagin)
                {
                    offsetStartDate = System.DateTime.Now.Date.GetFirstDayOfMonth();
                    offsetEndDate = System.DateTime.Now.Date.GetLastDayOfMonth();
                }
                else
                {
                    var maxDayOfStartDate = new DateTime(startYear, startMonth, 1).GetLastDayOfMonth().Day;
                    var maxDayOfEndDate = new DateTime(endYear, endMonth, 1).GetLastDayOfMonth().Day;

                    if (maxDayOfEndDate < endDay || !isCustomized)
                    {
                        endDay = maxDayOfEndDate;
                    }
                    if (maxDayOfStartDate < startDay)
                    {
                        startDay = maxDayOfStartDate;
                    }

                    if (DateTime.Now.Day >= startDay)
                    {
                        // 2013.1.15
                        // 2013.2.14
                        offsetStartDate = new DateTime(startYear, startMonth, startDay);

                        if (endMonth == 12)
                        {
                            endMonth = 1;
                            endYear = endYear + 1;
                        }

                        offsetEndDate = new DateTime(endYear, endMonth, endDay);
                    }
                    else
                    {
                        // 2012.12.15,
                        // 2013.1.14.
                        if (endMonth == 1)
                        {
                            startMonth = 12;
                            startYear = endYear - 1;
                        }

                        offsetStartDate = new DateTime(startYear, startMonth, startDay);

                        offsetEndDate = new DateTime(endYear, endMonth, endDay);
                    }
                }
            }
            else if (l_searchingScopr == TinyMoneyManager.Component.SearchingScope.CurrentYear)
            {
                offsetStartDate = new System.DateTime(System.DateTime.Now.Year, 1, 1, 0, 0, 0);
                offsetEndDate = new System.DateTime(System.DateTime.Now.Year, 12, 31, 23, 59, 59);
            }

            calculatingContext.StartDate = offsetStartDate;
            calculatingContext.EndDate = offsetEndDate;

        }

    }
}

