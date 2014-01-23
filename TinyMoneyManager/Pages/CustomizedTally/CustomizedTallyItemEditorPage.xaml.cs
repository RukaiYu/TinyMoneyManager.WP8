using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NkjSoft.Extensions;
using NkjSoft.WPhone.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TinyMoneyManager;
using TinyMoneyManager.Component;
using TinyMoneyManager.Controls;
using TinyMoneyManager.Data.Model;
using TinyMoneyManager.Language;
using TinyMoneyManager.Pages;
using TinyMoneyManager.ViewModels.CustomizedTallyManager;

namespace TinyMoneyManager.Pages.CustomizedTally
{
    public partial class CustomizedTallyItemEditorPage : PhoneApplicationPage
    {

        private ApplicationBarHelper apbh;
        public AccountItem associatedAccountItem;

        public TinyMoneyManager.ViewModels.CustomizedTallyManager.CustomizedTallyViewModel CustomizedTallyViewModel;
        public static System.Func<TallySchedule> EditingItemGetter;
        private FrequencySelector frequencySelector;

        public PageActionType pageAction;

        private string title = string.Empty;

        public CustomizedTallyItemEditorPage()
        {
            this.InitializeComponent();
            this.CustomizedTallyViewModel = ViewModelLocator.CustomizedTallyViewModel;
            this.apbh = new ApplicationBarHelper(this);
            TiltEffect.SetIsTiltEnabled(this, true);
            this.LoadContent();
        }

        private void AccountItemInfoEditor_Click(object sender, RoutedEventArgs e)
        {
            NewOrEditAccountItemPage.IsSelectionMode = true;
            NewOrEditAccountItemPage.RegisterSelectionMode(this.associatedAccountItem, delegate(AccountItem accountItem)
            {
                NewOrEditAccountItemPage.IsSelectionMode = false;
                this.BindEditingData(accountItem);
            });

            this.NavigateTo("{0}?action=selection&title={1}", new object[] { "/Pages/NewOrEditAccountItemPage.xaml", this.title });
        }

        public void BindEditingData(AccountItem accountItem)
        {
            this.UpdateAccountItemInfo(accountItem);
            this.associatedAccountItem = accountItem;
            this.AccountItemTypeAndCategoryInfo.Style = (Style)base.Resources["PhoneTextNormalStyle"];
            this.SetScheduledItemNameFromDetailsInfo(accountItem);
        }

        private void BindingDataToEdit()
        {
            if (((this.associatedAccountItem == null) || (this.associatedAccountItem.Account == null)) || (this.associatedAccountItem.Category == null))
            {
                if (this.AlertConfirm(AppResources.ScheduleManager_BadScheduledItemNeedToRemoveMessage, delegate
                {
                }, null) != MessageBoxResult.OK)
                {
                    this.CustomizedTallyViewModel.Delete<TallySchedule>(this.DataEditing);
                    this.SafeGoBack();
                }
            }
            else
            {
                this.NameBox.Text = this.DataEditing.Name;
                this.ToggleFavirote.IsChecked = new bool?(this.DataEditing.IsFavorite);
                this.UpdateAccountItemInfo(this.associatedAccountItem);
                this.frequencySelector.Update(this.DataEditing);
            }
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.SafeGoBack();
        }

        private void LoadContent()
        {
            System.Action<ApplicationBarIconButton>[] setters = new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = AppResources.Save;
            } };
            base.ApplicationBar.GetIconButtonFrom(0).SetPropertyValue(setters);
            System.Action<ApplicationBarIconButton>[] actionArray2 = new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = AppResources.Cancel;
            } };
            base.ApplicationBar.GetIconButtonFrom(1).SetPropertyValue(actionArray2);
            this.apbh.SelectContentWhenFocus = true;
            this.apbh.OriginalBar = base.ApplicationBar;
            this.apbh.AddTextBox(new TextBox[] { this.NameBox });
            this.frequencySelector = new FrequencySelector(this);
            this.frequencySelector.Header = AppResources.AppearPeriod.ToLowerInvariant();
            this.showTimeSettingPanel.Children.Add(this.frequencySelector);
        }

        public static void NavigateTo(PhoneApplicationPage fromPage, TallySchedule template, PageActionType pageAction)
        {
            EditingItemGetter = () => template;
            fromPage.NavigateTo("/Pages/CustomizedTally/CustomizedTallyItemEditorPage.xaml?action={0}", new object[] { pageAction });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.pageAction = e.GetNavigatingParameter("action", null).ToEnum<PageActionType>();
                if (this.pageAction == PageActionType.Add)
                {
                    this.DataEditing = new TallySchedule();
                }
                else if (EditingItemGetter != null)
                {
                    this.DataEditing = EditingItemGetter();
                    this.associatedAccountItem = this.DataEditing.ReBuildAccountItem();
                    this.BindingDataToEdit();
                }
                this.title = LocalizedStrings.GetCombinedText((this.pageAction == PageActionType.Add) ? AppResources.Create : AppResources.Edit, AppResources.TallyTemplate, false).ToLowerInvariant();
                this.PageTitle.Text = this.title;
            }
        }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            if (CheckedSave())
            {
                if (this.SubmitChange())
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        LoadData();
                    });

                    this.CancelButton_Click(sender, e);
                }
            }
        }

        private void LoadData()
        {
            Dispatcher.BeginInvoke(() =>
            {
                CustomizedTallyViewModel.LoadData();
            });
        }

        /// <summary>
        /// Checkeds the save.
        /// </summary>
        /// <returns></returns>
        private bool CheckedSave()
        {
            if (this.associatedAccountItem == null)
            {
                this.AlertNotification(AppResources.ScheduleManager_RequireAsscoiatedAccountItemMessage, null);
                return false;
            }

            if ((this.associatedAccountItem.Account == null) || (this.associatedAccountItem.Category == null))
            {
                this.AlertNotification(AppResources.NotAvaliableObjectMessage.FormatWith(new object[] { AppResources.ScheduleManager_RequireAsscoiatedAccountItemMessage }), null);
                return false;
            }

            return true;
        }

        private void SetScheduledItemNameFromDetailsInfo(AccountItem accountItem)
        {
        }

        public bool SubmitChange()
        {
            this.DataEditing.FromAccount = this.associatedAccountItem.Account;
            this.DataEditing.AssociatedCategory = this.associatedAccountItem.Category;
            this.DataEditing.Money = this.associatedAccountItem.Money;
            this.DataEditing.IsClaim = this.associatedAccountItem.IsClaim.GetValueOrDefault();
            this.DataEditing.IsFavorite = this.ToggleFavirote.IsChecked.GetValueOrDefault();
            this.DataEditing.Name = this.NameBox.Text;
            this.DataEditing.ProfileRecordType = ScheduleRecordType.TempleteRecord;
            this.DataEditing.DayofWeek = new System.DayOfWeek?(this.frequencySelector.Frequency.DayOfWeek);
            this.DataEditing.Frequency = this.frequencySelector.Frequency.Frequency;
            this.DataEditing.ValueForFrequency = new int?(this.frequencySelector.Frequency.Day);
            this.DataEditing.StartDate = new int?(this.frequencySelector.Frequency.Day);
            if (this.pageAction == PageActionType.Add)
            {
                this.DataEditing.Id = System.Guid.NewGuid();
                this.CustomizedTallyViewModel.AddAccountItemTemplete(this.DataEditing);
                this.CustomizedTallyViewModel.HasLoadItemsInStarupPage = false;
            }
            else
            {
                this.CustomizedTallyViewModel.Update<TallySchedule>(this.DataEditing);
                this.CustomizedTallyViewModel.HasLoadItemsInStarupPage = false;
                this.CustomizedTallyViewModel.LoadItemsInStartupPage();
            }
            return true;
        }

        private void UpdateAccountItemInfo(AccountItem accountItem)
        {
            this.AccountItemTypeAndCategoryInfo.Text = LocalizedStrings.GetLanguageInfoByKey(accountItem.Type.ToString());
            this.AccountItemCategoryInfo.Text = accountItem.Category.Name;
            this.AccountItemAccountName.Text = accountItem.Account.NameInfo;
            this.AccountItemAccountAmountInfo.Text = accountItem.MoneyInfo;
            this.DataEditing.Money = accountItem.Money;
        }

        public TallySchedule DataEditing { get; set; }
    }
}