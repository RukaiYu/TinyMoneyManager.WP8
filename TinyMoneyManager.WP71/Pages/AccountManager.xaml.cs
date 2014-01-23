namespace TinyMoneyManager.Pages
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;
    using TinyMoneyManager.ViewModels;
    using NkjSoft.Extensions;
    using TinyMoneyManager.Component.ReorderListBox;

    public partial class AccountManager : PhoneApplicationPage
    {

        private AccountViewModel accountViewModel;

        private ApplicationBar applicationBarForFunction;
        private IApplicationBar applicationBarForSendTransfingHistory;

        private bool hasLoaded;

        private TransferingHistoryViewModel transferingHistoryViewModel;
        private int _toPivotIndex;

        public AccountManager()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.InitializeIconButtons();
            base.Loaded += new RoutedEventHandler(this.AccountManager_Loaded);
            Account.accentColor = App.SystemAccentBrush.Color.ToString();
            Account.foregroundColor = App.SystemForegroundColor.Color.ToString();
            this.AccountsList.ReorderingCompleted += AccountsList_ReorderingCompleted;
        }

        void AccountsList_ReorderingCompleted(object sender, Component.ReorderListBox.ReorderActionCompletedEventArgs e)
        {
            e.HandleReorderingFor<Account>(this.AccountsList.Items,
            () => accountViewModel.Update());
            // this.AlertNotification("moving from: {0} {1} to {2}".FormatWith(e.OriginalIndex, e.Orinentation, e.TargetIndex));
        }

        private void AccountManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasLoaded)
            {
                this.hasLoaded = true;
                this.accountViewModel = ViewModelLocator.AccountViewModel;
                base.DataContext = this.accountViewModel;
                this.transferingHistoryViewModel = ViewModelLocator.TransferingHistoryViewModel;
                this.HistoryPivotItem.DataContext = this.transferingHistoryViewModel;

                this.AccountsManagerPivots.SelectedIndex = _toPivotIndex;
            }

            if (!ViewModelLocator.AccountViewModel.IsDataLoaded)
            {
                ViewModelLocator.AccountViewModel.LoadData();
            }
        }

        private void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account account = null;
            if (((this.AccountsList.SelectedIndex >= 0) && (e.AddedItems.Count > 0)) && ((account = e.AddedItems[0] as Account) != null))
            {
                AccountInfoViewer.CurrentAccountGetter = account;
                this.NavigateTo("/Pages/DialogBox/AccountInfoViewer.xaml?action=view&id={0}", new object[] { account.Id });
                this.AccountsList.SelectedIndex = -1;
            }
        }

        private void AddAccountIconButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/AccountEditorPage.xaml?action=Add");
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.AlertConfirm(this.GetLanguageInfoByKey("DeleteAccountItemMessage"), null, null) == MessageBoxResult.OK)
            {
                MenuItem item = sender as MenuItem;
                Account tag = item.Tag as Account;
                if (this.accountViewModel.EnsureUsed(tag.Id))
                {
                    this.AlertNotification(LocalizedStrings.GetLanguageInfoByKey("CategoryIsBeenUsedMessage"), null);
                }
                else
                {
                    this.accountViewModel.DeleteItem(tag);
                }
            }
        }

        private void DeleteTransferingItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.AlertConfirm(AppResources.DeleteAccountItemMessage, null, null) == MessageBoxResult.OK)
            {
                TransferingItem tag = (sender as MenuItem).Tag as TransferingItem;
                if (tag != null)
                {
                    this.transferingHistoryViewModel.DeleteItem(tag);
                }
            }
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string id = item.Tag.ToString();
            this.GotoEdit(id);
        }

        public void GotoEdit(string id)
        {
            this.NavigateTo("/Pages/AccountEditorPage.xaml?action=Edit&id={0}", new object[] { id });
        }

        private void InitializeIconButtons()
        {
            this.applicationBarForFunction = new ApplicationBar();
            this.applicationBarForFunction.Buttons.Add(ApplicationBar.GetIconButtonFrom(0).SetPropertyValue(new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = this.GetLanguageInfoByKey("AddButtonText");
            } }));
            this.applicationBarForFunction.Buttons.Add(ApplicationBar.GetIconButtonFrom(1).SetPropertyValue(new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = this.GetLanguageInfoByKey("TransferingAccount");
            } }));
            this.initialzeSendApplicationBar();
        }

        private void initialzeSendApplicationBar()
        {
            if (this.applicationBarForSendTransfingHistory == null)
            {
                ApplicationBarIconButton button2 = new ApplicationBarIconButton
                {
                    Text = this.GetLanguageInfoByKey("Send"),
                    IconUri = IconUirs.SendIconButton
                };
                ApplicationBarIconButton button = button2;
                button.Click += new System.EventHandler(this.sendButton_Click);
                ApplicationBar bar = new ApplicationBar
                {
                    Opacity = 0.3,
                    Mode = ApplicationBarMode.Minimized
                };
                this.applicationBarForSendTransfingHistory = bar;
                this.applicationBarForSendTransfingHistory.Buttons.Add(button);
                button.IsEnabled = AppSetting.Instance.HasEmail;
            }
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            this.transferingHistoryViewModel.GoStepDate(1);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (BudgetManager.Current.NeedUpdate)
            {
                ViewModelLocator.BudgetProjectViewModel.UpdateCurrentMonthBudgetSummary();
            }

            if (e.NavigationMode != NavigationMode.Back)
            {
                this._toPivotIndex = e.GetNavigatingParameter("toIndex", 0)
                    .ToInt32();
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AccountsManagerPivots != null)
            {
                if (this.AccountsManagerPivots.SelectedIndex == 0)
                {
                    base.ApplicationBar = this.applicationBarForFunction;
                }
                else
                {
                    base.ApplicationBar = this.applicationBarForSendTransfingHistory;
                    // if (!this.transferingHistoryViewModel.IsDataLoaded)
                    {
                        this.transferingHistoryViewModel.PerformLoadData();
                    }
                }
            }
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            this.transferingHistoryViewModel.GoStepDate(-1);
        }

        private void Revert_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            TransferingItem tag = (sender as MenuItem).Tag as TransferingItem;
            if ((tag != null) && (((this.AlertConfirm(AppResources.RevertWillDeleteItemConfirmMessage, null, null) == MessageBoxResult.OK) && !this.transferingHistoryViewModel.Revert(tag)) && (this.AlertConfirm(LocalizedStrings.GetLanguageInfoByKey("CantRevertWhenAccountsNoExistsConfirmationMessage"), null, null) == MessageBoxResult.OK)))
            {
                try
                {
                    this.transferingHistoryViewModel.DeleteItem(tag);
                }
                catch (Exception ex)
                {
                    this.AlertNotification(ex.Message);
                }
            }
        }

        private void sendButton_Click(object sender, System.EventArgs e)
        {
            Helper.SendEmail(this.transferingHistoryViewModel.GetSubjectOfHistory(this.HistoryPivotItem.Header.ToString()), this.transferingHistoryViewModel.BuildHistoryListToString().ToString());
        }

        private void SetAsDefaultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Account tag = (sender as MenuItem).Tag as Account;
            if (!tag.IsDefaultAccount)
            {
                this.accountViewModel.SetMostOftenAccount(tag);
            }
        }

        private void TransferAccountIconButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/AccountTransferingPage.xaml");
        }

        private void TransferingHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ViewTransferingHistory_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            this.NavigateTo("/Pages/AccountTransferingPage.xaml?action={0}&fromAccount={1}", new object[] { AccountTransferingPage.ViewHistoryActionName, item.Tag });
        }
    }
}

