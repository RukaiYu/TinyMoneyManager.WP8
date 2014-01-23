using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TinyMoneyManager.Pages.DialogBox
{
    using NkjSoft.Extensions;
    using System.IO;
    using System.IO.IsolatedStorage;
    using NkjSoft.WPhone.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Language;
    public partial class BackupDatabaseChooser : PhoneApplicationPage
    {
        private static PageActionHandler<string> PageActionHandler;

        private string _pageNextGoTo = string.Empty;
        private bool _hasDataLoaded;
        public BackupDatabaseChooser()
        {
            InitializeComponent();

            this.Loaded += BackupDatabaseChooser_Loaded;

            this.DatabaseListToChoose.FullModeHeader = LocalizedStrings.GetCombinedText(AppResources.Choose, AppResources.FileToBackup, false)
                .ToUpper();

            this.DatabaseListToChoose.SelectionChanged += DatabaseListToChoose_SelectionChanged;
        }

        void BackupDatabaseChooser_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._hasDataLoaded)
            {
                this._hasDataLoaded = true;

                var dataBaseFiles = new Dictionary<string, string>();

                dataBaseFiles.Add(AppResources.CurrentDatabase, TinyMoneyManager.Data.TinyMoneyDataContext.DbFileName);

                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // get database files.
                    var files = new string[] { };

                    foreach (var dir in iso.GetDirectoryNames())
                    {
                        var folder = Path.Combine(dir, "*.sdf");
                        files = iso.GetFileNames(folder);

                        foreach (var file in files)
                        {
                            dataBaseFiles.Add(Path.GetFileName(file), Path.Combine(dir, file));
                        }
                    }

                }

                this.DatabaseListToChoose.ItemsSource = dataBaseFiles;
            }
        }

        void DatabaseListToChoose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatabaseListToChoose.SelectedItem != null)
            {
                var item = (KeyValuePair<string, string>)DatabaseListToChoose.SelectedItem;

                using (IsolatedStorageFile s = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var createTime = s.GetCreationTime(item.Value);

                    var formatter = "{0}: \r\n\t{1}\r\n\r\n{2}: \r\n\t{3}";

                    FileInfoBlock.Text = formatter.FormatWith(AppResources.File, item.Value, AppResources.CreateAt, createTime.ToString("yyyy/MM/dd HH:mm"));
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.SafeGoBack();
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            var dataBaseFileSelected = ((KeyValuePair<string, string>)DatabaseListToChoose.SelectedItem).Value;
            PageActionHandler.OnSelected(dataBaseFileSelected);
            this.SafeGoBack(this._pageNextGoTo, true);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                this._pageNextGoTo = e.GetNavigatingParameter("pageNextGoTo");
            }
        }

        public static void GoTo(PhoneApplicationPage fromPage, string pageNextGoTo, Action<string> afterFileChosenAndBeforeGoNext)
        {
            PageActionHandler = new PageActionHandler<string>();
            PageActionHandler.AfterSelected = afterFileChosenAndBeforeGoNext;

            fromPage.NavigateTo("/Pages/DialogBox/BackupDatabaseChooser.xaml?pageNextGoTo={0}", pageNextGoTo);
        }

    }
}