using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using TinyMoneyManager;
    using TinyMoneyManager.Data.Model;

    public class GroupAccountItemViewModelBase<TKey> : ObservableCollection<TinyMoneyManager.Data.Model.AccountItem>
    {
        private TKey _key;

        public GroupAccountItemViewModelBase(TKey instanceOfKey)
        {
            this.Key = instanceOfKey;
            base.CollectionChanged += new NotifyCollectionChangedEventHandler(this.AccountItemGroupViewModelCollectionChanged);
        }

        protected virtual void AccountItemGroupViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("TotalAmount"));
        }

        public virtual void Edit(AccountItem accountItem)
        {
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        public void RaiseTotalMoneyChanged()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("TotalAmount"));
        }

        public virtual void RemoveItem(AccountItem account)
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

        public TKey Key
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
                return TinyMoneyManager.Data.Model.AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol(base.Items.Sum<AccountItem>(p => p.GetMoney()));
            }
        }
    }


}
