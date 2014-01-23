namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;

    [Table]
    public class PeopleAssociationData : MultipleThreadSupportedNotionObject
    {
        private EntityRef<TinyMoneyManager.Data.Model.AccountItem> _accountItem;
        private EntityRef<PeopleProfile> _peopleProfile;
        private decimal amount = 0M;
        private System.Guid attachedId;
        private string comments = string.Empty;
        private System.DateTime createAt = System.DateTime.Now;
        private int id;
        private System.Guid peopleId;

        public static void UpdateStructureAt_196(DatabaseSchemaUpdater updater)
        {
            updater.AddTable<PeopleAssociationData>();
        }

        [XmlIgnore, Association(Storage="_accountItem", ThisKey="AttachedId", OtherKey="Id")]
        public TinyMoneyManager.Data.Model.AccountItem AccountItem
        {
            get
            {
                return this._accountItem.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("AccountItem");
                this._accountItem.Entity = value;
                if (value != null)
                {
                    bool flag1 = this.attachedId != value.Id;
                    this.attachedId = value.Id;
                    this.OnNotifyPropertyChanged("AccountItem");
                }
            }
        }

        [Column]
        public decimal Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                if (this.amount != value)
                {
                    this.OnNotifyPropertyChanging("Amount");
                    this.amount = value;
                    this.OnNotifyPropertyChanged("Amount");
                }
            }
        }

        [Column]
        public System.Guid AttachedId
        {
            get
            {
                return this.attachedId;
            }
            set
            {
                if (this.attachedId != value)
                {
                    this.OnNotifyPropertyChanging("AttachedId");
                    this.attachedId = value;
                    this.OnNotifyPropertyChanged("AttachedId");
                }
            }
        }

        [Column]
        public string Comments
        {
            get
            {
                return this.comments;
            }
            set
            {
                if (this.comments != value)
                {
                    this.OnNotifyPropertyChanging("Comments");
                    this.comments = value;
                    this.OnNotifyPropertyChanged("Comments");
                }
            }
        }

        [Column]
        public System.DateTime CreateAt
        {
            get
            {
                return this.createAt;
            }
            set
            {
                if (this.createAt != value)
                {
                    this.OnNotifyPropertyChanging("CreateAt");
                    this.createAt = value;
                    this.OnNotifyPropertyChanged("CreateAt");
                }
            }
        }

        public string CreateDateInfo
        {
            get
            {
                return this.CreateAt.ToString(LocalizedObjectHelper.CultureInfoCurrentUsed.DateTimeFormat.ShortDatePattern);
            }
        }

        [Column(IsPrimaryKey=true, IsDbGenerated=true, DbType="INT NOT NULL Identity", CanBeNull=false, AutoSync=AutoSync.OnInsert)]
        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                if (this.id != value)
                {
                    this.OnNotifyPropertyChanging("Id");
                    this.id = value;
                    this.OnNotifyPropertyChanged("Id");
                }
            }
        }

        [Column]
        public System.Guid PeopleId
        {
            get
            {
                return this.peopleId;
            }
            set
            {
                if (this.peopleId != value)
                {
                    this.OnNotifyPropertyChanging("PeopleId");
                    this.peopleId = value;
                    this.OnNotifyPropertyChanged("PeopleId");
                }
            }
        }

        [Association(Storage="_peopleProfile", ThisKey="PeopleId", OtherKey="Id"), XmlIgnore]
        public PeopleProfile PeopleInfo
        {
            get
            {
                return this._peopleProfile.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("PeopleInfo");
                this._peopleProfile.Entity = value;
                if (value != null)
                {
                    bool flag1 = this.peopleId != value.Id;
                    this.peopleId = value.Id;
                    this.OnNotifyPropertyChanged("PeopleInfo");
                }
            }
        }
    }
}

