namespace TinyMoneyManager.Data.ScheduleManager
{
    using NkjSoft.Extensions;
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;
    using System.Collections.Generic;

    public class ExpenseOrIncomeScheduleHanlder : SchedulePlanningHandler
    {
        public static Action<AccountItem, TallySchedule> DataRecoverProcessor;

        public ExpenseOrIncomeScheduleHanlder(TinyMoneyDataContext db)
            : base(db)
        {
            this.HandlerType = RecordActionType.CreateTranscationRecord;
        }

        public override bool ParseScheduleDataToXmlNode(XElement scheduleDataEntry)
        {
            AccountItem data = new AccountItem
            {
                AccountId = scheduleDataEntry.Attribute("fromAccountId").Value.ToGuid()
            };
            if (data.AccountId == System.Guid.Empty)
            {
                return false;
            }
            data.CategoryId = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("categoryId"), p => p.Value).ToGuid();
            if (data.CategoryId == System.Guid.Empty)
            {
                return false;
            }
            data.CreateTime = System.DateTime.Now;
            data.Description = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("notes"), p => p.Value);
            data.Id = System.Guid.NewGuid();
            data.IsClaim = new bool?(scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("isClaim"), p => p.Value).ToBoolean(false));
            data.Money = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("amount"), p => p.Value).ToDecimal();
            data.State = AccountItemState.Active;
            data.Type = (ItemType)scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("itemType"), p => p.Value).ToInt32();
            data.AutoTokenId = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("autoTokenId"), p => p.Value).ToGuid();

            this.OnHandlerDataParsed(data);
            return true;
        }

        public override bool ProcessingExecute(TallySchedule scheduleData)
        {
            AccountItem entity = new AccountItem
            {
                Account = scheduleData.FromAccount,
                AccountId = scheduleData.FromAccountId,
                CategoryId = scheduleData.CategoryId,
                CreateTime = System.DateTime.Now.Date,
                Description = scheduleData.Notes,
                IsClaim = new bool?((scheduleData.RecordType == ItemType.Income) ? false : scheduleData.IsClaim),
                Money = scheduleData.Money,
                State = AccountItemState.Active,
                Type = scheduleData.RecordType,
                AutoTokenId = scheduleData.Id
            };
            Account account = entity.Account;
            decimal num = (entity.Type == ItemType.Expense) ? -entity.Money : entity.Money;
            decimal? balance = account.Balance;
            decimal num2 = num;
            account.Balance = balance.HasValue ? new decimal?(balance.GetValueOrDefault() + num2) : null;
            base.DataContext.AccountItems.InsertOnSubmit(entity);
            base.DataContext.SubmitChanges();
            return true;
        }

        /// <summary>
        /// Recovers the item processor.
        /// </summary>
        /// <param name="item">The item.</param>
        public static void RecoverItemProcessor(AccountItem item, TallySchedule taskInfo)
        {
            if (DataRecoverProcessor != null)
            {
                DataRecoverProcessor(item, taskInfo);
            }
        }
    }
}

