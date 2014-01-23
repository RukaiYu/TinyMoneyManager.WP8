namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class AnalyziationSummarization : MultipleThreadSupportedNotionObject
    {
        public AnalyziationSummaryEntry IncomeSummaryEntry { get; set; }

        public AnalyziationSummaryEntry ExpenseSummaryEntry { get; set; }

        public SearchingScope ScopeForSummary { get; set; }

        private string _amountInfo;
        public string AmountInfo
        {
            get { return _amountInfo; }
            set
            {
                _amountInfo = value;
                OnNotifyPropertyChanged("AmountInfo");
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnNotifyPropertyChanged("Title");
            }
        }

        private string _spliter;
        public string Spliter
        {
            get { return _spliter; }
            set
            {
                _spliter = value;
                OnNotifyPropertyChanged("Spliter");
            }
        }

        private bool showCompareInfo;
        public bool ShowCompareInfo
        {
            get
            {
                return AppSetting.Instance.ShowAssociatedAccountItemSummary && showCompareInfo;
            }
            set
            {
                if (showCompareInfo != value)
                {
                    showCompareInfo = value;
                    OnNotifyPropertyChanged("ShowCompareInfo");
                }
            }
        }

        public bool IncomeAmountInfoVisibility { get; set; }

        public AnalyziationSummarization(string spliter = "/")
        {
            this.Spliter = spliter;
            this.ShowCompareInfo = false;
            InitializeEntries();
        }

        /// <summary>
        /// Initializes the compare info.
        /// </summary>
        public void InitializeEntries()
        {
            IncomeSummaryEntry = new AnalyziationSummaryEntry(ItemType.Income);
            ExpenseSummaryEntry = new AnalyziationSummaryEntry(ItemType.Expense);
            this.IncomeAmountInfoVisibility = true;
        }

        /// <summary>
        /// Updatings the specified amount of expense to compare with scope.
        /// </summary>
        /// <param name="amountOfExpenseToCompareWithScope">The amount of expense to compare with scope.</param>
        /// <param name="amountOfIncomeToCompareWithScope">The amount of income to compare with scope.</param>
        public void UpdatingComparingInfo(decimal amountOfExpenseToCompareWithScope, decimal amountOfIncomeToCompareWithScope)
        {
            IncomeSummaryEntry.ComparationInfo.CompareAmount = amountOfIncomeToCompareWithScope;
            ExpenseSummaryEntry.ComparationInfo.CompareAmount = amountOfExpenseToCompareWithScope;

            Updating();
        }

        public void Updating()
        {
            IncomeSummaryEntry.ComparationInfo.CompareWith(TotalIncomeAmount);
            ExpenseSummaryEntry.ComparationInfo.CompareWith(TotalExpenseAmount);

            IncomeSummaryEntry.ComparationInfo.ToggleComparation();
            ExpenseSummaryEntry.ComparationInfo.ToggleComparation();
        }

        private string totalIncomeAmountInfo;

        public string TotalIncomeAmountInfo
        {
            get { return totalIncomeAmountInfo; }
            set
            {
                if (totalIncomeAmountInfo != value)
                {
                    OnNotifyPropertyChanging("TotalIncomeAmountInfo");
                    totalIncomeAmountInfo = value;
                    OnNotifyPropertyChanged("TotalIncomeAmountInfo");
                }
            }
        }

        private decimal totalIncomeAmount;
        public decimal TotalIncomeAmount
        {
            get { return totalIncomeAmount; }
            set
            {
                totalIncomeAmount = value;
                OnNotifyPropertyChanged("TotalIncomeAmount");
                TotalIncomeAmountInfo = AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol(value);
            }
        }

        private string totalExpenseAmountInfo;

        public string TotalExpenseAmountInfo
        {
            get
            {
                return totalExpenseAmountInfo;
            }
            set
            {
                if (totalExpenseAmountInfo != value)
                {
                    OnNotifyPropertyChanging("TotalExpenseAmountInfo");
                    totalExpenseAmountInfo = value;
                    OnNotifyPropertyChanged("TotalExpenseAmountInfo");
                }
            }
        }

        private decimal totalExpenseAmount;
        public decimal TotalExpenseAmount
        {
            get { return totalExpenseAmount; }
            set
            {
                totalExpenseAmount = value;
                OnNotifyPropertyChanged("TotalExpenseAmount");
                TotalExpenseAmountInfo = AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol(value);
            }
        }

        /// <summary>
        /// Gets or sets the money info.
        /// </summary>
        /// <value>
        /// The money info.
        /// </value>
        public AccountItemMoney MoneyInfo { get; set; }

    }
}

