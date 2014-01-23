namespace TinyMoneyManager.Component
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    public class MultipleThreadSupportedNotionObject : NotionObject
    {
        protected override void OnNotifyPropertyChanged(string propertyName)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => base.OnNotifyPropertyChanged(propertyName));
        }

        protected override void OnNotifyPropertyChanging(string propertyName)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => base.OnNotifyPropertyChanging(propertyName));
        }
    }
}

