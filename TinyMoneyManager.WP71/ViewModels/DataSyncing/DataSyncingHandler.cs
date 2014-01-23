namespace TinyMoneyManager.ViewModels.DataSyncing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;

    public class DataSyncingHandler<T> : MultipleThreadSupportedNotionObject where T : class
    {
        private T folder;

        public event System.EventHandler<DataSynchronizationInfo> Completed;

        public virtual void Backup(bool encryptData = false)
        {
        }

        public virtual void CreateFolder()
        {
        }

        public virtual void OnCompleted(DataSynchronizationInfo e)
        {
            if (this.Completed != null)
            {
                this.Completed(this, e);
            }
        }

        public virtual void Restore(bool isDataEncrypted = false)
        {
        }

        public T DataFile
        {
            get;
            set;
        }

        public T Folder
        {
            get
            {
                return this.folder;
            }
            set
            {
                if (this.folder != value)
                {
                    this.OnNotifyPropertyChanging("Folder");
                    this.folder = value;
                    this.OnNotifyPropertyChanged("Folder");
                }
            }
        }

        public SkyDriveDataSyncingViewModel Manager
        {
            get;
            set;
        }
    }
}

