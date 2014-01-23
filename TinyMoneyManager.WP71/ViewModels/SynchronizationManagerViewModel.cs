namespace TinyMoneyManager.ViewModels
{
    using System;
    using TinyMoneyManager.Component;

    public class SynchronizationManagerViewModel : NkjSoftViewModelBase
    {
        private bool isMainProcessStart;

        public void LoadData()
        {
            if (!base.IsDataLoaded)
            {
                base.IsDataLoaded = true;
            }
        }

        public bool IsMainProcessStart
        {
            get
            {
                return this.isMainProcessStart;
            }
            set
            {
                this.isMainProcessStart = value;
                this.OnNotifyPropertyChanged("IsMainProcessStart");
            }
        }
    }
}

