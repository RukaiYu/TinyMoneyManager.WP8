namespace TinyMoneyManager.Component
{
    using System;
    using System.Runtime.CompilerServices;

    public class SummaryDetails : NotionObject
    {
        private int count;
        private string name;
        private string notes;
        private decimal? totalAmount;

        public SummaryDetails()
        {
            this.TotalAmout = 0.0M;
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
                    this.OnNotifyPropertyChanged("Name");
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
                this.OnNotifyPropertyChanged("TotalAmout");
                this.OnNotifyPropertyChanged("AmountInfo");
            }
        }
    }
}

