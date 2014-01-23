namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TinyMoneyManager.Data.Model;

    public class TestViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private ObservableCollection<AccountItem> items;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public TestViewModel()
        {
            this.Items = new ObservableCollection<AccountItem>();
        }

        private void DownloadData()
        {
            this.Items.Add(new AccountItem());
        }

        public ObservableCollection<AccountItem> Items { get; set; }
    }
}

