namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class CommonGroupCollectionForLonglistSelector<TGroupKey, TGroupObject> : ObservableCollection<TGroupObject> where TGroupObject: IGroupedObject
    {
        private TGroupKey _key;

        public CommonGroupCollectionForLonglistSelector(TGroupKey instanceOfKey)
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

        public virtual void RemoveItem(TGroupObject account)
        {
            int index = base.Items.IndexOf(account);
            this.RemoveItem(index);
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
    }
}

