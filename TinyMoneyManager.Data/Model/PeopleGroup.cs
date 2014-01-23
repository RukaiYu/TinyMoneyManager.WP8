namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;

    [Table]
    public class PeopleGroup : NotionObject
    {
        private EntitySet<PeopleProfile> _peoples;
        [XmlIgnore, Column(IsVersion=true)]
        private Binary _version;
        private bool canBeDeleted = true;
        private System.Guid id;
        private bool isDeleted = false;
        private string name = string.Empty;

        public PeopleGroup()
        {
            this._peoples = new EntitySet<PeopleProfile>(new System.Action<PeopleProfile>(this.attach_ToDo), new System.Action<PeopleProfile>(this.detach_ToDo));
        }

        private void attach_ToDo(PeopleProfile accountItem)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            accountItem.AssociatedGroup = this;
        }

        private void detach_ToDo(PeopleProfile accountItem)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            accountItem.AssociatedGroup = null;
        }

        public static void HandleUpdatingAt_1_9_3(DatabaseSchemaUpdater updater)
        {
            updater.AddTable<PeopleGroup>();
        }

        public static void LoadDefaultGroups(TinyMoneyDataContext db, System.Collections.Generic.IEnumerable<PeopleGroup> groups)
        {
            db.PeopleGroups.InsertAllOnSubmit<PeopleGroup>(groups);
            db.SubmitChanges();
        }

        [Column]
        public bool CanBeDeleted
        {
            get
            {
                return this.canBeDeleted;
            }
            set
            {
                if (this.canBeDeleted != value)
                {
                    this.OnNotifyPropertyChanging("CanBeDeleted");
                    this.canBeDeleted = value;
                    this.OnNotifyPropertyChanged("CanBeDeleted");
                }
            }
        }

        [Column(IsPrimaryKey=true, DbType="UniqueIdentifier", CanBeNull=false, AutoSync=AutoSync.Default)]
        public System.Guid Id
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
        public bool IsDeleted
        {
            get
            {
                return this.isDeleted;
            }
            set
            {
                if (this.isDeleted != value)
                {
                    this.OnNotifyPropertyChanging("IsDeleted");
                    this.isDeleted = value;
                    this.OnNotifyPropertyChanged("IsDeleted");
                }
            }
        }

        [Column]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name != value)
                {
                    this.OnNotifyPropertyChanging("Name");
                    this.name = value;
                    this.OnNotifyPropertyChanged("Name");
                }
            }
        }

        [XmlIgnore, Association(Storage="_peoples", OtherKey="GroupId", ThisKey="Id")]
        public EntitySet<PeopleProfile> Peoples
        {
            get
            {
                return this._peoples;
            }
            set
            {
                this._peoples.Assign(value);
            }
        }
    }
}

