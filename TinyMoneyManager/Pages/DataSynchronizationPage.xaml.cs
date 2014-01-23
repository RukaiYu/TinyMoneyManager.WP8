namespace TinyMoneyManager.Pages
{
    using Coding4Fun.Phone.Controls;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.DataSynchronizationService;
    using TinyMoneyManager.DataSyncing;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels;

    public partial class DataSynchronizationPage : PhoneApplicationPage
    {
        private SynchronizationStepViewModel actionEndingStep;

        private ApplicationBar appBarForSync;
        private SynchronizationStepViewModel connectingServerStep;

        private SynchronizationStepViewModel dataCheckingStep;

        private DataSynchronizationHandler dataSynchronizationHandler;

        private DataSyncingObjectManager DataSyncingManager;
        private SynchronizationStepViewModel datatransferingStep;

        private string fromMode = string.Empty;

        public static string IPAndPortForSaftMode = "";
        private bool isFromBackKeyPress;

        private bool needRestart;
        public static bool NeedToConfirmRebuildDatabase = false;

        private DataSynchronizationServiceClient serverProxy;
        private ApplicationBarIconButton settingButton;

        private SynchronizationManagerViewModel syncingManagerViewModel = ViewModelLocator.SynchronizationManagerViewModel;

        private AboutPageViewModel tipsViewModel;

        public DataSynchronizationPage()
        {
            this.InitializeComponent();
            this.initializeAppBar();
            this.RestoreCheckingStatusPanel.Visibility = Visibility.Collapsed;
            this.InitializeStepViewModels();
            this.syncingManagerViewModel = ViewModelLocator.SynchronizationManagerViewModel;
            base.DataContext = this.syncingManagerViewModel;
            this.InitializeDataSynchronizationHandler();
            this.RestoreCheckingStatusPanel.DataContext = this.dataSynchronizationHandler;
            this.SetButtonText();
            base.Loaded += new RoutedEventHandler(this.DataSynchronizationPage_Loaded);
            base.BackKeyPress += new System.EventHandler<CancelEventArgs>(this.DataSynchronizationPage_BackKeyPress);
            this.InitializeTips();
        }

        private void client_GetDataCompleted(object sender, GetDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.ProcessError(this.connectingServerStep, e.Error);
            }
            else
            {
                this.connectingServerStep.Success();
                this.datatransferingStep.Start();
                if (this.SyncActionPicker.SelectedIndex == 0)
                {
                    try
                    {
                        DataSynchronizationInfo data = null;
                        BackupDataOption option = new BackupDataOption
                        {
                            TotalEffects = 0
                        };
                        DataBackupedFromPhone phone = this.dataSynchronizationHandler.BackupData(option, delegate(DataSynchronizationInfo callBack)
                        {
                            data = callBack;
                        });
                        DataSynchronizationArgs arg = new DataSynchronizationArgs
                        {
                            AccountItemListXmlSource = phone.AccountItemXmlSource,
                            AppSettingsXmlSource = data.GetMessage(),
                            CategoryListXmlSource = phone.CategoryListXmlSource,
                            SdfFileContent = phone.SdfFileContent
                        };
                        this.serverProxy.BackupAsync(arg, data);
                    }
                    catch (System.Exception exception)
                    {
                        this.ProcessError(this.datatransferingStep, exception);
                    }
                }
                else
                {
                    int selectedIndex = this.CoverOrAppendPicker.SelectedIndex;
                    this.dataSynchronizationHandler.RestoreOption = (selectedIndex == 0) ? ActionOption.Cover : ActionOption.Append;
                    this.serverProxy.RestoreAsync((selectedIndex == 0) ? DataSynchronizationActionOption.Cover : DataSynchronizationActionOption.Append);
                }
            }
        }

        private void client_OpenCompleted(object sender, AsyncCompletedEventArgs e)
        {
            System.Action action = null;
            if (e.Error != null)
            {
                this.ProcessError(this.connectingServerStep, e.Error);
            }
            else
            {
                if (action == null)
                {
                    action = delegate
                    {
                        this.serverProxy.GetDataAsync(AppSetting.Instance.UserId);
                    };
                }
                this.RunWithCatch(action, this.connectingServerStep);
            }
        }

        private void CloseButton_Click(object sender, System.EventArgs e)
        {
            base.NavigationService.GoBack();
        }

        private void CloseServer()
        {
            if (((this.serverProxy != null) && (this.serverProxy.State != CommunicationState.Closed)) && (this.serverProxy.State != CommunicationState.Closing))
            {
                try
                {
                    this.serverProxy.CloseAsync();
                }
                catch (System.Exception)
                {
                }
                finally
                {
                    this.serverProxy.Abort();
                }
            }
        }

        private void dataSynchronizationHandler_DataHandleringFailed(object sender, DataSynchronizationHandlerEventArgs e)
        {
            e.StepViewModel.Failed();
            this.dataSynchronizationHandler.IsProcessBarVisiable = false;
            this.RestoreCheckingStatusPanel.Visibility = Visibility.Collapsed;
        }

        private void dataSynchronizationHandler_DataHandleringSuccess(object sender, DataSynchronizationHandlerEventArgs e)
        {
            this.RestoreCheckingStatusPanel.Visibility = Visibility.Collapsed;
            this.serverProxy.CloseAsync();
        }

        private void DataSynchronizationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            this.isFromBackKeyPress = true;
            this.CloseServer();
        }

        private void DataSynchronizationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataSynchronizationHandler.ToBackupDataBase)
            {
                if (DataSynchronizationHandler.FromRestoreDatabase)
                {
                    this.SyncActionPicker.SelectedIndex = 1;
                }
                else
                {
                    this.SyncActionPicker.SelectedIndex = 0;
                }
            }
        }

        private void initializeAppBar()
        {
            ApplicationBar bar = new ApplicationBar
            {
                Opacity = 1.0
            };
            this.appBarForSync = bar;
            ApplicationBarIconButton button = new ApplicationBarIconButton
            {
                IconUri = new Uri("/icons/appbar.feature.settings.rest.png", UriKind.Relative)
            };
            this.settingButton = button;
            this.settingButton.Text = this.GetLanguageInfoByKey("Setting");
            this.settingButton.Click += new System.EventHandler(this.SynchronizationSettingButton_Click);
            this.appBarForSync.Buttons.Add(this.settingButton);
            base.ApplicationBar = this.appBarForSync;
            ViewModelLocator.SynchronizationManagerViewModel.LoadData();
        }

        private void InitializeDataSynchronizationHandler()
        {
            this.dataSynchronizationHandler = new DataSynchronizationHandler();
            this.DataSyncingManager = this.dataSynchronizationHandler.DataSyncingManager;
            this.dataSynchronizationHandler.DataHandlerStep = this.datatransferingStep;
            this.dataSynchronizationHandler.DataHandleringSuccess += new System.EventHandler<DataSynchronizationHandlerEventArgs>(this.dataSynchronizationHandler_DataHandleringSuccess);
            this.dataSynchronizationHandler.DataHandleringFailed += new System.EventHandler<DataSynchronizationHandlerEventArgs>(this.dataSynchronizationHandler_DataHandleringFailed);
        }

        private void InitializeStepViewModels()
        {
            this.connectingServerStep = new SynchronizationStepViewModel(this.GetLanguageInfoByKey("StepInfo_ConnectingServer"));
            this.datatransferingStep = new SynchronizationStepViewModel(this.GetLanguageInfoByKey("StepInfo_TransferingData"));
            this.dataCheckingStep = new SynchronizationStepViewModel(this.GetLanguageInfoByKey("StepInfo_CheckingData"));
            this.actionEndingStep = new SynchronizationStepViewModel(this.GetLanguageInfoByKey("StepInfo_EndingAction"));
            this.ConnectServerStepPanel.DataContext = this.connectingServerStep;
            this.DatatransferingStepPanel.DataContext = this.datatransferingStep;
            this.DataCheckStepPanel.DataContext = this.dataCheckingStep;
            this.ActionEndingStepPanel.DataContext = this.actionEndingStep;
        }

        private void InitializeTips()
        {
            this.tipsViewModel = new AboutPageViewModel();
            this.TipsListBox.ItemsSource = AboutPageViewModel.GetTips(2);
        }

        private void ip_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (((PopUpResult)e.PopUpResult) == PopUpResult.Ok)
            {
                IPAndPortForSaftMode = e.Result;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (this.needRestart)
            {
                int num = base.NavigationService.BackStack.Count<JournalEntry>();
                for (int i = 0; i < num; i++)
                {
                    base.NavigationService.RemoveBackEntry();
                }
            }
            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.NavigationMode != NavigationMode.Back) && DataSynchronizationHandler.ToBackupDataBase)
            {
                this.DataSynchronizationPivot.Header = AppResources.SafeMode;
                this.fromMode = this.GetNavigatingParameter("from", null);
                if (base.NavigationService.CanGoBack && this.fromMode.IsNullOrEmpty())
                {
                    int num = base.NavigationService.BackStack.Count<JournalEntry>();
                    for (int i = 0; i < num; i++)
                    {
                        base.NavigationService.RemoveBackEntry();
                    }
                }
            }
            base.OnNavigatedTo(e);
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
            string httpAddress = AppSetting.Instance.ServerSyncIPAddress.HttpAddress;
            if (string.IsNullOrEmpty(httpAddress))
            {
                this.Alert(LocalizedStrings.GetLanguageInfoByKey("NeedIPandPortMessage").FormatWith(new object[] { "http://yurukai.wordpress.com" }), null);
                this.ToggleSteps(false);
                this.ToggleStarting(false);
                this.settingButtonDoClick();
            }
            else
            {
                string remoteAddress = string.Format("http://{0}/DataSynchronizationService.svc/", httpAddress);
                if (Debugger.IsAttached)
                {
                    remoteAddress = "http://localhost:10010/DataSynchronizationService.svc/";
                }

                this.serverProxy = new DataSynchronizationServiceClient("BasicHttpBinding_IDataSynchronizationService", remoteAddress);
                this.serverProxy.OpenCompleted += new System.EventHandler<AsyncCompletedEventArgs>(this.client_OpenCompleted);
                this.serverProxy.GetDataCompleted += new System.EventHandler<GetDataCompletedEventArgs>(this.client_GetDataCompleted);
                this.serverProxy.BackupCompleted += new System.EventHandler<BackupCompletedEventArgs>(this.serverProxy_BackupCompleted);
                this.serverProxy.RestoreCompleted += new System.EventHandler<RestoreCompletedEventArgs>(this.serverProxy_RestoreCompleted);
                this.serverProxy.CloseCompleted += new System.EventHandler<AsyncCompletedEventArgs>(this.serverProxy_CloseCompleted);
                this.RunWithCatch(delegate
                {
                    this.serverProxy.OpenAsync();
                }, this.connectingServerStep);
            }
        }

        private void RunWithCatch(System.Action action, SynchronizationStepViewModel step)
        {
            try
            {
                action();
            }
            catch (System.Exception)
            {
                step.Failed();
                this.StopMainProcessBar();
            }
        }

        private void serverProxy_BackupCompleted(object sender, BackupCompletedEventArgs e)
        {
            System.Action action = null;
            DataSynchronizationInfo data;
            if (e.Error != null)
            {
                this.ProcessError(this.datatransferingStep, e.Error);
            }
            else if (!e.Result)
            {
                this.datatransferingStep.Failed();
            }
            else
            {
                data = e.UserState as DataSynchronizationInfo;
                if (data != null)
                {
                    if (action == null)
                    {
                        action = delegate
                        {
                            string text = data.GetMessage();
                            if (DataSynchronizationHandler.ToBackupDataBase)
                            {
                                this.needRestart = true;
                                this.Alert(text, null);
                                if (NeedToConfirmRebuildDatabase && (this.AlertConfirm(AppResources.EnsureToRebuildDatabase, null, null) == MessageBoxResult.OK))
                                {
                                    AppUpdater.ForceRebuildDatabase();
                                }
                                this.Alert(AppResources.RequireRestartAppMessage, null);
                            }
                            else
                            {
                                this.Alert(text, null);
                            }
                        };
                    }
                    this.InvokeInThread(action);
                }
                this.datatransferingStep.Success();
                this.dataCheckingStep.Start();
                this.dataCheckingStep.Success();
                this.serverProxy.CloseAsync();
            }
        }

        private void serverProxy_CloseCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.ProcessError(this.dataCheckingStep, e.Error);
            }
            else
            {
                if (!this.isFromBackKeyPress)
                {
                    this.actionEndingStep.Start();
                    this.actionEndingStep.Success();
                }
                this.StopMainProcessBar();
            }
        }

        private void serverProxy_RestoreCompleted(object sender, RestoreCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.ProcessError(this.datatransferingStep, e.Error);
            }
            else
            {
                this.datatransferingStep.Success();
                this.dataCheckingStep.Start();
                this.dataSynchronizationHandler.DataHandlerStep = this.dataCheckingStep;
                this.RestoreCheckingStatusPanel.Visibility = Visibility.Visible;
                if (e.Result != null)
                {
                    try
                    {
                        DataSynchronizationInfo result = this.dataSynchronizationHandler.Restore(e.Result);
                        if (DataSynchronizationHandler.FromRestoreDatabase)
                        {
                            this.needRestart = true;
                        }
                        this.dataCheckingStep.Success();
                        this.Alert(DataContextDataHandler.GetMessageWithRestoreTips(result, false), null);
                    }
                    catch (System.Exception exception)
                    {
                        AppUpdater.AddErrorLog("serverProxy_RestoreCompleted\tWhenRestoreData", exception.ToString(), new string[0]);
                        this.dataCheckingStep.Failed();
                    }
                }
                else
                {
                    this.dataCheckingStep.Failed();
                }
                this.RestoreCheckingStatusPanel.Visibility = Visibility.Collapsed;
                this.serverProxy.CloseAsync();
            }
        }

        private void SetButtonText()
        {
            SynchronizationStepViewModel.CurrentStepReplacer = this.GetLanguageInfoByKey("CurrentStepReplacer");
            SynchronizationStepViewModel.FailedReplacer = this.GetLanguageInfoByKey("FailedReplacer");
            SynchronizationStepViewModel.SuccessReplacer = this.GetLanguageInfoByKey("SuccessReplacer");
        }

        private void settingButton_Click(object sender, System.EventArgs e)
        {
            InputPrompt prompt = new InputPrompt
            {
                Message = this.GetLanguageInfoByKey("TipsWhenSetSafeModeIP").Replace("$Line$", "\r\n"),
                Value = "192.168.1.5:10010",
                IsCancelVisible = true,
                IsSubmitOnEnterKey = true
            };
            prompt.Completed += new System.EventHandler<PopUpEventArgs<string, PopUpResult>>(this.ip_Completed);
            prompt.Show();
        }

        private void settingButtonDoClick()
        {
            this.SynchronizationSettingButton_Click(this.settingButton, System.EventArgs.Empty);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleStarting(true);
            this.ToggleSteps(true);
            try
            {
                this.ProcessSync();
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
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

            if (this.serverProxy != null)
            {
                this.serverProxy.Abort();
            }
        }

        private void SyncActionPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CoverOrAppendPicker != null)
            {
                this.CoverOrAppendPickerRow.Visibility = (this.SyncActionPicker.SelectedIndex == 1) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SynchronizationSettingButton_Click(object sender, System.EventArgs e)
        {
            this.NavigateTo("/Pages/AppSettingPage/DataSyncingSettingPage.xaml");
        }

        private void ToggleStarting(bool isStart)
        {
            this.settingButton.IsEnabled = !isStart;
            this.StopButton.IsEnabled = isStart;
            this.StartButton.IsEnabled = !isStart;
            this.MainProcess.IsIndeterminate = isStart;
            this.ProcessBarRow.Visibility = isStart ? Visibility.Visible : Visibility.Collapsed;
            this.CoverOrAppendPicker.IsEnabled = !isStart && !DataSynchronizationHandler.ToBackupDataBase;
            this.SyncActionPicker.IsEnabled = !isStart;
        }

        private void ToggleSteps(bool isStart)
        {
            this.ConnectServerStepPanel.Visibility = isStart ? Visibility.Visible : Visibility.Collapsed;
            this.DatatransferingStepPanel.Visibility = Visibility.Collapsed;
            this.DataCheckStepPanel.Visibility = Visibility.Collapsed;
            this.ActionEndingStepPanel.Visibility = Visibility.Collapsed;
            this.connectingServerStep.ResetStep();
            this.datatransferingStep.ResetStep();
            this.dataCheckingStep.ResetStep();
            this.actionEndingStep.ResetStep();
        }
    }
}

