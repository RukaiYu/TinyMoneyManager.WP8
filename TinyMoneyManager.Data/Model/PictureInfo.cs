namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using NkjSoft.Extensions;
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Common;

    [Table]
    public class PictureInfo : MultipleThreadSupportedNotionObject, IPictureProvider
    {
        private EntityRef<TinyMoneyManager.Data.Model.AccountItem> _accountItem;
        private System.Guid attachedId = System.Guid.Empty;
        private string comments;
        private BitmapImage content;
        private System.DateTime createAt = System.DateTime.Now;
        private string fileName = string.Empty;
        private string fullPath = string.Empty;
        private int id;
        private System.Guid pictureId = System.Guid.Empty;
        private string tag;
        public static readonly string ScheduledAccountItemsTag = "ScheduledAccountItems";
        public static readonly string AccountItemsTag = "AccountItems";

        public void SetFileName()
        {
            this.FileName = "{0}.jpg".FormatWith(new object[] { this.pictureId });
        }

        public static void UpdateStructureAt_196(DatabaseSchemaUpdater updater)
        {
            updater.AddTable<PictureInfo>();
        }

        [XmlIgnore, Association(Storage = "_accountItem", ThisKey = "AttachedId", OtherKey = "Id")]
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

        [XmlIgnore]
        public BitmapImage Content
        {
            get
            {
                return this.content;
            }
            set
            {

                this.content = value;
                this.OnAsyncNotifyPropertyChanged("Content");
            }
        }

        [XmlIgnore]
        public System.IO.Stream ContentSource { get; set; }

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

        [Column]
        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                if (this.fileName != value)
                {
                    this.OnNotifyPropertyChanging("FileName");
                    this.fileName = value;
                    this.OnNotifyPropertyChanged("FileName");
                }
            }
        }

        [Column]
        public string FullPath
        {
            get
            {
                return this.fullPath;
            }
            set
            {
                if (this.fullPath != value)
                {
                    this.OnNotifyPropertyChanging("FullPath");
                    this.fullPath = value;
                    this.OnNotifyPropertyChanged("FullPath");
                    this.OnNotifyPropertyChanged("Content");
                }
            }
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
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
        public System.Guid PictureId
        {
            get
            {
                return this.pictureId;
            }
            set
            {
                if (this.pictureId != value)
                {
                    this.OnNotifyPropertyChanging("PictureId");
                    this.pictureId = value;
                    this.OnNotifyPropertyChanged("PictureId");
                }
            }
        }

        [Column]
        public string Tag
        {
            get
            {
                return this.tag;
            }
            set
            {
                if (this.tag != value)
                {
                    this.OnNotifyPropertyChanging("Tag");
                    this.tag = value;
                    this.OnNotifyPropertyChanged("Tag");
                }
            }
        }
    }
}

