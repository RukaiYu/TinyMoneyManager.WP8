namespace TinyMoneyManager.Controls.BorrowAndLean
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public partial class RepaymentOrReceiptEditor : UserControl, INotifyPropertyChanged
    {
        private decimal? alreadyAmountInOriginalCurrency = null;

        public ApplicationBarHelper aph;
        private Repayment currentObject;

        private bool isDataLoaded;



        public event PropertyChangedEventHandler PropertyChanged;

        public RepaymentOrReceiptEditor(PhoneApplicationPage page)
        {
            this.InitializeComponent();
            this.aph = new ApplicationBarHelper(page);
            this.aph.OriginalBar = page.ApplicationBar;
            this.aph.SelectContentWhenFocus = true;
            this.aph.AddTextBox(new TextBox[] { this.DescriptionTextBox, this.TotalMoneyBox, this.InterestBox });
            this.AccountName.Header = AppResources.BorrowIn + " " + AppResources.AccountName;
            this.AccountName.ItemsSource = ViewModelLocator.AccountViewModel.Accounts;
            base.Loaded += new RoutedEventHandler(this.BorrowAndLeanEditorControl_Loaded);
        }

        private void AccountName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account selectedItem = this.AccountName.SelectedItem as Account;
            if (selectedItem != null)
            {
                this.MoneyCurrency.Text = selectedItem.CurrencyType.GetCurrentString();
                if (this.currentObject != null)
                {
                    this.CalcutingAmountOfAlready(selectedItem, this.currentObject.PayFromAccount, this.TotalMoneyBox.Text.ToDecimal());
                }
            }
        }

        private void BorrowAndLeanEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.isDataLoaded)
            {
                ViewModelLocator.PeopleViewModel.LoadDataIfNot();
                this.InitializeEdit(this.currentObject);
                this.isDataLoaded = true;
            }
        }

        private void CalcutingAmountOfAlready(Account currentOfAccount, Account accountOfOld, decimal currentOfAmount)
        {
            this.BusyForWork(AppResources.CalculatingAmountInfoMessage);
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object callBack)
            {
                System.Func<Repayment, decimal> selector = null;
                Account account1 = currentOfAccount;
                CurrencyType newCurrency = account1.Currency;
                decimal decimal2 = 0.0M;
                if (selector == null)
                {
                    selector = p => p.GetMoneyForRepayOrReceive(new CurrencyType?(newCurrency));
                }
                this.alreadyAmountInOriginalCurrency = new decimal?(this.currentObject.RepayToOrGetBackFrom.RepayToOrGetBackFromItems.ToList<Repayment>().AsEnumerable<Repayment>().Select<Repayment, decimal>(selector).Sum());
                decimal2 = currentOfAmount;
                decimal conversionRateTo = this.currentObject.BorrowLoanCurrency.GetConversionRateTo(newCurrency);
                decimal totalExchanged = conversionRateTo * this.currentObject.RepayToOrGetBackFrom.Amount;
                decimal decimal1 = conversionRateTo * decimal2;
                string symBol = account1.CurrencyTypeSymbol;
                decimal alreadyExchanged = this.alreadyAmountInOriginalCurrency.GetValueOrDefault();
                decimal rangeEnding = totalExchanged - alreadyExchanged;
                this.Dispatcher.BeginInvoke(delegate
                {
                    this.TotalNeedRepayOrReceieveAmount.Text = newCurrency.GetAmountInfoWithCurrencySymbol(symBol, totalExchanged.ToMoneyF2());
                    this.AlreadyRepayOrReceieveAmount.Text = newCurrency.GetAmountInfoWithCurrencySymbol(symBol, alreadyExchanged.ToMoneyF2());
                    this.RangeOfThisTimeAmount.Text = "{0}1.00 ~ {1}".FormatWith(new object[] { symBol, newCurrency.GetAmountInfoWithCurrencySymbol(symBol, rangeEnding.ToMoneyF2()) });
                    this.MaxAmount = totalExchanged;
                    this.WorkDone();
                });
            });
        }

        private void DownAmountButton_Click(object sender, RoutedEventArgs e)
        {
            double result = 0.0;
            double.TryParse(this.TotalMoneyBox.Text, out result);
            result--;
            if (result <= 0.0)
            {
                result = 0.0;
            }
            this.TotalMoneyBox.Text = result.ToString("#0.00");
        }

        internal Repayment GetUpdatedObject()
        {
            decimal num = this.TotalMoneyBox.Text.ToDecimal();
            if ((num < this.MinAmount) || (num > this.MaxAmount))
            {
                this.AlertNotification(AppResources.NotAvaliableObjectMessageFormatter.FormatWith(new object[] { this.amountValueTitle.Text }), null);
                return null;
            }
            this.CurrentObject.PayFromAccount = this.AccountName.SelectedItem as Account;
            if (this.CurrentObject.PayFromAccount == null)
            {
                if (this.AccountName.Items.Count <= 0)
                {
                    this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { this.AccountName.Header }), null);
                    return null;
                }
                this.CurrentObject.PayFromAccount = this.AccountName.Items[0] as Account;
            }
            this.CurrentObject.Notes = this.DescriptionTextBox.Text;
            this.CurrentObject.Amount = num;
            this.CurrentObject.ExecuteDate = this.ExecuteDate.Value;
            this.CurrentObject.Interset = new double?(this.InterestBox.Text.ToDouble());
            this.CurrentObject.Status = RepaymentStatus.OnGoing;
            return this.currentObject;
        }

        private void InitializeEdit(Repayment leanObject)
        {
            this.CurrentObject = leanObject;
            this.PeopleName.Text = this.currentObject.RepayToOrGetBackFrom.ToPeople.Name;
            this.TotalMoneyBox.Text = this.currentObject.Amount.ToMoneyF2();
            this.AccountName.SelectedItem = ViewModelLocator.AccountViewModel.Accounts.FirstOrDefault<Account>(p => p.Id == this.currentObject.FromAccountId);
            this.DescriptionTextBox.Text = this.currentObject.Notes;
            this.ExecuteDate.Value = this.currentObject.ExecuteDate;
            this.InterestBox.Text = this.currentObject.Interset.GetValueOrDefault().ToString("#.##");
            this.SetAmountInfoForRepayOrReceieve();
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            this.MoreInfoPanel.Opacity = 1.0;
            this.MoreInfoPanel.IsHitTestVisible = true;
            this.MoreInfoButton.Visibility = Visibility.Collapsed;
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, e);
            }
        }

        private void SetAmountInfoForRepayOrReceieve()
        {
            string str = string.Empty;
            LeanType valueOrDefault = this.currentObject.RepayToOrGetBackFrom.BorrowOrLean.GetValueOrDefault();
            string str2 = this.currentObject.BorrowLoanTypeName.ToLowerInvariant();
            switch (valueOrDefault)
            {
                case LeanType.BorrowIn:
                    str = AppResources.Repayed.ToLowerInvariant();
                    this.PeopleNameTitle.Text = AppResources.Creditor.ToLowerInvariant();
                    break;

                case LeanType.LoanOut:
                    str = AppResources.Receieved.ToLowerInvariant();
                    this.PeopleNameTitle.Text = AppResources.Debtor.ToLowerInvariant();
                    break;
            }
            this.TotalNeedRepayOrReceieveTitle.Text = AppResources.TotalNeedRepayOrReceieveTitle.FormatWith(new object[] { str2 });
            this.AlreadyRepayOrReceieveTitle.Text = AppResources.AlreadyRepayOrReceieveTitle.FormatWith(new object[] { str });
            this.RangeOfThisTimeTitle.Text = AppResources.RangeOfThisTimeTitle.FormatWith(new object[] { str });
            string str3 = LocalizedStrings.GetCombinedText(str2, AppResources.AccountName.ToLowerInvariant(), false);
            this.AccountName.Header = str3;
            this.AccountName.FullModeHeader = LocalizedStrings.GetCombinedText(AppResources.Choose.ToUpperInvariant(), str3, false);
            this.ExecuteDate.Header = LocalizedStrings.GetCombinedText(str, AppResources.Date, false);
        }

        private void UpAmountButton_Click(object sender, RoutedEventArgs e)
        {
            double result = 0.0;
            double.TryParse(this.TotalMoneyBox.Text, out result);
            if (result <= 0.0)
            {
                result = 0.0;
            }
            this.TotalMoneyBox.Text = (result + 1.0).ToString("#0.00");
        }

        public Repayment CurrentObject
        {
            get
            {
                return this.currentObject;
            }
            set
            {
                if (this.currentObject != value)
                {
                    this.currentObject = value;
                }
            }
        }

        public decimal MaxAmount { get; private set; }

        public decimal MinAmount
        {
            get
            {
                return 1.0M;
            }
        }
    }
}

