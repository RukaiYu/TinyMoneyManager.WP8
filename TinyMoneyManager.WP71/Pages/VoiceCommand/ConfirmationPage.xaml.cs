using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using AccountBook.VoiceCommandData;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;
using TinyMoneyManager.Component.Common;
using NkjSoft.WPhone.Extensions;
namespace TinyMoneyManager.Pages.VoiceCommand
{
    public partial class ConfirmationPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        /// <summary>
        /// The _recognizer
        /// </summary>
        SpeechRecognizerUI _recognizer = new Windows.Phone.Speech.Recognition.SpeechRecognizerUI();

        /// <summary>
        /// The speaker
        /// </summary>
        SpeechSynthesizer speaker = new Windows.Phone.Speech.Synthesis.SpeechSynthesizer();

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        public string AccountName
        {
            get { return (string)GetValue(AccountNameProperty); }
            set { SetValue(AccountNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccountName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccountNameProperty =
            DependencyProperty.Register("AccountName", typeof(string), typeof(ConfirmationPage), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        /// <value>
        /// The name of the category.
        /// </value>
        public string CategoryName
        {
            get { return (string)GetValue(CategoryNameProperty); }
            set { SetValue(CategoryNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CategoryName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryNameProperty =
            DependencyProperty.Register("CategoryName", typeof(string), typeof(ConfirmationPage), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the money.
        /// </summary>
        /// <value>
        /// The money.
        /// </value>
        public string Money
        {
            get { return (string)GetValue(MoneyProperty); }
            set { SetValue(MoneyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Money.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoneyProperty =
            DependencyProperty.Register("Money", typeof(string), typeof(ConfirmationPage), new PropertyMetadata("0.00"));

        /// <summary>
        /// Gets or sets the type of the item.
        /// </summary>
        /// <value>
        /// The type of the item.
        /// </value>
        public ItemType ItemType
        {
            get { return (ItemType)GetValue(ItemTypeProperty); }
            set
            {
                SetValue(ItemTypeProperty, value);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ItemTypeString"));
                    PropertyChanged(this, new PropertyChangedEventArgs("PageTitle"));
                }
            }
        }

        // Using a DependencyProperty as the backing store for ItemType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.Register("ItemType", typeof(ItemType), typeof(ConfirmationPage), new PropertyMetadata(ItemType.Expense));

        /// <summary>
        /// Gets the item type string.
        /// </summary>
        /// <value>
        /// The item type string.
        /// </value>
        public string ItemTypeString
        {
            get
            {
                return LocalizedStrings.GetLanguageInfoByKey(ItemType.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public string Notes
        {
            get { return (string)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Notes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(string), typeof(ConfirmationPage), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the people.
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        public string People
        {
            get { return (string)GetValue(PeopleProperty); }
            set { SetValue(PeopleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for People.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PeopleProperty =
            DependencyProperty.Register("People", typeof(string), typeof(ConfirmationPage), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(ConfirmationPage), new PropertyMetadata(DateTime.Now));
        private string _moneyUnit;

        /// <summary>
        /// Gets the page title.
        /// </summary>
        /// <value>
        /// The page title.
        /// </value>
        public string PageTitle
        {
            get
            {
                return ItemTypeString;// LocalizedStrings.GetCombinedText(ItemTypeString, AppResources.Details).ToLower();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationPage" /> class.
        /// </summary>
        public ConfirmationPage()
        {
            InitializeComponent();

            _recognizer.Settings.ExampleText = AppResources.VoiceCommandConfirm_OkOrCancel;
            _recognizer.Settings.ListenText = AppResources.VoiceCommandConfirm_OkToCreate;

            DataContext = this;

            var voiceForRecognizer = InstalledSpeechRecognizers.Default;

            _recognizer.Recognizer.SetRecognizer(voiceForRecognizer);

            var voice = InstalledVoices.Default;

            if (voice == null)
            {
                voice = InstalledVoices.Default;
            }

            speaker.SetVoice(voice);

            AccountBook.VoiceCommandData.Manager.Instance.LoadSGRM(_recognizer, voiceForRecognizer.Language);

            _moneyUnit = VoiceCommandSettingUnitOfPrice(voiceForRecognizer.Language);
            Loaded += ConfirmationPage_Loaded;

            InitializeMenu();
        }

        private void InitializeMenu()
        {
            this.ApplicationBar.GetIconButtonFrom(0)
                .Text = AppResources.Save;

            this.ApplicationBar.GetIconButtonFrom(0)
                   .IsEnabled = false;

            this.ApplicationBar.GetIconButtonFrom(1)
                .Text = AppResources.Close;
        }

        /// <summary>
        /// Handles the Loaded event of the ConfirmationPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void ConfirmationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var moneyValue = new decimal(0.0);

            decimal.TryParse(Money, out moneyValue);

            Money = moneyValue.ToMoneyF2();

            await speaker.SpeakTextAsync(GetTextToConfirm());

            var result = await _recognizer.RecognizeWithUIAsync();

            if (result.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
            {
                App.appSession.tagEvent("EventOf_Using_VoiceCommand");

                var isOK = (bool)result.RecognitionResult.Semantics["Result"].Value;

                var ruleName = result.RecognitionResult.RuleName;
                if (isOK)
                {
                    this.BusyForWork(AppResources.VoiceCommandConfirm_CreatingItem);
                    await speaker.SpeakTextAsync(AppResources.VoiceCommandConfirm_CreatingItem);
                    var accountItem = await Manager.Instance.DetectAccountItem(AccountName, CategoryName, ItemType, moneyValue, Notes, People);

                    var succeed = true;

                    if (accountItem == null)
                    {
                        succeed = false;
                    }

                    try
                    {
                        succeed = Manager.Instance.ScheduleCreate(accountItem);
                    }
                    catch (Exception)
                    {
                        succeed = false;
                    }

                    this.WorkDone();
                    var msg = AppResources.VoiceCommandConfirm_ItemIsCreated.FormatWith((succeed ? AppResources.Succeed : AppResources.Failed)) + "！";
                    this.AlertNotification(msg);
                    this.ApplicationBar.GetIconButtonFrom(0)
                        .IsEnabled = false;
                    await speaker.SpeakTextAsync(msg);
                }
            }
        }

        /// <summary>
        /// Gets the text to confirm.
        /// </summary>
        /// <returns></returns>
        private string GetTextToConfirm()
        {
            //
            // 现金，早餐，5元 的支出记录
            var msgFormatter = AppResources.VoiceCommandConfirm_Summary;

            return msgFormatter.FormatWith(
             AccountName, CategoryName, Money, this.ItemTypeString, _moneyUnit);
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("voiceCommandName"))
            {
                // If so, get theon name of the voice command.
                string voiceCommandName
                  = NavigationContext.QueryString["voiceCommandName"];

                // Define app actions for each voice command name.
                switch (voiceCommandName)
                {
                    case "newRecord_income":
                    case "newRecord_expense":
                        this.CategoryName = NavigationContext.QueryString["category"];
                        this.AccountName = NavigationContext.QueryString["account"];
                        this.Money = NavigationContext.QueryString["number"];
                        this.ItemType = NavigationContext.QueryString["type"].ToEnum<ItemType>(Component.ItemType.Expense);
                        // Add code to display specified widgets.
                        break;

                    // Add cases for other voice commands. 
                    default:
                        // There is no match for the voice command name.
                        break;
                }
            }
            base.OnNavigatedTo(e);
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.SafeGoBack();
        }

        private void SaveAndClose_Click(object sender, EventArgs e)
        {
            try
            {
                speaker.CancelAll();
            }
            catch (Exception)
            {
            }
        }

        private void Edit_Click(object sender, EventArgs e)
        {

        }

        public static string VoiceCommandSettingUnitOfPrice(string lang)
        {
            var moneyUnit = AppSetting.Instance.VoiceCommandSettingUnitOfPrice;

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