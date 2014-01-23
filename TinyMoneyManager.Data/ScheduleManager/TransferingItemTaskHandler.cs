using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TinyMoneyManager.Data.Model;

namespace TinyMoneyManager.Data.ScheduleManager
{
    using NkjSoft.Extensions;
    using System;
    using System.Xml.Linq;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data;
    using TinyMoneyManager.Data.Model;

    public class TransferingItemTaskHandler : ScheduleManager.SchedulePlanningHandler
    {
        /// <summary>
        /// The data recover processor
        /// </summary>
        public static Action<TransferingItem> DataRecoverProcessor;

        public TransferingItemTaskHandler(TinyMoneyDataContext db = null)
            : base(db)
        {
            this.HandlerType = RecordActionType.CreateTransferingRecord;
        }

        public override bool ParseScheduleDataToXmlNode(XElement scheduleDataEntry)
        {
            TransferingItem data = new TransferingItem
            {
                FromAccountId = scheduleDataEntry.Attribute("fromAccountId").Value.ToGuid()
            };

            if (data.FromAccountId == System.Guid.Empty)
            {
                return false;
            }

            data.ToAccountId = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("toAccountId"), p => p.Value).ToGuid();
            if (data.ToAccountId == System.Guid.Empty)
            {
                return false;
            }

            data.TransferingDate = System.DateTime.Now;
            data.Notes = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("notes"), p => p.Value);

            data.Amount = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("amount"), p => p.Value).ToDecimal();

            data.TransferingPoundageAmount = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("transferingPoundageAmount"), p => p.Value).ToDecimal();

            data.AssociatedTaskId = scheduleDataEntry.TryGet<string, XAttribute>(p => p.Attribute("autoTokenId"), p => p.Value).ToGuid();

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
        /// Called when [handler data parsed].
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnHandlerDataParsed(IScheduledTaskItem data)
        {
            base.OnHandlerDataParsed(data);
        }

        /// <summary>
        /// Recovers the item processor.
        /// </summary>
        /// <param name="item">The item.</param>
        public static void RecoverItemProcessor(TransferingItem item)
        {
            if (DataRecoverProcessor != null)
            {
                DataRecoverProcessor(item);
            }
        }

    }
}
