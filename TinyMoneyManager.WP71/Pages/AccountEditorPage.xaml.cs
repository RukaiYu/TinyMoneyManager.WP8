namespace TinyMoneyManager.Pages
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.Pages.DialogBox;
    using System.ComponentModel;

    public partial class AccountEditorPage : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
    {

        private AccountViewModel accountViewModel;
        private ApplicationBarHelper applicationBarHelper;

        private decimal? newInitialBalance = null;
        public PageActionType pageAction;

        private Account _current;

        public Account Current
        {
            get { return _current; }
            set
            {
                if (value != _current)
                {
                    OnPropertyChanging("Current");
                    _current = value;
                    OnPropertyChanged("Current");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        public AccountEditorPage()
        {
            this.InitializeComponent();
            this.accountViewModel = ViewModelLocator.AccountViewModel;
            TiltEffect.SetIsTiltEnabled(this, true);
            this.AccountCategory.ItemsSource = AccountCategoryHelper.AccountCategories;
            this.AccountCategory.ItemCountThreshold = 7;
            this.CurrencyType.ItemsSource = CurrencyHelper.CurrencyTable;
            this.LoadContent();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.SafeGoBack();
        }

        private void CurrencyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender != null) && (this.Current != null))
            {
                CurrencyWapper selectedItem = (sender as ListPicker).SelectedItem as CurrencyWapper;
                if (selectedItem != null)
                {
                    this.Balance.Text = AccountItemMoney.GetMoneyInfoWithCurrency(selectedItem.Currency, this.Current.Balance, "{0}{1}");
                }
            }
        }

        private void InitialBalanceInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            decimal num = this.InitialBalanceInputBox.Text.ToDecimal();
            if (this.Current != null)
            {
                if (num == this.Current.InitialBalance)
                {
                    return;
                }
            }
            this.newInitialBalance = new decimal?(num);
            base.Focus();
        }

        private void LoadContent()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Save;
            base.ApplicationBar.GetIconButtonFrom(1).Text = AppResources.Cancel;
            this.applicationBarHelper = new ApplicationBarHelper(this);
            this.applicationBarHelper.AddTextBox(new TextBox[] { this.AccountName, this.TransferingPoundage, this.InitialBalanceInputBox, LineOfCredit })
                .SelectContentWhenFocus = true;
            this.applicationBarHelper.OriginalBar = base.ApplicationBar;
        }

        public void LoadEditing()
        {
            if (this.Current != null)
            {
                this.AccountName.Text = this.Current.Name;
                this.AccountCategory.IsEnabled = this.Current.CanChangeCategory;
                this.AccountCategory.SelectedIndex = (int)this.Current.Category;
                this.CurrencyType.SelectedItem = this.Current.CurrencyInfo;
                this.Balance.Text = this.Current.BalanceInfoWithoutTag;
                this.InitialBalanceInputBox.Text = this.Current.InitialBalance.GetValueOrDefault().ToMoneyF2();
                this.TransferingPoundage.Text = this.Current.TransferingPoundageRateInfo;
                this.LineOfCredit.Text = this.Current.LineOfCredit.GetValueOrDefault().ToMoneyF2();
                setPaymentDueDateInfo(this.Current.PaymentDueDay.GetValueOrDefault());

                base.DataContext = this;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string id = e.GetNavigatingParameter("id", null);
                this.pageAction = e.GetNavigatingParameter("action", null).ToEnum<PageActionType>();
                if (this.pageAction == PageActionType.Add)
                {
                    this.Current = new Account();
                    this.CategoryManagementPageTitle.Text = LocalizedStrings.GetCombinedText(AppResources.Create, AppResources.AccountName, false).ToUpperInvariant();
                }
                else
                {
                    this.Current = this.accountViewModel.Accounts.FirstOrDefault<Account>(p => p.Id == id.ToGuid());
                    this.CategoryManagementPageTitle.Text = LocalizedStrings.GetCombinedText(AppResources.Edit, AppResources.AccountInfo, false).ToUpperInvariant();
                    this.LoadEditing();
                }

                this.DataContext = this;
            }
        }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            if (this.AccountName.Text.Trim().Length == 0)
            {
                string text = LocalizedStrings.GetCombinedText(AppResources.AccountName, AppResources.EmptyTextMessage, false);
                this.Alert(text, null);
                this.AccountName.Focus();
            }
            else if (this.accountViewModel.ExistAccount(this.pageAction, this.Current))
            {
                this.Alert(AppResources.RecordAlreadyExist, null);
                this.AccountName.Focus();
            }
            else
            {
                if (this.pageAction == PageActionType.Add)
                {
                    this.Current.Id = System.Guid.NewGuid();
                    this.Current.Balance = 0;
                }
                this.Current.Name = this.AccountName.Text;
                this.Current.CurrencyInfo = this.CurrencyType.SelectedItem as CurrencyWapper;
                this.Current.Category = (TinyMoneyManager.Data.Model.AccountCategory)this.AccountCategory.SelectedIndex;
                this.Current.Poundage = this.TransferingPoundage.Text.ToDecimal();

                this.Current.PaymentDueDay = this.PaymentDueDate_EveryMonth_Day_Value.Tag.ToString().ToInt32();

                this.Current.LineOfCredit = LineOfCredit.Text.ToDecimal();

                if (this.newInitialBalance.HasValue)
                {
                    decimal? newInitialBalance = this.newInitialBalance;
                    decimal? initialBalance = this.Current.InitialBalance;

                    if ((newInitialBalance.GetValueOrDefault() != initialBalance.GetValueOrDefault()) || (newInitialBalance.HasValue != initialBalance.HasValue))
                    {
                        this.Current.InitialBalance = this.newInitialBalance.Value;
                        this.Current.InitialDateTime = new System.DateTime?(System.DateTime.Now);
                    }
                }

                if (this.pageAction == PageActionType.Add)
                {
                    if (this.newInitialBalance.HasValue)
                    {
                        this.Current.Balance = this.newInitialBalance;
                    }

                    this.accountViewModel.Accounts.Add(this.Current);
                    this.accountViewModel.AccountBookDataContext.Accounts.InsertOnSubmit(this.Current);
                }

                this.accountViewModel.UpdateOnSubmit(this.Current);
            }
            this.SafeGoBack();
        }

        private void PaymentDueDate_EveryMonth_Day_Value_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DaySelectorPage.AfterConfirmed = delegate(int v)
            {
                setPaymentDueDateInfo(v);
            };

            this.NavigateTo("/Pages/DialogBox/DaySelectorPage.xaml?title={0}&defValue={1}", new object[] { AppResources.SelectDayOfMonth, 
                this.PaymentDueDate_EveryMonth_Day_Value.Tag.ToString().ToInt32() });

        }

        private void setPaymentDueDateInfo(int v)
        {
            this.PaymentDueDate_EveryMonth_Day_Value.Tag = v;
            this.PaymentDueDate_EveryMonth_Day_Value.Text = AppResources.FrequencyDayOfMonthFormatter.FormatWith(new object[] { v });
        }

        private void AccountCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var categoryType = (TinyMoneyManager.Data.Model.AccountCategory)this.AccountCategory.SelectedIndex;

            if (this.Current != null)
            {
                this.Current.Category = categoryType;
            }
        }

    }
}

