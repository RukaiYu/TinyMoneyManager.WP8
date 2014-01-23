namespace TinyMoneyManager.Pages
{
    using Coding4Fun.Phone.Controls;
    using Microsoft.Live;
    using Microsoft.Live.Controls;
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.ViewModels;

    public partial class SkyDriveDataSyncingPage : PhoneApplicationPage
    {
        public static string DefaultBackupFileName = "accountBookData{0}";
        public static string DefaultFileExtension = ".txt";
        private static readonly string defaultFilenameForBackupAndRestore = "accountBookData{0}";
        public static string Filename = string.Empty;

        public static string[] files = new string[] { ".txt" };
        private bool hasLoaded;
        private bool hasLoadHelp;

        private string justMeTag;

        private SkyDriveFolderAndFileBrowser skfb;
        private SkyDriveDataSyncingPageViewModel skyDriveViewModel;

        public System.Action<SynchronizationStepViewModel> useForUpdatingStatusOfStep;

        public SkyDriveDataSyncingPage()
        {
            System.Action<Action> action = null;
            this.justMeTag = string.Empty;
            this.InitializeComponent();
            this.skyDriveViewModel = new SkyDriveDataSyncingPageViewModel();
            base.Loaded += new RoutedEventHandler(this.SkyDriveDataSyncingPage_Loaded);
            this.signInBtn.SessionChanged += new System.EventHandler<LiveConnectSessionChangedEventArgs>(this.signInBtn_SessionChanged);
            this.signInBtn.Click += new RoutedEventHandler(this.signInBtn_Click);
            this.signInBtn.SignInText = LocalizedStrings.GetLanguageInfoByKey("SignInText");
            TiltEffect.SetIsTiltEnabled(this, true);
            this.InitializeStepViewModels();
            this.SetButtonText();
            this.CanSyncingStart(false);
            if (action == null)
            {
                action = new Action<Action>((a) =>
                {
                    base.Dispatcher.BeginInvoke(a);
                });
            }
            this.skyDriveViewModel.StatusUpdatingCallBack = action;
            this.skyDriveViewModel.SyncingFinished += new System.EventHandler<System.EventArgs>(this.skyDriveViewModel_SyncingFinished);
            this.skyDriveViewModel.ReportingStatus += new System.EventHandler<ReportStatusHandlerEventArgs>(this.skyDriveViewModel_ReportingStatus);
        }

        private void AfterClosed(SkyDriveFolderAndFileBrowser browser)
        {
            this.ChangeFileNameButton.IsEnabled = false;
            if (browser.DialogResult == MessageBoxResult.OK)
            {
                this.skyDriveViewModel.IsBackupToAFolder = browser.IsSelectAFolderAsResult;
                this.skyDriveViewModel.FileForSyncing = browser.SelectedItem;
                if (browser.IsSelectAFolderAsResult && (this.skyDriveViewModel.HandlerAction == HandlerAction.Backup))
                {
                    if (this.skyDriveViewModel.CreateFileIfNoExistsWhenBackupData)
                    {
                        ObjectFromSkyDrive.CanFolderBeEnableSelectAsResult = true;
                        this.StartButton.IsEnabled = true;
                        this.ChangeFileNameButton.IsEnabled = true;
                        string fileName = this.FileName;
                        this.FileNameTextBoxBtn.Content = browser.SelectedItem.Name + "/" + fileName;
                        this.skyDriveViewModel.FileForSyncing.ParentId = browser.SelectedItem.Id;
                        this.skyDriveViewModel.FileForSyncing.Name = fileName;
                        this.skyDriveViewModel.FileNameRenamed = fileName;
                    }
                    else
                    {
                        ObjectFromSkyDrive.CanFolderBeEnableSelectAsResult = false;
                        this.StartButton.IsEnabled = this.StartButton.IsEnabled;
                    }
                }
                else
                {
                    this.StartButton.IsEnabled = true;
                    this.skyDriveViewModel.FileNameRenamed = browser.SelectedItem.Name + DefaultFileExtension;
                    this.FileNameTextBoxBtn.Content = this.SetFileNameButtonContent(browser.ParentItem, this.skyDriveViewModel.FileNameRenamed);
                }
            }
        }

        private void CanSyncingStart(bool isToStart)
        {
            this.StartButton.IsEnabled = isToStart;
            this.FileNameTextBoxBtn.IsEnabled = isToStart;
            this.SyncActionPicker.IsEnabled = isToStart;
        }

        private void ChangeFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowNewFileNameInputPopup();
        }

        private void FileNameTextBoxBtn_Click(object sender, RoutedEventArgs e)
        {
            System.EventHandler handler = null;
            if (this.skfb == null)
            {
                this.skfb = new SkyDriveFolderAndFileBrowser(this.liveConnectClient);
                this.skfb.FileFliters = SkyDriveDataSyncingPageViewModel.BackupAndRestoreDataSyncingMode ? files : new string[] { DefaultFileExtension };
                this.skfb.ReportingSyncingStatus += new System.EventHandler<ReportStatusHandlerEventArgs>(this.skfb_ReportingSyncingStatus);
                this.skfb.ObjectBrowseringChanged += new System.EventHandler<ObjectBrowseringChangedHandlerEventArgs>(this.skfb_ObjectBrowseringChanged);
                this.skfb.ObjectBrowseringChanging += new System.EventHandler<ObjectBrowseringChangedHandlerEventArgs>(this.skfb_ObjectBrowseringChanging);
                if (handler == null)
                {
                    handler = delegate(object so, System.EventArgs se)
                    {
                        this.AfterClosed(this.skfb);
                    };
                }
                this.skfb.Closed += handler;
                this.SetChooseFileTipsLine();
                this.skfb.Show();
            }
            else
            {
                this.SetChooseFileTipsLine();
                this.skfb.Show();
            }
        }



        private void InitializePageSettingViaMode()
        {
            bool backupAndRestoreDataSyncingMode = SkyDriveDataSyncingPageViewModel.BackupAndRestoreDataSyncingMode;
            string languageInfoByKey = string.Empty;
            if (!backupAndRestoreDataSyncingMode)
            {
                languageInfoByKey = LocalizedStrings.GetLanguageInfoByKey("Upload");
                this.SyncActionPicker.Items.Add(languageInfoByKey);
                ObjectFromSkyDrive.CanFolderBeEnableSelectAsResult = true;
            }
            else
            {
                languageInfoByKey = LocalizedStrings.GetLanguageInfoByKey("Backup");
                this.SyncActionPicker.ItemsSource = new string[] { languageInfoByKey, LocalizedStrings.GetLanguageInfoByKey("Restore") };
            }
            this.FileNameTextBoxBtn.Content = LocalizedStrings.GetLanguageInfoByKey("SkyDriveSelectFileOrFolderMessage").FormatWith(new object[] { languageInfoByKey });
            this.SyncActionPicker.SelectionChanged += new SelectionChangedEventHandler(this.SyncActionPicker_SelectionChanged);
        }

        private void InitializeStepViewModels()
        {
            this.ConnectServerStepPanel.DataContext = this.skyDriveViewModel.connectingServerStep;
            this.DatatransferingStepPanel.DataContext = this.skyDriveViewModel.datatransferingStep;
            this.DataCheckStepPanel.DataContext = this.skyDriveViewModel.dataCheckingStep;
            this.ActionEndingStepPanel.DataContext = this.skyDriveViewModel.actionEndingStep;
        }

        private void LoadHelpes()
        {
            if (!this.hasLoadHelp)
            {
                this.TipsListBox.ItemsSource = AboutPageViewModel.GetHelpTextFromFile("HelpsInSkyDriveSyncing.txt");
                this.hasLoadHelp = true;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            this.skyDriveViewModel.ReportingStatus -= new System.EventHandler<ReportStatusHandlerEventArgs>(this.skyDriveViewModel_ReportingStatus);
            this.skyDriveViewModel.SyncingFinished -= new System.EventHandler<System.EventArgs>(this.skyDriveViewModel_SyncingFinished);
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SuccessfullMessage = string.Empty;
            SkyDriveDataSyncingPageViewModel.BackupAndRestoreDataSyncingMode = true;
            DefaultFileExtension = ".txt";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.BusyForWork(LocalizedStrings.GetLanguageInfoByKey("LoginLiveIDMessage"));
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ProcessError(SynchronizationStepViewModel serverStep, System.Exception exception)
        {
            serverStep.Failed();
            this.StopMainProcessBar();
        }

        private void ProcessSync()
        {
            this.skyDriveViewModel.Start();
        }

        private void RunWithCatch(System.Action action, SynchronizationStepViewModel step)
        {
            try
            {
                action();
            }
            catch (System.Exception exception)
            {
                this.Alert(this.GetLanguageInfoByKey("GetExceptionMessage") + "\r\n" + exception.Message, null);
                AppUpdater.AddErrorLog("ExceptionWhenSyncingData", exception.StackTrace, new string[0]);
                step.Failed();
                this.StopMainProcessBar();
            }
        }

        private void SetButtonText()
        {
            SynchronizationStepViewModel.CurrentStepReplacer = this.GetLanguageInfoByKey("CurrentStepReplacer");
            SynchronizationStepViewModel.FailedReplacer = this.GetLanguageInfoByKey("FailedReplacer");
            SynchronizationStepViewModel.SuccessReplacer = this.GetLanguageInfoByKey("SuccessReplacer");
        }

        private void SetChooseFileTipsLine()
        {
            this.skfb.BrowsingTipLine = LocalizedStrings.GetLanguageInfoByKey("ChooseOneTypeFile").FormatWith(new object[] { DefaultFileExtension }) + (ObjectFromSkyDrive.CanFolderBeEnableSelectAsResult ? "/ {0}".FormatWith(new object[] { LocalizedStrings.GetLanguageInfoByKey("ChooseFolder") }) : string.Empty);
        }

        private string SetFileNameButtonContent(ObjectFromSkyDrive parent, string child)
        {
            string str = string.Empty;
            if (parent != null)
            {
                str = parent.Name + "/";
            }
            else
            {
                str = "/";
            }
            return (str + child);
        }

        private void ShowNewFileNameInputPopup()
        {
            string currentFilename = this.skyDriveViewModel.FileNameRenamed.Replace(DefaultFileExtension, string.Empty);
            InputPrompt prompt = new InputPrompt
            {
                Message = LocalizedStrings.GetLanguageInfoByKey("TypeInNewFileNameMessage"),
                Value = currentFilename
            };
            prompt.Completed += delegate(object o, PopUpEventArgs<string, PopUpResult> e)
            {
                if ((((PopUpResult)e.PopUpResult) == PopUpResult.Ok) && (!string.IsNullOrEmpty(e.Result) && (e.Result.Trim() != currentFilename)))
                {
                    string str = e.Result + DefaultFileExtension;
                    string text = this.GetLanguageInfoByKey("RenameFileConfirmMessage").FormatWith(new object[] { str });
                    if (this.AlertConfirm(text, null, null) == MessageBoxResult.OK)
                    {
                        string str3 = this.FileNameTextBoxBtn.Content.ToString();
                        str3 = str3.Substring(0, str3.IndexOf("/") + 1);
                        this.FileNameTextBoxBtn.Content = str3 + str;
                        this.skyDriveViewModel.FileNameRenamed = str;
                    }
                }
            };
            prompt.IsCancelVisible = true;
            prompt.Show();
        }

        private void signedInUser()
        {
        }

        private void signedOutUser()
        {
        }

        private void signInBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void signInBtn_SessionChanged(object sender, LiveConnectSessionChangedEventArgs args)
        {
            if (((args != null) && (args.Session != null)) && (args.Status == LiveConnectSessionStatus.Connected))
            {
                this.skyDriveViewModel.InitializeClient(args.Session);
                this.CanSyncingStart(true);
                this.StartButton.IsEnabled = false;
                this.signedInUser();
            }
            else
            {
                this.signedOutUser();
                this.CanSyncingStart(false);
            }
            this.WorkDone();
        }

        private void skfb_ObjectBrowseringChanged(object sender, ObjectBrowseringChangedHandlerEventArgs e)
        {
            if (e.ObjectType == "folder")
            {
                e.SelectAsResult = false;
            }
        }

        private void skfb_ObjectBrowseringChanging(object sender, ObjectBrowseringChangedHandlerEventArgs e)
        {
            if ((this.skyDriveViewModel.HandlerAction == HandlerAction.Backup) && (e.SharedWith != SkyDriveFolderAndFileBrowser.JustMeTag))
            {
                bool flag = this.AlertConfirm(LocalizedStrings.GetLanguageInfoByKey("SharedWithLevelOverHeightMessage").FormatWith(new object[] { e.SharedWith }), null, null) == MessageBoxResult.OK;
                e.SelectAsResult = flag;
                if (!flag)
                {
                    return;
                }
            }
            if ((e.ObjectType == "folder") && (this.skyDriveViewModel.HandlerAction == HandlerAction.Backup))
            {
                bool flag2 = this.AlertConfirm(LocalizedStrings.GetLanguageInfoByKey("ConfirmMessageWhenSelecteAFolderToBackupData").FormatWith(new object[] { this.FileName, e.Name }), null, null) == MessageBoxResult.OK;
                this.skyDriveViewModel.CreateFileIfNoExistsWhenBackupData = flag2;
                e.SelectAsResult = flag2;
            }
        }

        private void skfb_ReportingSyncingStatus(object sender, ReportStatusHandlerEventArgs e)
        {
            if (e.HasError)
            {
                if (e.Excetion.Message.Contains("LiveConnectException"))
                {
                    this.Alert(this.GetLanguageInfoByKey("LiveConnectExceptionMessage"), null);
                }
                else
                {
                    this.ShowErrorMessageAfterConfirm("From Action: \r\n" + e.ActionName + "\r\n\r\n" + e.Excetion.Message + "\r\n" + e.Excetion.StackTrace, false);
                }
            }
        }

        private void SkyDriveDataSyncingPage_Loaded(object sender, RoutedEventArgs e)
        {
            System.Action action = null;
            if (!this.hasLoaded)
            {
                if (action == null)
                {
                    action = delegate
                    {
                        this.LoadHelpes();
                        this.InitializePageSettingViaMode();
                    };
                }
                this.InvokeInThread(action);
                this.hasLoaded = true;
            }
        }

        private void skyDriveViewModel_ReportingStatus(object sender, ReportStatusHandlerEventArgs e)
        {
            this.InvokeInThread(delegate
            {
                string languageInfoByKey = e.Message;
                if (string.IsNullOrEmpty(languageInfoByKey))
                {
                    languageInfoByKey = LocalizedStrings.GetLanguageInfoByKey("UploadFileToSkyDriveSuccessfullyMessage");
                }
                if (!string.IsNullOrEmpty(SuccessfullMessage))
                {
                    languageInfoByKey = SuccessfullMessage;
                }
                if (DefaultFileExtension == ".xls")
                {
                    languageInfoByKey = languageInfoByKey + LocalizedStrings.GetLanguageInfoByKey("ExcelFileMustBeOpenInLocalOffice");
                }
                if (((e.Excetion != null) || e.Message.Contains("Exception")) || e.Message.Contains("Error"))
                {
                    languageInfoByKey = "抱歉，在同步操作的时候遇到：'{0}'。可能的原因：\r\n    {1}\r\n请稍后重试。".FormatWith(new object[] { e.Message, LocalizedStrings.GetLanguageInfoByKey("LiveConnectExceptionMessage") });
                }
                this.Alert(languageInfoByKey, null);
            });
        }

        private void skyDriveViewModel_SyncingFinished(object sender, System.EventArgs e)
        {
            this.ToggleStarting(false);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleStarting(true);
            this.ToggleSteps(true);
            this.BusyForWork("syncing...");
            try
            {
                this.ProcessSync();
            }
            catch (System.Exception exception)
            {
                this.Alert("Oh, i'm hit a crash! Sorry!~~\r\n" + exception.Message, null);
                this.StopMainProcessBar();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleStarting(false);
            this.ToggleSteps(false);
            this.StopMainProcessBar();
        }

        private void StopMainProcessBar()
        {
            this.MainProcess.IsIndeterminate = false;
            this.ProcessBarRow.Visibility = Visibility.Collapsed;
            this.RestoreCheckingStatusPanel.Visibility = Visibility.Collapsed;
            this.ToggleStarting(false);
            this.WorkDone();
        }

        private void SyncActionPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SyncActionPicker != null)
            {
                int selectedIndex = this.SyncActionPicker.SelectedIndex;
                this.skyDriveViewModel.HandlerAction = (HandlerAction)selectedIndex;
                object content = this.FileNameTextBoxBtn.Content;
                if (this.skyDriveViewModel.HandlerAction == HandlerAction.Backup)
                {
                    this.ChangeFileNameButton.IsEnabled = this.skyDriveViewModel.FileForSyncing != null;
                    ObjectFromSkyDrive.CanFolderBeEnableSelectAsResult = true;
                    content = (this.skyDriveViewModel.FileForSyncing == null) ? LocalizedStrings.GetLanguageInfoByKey("SkyDriveSelectFileOrFolderMessage").FormatWith(new object[] { this.SyncActionPicker.SelectedItem.ToString() }) : content;
                }
                else if (this.skyDriveViewModel.HandlerAction == HandlerAction.Restore)
                {
                    ObjectFromSkyDrive.CanFolderBeEnableSelectAsResult = false;
                    this.ChangeFileNameButton.IsEnabled = false;
                    this.skyDriveViewModel.FileForSyncing = null;
                    content = LocalizedStrings.GetLanguageInfoByKey("SkyDriveSelectFileMessage");
                }
                this.FileNameTextBoxBtn.Content = content;
            }
        }

        private void ToggleStarting(bool isStart)
        {
            this.PivotItemContainer.IsLocked = !isStart;
            this.StopButton.IsEnabled = isStart;
            this.StartButton.IsEnabled = !isStart;
            this.MainProcess.IsIndeterminate = isStart;
            this.ProcessBarRow.Visibility = isStart ? Visibility.Visible : Visibility.Collapsed;
            this.SyncActionPicker.IsEnabled = !isStart;
            this.FileNameTextBoxBtn.IsEnabled = !isStart;
            this.signInBtn.IsEnabled = !isStart;
            this.ChangeFileNameButton.IsEnabled = false;
            if (!isStart)
            {
                this.WorkDone();
            }
        }

        private void ToggleSteps(bool isStart)
        {
            this.ConnectServerStepPanel.Visibility = isStart ? Visibility.Visible : Visibility.Collapsed;
            this.DatatransferingStepPanel.Visibility = Visibility.Collapsed;
            this.DataCheckStepPanel.Visibility = Visibility.Collapsed;
            this.ActionEndingStepPanel.Visibility = Visibility.Collapsed;
            this.skyDriveViewModel.RestartAllStep();
        }

        private void ViaPCClient_Click(object sender, System.EventArgs e)
        {
            base.NavigationService.Navigate(new Uri("/Pages/DataSynchronizationPage.xaml", UriKind.RelativeOrAbsolute));
        }

        public string FileName
        {
            get
            {
                if (SkyDriveDataSyncingPageViewModel.BackupAndRestoreDataSyncingMode)
                {
                    return defaultFilenameForBackupAndRestore.FormatWith(new object[] { ".txt" });
                }
                string source = (Filename.Length == 0) ? DefaultBackupFileName : Filename;
                return source.FormatWith(new object[] { DefaultFileExtension });
            }
        }

        private LiveConnectClient liveConnectClient
        {
            get
            {
                return this.skyDriveViewModel.LiveConnector;
            }
        }

        public static string SuccessfullMessage
        {
            get;
            set;
        }
    }
}

