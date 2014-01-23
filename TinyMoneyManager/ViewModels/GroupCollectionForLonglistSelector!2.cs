namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Component;

    public class GroupCollectionForLonglistSelector<TGroupKey, TGroupObject> : ObservableCollection<TGroupObject> where TGroupObject : IMoney
    {
        private TGroupKey _key;

        public GroupCollectionForLonglistSelector(TGroupKey instanceOfKey)
        {
            this.Key = instanceOfKey;
            base.CollectionChanged += new NotifyCollectionChangedEventHandler(this.GroupCollectionForLonglistSelector_CollectionChanged);
        }

        public virtual void Edit(TGroupObject accountItem)
        {
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        private void GroupCollectionForLonglistSelector_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("TotalAmount"));
        }

        public void RaiseTotalMoneyChanged()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("TotalAmount"));
        }

        public virtual void RemoveItem(TGroupObject account)
        {
            int index = base.Items.IndexOf(account);
            this.RemoveItem(index);
        }

        private string defaultCurrencySymbol
        {
            get
            {
                return AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol(null);
            }
        }

        public bool HasItems
        {
            get
            {
                return (base.Count > 0);
            }
        }

        public virtual string HeaderInfo
        {
            get
            {
                return this.Key.ToString();
            }
        }

        public TGroupKey Key
        {
            get
            {
                return this._key;
            }
            set
            {
                this._key = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Key"));
            }
        }

        public string TotalAmount
        {
            get
            {
                return AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol(base.Items.Sum<TGroupObject>(p => p.GetMoney()));
            }
        }
    }
}

