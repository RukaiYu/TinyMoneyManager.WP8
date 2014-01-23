namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;

    [Table]
    public class BudgetProject : NotionObject, IMoney
    {
        private EntitySet<BudgetItem> _budgetItems;
        private decimal? amount;
        private System.DateTime? createAt = new System.DateTime?(System.DateTime.Now);
        private int endDay;
        private System.Guid id;
        private string name;
        private string notes;
        private string notesForShow;
        private ProjectTypes projectType;
        private int startDay;
        private decimal? totalAmount;

        public BudgetProject()
        {
            this.EndDay = 0x1f;
            this.startDay = 1;
            this.amount = 0;
            this.name = string.Empty;
            this.notes = string.Empty;
            this.projectType = ProjectTypes.Budget;
            this._budgetItems = new EntitySet<BudgetItem>(new System.Action<BudgetItem>(this.attach_ToDo), new System.Action<BudgetItem>(this.detach_ToDo));
            this.ItemType = TinyMoneyManager.Component.ItemType.Expense;
            this._budgetItems.CollectionChanged += new NotifyCollectionChangedEventHandler(this._budgetItems_CollectionChanged);
        }

        private void _budgetItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateTotalAmount(null);
        }

        private void attach_ToDo(BudgetItem toDo)
        {
            this.OnNotifyPropertyChanging("BudgetItem");
            toDo.BudgetProject = this;
        }

        private void detach_ToDo(BudgetItem toDo)
        {
            this.OnNotifyPropertyChanging("BudgetItem");
            toDo.BudgetProject = null;
        }

        public decimal? GetMoney()
        {
            return new decimal?(this.BudgetItems.Sum<BudgetItem>((System.Func<BudgetItem, Decimal>)(p => p.Money)));
        }

        public string GetNotesForProject(System.Collections.Generic.IEnumerable<BudgetItem> entitySet)
        {
            System.Collections.Generic.IEnumerable<String> source = (from p in entitySet
                                                                     select p.AssociatedCategory into x
                                                                     select x.Name).Take<string>(10);
            if (source.Count<string>() > 0)
            {
                return (source.ToStringLine<string>(",") + "...");
            }
            return string.Empty;
        }

        public void InitAssociatedBudgetItemsSummary()
        {
            this.AssociatedBudgetItemsSummary = this.GetNotesForProject(this._budgetItems);
        }

        public static void UpdateDataContext(DatabaseSchemaUpdater dataBaseUpdater)
        {
            if (dataBaseUpdater != null)
            {
                dataBaseUpdater.AddTable<BudgetProject>();
            }
        }

        public static void UpdateDataContext_At_v190(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<BudgetProject>("Amount");
        }

        public void UpdateTotalAmount(System.Collections.Generic.IEnumerable<BudgetItem> provider = null)
        {
            if (provider == null)
            {
                provider = this.BudgetItems;
            }
            if (provider.Count<BudgetItem>() == 0)
            {
                this.TotalAmount = this.amount;
            }
            else
            {
                this.TotalAmount = new decimal?(provider.Sum<BudgetItem>((System.Func<BudgetItem, Decimal>)(p => p.Amount)));
            }
            decimal? totalAmount = this.totalAmount;
            decimal? amount = this.amount;
            if ((totalAmount.GetValueOrDefault() > amount.GetValueOrDefault()) && (totalAmount.HasValue & amount.HasValue))
            {
                this.Amount = this.totalAmount;
            }
        }

        [Column(CanBeNull = true)]
        public decimal? Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                decimal? amount = this.amount;
                decimal? nullable2 = value;
                if ((amount.GetValueOrDefault() != nullable2.GetValueOrDefault()) || (amount.HasValue != nullable2.HasValue))
                {
                    this.OnNotifyPropertyChanging("Amount");
                    this.amount = value;
                    this.OnNotifyPropertyChanged("Amount");
                    this.OnNotifyPropertyChanged("AmountInfo");
                    this.OnNotifyPropertyChanged("TotalAmountInfo");
                }
            }
        }

        public string AmountInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(new decimal?(this.Amount.GetValueOrDefault()), "{0}{1}");
            }
        }

        [XmlIgnore]
        public string AssociatedBudgetItemsSummary
        {
            get
            {
                return this.notesForShow;
            }
            set
            {
                if (this.notesForShow != value)
                {
                    this.notesForShow = value;
                    this.OnNotifyPropertyChanged("AssociatedBudgetItemsSummary");
                }
            }
        }

        [Association(Storage = "_budgetItems", OtherKey = "ProjectId", ThisKey = "Id"), XmlIgnore]
        public EntitySet<BudgetItem> BudgetItems
        {
            get
            {
                return this._budgetItems;
            }
            set
            {
                this._budgetItems.Assign(value);
            }
        }

        [Column(CanBeNull = true)]
        public System.DateTime? CreateAt
        {
            get
            {
                return this.createAt;
            }
            set
            {
                if (this.createAt != value)
                {
                    this.OnNotifyPropertyChanging("CreateAt");
                    this.createAt = value;
                    this.OnNotifyPropertyChanged("CreateAt");
                }
            }
        }

        [XmlIgnore]
        public CurrencyType Currency { get; set; }

        [Column(CanBeNull = true)]
        public int EndDay
        {
            get
            {
                return this.endDay;
            }
            set
            {
                if (this.endDay != value)
                {
                    this.OnNotifyPropertyChanging("EndDay");
                    this.endDay = value;
                    this.OnNotifyPropertyChanged("EndDay");
                }
            }
        }

        [Column(IsPrimaryKey = true, DbType = "UniqueIdentifier", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public System.Guid Id
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

        [Column(CanBeNull = true)]
        public TinyMoneyManager.Component.ItemType ItemType { get; set; }

        [XmlIgnore]
        public decimal Money { get; set; }

        [Column(CanBeNull = true)]
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

        [Column(CanBeNull = true)]
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

        [Column(CanBeNull = false)]
        public ProjectTypes ProjectType
        {
            get
            {
                return this.projectType;
            }
            set
            {
                if (this.projectType != value)
                {
                    this.OnNotifyPropertyChanging("ProjectType");
                    this.projectType = value;
                    this.OnNotifyPropertyChanged("ProjectType");
                }
            }
        }

        [Column(CanBeNull = true)]
        public int StartDay
        {
            get
            {
                return this.startDay;
            }
            set
            {
                if (this.startDay != value)
                {
                    this.OnNotifyPropertyChanging("StartDay");
                    this.startDay = value;
                    this.OnNotifyPropertyChanged("StartDay");
                }
            }
        }

        [XmlIgnore]
        public decimal? TotalAmount
        {
            get
            {
                if (!this.totalAmount.HasValue || (this.totalAmount == 0M))
                {
                    this.totalAmount = this.GetMoney();
                }
                if ((this.BudgetItems == null) || (this.BudgetItems.Count == 0))
                {
                    this.totalAmount = this.amount;
                }
                return this.totalAmount;
            }
            set
            {
                decimal? nullable = value;
                decimal? totalAmount = this.totalAmount;
                if ((nullable.GetValueOrDefault() != totalAmount.GetValueOrDefault()) || (nullable.HasValue != totalAmount.HasValue))
                {
                    this.totalAmount = value;
                    this.OnNotifyPropertyChanged("TotalAmount");
                    this.OnNotifyPropertyChanged("AmountInfo");
                }
            }
        }

        public string TotalAmountInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(new decimal?(this.TotalAmount.GetValueOrDefault()), "{0}{1}");
            }
        }
    }
}

