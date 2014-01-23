namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using NkjSoft.Extensions;
    using RapidRepository;
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using System.Collections.Generic;

    [Table]
    public partial class AccountItem : NotionObject, IRapidEntity, IScheduledTaskItem
    {
        private EntityRef<TinyMoneyManager.Data.Model.Account> _account;
        private string _accountColor;
        private System.Guid _accountId;
        private System.Guid _autoTokenId;
        private EntityRef<AutoTokenItem> _autoTokenItem;
        private EntityRef<TinyMoneyManager.Data.Model.Category> _category;
        private System.Guid _categoryId;
        private System.DateTime _createTime;
        private string _description;
        private System.Guid _id;
        private bool _isAutomationToken;
        private decimal _money;
        private EntitySet<PeopleAssociationData> _peoples;
        private EntitySet<PictureInfo> _pictures;
        private AccountItemState _state;
        private ItemType _type;
        [XmlIgnore, Column(IsVersion = true)]
        private Binary _version;
        public static string CurrencySymbolCache;
        public static string DescriptionWithPictureInfoProperty = "DescriptionWithPictureInfo";
        private bool? isClaim = false;
        public const string IsClaimCheck = "√";
        private string moneyInfo;
        public string PageNameGetter;
        private string secondInfo;
        public const int ThertShold = 3;
        private string thirdInfo;
        private string typeInfo;

        public AccountItem()
        {
            this.Id = System.Guid.NewGuid();
            this.Description = string.Empty;
            this.CreateTime = System.DateTime.Now;
            this._pictures = new EntitySet<PictureInfo>(new System.Action<PictureInfo>(this.attach_ToDo), new System.Action<PictureInfo>(this.detach_ToDo));
            this._peoples = new EntitySet<PeopleAssociationData>(new System.Action<PeopleAssociationData>(this.attach_People), new System.Action<PeopleAssociationData>(this.detach_People));
            this.Pictures.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Pictures_CollectionChanged);
            this.Peoples.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Peoples_CollectionChanged);

            this._term = 3;
            this._dueDate = DateTime.Now;
            this._instalmentCalculatingWay = InstalmentItemMoneyCalculatingWay.NormallWay;
            this.IsInstallmentsItem = false;
        }

        private void attach_People(PeopleAssociationData peopleAssocidated)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            peopleAssocidated.AccountItem = this;
        }

        private void attach_ToDo(PictureInfo toDo)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            toDo.AccountItem = this;
        }

        private void Category_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnNotifyPropertyChanged("NameInfo");
        }

        private void ChangeMoneyInfo()
        {
            if (!this.isClaim.HasValue)
            {
                this.isClaim = false;
            }
            this.moneyInfo = (this.IsClaim.GetValueOrDefault() ? "√" : string.Empty) + AccountItemMoney.GetMoneyInfoWithCurrency(this._account.Entity.CurrencyTypeSymbol, new decimal?(this._money), "{0}{1}");
        }

        internal AccountItem Clone()
        {
            return new AccountItem { Id = this.Id, AccountId = this.AccountId, CategoryId = this.CategoryId, CreateTime = this.CreateTime, Description = this.Description, IsClaim = this.IsClaim, Money = this.Money, Type = this._type, State = this.State };
        }

        private void detach_People(PeopleAssociationData peopleAssocidated)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            peopleAssocidated.AccountItem = null;
        }

        private void detach_ToDo(PictureInfo toDo)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            toDo.AccountItem = null;
        }

        private string GetCreateDateInfo()
        {
            string format = (this.CreateTime.Date.Year != System.DateTime.Now.Year) ? LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.ShortDatePattern : LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.MonthDayPattern;
            format = format + " HH:mm, ddd";
            return this.CreateTime.ToString(format, LocalizedObjectHelper.CultureInfoCurrentUsed);
        }

        public decimal? GetMoney()
        {
            if (AppSetting.Instance.IgnoreCalimRecords && this.isClaim.GetValueOrDefault())
            {
                return 0.0M;
            }
            return new decimal?(CurrencyHelper.GetGlobleMoneyFrom(this.Currency, this.Money));
        }

        public string GetPeopleListWithNumberInfo(int numberOfPictures, int thertShold = 3)
        {
            if (((thertShold == 0) || (numberOfPictures == 0)) || (this.Peoples == null))
            {
                return LocalizedObjectHelper.GetLocalizedStringFrom("PeopleInfoWithNumberGreatThanNPeopleFormatter").FormatWith(new object[] { string.Empty, numberOfPictures }).Trim();
            }
            if (numberOfPictures > thertShold)
            {
                string str = (from p in this.Peoples.Take<PeopleAssociationData>(thertShold) select p.PeopleInfo.Name).ToStringLine<string>(",") + "...";
                return LocalizedObjectHelper.GetLocalizedStringFrom("PeopleInfoWithNumberGreatThanNPeopleFormatter").FormatWith(new object[] { str, numberOfPictures });
            }
            string str2 = (from p in this.Peoples.Take<PeopleAssociationData>(thertShold) select p.PeopleInfo.Name).ToStringLine<string>(",");
            return LocalizedObjectHelper.GetLocalizedStringFrom("PeopleInfoWithNumberLessThanNPeopleFormatter").FormatWith(new object[] { str2 });
        }

        public string GetPictureWithNumberInfo(int numberOfPictures)
        {
            return LocalizedObjectHelper.GetLocalizedStringFrom("PictureInfoWithNumberFormatter").FormatWith(new object[] { numberOfPictures });
        }

        private void Peoples_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnNotifyPropertyChanged("PeopleTotalInfoForEdit");
            this.OnNotifyPropertyChanged("PeopleTotalInfo");
            this.OnNotifyPropertyChanged("DescriptionWithPictureInfo");
        }

        private void Pictures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnNotifyPropertyChanged("PictureTotalInfoForEdit");
            this.OnNotifyPropertyChanged("PictureTotalInfo");
            this.OnNotifyPropertyChanged("DescriptionWithPictureInfo");
        }

        public void RaisePropertyChangd(string propertyName)
        {
            base.OnAsyncNotifyPropertyChanged(propertyName);
        }

        public static void UpdateStructureAt1_9_1(DatabaseSchemaUpdater updater)
        {
            updater.AddColumn<AccountItem>("IsClaim");
        }

        public static void UpdateStructureAt1_9_6(DatabaseSchemaUpdater dataBaseUpdater)
        {
        }

        [Association(Storage = "_account", ThisKey = "AccountId", OtherKey = "Id"), XmlIgnore]
        public TinyMoneyManager.Data.Model.Account Account
        {
            get
            {
                return this._account.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("Account");
                this._account.Entity = value;
                if (value != null)
                {
                    bool flag = this._accountId != value.Id;
                    this._accountId = value.Id;
                    if (flag)
                    {
                        this.OnNotifyPropertyChanged("AccountName");
                        this.AccountColor = value.CategoryColor;
                        this.OnNotifyPropertyChanged("MoneyInfo");

                        if (value.IsCreditCard && this.IsInstallmentsItem.GetValueOrDefault())
                        {
                            OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenCreatingProperty);
                            OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenViewingProperty);
                        }
                    }
                }
                this.OnNotifyPropertyChanged("Account");
            }
        }

        [XmlIgnore]
        public string AccountColor
        {
            get
            {
                if (string.IsNullOrEmpty(this._accountColor))
                {
                    this._accountColor = this.Account.CategoryColor;
                }
                return this._accountColor;
            }
            private set
            {
                if (value != this._accountColor)
                {
                    this._accountColor = value;
                    this.OnNotifyPropertyChanged("AccountColor");
                }
            }
        }

        [Column]
        public System.Guid AccountId
        {
            get
            {
                return this._accountId;
            }
            set
            {
                this.OnNotifyPropertyChanging("AccountId");
                this._accountId = value;
                this.OnNotifyPropertyChanged("AccountId");
            }
        }

        public string AccountName
        {
            get
            {
                return this.Account.Name;
            }
        }

        [Association(Storage = "_autoTokenItem", ThisKey = "AutoTokenId", OtherKey = "Id"), XmlIgnore]
        public AutoTokenItem AutoToken
        {
            get
            {
                return this._autoTokenItem.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("AutoToken");
                this._autoTokenItem.Entity = value;
                if (value != null)
                {
                    this._autoTokenId = value.Id;
                }
                this.OnNotifyPropertyChanged("AutoToken");
            }
        }

        [Column]
        public System.Guid AutoTokenId
        {
            get
            {
                return this._autoTokenId;
            }
            set
            {
                this.OnNotifyPropertyChanging("AutoTokenId");
                this._autoTokenId = value;
                this.OnNotifyPropertyChanged("AutoTokenId");
            }
        }

        [XmlIgnore, Association(Storage = "_category", ThisKey = "CategoryId", OtherKey = "Id")]
        public TinyMoneyManager.Data.Model.Category Category
        {
            get
            {
                return this._category.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("Category");
                this.OnNotifyPropertyChanging("CategoryId");
                this._category.Entity = value;
                if (value != null)
                {
                    this._categoryId = value.Id;
                    this.Category.PropertyChanged -= new PropertyChangedEventHandler(this.Category_PropertyChanged);
                    this.Category.PropertyChanged += new PropertyChangedEventHandler(this.Category_PropertyChanged);
                }
                this.OnNotifyPropertyChanged("Category");
                this.OnNotifyPropertyChanged("CategoryId");
                this.OnNotifyPropertyChanged("NameInfo");
            }
        }

        [Column]
        public System.Guid CategoryId
        {
            get
            {
                return this._categoryId;
            }
            set
            {
                this.OnNotifyPropertyChanging("CategoryId");
                this._categoryId = value;
                this.OnNotifyPropertyChanged("CategoryId");
                this.OnNotifyPropertyChanged("NameInfo");
            }
        }

        [XmlIgnore]
        public string CreateDateInfo
        {
            get
            {
                return this.GetCreateDateInfo();
            }
        }

        [Column]
        public System.DateTime CreateTime
        {
            get
            {
                return this._createTime;
            }
            set
            {
                if (value != this._createTime)
                {
                    this.OnNotifyPropertyChanging("CreateTime");
                    this._createTime = value;
                    this.OnNotifyPropertyChanged("CreateTime");
                    this.OnNotifyPropertyChanged("CreateDateInfo");
                }
            }
        }

        [XmlIgnore]
        public CurrencyType Currency
        {
            get
            {
                if (this.Account == null)
                {
                    return AppSetting.Instance.DefaultCurrency;
                }

                return this.Account.CurrencyType;
            }
        }

        [Column]
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this.OnNotifyPropertyChanging("Description");
                this._description = value;
                this.OnNotifyPropertyChanged("Description");
                this.OnNotifyPropertyChanged("DescriptionWithPictureInfo");
            }
        }

        public string DescriptionWithPeopleNames
        {
            get
            {
                return string.Empty;
            }
        }

        public string DescriptionWithPictureInfo
        {
            get
            {
                int pictureNumbers = this.PictureNumbers;
                string description = this.Description;
                string pictureWithNumberInfo = string.Empty;
                string peopleListWithNumberInfo = string.Empty;
                int length = this.Description.Length;
                if (pictureNumbers > 0)
                {
                    pictureWithNumberInfo = this.GetPictureWithNumberInfo(pictureNumbers);
                }
                int peopleNumbers = this.PeopleNumbers;
                if (peopleNumbers > 0)
                {
                    if (pictureNumbers > 0)
                    {
                        peopleListWithNumberInfo = "," + this.GetPeopleListWithNumberInfo(peopleNumbers, 0);
                    }
                    else
                    {
                        peopleListWithNumberInfo = this.GetPeopleListWithNumberInfo(peopleNumbers, 0);
                    }
                }
                if (((pictureNumbers > 0) || (peopleNumbers > 0)) && (length > 0))
                {
                    description = "," + this.Description;
                }
                return "{0}{1}{2}".FormatWith(new object[] { pictureWithNumberInfo, peopleListWithNumberInfo, description });
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
                this.OnNotifyPropertyChanging("Id");
                this._id = value;
                this.OnNotifyPropertyChanged("Id");
            }
        }

        [Column]
        public bool IsAutomationToken
        {
            get
            {
                return this._isAutomationToken;
            }
            set
            {
                this.OnNotifyPropertyChanging("IsAutomationToken");
                this._isAutomationToken = value;
                this.OnNotifyPropertyChanged("IsAutomationToken");
            }
        }

        [Column(CanBeNull = true)]
        public bool? IsClaim
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
                    this.OnNotifyPropertyChanged("MoneyInfo");
                }
            }
        }

        [Column]
        public decimal Money
        {
            get
            {
                return this._money;
            }
            set
            {
                if (value != this._money)
                {
                    this.OnNotifyPropertyChanging("Money");
                    this._money = value;
                    this.OnNotifyPropertyChanged("Money");
                    this.OnNotifyPropertyChanged("MoneyInfo");
                    this.OnNotifyPropertyChanged("MoneyInfoWithoutTag");
                    this.OnNotifyPropertyChanged(TotalCostMoneyInfoProperty);
                    this.OnNotifyPropertyChanged(MoneyOfEachTermProperty);
                    this.OnAsyncNotifyPropertyChanged(MoneyOfEachTermInfoProperty);
                    this.OnAsyncNotifyPropertyChanged(HasInstalmentInformationProperty);
                    this.OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenCreatingProperty);
                    this.OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenViewingProperty);
                }
            }
        }

        public string MoneyInfo
        {
            get
            {
                this.ChangeMoneyInfo();
                return this.moneyInfo;
            }
        }

        public string MoneyInfoWithoutTag
        {
            get
            {
                return this.Money.ToMoneyF2();
            }
        }

        public string NameInfo
        {
            get
            {
                if (this.Category == null)
                {
                    return LocalizedObjectHelper.GetLocalizedStringFrom("UntitleItem");
                }
                return this.Category.CategoryInfo;
            }
        }

        /// <summary>
        /// Gets the people numbers.
        /// </summary>
        /// <value>
        /// The people numbers.
        /// </value>
        public int PeopleNumbers
        {
            get
            {
                if (IsInDeattchedFromDatabaseMode)
                {
                    if (this.PeoplesOutOfDatabase == null)
                    {
                        return 0;
                    }

                    return PeoplesOutOfDatabase.Count;
                }
                else
                {
                    if (this.Peoples == null)
                    {
                        return 0;
                    }
                    return this.Peoples.Count;
                }

            }
        }

        [Association(Storage = "_peoples", OtherKey = "AttachedId", ThisKey = "Id"), XmlIgnore]
        public EntitySet<PeopleAssociationData> Peoples
        {
            get
            {
                return this._peoples;
            }
            set
            {
                this._peoples.Assign(value);
            }
        }

        public string PeopleTotalInfo
        {
            get
            {
                int peopleNumbers = this.PeopleNumbers;
                return this.GetPeopleListWithNumberInfo(peopleNumbers, 3);
            }
        }

        public string PeopleTotalInfoForEdit
        {
            get
            {
                int peopleNumbers = this.PeopleNumbers;
                if (peopleNumbers > 0)
                {
                    return this.GetPeopleListWithNumberInfo(peopleNumbers, 0);
                }
                return LocalizedObjectHelper.GetLocalizedStringFrom("LinkPeoples").ToLowerInvariant();
            }
        }


        private List<PictureInfo> _picturesOutOfDatabase;
        public static string PictureTotalInfoForEditProperty = "PictureTotalInfoForEdit";
        public static string PeopleTotalInfoForEditProperty = "PeopleTotalInfoForEdit";

        [XmlIgnore]
        public List<PictureInfo> PicturesOutOfDatabase
        {
            get { return _picturesOutOfDatabase; }
            set
            {
                if (value != _picturesOutOfDatabase)
                {
                    OnNotifyPropertyChanging("PicturesOutOfDatabase");
                    _picturesOutOfDatabase = value;
                    OnNotifyPropertyChanged("PicturesOutOfDatabase");
                    OnNotifyPropertyChanged("PictureTotalInfoForEdit");
                }
            }
        }

        [XmlIgnore]
        private List<PeopleAssociationData> _peoplesOutOfDatabase;

        [XmlIgnore]
        public List<PeopleAssociationData> PeoplesOutOfDatabase
        {
            get { return _peoplesOutOfDatabase; }
            set
            {
                if (value != _peoplesOutOfDatabase)
                {
                    OnNotifyPropertyChanging("PeoplesOutOfDatabase");
                    _peoplesOutOfDatabase = value;
                    OnNotifyPropertyChanged("PeoplesOutOfDatabase");
                    OnNotifyPropertyChanged(PeopleTotalInfoForEditProperty);
                }
            }
        }


        /// <summary>
        /// Gets the picture numbers.
        /// </summary>
        /// <value>
        /// The picture numbers.
        /// </value>
        public int PictureNumbers
        {
            get
            {
                if (IsInDeattchedFromDatabaseMode)
                {
                    if (this.PicturesOutOfDatabase == null)
                    {
                        return 0;
                    }

                    return PicturesOutOfDatabase.Count;
                }
                else
                {

                    if (this.Pictures == null)
                    {
                        return 0;
                    }
                    return this.Pictures.Count;
                }
            }
        }

        [Association(Storage = "_pictures", OtherKey = "AttachedId", ThisKey = "Id"), XmlIgnore]
        public EntitySet<PictureInfo> Pictures
        {
            get
            {
                return this._pictures;
            }
            set
            {
                this._pictures.Assign(value);
            }
        }

        public string PictureTotalInfo
        {
            get
            {
                int pictureNumbers = this.PictureNumbers;
                return this.GetPictureWithNumberInfo(pictureNumbers);
            }
        }

        public string PictureTotalInfoForEdit
        {
            get
            {
                int pictureNumbers = this.PictureNumbers;
                if (pictureNumbers > 0)
                {
                    return this.GetPictureWithNumberInfo(pictureNumbers);
                }
                return LocalizedObjectHelper.GetLocalizedStringFrom("AddPicture");
            }
        }

        [XmlIgnore]
        public bool IsInDeattchedFromDatabaseMode { get; set; }

        [XmlIgnore]
        public string SecondInfo
        {
            get
            {
                return this.secondInfo;
            }
            set
            {
                if (this.secondInfo != value)
                {
                    this.secondInfo = value;
                    this.OnAsyncNotifyPropertyChanged("SecondInfo");
                }
            }
        }

        [Column]
        public AccountItemState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this.OnNotifyPropertyChanging("State");
                this._state = value;
                this.OnNotifyPropertyChanged("State");
            }
        }

        [XmlIgnore]
        public string ThirdInfo
        {
            get
            {
                return this.thirdInfo;
            }
            set
            {
                if (this.thirdInfo != value)
                {
                    this.thirdInfo = value;
                    this.OnAsyncNotifyPropertyChanged("ThirdInfo");
                }
            }
        }

        [Column]
        public ItemType Type
        {
            get
            {
                return this._type;
            }
            set
            {
                this.OnNotifyPropertyChanging("Type");
                this._type = value;
                this.OnNotifyPropertyChanged("Type");
            }
        }

        [XmlIgnore]
        public string TypeInfo
        {
            get
            {
                return this.typeInfo;
            }
            set
            {
                if (this.typeInfo != value)
                {
                    this.typeInfo = value;
                    this.OnAsyncNotifyPropertyChanged("TypeInfo");
                }
            }
        }

        public void RemovePicture(PictureInfo pic)
        {
            if (this.PicturesOutOfDatabase != null)
            {
                this.PicturesOutOfDatabase.Remove(pic);
                OnNotifyPropertyChanged(PictureTotalInfoForEditProperty);
            }
        }

        public void RemovePeople(PeopleAssociationData people)
        {
            if (this.PeoplesOutOfDatabase != null)
            {
                this.PeoplesOutOfDatabase.Remove(people);
                OnNotifyPropertyChanged(PeopleTotalInfoForEditProperty);
            }
        }

        /// <summary>
        /// Adds the picture.
        /// </summary>
        /// <param name="pic">The pic.</param>
        public void AddPicture(PictureInfo pic)
        {
            if (this.PicturesOutOfDatabase == null)
            {
                this.PicturesOutOfDatabase = new List<PictureInfo>();
            }

            if (this.PicturesOutOfDatabase != null)
            {
                this.PicturesOutOfDatabase.Add(pic);
                OnNotifyPropertyChanged(PictureTotalInfoForEditProperty);
            }
        }

        /// <summary>
        /// Adds the picture.
        /// </summary>
        /// <param name="pic">The pic.</param>
        public void AddPeople(PeopleAssociationData personInfo)
        {
            if (this.PeoplesOutOfDatabase == null)
            {
                this.PeoplesOutOfDatabase = new List<PeopleAssociationData>();
            }

            if (this.PeoplesOutOfDatabase != null)
            {
                this.PeoplesOutOfDatabase.Add(personInfo);
                OnNotifyPropertyChanged(PeopleTotalInfoForEditProperty);
            }
        }

        /// <summary>
        /// Calculates the amount.
        /// </summary>
        public void CalculateAmount()
        {
            //
            var totalCost = this._totalCost.GetValueOrDefault();

            var poundage = this._totalPoundage.GetValueOrDefault();

            var terms = this._term.GetValueOrDefault();

            terms = terms == 0 ? 1 : terms;

            // amount + amount* poundage.

            var totalAmount = 0.0m;

            switch (this._instalmentCalculatingWay.GetValueOrDefault())
            {
                case InstalmentItemMoneyCalculatingWay.NormallWay:
                    totalAmount = totalCost + totalCost * poundage;
                    break;
                default:
                    break;
            }

            this.Money = totalAmount;

            OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenCreatingProperty);
            OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenViewingProperty);
        }
    }

    public partial class AccountItem
    {
        private decimal? _totalCost;
        public static string TotalCostProperty = "TotalCost";

        [Column(CanBeNull = true)]
        public decimal? TotalCost
        {
            get { return _totalCost; }
            set
            {
                if (value != _totalCost)
                {
                    OnNotifyPropertyChanging(TotalCostProperty);
                    _totalCost = value;
                    OnNotifyPropertyChanged(TotalCostProperty);
                }
            }
        }

        private bool? _isInstallmentsItem;
        public static string IsInstallmentsItemProperty = "IsInstallmentsItem";

        [Column(CanBeNull = true)]
        public bool? IsInstallmentsItem
        {
            get
            {
                return _isInstallmentsItem.HasValue ? _isInstallmentsItem.Value : false;
            }
            set
            {
                if (value != _isInstallmentsItem)
                {
                    OnNotifyPropertyChanging(IsInstallmentsItemProperty);
                    _isInstallmentsItem = value;
                    OnNotifyPropertyChanged(IsInstallmentsItemProperty);
                    OnNotifyPropertyChanged(HasInstalmentInformationProperty);
                    OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenCreatingProperty);
                    OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenViewingProperty);
                }
            }
        }

        private int? _term;
        public static string TermProperty = "Term";

        [Column(CanBeNull = true)]
        public int? Term
        {
            get { return _term; }
            set
            {
                if (value != _term)
                {
                    OnNotifyPropertyChanging(TermProperty);
                    _term = value;
                    OnNotifyPropertyChanged(TermProperty);
                    OnNotifyPropertyChanged(MoneyOfEachTermProperty);
                    OnAsyncNotifyPropertyChanged(MoneyOfEachTermInfoProperty);
                    OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenCreatingProperty);
                    OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenViewingProperty);
                }
            }
        }

        private Guid? _paymentAccountId;
        public static string PaymentAccountIdProperty = "PaymentAccountId";

        [Column(CanBeNull = true)]
        public Guid? PaymentAccountId
        {
            get { return _paymentAccountId; }
            set
            {
                if (value != _paymentAccountId)
                {
                    OnNotifyPropertyChanging(PaymentAccountIdProperty);
                    _paymentAccountId = value;
                    OnNotifyPropertyChanged(PaymentAccountIdProperty);
                }
            }
        }


        private Account _paymentAccount;

        public Account PaymentAccount
        {
            get { return _paymentAccount; }
            set { _paymentAccount = value; }
        }

        private decimal? _totalPoundage;
        public static string TotalPoundageProperty = "TotalPoundage";

        [Column(CanBeNull = true)]
        public decimal? TotalPoundage
        {
            get { return _totalPoundage; }
            set
            {
                if (value != _totalPoundage)
                {
                    OnNotifyPropertyChanging(TotalPoundageProperty);
                    _totalPoundage = value;
                    OnNotifyPropertyChanged(TotalPoundageProperty);
                    OnAsyncNotifyPropertyChanged("TotalPoundageInfo");
                    OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenCreatingProperty);
                    OnAsyncNotifyPropertyChanged(InstalmentsInformationWhenViewingProperty);
                }
            }
        }

        /// <summary>
        /// Gets the total poundage info.
        /// </summary>
        /// <value>
        /// The total poundage info.
        /// </value>
        [XmlIgnore]
        public string TotalPoundageInfo
        {
            get { return _totalPoundage.GetValueOrDefault().ToMoneyF2(); }
        }


        private InstalmentItemMoneyCalculatingWay? _instalmentCalculatingWay;
        public static string InstalmentCalculatingWayProperty = "InstalmentCalculatingWay";

        [Column(CanBeNull = true)]
        public InstalmentItemMoneyCalculatingWay? InstalmentCalculatingWay
        {
            get { return _instalmentCalculatingWay; }
            set
            {
                if (value != _instalmentCalculatingWay)
                {
                    OnNotifyPropertyChanging(InstalmentCalculatingWayProperty);
                    _instalmentCalculatingWay = value;
                    OnNotifyPropertyChanged(InstalmentCalculatingWayProperty);
                }
            }
        }

        public static string MoneyOfEachTermProperty = "MoneyOfEachTerm";

        /// <summary>
        /// Gets the money of each term.
        /// </summary>
        /// <value>
        /// The money of each term.
        /// </value>
        public decimal MoneyOfEachTerm
        {
            get
            {
                if (this.Term.HasValue && this.Term.Value > 0)
                {
                    return this.Money / this.Term.Value;
                }

                return 0.0M;
            }
        }

        /// <summary>
        /// Gets the amount of each term money info.
        /// </summary>
        /// <value>
        /// The amount of each term money info.
        /// </value>
        public string MoneyOfEachTermInfo
        {
            get
            {
                if (this.Account != null)
                {
                    return
                        AccountItemMoney.GetMoneyInfoWithCurrency(this.Account.CurrencyTypeSymbol, this.MoneyOfEachTerm);
                }
                else
                {
                    return "0.0";
                }
            }
        }

        private DateTime? _dueDate;
        public static string DueDateProperty = "DueDate";

        [Column(CanBeNull = true)]
        public DateTime? DueDate
        {
            get { return _dueDate; }
            set
            {
                if (value != _dueDate)
                {
                    OnNotifyPropertyChanging(DueDateProperty);
                    _dueDate = value;
                    OnNotifyPropertyChanged(DueDateProperty);
                }
            }
        }


        public static string HasInstalmentInformationProperty = "HasInstalmentInformation";

        /// <summary>
        /// Gets a value indicating whether this instance has instalment information.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has instalment information; otherwise, <c>false</c>.
        /// </value>
        public bool HasInstalmentInformation
        {
            get
            {
                return this.IsInstallmentsItem.HasValue && this.IsInstallmentsItem.Value;
            }
        }

        public static string TotalCostMoneyInfoProperty = "TotalCostMoneyInfo";

        /// <summary>
        /// Gets the total cost money info.
        /// </summary>
        /// <value>
        /// The total cost money info.
        /// </value>
        public string TotalCostMoneyInfo
        {
            get
            {
                return AccountItemMoney.GetMoneyInfoWithCurrency(this.Account.CurrencyTypeSymbol, this.TotalCost.GetValueOrDefault());
            }
        }

        public static string MoneyOfEachTermInfoProperty = "MoneyOfEachTermInfo";

        public static string InstalmentsInformationWhenCreatingProperty = "InstalmentsInformationWhenCreating";

        /// <summary>
        /// Gets the instalments information when creating.
        /// </summary>
        /// <value>
        /// The instalments information when creating.
        /// </value>
        public string InstalmentsInformationWhenCreating
        {
            get
            {
                if (!HasInstalmentInformation)
                {
                    return "Tap setting to create instalment information.";// LocalizedObjectHelper.GetLocalizedStringFrom("");
                }

                return "Total Cost: {0}, {1} term(s), {2}/term \r\nDue by: {3}"
                    .FormatWith(MoneyInfo, this.Term.Value, this.MoneyOfEachTermInfo, DueDate);
            }
        }

        public static readonly string InstalmentsInformationWhenViewingProperty = "InstalmentsInformationWhenViewing";

        public string InstalmentsInformationWhenViewing
        {
            get
            {
                var actualConsumption = LocalizedObjectHelper.GetLocalizedStringFrom("ActualConsumption");

                var termFormatter = LocalizedObjectHelper.GetLocalizedStringFrom("TermFormatter");

                var inTermFormatter = LocalizedObjectHelper.GetLocalizedStringFrom("ByTerms")
                    .FormatWith(Term.GetValueOrDefault());

                var dueBy = LocalizedObjectHelper.GetLocalizedStringFrom("DueDate");

                return "{0}: {1}, {2}, {3}/{4}, {5}: {6}"
                    .FormatWith(actualConsumption, TotalCostMoneyInfo, inTermFormatter, MoneyOfEachTermInfo, termFormatter.FormatWith(string.Empty),
                    dueBy, DueDate.GetValueOrDefault().ToShortDateString());
            }
        }

        public static void UpdateAt199(DatabaseSchemaUpdater databaseSchemaUpdater)
        {
            databaseSchemaUpdater.AddColumn<AccountItem>(TotalCostProperty);
            databaseSchemaUpdater.AddColumn<AccountItem>(TotalPoundageProperty);
            databaseSchemaUpdater.AddColumn<AccountItem>(TermProperty);
            databaseSchemaUpdater.AddColumn<AccountItem>(DueDateProperty);
            databaseSchemaUpdater.AddColumn<AccountItem>(PaymentAccountIdProperty);
            databaseSchemaUpdater.AddColumn<AccountItem>(IsInstallmentsItemProperty);
            databaseSchemaUpdater.AddColumn<AccountItem>(InstalmentCalculatingWayProperty);
        }
    }
}

