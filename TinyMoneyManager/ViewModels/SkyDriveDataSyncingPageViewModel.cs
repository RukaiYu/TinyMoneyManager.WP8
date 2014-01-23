namespace TinyMoneyManager.ViewModels
{
    using Microsoft.Live;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Media.Imaging;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;
    using TinyMoneyManager.Data;

    public class SkyDriveDataSyncingPageViewModel : NkjSoftViewModelBase
    {
        public SynchronizationStepViewModel actionEndingStep;
        public static bool BackupAndRestoreDataSyncingMode = true;
        public SynchronizationStepViewModel connectingServerStep;
        public SynchronizationStepViewModel dataCheckingStep;
        public SynchronizationStepViewModel datatransferingStep;
        public Stack<string> FolderIds = new Stack<string>();
        public System.Collections.Generic.Dictionary<String, NoteListItem> Notes;
        public System.Action<Action> StatusUpdatingCallBack;

        private string userName;
        public static readonly string UserNameProperty = "UseName";

        public event System.EventHandler<LiveConnectorSyncHandlerEventArgs> LiveConnectorSyncingObjectFailed;

        public event System.EventHandler<ReportStatusHandlerEventArgs> ReportingStatus;

        public event System.EventHandler<EventArgs> SyncingFinished;
        public SkyDriveDataSyncingPageViewModel()
        {
            this.DataContextSyncingDataHandler = DataContextDataHandler.Instance;
            this.InitializeStepViewModels();
        }

        private bool CheckBackupFileAvailable()
        {
            if (this.FileForSyncing == null)
            {
                return false;
            }
            return true;
        }

        public bool CheckRestoreFileAvailable()
        {
            bool flag = false;
            if ((this.FileForSyncing == null) || string.IsNullOrWhiteSpace(this.FileForSyncing.FileName))
            {
                this.OnReportingStatus(new ReportStatusHandlerEventArgs("NoFileSeletedForRestoreErrorMessage", "Please select a file from SkyDrive to restore data for AccountBook."));
                return flag;
            }
            return true;
        }

        public void DownloadData()
        {
            if (!this.VerifyStatus() || !this.CheckRestoreFileAvailable())
            {
                this.OnSyncingFinished(System.EventArgs.Empty);
            }
            else
            {
                string id = this.FileForSyncing.Id;
                System.IO.MemoryStream userState = new System.IO.MemoryStream();
                this.StatusUpdatingCallBack(delegate
                {
                    this.connectingServerStep.Success();
                    this.datatransferingStep.Start();
                });
                this.LiveConnector.DownloadCompleted += new System.EventHandler<LiveDownloadCompletedEventArgs>(this.LiveConnector_DownloadCompleted);
                this.LiveConnector.DownloadAsync(id + "/content?return_ssl_resources=true", userState);
            }
        }

        private string EnsureData()
        {
            if (BackupAndRestoreDataSyncingMode)
            {
                this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Action = TinyMoneyManager.Data.HandlerAction.Backup;
                return this.DataContextSyncingDataHandler.BackupData();
            }
            return NormalDataSetter();
        }

        private void FindNoteList(string newFolderId)
        {
            if ((this.FolderIds.Count == 0) || (this.FolderIds.Peek() != newFolderId))
            {
                this.FolderIds.Push(newFolderId);
            }
        }

        private void getUserName()
        {
            this.LiveConnector.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.getUserName_Callback);
            this.LiveConnector.GetAsync("/me");
        }

        private void getUserName_Callback(object sender, LiveOperationCompletedEventArgs e)
        {
            this.LiveConnector.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.getUserName_Callback);
            if (e.Error == null)
            {
                System.Collections.Generic.IDictionary<String, Object> result = e.Result;
                this.UserName = result["name"].ToString();
            }
            else
            {
                this.OnLiveConnectorSyncingObjectFailed(new LiveConnectorSyncHandlerEventArgs(LiveConnectorSyncObject.UserName, null, "Could not find user's Name"));
            }
        }

        private void getUserPicture()
        {
            this.getUserName();
        }

        private void getUserPicture_Callback(object sender, LiveDownloadCompletedEventArgs e)
        {
            this.LiveConnector.DownloadCompleted -= new System.EventHandler<LiveDownloadCompletedEventArgs>(this.getUserPicture_Callback);
            if (e.Error == null)
            {
                System.IO.MemoryStream userState = e.UserState as System.IO.MemoryStream;
                if (userState != null)
                {
                    new BitmapImage().SetSource(userState);
                }
                else
                {
                    this.OnLiveConnectorSyncingObjectFailed(new LiveConnectorSyncHandlerEventArgs(LiveConnectorSyncObject.UserPicture, null, "Could not find user's picture"));
                }
            }
            else
            {
                this.OnLiveConnectorSyncingObjectFailed(new LiveConnectorSyncHandlerEventArgs(LiveConnectorSyncObject.UserPicture, e.Error, "Could not find user's picture"));
            }
            this.getUserName();
        }

        public void InitializeClient(LiveConnectSession liveConnectSession)
        {
            this.LiveConnector = new LiveConnectClient(liveConnectSession);
        }

        private void InitializeStepViewModels()
        {
            this.connectingServerStep = new SynchronizationStepViewModel(LocalizedStrings.GetLanguageInfoByKey("StepInfo_ConnectingServer"));
            this.datatransferingStep = new SynchronizationStepViewModel(LocalizedStrings.GetLanguageInfoByKey("StepInfo_TransferingData"));
            this.dataCheckingStep = new SynchronizationStepViewModel(LocalizedStrings.GetLanguageInfoByKey("StepInfo_CheckingData"));
            this.actionEndingStep = new SynchronizationStepViewModel(LocalizedStrings.GetLanguageInfoByKey("StepInfo_EndingAction"));
        }

        private void LiveConnector_DownloadCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            System.Action action = null;
            System.Action action2 = null;
            this.LiveConnector.DownloadCompleted -= new System.EventHandler<LiveDownloadCompletedEventArgs>(this.LiveConnector_DownloadCompleted);
            if (e.Error == null)
            {
                if (action == null)
                {
                    action = delegate
                    {
                        this.datatransferingStep.Success();
                        this.dataCheckingStep.Start();
                    };
                }
                this.StatusUpdatingCallBack(action);
                try
                {
                    System.IO.MemoryStream result = e.Result as System.IO.MemoryStream;
                    if (result == null)
                    {
                        if (action2 == null)
                        {
                            action2 = delegate
                            {
                                this.OnSyncingFinished(System.EventArgs.Empty);
                                this.dataCheckingStep.Success();
                            };
                        }
                        this.StatusUpdatingCallBack(action2);
                    }
                    else
                    {
                        this.ProcessRestoreData(result);
                    }
                }
                catch (System.Exception exception)
                {
                    this.OnReportingStatus(new ReportStatusHandlerEventArgs("RestoreDataFromSkyDriveFailedMessage", exception.Message));
                    AppUpdater.AddErrorLog("RestoreDataFromSkyDriveFailedMessage", exception.ToString(), new string[0]);
                }
            }
            else
            {
                this.StatusUpdatingCallBack(new System.Action(this.datatransferingStep.Failed));
                this.OnReportingStatus(new ReportStatusHandlerEventArgs("DownloadDataFromSkyDriveFailedMessage", e.Error.Message));
            }
            this.OnSyncingFinished(System.EventArgs.Empty);
        }

        private void LiveConnector_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            System.Action action = null;
            this.LiveConnector.UploadCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_UploadCompleted);
            if (e.Error == null)
            {
                if (action == null)
                {
                    action = delegate
                    {
                        this.datatransferingStep.Success();
                        this.actionEndingStep.Start();
                        this.actionEndingStep.Success();
                    };
                }
                this.StatusUpdatingCallBack(action);
                string msg = string.Empty;
                if (BackupAndRestoreDataSyncingMode)
                {
                    msg = this.DataContextSyncingDataHandler.DataSynchronizationDataArg.GetMessage();
                }
                this.StatusUpdatingCallBack(delegate
                {
                    this.OnReportingStatus(new ReportStatusHandlerEventArgs("UploadDataToSkyDriveSuccessMessage", msg));
                });
            }
            else
            {
                this.StatusUpdatingCallBack(new System.Action(this.datatransferingStep.Failed));
                this.OnReportingStatus(new ReportStatusHandlerEventArgs("UploadDataToSkyDriveFailedMessage", e.Error.Message));
                AppUpdater.AddErrorLog("Error when Uploading DataToSkyDrive", e.Error.ToString(), new string[0]);
            }
            this.OnSyncingFinished(System.EventArgs.Empty);
        }

        protected virtual void OnLiveConnectorSyncingObjectFailed(LiveConnectorSyncHandlerEventArgs e)
        {
            System.EventHandler<LiveConnectorSyncHandlerEventArgs> liveConnectorSyncingObjectFailed = this.LiveConnectorSyncingObjectFailed;
            if (liveConnectorSyncingObjectFailed != null)
            {
                liveConnectorSyncingObjectFailed(this, e);
            }
        }

        protected virtual void OnReportingStatus(ReportStatusHandlerEventArgs e)
        {
            System.EventHandler<ReportStatusHandlerEventArgs> reportingStatus = this.ReportingStatus;
            if (reportingStatus != null)
            {
                reportingStatus(this, e);
            }
        }

        protected virtual void OnSyncingFinished(System.EventArgs e)
        {
            System.Action action = null;
            System.EventHandler<EventArgs> handler = this.SyncingFinished;
            if (handler != null)
            {
                if (action == null)
                {
                    action = delegate
                    {
                        handler(this, e);
                    };
                }
                this.StatusUpdatingCallBack(action);
            }
        }

        public void ProcessRestoreData(System.IO.Stream memoryStream)
        {
            System.Action action = null;
            System.Action action2 = null;
            this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Action = TinyMoneyManager.Data.HandlerAction.Restore;
            string dataForContext = string.Empty;
            memoryStream.Position = 0;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(memoryStream))
            {
                dataForContext = reader.ReadToEnd();
            }
            memoryStream.Close();
            memoryStream.Dispose();
            if (dataForContext.Contains("request_url_invalid"))
            {
                this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Result = OperationResult.Failed;
                if (action == null)
                {
                    action = delegate
                    {
                        this.OnReportingStatus(new ReportStatusHandlerEventArgs(null, LocalizedStrings.GetLanguageInfoByKey("FileNotFoundMessage").FormatWith(new object[] { this.FileForSyncing.FileName })));
                    };
                }
                this.StatusUpdatingCallBack(action);
            }
            else
            {
                this.DataContextSyncingDataHandler.RestoreData(dataForContext);
            }
            if (this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Result == OperationResult.Successfully)
            {
                if (action2 == null)
                {
                    action2 = delegate
                    {
                        this.dataCheckingStep.Success();
                        this.OnReportingStatus(new ReportStatusHandlerEventArgs("RestoreDataFromSkyDriveSuccessMessage", DataContextDataHandler.GetMessageWithRestoreTips(this.DataContextSyncingDataHandler.DataSynchronizationDataArg, false)));
                    };
                }
                this.StatusUpdatingCallBack(action2);
            }
            else
            {
                this.StatusUpdatingCallBack(new System.Action(this.dataCheckingStep.Failed));
            }
            this.StatusUpdatingCallBack(delegate
            {
                this.actionEndingStep.Start();
                this.actionEndingStep.Success();
            });
        }

        public void RestartAllStep()
        {
            this.StatusUpdatingCallBack(delegate
            {
                this.connectingServerStep.ResetStep();
                this.datatransferingStep.ResetStep();
                this.dataCheckingStep.ResetStep();
                this.actionEndingStep.ResetStep();
            });
        }

        public void SignUser()
        {
            this.getUserPicture();
        }

        public void Start()
        {
            if (this.HandlerAction == TinyMoneyManager.Data.HandlerAction.Backup)
            {
                this.UploadData();
            }
            else
            {
                this.DownloadData();
            }
        }

        internal void UploadData()
        {
            if (!this.VerifyStatus() || !this.CheckBackupFileAvailable())
            {
                this.OnSyncingFinished(System.EventArgs.Empty);
            }
            else
            {
                this.StatusUpdatingCallBack(new System.Action(this.connectingServerStep.Success));
                string parentId = this.FileForSyncing.ParentId;
                string fileNameRenamed = this.FileNameRenamed;
                string s = this.EnsureData();
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                System.IO.MemoryStream inputStream = new System.IO.MemoryStream(encoding.GetBytes(s));
                this.LiveConnector.UploadCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_UploadCompleted);
                this.LiveConnector.UploadAsync(parentId, fileNameRenamed, true, inputStream, inputStream);
                this.StatusUpdatingCallBack(new System.Action(this.datatransferingStep.Start));
            }
        }

        private bool VerifyStatus()
        {
            if ((this.LiveConnector == null) || (this.LiveConnector.Session == null))
            {
                this.connectingServerStep.Failed("NeedSignInMessage");
                return false;
            }
            this.connectingServerStep.Success();
            return true;
        }

        public bool CreateFileIfNoExistsWhenBackupData { get; set; }

        public DataContextDataHandler DataContextSyncingDataHandler { get; set; }

        public ObjectFromSkyDrive FileForSyncing { get; set; }

        public string FileNameRenamed { get; set; }

        public TinyMoneyManager.Data.HandlerAction HandlerAction { get; set; }

        public bool IsBackupToAFolder { get; set; }

        public LiveConnectClient LiveConnector { get; set; }

        public static System.Func<String> NormalDataSetter
        {
            get;
            set;
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                if (value != this.userName)
                {
                    this.OnNotifyPropertyChanging(UserNameProperty);
                    this.userName = value;
                    this.OnNotifyPropertyChanged(UserNameProperty);
                }
            }
        }
    }
}

