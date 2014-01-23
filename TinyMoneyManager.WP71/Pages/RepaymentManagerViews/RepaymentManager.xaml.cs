using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TinyMoneyManager.Component;
using TinyMoneyManager.Component.Converter;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.ViewModels.BudgetManagement;
using TinyMoneyManager.ViewModels.AppSettingManager;
using TinyMoneyManager.Controls;

namespace TinyMoneyManager.Pages.RepaymentManagerViews
{
    public partial class RepaymentManager : PhoneApplicationPage
    {
        private BudgetProjectManagementViewModel budgetManagerViewModel;

        private RepaymentManagerViewModel repaymentManagerVierModel;
        private bool _hasDataLoaded;

        public RepaymentManager()
        {
            this.InitializeComponent();
            this.repaymentManagerVierModel = ViewModelLocator.RepaymentManagerViewModel;
            this.budgetManagerViewModel = ViewModelLocator.BudgetProjectViewModel;
            base.DataContext = this.repaymentManagerVierModel;
            TiltEffect.SetIsTiltEnabled(this, true);
            base.Loaded += new RoutedEventHandler(this.RepaymentManager_Loaded);
        }

        private void AddItemIconButton_Click(object sender, System.EventArgs e)
        {
            if (ViewModelLocator.AccountViewModel.GetBankOrCreditAccountsCount() == 0)
            {
                this.AlertNotification(this.GetLanguageInfoByKey("NoneBankOrCreditAccountsMessage"), null);
            }
            else
            {
                this.NavigateTo("/Pages/RepaymentManagerViews/RepaymentItemEditor.xaml?action={0}&itemId={1}", new object[] { PageActionType.Add, string.Empty });
            }
        }

        private void CancelItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = ((MenuItem)sender).Tag as Repayment;
            if (tag != null)
            {
                this.repaymentManagerVierModel.CancelRepayment(tag);
            }
        }

        private void CompleteItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = ((MenuItem)sender).Tag as Repayment;
            if (tag != null)
            {
                this.repaymentManagerVierModel.CompleteRepayment(tag);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            Repayment tag = ((MenuItem)sender).Tag as Repayment;
            if ((tag != null) && ((tag.Status != RepaymentStatus.OnGoing) || (this.AlertConfirm(this.GetLanguageInfoByKey("DeleteOnGoingRapaymentMessage"), null, null) == MessageBoxResult.OK)))
            {
                this.repaymentManagerVierModel.DeleteRepayment(tag);
            }
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            System.Guid id = ((MenuItem)sender).Tag.ToString().ToGuid();
            if (id != System.Guid.Empty)
            {
                this.GoToEdit(id);
            }
        }

        private void GoToEdit(System.Guid id)
        {
            this.NavigateTo("/Pages/RepaymentManagerViews/RepaymentItemEditor.xaml?action={0}&itemId={1}", new object[] { PageActionType.Edit, id });
        }

        private void RepaymentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Repayment selectedItem = this.RepaymentList.SelectedItem as Repayment;
            if (selectedItem != null)
            {
                this.GoToEdit(selectedItem.Id);
                this.RepaymentList.SelectedItem = null;
            }
        }

        private void RepaymentManager_Loaded(object sender, RoutedEventArgs e)
        {
            this.repaymentManagerVierModel.LoadDataIfNot();
        }
    }
}