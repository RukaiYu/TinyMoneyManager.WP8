namespace TinyMoneyManager.Pages
{
    using Coding4Fun.Phone.Controls;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using Phone.Controls;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public partial class AccountTransferingPage : PhoneApplicationPage
    {
        private int accountDialogSelectedIndex;
        private PickerBoxDialog accountSelectorDialogBox;
        private string amountOverAccountBalanceMessage = string.Empty;

        public decimal amountToUpdate;
        private IApplicationBar applicationBarForTransfer;
        private string canTransferToSameAccount = string.Empty;
        private int chooseRequireSender;

        private ApplicationBarHelper editingBarManager;

        private string fromAccountNotEnoughMessage = string.Empty;
        private bool hasInitializedTransfering;
        private bool hasNavigated;
        private bool isClosed = true;

        private string mainTitle = string.Empty;

        private string operationSuccessfullyMessage = string.Empty;
        private int rollInAccountIndex;

        private string rollInTitle = " 转入 ";
        private int rollOutAccountIndex;
        private string rollOutTitle = " 转出 ";

        private ApplicationBarIconButton transerButton;
        private Account transferFrom;

        private decimal transferingPoundageAmount = 0.0M;

        private Account transferTo;
        public static string ViewHistoryActionName = "ViewHistory";

        public static bool IsSelectionMode { get; set; }

        private TransferingItem _transferingItem;

        public AccountTransferingPage()
        {
            this.InitializeComponent();
            this.initializeApplicationBar();
            this.rollInTitle = this.GetLanguageInfoByKey("RollIn");
            this.rollOutTitle = this.GetLanguageInfoByKey("RollOut");
            this.mainTitle = this.GetLanguageInfoByKey("AccountSelectorHeader");
            this.fromAccountNotEnoughMessage = this.rollOutTitle + this.GetLanguageInfoByKey("AccountBalanceNotEnoughMessage");
            this.canTransferToSameAccount = this.GetLanguageInfoByKey("CanTransferToSameAccount");
            this.operationSuccessfullyMessage = this.GetLanguageInfoByKey("OperationSuccessfullyMessage");
            this.amountOverAccountBalanceMessage = this.GetLanguageInfoByKey("AmountOverAccountBalanceMessage");
            this.InitializeTitle();
            this.InitPickerBoxDialog();
            TiltEffect.SetIsTiltEnabled(this, true);
            base.Loaded += new RoutedEventHandler(this.AccountTransferingPage_Loaded);
            this.TransferApplicationBar(0);
            this.CreateDate.Value = new System.DateTime?(System.DateTime.Now);
        }

        private void AccountTransferingPage_Loaded(object sender, RoutedEventArgs e)
        {
            System.Action a = null;
            if (!this.hasInitializedTransfering)
            {
                if (a == null)
                {
                    a = delegate
                    {
                        this.rollOutAccountIndex = 0;
                        this.rollInAccountIndex = (ViewModelLocator.AccountViewModel.Accounts.Count > 1) ? 1 : 0;
                        this.transferFrom = ViewModelLocator.AccountViewModel.Accounts[this.rollOutAccountIndex];
                        this.SelectRollOutAccountButton.DataContext = this.transferFrom;
                        this.transferTo = ViewModelLocator.AccountViewModel.Accounts[this.rollInAccountIndex];
                        this.SelectRollInAccountButton.DataContext = this.transferTo;
                        this.transerButton.IsEnabled = this.rollInAccountIndex != this.rollOutAccountIndex;
                        this.SetTransferingInfo();
                        this.hasInitializedTransfering = true;
                        this.MoneyCurrency.Text = this.transferFrom.CurrencyType.GetCurrentString();
                    };
                }
                base.Dispatcher.BeginInvoke(a);
            }
        }

        private void calculateAmountChanges()
        {
            decimal amount = this.AmountTextBox.Text.ToDecimal();
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {

                CurrencyType currencyFrom = this.transferFrom.Currency;
                CurrencyType currency = this.transferTo.Currency;
                this.amountToUpdate = currencyFrom.GetConversionRateTo(currency) * amount;
                string amountWouldBeChanged = "{0}{1}".FormatWith(new object[] { this.transferTo.CurrencyTypeSymbol, (this.amountToUpdate + this.transferTo.Balance.Value).ToMoneyF2() });
                this.Dispatcher.BeginInvoke(delegate
                {
                    this.TipsOfAmountChangesBlock.Text = "{0} {1}".FormatWith(new object[] { AppResources.AmountWillAddedToRepayToAccountMessage, amountWouldBeChanged });
                });
            });
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            base.NavigationService.GoBack();
        }

        private bool CheckIsTransferEnable()
        {
            bool flag = this.rollInAccountIndex != this.rollOutAccountIndex;
            if (this.rollInAccountIndex == this.rollOutAccountIndex)
            {
                this.ShowToasteMessage(this.canTransferToSameAccount, null);
                return flag;
            }
            flag = this.transferFrom.Balance > 0.0M;
            if (flag)
            {
                return flag;
            }
            if (this.transferFrom.Category != AccountCategory.CreditCard)
            {
                this.ShowToasteMessage(this.fromAccountNotEnoughMessage, null);
                return flag;
            }
            return true;
        }

        private void customDialog_Closed(object sender, System.EventArgs e)
        {
            if (!this.isClosed)
            {
                bool flag = false;
                if (this.chooseRequireSender == 0)
                {
                    this.rollOutAccountIndex = this.accountSelectorDialogBox.SelectedIndex;
                    this.transferFrom = ViewModelLocator.AccountViewModel.Accounts[this.rollOutAccountIndex];
                    this.SelectRollOutAccountButton.DataContext = this.transferFrom;
                }
                else if (this.chooseRequireSender == 1)
                {
                    this.rollInAccountIndex = this.accountSelectorDialogBox.SelectedIndex;
                    this.transferTo = ViewModelLocator.AccountViewModel.Accounts[this.rollInAccountIndex];
                    this.SelectRollInAccountButton.DataContext = this.transferTo;
                }
                this.isClosed = true;
                flag = this.CheckIsTransferEnable();
                this.SetTransferingInfo();
                this.transerButton.IsEnabled = flag;
            }
        }

        private void ExchangeAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.rollInAccountIndex != this.rollOutAccountIndex)
            {
                int rollOutAccountIndex = this.rollOutAccountIndex;
                this.rollOutAccountIndex = this.rollInAccountIndex;
                this.rollInAccountIndex = rollOutAccountIndex;
                this.transferFrom = ViewModelLocator.AccountViewModel.Accounts[this.rollOutAccountIndex];
                this.SelectRollOutAccountButton.DataContext = this.transferFrom;
                this.transferTo = ViewModelLocator.AccountViewModel.Accounts[this.rollInAccountIndex];
                this.SelectRollInAccountButton.DataContext = this.transferTo;
                this.transerButton.IsEnabled = this.CheckIsTransferEnable();
                this.MoneyCurrency.Text = this.transferFrom.CurrencyType.GetCurrentString();
                this.SetTransferingInfo();
            }
        }

        private void initializeApplicationBar()
        {
            this.transerButton = new ApplicationBarIconButton();
            this.transerButton.IconUri = new Uri("/icons/appbar.transfering.rest.png", UriKind.RelativeOrAbsolute);
            this.transerButton.Text = this.GetLanguageInfoByKey("TransferingAccount");
            this.transerButton.IsEnabled = false;
            this.transerButton.Click += new System.EventHandler(this.TransferButton_Click);
            ApplicationBarIconButton button = new ApplicationBarIconButton();
            button.Click += new System.EventHandler(this.CancelButton_Click);
            button.Text = this.GetLanguageInfoByKey("Cancel");
            button.IconUri = new Uri("/icons/appbar.cancel.rest.png", UriKind.RelativeOrAbsolute);
            ApplicationBar bar = new ApplicationBar
            {
                Opacity = 0.78
            };
            this.applicationBarForTransfer = bar;
            this.applicationBarForTransfer.Buttons.Add(this.transerButton);
            this.applicationBarForTransfer.Buttons.Add(button);
            this.editingBarManager = new ApplicationBarHelper(this);
            this.editingBarManager.OriginalBar = this.applicationBarForTransfer;
            this.editingBarManager.AddTextBox(new TextBox[] { this.AmountTextBox, this.Description, this.TransferingPoundage });
            base.ApplicationBar = this.applicationBarForTransfer;
        }

        private void InitializeTitle()
        {
            this.rollOutTitle = this.mainTitle.Insert(2, this.rollOutTitle);
            this.rollInTitle = this.mainTitle.Insert(2, this.rollInTitle);
        }

        private void InitPickerBoxDialog()
        {
            this.accountSelectorDialogBox = new PickerBoxDialog();
            this.accountSelectorDialogBox.Style = this.LayoutRoot.Resources["Custom"] as Style;
            this.accountSelectorDialogBox.ItemSource = ViewModelLocator.AccountViewModel.Accounts;
            this.accountSelectorDialogBox.Closed += new System.EventHandler(this.customDialog_Closed);
        }

        private void MainPivotTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.MainPivotTitle.SelectedIndex;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void SelectRollInAccountButton_Click(object sender, RoutedEventArgs e)
        {
            this.isClosed = false;
            this.accountSelectorDialogBox.Title = this.rollInTitle.ToUpperInvariant();
            this.chooseRequireSender = 1;
            this.accountSelectorDialogBox.Show();
        }

        private void SelectRollOutAccountButton_Click(object sender, RoutedEventArgs e)
        {
            this.isClosed = false;
            this.accountSelectorDialogBox.Title = this.rollOutTitle.ToUpperInvariant();
            this.chooseRequireSender = 0;
            this.accountSelectorDialogBox.Show();
        }

        private void SetTransferingInfo()
        {
            this.MoneyCurrency.Text = this.transferFrom.CurrencyType.GetCurrentString();
            if (this.transferFrom.NeedPoundage)
            {
                this.NeedPoundageGrid.Visibility = Visibility.Visible;
                this.TransferingPoundageRate.Text = this.transferFrom.Poundage.ToMoneyF2() + "% , ";
                if (this.transferFrom.Poundage > 0M)
                {
                    this.transferingPoundageAmount = (this.transferFrom.Poundage * this.AmountTextBox.Text.ToDecimal()) / 100.00M;
                    this.TransferingPoundage.Text = this.transferingPoundageAmount.ToMoneyF2();
                }
            }
            else
            {
                this.NeedPoundageGrid.Visibility = Visibility.Collapsed;
                this.transferingPoundageAmount = 0.0M;
                this.TransferingPoundage.Text = "";
                this.TransferingPoundageRate.Text = "0.0";
            }
            this.calculateAmountChanges();
        }

        private void ShowToasteMessage(string message, string title = null)
        {
            new ToastPrompt { Title = title ?? string.Empty, Message = message }.Show();
        }

        private void ShowTransferingPoundageRateTips_Click(object sender, RoutedEventArgs e)
        {
            this.Alert(AppResources.TransferingPoundageRateTips, null);
        }

        private void TextBoxForcus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.SelectionStart = 0;
            box.SelectionLength = box.Text.Length;
            this.MainPivotTitle.IsLocked = true;
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            this.InvokeInThread(delegate
            {
                this.MainPivotTitle.IsLocked = false;
                this.SetTransferingInfo();
            });
        }

        private void TransferApplicationBar(int state = 0)
        {
        }

        private void TransferButton_Click(object sender, System.EventArgs e)
        {
            decimal amount = this.AmountTextBox.Text.ToDecimal(() => 0.0M);
            if (amount != 0.0M)
            {
                if (this.transferFrom.Category != AccountCategory.CreditCard)
                {
                    if (amount > this.transferFrom.Balance)
                    {
                        this.ShowToasteMessage(this.amountOverAccountBalanceMessage.FormatWith(new object[] { this.transferFrom.BalanceInfo }), null);
                        return;
                    }
                }

                if (IsSelectionMode)
                {

                }

                ViewModelLocator.TransferingHistoryViewModel.Add(this.transferFrom, this.transferTo, amount, (this.transferingPoundageAmount == 0.0M) ? string.Empty : this.TransferingPoundage.Text, this.CreateDate.Value.GetValueOrDefault(), this.Description.Text);
                decimal? balance = this.transferFrom.Balance;
                decimal transferingPoundageAmount = this.transferingPoundageAmount;
                this.transferFrom.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() - transferingPoundageAmount) : null;

                ViewModelLocator.AccountViewModel.Transfer(this.transferFrom, this.transferTo, amount, this.amountToUpdate, true);

                this.ShowToasteMessage(this.operationSuccessfullyMessage, null);

                this.AmountTextBox.Text = "0.0";

                this.Description.Text = string.Empty;
            }
        }
    }
}

