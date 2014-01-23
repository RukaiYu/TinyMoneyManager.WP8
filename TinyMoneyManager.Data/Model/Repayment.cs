namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Component.Converter;
    using TinyMoneyManager.Data;

    [Table]
    public class Repayment : NotionObject, IMoney
    {
        private decimal _amount;
        private EntityRef<Category> _associatedCategory;
        private System.DateTime? _completedAt;
        private CurrencyType _currency;
        private System.DateTime _dueDate;
        private System.Guid _fromAccountId;
        private EntityRef<PeopleProfile> _fromPeople;
        private System.Guid _id;
        private string _notes;
        private EntityRef<Account> _payFromAccount;
        private EntityRef<Account> _payToAccount;
        private System.Guid _payToAccountId;
        private string _place;
        private System.DateTime _repayAt;
        private EntityRef<Repayment> _repayToOrGetBackFrom;
        private EntitySet<Repayment> _repayToOrGetBackFromItems;
        private RepaymentStatus _status;
        private EntityRef<PeopleProfile> _toPeople;
        private bool _useAlarm;
        public static string AlarmName;
        private CurrencyType borrowLoanCurrency;
        private LeanType? borrowOrLean;
        private System.Guid? categoryId;
        private System.DateTime? createAt;
        public const int DefaultLineHeight = 60;
        public const int DefaultLineHeightPlusOneRow = 80;
        private System.DateTime? executeDate;
        private ScheduleType? frequency;
        private System.Guid? fromPeopleId;
        private double? interest;
        public static string LocalizedCompletedAt;
        public static string ReminderName;
        private RepaymentType? repaymentRecordType;
        private System.Guid repayToId;
        private System.Guid? toPeopleId;

        public Repayment()
        {
            this.BorrowOrLean = 0;
            this.Amount = 0.0M;
            this.Notes = string.Empty;
            this.executeDate = new System.DateTime?(System.DateTime.Now);
            this.interest = 0.0;
            this._repayToOrGetBackFromItems = new EntitySet<Repayment>(new System.Action<Repayment>(this.attach_ToDo), new System.Action<Repayment>(this.detach_ToDo));
        }

        private void attach_ToDo(Repayment toDo)
        {
            this.OnNotifyPropertyChanging("RepayTo");
            toDo.RepayToOrGetBackFrom = this;
        }

        public void Cancel()
        {
            this.Status = RepaymentStatus.Cancel;
        }

        public Repayment Clone()
        {
            return new Repayment
            {
                _id = this.Id,
                _repayAt = this._repayAt,
                _amount = this._amount,
                _currency = this._currency,
                _notes = this._notes,
                _payToAccount = this._payToAccount,
                _payToAccountId = this._payToAccountId,
                _status = this._status,
                _completedAt = this._completedAt,
                interest = this.interest,
                fromPeopleId = this.fromPeopleId,
                _fromPeople = this._fromPeople,
                _fromAccountId = this._fromAccountId,
                _payFromAccount = this._payFromAccount,
                _place = this._place,
                _dueDate = this._dueDate,
                _useAlarm = this._useAlarm,
                repaymentRecordType = this.repaymentRecordType,
                categoryId = this.categoryId,
                AssociatedCategory = this.AssociatedCategory,
                createAt = this.createAt,
                frequency = this.frequency,
                borrowOrLean = this.borrowOrLean,
                toPeopleId = this.toPeopleId,
                executeDate = this.executeDate
            };
        }

        public void Completed()
        {
            this.CompletedAt = new System.DateTime?(System.DateTime.Now);
            this.Status = RepaymentStatus.Completed;
        }

        private void detach_ToDo(Repayment toDo)
        {
            this.OnNotifyPropertyChanging("RepayTo");
            toDo.RepayToOrGetBackFrom = null;
        }

        public string GetBorrowLoanInfoForPeople(string peopleName, bool WithoutAmountInfo = false)
        {
            string source = "{0},{1}";
            string str2 = string.Empty;
            switch (this.borrowOrLean.GetValueOrDefault())
            {
                case LeanType.BorrowIn:
                    source = LocalizedObjectHelper.GetLocalizedStringFrom("LoanOutOrRepayToSbFormatter");
                    str2 = LocalizedObjectHelper.GetLocalizedStringFrom("LoanOut");
                    break;

                case LeanType.LoanOut:
                    source = LocalizedObjectHelper.GetLocalizedStringFrom("BorrowInOrReceiptFromSbForrmater");
                    str2 = LocalizedObjectHelper.GetLocalizedStringFrom("BorrowIn");
                    break;

                case LeanType.Repayment:
                    source = LocalizedObjectHelper.GetLocalizedStringFrom("BorrowInOrReceiptFromSbForrmater");
                    str2 = LocalizedObjectHelper.GetLocalizedStringFrom("Receipt");
                    break;

                case LeanType.Receipt:
                    source = LocalizedObjectHelper.GetLocalizedStringFrom("LoanOutOrRepayToSbFormatter");
                    str2 = LocalizedObjectHelper.GetLocalizedStringFrom("Repayment");
                    break;
            }
            string str3 = peopleName.FormatWith(new object[] { this.PayFromAccount.Name });
            return source.FormatWith(new object[] { str2, str3, (WithoutAmountInfo ? this.AmountInfo : string.Empty) });
        }

        public string GetContentUsedForNotification()
        {
            return "{0}\t:      {1}\r\n{2}\t:      {3}\r\n{4}\t:      {5}\r\n{6}\t:      {7}\r\n{8}\t:      {9}".FormatWith(new object[] { LocalizedObjectHelper.GetLocalizedStringFrom("RepayAccountName"), this.PayToAccount.Name, LocalizedObjectHelper.GetLocalizedStringFrom("RepayFromAccountName"), this.PayFromAccount.Name, LocalizedObjectHelper.GetLocalizedStringFrom("Amount"), this.AmountInfo, LocalizedObjectHelper.GetLocalizedStringFrom("Place"), this.Place, LocalizedObjectHelper.GetLocalizedStringFrom("Notes"), this.Notes });
        }

        private string GetCreateDateInfo(System.DateTime date)
        {
            string format = (date.Date.Year != System.DateTime.Now.Year) ? LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.ShortDatePattern : LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.MonthDayPattern;
            format = format + ", ddd";
            return date.ToString(format, LocalizedObjectHelper.CultureInfoCurrentUsed);
        }

        public decimal? GetMoney()
        {
            return new decimal?(CurrencyHelper.GetGlobleMoneyFrom(this.BorrowLoanCurrency, this.Amount));
        }

        public decimal GetMoneyForRepayOrReceive(CurrencyType? currencyTo = new CurrencyType?())
        {
            if (!currencyTo.HasValue)
            {
                currencyTo = new CurrencyType?(this.BorrowLoanCurrency);
            }

            return (this.BorrowLoanCurrency.GetConversionRateTo(currencyTo.Value) * this.Amount);
        }

        public LeanType GetReverseRepaymentType()
        {
            LeanType receipt = LeanType.Receipt;
            switch (this.borrowOrLean.GetValueOrDefault())
            {
                case LeanType.BorrowIn:
                    return LeanType.Repayment;

                case LeanType.LoanOut:
                    return LeanType.Receipt;

                case LeanType.Repayment:
                    return LeanType.Repayment;

                case LeanType.Receipt:
                    return LeanType.Receipt;
            }
            return receipt;
        }

        public void RepayToOrGetBackFromItemsChanged()
        {
            this.OnAsyncNotifyPropertyChanged("RepayToOrGetBackFromItems");
        }

        public void RestoreFrom(Repayment repayment)
        {
            this.Id = repayment.Id;
            this.Amount = repayment.Amount;
            this.Notes = repayment.Notes;
            this.RepayAt = repayment.RepayAt;
            this.Status = repayment.Status;
            this._payToAccountId = repayment._payToAccountId;
            this._fromAccountId = repayment.FromAccountId;
            this.CompletedAt = repayment.CompletedAt;
            this.PayToAccount = repayment.PayToAccount;
            this.PayFromAccount = repayment.PayFromAccount;
            this.Place = repayment._place;
            this.DueDate = repayment._dueDate;
            this.UseAlarm = repayment._useAlarm;
            this._fromPeople = repayment._fromPeople;
            this.toPeopleId = repayment.toPeopleId;
            this.ToPeople = repayment.ToPeople;
            this.Interset = repayment.interest;
            this.repaymentRecordType = repayment.RepaymentRecordType;
            this.BorrowOrLean = repayment.borrowOrLean;
            this.categoryId = repayment.CategoryId;
            this.AssociatedCategory = repayment.AssociatedCategory;
            this.createAt = repayment.CreateAt;
            this.frequency = repayment.Frequency;
            this.ExecuteDate = repayment.executeDate;
            this.OnNotifyPropertyChanged("RepaymentInfoLine");
            this.OnNotifyPropertyChanged("RemindingInfo");
        }

        public static void UpdateOldData(TinyMoneyDataContext tinyMoneyDataContext)
        {
            ((System.Collections.Generic.IEnumerable<Repayment>)(from p in tinyMoneyDataContext.Repayments
                                                                 where ((int?)p.RepaymentRecordType) == null
                                                                 select p)).ForEach<Repayment>(delegate(Repayment p)
            {
                p.RepaymentRecordType = 0;
            });
            tinyMoneyDataContext.SubmitChanges();
        }

        public static void UpdateTableStructureAtV1_9_1(DatabaseSchemaUpdater databaseSchemaUpdater)
        {
            databaseSchemaUpdater.AddColumn<Repayment>("FromPeopleId");
            databaseSchemaUpdater.AddColumn<Repayment>("ToPeopleId");
            databaseSchemaUpdater.AddColumn<Repayment>("BorrowOrLean");
            databaseSchemaUpdater.AddColumn<Repayment>("CreateAt");
            databaseSchemaUpdater.AddColumn<Repayment>("Frequency");
            databaseSchemaUpdater.AddColumn<Repayment>("RepaymentRecordType");
            databaseSchemaUpdater.AddColumn<Repayment>("CategoryId");
            databaseSchemaUpdater.AddColumn<Repayment>("Interset");
        }

        public static void UpdateTableStructureAtV1_9_3(DatabaseSchemaUpdater databaseSchemaUpdater)
        {
            databaseSchemaUpdater.AddColumn<Repayment>("ExecuteDate");
            databaseSchemaUpdater.AddColumn<Repayment>("RepayToId");
            databaseSchemaUpdater.AddColumn<Repayment>("BorrowLoanCurrency");
        }

        public string AccountColor
        {
            get
            {
                return this.PayToAccount.Color;
            }
        }

        public string AccountName
        {
            get
            {
                return this.PayToAccount.Name;
            }
        }

        public string AccountWithBorrowLoanTypeTitle
        {
            get
            {
                return LocalizedObjectHelper.GetCombinedText(this.BorrowLoanTypeName, LocalizedObjectHelper.GetLocalizedStringFrom("AccountName")).ToLowerInvariant();
            }
        }

        [Column]
        public decimal Amount
        {
            get
            {
                return this._amount;
            }
            set
            {
                if (this._amount != value)
                {
                    this.OnNotifyPropertyChanging("Amount");
                    this._amount = value;
                    this.OnNotifyPropertyChanged("Amount");
                    this.OnNotifyPropertyChanged("AmountInfo");
                    this.OnNotifyPropertyChanged("BorrowLoanAmountInfo");
                    this.OnNotifyPropertyChanged("AmountString");
                    this.OnNotifyPropertyChanged("BorrowLoanAmountString");
                }
            }
        }

        public string AmountInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(this.Currency, new decimal?(this._amount), "{0}{1}");
            }
        }

        public string AmountString
        {
            get
            {
                return this.Amount.ToMoneyF2();
            }
        }

        public string AmountWithBorrowLoanTypeTitle
        {
            get
            {
                return "{0}{1}".FormatWith(new object[] { this.BorrowLoanTypeName, LocalizedObjectHelper.GetLocalizedStringFrom("AmountFormatterAfterWord") });
            }
        }

        [XmlIgnore, Association(Storage = "_associatedCategory", ThisKey = "CategoryId", OtherKey = "Id")]
        public Category AssociatedCategory
        {
            get
            {
                return this._associatedCategory.Entity;
            }
            set
            {
                if ((value != null) && (value.Id != this.categoryId))
                {
                    this._associatedCategory.Entity = value;
                    this.CategoryId = new System.Guid?(value.Id);
                    this.OnNotifyPropertyChanged("AssociatedCategory");
                }
            }
        }

        public string BorrowLoanAmountInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(this.BorrowLoanCurrency, new decimal?(this.Amount), "{0}{1}");
            }
        }

        public string BorrowLoanAmountString
        {
            get
            {
                return this.Amount.ToMoneyF2();
            }
        }

        [Column(CanBeNull = true)]
        public CurrencyType BorrowLoanCurrency
        {
            get
            {
                return this.borrowLoanCurrency;
            }
            set
            {
                if (this.borrowLoanCurrency != value)
                {
                    this.OnNotifyPropertyChanging("BorrowLoanCurrency");
                    this.borrowLoanCurrency = value;
                    this.OnNotifyPropertyChanged("BorrowLoanCurrency");
                }
            }
        }

        public string BorrowLoanInfoWithoutAmountInfo
        {
            get
            {
                string source = "{0},{1}";
                if ((((LeanType)this.borrowOrLean) == LeanType.BorrowIn) || (((LeanType)this.borrowOrLean) == LeanType.Receipt))
                {
                    source = LocalizedObjectHelper.GetLocalizedStringFrom("BorrowInOrReceiptFromSbForrmater");
                }
                else
                {
                    source = LocalizedObjectHelper.GetLocalizedStringFrom("LoanOutOrRepayToSbFormatter");
                }


                return source.FormatWith(new object[] { this.BorrowLoanTypeName, this.ToPeople == null ? "N/A" : this.ToPeople.Name, string.Empty });
            }
        }

        public string BorrowLoanTypeName
        {
            get
            {
                return LocalizedObjectHelper.GetLocalizedStringFrom(this.borrowOrLean.GetValueOrDefault().ToString());
            }
        }

        [Column]
        public LeanType? BorrowOrLean
        {
            get
            {
                return this.borrowOrLean;
            }
            set
            {
                if (this.borrowOrLean != value)
                {
                    this.OnNotifyPropertyChanging("BorrowOrLean");
                    this.borrowOrLean = value;
                    this.OnNotifyPropertyChanged("BorrowOrLean");
                }
            }
        }

        public bool CanChangeStatus
        {
            get
            {
                if (this.Status != RepaymentStatus.OnGoing)
                {
                    return (this.Status == RepaymentStatus.Cancel);
                }
                return true;
            }
        }

        public bool CanDoRepayOrReceieve
        {
            get
            {
                LeanType valueOrDefault = this.BorrowOrLean.GetValueOrDefault();
                if (valueOrDefault != LeanType.BorrowIn)
                {
                    return (valueOrDefault == LeanType.LoanOut);
                }
                return true;
            }
        }

        [Column(CanBeNull = true)]
        public System.Guid? CategoryId
        {
            get
            {
                return this.categoryId;
            }
            set
            {
                if (this.categoryId != value)
                {
                    this.OnNotifyPropertyChanging("CategoryId");
                    this.categoryId = value;
                    this.OnNotifyPropertyChanged("CategoryId");
                }
            }
        }

        [Column(CanBeNull = true)]
        public System.DateTime? CompletedAt
        {
            get
            {
                return this._completedAt;
            }
            set
            {
                this.OnNotifyPropertyChanging("CompletedAt");
                this._completedAt = value;
                this.OnNotifyPropertyChanged("CompletedAt");
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
        public CurrencyType Currency
        {
            get
            {
                if (this.PayToAccount != null)
                {
                    return this.PayToAccount.CurrencyType;
                }
                return AppSetting.Instance.DefaultCurrency;
            }
            set
            {
            }
        }

        public string DateWithBorrowLoanTypeTitle
        {
            get
            {
                return LocalizedObjectHelper.GetLocalizedStringFrom("DateFormatterAfterWord").FormatWith(new object[] { this.BorrowLoanTypeName }).ToLowerInvariant();
            }
        }

        [Column(CanBeNull = true)]
        public System.DateTime DueDate
        {
            get
            {
                return this._dueDate;
            }
            set
            {
                this.OnNotifyPropertyChanging("DueDate");
                this._dueDate = value;
                this.OnNotifyPropertyChanged("DueDate");
            }
        }

        [Column]
        public System.DateTime? ExecuteDate
        {
            get
            {
                return this.executeDate;
            }
            set
            {
                if (this.executeDate != value)
                {
                    this.OnNotifyPropertyChanging("ExecuteDate");
                    this.executeDate = value;
                    this.OnNotifyPropertyChanged("ExecuteDate");
                }
            }
        }

        public string ExecuteDateInfo
        {
            get
            {
                return this.GetCreateDateInfo(this.executeDate.GetValueOrDefault());
            }
        }

        [Column(CanBeNull = true)]
        public ScheduleType? Frequency
        {
            get
            {
                return this.frequency;
            }
            set
            {
                if (this.frequency != value)
                {
                    this.OnNotifyPropertyChanging("Frequency");
                    this.frequency = value;
                    this.OnNotifyPropertyChanged("Frequency");
                }
            }
        }

        [Column]
        public System.Guid FromAccountId
        {
            get
            {
                return this._fromAccountId;
            }
            set
            {
                this.OnNotifyPropertyChanging("FromAccountId");
                this._fromAccountId = value;
                this.OnNotifyPropertyChanged("FromAccountId");
            }
        }

        [Association(Storage = "_fromPeople", ThisKey = "FromPeopleId", OtherKey = "Id"), XmlIgnore]
        public PeopleProfile FromPeople
        {
            get
            {
                return this._fromPeople.Entity;
            }
            set
            {
                if ((value != null) && (value.Id != this.fromPeopleId))
                {
                    this._fromPeople.Entity = value;
                    this.FromPeopleId = new System.Guid?(value.Id);
                    this.OnNotifyPropertyChanged("FromPeople");
                }
            }
        }

        [Column(CanBeNull = true)]
        public System.Guid? FromPeopleId
        {
            get
            {
                return this.fromPeopleId;
            }
            set
            {
                if (this.fromPeopleId != value)
                {
                    this.OnNotifyPropertyChanging("FromPeopleId");
                    this.fromPeopleId = value;
                    this.OnNotifyPropertyChanged("FromPeopleId");
                }
            }
        }

        [Column(IsPrimaryKey = true, DbType = "UniqueIdentifier", CanBeNull = false, AutoSync = AutoSync.Default)]
        public System.Guid Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        public string InterestInfo
        {
            get
            {
                if (this.Interset.GetValueOrDefault() != 0.0)
                {
                    return this.Interset.GetValueOrDefault().ToString("F2");
                }
                return "N/A";
            }
        }

        [Column(CanBeNull = true)]
        public double? Interset
        {
            get
            {
                return this.interest;
            }
            set
            {
                if (this.interest != value)
                {
                    this.OnNotifyPropertyChanging("Interset");
                    this.interest = value;
                    this.OnNotifyPropertyChanged("Interset");
                }
            }
        }

        public bool IsRepaymentOrReceieve
        {
            get
            {
                LeanType valueOrDefault = this.BorrowOrLean.GetValueOrDefault();
                if (valueOrDefault != LeanType.Receipt)
                {
                    return (valueOrDefault == LeanType.Repayment);
                }
                return true;
            }
        }

        public int LineHeight
        {
            get
            {
                if (!string.IsNullOrEmpty(this._place))
                {
                    return 80;
                }
                return 60;
            }
        }

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
        public string Notes
        {
            get
            {
                return this._notes;
            }
            set
            {
                this.OnNotifyPropertyChanging("Notes");
                this._notes = value;
                this.OnNotifyPropertyChanged("Notes");
            }
        }

        [Association(Storage = "_payFromAccount", ThisKey = "FromAccountId", OtherKey = "Id"), XmlIgnore]
        public Account PayFromAccount
        {
            get
            {
                return this._payFromAccount.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("PayFromAccount");
                this._payFromAccount.Entity = value;
                if (value != null)
                {
                    bool flag = this._fromAccountId != value.Id;
                    this.FromAccountId = value.Id;
                    this.BorrowLoanCurrency = value.Currency;
                }
                this.OnNotifyPropertyChanged("PayFromAccount");
                this.OnNotifyPropertyChanged("BorrowLoanAmountInfo");
                this.OnNotifyPropertyChanged("PayFromAccountColor");
            }
        }

        public string PayFromAccountColor
        {
            get
            {
                return this.PayFromAccount.CategoryColor;
            }
        }

        [Association(Storage = "_payToAccount", ThisKey = "PayToAccountId", OtherKey = "Id"), XmlIgnore]
        public Account PayToAccount
        {
            get
            {
                return this._payToAccount.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("PayToAccount");
                this._payToAccount.Entity = value;
                if (value != null)
                {
                    bool flag = this.PayToAccountId != value.Id;
                    this.PayToAccountId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("AccountName");
                        this.OnNotifyPropertyChanged("AccountColor");
                    }
                }
                this.OnNotifyPropertyChanged("PayToAccount");
            }
        }

        [Column]
        public System.Guid PayToAccountId
        {
            get
            {
                return this._payToAccountId;
            }
            set
            {
                this.OnNotifyPropertyChanging("PayToAccountId");
                this._payToAccountId = value;
                this.OnNotifyPropertyChanged("PayToAccountId");
            }
        }

        public string PeopleWithBorrowLoanTypeTitle
        {
            get
            {
                string str = string.Empty;
                if ((((LeanType)this.borrowOrLean) == LeanType.BorrowIn) || (((LeanType)this.borrowOrLean) == LeanType.Receipt))
                {
                    return LocalizedObjectHelper.GetLocalizedStringFrom("Creditor");
                }
                if ((((LeanType)this.borrowOrLean) != LeanType.LoanOut) && (((LeanType)this.borrowOrLean) != LeanType.Repayment))
                {
                    return str;
                }
                return LocalizedObjectHelper.GetLocalizedStringFrom("Debtor");
            }
        }

        [Column]
        public string Place
        {
            get
            {
                return this._place;
            }
            set
            {
                this.OnNotifyPropertyChanging("Place");
                this._place = value;
                this.OnNotifyPropertyChanged("Place");
            }
        }

        public string RemindingInfo
        {
            get
            {
                if (this.Status == RepaymentStatus.Completed)
                {
                    return string.Format("{0} : {1}", LocalizedCompletedAt, this.CompletedAt.Value);
                }
                return string.Format("{0} - {1}, {2}.", this.RepayAt.ToShortDateString(), this.DueDate.ToShortDateString(), this.UseAlarm ? AlarmName : ReminderName);
            }
        }

        [Column]
        public System.DateTime RepayAt
        {
            get
            {
                return this._repayAt;
            }
            set
            {
                this.OnNotifyPropertyChanging("RepayAt");
                this._repayAt = value;
                this.OnNotifyPropertyChanged("RepayAt");
            }
        }

        public string RepaymentInfoLine
        {
            get
            {
                if (this.PayFromAccount == null && this.PayToAccount == null)
                {
                    return string.Empty;
                }

                return string.Format("{0} -> {1}", this.PayToAccount.Name, this.PayFromAccount.Name);
            }
        }

        [Column(CanBeNull = true)]
        public RepaymentType? RepaymentRecordType
        {
            get
            {
                return this.repaymentRecordType;
            }
            set
            {
                if (this.repaymentRecordType != value)
                {
                    this.OnNotifyPropertyChanging("RepaymentRecordType");
                    this.repaymentRecordType = value;
                    this.OnNotifyPropertyChanged("RepaymentRecordType");
                }
            }
        }

        [Column(CanBeNull = true)]
        public System.Guid RepayToId
        {
            get
            {
                return this.repayToId;
            }
            set
            {
                if (this.repayToId != value)
                {
                    this.OnNotifyPropertyChanging("RepayToId");
                    this.repayToId = value;
                    this.OnNotifyPropertyChanged("RepayToId");
                }
            }
        }

        [XmlIgnore, Association(Storage = "_repayToOrGetBackFrom", OtherKey = "Id", ThisKey = "RepayToId")]
        public Repayment RepayToOrGetBackFrom
        {
            get
            {
                return this._repayToOrGetBackFrom.Entity;
            }
            set
            {
                if (value != null)
                {
                    this.OnNotifyPropertyChanging("RepayToOrGetBackFrom");
                    this._repayToOrGetBackFrom.Entity = value;
                    this.RepayToId = value.Id;
                    this.OnNotifyPropertyChanged("RepayToOrGetBackFrom");
                }
            }
        }

        [Association(Storage = "_repayToOrGetBackFromItems", OtherKey = "RepayToId", ThisKey = "Id"), XmlIgnore]
        public EntitySet<Repayment> RepayToOrGetBackFromItems
        {
            get
            {
                return this._repayToOrGetBackFromItems;
            }
            set
            {
                this.OnNotifyPropertyChanging("RepayToOrGetBackFromItems");
                this._repayToOrGetBackFromItems.Assign(value);
                this.OnNotifyPropertyChanged("RepayToOrGetBackFromItems");
            }
        }

        public string ReverseRepaymentTypeName
        {
            get
            {
                string str = string.Empty;
                switch (this.borrowOrLean.GetValueOrDefault())
                {
                    case LeanType.BorrowIn:
                        return LocalizedObjectHelper.GetLocalizedStringFrom("Repayment");

                    case LeanType.LoanOut:
                        return LocalizedObjectHelper.GetLocalizedStringFrom("Receipt");

                    case LeanType.Repayment:
                        return LocalizedObjectHelper.GetLocalizedStringFrom("Repayment");

                    case LeanType.Receipt:
                        return LocalizedObjectHelper.GetLocalizedStringFrom("Receipt");
                }
                return str;
            }
        }

        [Column]
        public RepaymentStatus Status
        {
            get
            {
                return this._status;
            }
            set
            {
                if (this._status != value)
                {
                    this.OnNotifyPropertyChanging("Status");
                    this._status = value;
                    this.OnNotifyPropertyChanged("Status");
                    this.OnNotifyPropertyChanged("StatusColor");
                    this.OnNotifyPropertyChanged("RemindingInfo");
                }
            }
        }

        public string StatusColor
        {
            get
            {
                return RepaymentStatusToColorConverter.GetColorFromStatus(this.Status);
            }
        }

        [Association(Storage = "_toPeople", ThisKey = "ToPeopleId", OtherKey = "Id"), XmlIgnore]
        public PeopleProfile ToPeople
        {
            get
            {
                return this._toPeople.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("ToPeople");
                if (value != null)
                {
                    if (value.Id != this.toPeopleId)
                    {
                        this._toPeople.Entity = value;
                        this.ToPeopleId = new System.Guid?(value.Id);
                        this.OnNotifyPropertyChanged("BorrowLoanInfoWithoutAmountInfo");
                    }
                    this.OnNotifyPropertyChanged("ToPeople");
                }
            }
        }

        [Column(CanBeNull = true)]
        public System.Guid? ToPeopleId
        {
            get
            {
                return this.toPeopleId;
            }
            set
            {
                if (this.toPeopleId != value)
                {
                    this.OnNotifyPropertyChanging("ToPeopleId");
                    this.toPeopleId = value;
                    this.OnNotifyPropertyChanged("ToPeopleId");
                }
            }
        }

        [Column]
        public bool UseAlarm
        {
            get
            {
                return this._useAlarm;
            }
            set
            {
                this.OnNotifyPropertyChanging("UseAlarm");
                this._useAlarm = value;
                this.OnNotifyPropertyChanged("UseAlarm");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleted
        {
            get
            {
                return this.Amount <= this._repayToOrGetBackFromItems.Sum(p => p.GetMoneyForRepayOrReceive());
            }
        }

        /// <summary>
        /// Updates the table structure at V1_9_8.
        /// </summary>
        /// <param name="dataBaseUpdater">The data base updater.</param>
        /// <param name="updater">The updater.</param>
        public static void UpdateTableStructureAtV1_9_8(TinyMoneyDataContext db, Action<Repayment> updater)
        {
            var itemsToUpdate = db.Repayments.Where(p => p.RepaymentRecordType == RepaymentType.MoneyBorrowOrLeanRepayment).ToList();

            itemsToUpdate.ForEach(p => updater((p)));

            db.SubmitChanges();
        }

    }
}

