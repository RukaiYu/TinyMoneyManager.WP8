using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using NkjSoft.Extensions;
using TinyMoneyManager.Component;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using TinyMoneyManager.Pages;
using TinyMoneyManager.Pages.CategoryManager;
using TinyMoneyManager.Pages.DialogBox;
using TinyMoneyManager.Pages.DialogBox.PictureManager;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.ViewModels.AccountItemManager;

namespace TinyMoneyManager.Pages.AccountItemViews
{
    using TinyMoneyManager.Component.Common;
    using NkjSoft.WPhone.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Data.Model;
    public partial class InstallmentsItemEditor : PhoneApplicationPage
    {
        private ApplicationBarHelper aph;

        public static AccountItem currentObject;
        private bool hasLoadData;

        public AccountItem Current { get { return currentObject; } }

        public InstallmentsItemEditor()
        {
            InitializeComponent();

            aph = new ApplicationBarHelper(this);

            aph.OriginalBar = this.ApplicationBar;

            aph.SelectContentWhenFocus = true;
            TiltEffect.SetIsTiltEnabled(this, true);

            LoadContent();

            Loaded += InstallmentsItemEditor_Loaded;
        }

        void InstallmentsItemEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasLoadData)
            {
                this.hasLoadData = true;
                this.DataContext = this;

                this.AllTeams.SelectedIndex = 5;


                if (this.Current != null)
                {
                    try
                    {
                        if (Current.PaymentAccount != null)
                        {
                            this.RepaymentAccountSelector.SelectedItem = Current.PaymentAccount;
                        }

                        if (Current.Account != null)
                        {
                            this.CreditCardAccountSelector.SelectedItem = Current.Account;
                        }

                        if (this.Current.Account == null && this.RepaymentAccountSelector.Items.Count > 0)
                        {
                            this.Current.Account = this.RepaymentAccountSelector.Items[0] as Account;
                        }

                        this.Current.PropertyChanged += Current_PropertyChanged;

                        this.AllTeams.SelectedItem = (this.AllTeams.ItemsSource as Dictionary<int, string>)
                            .FirstOrDefault(p => p.Key == Current.Term.GetValueOrDefault());
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }


                this.AllTeams.SelectionChanged += AllTeams_SelectionChanged_1;
            }

        }

        void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == AccountItem.TotalPoundageProperty)
            {
                Current.CalculateAmount();
            }
        }

        private void LoadContent()
        {
            this.RepaymentAccountSelector.Header = LocalizedStrings.GetCombinedText(
                AppResources.Repayment, AppResources.AccountName).ToLower();

            this.RepaymentAccountSelector.FullModeHeader = LocalizedStrings.GetCombinedText(
                AppResources.Choose, this.RepaymentAccountSelector.Header.ToString())
                .ToUpper();

            this.CreditCardAccountSelector.FullModeHeader = LocalizedStrings.GetCombinedText(
                AppResources.Choose, this.CreditCardAccountSelector.Header.ToString())
                .ToUpper();

            this.CreditCardAccountSelector.ItemsSource = ViewModelLocator.AccountViewModel.Accounts.Where(p => p.IsCreditCard);

            this.RepaymentAccountSelector.ItemsSource = ViewModelLocator.AccountViewModel.Accounts.Where(p => !p.IsCreditCard);


            var terms = new Dictionary<int, string>();

            terms.Add(24, AppResources.TermFormatter.FormatWith(24));
            terms.Add(18, AppResources.TermFormatter.FormatWith(18));
            terms.Add(12, AppResources.TermFormatter.FormatWith(12));
            terms.Add(9, AppResources.TermFormatter.FormatWith(9));
            terms.Add(6, AppResources.TermFormatter.FormatWith(6));
            terms.Add(3, AppResources.TermFormatter.FormatWith(3));

            this.AllTeams.ItemsSource = terms;

        }

        private void CancelItemButton_Click(object sender, EventArgs e)
        {
            this.SafeGoBack();
        }

        private void OkMenuItem_Click(object sender, EventArgs e)
        {
            //Current.CalculateAmount();
            this.SafeGoBack();
        }

        /// <summary>
        /// Goes the specified from page.
        /// </summary>
        /// <param name="fromPage">From page.</param>
        public static void Go(PhoneApplicationPage fromPage, AccountItem accountItem)
        {
            currentObject = accountItem;
            fromPage.NavigateTo("/Pages/AccountItemViews/InstallmentsItemEditor.xaml");
        }

        private void AmountValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Amount.ToUpper(), this.Current.MoneyInfoWithoutTag, (t) =>
            {
                t.SelectAll();
                t.InputScope = MoneyInputTextBox.NumberInputScope;
            }, s => s.ToDecimal(delegate
            {
                this.AlertNotification(AppResources.AmountShouldBeGreatThanZeroMessage, null);
                return -9999999999.01M;
            }) > -9999999999.01M, delegate(string s)
            {
                decimal num2 = s.ToDecimal();
                this.Current.TotalCost = num2;

                Current.CalculateAmount();

                // ViewModelLocator.AccountViewModel.HandleAccountItemEditing(this.Current, this.Current.Account, oldMoney, oldBalance);
            });
        }

        /// <summary>
        /// Handles the 1 event of the AllTeams_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void AllTeams_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var itemValue = (KeyValuePair<int, string>)e.AddedItems[0];
                this.Current.Term = itemValue.Key;
            }
        }

        private void TotalPoundageValueEditor_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Poundage.ToUpper(),
                this.Current.TotalPoundageInfo, (t) =>
            {
                t.SelectAll();
                t.InputScope = MoneyInputTextBox.NumberInputScope;
            }, s => s.ToDecimal(() =>
            {
                this.AlertNotification(AppResources.AmountShouldBeGreatThanZeroMessage, null);
                return -9999999999.01M;
            }) >= 0, (s) =>
            {
                decimal num2 = s.ToDecimal();
                this.Current.TotalPoundage = num2;
                Current.CalculateAmount();
            });
        }
    }
}