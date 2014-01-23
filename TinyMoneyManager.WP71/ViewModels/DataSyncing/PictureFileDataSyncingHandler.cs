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

        private async void BackupPictures()
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.DetectingPicturesToSync, new object[0]);
            var result = await base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.DataFile.Id }));

            LiveConnector_GetPicturesListCompleted(result);
        }

        public async override void CreateFolder()
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.DetectingPictureFolder, new object[0]);

            var result = await base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.Folder.ParentObject.Id }));

            LiveConnector_GetPictureFolderCompleted(result);

        }

        private async void createPictureFolder()
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.Msg_CreatingFolder, new object[0]);

            System.Collections.Generic.Dictionary<String, Object> body = new System.Collections.Generic.Dictionary<String, Object>();
            body.Add("name", "Pictures");
            body.Add("type", "folder");
            var result = await base.Manager.LiveConnector.PostAsync(base.Folder.ParentObject.Id, body);
            LiveConnector_PostCreatePictureFolderCompleted(result);

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

        private void LiveConnector_DownloadPictureCompleted(LiveDownloadOperationResult e, string item)
        {
            this.downloadStep++;

            //if (e.Error != null)
            //{
            //    this.failedDownloadPictures.Add(item);
            //}
            using (System.IO.IsolatedStorage.IsolatedStorageFile file = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(item, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite))
                {
                    e.Stream.Position = 0;
                    stream.Position = 0;
                    e.Stream.CopyTo(stream);
                }
            }
            if (this.totalDownload == this.downloadStep)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
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
                });
            }
            else
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_StartDownloadFileFromServer, new object[] { this.downloadStep + 1, this.totalDownload });
            }
        }

        private void LiveConnector_GetFilesCompleted(LiveOperationResult e)
        {
            if (!base.Manager.HandleError(null, null))
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

        private void LiveConnector_GetPictureFolderCompleted(LiveOperationResult e)
        {
            GlobalIndicator.Instance.WorkDone();

            if (!base.Manager.HandleError(null, null))
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
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            CommonExtensions.Alert(null, "{0}: {1}.".FormatWith(new object[] { AppResources.FolderNotFoundMessage, "Pictures" }), AppResources.SyncPictures);
                            this.OnCompleted(null);
                        });
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

        private void LiveConnector_GetPicturesListCompleted(LiveOperationResult e)
        {
            if (!base.Manager.HandleError(null, null))
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

        private void LiveConnector_PostCreatePictureFolderCompleted(LiveOperationResult e)
        {
            if (!base.Manager.HandleError(null, null))
            {
                base.DataFile = base.Manager.BuildObject(e.Result);
                if ((base.DataFile != null) && !base.DataFile.Id.IsNullOrEmpty())
                {
                    this.BackupPictures();
                }
            }
        }

        private void LiveConnector_UploadCompleted(LiveOperationResult e, PictureInfo userState)
        {
            this.step++;
            //if (null != null)
            //{
            //    this.failedPictures.Add(userState);
            //}
            if (this.step == this.TotalPictures)
            {

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

        protected async void loopToDownloadPictures(System.Collections.Generic.List<ObjectFromSkyDrive> files)
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
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_TotalPicturesNeedToSyncToLocal, new object[] { this.totalDownload });
                GlobalIndicator.Instance.BusyForWork(AppResources.Msg_StartDownloadFileFromServer, new object[] { 1, this.totalDownload });
                foreach (ObjectFromSkyDrive drive in files)
                {
                    System.Threading.Thread.Sleep(20);
                    string userState = System.IO.Path.Combine(folderInPictures, drive.Name);
                    var result = await base.Manager.LiveConnector.DownloadAsync("{0}/content?return_ssl_resources=true".FormatWith(new object[] { drive.Id }));

                    LiveConnector_DownloadPictureCompleted(result, userState);
                }
            }
        }

        private async void loopToUploadPictures(System.Collections.Generic.List<PictureInfo> filesInfo)
        {
            GlobalIndicator.Instance.BusyForWork(AppResources.Msg_TotalPicturesNeedToBackup, new object[] { filesInfo.Count });
            this.failedPictures.Clear();

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

                        var result = await base.Manager.LiveConnector.UploadAsync(base.DataFile.Id, info.FileName, inputStream, OverwriteOption.Overwrite);

                        LiveConnector_UploadCompleted(result, info);
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

        private async void restorePictures()
        {
            if ((base.DataFile != null) && !base.DataFile.Id.IsNullOrEmpty())
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.StepInfo_TransferingData, new object[0]);

                var result = await base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.DataFile.Id }));

                LiveConnector_GetFilesCompleted(result);
            }
        }

        public int TotalPictures { get; private set; }
    }
}

