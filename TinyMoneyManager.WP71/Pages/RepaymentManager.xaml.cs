namespace TinyMoneyManager.Pages
{
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
    using TinyMoneyManager.ViewModels.NotificationManager;
    using TinyMoneyManager.Pages.NotificationCenter;

    public partial class RepaymentManager : PhoneApplicationPage
    {
        private BudgetProjectManagementViewModel budgetManagerViewModel;

        private NotificationsViewModel notificationManagerVierModel;
        private bool _hasDataLoaded;

        public RepaymentManager()
        {
            this.InitializeComponent();
            this.notificationManagerVierModel = ViewModelLocator.NotificationsViewModel;
            this.budgetManagerViewModel = ViewModelLocator.BudgetProjectViewModel;
            base.DataContext = this.notificationManagerVierModel;
            TiltEffect.SetIsTiltEnabled(this, true);
            base.Loaded += new RoutedEventHandler(this.RepaymentManager_Loaded);
            this.SetApplicationBarText();
            this.MainPivot.SelectionChanged += new SelectionChangedEventHandler(this.MainPivot_SelectionChanged);
            this.MonthlyExpenseGrid.DataContext = ViewModelLocator.MainPageViewModel.AccountMonthBudget;
        }

        private void AddItemIconButton_Click(object sender, System.EventArgs e)
        {
            NotificationEditor.Go(this, Guid.Empty, PageActionType.Add);
        }

        private void OldVersionButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/RepaymentManagerViews/RepaymentManager.xaml");
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((MenuItem)sender).Tag as TallySchedule;
            if ((tag != null) && ((this.AlertConfirm(this.GetLanguageInfoByKey("DeleteOnGoingRapaymentMessage"), null, null) == MessageBoxResult.OK)))
            {
                this.notificationManagerVierModel.DeleteNotification(tag);
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
            NotificationEditor.Go(this, id, PageActionType.Edit);
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.InvokeInThread(delegate
            {
                base.ApplicationBar.IsVisible = this.MainPivot.SelectedIndex == 1;
            });
            if (this.MainPivot.SelectedIndex == 0)
            {
                if (!ViewModelLocator.MainPageViewModel.IsSummaryListLoaded)
                {
                    this.budgetManagerViewModel.LoadMonthlyBudgetInfo();
                }
            }
            else
            {
                this.notificationManagerVierModel.LoadDataIfNot();
            }
        }

        private void RepaymentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.RepaymentList.SelectedItem as TallySchedule;
            if (selectedItem != null)
            {
                this.GoToEdit(selectedItem.Id);
                this.RepaymentList.SelectedItem = null;
            }
        }

        private void RepaymentManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasDataLoaded)
            {
                this._hasDataLoaded = true;

                var budgetAndStasticsSettings = new AppSettingListener("BudgetStaticDateRange");

                budgetAndStasticsSettings.ObjectListenTo = AppSetting.Instance;
                budgetAndStasticsSettings.NavigateUri = ViewPath.SettingPages.BudgetAndStasticsSettingsPage;
                budgetAndStasticsSettings.RegisterObjectPropertyChanged((o, e2) =>
                {
                    if (AppSetting.Instance.BudgetStatsicSettings.BudgetStatsicMode == BudgetStatsicMode.BudgetStaticModeOfByMonth)
                    {
                        return LocalizedStrings.GetLanguageInfoByKey(AppSetting.Instance.BudgetStatsicSettings.BudgetStatsicMode.ToString());
                    }

                    else
                    {
                        var startDayInfo = AppResources.FrequencyDayOfMonthFormatter.FormatWith(AppSetting.Instance.BudgetStatsicSettings.StartDay);
                        var endDayInfo = AppResources.FrequencyDayOfMonthFormatter.FormatWith(AppSetting.Instance.BudgetStatsicSettings.EndDay);

                        if (AppSetting.Instance.BudgetStatsicSettings.EndDay < AppSetting.Instance.BudgetStatsicSettings.StartDay)
                        {
                            endDayInfo = LocalizedStrings.GetCombinedText(LocalizedStrings.GetCombinedText(AppResources.Next,
                            AppResources.Month).ToLower(), AppResources.FrequencyDayOfMonthFormatter.FormatWith(AppSetting.Instance.BudgetStatsicSettings.EndDay));
                        }

                        return "{0}~{1}".FormatWith(startDayInfo, endDayInfo);
                    }
                }, "DisplayLanguage", "BudgetStatsicMode")
                    .NotifyFormat();

                BudgetAndStasticsSettingsButtonPanel.DataContext = budgetAndStasticsSettings;
            }
        }

        private void SetApplicationBarText()
        {
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.AddButtonText;
            base.ApplicationBar.GetMenuItemButtonFrom(0).Text = AppResources.RepaymentList;
        }

        private void ToggleActiveSwitch_Checked_1(object sender, RoutedEventArgs e)
        {
            var item = (sender as ToggleSwitch).Tag as TallySchedule;

            if (item != null)
            {
                notificationManagerVierModel.EnableNotification(item);
            }
        }

        private void ToggleActiveSwitch_Unchecked_1(object sender, RoutedEventArgs e)
        {
            var item = (sender as ToggleSwitch).Tag as TallySchedule;

            if (item != null)
            {
                notificationManagerVierModel.DisableNotification(item);
            }
        }

    }
}

