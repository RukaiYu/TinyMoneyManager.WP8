namespace TinyMoneyManager.Data.Model
{
    using RapidRepository;
    using System;
    using System.Linq;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using NkjSoft.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using Microsoft.Phone.Data.Linq;

    [Table]
    public partial class Account : NotionObject, IMoney, IRapidEntity, IOrderable
    {
        private EntitySet<AccountItem> _accountItems;
        private AccountCategory _category;
        [XmlIgnore]
        public static string accentColor;
        private static System.Func<AccountCategory, String> accountCategoryLocalizationStringGetter;
        private decimal? balance;
        private string balanceInfo;
        private string color;
        private TinyMoneyManager.Component.CurrencyType currencyType;
        private string currencyTypeSymbol;
        private CurrencyWapper currencyWapper;
        [XmlIgnore]
        public static string foregroundColor;
        private System.Guid id;
        private decimal? initialBlance;
        private System.DateTime? initialDateTime;
        private bool isDefaultAccount;
        private string name;
        private decimal poundage;
        private bool stasticBlance;

        public Account()
            : this(string.Empty)
        {
        }

        public Account(string name)
        {
            this.Order = 0;
            this.name = name;
            this.CanBeDeleted = true;
            this.stasticBlance = true;
            this.CanChangeCategory = true;
            this.balance = 0.0M;
            this.initialBlance = 0.0M;
            this._accountItems = new EntitySet<AccountItem>(new System.Action<AccountItem>(this.attach_ToDo), new System.Action<AccountItem>(this.detach_ToDo));
        }

        private void attach_ToDo(AccountItem accountItem)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            accountItem.Account = this;
        }

        public Account Clone()
        {
            return new Account { Id = this.Id, name = this.name, balance = this.balance, CanBeDeleted = this.CanBeDeleted, CanChangeCategory = this.CanChangeCategory, color = this.color, Category = this.Category, currencyType = this.currencyType, poundage = this.poundage, initialBlance = this.initialBlance, initialDateTime = this.initialDateTime, stasticBlance = this.stasticBlance };
        }

        private void detach_ToDo(AccountItem accountItem)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            accountItem.Account = null;
        }

        /// <summary>
        /// Gets the money.
        /// </summary>
        /// <returns></returns>
        public decimal? GetMoney()
        {
            if (this.IsCreditCard && !AppSetting.Instance.IncludeCreditCardAmountOnAsset)
            {
                return 0.0M;
            }

            return new decimal?(CurrencyHelper.GetGlobleMoneyFrom(this.currencyType, this.Money));
        }

        public void RaiseNameAndColorChanged()
        {
            this.OnNotifyPropertyChanged("Name");
            this.OnNotifyPropertyChanged("Color");
        }

        public void RestoreFrom(Account fromAccount)
        {
            this.Balance = fromAccount.balance;
            this.CanBeDeleted = fromAccount.CanBeDeleted;
            this.CanChangeCategory = fromAccount.CanChangeCategory;
            this.Color = fromAccount.Color;
            this.Category = fromAccount.Category;
            this.CurrencyType = fromAccount.CurrencyType;
            this.InitialBalance = fromAccount.initialBlance;
            this.Poundage = fromAccount.Poundage;
            this.initialDateTime = fromAccount.initialDateTime;
            this.Name = fromAccount.name;
            this.StasticsBlance = fromAccount.StasticsBlance;
        }

        public override string ToString()
        {
            return this.Color;
        }

        [XmlIgnore]
        public static System.Func<AccountCategory, String> AccountCategoryLocalizationStringGetter
        {
            get
            {
                if (accountCategoryLocalizationStringGetter == null)
                {
                    accountCategoryLocalizationStringGetter = category => LocalizedObjectHelper.GetLocalizedStringFrom(category.ToString());
                }
                return accountCategoryLocalizationStringGetter;
            }
        }

        [Association(Storage = "_accountItems", OtherKey = "AccountId", ThisKey = "Id"), XmlIgnore]
        public EntitySet<AccountItem> AccountItems
        {
            get
            {
                return this._accountItems;
            }
            set
            {
                this._accountItems.Assign(value);
            }
        }

        [Column(CanBeNull = true)]
        public decimal? Balance
        {
            get
            {
                return this.balance;
            }
            set
            {
                decimal? nullable = value;
                decimal? balance = this.balance;
                if ((nullable.GetValueOrDefault() != balance.GetValueOrDefault()) || (nullable.HasValue != balance.HasValue))
                {
                    this.OnNotifyPropertyChanging("Balance");
                    this.balance = value;
                    this.OnNotifyPropertyChanged("Balance");
                    this.balanceInfo = string.Empty;
                    this.OnNotifyPropertyChanged("BalanceInfo");
                    this.OnNotifyPropertyChanged("BalanceInfoWithoutTag");
                }
            }
        }

        [XmlIgnore]
        public string BalanceInfo
        {
            get
            {
                if (string.IsNullOrEmpty(this.balanceInfo))
                {
                    this.balanceInfo = AccountBalanceInfoConvert.GetBalanceInfo(this);
                }
                return this.balanceInfo;
            }
            private set
            {
                this.OnNotifyPropertyChanging("BalanceInfo");
                this.balanceInfo = value;
                this.OnNotifyPropertyChanged("BalanceInfo");
            }
        }

        public string BalanceInfoWithoutTag
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(this.CurrencyType, this.Balance, "{0}{1}");
            }
        }

        [Column]
        public bool CanBeDeleted { get; set; }

        [Column]
        public bool CanChangeCategory { get; set; }

        [Column]
        public AccountCategory Category
        {
            get
            {
                return this._category;
            }
            set
            {
                this.OnNotifyPropertyChanging("Category");
                this._category = value;
                this.OnNotifyPropertyChanged("Category");
                this.OnNotifyPropertyChanged("CategoryName");
                this.OnNotifyPropertyChanged("NameInfo");
                this.OnNotifyPropertyChanged("NeedPoundage");
                this.OnNotifyPropertyChanged("CategoryColor");
                this.OnNotifyPropertyChanged("IsCreditCard");
            }
        }

        public string CategoryColor
        {
            get
            {
                return AccountCategoryHelper.GetWapperByType(this.Category).Color;
            }
        }

        public string CategoryName
        {
            get
            {
                return ((AccountCategoryLocalizationStringGetter == null) ? this._category.ToString() : AccountCategoryLocalizationStringGetter(this.Category));
            }
        }

        [Column]
        public string Color
        {
            get
            {
                return this.color;
            }
            set
            {
                if (value != null)
                {
                    this.OnNotifyPropertyChanging("Color");
                    this.color = value;
                    this.OnNotifyPropertyChanged("Color");
                }
            }
        }

        [XmlIgnore]
        public string ColorForName
        {
            get
            {
                if (!this.IsDefaultAccount)
                {
                    return foregroundColor;
                }
                return accentColor;
            }
        }

        public TinyMoneyManager.Component.CurrencyType Currency
        {
            get
            {
                return this.CurrencyType;
            }
            set
            {
                this.CurrencyType = value;
            }
        }

        public int CurrencyIndex
        {
            get
            {
                return 0;
            }
        }

        [XmlIgnore]
        public CurrencyWapper CurrencyInfo
        {
            get
            {
                if (this.currencyWapper == null)
                {
                    this.currencyWapper = CurrencyHelper.GetCurrencyItemByType(this.CurrencyType);
                }
                return CurrencyHelper.GetCurrencyItemByType(this.CurrencyType);
            }
            set
            {
                this.CurrencyType = value.Currency;
            }
        }

        [Column]
        public TinyMoneyManager.Component.CurrencyType CurrencyType
        {
            get
            {
                return this.currencyType;
            }
            set
            {
                if (this.currencyType != value)
                {
                    this.OnNotifyPropertyChanging("CurrencyType");
                    this.currencyType = value;
                    this.CurrencyTypeSymbol = CurrencyHelper.GetCurrencyItemByType(value).CurrencyString;
                    this.OnNotifyPropertyChanged("CurrencyType");
                    this.balanceInfo = string.Empty;
                    this.OnNotifyPropertyChanged("BalanceInfo");
                    this.OnNotifyPropertyChanged("BalanceInfoWithoutTag");
                }
            }
        }

        [XmlIgnore]
        public string CurrencyTypeSymbol
        {
            get
            {
                if (string.IsNullOrEmpty(this.currencyTypeSymbol))
                {
                    this.currencyTypeSymbol = CurrencyHelper.GetCurrencyItemByType(this.CurrencyType).CurrencyString;
                }
                return this.currencyTypeSymbol;
            }
            private set
            {
                this.currencyTypeSymbol = value;
                this.OnNotifyPropertyChanged("BalanceInfo");
            }
        }

        public bool EnableOverdraft
        {
            get
            {
                return (this.Category == AccountCategory.CreditCard);
            }
        }

        [Column(IsPrimaryKey = true, DbType = "UniqueIdentifier", CanBeNull = false, AutoSync = AutoSync.Default)]
        public System.Guid Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.OnNotifyPropertyChanging("Id");
                this.id = value;
                this.OnNotifyPropertyChanged("Id");
            }
        }

        [Column(CanBeNull = true)]
        public decimal? InitialBalance
        {
            get
            {
                return this.initialBlance;
            }
            set
            {
                decimal? nullable = value;
                decimal? initialBlance = this.initialBlance;
                if ((nullable.GetValueOrDefault() != initialBlance.GetValueOrDefault()) || (nullable.HasValue != initialBlance.HasValue))
                {
                    this.OnNotifyPropertyChanging("InitialBalance");
                    this.initialBlance = value;
                    this.OnNotifyPropertyChanged("InitialBalance");
                }
            }
        }

        [Column]
        public System.DateTime? InitialDateTime
        {
            get
            {
                return this.initialDateTime;
            }
            set
            {
                this.OnNotifyPropertyChanging("InitialDateTime");
                this.initialDateTime = value;
                this.OnNotifyPropertyChanged("InitialDateTime");
            }
        }

        [XmlIgnore]
        public bool IsDefaultAccount
        {
            get
            {
                return (AppSetting.Instance.DefaultAccount == this.id);
            }
            set
            {
                this.OnNotifyPropertyChanging("ColorForName");
                this.isDefaultAccount = value;
                this.OnNotifyPropertyChanged("ColorForName");
            }
        }

        public decimal Money
        {
            get
            {
                return this.Balance.GetValueOrDefault();
            }
            set
            {
                this.Balance = new decimal?(value);
            }
        }

        [Column]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.OnNotifyPropertyChanging("Name");
                this.name = value;
                this.OnNotifyPropertyChanged("Name");
                this.OnNotifyPropertyChanged("NameInfo");
            }
        }

        public string NameInfo
        {
            get
            {
                return (this.CategoryName + ">" + this.Name);
            }
        }

        public bool NeedPoundage
        {
            get
            {
                if (this.Category != AccountCategory.BankCard)
                {
                    return (this.Category == AccountCategory.CreditCard);
                }
                return true;
            }
        }

        [Column]
        public decimal Poundage
        {
            get
            {
                return this.poundage;
            }
            set
            {
                this.OnNotifyPropertyChanging("Poundage");
                this.poundage = value;
                this.OnNotifyPropertyChanged("Poundage");
                this.OnNotifyPropertyChanged("TransferingPoundageRateInfo");
            }
        }

        [Column]
        public bool StasticsBlance
        {
            get
            {
                return this.stasticBlance;
            }
            set
            {
                this.OnNotifyPropertyChanging("StasticsBlance");
                this.stasticBlance = value;
                this.OnNotifyPropertyChanged("StasticsBlance");
            }
        }

        /// <summary>
        /// Gets the transfering poundage rate info.
        /// </summary>
        /// <value>
        /// The transfering poundage rate info.
        /// </value>
        public string TransferingPoundageRateInfo
        {
            get
            {
                return this.poundage.ToMoneyF2();
            }
        }

        /// <summary>
        /// Sets the payment due date.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="day">The day.</param>
        public static void SetPaymentDueDate(Account account, int day)
        {
            if (account == null)
                return;

            account.PaymentDueDay = day;
        }

        public static void ResetOrders(TinyMoneyDataContext db)
        {
            try
            {
                var accounts = db.Accounts
                    .ToList();

                if (accounts.Count > 0)
                {
                    accounts.ForEach((p, i) => p.Order = i + 1);

                    db.SubmitChanges();
                }

                accounts = null;
            }
            catch (Exception)
            {
            }
        }
    }


    public partial class Account
    {
        private decimal? _lineOfCredit;

        [Column(CanBeNull = true)]
        public decimal? LineOfCredit
        {
            get { return _lineOfCredit; }
            set
            {
                if (value != _lineOfCredit)
                {
                    OnNotifyPropertyChanging("LineOfCredit");
                    _lineOfCredit = value;
                    OnNotifyPropertyChanged("LineOfCredit");
                    OnNotifyPropertyChanged("LineOfCreditMoneyInfo");
                }
            }
        }


        /// <summary>
        /// Gets the line of credit money info.
        /// </summary>
        /// <value>
        /// The line of credit money info.
        /// </value>
        public string LineOfCreditMoneyInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(this.CurrencyTypeSymbol, this.LineOfCredit);
            }
        }


        private int? _paymentDueDay;

        [Column(CanBeNull = true)]
        public int? PaymentDueDay
        {
            get { return _paymentDueDay; }
            set
            {
                if (value != _paymentDueDay)
                {
                    OnNotifyPropertyChanging("PaymentDueDay");
                    _paymentDueDay = value;
                    OnNotifyPropertyChanged("PaymentDueDay");
                    OnNotifyPropertyChanged("PaymentDueDateInfo");
                }
            }
        }

        /// <summary>
        /// Gets the payment due date info.
        /// </summary>
        /// <value>
        /// The payment due date info.
        /// </value>
        public string PaymentDueDateInfo
        {
            get
            {
                return LocalizedObjectHelper.GetLocalizedStringFrom("FrequencyDayOfMonthFormatter").FormatWith(this.PaymentDueDay.GetValueOrDefault());
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is credit card.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is credit card; otherwise, <c>false</c>.
        /// </value>
        public bool IsCreditCard
        {
            get
            {
                return this.Category == AccountCategory.CreditCard;
            }
        }

        public static void UpdateDataContext_At_v1972(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<Account>("PaymentDueDay");
            dataBaseUpdater.AddColumn<Account>("LineOfCredit");
        }

        private int? _order;
        public static string OrderProperty = "Order";

        [Column(CanBeNull = true)]
        public int? Order
        {
            get { return _order; }
            set
            {
                if (value != _order)
                {
                    OnNotifyPropertyChanging(OrderProperty);
                    _order = value;
                    OnNotifyPropertyChanged(OrderProperty);
                }
            }
        }

        /// <summary>
        /// Updates the data context_ at_v199.
        /// </summary>
        /// <param name="dataBaseUpdater">The data base updater.</param>
        public static void UpdateDataContext_At_v199(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<Account>(OrderProperty);
        }
    }

}

