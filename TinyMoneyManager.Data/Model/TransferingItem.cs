namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xml.Serialization;
    using Microsoft.Phone.Data.Linq;
    using TinyMoneyManager.Component;

    [Table]
    public class TransferingItem : NotionObject, IScheduledTaskItem, IMoney
    {
        private static int yearOfCurrent = System.DateTime.Now.Year;

        private EntityRef<Account> _fromAccount;
        private EntityRef<Account> _toAccount;


        private decimal _amount;

        [Column]
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value != _amount)
                {
                    OnNotifyPropertyChanging("AmountInfo");
                    OnNotifyPropertyChanging("Amount");
                    _amount = value;
                    OnNotifyPropertyChanged("Amount");
                    OnNotifyPropertyChanged("AmountInfo");
                }
            }
        }

        private string _amountInfo;

        [XmlIgnore]
        public string AmountInfo
        {
            get
            {
                CurrencyType currency = CurrencyType.CNY;
                if (this.FromAccount == null)
                {
                    currency = this.Currency;
                }
                else
                {
                    currency = this.FromAccount.CurrencyType;
                }

                return AccountItemMoney.GetMoneyInfoWithCurrency(currency, this.Amount) + (this.TransferingPoundageAmount.GetValueOrDefault() > 0 ? ("(" + this.TransferingPoundageAmount + ")") : string.Empty);
            }
        }

        [Column]
        public CurrencyType Currency { get; set; }


        private Guid fromAccountId;

        [Column]
        public Guid FromAccountId
        {
            get { return fromAccountId; }
            set
            {
                if (value != fromAccountId)
                {
                    OnNotifyPropertyChanging("FromAccountId");
                    fromAccountId = value;
                    OnNotifyPropertyChanged("FromAccountId");
                }
            }
        }


        [XmlIgnore]
        public string FromAccountName
        {
            get
            {
                return this.FromAccount == null ? "N/A" : this.FromAccount.Name;
            }
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public string Notes { get; set; }


        private Guid _toAccountId;

        [Column]
        public Guid ToAccountId
        {
            get { return _toAccountId; }
            set
            {
                if (value != _toAccountId)
                {
                    OnNotifyPropertyChanging("ToAccountId");
                    _toAccountId = value;
                    OnNotifyPropertyChanged("ToAccountId");
                }
            }
        }


        [XmlIgnore]
        public string ToAccountName
        {
            get
            {
                return this.ToAccount == null ? "N/A" : this.ToAccount.Name;
            }
        }

        public string TransferDateInfo
        {
            get
            {
                return this.TransferingDate.ToString(ConstString.FormatFullWithoutYear, System.Threading.Thread.CurrentThread.CurrentCulture);
            }
        }

        private Guid _associatedTaskId;
        public static string AssociatedTaskIdProperty = "AssociatedTaskId";

        [Column(CanBeNull = true)]
        public Guid? AssociatedTaskId
        {
            get { return _associatedTaskId; }
            set
            {
                if (value != null && value != _associatedTaskId)
                {
                    OnNotifyPropertyChanging(AssociatedTaskIdProperty);
                    _associatedTaskId = value.Value;
                    OnNotifyPropertyChanged(AssociatedTaskIdProperty);
                }
            }
        }

        private decimal? _transferingPoundageAmount;

        /// <summary>
        /// Gets or sets the transfering poundage amount.
        /// </summary>
        /// <value>
        /// The transfering poundage amount.
        /// </value>
        [Column(CanBeNull = true)]
        public decimal? TransferingPoundageAmount
        {
            get { return _transferingPoundageAmount; }
            set
            {
                if (value != _transferingPoundageAmount)
                {
                    OnNotifyPropertyChanging("TransferingPoundageAmount");
                    _transferingPoundageAmount = value;
                    OnNotifyPropertyChanged("TransferingPoundageAmount");
                    OnNotifyPropertyChanged("AmountInfo");
                }
            }
        }

        [Column]
        public System.DateTime TransferingDate { get; set; }

        /// <summary>
        /// The _ tally schedule task
        /// </summary>
        private EntityRef<TallySchedule> _TallyScheduleTask;
        [XmlIgnore, Association(Storage = "_TallyScheduleTask", ThisKey = "AssociatedTaskId", OtherKey = "Id")]
        public TallySchedule TallyScheduleTask
        {
            get
            {
                return this._TallyScheduleTask.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("_TallyScheduleTask");
                this._TallyScheduleTask.Entity = value;
                if (value != null)
                {
                    bool flag = this.AssociatedTaskId.Value != value.Id;
                    this.AssociatedTaskId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("_TallyScheduleTask");
                    }
                }

                this.OnNotifyPropertyChanged("_TallyScheduleTask");
            }
        }

        /// <summary>
        /// Gets or sets from account.
        /// </summary>
        /// <value>
        /// From account.
        /// </value>
        [Association(Storage = "_fromAccount", ThisKey = "FromAccountId", OtherKey = "Id"), XmlIgnore]
        public Account FromAccount
        {
            get
            {
                return this._fromAccount.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("FromAccount");
                this._fromAccount.Entity = value;
                if (value != null)
                {
                    bool flag = this.FromAccountId != value.Id;
                    this.FromAccountId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("FromAccountName");
                    }
                }
                this.OnNotifyPropertyChanged("FromAccount");
            }
        }

        /// <summary>
        /// Gets or sets to account.
        /// </summary>
        /// <value>
        /// To account.
        /// </value>
        [XmlIgnore, Association(Storage = "_toAccount", ThisKey = "ToAccountId", OtherKey = "Id")]
        public Account ToAccount
        {
            get
            {
                return this._toAccount.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("ToAccount");
                this._toAccount.Entity = value;
                if (value != null)
                {
                    bool flag = this.ToAccountId != value.Id;
                    this.ToAccountId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("ToAccountName");
                    }
                }
                this.OnNotifyPropertyChanged("ToAccount");
            }
        }


        /// <summary>
        /// Updates the structure at_1_9_1.
        /// </summary>
        /// <param name="dataBaseUpdater">The data base updater.</param>
        public static void UpdateStructureAt_1_9_8(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<TransferingItem>("AssociatedTaskId");
            dataBaseUpdater.AddColumn<TransferingItem>("TransferingPoundageAmount");
        }

        public decimal? GetMoney()
        {
            return this.Amount;
        }

        [XmlIgnore]
        public decimal Money
        {
            get;
            set;
        }
    }
}

