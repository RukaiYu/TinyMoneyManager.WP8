namespace TinyMoneyManager.ViewModels.DataSyncing
{
    using mangoProgressIndicator;
    using NkjSoft.Extensions;
    using Microsoft.Live;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Language;

    public class SkyDriveDataSyncingViewModel : NkjSoftViewModelBase
    {
        public Stack<string> FolderIds = new Stack<string>();
        public static readonly string[] FolderNames = new string[] { "AccountBookDataFolder", "Pictures" };
        private bool isBusy;
        private bool isLogonToLiveId;
        public static string JustMeTag = string.Empty;
        private PictureFileDataSyncingHandler pictureFileHandler;
        public System.Collections.Generic.Dictionary<String, String> shareWithKeyValue;
        private HandlerAction syncAction = HandlerAction.Restore;
        private string userName;
        public static readonly string UserNameProperty = "UseName";
        private XmlFileDataSyncingHandler xmlDataHandle;

        public SkyDriveDataSyncingViewModel()
        {
            this.SyncActionIndex = 1;
            this.FileForSyncing = new ObjectFromSkyDrive();
            this.PicturesFolder = new ObjectFromSkyDrive();
            this.DataFolder = new ObjectFromSkyDrive();
            this.InitializeShareWithKeyValue();
        }

        public ObjectFromSkyDrive BuildObject(System.Collections.Generic.IDictionary<String, Object> entry)
        {
            if (entry == null)
            {
                return null;
            }
            ObjectFromSkyDrive drive = new ObjectFromSkyDrive
            {
                Id = entry["id"].ToString(),
                Name = this.GetValueFromDict<string>(entry, "name", v => v.ToString(), string.Empty),
                Type = this.GetValueFromDict<string>(entry, "type", v => v.ToString(), string.Empty),
                CreateTime = new System.DateTime?(this.GetValueFromDict<System.DateTime>(entry, "created_time", v => System.Convert.ToDateTime(v), System.DateTime.Now)),
                Size = this.GetValueFromDict<double>(entry, "size", v => System.Convert.ToDouble(v) / 1024.0, 0.0).ToString("F2") + " KB"
            };
            if (entry.ContainsKey("shared_with"))
            {
                System.Collections.Generic.Dictionary<String, Object> dictionary = entry["shared_with"] as System.Collections.Generic.Dictionary<String, Object>;
                if (dictionary == null)
                {
                    drive.ShareWith = "unknown";
                }
                else
                {
                    drive.ShareWith = this.GetShareWithByKey(dictionary["access"].ToString());
                }
            }
            drive.ParentId = this.GetValueFromDict<string>(entry, "parent_id", v => v.ToString(), string.Empty);
            return drive;
        }

        public void CheckFolderStructureOnServer()
        {
            this.IsBusy = true;
            GlobalIndicator.Instance.BusyForWork(AppResources.DectingDataSyncingRootFolder, new object[0]);
            try
            {
                this.LiveConnector.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetRootFolderCompleted);
                this.LiveConnector.GetAsync("/me/skydrive/files");
            }
            catch (System.Exception)
            {
                GlobalIndicator.Instance.WorkDone();
                this.IsBusy = false;
                throw;
            }
        }

        public void Connect()
        {
        }

        private void dataSyncingHandler_Completed(object sender, DataSynchronizationInfo e)
        {
            this.IsBusy = false;
            GlobalIndicator.Instance.WorkDone();
        }

        public System.Collections.Generic.List<ObjectFromSkyDrive> DectingDataSyncingRootFolder(LiveOperationCompletedEventArgs e)
        {
            System.Collections.Generic.List<ObjectFromSkyDrive> list = new System.Collections.Generic.List<ObjectFromSkyDrive>();
            if (e.Result.ContainsKey("data"))
            {
                System.Collections.Generic.List<Object> list2 = e.Result["data"] as System.Collections.Generic.List<Object>;
                foreach (System.Collections.Generic.IDictionary<String, Object> dictionary in list2)
                {
                    string str = this.GetValueFromDict<string>(dictionary, "name", p => p.ToString(), "unknown");
                    ObjectFromSkyDrive drive = this.BuildObject(dictionary);
                    list.Add(drive);
                    if (str == "AccountBookDataFolder")
                    {
                        this.HasRootDirFolder = true;
                    }
                }
                return list;
            }
            System.Collections.Generic.IDictionary<String, Object> result = e.Result;
            string str2 = this.GetValueFromDict<string>(result, "name", p => p.ToString(), "unknown");
            ObjectFromSkyDrive item = this.BuildObject(result);
            list.Add(item);
            if (str2 == "AccountBookDataFolder")
            {
                this.HasRootDirFolder = true;
            }
            return list;
        }

        public bool DownloadFile(string folderFrom, string fileId, System.Action<Stream> callBack = null)
        {
            string workText = AppResources.StepInfo_TransferingData;
            GlobalIndicator.Instance.BusyForWork(workText, new object[0]);
            try
            {
                new System.IO.MemoryStream();
                this.LiveConnector.DownloadCompleted += new System.EventHandler<LiveDownloadCompletedEventArgs>(this.LiveConnector_DownloadFileCompleted);
                this.LiveConnector.DownloadAsync(fileId + "/content?return_ssl_resources=true", callBack);
                return true;
            }
            catch (System.Exception exception)
            {
                this.HandleError(exception, workText);
                GlobalIndicator.Instance.WorkDone();
                return false;
            }
        }

        public string GetShareWithByKey(string key)
        {
            string str = key;
            if (!this.shareWithKeyValue.TryGetValue(key.ToLowerInvariant(), out str))
            {
                str = key;
            }
            return str;
        }

        public T GetValueFromDict<T>(System.Collections.Generic.IDictionary<String, Object> data, string key, System.Func<Object, T> callBack, T defVal)
        {
            object obj2 = null;
            if (data.TryGetValue(key, out obj2))
            {
                return callBack(obj2);
            }
            return defVal;
        }

        public bool HandleError(System.Exception exception, string actionName = null)
        {
            if (exception != null)
            {
                CommonExtensions.Alert(null, exception.Message, actionName ?? App.AppName);
                this.IsBusy = false;
            }
            return (exception != null);
        }

        private void handler_Completed(object sender, DataSynchronizationInfo e)
        {
            this.IsBusy = false;
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        public void InitializeLiveConnector(LiveConnectSession session)
        {
            this.LiveConnector = new LiveConnectClient(session);
            this.ObjectsFromSkyDrive = new System.Collections.Generic.List<ObjectFromSkyDrive>();
            this.CheckFolderStructureOnServer();
        }

        private void InitializeShareWithKeyValue()
        {
            this.shareWithKeyValue = new System.Collections.Generic.Dictionary<String, String>();
            foreach (string str in LocalizedStrings.GetLanguageInfoByKey("ShareWithKeyValue").Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string[] strArray2 = str.Split(new char[] { ',' });
                this.shareWithKeyValue.Add(strArray2[0].ToLowerInvariant(), (strArray2[1].Length == 0) ? strArray2[0] : strArray2[1]);
            }
            JustMeTag = this.shareWithKeyValue["just me"];
        }

        private void LiveConnector_DownloadFileCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            this.LiveConnector.DownloadCompleted -= new System.EventHandler<LiveDownloadCompletedEventArgs>(this.LiveConnector_DownloadFileCompleted);
            if (!this.HandleError(e.Error, null))
            {
                System.Action<Stream> userState = e.UserState as System.Action<Stream>;
                if (userState != null)
                {
                    userState(e.Result);
                }
                else
                {
                    CommonExtensions.Alert(null, AppResources.DataSyncingSuccessfulMessage, null);
                    GlobalIndicator.Instance.WorkDone();
                }
            }
            else
            {
                GlobalIndicator.Instance.WorkDone();
            }
        }

        private void LiveConnector_GetRootFolderCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            System.Action isOkToDo = null;
            this.LiveConnector.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetRootFolderCompleted);
            if (!this.HandleError(e.Error, null))
            {
                this.ObjectsFromSkyDrive = this.DectingDataSyncingRootFolder(e);
                if (!this.HasRootDirFolder)
                {
                    if (isOkToDo == null)
                    {
                        isOkToDo = delegate
                        {
                            GlobalIndicator.Instance.BusyForWork(AppResources.CreatingFolder, new object[0]);
                            System.Collections.Generic.Dictionary<String, Object> dictionary2 = new System.Collections.Generic.Dictionary<String, Object>();
                            dictionary2.Add("name", "AccountBookDataFolder");
                            dictionary2.Add("type", "folder");
                            System.Collections.Generic.Dictionary<String, Object> body = dictionary2;
                            this.LiveConnector.PostCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_PostCreateRootFolderCompleted);
                            this.LiveConnector.PostAsync("me/skydrive", body);
                        };
                    }
                    CommonExtensions.AlertConfirm(null, "是否允许{0}在您的SkyDrive根目录下创建用于存储{0}数据的目录?".FormatWith(new object[] { App.AppName }), isOkToDo, "确认创建根目录");
                }
                else
                {
                    this.RootFolder = this.ObjectsFromSkyDrive.FirstOrDefault<ObjectFromSkyDrive>(p => p.Name == "AccountBookDataFolder");
                    this.DataFolder = this.RootFolder;
                }
            }
            this.IsBusy = false;
            GlobalIndicator.Instance.WorkDone();
        }

        private void LiveConnector_PostCreateRootFolderCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            this.LiveConnector.PostCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_PostCreateRootFolderCompleted);
            if (!this.HandleError(e.Error, AppResources.CreatingFolder))
            {
                ObjectFromSkyDrive drive = this.BuildObject(e as System.Collections.Generic.IDictionary<String, Object>);
                if (drive != null)
                {
                    this.RootFolder = drive;
                    this.DataFolder = this.RootFolder;
                    CommonExtensions.AlertNotification(null, AppResources.CreatingFolderSuccessfully, null);
                    GlobalIndicator.Instance.WorkDone();
                }
            }
        }

        private void LiveConnector_UploadFileCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            this.LiveConnector.UploadCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_UploadFileCompleted);
            if (!this.HandleError(e.Error, AppResources.StepInfo_TransferingData))
            {
                System.Action<IDictionary<String, Object>> userState = e.UserState as System.Action<IDictionary<String, Object>>;
                if (userState != null)
                {
                    userState(e.Result);
                }
                else
                {
                    CommonExtensions.Alert(null, AppResources.DataSyncingSuccessfulMessage, null);
                    GlobalIndicator.Instance.WorkDone();
                }
            }
            else
            {
                GlobalIndicator.Instance.WorkDone();
            }
        }

        private void ProcessAction(DataSyncingHandler<ObjectFromSkyDrive> dataSyncingHandler)
        {
            if ((this.RootFolder == null) || this.RootFolder.Id.IsNullOrEmpty())
            {
                CommonExtensions.AlertNotification(null, AppResources.NeedLoginLiveIdMessage, null);
            }
            else if (this.IsBusy)
            {
                CommonExtensions.AlertNotification(null, AppResources.PleaseWaitWhileBusy, null);
            }
            else
            {
                this.IsBusy = true;
                dataSyncingHandler.Manager = this;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                if (this.SyncAction == HandlerAction.Backup)
                {
                    dataSyncingHandler.Backup(false);
                }
                else
                {
                    dataSyncingHandler.Restore(false);
                }
            }
        }

        private void ReportError(string p, string p_2, System.Exception exception)
        {
            throw new System.NotImplementedException();
        }

        public void SyncData()
        {
            this.XmlDataSyncingHandler.Folder = this.DataFolder;
            this.XmlDataSyncingHandler.DataFile = this.FileForSyncing;
            this.ProcessAction(this.XmlDataSyncingHandler);
        }

        public void SyncPictures()
        {
            this.PictureFilesDataSyncingHandler.Folder = this.PicturesFolder;
            this.PictureFilesDataSyncingHandler.Folder.ParentObject = this.RootFolder;
            this.ProcessAction(this.PictureFilesDataSyncingHandler);
        }

        public bool UploadFile(string folderTo, string fileName, System.IO.Stream fileContent, System.Action<IDictionary<String, Object>> callBack = null)
        {
            string workText = AppResources.StepInfo_TransferingData;
            GlobalIndicator.Instance.BusyForWork(workText, new object[0]);
            try
            {
                this.LiveConnector.UploadCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_UploadFileCompleted);
                this.LiveConnector.UploadAsync(folderTo, fileName, true, fileContent, callBack);
                return true;
            }
            catch (System.Exception exception)
            {
                this.HandleError(exception, workText);
                GlobalIndicator.Instance.WorkDone();
                return false;
            }
        }

        public ObjectFromSkyDrive DataFolder { get; set; }

        public string[] FileFliters { get; set; }

        public ObjectFromSkyDrive FileForSyncing { get; set; }

        public bool HasRootDirFolder { get; set; }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }
            set
            {
                if (this.isBusy != value)
                {
                    this.OnNotifyPropertyChanging("IsBusy");
                    this.isBusy = value;
                    this.OnNotifyPropertyChanged("IsBusy");
                }
            }
        }

        public bool IsLogonToLiveId
        {
            get
            {
                return this.isLogonToLiveId;
            }
            set
            {
                this.isLogonToLiveId = value;
                this.OnNotifyPropertyChanged("IsLogonToLiveId");
            }
        }

        public LiveConnectClient LiveConnector { get; set; }

        public System.Collections.Generic.List<ObjectFromSkyDrive> ObjectsFromSkyDrive { get; set; }

        public PictureFileDataSyncingHandler PictureFilesDataSyncingHandler
        {
            get
            {
                if (this.pictureFileHandler == null)
                {
                    this.pictureFileHandler = ViewModelLocator.instanceLoader.LoadSingelton<PictureFileDataSyncingHandler>("PictureFileDataSyncingHandler");
                    this.pictureFileHandler.Completed += new System.EventHandler<DataSynchronizationInfo>(this.handler_Completed);
                }
                return this.pictureFileHandler;
            }
        }

        public ObjectFromSkyDrive PicturesFolder { get; set; }

        public ObjectFromSkyDrive RootFolder { get; set; }

        public HandlerAction SyncAction
        {
            get
            {
                return this.syncAction;
            }
            set
            {
                if (value != this.syncAction)
                {
                    this.syncAction = value;
                    this.OnNotifyPropertyChanged("SyncAction");
                }
            }
        }

        public int SyncActionIndex
        {
            get
            {
                return (int)this.syncAction;
            }
            set
            {
                this.SyncAction = (HandlerAction)value;
                this.OnNotifyPropertyChanged("SyncActionIndex");
            }
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

        public XmlFileDataSyncingHandler XmlDataSyncingHandler
        {
            get
            {
                if (this.xmlDataHandle == null)
                {
                    this.xmlDataHandle = ViewModelLocator.instanceLoader.LoadSingelton<XmlFileDataSyncingHandler>("XmlFileDataSyncingHandler");
                    this.xmlDataHandle.Completed += new System.EventHandler<DataSynchronizationInfo>(this.handler_Completed);
                }
                return this.xmlDataHandle;
            }
        }
    }
}

