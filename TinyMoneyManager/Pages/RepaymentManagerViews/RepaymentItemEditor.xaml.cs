namespace TinyMoneyManager.Pages.RepaymentManagerViews
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.ViewModels.RepaymentManager;

    public partial class RepaymentItemEditor : PhoneApplicationPage
    {
        private AddOrEditRepaymentViewModel addOrEditRepaymentViewModel;

        private ApplicationBarHelper applicationBarHelper;

        private bool hasLoaded;
        private bool hasNavigated;

        public RepaymentItemEditor()
        {
            this.InitializeComponent();
            this.applicationBarHelper = new ApplicationBarHelper(this);
            this.applicationBarHelper.SelectContentWhenFocus = true;
            this.applicationBarHelper.AddTextBox(true, new TextBox[] { this.AmountTextBox, this.Place, this.Description });
            if (ViewModelLocator.AccountViewModel.Accounts.Count == 0)
            {
                ViewModelLocator.AccountViewModel.QuickLoadData();
            }
            this.addOrEditRepaymentViewModel = new AddOrEditRepaymentViewModel();
            base.DataContext = this.addOrEditRepaymentViewModel;
            base.Loaded += new RoutedEventHandler(this.RepaymentItemEditor_Loaded);
            this.InitializeApplicationBarText();
        }

        private void AccountName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account selectedItem = this.AccountName.SelectedItem as Account;
            if (selectedItem != null)
            {
                this.addOrEditRepaymentViewModel.Current.PayToAccount = selectedItem;
                this.AmountTextBox_TextChanged(sender, null);
            }
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((this.MoneyCurrency != null) && (this.AmountTextBox != null)) && (this.AmountWillAddedToRepayToAccount != null))
            {
                Account payFromAccount = this.addOrEditRepaymentViewModel.Current.PayFromAccount;
                Account payToAccount = this.addOrEditRepaymentViewModel.Current.PayToAccount;
                if (payFromAccount != null)
                {
                    bool flag = payFromAccount.Currency != payToAccount.Currency;
                    decimal num = this.AmountTextBox.Text.ToDecimal() * (flag ? payFromAccount.Currency.GetConversionRateTo(payToAccount.Currency) : 1.0M);
                    if (num == 0.0M)
                    {
                        this.InfoWhenRepaymentCompletedGrid.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (this.addOrEditRepaymentViewModel.Current.Status == TinyMoneyManager.Component.RepaymentStatus.OnGoing)
                        {
                            this.InfoWhenRepaymentCompletedGrid.Visibility = Visibility.Visible;
                        }
                        this.AmountWillAddedToRepayToAccount.Text = this.MoneyCurrency.Text + (num + this.addOrEditRepaymentViewModel.Current.PayToAccount.Balance.GetValueOrDefault()).ToMoneyF2();
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            base.NavigationService.GoBack();
        }

        private void EnableAlarmNotification_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void EnableAlarmNotification_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void FromAccountName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account selectedItem = this.FromAccountName.SelectedItem as Account;
            if (selectedItem != null)
            {
                ApplicationBarIconButton iconButtonFrom = base.ApplicationBar.GetIconButtonFrom(0);
                if (selectedItem.Id == this.addOrEditRepaymentViewModel.Current.PayToAccount.Id)
                {
                    iconButtonFrom.IsEnabled = false;
                }
                else
                {
                    this.addOrEditRepaymentViewModel.Current.PayFromAccount = selectedItem;
                    this.MoneyCurrency.Text = selectedItem.CurrencyType.GetCurrentString();
                    this.AmountTextBox_TextChanged(sender, null);
                    if (iconButtonFrom != null)
                    {
                        iconButtonFrom.IsEnabled = true;
                    }
                }
            }
        }

        private void InitializeApplicationBarText()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = this.GetLanguageInfoByKey("Save");
            base.ApplicationBar.GetIconButtonFrom(1).Text = this.GetLanguageInfoByKey("Cancel");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                string id = this.GetNavigatingParameter("itemId", null);
                this.addOrEditRepaymentViewModel.InitializeAction(id);
                this.addOrEditRepaymentViewModel.InitializeCurrent(id);
                if (this.addOrEditRepaymentViewModel.Action == PageActionType.Add)
                {
                    this.RepaymentStatus.IsEnabled = false;
                }
                else
                {
                    bool canChangeStatus = this.addOrEditRepaymentViewModel.Current.CanChangeStatus;
                    base.ApplicationBar.GetIconButtonFrom(0).IsEnabled = canChangeStatus;
                }
                this.MoneyCurrency.Text = (this.addOrEditRepaymentViewModel.Current.PayToAccount == null) ? AppSetting.Instance.DefaultCurrency.GetCurrentString() : this.addOrEditRepaymentViewModel.Current.PayToAccount.CurrencyType.GetCurrentString();
                this.AmountTextBox.Text = this.addOrEditRepaymentViewModel.Current.AmountString;
                this.RepaymentStatus.SelectedItem = RepaymentStatusHelper.Instance.RepaymentStatusList.FirstOrDefault<RepaymentStatusWapper>(p => p.Status == this.addOrEditRepaymentViewModel.Current.Status);
                this.hasNavigated = true;
            }
        }

        private void RepayAtDate_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if ((sender != null) && (this.RepayAtDateTime != null))
            {
                System.DateTime time = this.RepayAtDateTime.Value.Value;
                this.addOrEditRepaymentViewModel.Current.RepayAt = this.RepayAtDate.Value.GetValueOrDefault().Date.AddHours((double)time.Hour).AddMinutes((double)time.Minute).AddSeconds((double)time.Second);
            }
        }

        private void RepayDueDateTime_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if ((sender != null) && (this.RepayDueDateTime != null))
            {
                System.DateTime time = this.RepayDueDateTime.Value.Value;
                this.addOrEditRepaymentViewModel.Current.DueDate = this.RepayDueDate.Value.GetValueOrDefault().Date.AddHours((double)time.Hour).AddMinutes((double)time.Minute).AddSeconds((double)time.Second);
            }
        }

        private void RepaymentItemEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasLoaded)
            {
                this.RepaymentStatus.SelectionChanged += new SelectionChangedEventHandler(this.RepaymentStatus_SelectionChanged);
                if (this.addOrEditRepaymentViewModel.Current.Status != TinyMoneyManager.Component.RepaymentStatus.OnGoing)
                {
                    this.InfoWhenRepaymentCompletedGrid.Visibility = Visibility.Collapsed;
                }
                this.hasLoaded = true;
                if (this.addOrEditRepaymentViewModel.Current.Status == TinyMoneyManager.Component.RepaymentStatus.Completed)
                {
                    this.SettingUIByStatus();
                }
                this.RepayDueDateTime.ValueChanged += new System.EventHandler<DateTimeValueChangedEventArgs>(this.RepayDueDateTime_ValueChanged);
                this.RepayDueDate.ValueChanged += new System.EventHandler<DateTimeValueChangedEventArgs>(this.RepayDueDateTime_ValueChanged);
                this.RepayAtDate.ValueChanged += new System.EventHandler<DateTimeValueChangedEventArgs>(this.RepayAtDate_ValueChanged);
                this.RepayAtDateTime.ValueChanged += new System.EventHandler<DateTimeValueChangedEventArgs>(this.RepayAtDate_ValueChanged);
                this.FromAccountName.SelectionChanged += new SelectionChangedEventHandler(this.FromAccountName_SelectionChanged);
                this.AccountName.SelectionChanged += new SelectionChangedEventHandler(this.AccountName_SelectionChanged);
            }
        }

        private void RepaymentStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.RepaymentStatus != null)
            {
                Visibility visibility = (this.RepaymentStatus.SelectedIndex == 0) ? Visibility.Visible : Visibility.Collapsed;
                this.InfoWhenRepaymentCompletedGrid.Visibility = visibility;
                this.AlarmOrReminderSettingPanel.Visibility = visibility;
            }
        }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            this.addOrEditRepaymentViewModel.Current.PayToAccount = this.AccountName.SelectedItem as Account;
            this.addOrEditRepaymentViewModel.Current.PayFromAccount = this.FromAccountName.SelectedItem as Account;
            this.addOrEditRepaymentViewModel.Current.Amount = this.AmountTextBox.Text.ToDecimal();
            this.addOrEditRepaymentViewModel.Current.Status = (this.RepaymentStatus.SelectedItem as RepaymentStatusWapper).Status;
            if (this.ValidPassed())
            {
                this.addOrEditRepaymentViewModel.Submit(this.EnableAlarmNotification.IsChecked.GetValueOrDefault());
                this.CancelButton_Click(sender, e);
            }
        }

        private void SettingUIByStatus()
        {
            this.ManagementPageTitle.Text = this.GetLanguateInfoByKeys("{0}{1}", new string[] { "RepaymentItem", "Details" }).TrimStart(new char[0]);
            Thickness thickness = new Thickness(0.0);
            this.RepayAtDate.BorderThickness = thickness;
            this.AccountName.BorderThickness = thickness;
            this.Description.BorderThickness = thickness;
            this.AmountTextBox.BorderThickness = thickness;
            this.RepaymentStatus.BorderThickness = thickness;
            this.RepayDueDateTime.BorderThickness = thickness;
            this.FromAccountName.BorderThickness = thickness;
            this.EnableAlarmNotification.BorderThickness = thickness;
            this.Place.BorderThickness = thickness;
            this.AlarmOrReminderSettingPanel.Visibility = Visibility.Collapsed;
            this.RepayDueDateGrid.Visibility = Visibility.Collapsed;
            this.RepayAtDate.Header = LocalizedStrings.GetLanguageInfoByKey("CompletedAt");
            this.RepayAtDate.Value = this.addOrEditRepaymentViewModel.Current.CompletedAt;
            this.RepayAtDateTime.Value = this.addOrEditRepaymentViewModel.Current.CompletedAt;
            base.ApplicationBar.GetIconButtonFrom(0).IsEnabled = false;
            base.ApplicationBar.GetIconButtonFrom(1).Text = LocalizedStrings.GetLanguageInfoByKey("Close");
        }

        private bool ValidPassed()
        {
            if (this.addOrEditRepaymentViewModel.Current.Amount <= 0.0M)
            {
                this.AlertNotification(this.GetLanguageInfoByKey("AmountShouldBeGreatThanZeroMessage"), null);
                return false;
            }
            if (this.addOrEditRepaymentViewModel.Current.PayToAccount == null)
            {
                this.AlertNotification(this.GetLanguageInfoByKey("RepayToAccountCannotBeEmptyMessage"), null);
                return false;
            }
            if ((this.addOrEditRepaymentViewModel.Current.RepayAt < System.DateTime.Now) && (this.addOrEditRepaymentViewModel.Current.Status != TinyMoneyManager.Component.RepaymentStatus.Completed))
            {
                this.AlertNotification(this.GetLanguageInfoByKey("RepayDateShouldBeGreatterThanNowMessage"), null);
                return false;
            }
            System.DateTime? completedAt = this.addOrEditRepaymentViewModel.Current.CompletedAt;
            System.DateTime repayAt = this.addOrEditRepaymentViewModel.Current.RepayAt;
            if (completedAt.HasValue ? (completedAt.GetValueOrDefault() < repayAt) : false)
            {
                this.AlertNotification(this.GetLanguageInfoByKey("DueDateTimeMustBeGreatThanStartDateTimeMessage"), null);
                return false;
            }
            return true;
        }
    }
}

