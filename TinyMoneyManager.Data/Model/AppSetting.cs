namespace TinyMoneyManager.Data.Model
{
    using NkjSoft.Extensions;
    using RapidRepository;
    using System;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;

    [Table]
    public class AppSetting : NotionObject, IRapidEntity
    {
        private CurrencyType _defaultCurrency;
        private bool _useBackgroundImageForMainPage;
        private AfterAddNewRecordAction actionAfterAddNewRecord;
        private bool alertWhenBudgetIsOver;
        private string appName;
        private bool autoBackupWhenAppUp;
        private string backgroundImageForMainPage;
        public static readonly string BackgroundImageForMainPageKey = "BackgroundImageForMainPage";
        private CurrencySymbolStyle currencySymbolStyle;
        private System.DayOfWeek dayOfWeekForEveryDayBackup;
        private string email;
        private bool enablePocketLock;
        private bool enableSimpleAccountItemEditing;
        public static readonly string EnableSimpleAccountItemEditingKey = "EnableSimpleAccountItemEditing";
        private bool enableUsingAccountWhenItHasNegativeValue;
        public static readonly string EnableUsingAccountWhenItHasNegativeValueKey = "EnableAllAccountOverdraft";
        public static readonly string GlobleCurrencySymbolStyleKey = "GlobleCurrencySymbolStyle";
        private bool ignoreCalimRecords;
        private static AppSetting instance;
        private string language;
        public static string MainPageBackgroundPictureFileName = "mainPageBackgroundIamge.jpg";
        private string password;
        private IPAddress serverSyncIPAddress;

        private bool showAssociatedAccountItemSummary;
        private bool showCashAmountOnAsset;
        private Visibility showFavorites;
        private bool showRepaymentInfoOnTile;
        public static readonly string ShowRepaymentInfoOnTileKey = "ShowRepaymentInfoOnTile";


        private BudgetStatsicSetting _BudgetStatsicSettings;

        /// <summary>
        /// The budget statsic settings property
        /// </summary>
        public static string BudgetStatsicSettingsProperty = "BudgetStatsicSettings";

        /// <summary>
        /// Gets or sets the budget statsic settings.
        /// </summary>
        /// <value>
        /// The budget statsic settings.
        /// </value>
        public BudgetStatsicSetting BudgetStatsicSettings
        {
            get { return _BudgetStatsicSettings; }
            set
            {
                if (value != _BudgetStatsicSettings)
                {
                    OnNotifyPropertyChanging(BudgetStatsicSettingsProperty);
                    _BudgetStatsicSettings = value;
                    if (value != null)
                    {
                        value.PropertyChanged -= value_PropertyChanged;
                        value.PropertyChanged += value_PropertyChanged;
                    }

                    OnNotifyPropertyChanged(BudgetStatsicSettingsProperty);
                }
            }
        }

        void value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BudgetStatsicSetting.BudgetStatsicModeProperty)
            {
                OnNotifyPropertyChanged(e.PropertyName);
            }
        }

        /// <summary>
        /// Initializes the <see cref="AppSetting" /> class.
        /// </summary>
        static AppSetting()
        {
            Workdays = new string[] { "monday", "tuesday", "wednesday", "thursday", "friday" };
            Weekends = new string[] { "sunday", "saturday" };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSetting" /> class.
        /// </summary>
        public AppSetting()
        {
            this.UserId = System.Guid.NewGuid();
            this.password = string.Empty;
            this.email = string.Empty;
            this.serverSyncIPAddress = new IPAddress("192.168.1.100", 0x271a);
            this.UseBackgroundImageForMainPage = false;
            this.EnableAllAccountOverdraft = true;
            this.GlobleCurrencySymbolStyle = CurrencySymbolStyle.LongWithName;
            this.Profile = new UserProfile();
            this.SubscibeNotification = false;
            this.AutoBackupWhenAppUp = true;
            this.DefaultAccount = System.Guid.Empty;
            this.dayOfWeekForEveryDayBackup = System.DayOfWeek.Sunday;
            this.showAssociatedAccountItemSummary = true;
            this.showCashAmountOnAsset = true;
            this.IncludeCreditCardAmountOnAsset = false;
            this.BudgetStatsicSettings = new BudgetStatsicSetting();
            this._VoiceCommandSettingMaximumValue = 1999;
            this._VoiceCommandSettingMininumValue = 1;
            this._VoiceCommandSettingWithDigits = false;
            this.CategorySelectionMode = CategorySelectionModes.SubCategorySelectionEnabled;
        }

        public static bool IsEmail(string emailText)
        {
            return Regex.IsMatch(emailText, EMailRegExp);
        }

        public static void
            LoadAppSetting(System.Action loadCurrencyTable)
        {
            try
            {
                AppSettingRepository instance = AppSettingRepository.Instance;
                AppSetting.instance = instance.FirstOfDefault();
                if (AppSetting.instance == null)
                {
                    string name = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                    AppSetting setting = new AppSetting
                    {
                        Id = System.Guid.NewGuid(),
                        EnablePoketLock = false,
                        DisplayLanguage = name,
                        DefaultCurrency = CurrencyHelper.ConvertCurrencyBy(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol),
                        UserId = System.Guid.NewGuid(),
                        EnableAllAccountOverdraft = true,
                        ShowRepaymentInfoOnTile = true,
                        FavoritesPageVisibiable = Visibility.Visible,
                        BudgetStatsicSettings = new BudgetStatsicSetting(),
                        EnableVoiceCommand = true,
                        VoiceCommandSettingUnitOfPrice = "",
                    };
                    AppSetting.instance = setting;
                    AppSetting.instance.AutoBackupWhenAppUp = true;
                    instance.Add(AppSetting.instance);


                    RapidContext.CurrentContext.SaveChanges();
                }

                if (AppSetting.instance.BudgetStatsicSettings == null)
                {
                    AppSetting.instance.BudgetStatsicSettings = new BudgetStatsicSetting();
                }

                AppSetting.instance.BudgetStatsicSettings.Calculate();
                AppSetting.instance.DisplayLanguage = AppSetting.instance.DisplayLanguage ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

                AppSetting.instance.ShowAssociatedAccountItemSummary = false;
                if (!LanguageType.SupportDisplayLanguages.Contains<string>(AppSetting.instance.DisplayLanguage))
                {
                    AppSetting.instance.DisplayLanguage = "en-US";
                }
                loadCurrencyTable();
            }
            catch (System.Exception)
            {
            }
        }

        public AccountCategoryWapper[] AccountCategoryWappers { get; set; }

        public AfterAddNewRecordAction ActionAfterAddNewRecord
        {
            get
            {
                return this.actionAfterAddNewRecord;
            }
            set
            {
                if (this.actionAfterAddNewRecord != value)
                {
                    this.OnNotifyPropertyChanging("ActionAfterAddNewRecord");
                    this.actionAfterAddNewRecord = value;
                    this.OnNotifyPropertyChanged("ActionAfterAddNewRecord");
                }
            }
        }

        public bool AlertWhenBudgetIsOver
        {
            get
            {
                return this.alertWhenBudgetIsOver;
            }
            set
            {
                if (this.alertWhenBudgetIsOver != value)
                {
                    this.OnNotifyPropertyChanging("AlertWhenBudgetIsOver");
                    this.alertWhenBudgetIsOver = value;
                    this.OnNotifyPropertyChanged("AlertWhenBudgetIsOver");
                }
            }
        }

        public string AppName
        {
            get
            {
                return this.appName;
            }
            set
            {
                if (this.appName != value)
                {
                    this.OnNotifyPropertyChanging("AppName");
                    this.appName = value;
                    this.OnNotifyPropertyChanged("AppName");
                }
            }
        }

        public bool AutoBackupWhenAppUp
        {
            get
            {
                return this.autoBackupWhenAppUp;
            }
            set
            {
                this.autoBackupWhenAppUp = value;
                this.OnNotifyPropertyChanged("AutoBackupWhenAppUp");
            }
        }

        public string BackgroundImageForMainPage
        {
            get
            {
                return this.backgroundImageForMainPage;
            }
            set
            {
                this.backgroundImageForMainPage = value;
                this.OnNotifyPropertyChanged(BackgroundImageForMainPageKey);
            }
        }

        public string CurrencyConversionRateTable { get; set; }

        public CurrencyWapper CurrencyInfo
        {
            get
            {
                return CurrencyHelper.GetCurrencyItemByType(this._defaultCurrency);
            }
            set
            {
                this.DefaultCurrency = value.Currency;
            }
        }

        public System.DayOfWeek DayOfWeekForEveryDayBackup
        {
            get
            {
                return this.dayOfWeekForEveryDayBackup;
            }
            set
            {
                this.dayOfWeekForEveryDayBackup = value;
                this.OnNotifyPropertyChanged("DayOfWeekForEveryDayBackup");
            }
        }

        public System.Guid DefaultAccount { get; set; }

        public CurrencyType DefaultCurrency
        {
            get
            {
                return this._defaultCurrency;
            }
            set
            {
                this._defaultCurrency = value;
                this.OnNotifyPropertyChanged("DefaultCurrency");
            }
        }

        [Column]
        public string DisplayLanguage
        {
            get
            {
                return this.language;
            }
            set
            {
                if (this.language != value)
                {
                    this.language = value;
                    if (LocalizedObjectHelper.GetLocalizedStringFrom != null)
                    {
                        this.AppName = LocalizedObjectHelper.GetLocalizedStringFrom("AppName");
                    }
                    else if (this.appName.IsNullOrEmpty())
                    {
                        this.AppName = "ACCOUNT BOOK";
                        if ((System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-TW") || (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-HK"))
                        {
                            this.AppName = "手機帳本";
                        }
                        else if ((System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN") || (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-SG"))
                        {
                            this.AppName = "手机账本";
                        }
                    }
                    this.OnNotifyPropertyChanged("DisplayLanguage");
                }
            }
        }

        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                if (!IsEmail(value))
                {
                    value = string.Empty;
                }
                this.email = value;
                this.OnNotifyPropertyChanged("Email");
            }
        }

        public static string EMailRegExp
        {
            get
            {
                return @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
            }
        }

        public bool EnableAllAccountOverdraft
        {
            get
            {
                return this.enableUsingAccountWhenItHasNegativeValue;
            }
            set
            {
                this.enableUsingAccountWhenItHasNegativeValue = value;
                this.OnNotifyPropertyChanged(EnableUsingAccountWhenItHasNegativeValueKey);
            }
        }

        public bool EnablePoketLock
        {
            get
            {
                return this.enablePocketLock;
            }
            set
            {
                this.enablePocketLock = value;
                this.OnNotifyPropertyChanged("EnablePoketLock");
            }
        }

        private int _categorySelectionModeIndex;

        /// <summary>
        /// Gets or sets the index of the category selection mode.
        /// </summary>
        /// <value>
        /// The index of the category selection mode.
        /// </value>
        public int CategorySelectionModeIndex
        {
            get { return (int)_categorySelectionMode; }
            set
            {
                if (value != _categorySelectionModeIndex)
                {
                    OnNotifyPropertyChanging("CategorySelectionModeIndex");
                    _categorySelectionModeIndex = value;
                    CategorySelectionMode = value.ToEnum<CategorySelectionModes>();
                    OnNotifyPropertyChanged("CategorySelectionModeIndex");
                }
            }
        }

        private CategorySelectionModes _categorySelectionMode;
        public static string CategorySelectionModeProperty = "CategorySelectionMode";
        public CategorySelectionModes CategorySelectionMode
        {
            get { return _categorySelectionMode; }
            set
            {
                if (value != _categorySelectionMode)
                {
                    OnNotifyPropertyChanging(CategorySelectionModeProperty);
                    _categorySelectionMode = value;
                    OnNotifyPropertyChanged(CategorySelectionModeProperty);
                }
            }
        }

        public bool EnableSimpleAccountItemEditing
        {
            get
            {
                return this.enableSimpleAccountItemEditing;
            }
            set
            {
                this.enableSimpleAccountItemEditing = value;
                this.OnNotifyPropertyChanged(EnableSimpleAccountItemEditingKey);
            }
        }

        public Visibility FavoritesPageVisibiable
        {
            get
            {
                return this.showFavorites;
            }
            set
            {
                if (value != this.showFavorites)
                {
                    this.OnNotifyPropertyChanging("FavoritesPageVisibiable");
                    this.showFavorites = value;
                    this.OnNotifyPropertyChanged("FavoritesPageVisibiable");
                }
            }
        }

        public CurrencySymbolStyle GlobleCurrencySymbolStyle
        {
            get
            {
                return this.currencySymbolStyle;
            }
            set
            {
                this.currencySymbolStyle = value;
                this.OnNotifyPropertyChanged(GlobleCurrencySymbolStyleKey);
            }
        }

        public bool HasEmail
        {
            get
            {
                return (this.Email.Trim().Length > 0);
            }
        }

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public System.Guid Id { get; set; }

        public bool IgnoreCalimRecords
        {
            get
            {
                return this.ignoreCalimRecords;
            }
            set
            {
                if (this.ignoreCalimRecords != value)
                {
                    this.OnNotifyPropertyChanging("IgnoreCalimRecords");
                    this.ignoreCalimRecords = value;
                    this.OnNotifyPropertyChanged("IgnoreCalimRecords");
                }
            }
        }

        public static AppSetting Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        [XmlIgnore]
        public bool IsFavoritesPageVisibiable
        {
            get
            {
                return (this.FavoritesPageVisibiable == Visibility.Visible);
            }
            set
            {
                this.FavoritesPageVisibiable = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
                this.OnNotifyPropertyChanged("Password");
            }
        }

        public UserProfile Profile { get; set; }

        public IPAddress ServerSyncIPAddress
        {
            get
            {
                return this.serverSyncIPAddress;
            }
            set
            {
                this.serverSyncIPAddress = value;
                this.OnNotifyPropertyChanged("ServerSyncIPAddress");
            }
        }

        public bool ShowAssociatedAccountItemSummary
        {
            get
            {
                return this.showAssociatedAccountItemSummary;
            }
            set
            {
                this.showAssociatedAccountItemSummary = value;
                this.OnNotifyPropertyChanged("ShowAssociatedAccountItemSummary");
            }
        }

        public bool ShowCashAmountOnAsset
        {
            get
            {
                return this.showCashAmountOnAsset;
            }
            set
            {
                if (this.showCashAmountOnAsset != value)
                {
                    this.OnNotifyPropertyChanging("ShowCashAmountOnAsset");
                    this.showCashAmountOnAsset = value;
                    this.OnNotifyPropertyChanged("ShowCashAmountOnAsset");
                }
            }
        }

        public bool ShowRepaymentInfoOnTile
        {
            get
            {
                return this.showRepaymentInfoOnTile;
            }
            set
            {
                this.showRepaymentInfoOnTile = value;
                this.OnNotifyPropertyChanged(ShowRepaymentInfoOnTileKey);
            }
        }

        public bool SubscibeNotification { get; set; }

        public bool UseBackgroundImageForMainPage
        {
            get
            {
                return this._useBackgroundImageForMainPage;
            }
            set
            {
                this._useBackgroundImageForMainPage = value;
                this.OnNotifyPropertyChanged("UseBackgroundImageForMainPage");
            }
        }

        public System.Guid UserId { get; set; }

        public static string[] Weekends
        {
            get;
            set;
        }

        public static string[] Workdays
        {
            get;
            set;
        }

        private bool _IncludeCreditCardAmountOnAsset;

        /// <summary>
        /// Gets or sets a value indicating whether [include credit card amount on asset].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include credit card amount on asset]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeCreditCardAmountOnAsset
        {
            get
            {
                return this._IncludeCreditCardAmountOnAsset;
            }
            set
            {
                this.OnNotifyPropertyChanging("IncludeCreditCardAmountOnAsset");
                this._IncludeCreditCardAmountOnAsset = value;
                this.OnNotifyPropertyChanged("IncludeCreditCardAmountOnAsset");
            }
        }

        private bool _EnableVoiceCommand;
        public static string EnableVoiceCommandProperty = "EnableVoiceCommand";
        public bool EnableVoiceCommand
        {
            get { return _EnableVoiceCommand; }
            set
            {
                if (value != _EnableVoiceCommand)
                {
                    OnNotifyPropertyChanging(EnableVoiceCommandProperty);
                    _EnableVoiceCommand = value;
                    OnNotifyPropertyChanged(EnableVoiceCommandProperty);
                }
            }
        }

        private bool _VoiceCommandSettingWithDigits;
        public static string VoiceCommandSettingWithDigitsProperty = "VoiceCommandSettingWithDigits";

        /// <summary>
        /// Gets or sets a value indicating whether [voice command setting with digits].
        /// </summary>
        /// <value>
        /// <c>true</c> if [voice command setting with digits]; otherwise, <c>false</c>.
        /// </value>
        public bool VoiceCommandSettingWithDigits
        {
            get { return _VoiceCommandSettingWithDigits; }
            set
            {
                if (value != _VoiceCommandSettingWithDigits)
                {
                    OnNotifyPropertyChanging(VoiceCommandSettingWithDigitsProperty);
                    _VoiceCommandSettingWithDigits = value;
                    OnNotifyPropertyChanged(VoiceCommandSettingWithDigitsProperty);
                    OnNotifyPropertyChanged("VoiceCommandSettingWithDigitsText");
                }
            }
        }

        private double _VoiceCommandSettingMaximumValue;
        public static string VoiceCommandSettingMaximumValueProperty = "VoiceCommandSettingMaximumValue";

        /// <summary>
        /// Gets or sets the voice command setting maximum value.
        /// </summary>
        /// <value>
        /// The voice command setting maximum value.
        /// </value>
        public double VoiceCommandSettingMaximumValue
        {
            get { return _VoiceCommandSettingMaximumValue; }
            set
            {
                if (value != _VoiceCommandSettingMaximumValue)
                {
                    OnNotifyPropertyChanging(VoiceCommandSettingMaximumValueProperty);
                    _VoiceCommandSettingMaximumValue = value;
                    OnNotifyPropertyChanged(VoiceCommandSettingMaximumValueProperty);
                }
            }
        }

        private double _VoiceCommandSettingMininumValue;
        public static string VoiceCommandSettingMininumValueProperty = "VoiceCommandSettingMininumValue";

        /// <summary>
        /// Gets or sets the voice command setting mininum value.
        /// </summary>
        /// <value>
        /// The voice command setting mininum value.
        /// </value>
        public double VoiceCommandSettingMininumValue
        {
            get { return _VoiceCommandSettingMininumValue; }
            set
            {
                if (value != _VoiceCommandSettingMininumValue)
                {
                    OnNotifyPropertyChanging(VoiceCommandSettingMininumValueProperty);
                    _VoiceCommandSettingMininumValue = value;
                    OnNotifyPropertyChanged(VoiceCommandSettingMininumValueProperty);
                }
            }
        }

        /// <summary>
        /// Gets the voice command setting with digits text.
        /// </summary>
        /// <value>
        /// The voice command setting with digits text.
        /// </value>
        public string VoiceCommandSettingWithDigitsText
        {
            get
            {
                return VoiceCommandSettingWithDigits ? "（支持说 .5 的小数部分）" : "";
            }
        }

        private string _VoiceCommandSetting_CommandPrefix;

        public string VoiceCommandSetting_CommandPrefix
        {
            get { return _VoiceCommandSetting_CommandPrefix; }
            set
            {
                if (value != _VoiceCommandSetting_CommandPrefix)
                {
                    OnNotifyPropertyChanging("VoiceCommandSetting_CommandPrefix");
                    _VoiceCommandSetting_CommandPrefix = value;
                    OnNotifyPropertyChanged("VoiceCommandSetting_CommandPrefix");
                }
            }
        }

        private string _VoiceCommandSettingUnitOfPrice;

        /// <summary>
        /// Gets or sets the voice command setting unit of price.
        /// </summary>
        /// <value>
        /// The voice command setting unit of price.
        /// </value>
        public string VoiceCommandSettingUnitOfPrice
        {
            get
            {

                var result = _VoiceCommandSettingUnitOfPrice;

                if (result.IsNullOrEmpty())
                {
                    result = GetMoneyUnitOfLanguage(this.DisplayLanguage.IsNullOrEmpty() ? "en-US" : this.DisplayLanguage);
                }

                _VoiceCommandSettingUnitOfPrice = result;

                return _VoiceCommandSettingUnitOfPrice;
            }
            set
            {
                if (value != _VoiceCommandSettingUnitOfPrice)
                {
                    OnNotifyPropertyChanging("VoiceCommandSettingUnitOfPrice");
                    _VoiceCommandSettingUnitOfPrice = value;
                    OnNotifyPropertyChanged("VoiceCommandSettingUnitOfPrice");
                }
            }
        }

        public static string GetMoneyUnitOfLanguage(string lang)
        {
            var moneyUnit = "";

            if (moneyUnit.IsNullOrEmpty())
            {
                switch (lang.ToLower())
                {
                    // by default, to use en-us voice conmmand.
                    default:
                    case "en":
                        moneyUnit = "dollars";
                        break;
                    case "zh-cn":
                    case "zh-sg":
                        moneyUnit = "元";
                        break;
                    case "zh-tw":
                    case "zh-hk":
                        moneyUnit = "元";
                        break;
                }
            }

            return moneyUnit;
        }

    }
}

