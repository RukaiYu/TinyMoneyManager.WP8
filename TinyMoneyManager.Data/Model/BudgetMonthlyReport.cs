namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using System;
    using System.Data.Linq.Mapping;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;

    [Table]
    public class BudgetMonthlyReport : NotionObject
    {
        private System.Guid budgetProjectId;
        private int id;
        private TinyMoneyManager.Component.ItemType itemType;
        private decimal money;
        private int month;
        private int year;

        public decimal? GetMoney()
        {
            return new decimal?(this.Amount);
        }

        public static void UpdateUpdateDataContext(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddTable<BudgetMonthlyReport>();
        }

        [Column(CanBeNull=true)]
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
                    this.OnNotifyPropertyChanging("Amount");
                    this.money = value;
                    this.OnNotifyPropertyChanged("Amount");
                }
            }
        }

        [Column(CanBeNull=true)]
        public System.Guid BudgetProjectId
        {
            get
            {
                return this.budgetProjectId;
            }
            set
            {
                if (this.budgetProjectId != value)
                {
                    this.OnNotifyPropertyChanging("BudgetProjectId");
                    this.budgetProjectId = value;
                    this.OnNotifyPropertyChanged("BudgetProjectId");
                }
            }
        }

        [XmlIgnore]
        public CurrencyType Currency { get; set; }

        [Column(IsPrimaryKey=true, IsDbGenerated=true, DbType="INT NOT NULL Identity", CanBeNull=false, AutoSync=AutoSync.OnInsert)]
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

        [Column(CanBeNull=true)]
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
                    this.OnNotifyPropertyChanging("ItemType");
                    this.itemType = value;
                    this.OnNotifyPropertyChanged("ItemType");
                }
            }
        }

        [Column(CanBeNull=true)]
        public int Month
        {
            get
            {
                return this.month;
            }
            set
            {
                if (this.month != value)
                {
                    this.OnNotifyPropertyChanging("Month");
                    this.month = value;
                    this.OnNotifyPropertyChanged("Month");
                }
            }
        }

        [Column(CanBeNull=true)]
        public int Year
        {
            get
            {
                return this.year;
            }
            set
            {
                if (this.year != value)
                {
                    this.OnNotifyPropertyChanging("Year");
                    this.year = value;
                    this.OnNotifyPropertyChanged("Year");
                }
            }
        }
    }
}

