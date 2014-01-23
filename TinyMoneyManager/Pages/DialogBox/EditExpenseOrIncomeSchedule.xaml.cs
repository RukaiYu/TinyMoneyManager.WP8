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

        private FrequencySelector frequencySelector;

        private SchedulePlanningManager manager;

        public TallySchedule scheduleDataInfo;
        private ScheduleManagerViewModel scheduleManagerViewModel;


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
        }

        private void AccountItemInfoEditor_Click(object sender, RoutedEventArgs e)
        {
            NewOrEditAccountItemPage.IsSelectionMode = true;
            NewOrEditAccountItemPage.RegisterSelectionMode(this.AssociatedAccountItem, delegate(AccountItem accountItem)
            {
                NewOrEditAccountItemPage.IsSelectionMode = false;
                this.BindEditingData(accountItem);
            });
            this.NavigateTo("{0}?action=selection", new object[] { "/Pages/NewOrEditAccountItemPage.xaml" });
        }

        public void BindEditingData(AccountItem accountItem)
        {
            this.UpdateAccountItemInfo(accountItem);
            this.AssociatedAccountItem = accountItem;
            this.AccountItemTypeAndCategoryInfo.Style = (Style)base.Resources["PhoneTextNormalStyle"];
            this.SetScheduledItemNameFromDetailsInfo(accountItem);
        }

        public void BindingDataToEdit()
        {
            if (((this.associatedAccountItem == null) || (this.associatedAccountItem.Account == null)) || (this.associatedAccountItem.Category == null))
            {
                if (this.AlertConfirm(AppResources.ScheduleManager_BadScheduledItemNeedToRemoveMessage, delegate
                {
                }, null) != MessageBoxResult.OK)
                {
                    this.scheduleManagerViewModel.Delete(this.scheduleDataInfo);
                    this.SafeGoBack();
                }
            }
            else
            {
                this.UpdateAccountItemInfo(this.associatedAccountItem);
                this.ToggleActiveSwitch.Visibility = Visibility.Visible;
                this.ToggleActiveSwitch.IsChecked = this.scheduleDataInfo.IsActive;
                this.ItemSelector.IsEnabled = false;
                this.frequencySelector.Update(this.scheduleDataInfo);
            }
        }

        private void Calcel_Click(object sender, System.EventArgs e)
        {
            this.SafeGoBack();
        }

        private bool CheckSave()
        {
            if (this.AssociatedAccountItem == null)
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
            this.SetScheduledItemNameFromDetailsInfo(this.associatedAccountItem);
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
            System.Collections.Generic.Dictionary<String, String> dictionary = dictionary2;
            this.ItemSelector.ItemsSource = dictionary;
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
            this.ShowTipsOnce(IsolatedAppSetingsHelper.HowToCreateExpenseOrIncomeSchedule, "CreateExpenseOrIncomeSchedule", null);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.action = this.GetNavigatingParameter("action", null);
                if (this.action == "edit")
                {
                    System.Guid id = this.GetNavigatingParameter("id", null).ToGuid();
                    this.scheduleDataInfo = this.scheduleManagerViewModel.Tasks.FirstOrDefault<TallySchedule>(p => p.Id == id);
                    this.scheduleManagerViewModel.Current = this.scheduleDataInfo;
                    this.associatedAccountItem = this.scheduleManagerViewModel.ReBuildAccountItem(this.scheduleDataInfo);
                    this.BindingDataToEdit();
                }
                else
                {
                    this.scheduleDataInfo = new TallySchedule();
                }
                this.InitializedApplicationBar();
            }
        }

        private void SaveAndClose_Click(object sender, System.EventArgs e)
        {
            if (this.CheckSave())
            {
                this.scheduleDataInfo.ActionType = (this.AssociatedAccountItem.Type == ItemType.Expense) ? RecordActionType.CrateExpenseRecord : RecordActionType.CreateIncomeRecord;
                this.scheduleDataInfo.AssociatedCategory = this.AssociatedAccountItem.Category;
                this.scheduleDataInfo.FromAccount = this.AssociatedAccountItem.Account;
                this.scheduleDataInfo.IsClaim = this.AssociatedAccountItem.IsClaim.GetValueOrDefault();
                this.scheduleDataInfo.Money = this.AssociatedAccountItem.Money;
                this.scheduleDataInfo.Name = this.NameBlock.Text;
                this.scheduleDataInfo.Notes = this.AssociatedAccountItem.Description;
                this.scheduleDataInfo.RecordType = this.AssociatedAccountItem.Type;
                this.scheduleDataInfo.DayofWeek = new System.DayOfWeek?(this.frequencySelector.Frequency.DayOfWeek);
                this.scheduleDataInfo.Frequency = this.frequencySelector.Frequency.Frequency;
                this.scheduleDataInfo.ValueForFrequency = new int?(this.frequencySelector.Frequency.Day);
                this.scheduleDataInfo.StartDate = new int?(this.frequencySelector.Frequency.Day);
                this.scheduleDataInfo.IsActive = new bool?(this.ToggleActiveSwitch.IsChecked.GetValueOrDefault());
                this.scheduleDataInfo.CategoryId = this.AssociatedAccountItem.CategoryId;
                this.scheduleDataInfo.ProfileRecordType = ScheduleRecordType.ScheduledRecord;
                if (this.action != "edit")
                {
                    this.scheduleDataInfo.Id = System.Guid.NewGuid();
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
    }
}

