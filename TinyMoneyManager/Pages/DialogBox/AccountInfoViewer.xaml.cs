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
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.Language;

using NkjSoft.WPhone.Extensions;
using NkjSoft.Extensions;
using TinyMoneyManager.Component;
using Microsoft.Phone.Shell;
using TinyMoneyManager.Controls;
using JasonPopupDemo;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace TinyMoneyManager.Pages.DialogBox
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AccountInfoViewer : PhoneApplicationPage
    {
        private ApplicationBar applicationBarForEditOrDelete;
        private ApplicationBar applicationBarForRefineData;
        private AssociatedItemsSelectorInAccountViewer associatedItemsSelector;
        public static Account CurrentAccountGetter;
        private ApplicationBarIconButton deleteIconButton;
        private ApplicationBarIconButton editIconButton;

        private ApplicationBarIconButton refineDataIconButton;

        public Account Current
        {
            get
            {
                return CurrentAccountGetter;
            }
        }

        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> RelatedItems { get; set; }

        private AccountViewModel vm;

        public AccountInfoViewer()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.InitializeApplicationBars();
            base.DataContext = this;
            this.vm = ViewModelLocator.AccountViewModel;
            this.RelatedItems = new ObservableCollection<GroupByCreateTimeAccountItemViewModel>();
            if (this.vm.SearchingConditionAssociatedItemsForCurrentViewAccount == null)
            {
                this.vm.SearchingConditionAssociatedItemsForCurrentViewAccount = new AssociatedItemsSelectorOption();
            }
        }

        private void AccountCategoryEditorButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void associatedItemsSelector_Confirmed(object sender, System.EventArgs e)
        {
            this.vm.HasLoadAssociatedItemsForCurrentViewAccount = false;
            this.LoadAssociatedItems();
        }

        private void deleteIconButton_Click(object sender, System.EventArgs e)
        {
            if (this.AlertConfirm(AppResources.DeleteAccountItemMessage, null, null) == MessageBoxResult.OK)
            {
                Account current = this.Current;
                if (this.vm.EnsureUsed(current.Id))
                {
                    this.AlertNotification(AppResources.CategoryIsBeenUsedMessage, null);
                }
                else
                {
                    this.vm.DeleteItem(current);
                    base.NavigationService.GoBack();
                }
            }
        }

        private void editIconButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/AccountEditorPage.xaml?action=edit&id={0}&from=view", new object[] { this.Current.Id });
        }

        private void InitializeApplicationBars()
        {
            this.applicationBarForEditOrDelete = new ApplicationBar();
            this.applicationBarForRefineData = new ApplicationBar();
            this.editIconButton = new ApplicationBarIconButton(IconUirs.EditIcon);
            this.editIconButton.Text = AppResources.Edit;
            this.deleteIconButton = new ApplicationBarIconButton(IconUirs.DeleteIcon);
            this.deleteIconButton.Text = AppResources.Delete;
            this.refineDataIconButton = new ApplicationBarIconButton(IconUirs.SearchRefineIconButton);
            this.refineDataIconButton.Text = AppResources.Refine;
            this.editIconButton.Click += new System.EventHandler(this.editIconButton_Click);
            this.deleteIconButton.Click += new System.EventHandler(this.deleteIconButton_Click);
            this.applicationBarForEditOrDelete.Buttons.Add(this.editIconButton);
            this.applicationBarForEditOrDelete.Buttons.Add(this.deleteIconButton);
            this.refineDataIconButton.Click += new System.EventHandler(this.refineDataIconButton_Click);
            this.applicationBarForRefineData.Buttons.Add(this.refineDataIconButton);
            this.deleteIconButton.IsEnabled = this.Current.CanBeDeleted;
        }


        private void LoadAssociatedItems()
        {
            System.Threading.WaitCallback callBack = null;
            if (!this.vm.HasLoadAssociatedItemsForCurrentViewAccount)
            {
                this.BusyForWork(AppResources.Loading);
                this.RelatedItems.Clear();
                if (callBack == null)
                {
                    callBack = delegate(object o)
                    {
                        this.vm.LoadAssociatedItemsForAccount(this.Current, this.RelatedItems);
                        this.InvokeInThread(delegate
                        {
                            this.WorkDone();
                        });
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        private void MoneyCurrencyPanel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.CurrentBalance, this.Current.Balance.Value.ToMoneyF2(), delegate(TextBox tb)
            {
                tb.InputScope = MoneyInputTextBox.NumberInputScope;
                tb.SelectAll();
            }, null, delegate(string result)
            {
                this.Current.InitialDateTime = new System.DateTime?(System.DateTime.Now);
                this.Current.Balance = new decimal?(result.ToDecimal());
                this.vm.UpdateOnSubmit(this.Current);
            });
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            EditValueInTextBoxEditor.ResultCallBack = delegate(string s)
            {
                CurrentAccountGetter.Name = s;
                this.vm.SubmitChanges();
            };
            this.NavigateTo(ViewPath.EditValueInTextBoxEditor, new object[] { AppResources.Name, CurrentAccountGetter.Name });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.MainPivot.SelectedIndex = this.GetNavigatingParameter("to", null).ToInt32();
                this.IncomeOrExpenseDetailsPivot.Header = AppResources.AccountInfo.ToLowerInvariant();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.vm.HasLoadAssociatedItemsForCurrentViewAccount = false;
            base.OnNavigatingFrom(e);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MainPivot.SelectedIndex == 1)
            {
                base.ApplicationBar = this.applicationBarForRefineData;
                this.LoadAssociatedItems();
            }
            else
            {
                base.ApplicationBar = this.applicationBarForEditOrDelete;
            }
        }

        private void refineDataIconButton_Click(object sender, System.EventArgs e)
        {
            if (this.associatedItemsSelector == null)
            {
                this.associatedItemsSelector = new AssociatedItemsSelectorInAccountViewer(this.vm.SearchingConditionAssociatedItemsForCurrentViewAccount);
                this.associatedItemsSelector.Confirmed += new System.EventHandler<EventArgs>(this.associatedItemsSelector_Confirmed);
            }
            new PopupCotainer(this).Show(this.associatedItemsSelector);
        }

        private void RelatedItemsListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountItem selectedItem = (sender as LongListSelector).SelectedItem as AccountItem;
            if (selectedItem != null)
            {
                NavigationDataInfoFromAccountItem dataProvider = new NavigationDataInfoFromAccountItem
                {
                    Key = selectedItem.Id,
                    PageName = selectedItem.PageNameGetter
                };
                TinyMoneyManagerNavigationService.Instance.Navigate(this, dataProvider);
                this.RelatedItemsListControl.SelectedItem = null;
            }
        }

        private void PaymentDueDatePanel_Click(object sender, RoutedEventArgs e)
        {
            DaySelectorPage.AfterConfirmed = (v) =>
            {
                this.Current.PaymentDueDay = v;
                this.vm.UpdateOnSubmit(this.Current);
            };

            this.NavigateTo("/Pages/DialogBox/DaySelectorPage.xaml?title={0}&defValue={1}", new object[] { AppResources.SelectDayOfMonth, 
                this.Current.PaymentDueDay.GetValueOrDefault() });

        }

        private void LineOfCreditPanel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.LineOfCredit, this.Current.LineOfCredit.GetValueOrDefault().ToMoneyF2(), (tb) =>
            {
                tb.InputScope = MoneyInputTextBox.NumberInputScope;
                tb.SelectAll();
            }, (text) =>
            {
                var amountValue = text.ToDecimal();

                var isValid = amountValue >= 0;

                return isValid;

            }, (result) =>
            {
                this.Current.LineOfCredit = result.ToDecimal();
                this.vm.UpdateOnSubmit(this.Current);
            });
        }

    }
}