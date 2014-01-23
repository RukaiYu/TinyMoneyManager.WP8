namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class ViewModeConfig : NotionObject
    {
        private int customizedSearchingIndex = -1;
        public System.Action<ViewModeConfig> DoCustomizedSearching;
        private DetailsCondition searchingCondition;
        public static readonly string ViewDateTimeProperty = "ViewDateTime";

        public ViewModeConfig()
        {
            this.ViewDateTime = System.DateTime.Now;
            this.Mode = ViewMode.Day;
            this.searchingCondition = new DetailsCondition();
        }

        public bool EnsureData(AccountItem accountItem)
        {
            return accountItem.CreateTime.AtSameYearMonth(this.ViewDateTime);
        }

        public string GetTitle()
        {
            if (this.customizedSearchingIndex != -1)
            {
                return AppResources.Customize;
            }
            return string.Format("{0}/{1}", this.Year, this.Month);
        }

        public void RaiseChange(int? year, int? month)
        {
            if (year.HasValue && month.HasValue)
            {
                this.ViewDateTime = new System.DateTime(year.Value, month.Value, 1);
            }
        }

        public void Searching()
        {
            if (this.DoCustomizedSearching != null)
            {
                this.OnNotifyPropertyChanged(ViewDateTimeProperty);
                this.DoCustomizedSearching(this);
            }
        }

        public int CustomizedSearchingIndex
        {
            get
            {
                return this.customizedSearchingIndex;
            }
            set
            {
                if (this.customizedSearchingIndex != value)
                {
                    this.OnNotifyPropertyChanging("CustomizedSearchingIndex");
                    this.customizedSearchingIndex = value;
                    this.OnNotifyPropertyChanged("CustomizedSearchingIndex");
                }
            }
        }

        public int Day { get; set; }

        public bool HasBindPropertyChangedEvent { get; set; }

        public bool IsCustomized
        {
            get
            {
                return (this.customizedSearchingIndex != -1);
            }
        }

        public TinyMoneyManager.Component.ItemType ItemType { get; set; }

        private string itemTypeTitle
        {
            get
            {
                if (this.ItemType != TinyMoneyManager.Component.ItemType.Expense)
                {
                    return "ExpensesBudgetType";
                }
                return "IncomeBudgetType";
            }
        }

        public ViewMode Mode { get; set; }

        public int Month { get; set; }

        public DetailsCondition SearchingCondition
        {
            get
            {
                return this.searchingCondition;
            }
            set
            {
                if (this.searchingCondition != value)
                {
                    this.OnNotifyPropertyChanging("SearchingCondition");
                    this.searchingCondition = value;
                    this.OnNotifyPropertyChanged("SearchingCondition");
                }
            }
        }

        public System.DateTime ViewDateTime
        {
            get
            {
                return new System.DateTime(this.Year, this.Month, this.Day);
            }
            set
            {
                if (((value.Date.Day != this.Day) || (value.Date.Month != this.Month)) || (value.Year != this.Year))
                {
                    this.Day = value.Day;
                    this.Month = value.Month;
                    this.Year = value.Year;
                    this.OnNotifyPropertyChanged(ViewDateTimeProperty);
                }
            }
        }

        public int Year { get; set; }

        public string YearMonthDate
        {
            get
            {
                return this.ViewDateTime.ToString(LocalizedStrings.CultureName.DateTimeFormat.YearMonthPattern, LocalizedStrings.CultureName);
            }
        }
    }
}

