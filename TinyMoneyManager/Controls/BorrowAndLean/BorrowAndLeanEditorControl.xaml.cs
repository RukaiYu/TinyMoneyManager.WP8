namespace TinyMoneyManager.Controls.BorrowAndLean
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public partial class BorrowAndLeanEditorControl : UserControl, INotifyPropertyChanged
    {
        public ApplicationBarHelper aph;

        private Repayment currentObject;

        private bool isDataLoaded;


        public event PropertyChangedEventHandler PropertyChanged;

        public BorrowAndLeanEditorControl(PhoneApplicationPage page)
        {
            this.InitializeComponent();
            this.aph = new ApplicationBarHelper(page);
            this.aph.SelectContentWhenFocus = true;
            this.aph.AddTextBox(new TextBox[] { this.DescriptionTextBox, this.TotalMoneyBox, this.InterestBox });
            this.AccountName.Header = AppResources.BorrowIn + " " + AppResources.AccountName;
            this.BorrowLeanDebtor.ItemsSource = ViewModelLocator.PeopleViewModel.PeopleList;
            this.AccountName.ItemsSource = ViewModelLocator.AccountViewModel.Accounts;
            base.Loaded += new RoutedEventHandler(this.BorrowAndLeanEditorControl_Loaded);
        }

        private void AccountName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account selectedItem = this.AccountName.SelectedItem as Account;
            if (selectedItem != null)
            {
                this.MoneyCurrency.Text = selectedItem.CurrencyType.GetCurrentString();
            }
        }

        private void BorrowAndLeanEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.isDataLoaded)
            {
                ViewModelLocator.PeopleViewModel.LoadDataIfNot();
                if (this.currentObject.Id != System.Guid.Empty)
                {
                    this.InitializeEdit(this.currentObject);
                }
                this.isDataLoaded = true;
            }
        }

        private void BorrowLeanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((this.BorrowLeanType == null) || (this.AccountName == null))
            {
                return;
            }
            ListPickerItem selectedItem = this.BorrowLeanType.SelectedItem as ListPickerItem;
            if (selectedItem == null)
            {
                return;
            }
            string str = selectedItem.Content.ToString().ToLowerInvariant();
            this.AccountName.Header = LocalizedStrings.GetCombinedText(str, AppResources.AccountName, false);
            this.AccountName.FullModeHeader = AppResources.Choose.ToUpperInvariant() + this.AccountName.Header;
            if ((this.BorrowLeanType == null) || (this.BorrowLeanDebtor == null))
            {
                return;
            }
            switch (this.BorrowLeanType.SelectedIndex)
            {
                case 0:
                case 2:
                    this.BorrowLeanDebtor.Header = AppResources.Creditor;
                    goto Label_00C1;

                case 1:
                case 3:
                    this.BorrowLeanDebtor.Header = AppResources.Debtor;
                    break;
            }
        Label_00C1:
            this.ExecuteDate.Header = LocalizedStrings.GetCombinedText(str, AppResources.Date, false);
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
            this.CurrentObject.ToPeople = this.BorrowLeanDebtor.SelectedItem as PeopleProfile;
            if (this.CurrentObject.ToPeople == null)
            {
                if (this.BorrowLeanDebtor.Items.Count <= 0)
                {
                    this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { this.BorrowLeanDebtor.Header }), null);
                    return null;
                }
                this.CurrentObject.ToPeople = this.BorrowLeanDebtor.Items[0] as PeopleProfile;
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
            this.CurrentObject.Amount = this.TotalMoneyBox.Text.ToDecimal();
            this.CurrentObject.ExecuteDate = this.ExecuteDate.Value;
            this.CurrentObject.BorrowOrLean = new LeanType?((LeanType)System.Enum.Parse(typeof(LeanType), (string)(this.BorrowLeanType.SelectedItem as ListPickerItem).Tag, true));
            this.CurrentObject.Interset = new double?(this.InterestBox.Text.ToDouble());
            this.CurrentObject.Status = RepaymentStatus.OnGoing;
            return this.currentObject;
        }

        public void InitializeAdd(Repayment leanObject)
        {
            this.CurrentObject = leanObject;
            if (this.AccountName != null)
            {
                this.AccountName.SelectedIndex = 0;
            }
            if (this.BorrowLeanDebtor != null)
            {
                this.BorrowLeanDebtor.SelectedIndex = 0;
            }
        }

        public void InitializeEdit(Repayment leanObject)
        {
            this.CurrentObject = ViewModelLocator.BorrowLeanViewModel.AccountBookDataContext.Repayments.FirstOrDefault<Repayment>(p => p.Id == leanObject.Id);
            int num = (int)this.currentObject.BorrowOrLean.Value;
            if (num == 4)
            {
                num = 2;
            }
            this.BorrowLeanType.SelectedIndex = num;
            this.BorrowLeanType.IsEnabled = false;
            this.BorrowLeanDebtor.SelectedItem = ViewModelLocator.PeopleViewModel.PeopleList.FirstOrDefault<PeopleProfile>(p => p.Id == this.currentObject.ToPeopleId);
            this.AccountName.SelectedItem = ViewModelLocator.AccountViewModel.Accounts.FirstOrDefault<Account>(p => p.Id == this.currentObject.FromAccountId);
            this.TotalMoneyBox.Text = this.currentObject.Amount.ToMoneyF2();
            this.DescriptionTextBox.Text = this.currentObject.Notes;
            this.ExecuteDate.Value = this.currentObject.ExecuteDate;
            this.InterestBox.Text = this.currentObject.Interset.GetValueOrDefault().ToString("#.##");
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

        private void TotalMoneyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
    }
}

