namespace TinyMoneyManager.Component
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;

    public abstract class NotionObject : INotifyPropertyChanged, INotifyPropertyChanging
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected NotionObject()
        {
        }

        protected virtual void OnAsyncNotifyPropertyChanged(string propertyName)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.OnNotifyPropertyChanged(propertyName);
            });
        }

        protected virtual void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnNotifyPropertyChanging(string propertyName)
        {
            PropertyChangingEventHandler propertyChanging = this.PropertyChanging;
            if (propertyChanging != null)
            {
                propertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
    }
}

