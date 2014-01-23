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

        public string BuildeOnlySearchingScope()
        {
            string format = "{0} ~ {1}";
            if (this.SearchingScope == TinyMoneyManager.Component.SearchingScope.All)
            {
                return LocalizedStrings.GetLanguageInfoByKey("All");
            }
            format = string.Format(format, this.StartDate.GetValueOrDefault().ToShortDateString(), this.EndDate.GetValueOrDefault().ToShortDateString());
            if (this.SearchingScope != TinyMoneyManager.Component.SearchingScope.Today)
            {
                return string.Format("{0}({1})", LocalizedStrings.GetLanguageInfoByKey(this.SearchingScope.ToString()), format);
            }
            if (this.searchingScope != TinyMoneyManager.Component.SearchingScope.Customize)
            {
                format = LocalizedStrings.GetLanguageInfoByKey(this.SearchingScope.ToString()) + "(" + System.DateTime.Now.Date.ToLongDateString() + ")";
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

        private void initializeStartDateAndEndDateBySearchScope()
        {
            System.DateTime date = System.DateTime.Now.Date;
            System.DateTime lastDayOfMonth = date;
            if (this.searchingScope == TinyMoneyManager.Component.SearchingScope.LastMonth)
            {
                System.DateTime time3 = date.AddMonths(-1);
                date = new System.DateTime(time3.Year, time3.Month, 1, 0, 0, 0);
                time3 = date.AddMonths(1).AddDays(-1.0).Date;
                lastDayOfMonth = new System.DateTime(time3.Year, time3.Month, time3.Date.Day, 0x17, 0x3b, 0x3b);
            }
            if (this.searchingScope == TinyMoneyManager.Component.SearchingScope.CurrentWeek)
            {
                date = date.GetDateTimeOfFisrtDayOfWeek();
                lastDayOfMonth = date.AddDays(7.0).Date;
            }
            else if (this.searchingScope == TinyMoneyManager.Component.SearchingScope.CurrentMonth)
            {
                date = System.DateTime.Now.Date.GetFirstDayOfMonth();
                lastDayOfMonth = System.DateTime.Now.Date.GetLastDayOfMonth();
            }
            else if (this.searchingScope == TinyMoneyManager.Component.SearchingScope.CurrentYear)
            {
                date = new System.DateTime(System.DateTime.Now.Year, 1, 1);
                lastDayOfMonth = new System.DateTime(System.DateTime.Now.Year + 1, 1, 1, 0x17, 0x3b, 0x3b);
            }
            this.StartDate = new System.DateTime?(date);
            this.EndDate = new System.DateTime?(lastDayOfMonth);
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
                    this.initializeStartDateAndEndDateBySearchScope();
                    this.OnNotifyPropertyChanged("SearchingScope");
                    this.OnNotifyPropertyChanged("SearchingScopeIndex");
                }
            }
        }

        public int SearchingScopeIndex
        {
            get
            {
                return (int) this.searchingScope;
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
    }
}

