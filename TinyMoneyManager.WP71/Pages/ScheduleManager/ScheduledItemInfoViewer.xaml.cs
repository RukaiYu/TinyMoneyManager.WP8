namespace TinyMoneyManager.Pages.ScheduleManager
{
    using JasonPopupDemo;
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages;
    using TinyMoneyManager.ViewModels;
    using TinyMoneyManager.ViewModels.ScheduleManager;

    public partial class ScheduledItemInfoViewer : PhoneApplicationPage, INotifyPropertyChanged
    {
        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> AssociatedItems;
        private AssociatedItemsSelectorInAccountViewer associatedItemsSelector;
        public static System.Func<TallySchedule> CurrentAccountGetter;

        private ScheduleManagerViewModel schduleManagerViewModle;
        private string stasticItemsTips;

        public event PropertyChangedEventHandler PropertyChanged;

        public ScheduledItemInfoViewer()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.InitializeApplicationBars();
            base.DataContext = this;
            this.schduleManagerViewModle = ViewModelLocator.ScheduleManagerViewModel;
            base.Loaded += new RoutedEventHandler(this.ScheduledItemInfoViewer_Loaded);
        }

        private void associatedItemsSelector_Confirmed(object sender, System.EventArgs e)
        {
            this.schduleManagerViewModle.HasLoadAssociatedItemsForCurrentViewAccount = false;
            this.LoadAssociatedItems();
        }

        private void DeleteBudgetMenuItem(object sender, RoutedEventArgs e)
        {
            if (this.AlertConfirm(AppResources.DeleteAccountItemMessage, null, null) == MessageBoxResult.OK)
            {
                AccountItem tag = (sender as MenuItem).Tag as AccountItem;
                ViewModelLocator.AccountViewModel.HandleAccountItemDeleting(tag);
                GroupByCreateTimeAccountItemViewModel.RemoveItemFrom(this.AssociatedItems, tag);
            }
        }

        private void deleteIconButton_Click(object sender, System.EventArgs e)
        {
            if (this.AlertConfirm(AppResources.DeleteAccountItemMessage, null, null) == MessageBoxResult.OK)
            {
                this.schduleManagerViewModle.Delete(this.Current);
                base.NavigationService.GoBack();
            }
        }

        private void InitializeApplicationBars()
        {
        }

        private void LoadAssociatedItems()
        {
            this.BusyForWork(AppResources.Loading);

            if (!this.schduleManagerViewModle.HasLoadAssociatedItemsForCurrentViewAccount)
            {
                int totalRecords = 0;
                decimal sumOfAmount = 0.0M;
                System.Action<AccountItem> itemAdded = delegate(AccountItem a)
                {
                    totalRecords++;
                    sumOfAmount += a.GetMoney().GetValueOrDefault();
                };

                if (this.Current != null && this.Current.ActionHandlerType == RecordActionType.CreateTransferingRecord)
                {
                    this.RelatedItemsListControl.ItemTemplate = LayoutRoot.Resources["TemplateForTransferingAccountItem"] as DataTemplate;
                }
                else
                {
                }

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    var items = new ObservableCollection<GroupByCreateTimeAccountItemViewModel>(
                        this.schduleManagerViewModle.GetGroupedRelatedItems(this.Current, true, itemAdded));

                    Dispatcher.BeginInvoke(() =>
                    {
                        this.AssociatedItems = items;
                        this.RelatedItemsListControl.ItemsSource = this.AssociatedItems;

                        this.StasticItemsTips = AppResources.RecordsAndAmountInfoForAccountRealtedItems.FormatWith(
                            totalRecords, LocalizedObjectHelper.GetLocalizedStringFrom(this.Current.RecordType.ToString()),
                            AccountItemMoney.GetMoneyInfoWithCurrency(AppSetting.Instance.CurrencyInfo.CurrencyString, sumOfAmount));

                        this.WorkDone();
                    });
                });
            }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.GetNavigatingParameter("to", null);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.schduleManagerViewModle.HasLoadAssociatedItemsForCurrentViewAccount = false;
            base.OnNavigatingFrom(e);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void refineDataIconButton_Click(object sender, System.EventArgs e)
        {
            if (this.associatedItemsSelector == null)
            {
                this.associatedItemsSelector = new AssociatedItemsSelectorInAccountViewer(this.schduleManagerViewModle.SearchingCondition);
                this.associatedItemsSelector.Confirmed += new System.EventHandler<System.EventArgs>(this.associatedItemsSelector_Confirmed);
                this.associatedItemsSelector.ExportDataTypeSelector.SelectedIndex = 2;
                this.associatedItemsSelector.ExportDataTypeSelector.IsEnabled = false;
            }
            new PopupCotainer(this).Show(this.associatedItemsSelector);
        }

        private void RelatedItemsListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Current != null
                && this.Current.ActionHandlerType == RecordActionType.CreateTransferingRecord)
            {
                return;
            }

            AccountItem selectedItem = this.RelatedItemsListControl.SelectedItem as AccountItem;
            if (selectedItem != null)
            {
                NewOrEditAccountItemPage.currentEditObject = selectedItem;
                this.NavigateTo("/Pages/AccountItemViews/AccountItemViewer.xaml?fromSelf=false&id={0}", new object[] { selectedItem.Id });
                this.RelatedItemsListControl.SelectedItem = null;
            }
        }

        private void ScheduledItemInfoViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadAssociatedItems();
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.refineDataIconButton_Click(sender, null);
        }

        public TallySchedule Current
        {
            get
            {
                return CurrentAccountGetter();
            }
        }

        public string StasticItemsTips
        {
            get
            {
                return this.stasticItemsTips;
            }
            set
            {
                if (value != this.stasticItemsTips)
                {
                    this.stasticItemsTips = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StasticItemsTips"));
                    }
                }
            }
        }
    }
}

