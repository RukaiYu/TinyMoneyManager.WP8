namespace TinyMoneyManager.Pages.AppSettingPage
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Threading;
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
    using System.Text;

    public partial class AutoBackupingSettingPage : PhoneApplicationPage
    {
        private DataContextDataHandler appUpgrader;
        private bool _hasDataLoaded;

        public AutoBackupingSettingPage()
        {
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            this.appUpgrader = this.appUpgrader ?? DataContextDataHandler.Instance;
            base.DataContext = AppSetting.Instance;
            base.Loaded += new RoutedEventHandler(this.DataSettingPage_Loaded);
        }

        private void DataSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasDataLoaded)
            {
                this._hasDataLoaded = true;

                loadFiles();
            }
        }

        private void loadFiles()
        {
            var dataBaseFiles = new Dictionary<string, string>();

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.DirectoryExists("Data"))
                {
                    // get database files.
                    var files = new string[] { };

                    var folder = Path.Combine("Data", "*.data");
                    files = iso.GetFileNames(folder);

                    foreach (var file in files)
                    {
                        dataBaseFiles.Add(Path.GetFileNameWithoutExtension(file), Path.Combine("Data", file));
                    }
                }
            }

            this.DatafileListToChoose.ItemsSource = dataBaseFiles;

            this.RestoreDataAfterUpdating.IsEnabled = dataBaseFiles.Count > 0;
        }

        private void DoBackup()
        {
            this.MainProgressBar.IsIndeterminate = true;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    DataSynchronizationInfo info = this.appUpgrader.BackupDataToLocal();
                    bool flag = info.Result == OperationResult.Successfully;

                    Dispatcher.BeginInvoke(() =>
                    {
                        this.Alert(flag ? AppResources.BackupdataToLocalSucceed : AppResources.BackupdataToLocalFailed, null);

                        loadFiles();
                    });
                }
                catch (System.Exception exception)
                {
                    Dispatcher.BeginInvoke(() => this.Alert("Backup data failed cause of:\r\n" + exception.Message, null));
                }
                finally
                {
                    Dispatcher.BeginInvoke(() => this.MainProgressBar.IsIndeterminate = false);
                }
            });
        }

        private void DoRestore()
        {
            try
            {
                this.MainProgressBar.IsIndeterminate = true;
                if (DatafileListToChoose.SelectedItem != null)
                {
                    var item = (KeyValuePair<string, string>)DatafileListToChoose.SelectedItem;
                    DataSynchronizationInfo result = this.appUpgrader.RestoreDataFromLocal(item.Value);
                    switch (result.Result)
                    {
                        case OperationResult.Successfully:
                            this.Alert(DataContextDataHandler.GetMessageWithRestoreTips(result, true), null);

                            break;

                        case OperationResult.Failed:
                            this.Alert(this.GetLanguageInfoByKey("RestoreFailedMessage"), null);
                            break;

                        case OperationResult.Cancel:
                            break;
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                this.Alert(LocalizedStrings.GetLanguageInfoByKey("FileNotFoundExceptionMessage"), null);
            }
            catch (System.Exception exception)
            {
                this.Alert(exception.Message, null);
            }
            finally
            {
                this.MainProgressBar.IsIndeterminate = false;
            }
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

        private void RestoreDataAfterUpdating_Click(object sender, RoutedEventArgs e)
        {
            this.MainProgressBar.IsIndeterminate = true;
            this.InvokeInThread(delegate
            {
                this.DoRestore();
            });
        }

        public static void Go(PhoneApplicationPage fromPage)
        {
            fromPage.NavigateTo("/Pages/AppSettingPage/AutoBackupingSettingPage.xaml");
        }

        private void BackupDataBeforeUpdating_Click(object sender, RoutedEventArgs e)
        {
            this.DoBackup();
        }

        private void DatafileListToChoose_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (DatafileListToChoose.SelectedItem != null)
            {
                var item = (KeyValuePair<string, string>)DatafileListToChoose.SelectedItem;

                using (IsolatedStorageFile s = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var createTime = s.GetCreationTime(item.Value);

                    var formatter = "{0}: \t{1}";

                    FileInfoBlock.Text = formatter.FormatWith(AppResources.CreateAt, createTime.ToString("yyyy/MM/dd HH:mm"));
                }
            }

        }

        private void SendLog_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var content = "N/A";
                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(@"\data\log.log"))
                    {
                        using (StreamReader sr = new StreamReader(iso.OpenFile(@"\data\log.log", FileMode.Open), Encoding.UTF8))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }

                Helper.SendEmail("[log]", content);
            }
            catch (Exception)
            {
            }
        }
    }
}

