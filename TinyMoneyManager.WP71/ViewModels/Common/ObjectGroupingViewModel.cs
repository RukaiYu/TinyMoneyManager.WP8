using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TinyMoneyManager.Component;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.ViewModels.Common
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public class ObjectGroupingViewModel<TKey, TElement> : ObservableCollection<TElement> where TElement : IMoney
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupAccountItemViewModelBase&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="instanceOfKey">The instance of key.</param>
        public ObjectGroupingViewModel(TKey instanceOfKey)
        {
            this.Key = instanceOfKey;
            this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(BorrowLeanGroupingViewModel_CollectionChanged);
        }

        public ObjectGroupingViewModel()
            : this(default(TKey))
        {

        }

        void BorrowLeanGroupingViewModel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("TotalAmount"));
        }

        private TKey _key;

        public TKey Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Key"));
            }
        }

        public virtual void Edit(TElement accountItem)
        {

        }

        public virtual void RemoveItem(TElement account)
        {
            var index = this.Items.IndexOf(account);
            this.RemoveItem(index);
        }

        public virtual string HeaderInfo
        {
            get { return Key.ToString(); }
        }

        public bool HasItems { get { return this.Count > 0; } }

        public virtual string TotalAmount
        {
            get
            {
                return AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol(
                    this.Items.Sum(p => p.GetMoney()));
            }
        }

        private string defaultCurrencySymbol
        {
            get
            {
                return AppSetting.Instance.DefaultCurrency.GetGloableCurrencySymbol();
            }
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        /// <summary>
        /// Raises the total money changed.
        /// </summary>
        public void RaiseTotalMoneyChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("TotalAmount"));
        }

    }

}
