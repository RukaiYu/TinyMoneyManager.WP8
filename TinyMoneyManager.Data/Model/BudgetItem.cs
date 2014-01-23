namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;

    [Table]
    public class BudgetItem : NotionObject, IMoney
    {
        private EntityRef<Category> _associatedCategory;
        private EntityRef<TinyMoneyManager.Data.Model.BudgetProject> _budgetProject;
        private int _id;
        private decimal amount;
        private TinyMoneyManager.Data.Model.BudgetType budgetItemType;
        private System.Guid budgetTargetId;
        private ItemType budgetType;
        private System.Guid projectId;
        private DuringMode? settleType;

        public BudgetItem()
        {
            this.BudgetType = ItemType.Expense;
            this.BudgetItemType = TinyMoneyManager.Data.Model.BudgetType.Category;
        }

        public decimal? GetMoney()
        {
            return this.BudgetProject.TotalAmount;
        }

        public static void UpdateDataContext(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddTable<BudgetItem>();
        }

        public static void UpdateDataContextAt_190(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<BudgetItem>("SettleType");
        }

        [Column]
        public decimal Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                if (this.amount != value)
                {
                    this.OnNotifyPropertyChanging("Amount");
                    this.amount = value;
                    this.OnNotifyPropertyChanged("Amount");
                    this.OnNotifyPropertyChanged("AmountInfo");
                }
            }
        }

        [XmlIgnore]
        public string AmountInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(new decimal?(this.amount), "{0}{1}");
            }
        }

        [Association(Storage = "_associatedCategory", ThisKey = "BudgetTargetId", OtherKey = "Id"), XmlIgnore]
        public Category AssociatedCategory
        {
            get
            {
                return this._associatedCategory.Entity;
            }
            set
            {
                if ((value != null) && (value.Id != this.BudgetTargetId))
                {
                    this._associatedCategory.Entity = value;
                    this.BudgetTargetId = value.Id;
                    this.OnNotifyPropertyChanged("AssociatedCategory");
                }
            }
        }

        [Column]
        public TinyMoneyManager.Data.Model.BudgetType BudgetItemType
        {
            get
            {
                return this.budgetItemType;
            }
            set
            {
                if (this.budgetItemType != value)
                {
                    this.OnNotifyPropertyChanging("BudgetItemType");
                    this.budgetItemType = value;
                    this.OnNotifyPropertyChanged("BudgetItemType");
                }
            }
        }

        [XmlIgnore, Association(Storage = "_budgetProject", ThisKey = "ProjectId", OtherKey = "Id")]
        public TinyMoneyManager.Data.Model.BudgetProject BudgetProject
        {
            get
            {
                return this._budgetProject.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("BudgetProject");
                this._budgetProject.Entity = value;
                if (value != null)
                {
                    this.projectId = value.Id;
                }
                this.OnNotifyPropertyChanged("BudgetProject");
            }
        }

        [Column]
        public System.Guid BudgetTargetId
        {
            get
            {
                return this.budgetTargetId;
            }
            set
            {
                if (this.budgetTargetId != value)
                {
                    this.OnNotifyPropertyChanging("BudgetTargetId");
                    this.budgetTargetId = value;
                    this.OnNotifyPropertyChanged("BudgetTargetId");
                }
            }
        }

        [Column]
        public ItemType BudgetType
        {
            get
            {
                return this.budgetType;
            }
            set
            {
                if (this.budgetType != value)
                {
                    this.OnNotifyPropertyChanging("BudgetType");
                    this.budgetType = value;
                    this.OnNotifyPropertyChanged("BudgetType");
                }
            }
        }

        [XmlIgnore]
        public CurrencyType Currency { get; set; }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this.OnNotifyPropertyChanging("Id");
                this._id = value;
                this.OnNotifyPropertyChanged("Id");
            }
        }

        [XmlIgnore]
        public decimal Money
        {
            get
            {
                return this.Amount;
            }
            set
            {
                this.Amount = value;
            }
        }

        [Column]
        public System.Guid ProjectId
        {
            get
            {
                return this.projectId;
            }
            set
            {
                if (this.projectId != value)
                {
                    this.OnNotifyPropertyChanging("ProjectId");
                    this.projectId = value;
                    this.OnNotifyPropertyChanged("ProjectId");
                }
            }
        }

        [Column(CanBeNull = true)]
        public DuringMode? SettleType
        {
            get
            {
                return this.settleType;
            }
            set
            {
                if (this.settleType != value)
                {
                    this.OnNotifyPropertyChanging("SettleType");
                    this.settleType = value;
                    this.OnNotifyPropertyChanged("SettleType");
                }
            }
        }

        public int SettleTypeIndex
        {
            get
            {
                return (int)this.settleType.GetValueOrDefault();
            }
        }
    }
}

