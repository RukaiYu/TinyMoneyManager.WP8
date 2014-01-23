namespace TinyMoneyManager.ViewModels.DataSyncing
{
    using mangoProgressIndicator;
    using Microsoft.Live;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class PictureFileDataSyncingHandler : DataSyncingHandler<ObjectFromSkyDrive>
    {
        private int downloadStep;
        private System.Collections.Generic.List<String> failedDownloadPictures = new System.Collections.Generic.List<String>();
        private System.Collections.Generic.List<PictureInfo> failedPictures = new System.Collections.Generic.List<PictureInfo>();
        private int step;
        public int totalDownload;

        public override void Backup(bool encryptData = false)
        {
            if ((base.Folder == null) || base.Folder.Id.IsNullOrEmpty())
            {
                this.CreateFolder();
            }
            else
            {
                this.BackupPictures();
            }
        }

        private void BackupPictures()
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.DetectingPicturesToSync, new object[0]);
            base.Manager.LiveConnector.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetPicturesListCompleted);
            base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.DataFile.Id }));
        }

        public override void CreateFolder()
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.DetectingPictureFolder, new object[0]);
            base.Manager.LiveConnector.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetPictureFolderCompleted);
            base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.Folder.ParentObject.Id }));
        }

        private void createPictureFolder()
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.Msg_CreatingFolder, new object[0]);
            base.Manager.LiveConnector.PostCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_PostCreatePictureFolderCompleted);
            System.Collections.Generic.Dictionary<String, Object> body = new System.Collections.Generic.Dictionary<String, Object>();
            body.Add("name", "Pictures");
            body.Add("type", "folder");
            base.Manager.LiveConnector.PostAsync(base.Folder.ParentObject.Id, body);
        }

        private System.Collections.Generic.List<PictureInfo> Filter(System.Collections.Generic.List<PictureInfo> filesInfo)
        {
            return filesInfo;
        }

        public void FilterToBack(System.Collections.Generic.List<PictureInfo> filesInfo, System.Collections.Generic.List<ObjectFromSkyDrive> picturesOnServer, bool forceSync = false)
        {
            if (!forceSync)
            {
                ((System.Collections.Generic.IEnumerable<PictureInfo>)(from p in picturesOnServer select filesInfo.FirstOrDefault<PictureInfo>(x => x.PictureId.ToString() == System.IO.Path.GetFileNameWithoutExtension(p.FileName)))).ForEach<PictureInfo>(delegate(PictureInfo p)
                {
                    filesInfo.Remove(p);
                });
                System.Console.WriteLine(filesInfo.Count);
            }
        }

        private System.Collections.Generic.List<ObjectFromSkyDrive> FilterToRestore(System.Collections.Generic.List<ObjectFromSkyDrive> files, System.Collections.Generic.List<String> filters)
        {
            return (from p in files
                    where !filters.Contains(p.Name)
                    select p).ToList<ObjectFromSkyDrive>();
        }

        private void LiveConnector_DownloadPictureCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            this.downloadStep++;
            string item = e.UserState.ToString();
            if (e.Error != null)
            {
                this.failedDownloadPictures.Add(item);
            }
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(item, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite))
                {
                    e.Result.Position = 0;
                    stream.Position = 0;
                    e.Result.CopyTo(stream);
                }
            }
            if (this.totalDownload == this.downloadStep)
            {
                base.Manager.LiveConnector.DownloadCompleted -= new System.EventHandler<LiveDownloadCompletedEventArgs>(this.LiveConnector_DownloadPictureCompleted);
                CommonExtensions.AlertNotification(null, AppResources.DataSyncingSuccessfulMessage, null);
                GlobalIndicator.Instance.WorkDone();
                DataSynchronizationInfo info = new DataSynchronizationInfo
                {
                    Result = OperationResult.Successfully
                };
                System.Collections.Generic.Dictionary<String, Int32> dictionary = new System.Collections.Generic.Dictionary<String, Int32>();
                dictionary.Add(AppResources.Picture, this.downloadStep);
                info.HandlingInfo = dictionary;
                this.OnCompleted(info);
            }
            else
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_StartDownloadFileFromServer, new object[] { this.downloadStep + 1, this.totalDownload });
            }
        }

        private void LiveConnector_GetFilesCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            base.Manager.LiveConnector.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetFilesCompleted);
            if (!base.Manager.HandleError(e.Error, null))
            {
                System.Collections.Generic.List<ObjectFromSkyDrive> files = base.Manager.DectingDataSyncingRootFolder(e);
                if (files.Count == 0)
                {
                    this.OnCompleted(null);
                }
                else
                {
                    this.loopToDownloadPictures(files);
                }
            }
            else
            {
                this.OnCompleted(null);
            }
        }

        private void LiveConnector_GetPictureFolderCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            GlobalIndicator.Instance.WorkDone();
            base.Manager.LiveConnector.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetPictureFolderCompleted);
            if (!base.Manager.HandleError(e.Error, null))
            {
                base.Manager.PicturesFolder = base.Manager.DectingDataSyncingRootFolder(e).FirstOrDefault<ObjectFromSkyDrive>(p => p.Name == "Pictures");
                base.DataFile = base.Manager.PicturesFolder;
                if (base.Manager.PicturesFolder == null)
                {
                    if (base.Manager.SyncAction == HandlerAction.Backup)
                    {
                        this.createPictureFolder();
                    }
                    else
                    {
                        CommonExtensions.Alert(null, "{0}: {1}.".FormatWith(new object[] { AppResources.FolderNotFoundMessage, "Pictures" }), AppResources.SyncPictures);
                        this.OnCompleted(null);
                    }
                }
                else if (base.Manager.SyncAction == HandlerAction.Backup)
                {
                    this.BackupPictures();
                }
                else
                {
                    this.restorePictures();
                }
            }
        }

        private void LiveConnector_GetPicturesListCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            base.Manager.LiveConnector.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetPicturesListCompleted);
            if (!base.Manager.HandleError(e.Error, null))
            {
                System.Collections.Generic.List<ObjectFromSkyDrive> picturesOnServer = base.Manager.DectingDataSyncingRootFolder(e);
                System.Collections.Generic.List<PictureInfo> filesInfo = base.Manager.AccountBookDataContext.PictureInfos.ToList<PictureInfo>();
                this.FilterToBack(filesInfo, picturesOnServer, false);
                if (filesInfo.Count > 0)
                {
                    this.loopToUploadPictures(filesInfo);
                }
                else
                {
                    GlobalIndicator.Instance.WorkDone();
                    this.OnCompleted(null);
                }
            }
        }

        private void LiveConnector_PostCreatePictureFolderCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            base.Manager.LiveConnector.PostCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_PostCreatePictureFolderCompleted);
            if (!base.Manager.HandleError(e.Error, null))
            {
                base.DataFile = base.Manager.BuildObject(e.Result);
                if ((base.DataFile != null) && !base.DataFile.Id.IsNullOrEmpty())
                {
                    this.BackupPictures();
                }
            }
        }

        private void LiveConnector_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            this.step++;
            if (e.Error != null)
            {
                this.failedPictures.Add(e.UserState as PictureInfo);
            }
            if (this.step == this.TotalPictures)
            {
                base.Manager.LiveConnector.UploadCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_UploadCompleted);
                GlobalIndicator.Instance.WorkDone();
                DataSynchronizationInfo info = new DataSynchronizationInfo
                {
                    Result = OperationResult.Successfully
                };
                System.Collections.Generic.Dictionary<String, Int32> dictionary = new System.Collections.Generic.Dictionary<String, Int32>();
                dictionary.Add(AppResources.Picture, this.TotalPictures);
                info.HandlingInfo = dictionary;
                this.OnCompleted(info);
            }
            else
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_StartTransferingFileToServer, new object[] { this.step + 1, this.TotalPictures });
            }
        }

        protected void loopToDownloadPictures(System.Collections.Generic.List<ObjectFromSkyDrive> files)
        {
            this.failedPictures.Clear();
            string folderInPictures = ViewModelLocator.PicturesViewModel.GetFolderInPictures("AccountItmes");
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(folderInPictures))
                {
                    file.CreateDirectory(folderInPictures);
                }
                else
                {
                    string[] fileNames = file.GetFileNames(@"{0}\*.jpg".FormatWith(new object[] { folderInPictures }));
                    System.Console.WriteLine(fileNames.Length);
                    files = this.FilterToRestore(files, (from p in fileNames select p).ToList<string>());
                }
            }
            this.downloadStep = 0;
            this.totalDownload = files.Count;
            if (this.totalDownload == 0)
            {
                DataSynchronizationInfo e = new DataSynchronizationInfo
                {
                    Action = HandlerAction.Restore,
                    Result = OperationResult.Successfully
                };
                this.OnCompleted(e);
            }
            else
            {
                base.Manager.LiveConnector.DownloadCompleted += new System.EventHandler<LiveDownloadCompletedEventArgs>(this.LiveConnector_DownloadPictureCompleted);
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_TotalPicturesNeedToSyncToLocal, new object[] { this.totalDownload });
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_StartDownloadFileFromServer, new object[] { 1, this.totalDownload });
                foreach (ObjectFromSkyDrive drive in files)
                {
                    System.Threading.Thread.Sleep(20);
                    string userState = System.IO.Path.Combine(folderInPictures, drive.Name);
                    base.Manager.LiveConnector.DownloadAsync("{0}/content?return_ssl_resources=true".FormatWith(new object[] { drive.Id }), userState);
                }
            }
        }

        private void loopToUploadPictures(System.Collections.Generic.List<PictureInfo> filesInfo)
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.Msg_TotalPicturesNeedToBackup, new object[] { filesInfo.Count });
            this.failedPictures.Clear();
            base.Manager.LiveConnector.UploadCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_UploadCompleted);
            this.TotalPictures = filesInfo.Count;
            int num = 0;
            GlobalIndicator.Instance.BusyForWork(AppResources.Msg_StartTransferingFileToServer, new object[] { 1, this.TotalPictures });
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (PictureInfo info in filesInfo)
                {
                    num++;
                    if (!file.FileExists(info.FullPath))
                    {
                        this.failedPictures.Add(info);
                    }
                    else
                    {
                        System.IO.IsolatedStorage.IsolatedStorageFileStream inputStream = file.OpenFile(info.FullPath, System.IO.FileMode.Open);
                        if (inputStream == null)
                        {
                            this.failedPictures.Add(info);
                            continue;
                        }
                        System.Threading.Thread.Sleep(20);
                        base.Manager.LiveConnector.UploadAsync(base.DataFile.Id, info.FileName, inputStream, info);
                    }
                }
            }
        }

        public override void Restore(bool isDataEncrypted = false)
        {
            if ((base.Folder == null) || base.Folder.Id.IsNullOrEmpty())
            {
                this.CreateFolder();
            }
            else
            {
                this.restorePictures();
            }
        }

        private void restorePictures()
        {
            if ((base.DataFile != null) && !base.DataFile.Id.IsNullOrEmpty())
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.StepInfo_TransferingData, new object[0]);
                base.Manager.LiveConnector.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetFilesCompleted);
                base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.DataFile.Id }));
            }
        }

        public int TotalPictures { get; private set; }
    }
}

