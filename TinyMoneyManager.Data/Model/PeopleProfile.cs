namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using NkjSoft.Extensions;
    using RapidRepository;
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;

    [Table]
    public class PeopleProfile : NotionObject, IRapidEntity
    {
        private EntityRef<PeopleGroup> _peopleGroup;
        private CurrencyWapper currencyInfo;
        private string firstName = string.Empty;
        private System.Guid groupId;
        private CurrencyType? hasCurrency;
        private string homeAddress;
        private string lastName = string.Empty;
        private string name = string.Empty;
        private string notes;
        private string personalEmail;
        private string phone;
        private string telephone;
        private string workingAddress;
        private string workingEmail;

        public static int CompareByFirstName(object obj1, object obj2)
        {
            PeopleProfile profile = (PeopleProfile)obj1;
            PeopleProfile profile2 = (PeopleProfile)obj2;
            int num = 0;
            if (!string.IsNullOrEmpty(profile.firstName))
            {
                num = profile.FirstName.CompareTo(profile2.FirstName);
            }
            if ((num == 0) && !string.IsNullOrEmpty(profile.lastName))
            {
                num = profile.LastName.CompareTo(profile2.LastName);
            }
            return num;
        }

        public static string GetFirstLetterFromChinese(string str)
        {
            if (str.CompareTo("吖") < 0)
            {
                string s = str.Substring(0, 1).ToLowerInvariant();
                if (char.IsNumber(s, 0))
                {
                    return "#";
                }
                return s;
            }
            if (str.CompareTo("八") < 0)
            {
                return "A";
            }
            if (str.CompareTo("嚓") < 0)
            {
                return "B";
            }
            if (str.CompareTo("咑") < 0)
            {
                return "C";
            }
            if (str.CompareTo("妸") < 0)
            {
                return "D";
            }
            if (str.CompareTo("发") < 0)
            {
                return "E";
            }
            if (str.CompareTo("旮") < 0)
            {
                return "F";
            }
            if (str.CompareTo("哈") < 0)
            {
                return "G";
            }
            if (str.CompareTo("讥") < 0)
            {
                return "H";
            }
            if (str.CompareTo("咔") < 0)
            {
                return "J";
            }
            if (str.CompareTo("垃") < 0)
            {
                return "K";
            }
            if (str.CompareTo("嘸") < 0)
            {
                return "L";
            }
            if (str.CompareTo("拏") < 0)
            {
                return "M";
            }
            if (str.CompareTo("噢") < 0)
            {
                return "N";
            }
            if (str.CompareTo("妑") < 0)
            {
                return "O";
            }
            if (str.CompareTo("七") < 0)
            {
                return "P";
            }
            if (str.CompareTo("亽") < 0)
            {
                return "Q";
            }
            if (str.CompareTo("仨") < 0)
            {
                return "R";
            }
            if (str.CompareTo("他") < 0)
            {
                return "S";
            }
            if (str.CompareTo("哇") < 0)
            {
                return "T";
            }
            if (str.CompareTo("夕") < 0)
            {
                return "W";
            }
            if (str.CompareTo("丫") < 0)
            {
                return "X";
            }
            if (str.CompareTo("帀") < 0)
            {
                return "Y";
            }
            if (str.CompareTo("咗") < 0)
            {
                return "Z";
            }
            return "#";
        }

        public static string GetFirstNameKey(PeopleProfile person)
        {
            char c = '#';
            if (person != null)
            {
                c = string.IsNullOrEmpty(person.name) ? '#' : person.name[0];
                c = char.ToLower(c, LocalizedObjectHelper.CultureInfoCurrentUsed);
            }
            if ((c < 'a') || (c > 'z'))
            {
                c = GetFirstLetterFromChinese(person.name).ToLowerInvariant()[0];
                if ((c < 'a') || (c > 'z'))
                {
                    c = '#';
                }
            }
            return c.ToString();
        }

        public static void UpdateStructureAt_1_9_3(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<PeopleProfile>("HasCurrency");
        }

        [Association(Storage = "_peopleGroup", ThisKey = "GroupId", OtherKey = "Id"), XmlIgnore]
        public PeopleGroup AssociatedGroup
        {
            get
            {
                return this._peopleGroup.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("PeopleGroup");
                this._peopleGroup.Entity = value;
                if (value != null)
                {
                    bool flag = this.groupId != value.Id;
                    this.groupId = value.Id;
                }
                this.OnNotifyPropertyChanged("AssociatedGroup");
            }
        }

        public string AssociatedGroupNameInfo
        {
            get
            {
                return (((this.AssociatedGroup == null) ? "N/A" : this.AssociatedGroup.Name) + LocalizedObjectHelper.GetLocalizedStringFrom("Group").ToLowerInvariant());
            }
        }

        [XmlIgnore]
        public CurrencyWapper CurrencyInfo
        {
            get
            {
                if (this.currencyInfo == null)
                {
                    this.currencyInfo = CurrencyHelper.GetCurrencyItemByType(this.hasCurrency.GetValueOrDefault());
                }
                return CurrencyHelper.GetCurrencyItemByType(this.hasCurrency.GetValueOrDefault());
            }
            set
            {
                this.HasCurrency = new CurrencyType?(value.Currency);
                this.currencyInfo = value;
                this.OnNotifyPropertyChanged("CurrencyInfo");
            }
        }

        [Column]
        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                if (this.firstName != value)
                {
                    this.OnNotifyPropertyChanging("FirstName");
                    this.firstName = value;
                    this.OnNotifyPropertyChanged("FirstName");
                }
            }
        }

        [XmlIgnore]
        public string FullName
        {
            get
            {
                return (this.FirstName + " " + this.LastName);
            }
        }

        [Column]
        public System.Guid GroupId
        {
            get
            {
                return this.groupId;
            }
            set
            {
                if (this.groupId != value)
                {
                    this.OnNotifyPropertyChanging("GroupId");
                    this.groupId = value;
                    this.OnNotifyPropertyChanged("GroupId");
                }
            }
        }

        public string GroupNameDashName
        {
            get
            {
                if (this.AssociatedGroup == null)
                {
                    return this.name;
                }
                return "{0}->{1}".FormatWith(new object[] { this.AssociatedGroup.Name, this.name });
            }
        }

        [Column(CanBeNull = true)]
        public CurrencyType? HasCurrency
        {
            get
            {
                return this.hasCurrency;
            }
            set
            {
                if (this.hasCurrency != value)
                {
                    this.OnNotifyPropertyChanging("HasCurrency");
                    this.hasCurrency = value;
                    this.OnNotifyPropertyChanged("HasCurrency");
                }
            }
        }

        [Column]
        public string HomeAddress
        {
            get
            {
                return this.homeAddress;
            }
            set
            {
                if (this.homeAddress != value)
                {
                    this.OnNotifyPropertyChanging("HomeAddress");
                    this.homeAddress = value;
                    this.OnNotifyPropertyChanged("HomeAddress");
                }
            }
        }

        [Column(IsPrimaryKey = true, DbType = "UniqueIdentifier", CanBeNull = false, AutoSync = AutoSync.Default)]
        public System.Guid Id { get; set; }

        [Column]
        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                if (this.lastName != value)
                {
                    this.OnNotifyPropertyChanging("LastName");
                    this.lastName = value;
                    this.OnNotifyPropertyChanged("LastName");
                }
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
        public string PersonalEmail
        {
            get
            {
                return this.personalEmail;
            }
            set
            {
                if (this.personalEmail != value)
                {
                    this.OnNotifyPropertyChanging("PersonalEmail");
                    this.personalEmail = value;
                    this.OnNotifyPropertyChanged("PersonalEmail");
                }
            }
        }

        [Column]
        public string Phone
        {
            get
            {
                return this.phone;
            }
            set
            {
                if (this.phone != value)
                {
                    this.OnNotifyPropertyChanging("Phone");
                    this.phone = value;
                    this.OnNotifyPropertyChanged("Phone");
                }
            }
        }

        [Column]
        public string Telephone
        {
            get
            {
                return this.telephone;
            }
            set
            {
                if (this.telephone != value)
                {
                    this.OnNotifyPropertyChanging("Telephone");
                    this.telephone = value;
                    this.OnNotifyPropertyChanged("Telephone");
                }
            }
        }

        [Column]
        public string WorkingAddress
        {
            get
            {
                return this.workingAddress;
            }
            set
            {
                if (this.workingAddress != value)
                {
                    this.OnNotifyPropertyChanging("WorkingAddress");
                    this.workingAddress = value;
                    this.OnNotifyPropertyChanged("WorkingAddress");
                }
            }
        }

        [Column]
        public string WorkingEmail
        {
            get
            {
                return this.workingEmail;
            }
            set
            {
                if (this.workingEmail != value)
                {
                    this.OnNotifyPropertyChanging("WorkingEmail");
                    this.workingEmail = value;
                    this.OnNotifyPropertyChanged("WorkingEmail");
                }
            }
        }


        private decimal _totalOwnMe;
        public static string TotalOwnMeProperty = "TotalOwnMe";

        /// <summary>
        /// Gets or sets the total own me.
        /// </summary>
        /// <value>
        /// The total own me.
        /// </value>
        [XmlIgnore]
        public decimal TotalOwnMe
        {
            get { return _totalOwnMe; }
            set
            {
                if (value != _totalOwnMe)
                {
                    OnNotifyPropertyChanging(TotalOwnMeProperty);
                    _totalOwnMe = value;
                    OnAsyncNotifyPropertyChanged(TotalOwnMeProperty);
                    OnAsyncNotifyPropertyChanged("TotalOwnMeInfo");
                }
            }
        }

        private decimal _totalOwnHim;
        public static string TotalOwnHimProperty = "TotalOwnHim";

        /// <summary>
        /// Gets or sets the total own him.
        /// </summary>
        /// <value>
        /// The total own him.
        /// </value>
        [XmlIgnore]
        public decimal TotalOwnHim
        {
            get { return _totalOwnHim; }
            set
            {
                if (value != _totalOwnHim)
                {
                    OnNotifyPropertyChanging(TotalOwnHimProperty);
                    _totalOwnHim = value;
                    OnAsyncNotifyPropertyChanged(TotalOwnHimProperty);
                    OnAsyncNotifyPropertyChanged("TotalOwnHimInfo");
                }
            }
        }

        /// <summary>
        /// Gets or sets the total own me info.
        /// </summary>
        /// <value>
        /// The total own me info.
        /// </value>
        [XmlIgnore]
        public string TotalOwnMeInfo
        {
            get
            {
                return CurrencyExtensions.GetCurrencySymbolWithMoney(AppSetting.Instance.DefaultCurrency, this.TotalOwnMe);
            }
        }

        /// <summary>
        /// Gets or sets the total own him info.
        /// </summary>
        /// <value>
        /// The total own him info.
        /// </value>
        [XmlIgnore]
        public string TotalOwnHimInfo
        {
            get
            {
                return CurrencyExtensions.GetCurrencySymbolWithMoney(AppSetting.Instance.DefaultCurrency, this.TotalOwnHim);
            }
        }

        private string _loaningShortTitle;

        [XmlIgnore]
        public string LoaningShortTitle
        {
            get { return _loaningShortTitle; }
            set
            {
                if (value != _loaningShortTitle)
                {
                    OnNotifyPropertyChanging("LoaningShortTitle");
                    _loaningShortTitle = value;
                    OnAsyncNotifyPropertyChanged("LoaningShortTitle");
                }
            }
        }

        private string _loaningShortInfo;

        [XmlIgnore]
        public string LoaningShortInfo
        {
            get { return _loaningShortInfo; }
            set
            {
                _loaningShortInfo = value;
                OnAsyncNotifyPropertyChanged("LoaningShortInfo");
            }
        }


    }
}

