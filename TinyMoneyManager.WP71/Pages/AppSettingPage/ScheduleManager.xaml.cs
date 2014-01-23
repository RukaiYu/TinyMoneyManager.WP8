namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.ScheduleManager;
    using TinyMoneyManager.ViewModels.ScheduleManager;

    public partial class ScheduleManager : PhoneApplicationPage
    {

        private ScheduleManagerViewModel scheduleManagerViewModel;

        public ScheduleManager()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.scheduleManagerViewModel = ViewModelLocator.ScheduleManagerViewModel;
            base.DataContext = this.scheduleManagerViewModel;
            base.Loaded += new RoutedEventHandler(this.ScheduleManager_Loaded);
            this.InitializeMenus();
        }

        private void AddScheduleIconButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo(ViewPath.ScheduleEditorPage, new object[] { "add", "" });
        }

        private void Delete_Item_Click(object sender, RoutedEventArgs e)
        {
            System.Action isOkToDo = null;
            TallySchedule scheduleItem = (sender as MenuItem).Tag as TallySchedule;
            if (scheduleItem != null)
            {
                if (isOkToDo == null)
                {
                    isOkToDo = delegate
                    {
                        this.scheduleManagerViewModel.Delete(scheduleItem);
                    };
                }
                this.AlertConfirm(AppResources.DeleteAccountItemMessage, isOkToDo, null);
            }
        }

        private void Edit_Item_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule tag = (sender as MenuItem).Tag as TallySchedule;
            if (tag != null)
            {
                this.GoToEditTallyScheduleItem(tag);
            }
        }

        private void ExecuteNow_Click(object sender, System.EventArgs e)
        {
            this.scheduleManagerViewModel.Test();
            this.scheduleManagerViewModel.RecoveryDatas(true, null);
        }

        private void ExecuteNow_Item_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule tag = (sender as MenuItem).Tag as TallySchedule;
            if (tag != null)
            {
                this.scheduleManagerViewModel.ExecuteTask(tag);
            }
        }

        private void GoToEditTallyScheduleItem(TallySchedule scheduleItem)
        {
            this.NavigateTo(ViewPath.ScheduleEditorPage, new object[] { "edit", scheduleItem.Id });
            this.ScheduledItems.SelectedItem = null;
        }


        private void InitializeMenus()
        {
            base.ApplicationBar.GetMenuItemButtonFrom(0).Text = AppResources.Refresh;
            base.ApplicationBar.GetIconButtonFrom(0).Text = AppResources.Add;
            System.Action<ApplicationBarMenuItem>[] setters = new System.Action<ApplicationBarMenuItem>[] { delegate (ApplicationBarMenuItem p) {
                p.Text = AppResources.ExecuteNow;
            } };
            base.ApplicationBar.GetMenuItemButtonFrom(1).SetPropertyValue(setters);
            ApplicationBarMenuItem item = new ApplicationBarMenuItem(AppResources.Tips);
            item.Click += delegate(object o, System.EventArgs e)
            {
                this.Alert(AppResources.HowToCreateExpenseOrIncomeSchedule, null);
            };

            base.ApplicationBar.MenuItems.Add(item);
        }

        private void RefreshPlanning_Click(object sender, System.EventArgs e)
        {
            this.AlertConfirm(AppResources.RefreshSchedulePlaningMessage, delegate
            {
                this.scheduleManagerViewModel.ScheduleManager.Update(true);
            }, null);
        }

        private void ScheduledItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedItem = this.ScheduledItems.SelectedItem;
            if (selectedItem != null)
            {
                TallySchedule scheduleItem = selectedItem as TallySchedule;
                this.GoToEditTallyScheduleItem(scheduleItem);
            }
        }

        private void ScheduleManager_Loaded(object sender, RoutedEventArgs e)
        {
            this.scheduleManagerViewModel.SetupPlanningFirstTime();
            this.scheduleManagerViewModel.LoadDataIfNot();
            this.scheduleManagerViewModel.RecoveryDatas(true, null);
        }

        private void View_History_Click(object sender, RoutedEventArgs e)
        {
            TallySchedule scheduleItem = (sender as MenuItem).Tag as TallySchedule;
            ScheduledItemInfoViewer.CurrentAccountGetter = () => scheduleItem;
            this.NavigateTo("/Pages/ScheduleManager/ScheduledItemInfoViewer.xaml?action=view");
        }
    }
}

