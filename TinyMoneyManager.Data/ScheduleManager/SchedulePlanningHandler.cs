namespace TinyMoneyManager.Data.ScheduleManager
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using NkjSoft.Extensions;

    public abstract class SchedulePlanningHandler
    {
        public System.Action<SchedulePlanningHandler, IScheduledTaskItem> HandlerDataParsed;

        public SchedulePlanningHandler(TinyMoneyDataContext db)
        {
            this.DataContext = db;
        }

        public static SchedulePlanningHandler CreatePlanningHandler(string recordType, TinyMoneyDataContext db = null)
        {
            SchedulePlanningHandler handler = null;
            string str = recordType;
            if (str != null)
            {
                if (!(str == "CrateExpenseRecord"))
                {
                    if (str == "CreateIncomeRecord")
                    {
                        handler.HandlerType = RecordActionType.CreateIncomeRecord;
                        return new ExpenseOrIncomeScheduleHanlder(db);
                    }
                    if (((str == "CreateTransferingRecord") || (str == "CreateBorrowRecord")) || (str == "CreateLeanRecord"))
                    {
                    }
                    return handler;
                }
                handler = new ExpenseOrIncomeScheduleHanlder(db)
                {
                    HandlerType = RecordActionType.CrateExpenseRecord
                };
            }
            return handler;
        }

        public static SchedulePlanningHandler CreatePlanningHandler(RecordActionType recordType, TinyMoneyDataContext db)
        {
            SchedulePlanningHandler handler = null;
            switch (recordType)
            {
                case RecordActionType.CrateExpenseRecord:
                case RecordActionType.CreateIncomeRecord:
                    return new ExpenseOrIncomeScheduleHanlder(db);

                case RecordActionType.CreateTransferingRecord:
                    return new TransferingItemTaskHandler(db);
                case RecordActionType.CreateBorrowRecord:
                case RecordActionType.CreateLeanRecord:
                    return handler;
            }
            return handler;
        }

        protected virtual void OnHandlerDataParsed(IScheduledTaskItem data)
        {
            if (this.HandlerDataParsed != null)
            {
                this.HandlerDataParsed(this, data);
            }
        }

        public virtual bool ParseScheduleDataToXmlNode(XElement scheduleDataEntry)
        {
            return true;
        }

        public virtual void ParseScheduleDataToXmlNode(XElement node, TallySchedule scheduleDataEntry)
        {
            node.SetAttributeValue("frequency", scheduleDataEntry.Frequency);
            node.SetAttributeValue("dayOfWeek", scheduleDataEntry.DayofWeek);
            node.SetAttributeValue("date", scheduleDataEntry.StartDate);

            var lastExecutedDate = node.TryGet(p => p.Attribute("lastExecutedDate"), p => p.Value);

            node.SetAttributeValue("lastExecutedDate", lastExecutedDate);

            var hasExecuted = node.TryGet(p => p.Attribute("hasExecuted"), p => p.Value).ToBoolean(false);
            if (scheduleDataEntry.HasExecuteChanged)
            {
                hasExecuted = false;
            }

            node.SetAttributeValue("hasExecuted", hasExecuted);
            node.SetAttributeValue("action", scheduleDataEntry.ActionType);
            node.SetAttributeValue("fromAccountId", scheduleDataEntry.FromAccountId);
            node.SetAttributeValue("toAccountId", scheduleDataEntry.ToAccountId);
            node.SetAttributeValue("categoryId", scheduleDataEntry.CategoryId);
            node.SetAttributeValue("amount", scheduleDataEntry.Money);
            node.SetAttributeValue("currency", scheduleDataEntry.Currency);
            node.SetAttributeValue("notes", scheduleDataEntry.Notes);
            node.SetAttributeValue("itemType", (int)scheduleDataEntry.RecordType);
            node.SetAttributeValue("isClaim", scheduleDataEntry.IsClaim);
            node.SetAttributeValue("active", scheduleDataEntry.IsActive);
            node.SetAttributeValue("transferingPoundageAmount", scheduleDataEntry.TransferingPoundageAmount);
            node.SetAttributeValue("dataProvider", scheduleDataEntry.DataProvider);
            node.SetAttributeValue("actionHandlerType", scheduleDataEntry.ActionHandlerType);
            node.SetAttributeValue("actionHandlerType", scheduleDataEntry.ActionHandlerType);
            node.SetAttributeValue("pictureIds", scheduleDataEntry.PictureIds);
            node.SetAttributeValue("peopleIds", scheduleDataEntry.PeopleIds);
        }

        public abstract bool ProcessingExecute(TallySchedule scheduleData);

        public TinyMoneyDataContext DataContext { get; private set; }

        public RecordActionType HandlerType { get; set; }
    }
}

