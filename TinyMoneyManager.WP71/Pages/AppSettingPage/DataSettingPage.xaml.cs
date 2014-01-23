namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Data.ScheduleManager;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class DataSettingPage : PhoneApplicationPage
    {

        private DataContextDataHandler appUpgrader;

        public DataSettingPage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.appUpgrader = this.appUpgrader ?? DataContextDataHandler.Instance;
            base.DataContext = AppSetting.Instance;
            base.Loaded += new RoutedEventHandler(this.DataSettingPage_Loaded);
        }

        private void AutoBackupWhenAppUpSwitcher_Checked(object sender, RoutedEventArgs e)
        {
            AppSetting.Instance.AutoBackupWhenAppUp = true;
        }

        private void AutoBackupWhenAppUpSwitcher_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void BackupDataButton_Click(object sender, RoutedEventArgs e)
        {
            Pages.DialogBox.BackupDatabaseChooser.GoTo(this, "/Pages/DataSynchronizationPage.xaml?from=normal", (s) =>
            {
                this.Alert(AppResources.TipsWhenToBackupDatabase, null);
                DataSynchronizationHandler.DatabaseFileToBackup = s;
                DataSynchronizationHandler.ToBackupDataBase = true;
            });
        }

        private void DataSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeDataCenterInfo();
        }

        private void InitializeDataCenterInfo()
        {
            this.IsThereHasSavedDataBaseFile.Text = "\r\n\x00b7 有崩溃时保存的，旧版本的'数据库文件(.sdf)'。\r\n  版本\t\t: {1}\r\n  当前版本:\t {2}. \r\n请访问 [{0}] 获取提取方式。";
            this.IsThereHasOneLocalFileNameWhenCrashed.Text = "\r\n\x00b7 有崩溃时保存的，旧版本'数据备份文件(.data)'。\r\n请单击 【还原数据】按钮进行恢复。";
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            SettingPageViewModel.Update();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataSynchronizationHandler.ToBackupDataBase = false;
        }

        private void ReloadDefaultCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ResetDataButton_Click(object sender, RoutedEventArgs e)
        {
            string text = "{0}\r\n{1}".FormatWith(new object[] { AppResources.ResetAllData_Tips, AppResources.OptNeedRestart });
            this.AlertConfirm(text, delegate
            {
                System.Action action = null;
                try
                {
                    this.BusyForWork("{0} ...".FormatWith(new object[] { AppResources.Reset.ToLowerInvariant() }));
                    base.IsEnabled = false;
                    if (action == null)
                    {
                        action = delegate
                        {
                            DataContextDataHandler.RemoveData(delegate(TinyMoneyDataContext db)
                            {
                                new SecondSchedulePlanningManager(db).Update(true);
                            });
                            DataContextDataHandler.SetReloadViewModelDataFlag();
                            this.AlertNotification("{0} {1}".FormatWith(new object[] { AppResources.Reset, AppResources.OperationSuccessfullyMessage.ToLowerInvariant() }), null);
                            base.IsEnabled = true;
                            this.WorkDone();

                            if (NavigationService.CanGoBack)
                            {
                                MessageBox.Show(AppResources.OptNeedRestart);

                                var length = NavigationService.BackStack.Count();
                                for (int i = 0; i < length; i++)
                                {
                                    NavigationService.RemoveBackEntry();
                                }

                            }

                        };
                    }
                    this.InvokeInThread(action);
                }
                catch (System.Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }, AppResources.Reset);
        }

        private void RestoreDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Alert(AppResources.TipsWhenToBackupDatabase, null);
            DataSynchronizationHandler.ToBackupDataBase = true;
            DataSynchronizationHandler.FromRestoreDatabase = true;
            this.NavigateTo("/Pages/DataSynchronizationPage.xaml?from=normal");
        }

        /// <summary>
        /// Handles the Click event of the AutoBackupingDataButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void AutoBackupingDataButton_Click(object sender, RoutedEventArgs e)
        {
            AutoBackupingSettingPage.Go(this);
        }
    }
}

