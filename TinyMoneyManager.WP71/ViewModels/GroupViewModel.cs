namespace TinyMoneyManager.ViewModels
{
    using mangoProgressIndicator;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;

    public class GroupViewModel : NotionObject
    {
        private ObservableCollection<GroupByCreateTimeAccountItemViewModel> groupItems;
        private bool isDataLoaded;
        private TinyMoneyManager.ViewModels.ItemAdding itemAdding;
        private AccountItemDataLodingHandler loadingDataHandler;
        private ViewModeConfig viewModeInfo;

        public GroupViewModel(AccountItemDataLodingHandler loadingDataHandler)
        {
            this.GroupItems = new ObservableCollection<GroupByCreateTimeAccountItemViewModel>();
            this.viewModeInfo = new ViewModeConfig();
            this.isDataLoaded = false;
            this.viewModeInfo.PropertyChanged += new PropertyChangedEventHandler(this.ViewModeInfo_PropertyChanged);
            this.LoadingDataHandler = loadingDataHandler;
        }

        public void Delete(AccountItem item)
        {
        }

        public virtual void Load()
        {
            ViewModeConfig viewModeInfo = this.viewModeInfo;
            System.Collections.Generic.List<AccountItem> data = this.LoadingDataHandler(viewModeInfo, this.AccountItemType).ToList<AccountItem>();
            System.Collections.Generic.IEnumerable<DateTime> dates = (from p in data select p.CreateTime.Date).Distinct<System.DateTime>();
            GlobalIndicator.Instance.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { LocalizedStrings.GetCombinedText(this.AccountItemType.ToString(), "Record", true) }), new object[0]);
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.GroupItems.Clear();
                  
                foreach (var item in dates)
                {
                    GroupByCreateTimeAccountItemViewModel agvm = new GroupByCreateTimeAccountItemViewModel(item);

                    data.Where(p => p.CreateTime.Date == item.Date).ToList().ForEach(x => agvm.Add(x));

                    GroupItems.Add(agvm);
                }

                foreach (GroupByCreateTimeAccountItemViewModel model in this.GroupItems)
                {
                    foreach (AccountItem item in model)
                    {
                        item.RaisePropertyChangd(AccountItem.DescriptionWithPictureInfoProperty);
                    }
                }

                GlobalIndicator.Instance.WorkDone();
                this.IsDataLoaded = true;
            });
        }

        protected void OnItemAdding(AccountItem accountItem)
        {
            TinyMoneyManager.ViewModels.ItemAdding itemAdding = this.ItemAdding;
            if (itemAdding != null)
            {
                itemAdding(accountItem);
            }
        }

        private void ViewModeInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            System.Threading.WaitCallback callBack = null;
            if (e.PropertyName == ViewModeConfig.ViewDateTimeProperty)
            {
                if (callBack == null)
                {
                    callBack = delegate(object o)
                    {
                        this.Load();
                    };
                }
                System.Threading.ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        public ItemType AccountItemType
        {
            get
            {
                return this.viewModeInfo.ItemType;
            }
            set
            {
                this.viewModeInfo.ItemType = value;
            }
        }

        public ObservableCollection<GroupByCreateTimeAccountItemViewModel> GroupItems
        {
            get
            {
                return this.groupItems;
            }
            set
            {
                if (this.groupItems != value)
                {
                    this.OnNotifyPropertyChanging("GroupItems");
                    this.groupItems = value;
                    this.OnNotifyPropertyChanged("GroupItems");
                }
            }
        }

        public bool IsDataLoaded
        {
            get
            {
                return this.isDataLoaded;
            }
            set
            {
                this.isDataLoaded = value;
                this.OnNotifyPropertyChanged("IsDataLoaded");
            }
        }

        public TinyMoneyManager.ViewModels.ItemAdding ItemAdding
        {
            get
            {
                return this.itemAdding;
            }
            set
            {
                if (this.itemAdding != value)
                {
                    this.OnNotifyPropertyChanging("ItemAdding");
                    this.itemAdding = value;
                    this.OnNotifyPropertyChanged("ItemAdding");
                }
            }
        }

        public AccountItemDataLodingHandler LoadingDataHandler
        {
            get
            {
                return this.loadingDataHandler;
            }
            set
            {
                if (this.loadingDataHandler != value)
                {
                    this.OnNotifyPropertyChanging("LoadingDataHandler");
                    this.loadingDataHandler = value;
                    this.OnNotifyPropertyChanged("LoadingDataHandler");
                }
            }
        }

        public ViewModeConfig ViewModeInfo
        {
            get
            {
                return this.viewModeInfo;
            }
            set
            {
                this.viewModeInfo = value;
                this.OnNotifyPropertyChanged("ViewModeInfo");
            }
        }
    }
}

