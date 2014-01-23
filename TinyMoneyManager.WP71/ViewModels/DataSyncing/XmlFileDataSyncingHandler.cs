namespace TinyMoneyManager.ViewModels.DataSyncing
{
    using mangoProgressIndicator;
    using Microsoft.Live;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls.SkyDriveDataSyncing;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Language;

    public class XmlFileDataSyncingHandler : DataSyncingHandler<ObjectFromSkyDrive>
    {
        public XmlFileDataSyncingHandler()
        {
            this.DataContextSyncingDataHandler = DataContextDataHandler.Instance;
        }

        public override void Backup(bool encrypted = false)
        {
            this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Action = HandlerAction.Backup;
            string dataToBackup = this.DataContextSyncingDataHandler.BackupData();
            if (encrypted)
            {
                dataToBackup = this.EncryptData(dataToBackup);
            }

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            System.IO.MemoryStream fileContent = new System.IO.MemoryStream(encoding.GetBytes(dataToBackup));
            base.Manager.UploadFile(base.Folder.Id, this.FileName, fileContent, delegate(System.Collections.Generic.IDictionary<String, Object> file)
            {
                if (base.Folder.Files == null)
                {
                    base.Folder.Files = new System.Collections.Generic.List<ObjectFromSkyDrive>();
                }
                else
                {
                    base.Folder.Files.Clear();
                }
                base.Folder.Files.Add(base.Manager.BuildObject(file));
                CommonExtensions.AlertNotification(null, AppResources.DataSyncingSuccessfulMessage, null);
                GlobalIndicator.Instance.WorkDone();
                this.OnCompleted((this.DataContextSyncingDataHandler.DataSynchronizationDataArg));
            });
        }

        public override void CreateFolder()
        {
            base.CreateFolder();
        }

        private string EncryptData(string dataToBackup)
        {
            return dataToBackup;
        }

        private async void processRestore(bool encryptedData = false)
        {
            var result = await base.Manager.DownloadFile(string.Empty, base.DataFile.Id);

            if (result != null)
            {
                if (result.Stream != null)
                {
                    this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Action = HandlerAction.Restore;
                    string dataForContext = string.Empty;
                    result.Stream.Position = 0;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(result.Stream))
                    {
                        dataForContext = reader.ReadToEnd();
                    }
                    result.Stream.Close();
                    result.Stream.Dispose();
                    if (dataForContext.Contains("request_url_invalid"))
                    {
                        this.DataContextSyncingDataHandler.DataSynchronizationDataArg.Result = OperationResult.Failed;
                        CommonExtensions.Alert(null, AppResources.FileNotFoundMessage.FormatWith(new object[] { base.DataFile.Name }), null);
                    }
                    else
                    {
                        if (!dataForContext.IsNullOrEmpty())
                        {
                            this.DataContextSyncingDataHandler.RestoreData(dataForContext);
                        }

                        CommonExtensions.AlertNotification(null, AppResources.DataSyncingSuccessfulMessage, null);
                    }
                }
                else
                {
                    CommonExtensions.Alert(null, AppResources.FileNotFoundMessage.FormatWith(new object[] { base.DataFile.Name }), null);
                }
                GlobalIndicator.Instance.WorkDone();
                this.OnCompleted((this.DataContextSyncingDataHandler.DataSynchronizationDataArg));
            }
        }

        public override async void Restore(bool isDataEncrypted = false)
        {
            this.IsDataEncrypted = isDataEncrypted;
            if ((base.DataFile == null) || base.DataFile.Id.IsNullOrEmpty())
            {
                GlobalIndicator.Instance.BusyForWork(AppResources.DectingFile, new object[0]);

                var result = await base.Manager.LiveConnector.GetAsync("{0}/files".FormatWith(new object[] { base.Folder.Id }));

                if (result != null)
                {
                    GlobalIndicator.Instance.WorkDone();
                    //  base.Manager.LiveConnector.GetCompleted -= new System.EventHandler<LiveOperationCompletedEventArgs>(this.LiveConnector_GetCompleted);
                    // if (!base.Manager.HandleError(e.Error, null))
                    {
                        System.Collections.Generic.List<ObjectFromSkyDrive> source = base.Manager.DectingDataSyncingRootFolder(result);
                        base.Manager.FileForSyncing = source.FirstOrDefault<ObjectFromSkyDrive>(p => p.Name == "AccountBook.SyncData.xml.txt");
                        base.DataFile = base.Manager.FileForSyncing;
                        base.Manager.PicturesFolder = source.FirstOrDefault<ObjectFromSkyDrive>(p => p.Name == "Pictures");
                        if (base.DataFile == null)
                        {
                            CommonExtensions.Alert(null, AppResources.FileNotFoundExceptionMessage, null);
                            this.OnCompleted(null);
                        }
                        else
                        {
                            this.processRestore(this.IsDataEncrypted);
                        }
                    }
                }
            }
            else
            {
                this.processRestore(isDataEncrypted);
            }
        }

        public DataContextDataHandler DataContextSyncingDataHandler { get; set; }

        public string FileName
        {
            get
            {
                return "AccountBook.SyncData.xml.txt";
            }
        }

        public bool IsDataEncrypted { get; set; }
    }
}

