namespace TinyMoneyManager.Pages.DialogBox
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Data.ScheduleManager;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages;
    using TinyMoneyManager.Pages.ScheduleManager;
    using TinyMoneyManager.ViewModels.ScheduleManager;

    public partial class EditExpenseOrIncomeSchedule : PhoneApplicationPage
    {
        private string action = "add";
        private AccountItem associatedAccountItem;
        private TransferingItem assoicatedTransferingItem;

        private FrequencySelector frequencySelector;

        private SchedulePlanningManager manager;

        public TallySchedule scheduleDataInfo;
        private ScheduleManagerViewModel scheduleManagerViewModel;

        private RecordActionType taskType;
        private bool hasInitializedBinding;

        public EditExpenseOrIncomeSchedule()
        {
            this.InitializeComponent();
            this.manager = new SecondSchedulePlanningManager(ViewModelLocator.AccountBookDataContext);
            TiltEffect.SetIsTiltEnabled(this, true);
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Save;
            base.ApplicationBar.GetIconButtonFrom(1).Text = AppResources.Cancel;
            this.SetExampleName();
            this.scheduleManagerViewModel = ViewModelLocator.ScheduleManagerViewModel;
            this.scheduleDataInfo = new TallySchedule();
            this.manager = this.scheduleManagerViewModel.ScheduleManager;
            this.InitializedFrequencySelector();
            base.DataContext = this.scheduleManagerViewModel;
            this.taskType = RecordActionType.CreateTranscationRecord;
            this.Loaded += EditExpenseOrIncomeSchedule_Loaded;
        }

        void EditExpenseOrIncomeSchedule_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasInitializedBinding)
            {
                this.action = this.GetNavigatingParameter("action", null);
                this.InitializedApplicationBar();
                if (ViewModelLocator.AccountViewModel.Accounts.Count == 0)
                {
                    ViewModelLocator.AccountViewModel.QuickLoadData();
                }

                hasInitializedBinding = true;

                if (this.action == "edit")
                {
                    System.Guid id = this.GetNavigatingParameter("id", null).ToGuid();
                    this.scheduleDataInfo = this.scheduleManagerViewModel.Tasks.FirstOrDefault<TallySchedule>(p => p.Id == id);
                    this.scheduleManagerViewModel.Current = this.scheduleDataInfo;

                    this.taskType = this.scheduleDataInfo.ActionHandlerType;

                    if (this.taskType == RecordActionType.CreateTranscationRecord)
                    {
                        this.associatedAccountItem = this.scheduleManagerViewModel.ReBuildAccountItem(this.scheduleDataInfo);

                        if (this.associatedAccountItem != null)
                        {
                            this.associatedAccountItem.PropertyChanged += associatedAccountItem_PropertyChanged;
                        }
                    }
                    else
                    {
                        this.assoicatedTransferingItem = this.scheduleManagerViewModel.ReBuildTransferingItem(this.scheduleDataInfo);
                    }

                    this.BindingDataToEdit();
                }
                else
                {
                    this.scheduleDataInfo = new TallySchedule();
                }
            }
        }

        void associatedAccountItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // to save pictures and peoples when the property changed.
            if (e.PropertyName == AccountItem.PictureTotalInfoForEditProperty)
            {
                this.scheduleDataInfo.Pictures = this.associatedAccountItem.PicturesOutOfDatabase
                    .Select(p => p.PictureId).ToArray();

                this.manager.UpdateTaskInfo(this.scheduleDataInfo);
            }

            if (e.PropertyName == AccountItem.PeopleTotalInfoForEditProperty)
            {
                this.scheduleDataInfo.Peoples = this.associatedAccountItem.PeoplesOutOfDatabase
                    .Select(p => p.PeopleId).ToArray();

                this.manager.UpdateTaskInfo(this.scheduleDataInfo);
            }
        }

        private void AccountItemInfoEditor_Click(object sender, RoutedEventArgs e)
        {
            if (taskType == RecordActionType.CreateTranscationRecord)
            {
                NewOrEditAccountItemPage.IsSelectionMode = true;
                NewOrEditAccountItemPage.SelectionObjectType = ScheduleRecordType.ScheduledRecord;
                NewOrEditAccountItemPage.ScheduledTaskInfo = scheduleDataInfo;
                NewOrEditAccountItemPage.RegisterSelectionMode(this.AssociatedAccountItem, (accountItem) =>
                {
                    NewOrEditAccountItemPage.IsSelectionMode = false;

                    NewOrEditAccountItemPage.SelectionObjectType = ScheduleRecordType.ProfileRecord;
                    this.BindEditingData(accountItem);
                });
                this.NavigateTo("{0}?action=selection", new object[] { "/Pages/NewOrEditAccountItemPage.xaml" });
            }
            else if (taskType == RecordActionType.CreateTransferingRecord)
            {
                AccountTransferingPage.IsSelectionMode = true;
                AccountTransferingPage.RegisterSelectionMode(this.assoicatedTransferingItem, transferingItem =>
                {
                    NewOrEditAccountItemPage.IsSelectionMode = false;
                    this.BindEditingData(transferingItem);
                    this.assoicatedTransferingItem = transferingItem;
                });

                this.NavigateTo("{0}?action=selection", new object[] { "/Pages/AccountTransferingPage.xaml" });
            }
        }

        public void BindEditingData(AccountItem accountItem)
        {
            if (taskType == RecordActionType.CreateTranscationRecord)
            {
                this.UpdateAccountItemInfo(accountItem);
                this.AssociatedAccountItem = accountItem;
                this.AccountItemTypeAndCategoryInfo.Style = (Style)base.Resources["PhoneTextNormalStyle"];
                this.SetScheduledItemNameFromDetailsInfo(accountItem);
            }
            else if (taskType == RecordActionType.CreateTransferingRecord)
            {
                BindEditingData(assoicatedTransferingItem);
            }
        }

        public void BindEditingData(TransferingItem transferingItem)
        {
            this.UpdateAssoicatedTransferingItemInfo(transferingItem);
            this.AccountItemTypeAndCategoryInfo.Style = (Style)base.Resources["PhoneTextNormalStyle"];
            this.SetScheduledItemNameFromDetailsInfo(transferingItem);
        }

        /// <summary>
        /// Updates the assoicated transfering item info.
        /// </summary>
        /// <param name="assoicatedTransferingItem">The assoicated transfering item.</param>
        private void UpdateAssoicatedTransferingItemInfo(TransferingItem transferingItem)
        {
            this.AccountItemTypeAndCategoryInfo.Text = "{0}“{1}”".FormatWith(AppResources.From.ToLower(), transferingItem.FromAccountName);
            this.AccountItemAccountName.Text = "{0}“{1}”".FormatWith(AppResources.To.ToLower(), transferingItem.ToAccountName);
            this.AccountItemAccountAmountInfo.Text = transferingItem.AmountInfo;
            this.scheduleDataInfo.Money = transferingItem.Amount;
        }

        public void BindingDataToEdit()
        {
            if (taskType == RecordActionType.CreateTranscationRecord)
            {
                if (((this.associatedAccountItem == null) || (this.associatedAccountItem.Account == null)) || (this.associatedAccountItem.Category == null))
                {
                    if (this.AlertConfirm(AppResources.ScheduleManager_BadScheduledItemNeedToRemoveMessage, delegate
                    {
                    }, null) != MessageBoxResult.OK)
                    {
                        this.scheduleManagerViewModel.Delete(this.scheduleDataInfo);
                        this.SafeGoBack();
                        return;
                    }
                }
                else
                {
                    this.UpdateAccountItemInfo(this.associatedAccountItem);
                }
            }
            else if (taskType == RecordActionType.CreateTransferingRecord)
            {
                this.ItemSelector.SelectedIndex = 1;
                if (((this.assoicatedTransferingItem == null) || (this.assoicatedTransferingItem.FromAccount == null)) || (this.assoicatedTransferingItem.ToAccount == null))
                {
                    if (this.AlertConfirm(AppResources.ScheduleManager_BadScheduledItemNeedToRemoveMessage, delegate
                    {
                    }, null) != MessageBoxResult.OK)
                    {
                        this.scheduleManagerViewModel.Delete(this.scheduleDataInfo);
                        this.SafeGoBack();
                        return;
                    }
                }
                else
                {
                    this.UpdateAssoicatedTransferingItemInfo(this.assoicatedTransferingItem);
                }
            }

            this.ToggleActiveSwitch.Visibility = Visibility.Visible;
            this.ToggleActiveSwitch.IsChecked = this.scheduleDataInfo.IsActive;
            this.ItemSelector.IsEnabled = false;
            this.frequencySelector.Update(this.scheduleDataInfo);
        }

        private void Calcel_Click(object sender, System.EventArgs e)
        {
            this.SafeGoBack();
        }

        private bool CheckSave()
        {
            if (taskType == RecordActionType.CreateTranscationRecord)
            {
                if (this.associatedAccountItem == null)
                {
                    this.AlertNotification(AppResources.ScheduleManager_RequireAsscoiatedAccountItemMessage, null);
                    return false;
                }
                if ((this.AssociatedAccountItem.Account == null) || (this.AssociatedAccountItem.Category == null))
                {
                    this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { AppResources.ScheduleManager_RequireAsscoiatedAccountItemMessage }), null);
                    return false;
                }
                if (this.scheduleDataInfo.Name.IsNullOrEmpty())
                {
                    this.AlertNotification(AppResources.SomethingIsRequired.FormatWith(new object[] { AppResources.Name }), null);
                    return false;
                }
            }

            else if (taskType == RecordActionType.CreateTransferingRecord)
            {
                if (this.assoicatedTransferingItem == null)
                {
                    this.AlertNotification(AppResources.ScheduleManager_RequireAsscoiatedAccountItemMessage, null);
                    return false;
                }
                if ((this.assoicatedTransferingItem.FromAccount == null) || (this.assoicatedTransferingItem.FromAccount == null))
                {
                    this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { AppResources.ScheduleManager_RequireAsscoiatedAccountItemMessage }), null);
                    return false;
                }

            }

            this.scheduleDataInfo.Notes = this.frequencySelector.Frequency.GetSummary(this.frequencySelector.DayOfWeekSelector.SelectedItem.ToString());
            return true;
        }

        private void deleteMenu_Click(object sender, System.EventArgs e)
        {
            System.Action isOkToDo = null;
            TallySchedule scheduleItem = this.scheduleDataInfo;
            if (scheduleItem != null)
            {
                if (isOkToDo == null)
                {
                    isOkToDo = delegate
                    {
                        this.scheduleManagerViewModel.Delete(scheduleItem);
                        this.SafeGoBack();
                    };
                }
                this.AlertConfirm(AppResources.DeleteAccountItemMessage, isOkToDo, null);
            }
        }

        private void executeOnceButton_Click(object sender, System.EventArgs e)
        {
            if (this.scheduleDataInfo != null)
            {
                this.scheduleManagerViewModel.ExecuteTask(this.scheduleDataInfo);
            }
        }

        private void Frequency_EveryMonth_Day_Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.SetScheduledItemNameFromDetailsInfo(this.associatedAccountItem);
        }

        private void Frequency_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Day")
            {
                this.FrequencyType_SelectionChanged(sender, null);
            }
        }

        private void FrequencyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (taskType == RecordActionType.CreateTranscationRecord)
            {
                this.SetScheduledItemNameFromDetailsInfo(this.associatedAccountItem);
            }
            else if (taskType == RecordActionType.CreateTransferingRecord)
            {
                this.SetScheduledItemNameFromDetailsInfo(this.assoicatedTransferingItem);
            }
        }

        private void InitializedApplicationBar()
        {
            if (this.action == "edit")
            {
                ApplicationBarMenuItem item = new ApplicationBarMenuItem(AppResources.Delete);
                ApplicationBarMenuItem item2 = new ApplicationBarMenuItem(AppResources.LoadAssociated);
                ApplicationBarMenuItem item3 = new ApplicationBarMenuItem(AppResources.ExecuteNow);
                base.ApplicationBar.MenuItems.Add(item);
                base.ApplicationBar.MenuItems.Add(item2);
                base.ApplicationBar.MenuItems.Add(item3);
                item2.Click += new System.EventHandler(this.viewRecentItemsMenu_Click);
                item.Click += new System.EventHandler(this.deleteMenu_Click);
                item3.Click += new System.EventHandler(this.executeOnceButton_Click);
            }
        }

        private void InitializedFrequencySelector()
        {
            System.Collections.Generic.Dictionary<String, String> dictionary2 = new System.Collections.Generic.Dictionary<string, string>();
            dictionary2.Add("ExpenseOrIncomeTask", AppResources.SelectAccountItemsLabel);
            dictionary2.Add("TransferingAccountTask", AppResources.TransferingAccount.ToLower());

            this.ItemSelector.ItemsSource = dictionary2;
            this.frequencySelector = new FrequencySelector(this);
            Grid.SetRow(this.frequencySelector, 2);
            this.ContentPanel.Children.Add(this.frequencySelector);
            this.frequencySelector.FrequencyType.SelectionChanged += new SelectionChangedEventHandler(this.FrequencyType_SelectionChanged);
            this.frequencySelector.Frequency.PropertyChanged += new PropertyChangedEventHandler(this.Frequency_PropertyChanged);
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            EditValueInTextBoxEditor.ResultCallBack = delegate(string s)
            {
                if (s.Length == 0)
                {
                    this.SetExampleName();
                }
                else
                {
                    this.NameBlock.Text = s;
                    this.scheduleDataInfo.Name = s;
                }
            };

            this.NavigateTo(ViewPath.EditValueInTextBoxEditor, new object[] { AppResources.Name, this.scheduleDataInfo.Name });
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            this.scheduleManagerViewModel.Current = new TallySchedule();
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                this.ShowTipsOnce(IsolatedAppSetingsHelper.HowToCreateExpenseOrIncomeSchedule, "CreateExpenseOrIncomeSchedule", null);
            }
        }

        private void SaveAndClose_Click(object sender, System.EventArgs e)
        {
            if (this.CheckSave())
            {
                this.scheduleDataInfo.ProfileRecordType = ScheduleRecordType.ScheduledRecord;
                this.scheduleDataInfo.Name = this.NameBlock.Text; this.scheduleDataInfo.DayofWeek = new System.DayOfWeek?(this.frequencySelector.Frequency.DayOfWeek);
                this.scheduleDataInfo.Frequency = this.frequencySelector.Frequency.Frequency;
                this.scheduleDataInfo.ValueForFrequency = new int?(this.frequencySelector.Frequency.Day);
                this.scheduleDataInfo.StartDate = new int?(this.frequencySelector.Frequency.Day);
                this.scheduleDataInfo.IsActive = new bool?(this.ToggleActiveSwitch.IsChecked.GetValueOrDefault());

                if (this.action != "edit")
                {
                    this.scheduleDataInfo.Id = System.Guid.NewGuid();
                }

                if (taskType == RecordActionType.CreateTranscationRecord)
                {
                    this.scheduleDataInfo.ActionType = (this.AssociatedAccountItem.Type == ItemType.Expense) ? RecordActionType.CrateExpenseRecord : RecordActionType.CreateIncomeRecord;
                    this.scheduleDataInfo.AssociatedCategory = this.AssociatedAccountItem.Category;
                    this.scheduleDataInfo.FromAccount = this.AssociatedAccountItem.Account;
                    this.scheduleDataInfo.IsClaim = this.AssociatedAccountItem.IsClaim.GetValueOrDefault();
                    this.scheduleDataInfo.Money = this.AssociatedAccountItem.Money;
                    this.scheduleDataInfo.Notes = this.AssociatedAccountItem.Description;
                    this.scheduleDataInfo.RecordType = this.AssociatedAccountItem.Type;
                    this.scheduleDataInfo.CategoryId = this.AssociatedAccountItem.CategoryId;

                    if (this.AssociatedAccountItem.PictureNumbers > 0)
                    {
                        if (action != "edit")
                        {
                            this.scheduleDataInfo.Pictures = this.associatedAccountItem.PicturesOutOfDatabase.Select(p => p.PictureId)
                             .ToArray();

                            this.associatedAccountItem.Pictures.ForEach(p =>
                            {
                                p.AttachedId = this.scheduleDataInfo.Id;
                                p.Tag = "ScheduledAccountItems";
                            });


                            ViewModelLocator.PicturesViewModel.SavePictures(this.AssociatedAccountItem.Pictures);

                            ViewModelLocator.PicturesViewModel.InsertPicturesOnSubmit(this.AssociatedAccountItem.Pictures);
                        }
                    }

                    if (this.associatedAccountItem.PeopleNumbers > 0)
                    {
                        if (action != "edit")
                        {
                            this.scheduleDataInfo.Peoples = this.associatedAccountItem.PeoplesOutOfDatabase.Select(p => p.PeopleId)
                                 .ToArray();

                            this.associatedAccountItem.PeoplesOutOfDatabase.ForEach(p =>
                            {
                                p.AttachedId = this.scheduleDataInfo.Id;
                                // p.Tag = "ScheduledAccountItems";
                            });

                            ViewModelLocator.PeopleViewModel.InsertAndSubmit(this.AssociatedAccountItem.Peoples);
                        }
                    }

                }
                else
                {
                    this.scheduleDataInfo.FromAccount = this.assoicatedTransferingItem.FromAccount;
                    this.scheduleDataInfo.ToAccount = this.assoicatedTransferingItem.ToAccount;
                    this.scheduleDataInfo.Money = this.assoicatedTransferingItem.Amount;
                    this.scheduleDataInfo.Notes = this.assoicatedTransferingItem.Notes;
                    this.scheduleDataInfo.Currency = this.assoicatedTransferingItem.Currency;
                    this.scheduleDataInfo.ActionType = RecordActionType.CreateTransferingRecord;
                    this.scheduleDataInfo.RecordType = ItemType.All;
                    this.scheduleDataInfo.TransferingPoundageAmount = this.assoicatedTransferingItem.TransferingPoundageAmount;
                }

                if (this.action != "edit")
                {
                    this.scheduleManagerViewModel.CreateAccountItemScheduleItem(this.scheduleDataInfo);
                }
                else
                {
                    this.scheduleManagerViewModel.Update(this.scheduleDataInfo);
                }

                base.NavigationService.GoBack();
            }
        }

        private void SetExampleName()
        {
            this.NameBlock.Text = "{0}({1})".FormatWith(new object[] { AppResources.WatermaskForName, AppResources.ScheduleItemNameExapmle });
        }

        private void SetScheduledItemNameFromDetailsInfo(AccountItem accountItem)
        {
            if (this.action == "edit")
            {
                this.NameBlock.Text = this.scheduleDataInfo.Name;
            }
            else
            {
                if (this.action == "add")
                {
                    this.SetExampleName();
                }
                if (accountItem != null)
                {
                    string str = (accountItem == null) ? string.Empty : accountItem.MoneyInfo;
                    string str2 = (accountItem == null) ? string.Empty : accountItem.Category.CategoryInfo;
                    this.NameBlock.Text = AppResources.ScheduleItemNameFormatter.FormatWith(new object[] { this.frequencySelector.Frequency.GetSummary(string.Empty), str, str2 });
                    this.scheduleDataInfo.Name = this.NameBlock.Text;
                }
            }
        }

        /// <summary>
        /// Sets the scheduled item name from details info.
        /// </summary>
        /// <param name="transferingItem">The transfering item.</param>
        private void SetScheduledItemNameFromDetailsInfo(TransferingItem transferingItem)
        {
            if (this.action == "edit")
            {
                this.NameBlock.Text = this.scheduleDataInfo.Name;
            }
            else
            {
                if (this.action == "add")
                {
                    this.SetExampleName();
                }
                if (transferingItem != null)
                {
                    string str = (transferingItem == null) ? string.Empty : transferingItem.AmountInfo;
                    string str2 = (transferingItem == null) ? string.Empty : transferingItem.Notes;
                    this.NameBlock.Text = AppResources.ScheduleItemNameFormatter.FormatWith(new object[] {
                        this.frequencySelector.Frequency.GetSummary(string.Empty), str, str2 });
                    this.scheduleDataInfo.Name = this.NameBlock.Text;
                }
            }
        }

        private void UpdateAccountItemInfo(AccountItem accountItem)
        {
            this.AccountItemTypeAndCategoryInfo.Text = string.Format("{0}, {1}", LocalizedStrings.GetLanguageInfoByKey(accountItem.Type.ToString()), accountItem.Category.CategoryInfo);
            this.AccountItemAccountName.Text = accountItem.Account.NameInfo;
            this.AccountItemAccountAmountInfo.Text = accountItem.MoneyInfo;
            this.scheduleDataInfo.Money = accountItem.Money;
        }

        private void viewRecentItemsMenu_Click(object sender, System.EventArgs e)
        {
            TallySchedule scheduleItem = this.scheduleDataInfo;
            ScheduledItemInfoViewer.CurrentAccountGetter = () => scheduleItem;
            this.NavigateTo("/Pages/ScheduleManager/ScheduledItemInfoViewer.xaml?action=view");
        }

        public AccountItem AssociatedAccountItem
        {
            get
            {
                return this.associatedAccountItem;
            }
            set
            {
                if (this.associatedAccountItem != value)
                {
                    this.associatedAccountItem = value;
                }
            }
        }

        private void ItemSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var indexOfSelectedItem = ItemSelector.SelectedIndex;

                if (indexOfSelectedItem == 0)
                {
                    this.taskType = RecordActionType.CreateTranscationRecord;
                }
                else
                {
                    this.taskType = RecordActionType.CreateTransferingRecord;
                }
            }
        }
    }
}

