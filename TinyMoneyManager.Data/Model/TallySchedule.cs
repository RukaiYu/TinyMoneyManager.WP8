namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using RapidRepository;
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Common;

    using NkjSoft.Extensions;
    using System.Linq;
    [Table]
    public class TallySchedule : NotionObject, IMoney, IRapidEntity
    {
        private EntityRef<Category> _associatedCategory;
        private EntityRef<Account> _fromAccount;
        private EntityRef<Account> _toAccount;
        private RecordActionType actionType;
        private string activeColor = "green";
        private System.Guid categoryId;
        private CurrencyType currency;
        private System.DayOfWeek? dayOfWeek;
        private System.DateTime endTime = System.DateTime.Now;
        private int? estimateDate = 30;
        private ScheduleType frequency;
        private System.Guid fromAccountId = System.Guid.Empty;
        private System.Guid id = System.Guid.Empty;
        private bool? isActive;
        private bool isClaim = false;
        public static System.Func<Guid, Boolean> IsCompletedForTodayHandler;
        private bool isFavorite = false;
        private decimal money;
        private string name = string.Empty;
        private string notes = string.Empty;
        private ScheduleRecordType profileRecordType = ScheduleRecordType.ProfileRecord;
        private ItemType recordType = ItemType.Expense;
        private int? startDate = 1;
        private System.DateTime startTime = System.DateTime.Now;
        private System.Guid toAccountId = System.Guid.Empty;
        private int? valueForFrequency = 0;


        public TallySchedule()
        {
            this.RecurrenceInterval = 0;
            this.name = string.Empty;
            this.isActive = true;
        }

        public AccountItem CreateEmptyAccountItem()
        {
            AccountItem item = this.ReBuildAccountItem();
            item.Description = this.name;
            return item;
        }

        private static string GetDayOfWeek(System.DayOfWeek dow)
        {
            return LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.GetDayName(dow);
        }

        public decimal? GetMoney()
        {
            throw new System.NotImplementedException();
        }

        public void RaiseTallyCompleted()
        {
            this.OnAsyncNotifyPropertyChanged("StatusBackImageUri");
        }

        public AccountItem ReBuildAccountItem()
        {
            return new AccountItem { AccountId = this.fromAccountId, Account = this.FromAccount, Category = this.AssociatedCategory, CategoryId = this.categoryId, Description = this.notes, IsClaim = new bool?(this.isClaim), Money = this.Money, State = AccountItemState.Active, Type = this.recordType, AutoTokenId = this.id };
        }

        [Column]
        public RecordActionType ActionType
        {
            get
            {
                return this.actionType;
            }
            set
            {
                if (this.actionType != value)
                {
                    this.OnNotifyPropertyChanging("ActionType");
                    this.actionType = value;
                    this.OnNotifyPropertyChanged("ActionType");
                    this.OnNotifyPropertyChanged("TaskTitle");
                    this.OnNotifyPropertyChanged("TaskSubTitle");
                }
            }
        }

        /// <summary>
        /// Gets the type of the action handler.
        /// </summary>
        /// <value>
        /// The type of the action handler.
        /// </value>
        public RecordActionType ActionHandlerType
        {
            get
            {
                if (actionType == RecordActionType.CreateIncomeRecord ||
                     actionType == RecordActionType.CrateExpenseRecord)
                {
                    return RecordActionType.CreateTranscationRecord;
                }
                else
                {
                    return actionType;
                }
            }
        }

        [XmlIgnore]
        public string ActiveColor
        {
            get
            {
                return this.activeColor;
            }
            set
            {
                if (this.activeColor != value)
                {
                    this.OnNotifyPropertyChanging("ActiveColor");
                    this.activeColor = value;
                    this.OnNotifyPropertyChanged("ActiveColor");
                }
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
                    this.categoryId = value.Id;
                    this.OnNotifyPropertyChanged("AssociatedCategory");
                    this.OnNotifyPropertyChanged("SubTitle");
                    this.OnNotifyPropertyChanged("TaskSubTitle");
                }
            }
        }

        [Column]
        public System.Guid CategoryId
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

        [Column]
        public CurrencyType Currency
        {
            get
            {
                return this.currency;
            }
            set
            {
                if (this.currency != value)
                {
                    this.OnNotifyPropertyChanging("Currency");
                    this.currency = value;
                    this.OnNotifyPropertyChanged("Currency");
                }
            }
        }

        [Column(CanBeNull = true)]
        public System.DayOfWeek? DayofWeek
        {
            get
            {
                return this.dayOfWeek;
            }
            set
            {
                if (this.dayOfWeek != value)
                {
                    this.OnNotifyPropertyChanging("DayofWeek");
                    this.dayOfWeek = value;
                    this.OnNotifyPropertyChanged("FrequencyName");
                    this.OnNotifyPropertyChanged("DayofWeek");
                }
            }
        }

        [Column]
        public System.DateTime EndTime
        {
            get
            {
                return this.endTime;
            }
            set
            {
                if (this.endTime != value)
                {
                    this.OnNotifyPropertyChanging("EndTime");
                    this.endTime = value;
                    this.OnNotifyPropertyChanged("EndTime");
                }
            }
        }

        [Column]
        public int? EstimateDate
        {
            get
            {
                return this.estimateDate;
            }
            set
            {
                if (this.estimateDate != value)
                {
                    this.OnNotifyPropertyChanging("EstimateDate");
                    this.estimateDate = value;
                    this.OnNotifyPropertyChanged("EstimateDate");
                }
            }
        }

        [Column]
        public ScheduleType Frequency
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
                    this.HasExecuteChanged = true;
                    this.OnNotifyPropertyChanged("FrequencyName");
                    this.OnNotifyPropertyChanged("FrequencyShortName");
                    this.OnNotifyPropertyChanged("Frequency");
                }
            }
        }

        public string FrequencyName
        {
            get
            {
                this.OnNotifyPropertyChanged("Name");
                return FrequencyInfo.GetSummary(this.frequency, this.ValueForFrequency.GetValueOrDefault(), this.dayOfWeek.GetValueOrDefault(), GetDayOfWeek(this.dayOfWeek.GetValueOrDefault()));
            }
        }

        public string FrequencyShortName
        {
            get
            {
                this.OnNotifyPropertyChanged("Name");
                string str = LocalizedObjectHelper.GetLocalizedStringFrom(this.Frequency.ToString());
                int index = str.IndexOf("(");
                return ((index > 0) ? str.Substring(0, index) : str);
            }
        }

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
                    bool flag = this.fromAccountId != value.Id;
                    this.fromAccountId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("FromAccountName");
                    }
                }
                this.OnNotifyPropertyChanged("FromAccount");
                this.OnNotifyPropertyChanged("SubTitle");
                this.OnNotifyPropertyChanged("TaskTitle");
            }
        }

        [Column]
        public System.Guid FromAccountId
        {
            get
            {
                return this.fromAccountId;
            }
            set
            {
                if (this.fromAccountId != value)
                {
                    this.OnNotifyPropertyChanging("FromAccountId");
                    this.fromAccountId = value;
                    this.OnNotifyPropertyChanged("FromAccountId");
                    this.OnNotifyPropertyChanged("SubTitle");
                }
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

        [Column]
        public bool? IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                if (this.isActive != value)
                {
                    this.OnNotifyPropertyChanging("IsActive");
                    this.isActive = value;
                    this.OnNotifyPropertyChanged("IsActive");
                    if (value.HasValue && value.GetValueOrDefault())
                    {
                        this.ActiveColor = "green";
                    }
                    else
                    {
                        this.ActiveColor = "red";
                    }
                }
            }
        }

        [Column]
        public bool IsClaim
        {
            get
            {
                return this.isClaim;
            }
            set
            {
                if (this.isClaim != value)
                {
                    this.OnNotifyPropertyChanging("IsClaim");
                    this.isClaim = value;
                    this.OnNotifyPropertyChanged("IsClaim");
                }
            }
        }

        public bool IsCompletedForToday
        {
            get
            {
                return IsCompletedForTodayHandler(this.Id);
            }
        }

        [Column]
        public bool IsFavorite
        {
            get
            {
                return this.isFavorite;
            }
            set
            {
                if (this.isFavorite != value)
                {
                    this.OnNotifyPropertyChanging("IsFavorite");
                    this.isFavorite = value;
                    this.OnNotifyPropertyChanged("IsFavorite");
                }
            }
        }

        [Column]
        public decimal Money
        {
            get
            {
                return this.money;
            }
            set
            {
                if (this.money != value)
                {
                    this.OnNotifyPropertyChanging("Money");
                    this.money = value;
                    this.OnNotifyPropertyChanged("MoneyInfo");
                    this.OnNotifyPropertyChanged("Money");
                    this.OnNotifyPropertyChanged("SubTitle");
                }
            }
        }

        public string MoneyInfo
        {
            get
            {
                return ((this.IsClaim ? "√" : string.Empty) + AccountItemMoney.GetMoneyInfoWithCurrency(this._fromAccount.Entity.CurrencyTypeSymbol, new decimal?(this.money), "{0}{1}"));
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
                if (this.name != value)
                {
                    this.OnNotifyPropertyChanging("Name");
                    this.name = value;
                    this.OnNotifyPropertyChanged("Name");
                    this.OnNotifyPropertyChanged("NotificationSummary");
                }
            }
        }

        [Column]
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

        [Column]
        public ScheduleRecordType ProfileRecordType
        {
            get
            {
                return this.profileRecordType;
            }
            set
            {
                if (this.profileRecordType != value)
                {
                    this.OnNotifyPropertyChanging("ProfileRecordType");
                    this.profileRecordType = value;
                    this.OnNotifyPropertyChanged("ProfileRecordType");
                }
            }
        }

        [Column]
        public ItemType RecordType
        {
            get
            {
                return this.recordType;
            }
            set
            {
                if (this.recordType != value)
                {
                    this.OnNotifyPropertyChanging("RecordType");
                    this.recordType = value;
                    this.OnNotifyPropertyChanged("RecordType");
                }
            }
        }

        [Column]
        public int? StartDate
        {
            get
            {
                return this.startDate;
            }
            set
            {
                if (this.startDate != value)
                {
                    this.OnNotifyPropertyChanging("StartDate");
                    this.startDate = value;
                    this.OnNotifyPropertyChanged("StartDate");
                    this.OnNotifyPropertyChanged("FrequencyName");
                }
            }
        }

        [Column]
        public System.DateTime StartTime
        {
            get
            {
                return this.startTime;
            }
            set
            {
                if (this.startTime != value)
                {
                    this.OnNotifyPropertyChanging("StartTime");
                    this.startTime = value;
                    this.OnNotifyPropertyChanged("StartTime");
                    this.OnNotifyPropertyChanged("ShortTimeInfo");
                    this.OnNotifyPropertyChanged("NotificationSummary");
                }
            }
        }

        public string ShortTimeInfo
        {
            get
            {
                return this.startTime.ToShortTimeString();
            }
        }

        public string ShortDateInfo
        {
            get
            {
                return this.startTime.ToShortDateString();
            }
        }

        /// <summary>
        /// Gets the notification summary.
        /// </summary>
        /// <value>
        /// The notification summary.
        /// </value>
        public string NotificationSummary
        {
            get
            {
                return "{0}, {1}\r\n{2}".FormatWith(RecurrenceIntervalName, ShortDateInfo, Name);
            }
        }

        /// <summary>
        /// Gets the notification summary.
        /// </summary>
        /// <value>
        /// The notification summary.
        /// </value>
        public string NotificationSummaryWithoutName
        {
            get
            {
                var newStartTime = this.StartTime;

                newStartTime = EnsureTimeByRecurrenceInterval((int)RecurrenceInterval);

                string format = (newStartTime.Date.Year != System.DateTime.Now.Year) ? LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.ShortDatePattern : LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.MonthDayPattern;
                format = format + " HH:mm, ddd";
                var timeInfo = newStartTime.ToString(format, LocalizedObjectHelper.CultureInfoCurrentUsed);

                return "{0}, {1}".FormatWith(RecurrenceIntervalName, timeInfo);
            }
        }


        public string StatusBackImageUri
        {
            get
            {
                if (this.IsCompletedForToday)
                {
                    return "/images/Template_Background.png";
                }
                return string.Empty;
            }
        }

        public string SubTitle
        {
            get
            {
                return (this.AssociatedCategory.Name + "\r\n" + this.FromAccount.Name + "\r\n" + this.MoneyInfo);
            }
        }

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
                    bool flag = this.toAccountId != value.Id;
                    this.toAccountId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("ToAccountName");
                    }
                }
                this.OnNotifyPropertyChanged("ToAccount");
                this.OnNotifyPropertyChanged("TaskSubTitle");
            }
        }

        [Column]
        public System.Guid ToAccountId
        {
            get
            {
                return this.toAccountId;
            }
            set
            {
                if (this.toAccountId != value)
                {
                    this.OnNotifyPropertyChanging("ToAccountId");
                    this.toAccountId = value;
                    this.OnNotifyPropertyChanged("ToAccountId");
                }
            }
        }

        [Column(CanBeNull = true)]
        public int? ValueForFrequency
        {
            get
            {
                return this.valueForFrequency;
            }
            set
            {
                if (this.valueForFrequency != value)
                {
                    this.OnNotifyPropertyChanging("ValueForFrequency");
                    this.valueForFrequency = value;
                    this.OnNotifyPropertyChanged("FrequencyName");
                    this.OnNotifyPropertyChanged("FrequencyShortName");
                    this.OnNotifyPropertyChanged("ValueForFrequency");
                }
            }
        }

        [Column]
        public string DataProvider { get; set; }

        private decimal? _TransferingPoundageAmount;

        [Column(CanBeNull = true)]
        public decimal? TransferingPoundageAmount
        {
            get { return _TransferingPoundageAmount; }
            set
            {
                if (value != _TransferingPoundageAmount)
                {
                    OnNotifyPropertyChanging("TransferingPoundageAmount");
                    _TransferingPoundageAmount = value;
                    OnNotifyPropertyChanged("TransferingPoundageAmount");
                    OnNotifyPropertyChanged("TaskSubTitle");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TaskTitle
        {
            get
            {
                var actionName = string.Empty;
                if (this.ActionHandlerType == RecordActionType.CreateTranscationRecord)
                {
                    actionName = LocalizedObjectHelper.GetLocalizedStringFrom(RecordType.ToString());
                }
                else
                {
                    actionName = LocalizedObjectHelper.GetLocalizedStringFrom("TransferingAccount");

                }

                return string.Format("{0}, {1}“{2}”",
                     actionName,
                     LocalizedObjectHelper.GetLocalizedStringFrom("From"),
                     FromAccount.Name);
            }
        }

        public string TaskSubTitle
        {
            get
            {
                if (this.ActionHandlerType == RecordActionType.CreateTransferingRecord)
                {
                    string titleFormatter = "{0}“{1}”";

                    var poundageValue = "";
                    if (this._TransferingPoundageAmount.HasValue && this._TransferingPoundageAmount.Value != 0.0m)
                    {
                        titleFormatter = "{0}“{1}”, {2}: {3} {4}";
                        poundageValue = this._TransferingPoundageAmount.Value.ToMoneyF2();


                        return string.Format(titleFormatter,
                            LocalizedObjectHelper.GetLocalizedStringFrom("To"),
                            ToAccount.Name,
                            LocalizedObjectHelper.GetLocalizedStringFrom("Poundage"),
                            poundageValue,
                            notes);
                    }

                    return string.Format(titleFormatter,
                         LocalizedObjectHelper.GetLocalizedStringFrom("To"),
                         ToAccount.Name,
                         this.notes);
                }

                if (this.AssociatedCategory != null)
                {
                    return this.AssociatedCategory.CategoryInfo;
                }
                else
                {
                    return this.Notes;
                }
            }
        }

        /// <summary>
        /// Updates the structure at_1_9_1.
        /// </summary>
        /// <param name="dataBaseUpdater">The data base updater.</param>
        public static void UpdateStructureAt_1_9_1(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<TallySchedule>("Notes");
            dataBaseUpdater.AddColumn<TallySchedule>("IsActive");
        }

        /// <summary>
        /// Updates the structure at_1_9_1.
        /// </summary>
        /// <param name="dataBaseUpdater">The data base updater.</param>
        public static void UpdateStructureAt_1_9_8(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<TallySchedule>("DataProvider");
            dataBaseUpdater.AddColumn<TallySchedule>("TransferingPoundageAmount");
            dataBaseUpdater.AddColumn<TallySchedule>("PeopleIds");
            dataBaseUpdater.AddColumn<TallySchedule>("PictureIds");
        }

        private static readonly char[] splitor = new char[] { '|' };

        private Guid[] _peoples;

        [XmlIgnore]
        public Guid[] Peoples
        {
            get
            {
                if (_peoples == null && _PeopleIds != null)
                {
                    _peoples = this.PeopleIds.Split(splitor, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.ToGuid())
                        .ToArray();
                }

                return _peoples;
            }
            set
            {
                if (value != _peoples)
                {
                    OnNotifyPropertyChanging("Peoples");
                    _peoples = value;

                    if (value != null)
                    {
                        this.PeopleIds = string.Join("|", value);
                    }

                    OnNotifyPropertyChanged("Peoples");
                }
            }
        }

        private Guid[] _pictures;

        [XmlIgnore]
        public Guid[] Pictures
        {
            get
            {
                if (_pictures == null && PictureIds != null)
                {
                    _pictures = this.PictureIds.Split(splitor, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.ToGuid())
                        .ToArray();
                }

                return _pictures;
            }
            set
            {
                if (value != _pictures)
                {
                    OnNotifyPropertyChanging("Pictures");
                    _pictures = value;

                    if (value != null)
                    {
                        this.PictureIds = string.Join("|", value);
                    }

                    OnNotifyPropertyChanged("Pictures");
                }
            }
        }

        private string _PeopleIds;

        [Column(CanBeNull = true)]
        public string PeopleIds
        {
            get { return _PeopleIds; }
            set
            {
                if (value != _PeopleIds)
                {
                    OnNotifyPropertyChanging("PeopleIds");
                    _PeopleIds = value;
                    if (value != null && value.Length > 0)
                    {
                        _peoples = value.Split(splitor, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.ToGuid())
                            .ToArray();
                    }
                    OnNotifyPropertyChanged("PeopleIds");
                }
            }
        }

        private string _PictureIds;

        [Column(CanBeNull = true)]
        public string PictureIds
        {
            get { return _PictureIds; }
            set
            {
                if (value != _PictureIds)
                {
                    OnNotifyPropertyChanging("PictureIds");
                    _PictureIds = value;

                    if (value != null && value.Length > 0)
                    {
                        _pictures = this.PictureIds.Split(splitor, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.ToGuid())
                            .ToArray();
                    }

                    OnNotifyPropertyChanged("PictureIds");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has execute changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has execute changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasExecuteChanged { get; set; }

        private int? _RecurrenceInterval;
        public static string RecurrenceIntervalProperty = "RecurrenceInterval";

        [Column(CanBeNull = true)]
        public int? RecurrenceInterval
        {
            get { return _RecurrenceInterval; }
            set
            {
                if (value != _RecurrenceInterval)
                {
                    OnNotifyPropertyChanging(RecurrenceIntervalProperty);
                    _RecurrenceInterval = value;
                    OnNotifyPropertyChanged(RecurrenceIntervalProperty);
                    OnNotifyPropertyChanged("RecurrenceIntervalName");
                    OnNotifyPropertyChanged("NotificationSummary");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string RecurrenceIntervalName
        {
            get
            {

                /*
                 None = 0,
        //
        // Summary:
        //     Daily recurrence.
        Daily = 1,
        //
        // Summary:
        //     Weekly recurrence.
        Weekly = 2,
        //
        // Summary:
        //     Monthly recurrence.
        Monthly = 3,
        //
        // Summary:
        //     Recurring at the end of each month.
        EndOfMonth = 4,
        //
        // Summary:
        //     Yearly recurrence.
        Yearly = 5,
                 */

                var val = "OnlyOnce";

                switch (RecurrenceInterval)
                {
                    case 0:
                    default:
                        break;
                    case 1:
                        val = "Daily";
                        break;
                    case 2:
                        val = "Weekly";
                        break;
                    case 3:
                        val = "Monthly";
                        break;
                    case 4:
                        val = "EndOfMonth";
                        break;
                    case 5:
                        val = "Yearly";
                        break;
                }

                return LocalizedObjectHelper.GetLocalizedStringFrom(val);
            }
        }

        /// <summary>
        /// Ensures the time by recurrence interval.
        /// </summary>
        /// <param name="recurrenceInterval">The recurrence interval.</param>
        /// <returns></returns>
        private DateTime EnsureTimeByRecurrenceInterval(int recurrenceInterval)
        {

            var date = this.StartTime;

            switch (recurrenceInterval)
            {
                case 0:
                default:
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 1:
                    date = DateTime.Now.Date + StartTime.TimeOfDay;
                    break;
            }

            return date;
        }

        /// <summary>
        /// Updates the table structure at V1_9_9.
        /// </summary>
        /// <param name="databaseSchemaUpdater">The database schema updater.</param>
        public static void UpdateTableStructureAtV1_9_9(DatabaseSchemaUpdater databaseSchemaUpdater)
        {
            databaseSchemaUpdater.AddColumn<TallySchedule>("RecurrenceInterval");
        }
    }

}

