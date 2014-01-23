namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using System;
    using System.Data.Linq.Mapping;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;

    [Table]
    public class SchedulePlanningTable : NotionObject
    {
        private System.DateTime? executedDate;
        private bool hasExecuted;
        private System.DateTime? planedExecuteDate;
        private RecordActionType rowActionType;
        private System.Guid scheduleDataId;
        private PlanStatus status;
        private string targetAddRowTableName;

        public static void UpdateStructureAt_1_9_1(DatabaseSchemaUpdater databaseUpdater)
        {
            databaseUpdater.AddTable<SchedulePlanningTable>();
        }

        [Column]
        public System.DateTime? ExecutedDate
        {
            get
            {
                return this.executedDate;
            }
            set
            {
                if (this.executedDate != value)
                {
                    this.OnNotifyPropertyChanging("ExecutedDate");
                    this.executedDate = value;
                    this.OnNotifyPropertyChanged("ExecutedDate");
                }
            }
        }

        [XmlIgnore]
        public bool HasExecuted
        {
            get
            {
                return this.hasExecuted;
            }
            set
            {
                if (this.hasExecuted != value)
                {
                    this.OnNotifyPropertyChanging("HasExecuted");
                    this.hasExecuted = value;
                    this.OnNotifyPropertyChanged("HasExecuted");
                }
            }
        }

        [Column(IsPrimaryKey=true, IsDbGenerated=true, DbType="INT NOT NULL Identity", CanBeNull=false, AutoSync=AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public System.DateTime? PlanExecuteDate
        {
            get
            {
                return this.planedExecuteDate;
            }
            set
            {
                if (this.planedExecuteDate != value)
                {
                    this.OnNotifyPropertyChanging("PlanExecuteDate");
                    this.planedExecuteDate = value;
                    this.OnNotifyPropertyChanged("PlanExecuteDate");
                }
            }
        }

        [Column]
        public RecordActionType RowActionType
        {
            get
            {
                return this.rowActionType;
            }
            set
            {
                if (this.rowActionType != value)
                {
                    this.OnNotifyPropertyChanging("RowActionType");
                    this.rowActionType = value;
                    this.OnNotifyPropertyChanged("RowActionType");
                }
            }
        }

        [Column]
        public System.Guid ScheduleDataId
        {
            get
            {
                return this.scheduleDataId;
            }
            set
            {
                if (this.scheduleDataId != value)
                {
                    this.OnNotifyPropertyChanging("ScheduleDataId");
                    this.scheduleDataId = value;
                    this.OnNotifyPropertyChanged("ScheduleDataId");
                }
            }
        }

        [Column]
        public PlanStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                if (this.status != value)
                {
                    this.OnNotifyPropertyChanging("Status");
                    this.status = value;
                    this.OnNotifyPropertyChanged("Status");
                }
            }
        }

        [Column]
        public string TargetAddRowTableName
        {
            get
            {
                return this.targetAddRowTableName;
            }
            set
            {
                if (this.targetAddRowTableName != value)
                {
                    this.OnNotifyPropertyChanging("TargetAddRowTableName");
                    this.targetAddRowTableName = value;
                    this.OnNotifyPropertyChanged("TargetAddRowTableName");
                }
            }
        }
    }
}

