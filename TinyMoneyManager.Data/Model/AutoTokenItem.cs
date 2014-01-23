namespace TinyMoneyManager.Data.Model
{
    using System;
    using System.Data.Linq.Mapping;
    using TinyMoneyManager.Component;

    [Table]
    public class AutoTokenItem : NotionObject
    {
        public System.Guid _accountItemId;
        private System.DateTime _creatAt;
        private System.Guid _id;
        private System.DateTime _repeatAt;
        private RepeatMode _repeatFreq;

        [Column]
        public System.Guid AccountItemId
        {
            get
            {
                return this._accountItemId;
            }
            set
            {
                this.OnNotifyPropertyChanging("AccountItemId");
                this._accountItemId = value;
                this.OnNotifyPropertyChanged("AccountItemId");
            }
        }

        [Column]
        public System.DateTime CreatAt
        {
            get
            {
                return this._creatAt;
            }
            set
            {
                this.OnNotifyPropertyChanging("CreatAt");
                this._creatAt = value;
                this.OnNotifyPropertyChanged("CreatAt");
            }
        }

        [Column(IsPrimaryKey=true, DbType="UniqueIdentifier", CanBeNull=false, AutoSync=AutoSync.Default)]
        public System.Guid Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this.OnNotifyPropertyChanging("Id");
                this._id = value;
                this.OnNotifyPropertyChanged("Id");
            }
        }

        [Column]
        public System.DateTime RepeatAt
        {
            get
            {
                return this._repeatAt;
            }
            set
            {
                this.OnNotifyPropertyChanging("RepeatAt");
                this._repeatAt = value;
                this.OnNotifyPropertyChanged("RepeatAt");
            }
        }

        [Column]
        public RepeatMode RepeatFreq
        {
            get
            {
                return this._repeatFreq;
            }
            set
            {
                this.OnNotifyPropertyChanging("RepeatFreq");
                this._repeatFreq = value;
                this.OnNotifyPropertyChanged("RepeatFreq");
            }
        }
    }
}

