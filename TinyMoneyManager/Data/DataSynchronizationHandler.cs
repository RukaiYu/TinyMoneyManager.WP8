using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NkjSoft.WPhone;
using NkjSoft.WPhone.Extensions;
using TinyMoneyManager.Data.Model;
using System.Collections.Generic;
using TinyMoneyManager.DataSyncing;
using TinyMoneyManager.ViewModels;
using TinyMoneyManager.Component;
using System.IO;
using System.Text;

namespace TinyMoneyManager.Data
{
    using NkjSoft.Extensions;
    public class DataSynchronizationHandler : NkjSoftViewModelBase
    {
        public DataSyncingObjectManager DataSyncingManager { get; set; }

        private DataContextDataHandler dataContextDataHandler;

        private bool isFinished;

        public bool IsProcessBarVisiable
        {
            get { return isFinished; }
            set
            {
                isFinished = value;
                OnNotifyPropertyChanged("IsProcessBarVisiable");
            }
        }

        public ActionOption RestoreOption { get; set; }

        public SynchronizationStepViewModel DataHandlerStep { get; set; }

        public event EventHandler<DataSynchronizationHandlerEventArgs> DataHandleringFailed;

        public event EventHandler<DataSynchronizationHandlerEventArgs> DataHandleringSuccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSynchronizationHandler"/> class.
        /// </summary>
        /// <param name="dataSyncingObjectManager">The data syncing object manager.</param>
        public DataSynchronizationHandler()
        {
            this.IsProcessBarVisiable = false;
            DataSyncingManager = new DataSyncingObjectManager();
            dataContextDataHandler = DataContextDataHandler.Instance;
        }

        public DataSynchronizationInfo Restore(DataSynchronizationService.DataSynchronizationArgs e)
        {
            if (FromRestoreDatabase)
            {
                dataContextDataHandler.RestoreDatabase(e);
            }
            else
            {
                dataContextDataHandler.RestoreData(e.AccountItemListXmlSource);
            }

            return dataContextDataHandler.DataSynchronizationDataArg;
        }

        public static bool ToBackupDataBase = false;

        public static bool FromRestoreDatabase = false;

        /// <summary>
        /// Backups the data.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="callBack">The call back.</param>
        /// <returns></returns>
        public DataBackupedFromPhone BackupData(BackupDataOption option, Action<DataSynchronizationInfo> callBack)
        {
            var data = string.Empty;
            //todo:Test:
            DataBackupedFromPhone result = new DataBackupedFromPhone();
            if (ToBackupDataBase)
            {
                result.SdfFileContent = ExtractDataFromSdb();
                result.CategoryListXmlSource = "accountBook_database_v{0}.0.sdf".FormatWith(DatabaseVersion.LatestVersion);
                result.AppSettingXmlSource = "This is sdf file content. only for database version : " + DatabaseVersion.LatestVersion;

            }
            else
            {
                data = dataContextDataHandler.BackupData();
                result.AccountItemXmlSource = data;
            }

            if (callBack != null)
                callBack(dataContextDataHandler.DataSynchronizationDataArg);
            return result;
        }

        public byte[] ExtractDataFromSdb()
        {
            this.dataContextDataHandler = new DataContextDataHandler();
            this.dataContextDataHandler.DataSynchronizationDataArg.Action = HandlerAction.Backup;

            dataContextDataHandler.DataSynchronizationDataArg.HandlingInfo.Add("sdf", 1);

            byte[] data = null;

            if (AccountBookDataContext != null)
            {
                AccountBookDataContext.Dispose();
            }

            using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var sdfFile = iso.OpenFile(TinyMoneyDataContext.DbFileName, FileMode.Open))
                {
                    using (BinaryReader sr = new BinaryReader(sdfFile))
                    {
                        var length = sdfFile.Length;

                        data = sr.ReadBytes((Int32)length);
                    }
                }
            }
            ViewModelLocator.AccountBookDataContext = new TinyMoneyDataContext();
            DataSyncingManager.AddObject("SdfFileContentForV" + DatabaseVersion.LatestVersion, "SdfFile");
            return data;
        }

        /// <summary>
        /// Raises the <see cref="E:Success"/> event.
        /// </summary>
        /// <param name="e">The <see cref="TinyMoneyManager.Data.DataSynchronizationHandlerEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSuccess(DataSynchronizationHandlerEventArgs e)
        {
            var handler = this.DataHandleringSuccess;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>
        /// Raises the <see cref="E:Failed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="TinyMoneyManager.Data.DataSynchronizationHandlerEventArgs"/> instance containing the event data.</param>
        protected virtual void OnFailed(DataSynchronizationHandlerEventArgs e)
        {
            var handler = this.DataHandleringFailed;
            if (handler != null)
                handler(this, e);
        }


    }

}
