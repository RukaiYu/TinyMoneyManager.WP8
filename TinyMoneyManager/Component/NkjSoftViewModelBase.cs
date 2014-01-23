namespace TinyMoneyManager.Component
{
    using NkjSoft.Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Data.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Language;

    public class NkjSoftViewModelBase : NotionObject
    {
        private bool isDataLoaded;

        public NkjSoftViewModelBase()
        {
            this.AccountBookDataContext = ViewModelLocator.AccountBookDataContext;
        }

        public virtual void Delete<T>(T obj) where T : class
        {
            this.AccountBookDataContext.GetTable<T>().DeleteOnSubmit(obj);
            this.AccountBookDataContext.SubmitChanges();
        }

        public virtual bool DeletingObjectService<T>(T instanceOfT, System.Func<T, String> titleGetter = null, System.Action callBack = null) where T : class
        {
            if (instanceOfT == null)
            {
                return false;
            }
            return (CommonExtensions.AlertConfirm(null, AppResources.DeleteAccountItemMessage, () =>
            {
                this.Delete<T>(instanceOfT);
                if (callBack != null)
                {
                    callBack();
                }
            }, AppResources.DeletingObject.FormatWith(new object[] { titleGetter(instanceOfT) })) == MessageBoxResult.OK);
        }

        public virtual void InsertAndSubmit<T>(T obj) where T : class
        {
            this.AccountBookDataContext.GetTable<T>().InsertOnSubmit(obj);
            this.SubmitChanges();
        }

        public virtual void LoadData()
        {
        }

        public virtual void LoadDataIfNot()
        {
            if (!this.IsDataLoaded)
            {
                this.LoadData();
                this.IsDataLoaded = true;
            }
        }

        public virtual void QuickLoadData()
        {
            this.IsDataLoaded = true;
        }

        public virtual void SubmitChanges()
        {
            this.AccountBookDataContext.SubmitChanges();
        }

        public virtual void Update<T>(T instanceOfT) where T : class
        {
            this.AccountBookDataContext.SubmitChanges();
        }

        public virtual TinyMoneyDataContext AccountBookDataContext { get; set; }

        public bool IsDataLoaded
        {
            get
            {
                return this.isDataLoaded;
            }
            set
            {
                this.OnNotifyPropertyChanging("IsDataLoaded");
                this.isDataLoaded = value;
                this.OnNotifyPropertyChanged("IsDataLoaded");
            }
        }
    }
}

