namespace TinyMoneyManager.Controls.SkyDriveDataSyncing
{
    using Microsoft.Phone.Controls;
    using NkjSoft.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;

    public class ObjectFromSkyDrive : NotionObject
    {
        public static bool CanFolderBeEnableSelectAsResult = true;
        private string fileType;
        private string from;
        private string name;
        private string objectTypeIconImagePath;
        private string sharedWith;
        private string size;
        private string type;

        public ObjectFromSkyDrive()
        {
            this.FileType = "default";
            this.Size = "0 KB";
        }

        public ObjectFromSkyDrive(string id, string type)
            : this()
        {
            this.Id = id;
            this.FileType = type;
            this.type = (type == "folder") ? "folder" : "file";
            this.Files = new System.Collections.Generic.List<ObjectFromSkyDrive>(4);
            this.sharedWith = string.Empty;
        }

        public static string EnsureTypeImagePath(string fileType)
        {
            if (fileType == "docx")
            {
                fileType = "doc";
            }
            if (fileType == "xlsx")
            {
                fileType = "xls";
            }
            return string.Format("/TinyMoneyManager;component/images/fileType/{0}.png", fileType);
        }

        public System.DateTime? CreateTime { get; set; }

        public string Description { get; set; }

        public bool EnableSelectAsResult
        {
            get
            {
                return ((CanFolderBeEnableSelectAsResult && (this.name != "..")) && (this.type == "folder"));
            }
        }

        public string FileName
        {
            get
            {
                if (this.fileType == "folder")
                {
                    return this.name;
                }
                return "{0}.{1}".FormatWith(new object[] { this.name, this.fileType });
            }
        }

        public System.Collections.Generic.List<ObjectFromSkyDrive> Files { get; set; }

        public string FileType
        {
            get
            {
                return this.fileType;
            }
            set
            {
                this.fileType = value;
                this.ObjectTypeIconImagePath = EnsureTypeImagePath(value);
                this.OnNotifyPropertyChanged("FileType");
            }
        }

        public string From
        {
            get
            {
                return this.from;
            }
            set
            {
                if (this.from != value)
                {
                    this.from = value;
                    this.OnNotifyPropertyChanged("From");
                }
            }
        }

        public string Id { get; set; }

        public System.DateTime ModifiedDate { get; set; }

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
                    this.name = value;
                    this.OnNotifyPropertyChanged("Name");
                }
            }
        }

        public string ObjectTypeIconImagePath
        {
            get
            {
                return this.objectTypeIconImagePath;
            }
            set
            {
                if (this.objectTypeIconImagePath != value)
                {
                    this.objectTypeIconImagePath = value;
                    this.OnNotifyPropertyChanged("ObjectTypeIconImagePath");
                }
            }
        }

        public string ParentId { get; set; }

        public ObjectFromSkyDrive ParentObject { get; set; }

        public string ShareWith
        {
            get
            {
                return this.sharedWith;
            }
            set
            {
                if (this.sharedWith != value)
                {
                    this.sharedWith = value;
                    this.OnNotifyPropertyChanged("ShareWith");
                }
            }
        }

        public string Size
        {
            get
            {
                return this.size;
            }
            set
            {
                if (this.size != value)
                {
                    this.size = value;
                    this.OnNotifyPropertyChanged("Size");
                }
            }
        }

        public string SizeForUI
        {
            get
            {
                if (this.FileType == "folder")
                {
                    return string.Empty;
                }
                return this.size;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
                this.OnNotifyPropertyChanged("Type");
            }
        }

        public System.DateTime? UpdateTime { get; set; }

        public string UpdateTimeString
        {
            get
            {
                string str = string.Empty;
                if (this.UpdateTime.HasValue)
                {
                    FullViewDateTimeConverter hourlyDateTimeConverter = CommonExtensions.GetHourlyDateTimeConverter();
                    try
                    {
                        if (hourlyDateTimeConverter != null)
                        {
                            str = hourlyDateTimeConverter.Convert(this.UpdateTime.Value, typeof(string), null, LocalizedStrings.CultureName).ToString();
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
                return str;
            }
        }
    }
}

