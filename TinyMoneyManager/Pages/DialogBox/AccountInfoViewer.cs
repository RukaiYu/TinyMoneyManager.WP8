namespace TinyMoneyManager.Pages.DialogBox
{
    using JasonPopupDemo;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public class AccountInfoViewer : PhoneApplicationPage
    {
        private bool _contentLoaded;
        internal TextBlock AccountCategoryBlock;
        internal HyperlinkButton AccountCategoryEditor;
        private ApplicationBar applicationBarForEditOrDelete;
        private ApplicationBar applicationBarForRefineData;
        private AssociatedItemsSelectorInAccountViewer associatedItemsSelector;
        public static Account CurrentAccountGetter;
        private ApplicationBarIconButton deleteIconButton;
        private ApplicationBarIconButton editIconButton;
        internal PivotItem IncomeOrExpenseDetailsPivot;
        internal Grid LayoutRoot;
        internal LockablePivot MainPivot;
        internal TextBlock MoneyCurrencyBlock;
        internal HyperlinkButton MoneyCurrencyPanel;
        internal TextBlock NameBlock;
        internal HyperlinkButton NameEditor;
        internal TextBlock PoundageBlock;
        private ApplicationBarIconButton refineDataIconButton;
        internal LongListSelector RelatedItemsListControl;
        internal PivotItem RelatedItemsPivot;
        internal HyperlinkButton TransferingPoundageRatePanel;
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

        [System.Diagnostics.DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Application.LoadComponent(this, new Uri("/TinyMoneyManager;component/Pages/DialogBox/AccountInfoViewer.xaml", UriKind.Relative));
                this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
                this.MainPivot = (LockablePivot) base.FindName("MainPivot");
                this.IncomeOrExpenseDetailsPivot = (PivotItem) base.FindName("IncomeOrExpenseDetailsPivot");
                this.NameEditor = (HyperlinkButton) base.FindName("NameEditor");
                this.NameBlock = (TextBlock) base.FindName("NameBlock");
                this.AccountCategoryEditor = (HyperlinkButton) base.FindName("AccountCategoryEditor");
                this.AccountCategoryBlock = (TextBlock) base.FindName("AccountCategoryBlock");
                this.MoneyCurrencyPanel = (HyperlinkButton) base.FindName("MoneyCurrencyPanel");
                this.MoneyCurrencyBlock = (TextBlock) base.FindName("MoneyCurrencyBlock");
                this.TransferingPoundageRatePanel = (HyperlinkButton) base.FindName("TransferingPoundageRatePanel");
                this.PoundageBlock = (TextBlock) base.FindName("PoundageBlock");
                this.RelatedItemsPivot = (PivotItem) base.FindName("RelatedItemsPivot");
                this.RelatedItemsListControl = (LongListSelector) base.FindName("RelatedItemsListControl");
            }
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
                    callBack = delegate (object o) {
                        this.vm.LoadAssociatedItemsForAccount(this.Current, this.RelatedItems);
                        this.InvokeInThread(delegate {
                            this.WorkDone();
                        });
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        private void MoneyCurrencyPanel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.CurrentBalance, this.Current.Balance.Value.ToMoneyF2(), delegate (TextBox tb) {
                tb.InputScope = MoneyInputTextBox.NumberInputScope;
                tb.SelectAll();
            }, null, delegate (string result) {
                this.Current.InitialDateTime = new System.DateTime?(System.DateTime.Now);
                this.Current.Balance = new decimal?(result.ToDecimal());
                this.vm.UpdateOnSubmit(this.Current);
            });
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            EditValueInTextBoxEditor.ResultCallBack = delegate (string s) {
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
                this.associatedItemsSelector.Confirmed += new System.EventHandler<EventArgs><System.EventArgs>(this.associatedItemsSelector_Confirmed);
            }
            new PopupCotainer(this).Show(this.associatedItemsSelector);
        }

        private void RelatedItemsListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountItem selectedItem = (sender as LongListSelector).SelectedItem as AccountItem;
            if (selectedItem != null)
            {
                NavigationDataInfoFromAccountItem dataProvider = new NavigationDataInfoFromAccountItem {
                    Key = selectedItem.Id,
                    PageName = selectedItem.PageNameGetter
                };
                TinyMoneyManagerNavigationService.Instance.Navigate(this, dataProvider);
                this.RelatedItemsListControl.SelectedItem = null;
            }
        }

        public Account Current
        {
            get
            {
                return CurrentAccountGetter;
            }
        }

        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> RelatedItems { get; set; }
    }
}

