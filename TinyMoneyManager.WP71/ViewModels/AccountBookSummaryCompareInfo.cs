namespace TinyMoneyManager.ViewModels
{
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class AccountBookSummaryCompareInfo : MultipleThreadSupportedNotionObject
    {
        private string amountInfo;
        private string amountInfoWithArrow;
        private string arrowImageUriString;
        private string compareInfoKey;
        private const string downArrow = "/TinyMoneyManager;component/images/arrow_down.png";
        private string downArrowRelatedTo;
        private const string downRedArrow = "/TinyMoneyManager;component/images/arrow_down_Red.png";
        private bool hasCompareInfo;
        private IncreaseOrDecrease increaseOrDecrease;
        private TinyMoneyManager.Component.ItemType itemType;
        private decimal money;
        private const string rightArrow = "/TinyMoneyManager;component/images/arrow_Right.png";
        private SearchingScope searchingScope;
        private const string upArrow = "/TinyMoneyManager;component/images/arrow_up.png";
        private string upArrowRelatedTo;
        private const string upGreenArrow = "/TinyMoneyManager;component/images/arrow_up_Green.png";

        public AccountBookSummaryCompareInfo()
        {
            this.CompareAmount = 0M;
            this.Amount = 0M;
            this.hasCompareInfo = true;
            this.itemType = TinyMoneyManager.Component.ItemType.All;
        }

        public AccountBookSummaryCompareInfo(TinyMoneyManager.Component.ItemType arg_itemType) : this()
        {
            this.ItemType = arg_itemType;
        }

        public void CalculateBalanceMovingData()
        {
            string str = string.Empty;
            if (this.Amount > 0M)
            {
                str = "↑";
                this.ArrowImageUriString = this.upArrowRelatedTo;
                this.BalanceMoving = IncreaseOrDecrease.Increase;
            }
            else if (this.Amount == 0M)
            {
                str = "→";
                this.ArrowImageUriString = "/TinyMoneyManager;component/images/arrow_Right.png";
                this.BalanceMoving = IncreaseOrDecrease.Hold;
            }
            else if (this.Amount < 0M)
            {
                str = "↓";
                this.ArrowImageUriString = this.downArrowRelatedTo;
                this.BalanceMoving = IncreaseOrDecrease.Decrease;
            }
            this.AmountInfo = System.Math.Abs(this.Amount).ToMoneyF2(LocalizedStrings.CultureName);
            this.AmountInfoWithArrow = "{0}{1}".FormatWith(new object[] { str, this.amountInfo });
        }

        public void CompareWith(decimal compareFromAmount)
        {
            this.Amount = compareFromAmount - this.CompareAmount;
        }

        public void ToggleComparation()
        {
            this.OnNotifyPropertyChanged("HasCompareInfo");
        }

        public decimal Amount
        {
            get
            {
                return this.money;
            }
            set
            {
                if (this.money != value)
                {
                    this.money = value;
                    this.OnNotifyPropertyChanged("Amount");
                    this.CalculateBalanceMovingData();
                    this.OnNotifyPropertyChanged("AmountInfo");
                    this.OnNotifyPropertyChanged("HasCompareInfo");
                }
            }
        }

        public string AmountInfo
        {
            get
            {
                return this.amountInfo;
            }
            private set
            {
                if (this.amountInfo != value)
                {
                    this.amountInfo = value;
                    this.OnNotifyPropertyChanged("AmountInfo");
                }
            }
        }

        public string AmountInfoWithArrow
        {
            get
            {
                return this.amountInfoWithArrow;
            }
            set
            {
                if (this.amountInfoWithArrow != value)
                {
                    this.OnNotifyPropertyChanging("AmountInfoWithArrow");
                    this.amountInfoWithArrow = value;
                    this.OnNotifyPropertyChanged("AmountInfoWithArrow");
                }
            }
        }

        public string ArrowImageUriString
        {
            get
            {
                return this.arrowImageUriString;
            }
            set
            {
                if (this.arrowImageUriString != value)
                {
                    this.arrowImageUriString = value;
                    this.OnNotifyPropertyChanged("ArrowImageUriString");
                }
            }
        }

        public IncreaseOrDecrease BalanceMoving
        {
            get
            {
                return this.increaseOrDecrease;
            }
            set
            {
                if (this.increaseOrDecrease != value)
                {
                    this.increaseOrDecrease = value;
                    this.OnNotifyPropertyChanged("BalanceMoving");
                }
            }
        }

        public string BalanceMovingSymbol
        {
            get
            {
                string str = "→";
                if (this.BalanceMoving == IncreaseOrDecrease.Increase)
                {
                    return "↑";
                }
                if (this.BalanceMoving == IncreaseOrDecrease.Decrease)
                {
                    str = "↓";
                }
                return str;
            }
        }

        public decimal CompareAmount { get; set; }

        public string CompareInfoKey
        {
            get
            {
                return this.compareInfoKey;
            }
            set
            {
                if (this.compareInfoKey != value)
                {
                    this.compareInfoKey = value;
                    this.OnNotifyPropertyChanged("CompareInfoKey");
                }
            }
        }

        public bool HasCompareInfo
        {
            get
            {
                return (AppSetting.Instance.ShowAssociatedAccountItemSummary && (this.Amount != 0M));
            }
            private set
            {
                if (this.hasCompareInfo != value)
                {
                    this.hasCompareInfo = value;
                    this.OnNotifyPropertyChanged("HasCompareInfo");
                }
            }
        }

        public TinyMoneyManager.Component.ItemType ItemType
        {
            get
            {
                return this.itemType;
            }
            set
            {
                if (this.itemType != value)
                {
                    this.itemType = value;
                    if (this.itemType == TinyMoneyManager.Component.ItemType.Income)
                    {
                        this.upArrowRelatedTo = "/TinyMoneyManager;component/images/arrow_up_Green.png";
                        this.downArrowRelatedTo = "/TinyMoneyManager;component/images/arrow_down_Red.png";
                    }
                    else if (this.itemType == TinyMoneyManager.Component.ItemType.Expense)
                    {
                        this.upArrowRelatedTo = "/TinyMoneyManager;component/images/arrow_up.png";
                        this.downArrowRelatedTo = "/TinyMoneyManager;component/images/arrow_down.png";
                    }
                    this.OnNotifyPropertyChanged("ItemType");
                }
            }
        }

        public SearchingScope Scope
        {
            get
            {
                return this.searchingScope;
            }
            set
            {
                if (this.searchingScope != value)
                {
                    this.OnNotifyPropertyChanging("Scope");
                    this.searchingScope = value;
                    this.OnNotifyPropertyChanged("Scope");
                }
            }
        }
    }
}

