namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Data.Linq.Mapping;
    using TinyMoneyManager.Component;

    [Table]
    public class CurrencyRateTable : NotionObject
    {
        private System.Guid fromCurrencyId;
        private int id;
        private double rate;
        private System.Guid toCurrencyId;

        public System.Guid FromCurrencyId
        {
            get
            {
                return this.fromCurrencyId;
            }
            set
            {
                if (this.fromCurrencyId != value)
                {
                    this.OnNotifyPropertyChanging("FromCurrencyId");
                    this.fromCurrencyId = value;
                    this.OnNotifyPropertyChanged("FromCurrencyId");
                }
            }
        }

        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                if (this.id != value)
                {
                    this.OnNotifyPropertyChanging("Id");
                    this.id = value;
                    this.OnNotifyPropertyChanged("Id");
                }
            }
        }

        public double Rate
        {
            get
            {
                return this.rate;
            }
            set
            {
                if (this.rate != value)
                {
                    this.OnNotifyPropertyChanging("Rate");
                    this.rate = value;
                    this.OnNotifyPropertyChanged("Rate");
                }
            }
        }

        public System.Guid ToCurrencyId
        {
            get
            {
                return this.toCurrencyId;
            }
            set
            {
                if (this.toCurrencyId != value)
                {
                    this.OnNotifyPropertyChanging("ToCurrencyId");
                    this.toCurrencyId = value;
                    this.OnNotifyPropertyChanged("ToCurrencyId");
                }
            }
        }
    }
}

