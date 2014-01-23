namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    /// <summary>
    /// 
    /// </summary>
    public class AnalyziationSummaryEntry : MultipleThreadSupportedNotionObject
    {
        /// <summary>
        /// 
        /// </summary>
        private decimal amount;

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount
        {
            get { return amount; }
            set
            {
                if (amount != value)
                {
                    OnNotifyPropertyChanging("Amount");
                    amount = value;
                    OnNotifyPropertyChanged("Amount");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string amountStringWithCurrencySymbol;

        /// <summary>
        /// Gets or sets the amount info with currency symbol.
        /// </summary>
        /// <value>
        /// The amount info with currency symbol.
        /// </value>
        public string AmountInfoWithCurrencySymbol
        {
            get { return amountStringWithCurrencySymbol; }
            set
            {
                if (amountStringWithCurrencySymbol != value)
                {
                    OnNotifyPropertyChanging("AmountInfoWithCurrencySymbol");
                    amountStringWithCurrencySymbol = value;
                    OnNotifyPropertyChanged("AmountInfoWithCurrencySymbol");
                }
            }
        }

        /// <summary>
        /// Gets or sets the comparation info.
        /// </summary>
        /// <value>
        /// The comparation info.
        /// </value>
        public AccountBookSummaryCompareInfo ComparationInfo { get; set; }

        /// <summary>
        /// Updatings the specified amount of expense to compare with scope.
        /// </summary>
        /// <param name="amountToCompareWithScope">The amount to compare with scope.</param>
        public void UpdatingComparingInfo(decimal amountToCompareWithScope)
        {
            this.ComparationInfo.CompareWith(amountToCompareWithScope);
        }

        private bool showCompareInfo;
        public bool ShowCompareInfo
        {
            get
            {
                return AppSetting.Instance.ShowAssociatedAccountItemSummary && ComparationInfo.HasCompareInfo;
            }
            set
            {
                if (showCompareInfo != value)
                {
                    showCompareInfo = value;
                    InitializeCompareInfo();
                    OnNotifyPropertyChanged("ShowCompareInfo");
                }
            }
        }

        private void InitializeCompareInfo()
        {
            if (ComparationInfo == null)
                ComparationInfo = new AccountBookSummaryCompareInfo();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyziationSummaryEntry"/> class.
        /// </summary>
        /// <param name="summaryItemType">Type of the summary item.</param>
        public AnalyziationSummaryEntry(ItemType summaryItemType)
        {
            this.ShowCompareInfo = false;
            InitializeCompareInfo();
            ComparationInfo.ItemType = summaryItemType;
        }
    }

}

