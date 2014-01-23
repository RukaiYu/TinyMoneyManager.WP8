namespace TinyMoneyManager.Component
{
    using System;
    using System.Runtime.CompilerServices;

    using NkjSoft.Extensions;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.ViewModels.Common;
    public class SummaryDetails : NotionObject, IMoney
    {
        private int count;
        private string name;
        private string notes;
        private decimal? totalAmount;

        public SummaryDetails()
        {
            this.TotalAmout = 0.0M;

            this.Currency = AppSetting.Instance.DefaultCurrency;
        }

        public ItemType AccountItemType { get; set; }

        public string AmountInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(new decimal?(this.totalAmount.GetValueOrDefault()), "{0}{1}");
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
                this.OnNotifyPropertyChanged("Count");
            }
        }

        public System.DateTime Date { get; set; }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name != value)
                {
                    this.OnNotifyPropertyChanging("Name");
                    this.name = value;
                    this.OnAsyncNotifyPropertyChanged("Name");
                }
            }
        }

        public string Notes
        {
            get
            {
                return this.notes;
            }
            set
            {
                if (this.notes != value)
                {
                    this.OnNotifyPropertyChanging("Notes");
                    this.notes = value;
                    this.OnNotifyPropertyChanged("Notes");
                }
            }
        }

        public object Tag { get; set; }

        public decimal? TotalAmout
        {
            get
            {
                return this.totalAmount;
            }
            set
            {
                this.totalAmount = value;
                this.OnAsyncNotifyPropertyChanged("TotalAmout");
                this.OnAsyncNotifyPropertyChanged("AmountInfo");
            }
        }

        /// <summary>
        /// Gets the total amout info.
        /// </summary>
        /// <value>
        /// The total amout info.
        /// </value>
        public virtual string TotalAmoutInfo
        {
            get { return TotalAmout.GetValueOrDefault().ToMoneyF2(); }
        }

        public virtual decimal? GetMoney()
        {
            return this.TotalAmout;
        }

        public CurrencyType Currency
        {
            get;
            set;
        }

        public decimal Money
        {
            get
            {
                return this.totalAmount.GetValueOrDefault();
            }
            set
            {
                this.totalAmount = value;
                OnAsyncNotifyPropertyChanged("Money");
            }
        }
    }

    public class NetSummaryDetails : SummaryDetails
    {
        public override string TotalAmoutInfo
        {
            get
            {
                //if (AccountItemType == ItemType.Expense && TotalAmout.GetValueOrDefault() > decimal.Zero)
                //{
                //    return (0 - TotalAmout.GetValueOrDefault()).ToMoneyF2();
                //}

                return base.TotalAmoutInfo;
            }
        }

        /// <summary>
        /// Gets the money.
        /// </summary>
        /// <returns></returns>
        public override decimal? GetMoney()
        {
            if (this.AccountItemType == ItemType.Expense)
            {
                if (this.TotalAmout.GetValueOrDefault() > decimal.Zero)
                {
                    return (0 - this.TotalAmout.GetValueOrDefault());
                }
            }

            return base.GetMoney();
        }

    }
}

