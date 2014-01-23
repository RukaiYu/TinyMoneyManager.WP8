using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;

namespace TinyMoneyManager.Pages.BudgetManagement
{
    using TinyMoneyManager.Component;

    using NkjSoft.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using NkjSoft.WPhone.Extensions;
    using TinyMoneyManager.Component.Common;
    using TinyMoneyManager.Language;
    using Microsoft.Phone.Shell;
    using TinyMoneyManager.Pages.DialogBox;
    using System.Windows.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Controls;
    using System.Threading;
    using TinyMoneyManager.Data;
    public partial class BudgetItemAmountEditor : PhoneApplicationPage
    {
        public static Func<BudgetItem> BudgetItemGetter;

        public static Action<object> CheckedPassed;

        public static Action<BudgetItem> DeleteItemCallback;

        public BudgetItem budgetItem;

        public BudgetItemAmountEditor()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(BudgetItemAmountEditor_Loaded);
            this.KeyNameResultBox.KeyUp += new KeyEventHandler(KeyNameResultBox_KeyUp);

            this.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Save;
            this.ApplicationBar.GetIconButtonFrom(1).Text = AppResources.Delete;

        }
        void KeyNameResultBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                SaveButton_Click(sender, e);
            }
        }

        void BudgetItemAmountEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyNameResultBox.Focus();
            this.KeyNameResultBox.SelectAll();

            Thread th = new Thread(() =>
            {
                this.InvokeInThread(() =>
                {
                    var countForLastSettleAmountForCategory =
                        ViewModelLocator.BudgetProjectViewModel.CountForLastSettleAmountForCategory(budgetItem);

                    LatestSuggestionBudgetAmount.Text = AccountItemMoney.GetMoneyInfoWithCurrency(countForLastSettleAmountForCategory);
                });
            });
            th.IsBackground = true;
            th.Start();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (CheckSave())
            {
                this.SafeGoBack();
            }
        }

        private bool CheckSave()
        {
            var money = this.KeyNameResultBox.Text.ToDecimal();

            if (money == 0m)
            {
                this.AlertNotification(AppResources.RequireInputTextDataMessageWithFormatter.FormatWith(AppResources.Amount));
                return false;
            }

            budgetItem.SettleType = (DuringMode)FilterType.SelectedIndex;
            budgetItem.Amount = money;
            OnCheckedPassed(budgetItem);
            return true;
        }

        public void OnCheckedPassed(object result)
        {
            if (CheckedPassed != null)
            {
                CheckedPassed(result);
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.budgetItem = null;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                budgetItem = BudgetItemGetter();
                this.LastScopeInfoTitleBlock.Text = AppResources.BudgetForLastMonthCertainExpenseTitle.FormatWith(budgetItem.AssociatedCategory.CategoryInfo);

                this.FilterType.DataContext = budgetItem;

                this.KeyNameBlock.Text = AppResources.BudgetForCertain.FormatWith(this.GetNavigatingParameter("currentScope"));
                this.FilterType.SelectedIndex = budgetItem.SettleTypeIndex;
                this.KeyNameResultBox.Text = budgetItem.Amount.ToMoneyF2();
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (DeleteItemCallback != null)
                DeleteItemCallback(this.budgetItem);

            this.SafeGoBack();
        }
    }
}